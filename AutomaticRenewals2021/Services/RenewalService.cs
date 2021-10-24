using AutomaticRenewals2021.Models;
using COTDP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace COTDP.Services
{
    public class RenewalService
    {
        DataUtility objDUT = new DataUtility();
        Utility utility = new Utility();
        DonorService donorService = new DonorService();
        public ServiceResponse AutomaticRenewalProcess()
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                objDUT.EnviarCorreoLP(0, "", "Automatic Process Began", "Automatic Process Began", "lisander23@gmail.com", null, "", "");
                WriteLog("START PROCESS: " + DateTime.Now, 1000);

                RunAutomaticRenewals();

                RunOneTimePaymentRequest();

                RunRecurrentPaymentRequests();
                response.Success = true;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                WriteLog("Error in Load Method: " + ex.Message);
            }

            return response;
        }


        #region "Renewal Processes"
        private ServiceResponse RenewalProcess(Donor donor, string CuentaCerrada = "NO", string RenewalDate = "")
        {
            ServiceResponse response = new ServiceResponse();
            long regno = donor.regno;
            int RID = donor.RID;
            try
            {
                double balance = utility.GetBalance(regno);
                double Total = Convert.ToDouble(utility.getBalance(regno));
                double RenewalAmount = GetRenewalAmount(regno);

                string renewdate = objDUT.GetScalar("select convert(varchar, doc+27,101) from member_master where regno=" + regno + "").ToString();

                if (Total >= RenewalAmount)
                {
                    string currentdate = System.DateTime.UtcNow.ToShortDateString();
                    if (Convert.ToDateTime(renewdate) > Convert.ToDateTime(currentdate))
                    {
                        WriteLog("Sorry, you cannot renew until you receive your renewal email - User: " + regno);
                        response.Success = false;
                        response.Message = "NOTIFICATION MAIL NOT SENT YET";
                        return response;
                    }
                    else
                    {
                        ServiceResponse respuesta = new ServiceResponse();
                        if (CuentaCerrada == "SI")
                        {
                            WriteLog(donor.LoginId + ": COMIENZA SAVE PROCESS");
                            respuesta = UpgradeAccount(RenewalAmount, donor, renewdate, "SI");
                            WriteLog(donor.LoginId + ": FINALIZÓ SAVE PROCESS");
                        }
                        else
                        {
                            WriteLog(donor.LoginId + ": COMIENZA SAVE PROCESS", 500);
                            respuesta = UpgradeAccount(RenewalAmount, donor, renewdate);
                            WriteLog(donor.LoginId + ": FINALIZÓ SAVE PROCESS", 500);
                        }
                        response.Success = respuesta.Success;
                        response.Message = respuesta.Message;
                        return response;
                    }
                }
                else
                {
                    if (donor.PaymentMode != "Carity Causes" && Convert.ToDateTime(RenewalDate) < DateTime.Now)
                    {
                        //AQUÍ DEBO ENVIAR EL CORREO INDICÁNDOLE A LA PERSONA QUE TIENE RENOVACIÓN AUTOMÁTICA CONFIGURADA PERO NO TIENE SALDO
                        SendInsufficientFundsMail(donor, CuentaCerrada, RenewalDate);
                    }
                    response.Success = false;
                    response.Message = "INSUFFICIENT FUNDS (14)";
                    return response;
                }
            }
            catch (Exception ex)
            {
                WriteLog("Error in CalculoInicial - User: " + regno + ". Error: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }



        public ServiceResponse RunAutomaticRenewals()
        {
            ServiceResponse response = new ServiceResponse();
            try
            {

                //autogetpendingmember();
                WriteLog("*** STARTING *** AUTOMATIC RENEWALS PROCESSS HAS STARTED: " + DateTime.Now, 500);

                AutomaticGetPendingMember();
                CheckPendingmembers2();

                WriteLog("*** ENDING *** AUTOMATIC RENEWAL PROCESS ENDS SUCCESSFULLY: " + DateTime.Now, 500);
                objDUT.EnviarCorreoLP(0, "", "Automatic Process Ended Successfully", "Automatic Process Ended Successfully", "lisander23@gmail.com", null, "", "");
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
            }
            return response;
        }

        public void RunAutomaticRenewalPayment(Donor _donor)
        {
            string strRenewalDate = Convert.ToDateTime(_donor.RenewalDate.ToString()).ToString("MM/dd/yyyy");
            var resultado = RenewalProcess(_donor, "NO", _donor.RenewalDate.Date.ToString());
            //if (resultado != "RENEWAL SUCCESSFULL" && resultado != "INSUFFICIENT FUNDS (14)")
            if (resultado.Success == false)
            {
                //ENVIAR CORREO INDICANDO QUE SE DEBE PAGAR LA RENOVACIÓN
                PaymentRenewal3DaysBeforeEmail_7(_donor);
            }
        }

        public void CheckPendingmembers2()
        {
            //WriteLog("START CHECK PENDING DONOR. ", 500);

            //****************************  CORREOS A REGISTRACIONES FT PENDIENTES POR PAGAR ******************************************************
            //CorreosRegistracionesFTpendientes();

            //****************************  CORREOS A REGISTRACIONES CC PENDIENTES POR PAGAR ******************************************************
            //CorreosRegistracionesCCpendientes();

            // Delay of 1 minute between remember mail and payment mail
            //WriteLog("ESPERA DE UN MINUTO ENTRE CORREOS DE AVISO Y PROCESAMIENTO DE PAGOS");
            //Esperar(60000);

            //******************************* PAGO DE REGISTRACIONES FT PENDIENTES *******************************************************
            //  LAS REGISTRACIONES FT PENDIENTES NO SE PAGAN DE MANERA AUTOMÁTICA POR LO TANTO NO HAGO NADA
            //PagoRegistracionesFTpendientes();

            //******************************* PAGO DE REGISTRACIONES CC PENDIENTES *******************************************************
            //PagoRegistracionesCCpendientes();

            //*****************************   CORREO Y PAGO DE RENOVACIONES PARA DONANTES FT Y PAGOS ***********************************************************************
            ManejoRenovacionesFT_EPinPendientes();

            //*****************************   CORREO Y PAGO DE RENOVACIONES PARA DONANTES CC ***********************************************************************
            //ManejoRenovacionesCCPendientes();
        }

        public List<Donor> ConvertRMDonorToDonorList(List<RenewalMailLPDonor> rmDonorList)
        {
            List<Donor> DonorList = new List<Donor>();
            foreach (RenewalMailLPDonor rmDonor in rmDonorList)
            {
                DonorList.Add(ConvertRMDonorToDonor(rmDonor));
            }
            return DonorList;
        }

        public Donor ConvertRMDonorToDonor(RenewalMailLPDonor rmDonor)
        {
            Donor donor = new Donor();
            donor.regno = rmDonor.regno;
            donor.LoginId = rmDonor.LoginId;
            donor.RenewalDate = rmDonor.RenewalDate;
            donor.AutomaticRenewal = rmDonor.AutomaticRenewal;
            donor.RenewalType = rmDonor.RenewalType;
            donor.MailSent = rmDonor.MailSent;
            donor.StatusTransaction = rmDonor.StatusTransaction;
            donor.Comments = rmDonor.Comments;
            donor.RID = rmDonor.RID;
            donor.CountryId = rmDonor.CID;
            donor.LastMailSentDate = rmDonor.LastMailSentDate;
            donor.PaymentMode = rmDonor.PaymentMode;
            donor.Language = rmDonor.Language;
            donor.EmailId = rmDonor.EmailId;
            donor.Donation = Convert.ToDecimal(rmDonor.Donation);
            return donor;
        }

        public void ManejoRenovacionesFT_EPinPendientes()  //(NUEVO) AQUÍ SE MANDA CORREO INDICANDO QUE DEBEN PAGAR Y EN LOS CASOS QUE TENGAN RENOVACIÓN AUTOMÁTICA ACTIVADA, PUES SE PAGA DE UNA VEZ SI TIENEN SALDO
        {
            WriteLog("RENEWALS PAYMENTS HAS STARTED.", 1000);

            List<RenewalMailLPDonor> rmDonorList = GetDonorfromRenewalMail();
            List<Donor> DonorList = new List<Donor>();
            DonorList = ConvertRMDonorToDonorList(rmDonorList);

            if (DonorList.Count == 0)
            {
                WriteLog("THERE IS NO RENEWAL EPIN/FT PENDING FOR PAYMENTS.", 500);
            }
            else
            {
                foreach (Donor _donor in DonorList)
                {
                    if (_donor.LoginId == "Chris7")
                    {
                        if (_donor.LoginId == "Chris7")
                        {

                        }
                    }

                    if (_donor.StatusTransaction != "PAID")
                    {
                        DateTime now = DateTime.Now;
                        if (_donor.RenewalDate.AddDays(-3).Date <= now.Date && now.Date < _donor.RenewalDate.Date)  //LAS RENOVACIONES SE DEBEN EJECUTAR 27 DÍAS DESPUÉS DE VENCIDAS
                        {
                            RenewalActions3PreviousDay(_donor);
                        }
                        else if (now.Date == _donor.RenewalDate)
                        {
                            RenewalActionsDay(_donor);
                        }
                        else if (now.ToShortDateString() == _donor.RenewalDate.AddDays(1).ToShortDateString())
                        {
                            //mensaje del día siguiente del día aniversario si no se pagó la renovación pero antes de los 8 días después de la renovación
                            RenewalActionsNextDay(_donor);
                        }
                        else if (now.Date > _donor.RenewalDate.AddDays(1) && now.Date < _donor.RenewalDate.AddDays(8).Date)
                        {
                            //En este rango de fechas debo intentar el pago automático, si es exitoso lo notifico, de lo contrario no le notifico nada al donante
                            RenewalActionsUntil8Days(_donor);
                        }
                        else if (now.Date >= _donor.RenewalDate.AddDays(8).Date && now < _donor.RenewalDate.AddDays(16).Date) //MENSAJE ENTRE 8 Y 16 DÍAS DESPUÉS DE LA FECHA DE RENOVACIÓN
                        {
                            RenewalActions8_16(_donor);
                        }
                        else if (now.Date >= _donor.RenewalDate.AddDays(16))//MENSAJE DESPUÉS DE 16 DÍAS DE LA FECHA DE RENOVACIÓN
                        {
                            RenewalActionsAfter16Days(_donor);
                        }
                        else
                        {
                            WriteLog(_donor.LoginId + ": NO RENEWAL ACTIONS DONE...", 1000);
                        }
                    }
                }
            }
            WriteLog("RENEWAL PAYMENTS ARE DONE.", 500);
        }


        #endregion


        #region "Renewal Actions"

        public void RenewalActions3PreviousDay(Donor _donor)
        {
            //SE ENVÍA EL MISMO CORREO DE AVISO PARA DONANTES MANUALES Y AUTOMÁTICOS
            PaymentRenewal3DaysBeforeEmail_7(_donor);
        }


        public void RenewalActionsDay(Donor _donor)
        {
            if (_donor.AutomaticRenewal == true)
            {
                //SI EL PAGO AUTOMÁTICO ESTÁ ACTIVADO ENTONCES INTENTO EL PAGO
                RunAutomaticRenewalPayment(_donor);
            }
            else
            {
                //PARA LOS DONANTES MANUALES NO SE HACE NADA EL DÍA DE LA RENOVACIÓN
                WriteLog(_donor.LoginId + ": RENEWAL DATE: " + DateTime.Now.ToShortDateString() + ". NO MAIL SENT. MANUAL RENEWAL. NO RENEWAL ACTIONS DONE.", 500);
            }
        }

        public void RenewalActionsNextDay(Donor _donor)
        {
            string RenewalDate = _donor.RenewalDate.ToString("MM/dd/yyyy");
            if (_donor.AutomaticRenewal == true)  //SI EL PAGO AUTOMÁTICO ESTÁ ACTIVADO ENTONCES INTENTO EL PAGO
            {
                var resultado = RenewalProcess(_donor, "NO", _donor.RenewalDate.Date.ToString());
                if (resultado.Success == false)
                {
                    if (_donor.MailSent.ToString().Trim() != "ALERT PAYMENT PAST DUE (9)")
                    {
                        SendPaymentPastDueMail(_donor, RenewalDate, "AUTOMATIC");
                    }
                }
            }
            else
            {
                if (_donor.MailSent.ToString().Trim() != "ALERT PAYMENT PAST DUE (9)")
                {

                    SendPaymentPastDueMail(_donor, RenewalDate, "MANUAL");
                }
            }
        }

        public void RenewalActionsUntil8Days(Donor _donor)
        {
            string RenewalDate = Convert.ToDateTime(_donor.RenewalDate.ToString()).ToString("MM/dd/yyyy");
            if (_donor.AutomaticRenewal == true)
            {
                var resultado = RenewalProcess(_donor, "NO", _donor.RenewalDate.Date.ToString());
                if (resultado.Success == false)
                {
                    if (CheckIfDonorForgotPayRenewal(_donor) == "ENVIARCORREO11") //TENGO QUE VERIFICAR SI EL PAGO DE LA REGISTRACIÓN FUE AYER, PARA ENVIAR EL CORREO DE OLVIDO
                    {
                        SendForgotRenewalMail(_donor);
                    }
                    else
                    {
                        //El pago no fue exitoso pero en este rango de fechas no debo notificarle nada al donante
                    }
                }
            }
            else
            {
                //Como el pago automático no está activado no debo notificarle nada al donante en este rango de fechas
                UpdateRenewalMailByRID("PERIOD BETWEEN 1 AND 8 DAYS AFTER RENEWALDATE", _donor, "RenewalActionsUntil8Days");
            }
        }

        public void RenewalActions8_16(Donor _donor)
        {
            string RenewalDate = Convert.ToDateTime(_donor.RenewalDate.ToString()).ToString("MM/dd/yyyy");
            if (_donor.AutomaticRenewal == true)
            {
                var resultado = RenewalProcess(_donor, "NO", _donor.RenewalDate.Date.ToString());
                //if (resultado != "RENEWAL SUCCESSFULL")
                if (resultado.Success == false)
                {
                    if (CheckIfDonorForgotPayRenewal(_donor) == "ENVIARCORREO11") //TENGO QUE VERIFICAR SI EL PAGO DE LA REGISTRACIÓN FUE AYER, PARA ENVIAR EL CORREO DE OLVIDO
                    {
                        SendForgotRenewalMail(_donor);
                    }
                    else
                    {
                        SendSadnessReposessedAccountMail(_donor, RenewalDate);
                    }
                }
            }
            else
            {
                SendSadnessReposessedAccountMail(_donor, RenewalDate);
            }
        }

        public void RenewalActionsAfter16Days(Donor _donor)
        {
            string RenewalDate = _donor.RenewalDate.ToString("MM/dd/yyyy");
            if (_donor.AutomaticRenewal == true) //SI EL PAGO AUTOMÁTICO ESTÁ ACTIVADO INTENTARÉ EL PAGO
            {
                string CuentaCerrada = "NO";
                if (_donor.MailSent.ToString().Trim() == "NOTICE OF CLOSED ACCOUNT ACCOUNT (16)" || _donor.MailSent.ToString().Trim() == "NOTICE OF CLOSED ACCOUNT (16)")
                {
                    CuentaCerrada = "SI";
                }
                var resultado = RenewalProcess(_donor, CuentaCerrada, _donor.RenewalDate.Date.ToString());
                if (resultado.Success == false)
                {
                    if (_donor.MailSent.ToString().Trim() != "NOTICE OF CLOSED ACCOUNT (16)" && _donor.MailSent.ToString().Trim() != "PAYMENT RENEWAL EMAIL (7) AFTER ACCOUNT CLOSED") //SI LA CUENTA NO TIENE ESTATUS DE CERRADA Y EL PAGO FALLA ENTONCES DEBO ENVIAR EL CORREO DE QUE SE ESTÁ CERANDO LA CUENTA, DE LO CONTRARIO NO LO HAGO
                    {
                        //ENVIAR CORREO Sadness: Repossessed Account _10 si no lo ha enviado antes
                        SendNoticeClosedAccountMail(_donor, RenewalDate);
                    }
                }
                else
                {
                    if (CheckIfDonorForgotPayRenewal(_donor) == "ENVIARCORREO") //TENGO QUE VERIFICAR SI EL PAGO DE LA REGISTRACIÓN FUE AYER, PARA ENVIAR EL CORREO DE OLVIDO
                    {
                        //ENVIAR CORREO FORGOT YOR RENEWAL _11
                        SendForgotRenewalMail(_donor);
                    }
                }
            }
            else
            {
                if ((_donor.MailSent.ToString().Trim() != "NOTICE OF CLOSED ACCOUNT (16)") && (_donor.MailSent.ToString().Trim() != "PAYMENT RENEWAL EMAIL (7) AFTER ACCOUNT CLOSED"))
                {                                 //AUNQUE TENGA STATUS DE CUENTA CERRADA SE LE ENVIARÁ EL MENSAJE DE QUE DEBE PAGAR
                    SendNoticeClosedAccountMail(_donor, RenewalDate);
                }
                else   //EN ESTE CASO LOS DONANTES YA TIENEN SUS CUENTAS "CERRADAS" PERO AÚN DEBE MANDÁRSELES EL CORREO DE QUE DEBEN PAGAR
                {
                    //NO DEBO ENVIAR NINGÚN CORREO AUNQUE LA CUENTA ESTÉ CERRADA, SE DEBE REPETIR EL CICLO PARA CADA CUOTA DE RENOVACIÓN VENCIDA
                    //SendPaymentNoticeMail(_donor);
                    WriteLog(_donor.LoginId + " - RENEWALDATE: " + _donor.RenewalDate.ToShortDateString() + ". NO MAIL SENT. ACCOUNT WAS PREVIOUSLY CLOSED.", 500);
                }
            }
        }

        #endregion"


        #region "Renewal Database Processes"
        private void autogetpendingmember()
        {
            try
            {
                WriteLog("START AUTOPENDING SQL PROCESS", 500);

                string storprocedure = "automatic_getpendingmember_lp";
                SqlParameter[] arrparm = new SqlParameter[1];
                arrparm[0] = new SqlParameter("@intresult", SqlDbType.Int);
                arrparm[0].Direction = ParameterDirection.Output;
                int i = objDUT.ExecuteSqlSP(arrparm, storprocedure);
                if (i > 0)
                {
                    WriteLog("FINISH AUTOPENDING SQL PROCESS SUCCESSFULLY", 500);
                }
                else
                {
                    WriteLog("FINISH AUTOPENDING SQL PROCESS WITH ERRORS", 500);
                }
            }
            catch (Exception ex)
            {
                WriteLog("ERROR EXECUTING SQL AUTOPENDING PROCESS " + ex.Message, 500);
            }
        }

        public List<RenewalMailLPDonor> GetDonorsToRenewFromRenewalMailLP(long regno = 0)
        {
            List<RenewalMailLPDonor> DonorList = new List<RenewalMailLPDonor>();
            try
            {
                //OBTENGO EL ÚLTIMO REGISTRO EN RENEWAL_MAIL_LP PARA CADA DONANTE(CADA REGNO)


                string query = "SELECT A.REGNO,A.LOGINID,A.RENEWALDATE,A.MAILSENT,A.RID,A.RENEWALTYPE,A.STATUSTRANSACTION, ";
                query = query + "A.COMMENTS,A.REGISTERDATE,A.AMOUNT,A.DONATION,B.paymentMode,C.AutomaticRenewal, B.LANGUAGE, ";
                query = query + "B.EMAILID, A.CID FROM RENEWAL_MAIL_LP A, MEMBER_Master B, Member_login C ";
                query = query + " WHERE A.REGNO = B.REGNO AND A.REGNO = C.REGNO AND RID IN(SELECT MAX(RID) FROM RENEWAL_MAIL_LP ";
                //query = query + " GROUP BY REGNO) AND STATUSTRANSACTION = 'PENDING' AND RENEWALTYPE = 'RENEWAL'";
                query = query + " GROUP BY REGNO) AND RENEWALTYPE = 'RENEWAL'";

                if (regno > 0)
                {
                    query = query + " AND REGNO = " + regno;
                }
                query = query + " ORDER BY RID";

                DataTable dt = objDUT.GetDataTable(query);

                foreach (DataRow line in dt.Rows)
                {
                    RenewalMailLPDonor donor = new RenewalMailLPDonor();
                    donor = donorService.ConvertDataRowToRenewalMailLPDonor(line);
                    DonorList.Add(donor);
                }
                return DonorList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public List<RenewalDonor> GetDonorsToRenewList(long regno = 0)
        {
            List<RenewalDonor> DonorList = new List<RenewalDonor>();
            try
            {
                //OBTENGO EL ÚLTIMO REGISTRO EN RENEWAL_MAIL_LP PARA CADA DONANTE(CADA REGNO)
                string query = "SELECT DISTINCT RT.Regno, anniversarydate,rmailstatus,Solved,LOGINID,Renewaltype,MAILNEXTRENEWAL,LASTRENEWAMT, CID, km.kitprice ";
                query = query + " FROM recharge_transaction RT ";
                query = query + "INNER JOIN MEMBER_Master M ON RT.Regno = RT.Regno INNER JOIN KIT_MASTER KM ON KM.CountryID = M.CID ";
                query = query + "WHERE anniversarydate IN(SELECT MAX(anniversarydate) FROM recharge_transaction GROUP BY Regno) ";
                query = query + "AND RT.Regno = M.REGNO  AND KM.COUNTRYID = M.CID AND RENEWALTYPE IN('Renewal', 'RegisterPaid') ";
                query = query + "AND KM.kitdesc IN('help others','looking for grants','help yourself')";
                if (regno > 0)
                {
                    query = query + " AND M.REGNO = " + regno;
                }
                query = query + " ORDER BY ANNIVERSARYDATE";

                DataTable dt = objDUT.GetDataTable(query);

                foreach (DataRow line in dt.Rows)
                {
                    RenewalDonor donor = new RenewalDonor();
                    donor = donorService.ConvertDataRowToRenewalDonor(line);
                    DonorList.Add(donor);
                }
                return DonorList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public RenewalMailLPDonor CalculateAmountDonation(RenewalMailLPDonor donor)
        {
            double Donation = 0;
            try
            {
                string query = "SELECT TOP 1 RENEWALDATE FROM RENEWAL_MAIL_LP WHERE REGNO = " + donor.regno + " ORDER BY RID DESC";
                //DETERMINO LA CANTIDAD RECIBIDA EN EL MES PARA EL DONANTE
                DataTable dt = objDUT.GetDataTable(query);
                DateTime Renewaldate = Convert.ToDateTime(dt.Rows[0][0].ToString());

                query = "select isnull(SUM(CREDIT), 0) from member_account where regno = " + donor.regno + " AND TRANSTYPE " +
                " IN('LevelIncome', 'ReNewLevelIncome') AND trStatus = 1 AND Renewalstatus = 0 and transdate between '" +
                 Renewaldate + "' and DATEADD(mm,1,'" + Renewaldate + "') ";
                dt = objDUT.GetDataTable(query);

                double LastRenewalAmount = Convert.ToDouble(dt.Rows[0][0].ToString());
                Donation = LastRenewalAmount * 0.1 > donor.Donation ? LastRenewalAmount * 0.1 : donor.Donation;
                donor.LastRenewalAmount = LastRenewalAmount;
                donor.Donation = Donation;
            }
            catch (Exception ex)
            {
                return null;
            }
            return donor;
        }

        public RenewalMailLPDonor GetLastRenewalMailLPbyRegno(long regno)
        {
            string query = "SELECT top 1 * FROM RENEWAL_MAIL_LP WHERE REGNO=" + regno + "  ORDER BY RID DESC";
            DataTable dt = objDUT.GetDataTable(query);

            RenewalMailLPDonor donor = new RenewalMailLPDonor();
            donor = donorService.ConvertDataRowToRenewalMailLPDonor(dt.Rows[0]);

            return donor;
        }

        public List<RenewalMailLPDonor> GetRenewalMailLPList(long regno)
        {
            string query = "SELECT top 1 * FROM RENEWAL_MAIL_LP WHERE REGNO=" + regno + "  ORDER BY RID DESC";
            DataTable dt = objDUT.GetDataTable(query);

            List<RenewalMailLPDonor> DonorList = new List<RenewalMailLPDonor>();
            foreach (DataRow line in dt.Rows)
            {
                RenewalMailLPDonor donor = new RenewalMailLPDonor();
                donor = donorService.ConvertDataRowToRenewalMailLPDonor(line);
                DonorList.Add(donor);
            }
            return DonorList;
        }

        public ServiceResponse InsertRenewalMailLP(RenewalMailLPDonor donor, int PaymentNumber)
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                long regno = donor.regno;
                string LoginId = donor.LoginId;
                DateTime AnniversaryDate = donor.RenewalDate;
                double LastRenewalAmount = donor.LastRenewalAmount;
                double CID = donor.CID;

                string query = "INSERT INTO Renewal_mail_LP(Regno,LOGINID,RENEWALDATE,MAILSENT,RENEWALTYPE,STATUSTRANSACTION," +
                               "COMMENTS,REGISTERDATE,AMOUNT,DONATION,CID) values(@Regno, @LOGINID, " +
                               "@ANNIVERSARYDATE, 'NO', 'RENEWAL', 'PENDING', " +
                               "@RENEWALPAYMENT, @IST, @LASTRENEWAMT, @DONATION, @CID)";

                SqlParameter[] arrParam = new SqlParameter[8];
                arrParam[0] = new SqlParameter("@Regno", SqlDbType.VarChar, 200);
                arrParam[0].Value = donor.regno;
                arrParam[1] = new SqlParameter("@LOGINID", SqlDbType.VarChar, 200);
                arrParam[1].Value = donor.LoginId;
                arrParam[2] = new SqlParameter("@ANNIVERSARYDATE", SqlDbType.VarChar, 200); //FECHA DE LA RENOVACIÓN

                if (donor.RenewalDate.Day > 28)
                {
                    arrParam[2].Value = donor.RenewalDate.AddMonths(1).ToShortDateString();
                }
                else
                {
                    arrParam[2].Value = Convert.ToDateTime(donor.RenewalDate.AddMonths(1).Month + "/" + donor.RenewalDate.Day + "/" + donor.RenewalDate.Year).ToString("MM/dd/yyyy");
                }

                arrParam[3] = new SqlParameter("@RENEWALPAYMENT", SqlDbType.VarChar, 200);
                arrParam[3].Value = "RENEWAL PAYMENT: " + PaymentNumber;
                arrParam[4] = new SqlParameter("@IST", SqlDbType.DateTime);  //FECHA EN QUE SE INSERTA EL REGISTRO EN RENEWAL_MAIL_LP
                arrParam[4].Value = DateTime.Now;
                arrParam[5] = new SqlParameter("@LASTRENEWAMT", SqlDbType.Float);
                arrParam[5].Value = donor.LastRenewalAmount;
                arrParam[6] = new SqlParameter("@DONATION", SqlDbType.Float);
                arrParam[6].Value = donor.Donation;
                arrParam[7] = new SqlParameter("@CID", SqlDbType.Int);
                arrParam[7].Value = donor.CID;
                objDUT.ExecuteSql(arrParam, query);
                response.Success = true;
                WriteLog(LoginId + ": SE INSERTÓ NUEVO REGISTRO EN RENEWAL_MAIL_LP", 500);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public ServiceResponse AutomaticGetPendingMember()
        {
            WriteLog("GETTING PENDING DONORS: " + DateTime.Now, 1000);
            ServiceResponse response = new ServiceResponse();
            try
            {
                List<RenewalMailLPDonor> DonorList = GetDonorsToRenewFromRenewalMailLP();
                double DifferenceDays = 0;
                DateTime currentDate = DateTime.Now;
                currentDate = currentDate.Date;
                foreach (var donor in DonorList)
                {
                    DifferenceDays = (currentDate - donor.RenewalDate).TotalDays;
                    while (DifferenceDays > 27)
                    {
                        RenewalMailLPDonor _donor = CalculateAmountDonation(donor);

                        //SE INSERTA EL REGISTRO EN RENEWAL_MAIL_LP SIN IMPORTAR SI ES 1ER RENEWAL O SUPERIOR
                        var LastRenewalMailLP = GetLastRenewalMailLPbyRegno(_donor.regno);
                        int LastTime = 1;
                        if (LastRenewalMailLP.Comments.Contains("RENEWAL PAYMENT"))
                        {
                            LastTime = Convert.ToInt16(LastRenewalMailLP.Comments.Substring(LastRenewalMailLP.Comments.IndexOf(':') + 2, LastRenewalMailLP.Comments.Length - LastRenewalMailLP.Comments.IndexOf(':') - 2));
                            LastTime++;
                        }

                        InsertRenewalMailLP(_donor, LastTime);


                        //GET RENEWALDATE FROM RENEWAL_MAIL_LP PARA ESE DONANTE
                        var DonorAgain = GetDonorfromRenewalMail(_donor.regno);
                        DifferenceDays = (currentDate - DonorAgain.LastOrDefault().RenewalDate).TotalDays;
                        _donor.RenewalDate = DonorAgain.LastOrDefault().RenewalDate;
                    }
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            WriteLog("*** PENDING DONORS HAVE BEEN ADDED TO RENEWAL MAIL LP TABLE ***" + DateTime.Now, 500);
            return response;
        }

        public int RunSpUpgrade(Donor donor, double txtamtnet)
        {
            string query1 = "select loginid from member_login where regno=" + donor.regno;
            DataTable LoginID = objDUT.GetDataTable(query1);
            string query = "SELECT KID FROM MEMBER_MASTER WHERE REGNO=" + donor.regno;
            DataTable dt3 = objDUT.GetDataTable(query);
            int DonorKid = Convert.ToInt16(dt3.Rows[0][0].ToString());
            SqlParameter[] objMemInsert = new SqlParameter[6];
            objMemInsert[0] = new SqlParameter("@regno", SqlDbType.BigInt, 8);
            objMemInsert[0].Value = donor.regno;
            objMemInsert[1] = new SqlParameter("@paymentmode", SqlDbType.VarChar, 200);
            objMemInsert[1].Value = "e-wallet";
            objMemInsert[2] = new SqlParameter("@epinno", SqlDbType.VarChar, 200);
            objMemInsert[2].Value = "";
            objMemInsert[3] = new SqlParameter("@KID", SqlDbType.Decimal, 200);  //CREO QUE ESTE ES EL VALOR QUE ESTÁ RÍGIDO EN 1 Y DEBE SER MODIFICADO LISANDER PRADO
            objMemInsert[3].Value = DonorKid;
            //objMemInsert[3].Value = Math.Round(Convert.ToDecimal(txtamtnet.Value) * Convert.ToDecimal(0.05), 2);
            objMemInsert[4] = new SqlParameter("@totalamt", SqlDbType.Money);
            objMemInsert[4].Value = Convert.ToDecimal(txtamtnet);           //CREO QUE A PARTIR DE ESTE VALOR DEBO CALCULAR EL VALOR DEL KID, SERÍA EL 5% DE ESTE VALOR LISANDER PRADO
            objMemInsert[5] = new SqlParameter("@intResult", SqlDbType.Int, 4);
            objMemInsert[5].Direction = ParameterDirection.Output;
            int result1;
            result1 = objDUT.ExecuteSqlSP(objMemInsert, "sp_upgrade");
            return 1;
        }

        public ServiceResponse UpdateRenewalTables(long previousRID, Donor donor)
        {
            ServiceResponse response = new ServiceResponse();

            try
            {
                long regno = donor.regno;
                //GET LAST REGISTER IN RECHARGE_TRANSACTION FOR DONOR
                string query = "select TOP 1 Rid from recharge_transaction where regno=" + regno + " and paymentdate is null ORDER BY anniversarydate DESC";
                DataTable tbDonor = objDUT.GetDataTable(query);

                if (tbDonor.Rows.Count > 0)
                {
                    string newRID = tbDonor.Rows[0]["Rid"].ToString();

                    // MARK AS SOLVED=1 THE REGISTER IN RECHARGE_TRANSACTION
                    query = "UPDATE RECHARGE_TRANSACTION SET SOLVED=1,PaymentDate='" + utility.GetTimeLP() + "' WHERE RID=" + newRID;
                    objDUT.ExecuteSql(query);
                }

                int result = objDUT.ExecuteSql("update Renewal_mail set rstatus=0 where regno=" + regno + "");
                query = "UPDATE RENEWAL_MAIL_LP SET STATUSTRANSACTION='PAID' WHERE RID=" + previousRID;
                result = objDUT.ExecuteSql(query);

                query = "SELECT RENEWALDATE FROM RENEWAL_MAIL_LP WHERE RID=" + previousRID;
                DataTable tbprov = objDUT.GetDataTable(query);
                string RENEWALDATE = Convert.ToDateTime(tbprov.Rows[0][0]).ToString("MM/dd/yyyy");
                WriteLog(donor.LoginId + "/ " + regno + ": " + " AUTOMATIC RENEWAL EXECUTED " + RENEWALDATE);

                response.Success = true;
                response.Message = RENEWALDATE;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

            }
            return response;
        }

        private ServiceResponse UpgradeAccount(double txtamtnet, Donor donor, string renewdate, string CuentaCerrada = "NO")
        {
            ServiceResponse response = new ServiceResponse();
            long regno = donor.regno;
            long RID = donor.RID;

            try
            {
                var result1 = RunSpUpgrade(donor, txtamtnet);

                //WriteToFile("Result1=" + result1);
                if (result1 <= 2) //AUTOMATIC RENEWAL SUCCESSFULLY
                {

                    var UpdateResponse = UpdateRenewalTables(RID, donor);

                    if (UpdateResponse.Success == true)
                    {
                        string RENEWALDATE = UpdateResponse.Message;
                        SendAutomaticPaymentConfirmationMail(donor, txtamtnet, RENEWALDATE, RID);
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = UpdateResponse.Message;
                    }


                    //Detect Payment Origen
                    DataTable dt1 = objDUT.GetDataTable("SELECT * FROM MEMBER_ACCOUNT WHERE REGNO=" + regno + " order by acid desc");
                    objDUT.DetectPaymentOrigen(regno, Convert.ToDecimal(txtamtnet), dt1.Rows[0]["Transtype"].ToString(), dt1.Rows[0]["Remark"].ToString(), dt1.Rows[0]["Transdate"].ToString(), Convert.ToInt32(dt1.Rows[0]["Acid"].ToString()));

                    //CHECK ADMINBOXES #33 Y #37
                    int resultado = CheckAdminBoxes33_37(regno);
                    if (resultado == 1)
                    {
                        WriteLog(donor.LoginId + ": CAJAS 33 Y 37 CUADRADAS");
                    }
                    else
                    {
                        WriteLog(donor.LoginId + ": (ALERTA) CAJAS 33 Y 37 DESCUADRADAS");
                    }
                    response.Success = true;
                    response.Message = "RENEWAL SUCCESSFULL";
                    return response;
                }
                else if (result1 == 6)
                {
                    WriteLog("Sorry, you cannot renew until you receive your renewal email. User: " + donor.LoginId + ", regno: " + regno);
                    response.Success = false;
                    response.Message = "RENEWAL MAIL NOT SENT YET";
                    return response;
                }
                else if (result1 == 5)
                {
                    if (CuentaCerrada == "NO")
                    {
                        //SEND MAIL NOTIFYING TO THE USER THAT HIS BALANCE IS NOT ENOUGH");
                        WriteLog("In this case, your Automatic Renewal couldn't be executed because you don't have enough money in your wallet to do it. User: " + donor.LoginId + ", regno: " + regno);
                        SendInsufficientFunds_14(donor);
                    }
                    response.Success = false;
                    response.Message = "BALANCE IS NOT ENOUGH";
                    return response;
                }
                else
                {
                    response.Success = false;
                    response.Message = "OTRO CASO";
                    return response;
                }
            }
            catch (Exception ex)
            {
                this.WriteLog("Error Executing Automatic Renewal in process SAVE " + ex.Message);
                objDUT.EnviarCorreoLP(0, "", "Error Executing Automatic Renewal in process SAVE " + ex.Message, "Error Executing Automatic Renewal in process SAVE " + ex.Message, "lisander23@gmail.com");
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public void UpdateRenewalMailByRID(string MailSent, Donor donor, string Method)
        {
            string query = "UPDATE RENEWAL_MAIL_LP SET MAILSENT='" + MailSent + "', LASTMAILSENTDATE = CURRENT_TIMESTAMP WHERE RID=" + donor.RID;
            int res = objDUT.ExecuteSql(query);

            if (Method == "RenewalActionsUntil8Days")
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". NO MAIL SENT. " + MailSent, 500);
            }
            else if (Method == "SendInsufficientFundsMail")
            {

            }
            else if (Method == "SendPaymentPastDueMail")
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". MAIL SENT: " + MailSent, 500);
            }
            else if (Method == "PaymentRenewal3DaysBeforeEmail_7")
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". MAIL SENT: " + MailSent, 500);
            }
            else if (Method == "SendAutomaticPaymentConfirmationMail")
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". MAIL SENT: " + MailSent, 500);
            }
            else if (Method == "SendNoticeClosedAccountMail")
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". MAIL SENT: " + MailSent, 500);
            }

        }

        public double GetRenewalAmount(long regno)
        {
            SqlParameter[] arparm = new SqlParameter[1];
            arparm[0] = new SqlParameter("@regno", SqlDbType.Int);
            arparm[0].Value = regno;
            double renewalamt = Convert.ToDouble(objDUT.GetScalerSP(arparm, "Gegcurrentrenewamt_LP"));
            return renewalamt;
        }

        public List<RenewalMailLPDonor> GetDonorfromRenewalMail(long regno = 0)
        {
            string query = "SELECT A.REGNO,A.LOGINID,A.RENEWALDATE,A.MAILSENT,A.RID,A.RENEWALTYPE,A.STATUSTRANSACTION,A.COMMENTS,A.REGISTERDATE," +
               "A.AMOUNT,A.DONATION,B.paymentMode,C.AutomaticRenewal, B.LANGUAGE, B.EMAILID, A.CID FROM RENEWAL_MAIL_LP A,MEMBER_Master B, Member_login C WHERE " +
            //" STATUSTRANSACTION = 'PENDING' AND RENEWALTYPE = 'RENEWAL' AND A.REGNO = B.REGNO AND A.REGNO = C.REGNO ";
            " RENEWALTYPE = 'RENEWAL' AND A.REGNO = B.REGNO AND A.REGNO = C.REGNO ";
            if (regno > 0)
            {
                query = query + " AND A.REGNO = " + regno;
            }
            query = query + "ORDER BY RID ";

            DataTable dt1 = objDUT.GetDataTable(query);
            var DonorList = donorService.ConvertDataRowToRenewalMailLPDonorList(dt1);
            return DonorList;
        }

        #endregion


        #region "Renewal Mails"
        public void SendInsufficientFundsMail(Donor donor, string CuentaCerrada, string RenewalDate)
        {
            if (donor.MailSent != "INSUFFICIENT FUNDS (14)" && CuentaCerrada == "NO" && Convert.ToDateTime(RenewalDate).Date >= DateTime.Now.Date)
            {
                SendInsufficientFunds_14(donor);
                UpdateRenewalMailByRID("INSUFFICIENT FUNDS (14)", donor, "SendInsufficientFundsMail");
            }
            else
            {
                WriteLog(donor.LoginId + ": (ANTERIORMENTE)  MAILSENT=INSUFFICIENT FUNDS (14) AGAIN IN RENEWAL_MAIL_LP");
            }
        }

        public void SendAutomaticPaymentConfirmationMail(Donor donor, double txtamtnet, string RENEWALDATE, long RID)
        {
            AutomaticPaymentConfirmation_8(donor, txtamtnet.ToString(), RENEWALDATE);
            donor.RID = Convert.ToInt32(RID);
            UpdateRenewalMailByRID("AUTOMATIC PAYMENT CONFIRMATION (8)", donor, "SendAutomaticPaymentConfirmationMail");
        }
        public void SendNoticeClosedAccountMail(Donor _donor, string RenewalDate)
        {
            SendNoticeClosedAccount_13(_donor, RenewalDate);
            UpdateRenewalMailByRID("NOTICE OF CLOSED ACCOUNT (16)", _donor, "SendNoticeClosedAccountMail");
        }

        public ServiceResponse SendForgotRenewalMail(Donor donor)
        {
            ServiceResponse response = new ServiceResponse();
            SendForgotRenewal_11(donor);
            UpdateRenewalMailByRID("FORGOT YOUR RENEWAL (11)", donor, "SendForgotRenewalMail");
            return response;
        }

        public void SendSadnessReposessedAccountMail(Donor donor, string RenewalDate)
        {
            if (donor.MailSent.ToString().Trim() != "SADNESS REPOSSESSED ACCOUNT (10)")
            {
                SendSadnessRepossessedAccount_10(donor, RenewalDate);
                UpdateRenewalMailByRID("SADNESS REPOSSESSED ACCOUNT (10)", donor, "SendSadnessReposessedAccountMail");
            }
            else
            {
                WriteLog(donor.LoginId + ": NO RENEWAL ACTIONS DONE...", 500);
            }
        }
        public void PaymentRenewal3DaysBeforeEmail_7(Donor donor)
        {
            string strRenewalDate = donor.RenewalDate.ToString("MM/dd/yyyy");
            if (donor.MailSent.ToString().Trim() != "PAYMENT RENEWAL EMAIL 3 DAYS BEFORE (7)")
            {
                //ENVIAR CORREO PAYMENTRENEWALEMAIL_7
                PaymentRenewal3DaysBeforeEmail_7(donor, strRenewalDate);
                UpdateRenewalMailByRID("PAYMENT RENEWAL EMAIL 3 DAYS BEFORE (7)", donor, "PaymentRenewal3DaysBeforeEmail_7");
            }
            else
            {
                WriteLog(donor.LoginId + " - RENEWALDATE: " + donor.RenewalDate.ToShortDateString() + ". NO MAIL SENT. NO RENEWAL ACTIONS DONE...", 500);
            }
        }

        public void SendPaymentPastDueMail(Donor donor, string RenewalDate, string donorType)
        {
            if (donorType == "AUTOMATIC")
            {
                SendPaymentPastDue_9Automatic(donor, RenewalDate);
            }
            else
            {
                SendPaymentPastDue_9Manual(donor, RenewalDate);
            }

            UpdateRenewalMailByRID("ALERT PAYMENT PAST DUE (9)", donor, "SendPaymentPastDueMail");
        }






        protected void PaymentRegistrationCC_2(long mxregno, decimal Amount, string RenewalDate = "") // _2-Registracion: Payment Confirmation
        {
            //try
            //{
            //    //DETECT DONOR LANGUAGE
            //    string query = "SELECT LANGUAGE FROM MEMBER_Master WHERE REGNO=" + mxregno;
            //    DataTable dt = objDUT.GetDataTable(query);
            //    string Language = dt.Rows[0][0].ToString();
            //    String MessageMail = "";

            //    if (Language == "Spanish")
            //    {
            //        MessageMail = "<tr><td colspan='3' align='justify'><p style = 'text-align=justify'>Confirmamos que hemos recibido su donación por US $ " + Math.Round(Amount, 2) + ", correspondiente a su inscripción realizada el " + RenewalDate + ".</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'><p style = 'text-align=justify'>Le agradecemos desde el fondo de nuestros corazones por su apoyo tan apreciado.</p></td></tr>" +
            //        "<tr><td>Que Dios le bendiga.<br></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td>Atentamente,<br></td></tr>";
            //        SendFinalMail(MessageMail, mxregno, "Recibo por su Inscripción.");

            //    }
            //    else
            //    {
            //        MessageMail = "<tr><td colspan='3' align='justify'><p style = 'text-align=justify'>We confirm that we received your $ " + Math.Round(Amount, 2) + " donation corresponding to your " + RenewalDate + " Registration Payment.</p></td></tr>" +
            //     "<tr><td>&nbsp;</td></tr>" +
            //     "<tr><td colspan='3' align='justify'><p style = 'text-align=justify'>We thank you from the bottom of our heart for your so appreciated support.</p></td></tr>" +
            //     "<tr><td>God Bless you and your family<br></td></tr>" +
            //     "<tr><td>&nbsp;</td></tr>" +
            //     "<tr><td>Sincerely,<br></td></tr>";
            //        SendFinalMail(MessageMail, mxregno, "Registracion Confirmation.");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        protected void SendEndOfTrialPeriod_4(long mxregno, string message = "")  // END OF TRIAL PERIOD _4
        {
            //try
            //{
            //    //DETECT DONOR LANGUAGE
            //    string query = "SELECT LANGUAGE FROM MEMBER_Master WHERE REGNO=" + mxregno;
            //    DataTable dt = objDUT.GetDataTable(query);
            //    string Language = dt.Rows[0][0].ToString();
            //    String MessageMail = "";

            //    if (Language == "Spanish")
            //    {
            //        MessageMail = "<tr><td colspan='3' align='Justify'>El período de prueba de nuestros programas sin colaborar con alguna donación está llegando a su final. Pensamos que usted ha tenido tiempo de sobra para comprobar que nuestros programas son excepcionales.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='Justify'>Estamos seguros de que usted pudo registrar en nuestros programas a muchas personas muy deseosas de ayudar a una causa en especial.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Ha llegado el momento en que usted tiene que decidir convertirse en un donante en el programa de su elección.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Si usted no tiene ningún dinero en su Alcancía, usted tendrá que usar las herramientas disponibles en la sección \"Depositar fondos\" de su sección \"Cartera Electrónica-Alcancia\" para acreditar su cuenta. Una vez que eso está hecho, usted podrá pagar las donaciones de su inscripción y de su renovación mensual, usando la sección \"Mandar Donaciones\" de su Oficina Virtual.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Si es necesario, le recomendamos que vea de nuevo nuestros tutoriales en: cotdp.org para cualquier duda que pueda tener.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Es importante efectuar los pagos de sus donaciones para su inscripción y para su donación mensual antes del día 31 de su período de prueba de 30 días.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Si el pago no se realiza a tiempo, su cuenta podría ser tomada por COTDP.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>Aspiramos a que se registre porque estaríamos muy felices de contar con usted para que nos ayude a tener un impacto positivo en este mundo.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //          "<tr><td>God Bless you and your family<br></td></tr><tr><td>&nbsp;</td></tr><tr><td>Sincerely,<br></td></tr>";
            //        SendFinalMail(MessageMail, mxregno, "Fin del período de prueba.");
            //    }
            //    else
            //    {
            //        MessageMail = "<tr><td colspan='3' align='Justify'>Your free trial period is coming to an end and we are sure that you had plenty of time to try out our program.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='Justify'>We are confident that you have been able to recruit a lot of enthusiastic people ready to work for their own cause.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>It is time now to make the decision of becoming a donor in the program of your choice.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>If you do not have any money in your E-wallet account, you will have to use  the tools available in the section “Deposit Funds” of your “E-wallet section” to credit your account. Once done, you will pay the donations for your registration and monthly renewal using the “Renewal Account” section of your back office.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>It is time now to make the decision of becoming a donor in the program of your choice.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>If needed, we recommend you to watch our tutorials in our website: <a href=\"https://cotdp.org\">cotdp.org</a> for any doubt you might have.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>It is important  to make the payments of your donations for the registration and for the monthly donation before the day 31 of your 30 days trial period.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>If payment is not made on time, your account might be taken over by COTDP.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan='3' align='justify'>We count on you and we would be very happy to have you helping us to make a difference in this world.</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //          "<tr><td>God Bless you and your family<br></td></tr><tr><td>&nbsp;</td></tr><tr><td>Sincerely,<br></td></tr>";
            //        SendFinalMail(MessageMail, mxregno, "End Of Trial Period.");
            //    }


            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        protected void RenewalConfirmation_5(Donor donor, string amtnet, string RenewalDate = "") //RENEWAL CONFIRMATION 5
        {
            try
            {
                decimal Amount = donor.Donation;
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Confirmamos que hemos recibido su donación por US $ " + amtnet + " correspondiente a la renovación de " + RenewalDate + ".");
                    BodyMsg.Add("Le agradecemos desde el fondo de nuestro corazón por su apoyo tan apreciado.");
                    BodyMsg.Add("Que Dios le bendiga a usted así como a toda su familia.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Atentamente,");
                    BodyMsg.Add("");
                    Title = "Recibo por su Donación Mensual.";
                }
                else
                {
                    BodyMsg.Add("We confirm that we received your $" + amtnet + " donation corresponding to:" + RenewalDate + ".");
                    BodyMsg.Add("We thank you from the bottom of our heart for your so appreciated support.");
                    BodyMsg.Add("God Bless you and your family.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Sincerely,");
                    BodyMsg.Add("");
                    Title = "Renewal Confirmation.";
                }
                MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void DebitNotificationCC_6(long mxregno, decimal Amount, string RenewalDate = "") // _6 DEBIT NOTIFICATION
        {
            //try
            //{
            //    //DETECT DONOR LANGUAGE
            //    string query = "SELECT LANGUAGE FROM MEMBER_Master WHERE REGNO=" + mxregno;
            //    DataTable dt = objDUT.GetDataTable(query);
            //    string Language = dt.Rows[0][0].ToString();
            //    String MessageMail = "";

            //    if (Language == "Spanish")
            //    {
            //        MessageMail = "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>Tenemos el placer de informarle que el saldo de su cartera electrónica/Alcancía le permite pagar su donación de $ " + Math.Round(Amount, 2) + ".</p></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>Si es el pago de la donación de su renovación mensual de " + RenewalDate + " está pendiente y si su cuenta tiene suficiente dinero para pagarla, el valor correspondiente a esta donación se debitará de su cuenta en un minuto.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style ='text-align=justify'>Todas sus renovaciones mensuales estándar siempre se harán automáticamente, contando 27 días a partir del día del mes correspondiente a la fecha de aniversario de su registración.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td>Usted está ahora en el camino para un cambio de vida para usted y para las causas de muchas otras personas.</td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>Dios le bendiga.<br></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>Sinceramente,<br></td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Ya puede pagar.");

            //    }
            //    else
            //    {
            //        MessageMail = "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>We are very pleased to let you know that the balance of your E-wallet allows you to pay your registering donation of $ " + Math.Round(Amount, 2) + ".</p></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>If it is time for you to pay for your " + RenewalDate + " monthly renewal donation and your account has enough money to cover it, the corresponding amount will also be debited from your account within a minute.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style ='text-align=justify'>All your standard monthly renewals will always be done automatically counting 27 days from the day of the month corresponding to the anniversary date of your registration.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td>You are now on the path to a life changing for your and so many other people’s causes.</td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>God Bless you and your family.<br></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>Sincerely,<br></td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Debit Notification.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        protected void DebitNotificationCC_6B(long mxregno, decimal Amount, string RenewalDate = "") // _6 DEBIT NOTIFICATION
        {
            //try
            //{
            //    //DETECT DONOR LANGUAGE
            //    string query = "SELECT LANGUAGE FROM MEMBER_Master WHERE REGNO=" + mxregno;
            //    DataTable dt = objDUT.GetDataTable(query);
            //    string Language = dt.Rows[0][0].ToString();
            //    String MessageMail = "";

            //    if (Language == "Spanish")
            //    {
            //        MessageMail = "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>Tenemos el placer de informarle que el saldo de su cartera electrónica/Alcancía le permite pagar su donación de $ " + Math.Round(Amount, 2) + " correspondiente a " + RenewalDate + ".</p></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>Este mensaje es para informarle que la donación correspondiente a su renovación mensual se debitará de su cuenta en un minuto.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td colspan = '3' align='justify'><p style ='text-align=justify'>Le agradecemos por participar en nuestros programas.</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>Dios le bendiga.<br></td></tr>" +
            //         "<tr><td>&nbsp;</td></tr>" +
            //         "<tr><td>Sinceramente,<br></td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Ya puede pagar.");
            //    }
            //    else
            //    {
            //        MessageMail = "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>We are very pleased to let you know that the balance of your E-wallet allows you to pay your monthly renewal donation of $ " + Math.Round(Amount, 2) + " corresponding to " + RenewalDate + ".</p></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan = '3' align='justify'><p style = 'text-align=justify'>This email is to inform you that the donation corresponding to your monthly renewal payment will be debited from your account within a minute.</p></td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan = '3' align='justify'><p style ='text-align=justify'>We thank you for participating in our programs.</p></td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td>God Bless you and your family<br></td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //        "<tr><td>Sincerely,<br></td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Debit Notification.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        protected void PaymentRenewal3DaysBeforeEmail_7(Donor donor, string RenewalDate = "") // 7. PAYMENT-RENEWAL EMAIL
        {
            try
            {
                decimal Amount = donor.Donation;
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Esperamos que la experiencia que usted está teniendo con nuestra Iglesia y sus programas le esté  trayendo muchas bendiciones.");
                    BodyMsg.Add("Le invitamos una vez más a que siga apoyando nuestros programas. Usted sabe cómo su participación en nuestros programas ayuda a tanta gente necesitada.");
                    BodyMsg.Add("A fin de alcanzar nuestros objetivos, algo de dinero es necesario. Por este motivo, le pedimos que haga una donación por un valor de $ " + Math.Round(Amount, 2) + " correspondiente a su compromiso del " + RenewalDate + ". Esta cantidad le mantendrá como un donante registrado en el programa de su elección.");
                    BodyMsg.Add("Para los donantes que eligieron el sistema de pago automático, no tienen que hacer absolutamente nada. El monto se debitará automáticamente en la fecha indicada en el paragrafo siguiente.");
                    BodyMsg.Add("A partir del momento en que usted reciba este e-mail, usted tendrá que hacer su donación hasta la fecha de su renovación - la fecha de cumpleaños de su registración (" + RenewalDate + ").");
                    BodyMsg.Add("Si esta es su primera renovación, usted tendrá que ir probablemente a la sección \"Cartera Electrónica/ Alcancia > Depositar Fondos\" para depositar algún dinero en su cuenta. Usted podrá hacer entonces una renovación \"Manual\" usando la sección \"Centro Financiero> Mandar Donaciones\".");
                    BodyMsg.Add("Cuando su cuenta recibe suficiente dinero cada mes como para cubrir el monto de sus donaciones, le sugerimos que elija la opción \"Automático\" en la sección \"Centro financiero> Mandar Donaciones\".");
                    BodyMsg.Add("");
                    BodyMsg.Add("Agradecemos su participación en nuestros programas.");
                    BodyMsg.Add("Que Dios le bendiga.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Sinceramente,");
                    Title = "Es Tiempo de Renovar.";
                }
                else
                {
                    BodyMsg.Add("We hope the experience you are having with our Church and its programs is bringing you a lot of blessings.");
                    BodyMsg.Add("We encourage you to keep participating in those programs knowing that everybody is helping others who need it.");
                    BodyMsg.Add("In order to accomplish our goals, some money is needed. For this reason we are asking you to make a donation for the amount of $ " + Math.Round(Amount, 2) + " corresponding to your " + RenewalDate + " payment. This amount will keep you as a Registered Donor in the program of your choice.");
                    BodyMsg.Add("For the donors who chose automatic payment, the donation scheduled as indicated in the following paragraph will be debited automatically. No need to do anything.");
                    BodyMsg.Add("From the moment you receive this email, you will have to make your donation up to the date of your renewal—the birthday date of your registration (" + RenewalDate + ").");
                    BodyMsg.Add("If you just started in our programs, you will have to go to the “E-wallet>Deposit Funds” section to deposit some money into your account. You will then make a “Manual” renewal using the  “Financial Manager>Renewal Account” section.");
                    BodyMsg.Add("If or when your account is receiving enough money to cover your monthly donation, we recommend you to choose the “Automatic” option in the “Financial Manager>Renewal Account” section.");
                    BodyMsg.Add("");
                    BodyMsg.Add("We thank you for participating in our programs.");
                    BodyMsg.Add("");
                    BodyMsg.Add("God Bless you and your family.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Sincerely,");
                    Title = "It’s Time To Renew.";
                }
                MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void AutomaticPaymentConfirmation_8(Donor donor, string amtnet, string RenewalDate = "") //AUTOMATIC PAYMENT CONFIRMATION 8
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Confirmamos que hemos recibido el pago automático de $ " + amtnet + " correspondiente a su donación mensual para la fecha: " + RenewalDate + ".");
                    BodyMsg.Add("Le agradecemos del fondo de nuestro corazón por su apoyo tan apreciado.");
                    BodyMsg.Add("Que Dios le bendiga así como a toda su familia.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Atentamente,");
                    Title = "Recibo del Pago Automático Mensual.";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("We confirm that we received your automated payment of $" + amtnet + " corresponding to your monthly donation for the month of: " + RenewalDate + ".");
                    BodyMsg.Add("We thank you from the bottom of our heart for your so appreciated support.");
                    BodyMsg.Add("God Bless you and your family.");
                    BodyMsg.Add("Contact us if you cannot pay your donation for some serious reason. We do not want you to lose your account.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Sincerely,");
                    Title = "Automatic Payment Confirmation.";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendPaymentPastDue_9Automatic(Donor donor, string RenewalDate = "")  // Donantes Pagos Payment Past Due_9
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Le estamos enviando todas nuestras bendiciones esperando que usted esté teniendo una experiencia muy buena y positiva con nuestros programas.");
                    BodyMsg.Add("Este mensaje es para informarle que por alguna razón, su donación mensual automática correspondiente a la fecha " + RenewalDate + " no se concretó.");
                    BodyMsg.Add("Si usted no tiene fondos en su alcancía puede hacer un depósito.Le invitamos a hacer su pago manualmente lo más pronto posible.");
                    BodyMsg.Add("Póngase en contacto con nosotros si usted no puede pagar su donación por algún motivo serio. No queremos que usted pierda su cuenta.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Que Dios le bendiga,");
                    Title = "¡Alerta, Cuenta Morosa!";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("We are sending you all our blessings hoping that you are having a very good and positive experience with our programs.");
                    BodyMsg.Add("This message is to let you know that for any reason, yor monthly donation corresponding to the date " + RenewalDate + " did not materialized.");
                    BodyMsg.Add("If you don't have money in your donor balance then you can make a deposit. We invite you to make your payment as soon as possible in order to avoid having your account repossessed by our ministry.");
                    BodyMsg.Add("Contact us if you cannot pay your donation for some serious reason. We do not want you to lose your account.");
                    BodyMsg.Add("");
                    BodyMsg.Add("God bless you,");
                    Title = "Alert, Payment Past Due!";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendPaymentPastDue_9Manual(Donor donor, string RenewalDate = "")  // Donantes Pagos Payment Past Due_9
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Le estamos enviando todas nuestras bendiciones esperando que usted esté teniendo una experiencia muy buena y positiva con nuestros programas.");
                    BodyMsg.Add("Este mensaje es para informarle que no se ha recibido su donación mensual correspondiente a " + RenewalDate + " fecha.");
                    BodyMsg.Add("Le invitamos a hacer su pago lo más pronto rápidamente posible para evitar que su cuenta sea retomada por nuestro ministerio.");
                    BodyMsg.Add("Póngase en contacto con nosotros si usted no puede pagar su donación por algún motivo serio. No queremos que usted pierda su cuenta.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Que Dios le bendiga,");
                    Title = "¡Alerta, Cuenta Morosa!";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("We are sending you all our blessings hoping that you are having a very good and positive experience with our programs.");
                    BodyMsg.Add("This message is to let you know that for any reason, yor monthly donation corresponding to the date " + RenewalDate + " did not materialized.");
                    BodyMsg.Add("If you don't have money in your donor balance then you can make a deposit. We invite you to make your payment as soon as possible in order to avoid having your account repossessed by our ministry.");
                    BodyMsg.Add("Contact us if you cannot pay your donation for some serious reason. We do not want you to lose your account.");
                    BodyMsg.Add("");
                    BodyMsg.Add("God bless you,");
                    Title = "Alert, Payment Past Due!";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendSadnessRepossessedAccount_10(Donor donor, string RenewalDate)  // Sadness: Repossessed Account_10
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Le hemos enviado un email el " + donor.RenewalDate.AddDays(-3).Date.ToShortDateString() + " para recordarle que debía hacer su donación mensual correspondiente al: " + RenewalDate + ". Le hemos enviado otro e-mail el " + donor.RenewalDate.AddDays(1).ToShortDateString() + " para informarle que no habíamos recibidos ningún pago correspondiente a esta donación. Hasta el día de hoy no hemos recibido ninguna donación o contacto de parte de usted.");
                    BodyMsg.Add("Tenemos la obligación de garantizar que los programas estén funcionando adecuadamente y que nadie este bloqueado en el proceso de recolección de fondos para su causa. Como usted no está donando su tiempo y dinero a nuestros programas, su cuenta ahora está impidiendo que otros donantes puedan alcanzar sus metas.");
                    BodyMsg.Add("Es con una profunda tristeza que le estamos bloqueando el acceso a su cuenta a partir del día de hoy. Vamos a esperar unos días más antes de eliminar el acceso a su cuenta de forma definitiva.");
                    BodyMsg.Add("Esperamos que usted se comunique con nosotros lo más pronto posible para que no vaya a perder la cuenta que abrió en nuestros programas.");
                    BodyMsg.Add("Nos despedimos de usted,");
                    BodyMsg.Add("");
                    BodyMsg.Add("Atentamente,");
                    Title = "¡Comuníquese con Nosotros Por Favor!";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("We sent you an email on " + donor.RenewalDate.AddDays(-3).Date.ToShortDateString() + " to inform you about your upcoming monthly renewal donation corresponding to: " + RenewalDate + ". We sent you another email on " + donor.RenewalDate.AddDays(1).Date.ToShortDateString() + " to let you know that this payment was past due and consequently we would have to repossess your account if we did not receive your donation or if we did not receive any contact from you. No donation or contact has been received from you until today.");
                    BodyMsg.Add("We have the obligation to make sure the programs are working properly and that nobody will be blocked in the process of collecting the funds for their cause. Because you are not donating your time and money to the programs, your account is now blocking other peoples to achieving their goals.");
                    BodyMsg.Add("It is with a very deep sadness that we are blocking you from accessing your account starting today. We will wait a few more days before taking over your account permanently.");
                    BodyMsg.Add("We hope to hear from you before you lose the account you opened in our program.");
                    BodyMsg.Add("");
                    BodyMsg.Add("Warms Regards,");
                    Title = "You Need To Contact Us.";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendForgotRenewal_11(Donor donor)  // Sadness: Repossessed Account_10
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Le agradecemos mucho por su decisión de registrarse en uno de nuestros programas. Usted eligió la opción de probar gratuitamente nuestros programas por 30 días y eso implica que al final del tiempo de prueba, usted esté pagando su inscripción así como su primera donación mensual.");
                    BodyMsg.Add("Por alguna razón, recibimos el pago de la registración, pero no recibimos ningún pago para la renovación mensual.");
                    BodyMsg.Add("Pensemos que esta situación se debe seguramente al hecho que usted está recién empezando con nuestros programas y no se percató de este compromiso.");
                    BodyMsg.Add("Le invitamos a que haga su pago lo más pronto posible para evitar que su cuenta se encuentre bajo la amenaza de ser suspendida.");
                    BodyMsg.Add("Contáctenos si usted está atravesando por cualquier situación que podría justificar su morosidad. No queremos que usted pierda su cuenta,");
                    BodyMsg.Add("");
                    BodyMsg.Add("¡Que Dios le bendiga!");
                    Title = "Olvidó pagar su renovación mensual.";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("We thank you for your decision to become a registered donor in our program. You chose the 30 days free trial option which implies the payment of the registration at the end of the 30 days trial period as well as the payment of the renewal monthly donation.");
                    BodyMsg.Add("For some reasons, we received the payment for the registration but we did not receive any payment for the monthly renewal.");
                    BodyMsg.Add("We invite you to make your payment as soon as possible in order to avoid having your account repossessed by our ministry.");
                    BodyMsg.Add("Contact us if you cannot pay your donation for some serious reason. We do not want you to lose your account,");
                    BodyMsg.Add("God bless you!,");
                    BodyMsg.Add("");
                    BodyMsg.Add("");
                    BodyMsg.Add("");
                    Title = "You Forgot Your Renewal.";
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendFTPaymentPastDue_12(long mxregno, string message = "", string RenewalDate = "", string FifthDate = "")  // FreeTrial Payment Past Due_12
        {
            //try
            //{
            //    //DETECT DONOR LANGUAGE
            //    string query = "SELECT LANGUAGE FROM MEMBER_Master WHERE REGNO=" + mxregno;
            //    DataTable dt = objDUT.GetDataTable(query);
            //    string Language = dt.Rows[0][0].ToString();
            //    String MessageMail = "";

            //    if (Language == "Spanish")
            //    {

            //        MessageMail = "<tr><td colspan='3' align='Justify'>Le estamos mandando todas nuestras bendiciones esperando que usted esté teniendo una experiencia muy buena y positiva con nuestros programas.</td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //      "<tr><td colspan='3' align='Justify'>Le queremos informar que el tiempo de prueba de 30 días terminó y no hemos recibido ninguna donación para su registración y su renovación mensual correspondientes al " + RenewalDate + ".</td></tr>" +
            //      "<tr><td>&nbsp;</td></tr>" +
            //      "<tr><td colspan='3' align='Justify'>Le invitamos a que haga su pago lo más pronto posible para evitar que su cuenta esté bajo amenaza de ser tomada por nuestro ministerio. Si usted no quiere seguir participando en nuestros programas, tenga la gentileza de mandarnos un correo para informarnos de su decisión. De esta manera usted no bloqueará a su patrocinador que ha tenido solamente muy buenas intenciones hacia usted. </td></tr>" +
            //      "<tr><td>&nbsp;</td></tr>" +
            //      "<tr><td colspan='3' align='Justify'>Póngase en contacto con nosotros si usted no puede pagar sus donaciones por algún motivo serio. No queremos que usted pierda su cuenta.</td></tr>" +
            //      "<tr><td>&nbsp;</td></tr>" +
            //      "<tr><td colspan='3' align='justify'>Que Dios le bendiga.</td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Cuenta en Período de Prueba - Morosa.");
            //    }
            //    else
            //    {
            //        MessageMail = "<tr><td colspan='3' align='Justify'>We are sending you all our blessings hoping that you are having a very good and positive experience with our programs.</td></tr>" +
            //        "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan='3' align='Justify'>This message is to let you know that your " + Convert.ToDateTime(RenewalDate).ToShortDateString() + " donations for your registration and your monthly donation are now past due.</td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan='3' align='Justify'>We invite you to make your payment as soon as possible in order to avoid having your account repossessed by our ministry. </td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan='3' align='Justify'>Contact us if you cannot pay your dues for some serious reason. We do not want you to lose your account.</td></tr>" +
            //       "<tr><td>&nbsp;</td></tr>" +
            //       "<tr><td colspan='3' align='justify'>God bless you,</td></tr>";
            //        objDUT.EnviarCorreoLP(mxregno, GetLoginId(mxregno), MessageMail, "Free Trial, Payment Past Due.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        protected void SendNoticeClosedAccount_13(Donor donor, string RenewalDate = "")  // _13- Notice of Closed Account
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Estamos enviando este aviso final para informarle que COTDP ha recuperado definitivamente la cuenta que usted abrió en nuestro programa.");
                    BodyMsg.Add("Esperamos que usted esté bien y le rogamos que se ponga en contacto con nosotros si es que usted ha sufrido algún evento inesperado que ha perjudicado su comunicación con nosotros.");
                    BodyMsg.Add("Le agradecemos su intención de haber querido ayudarnos en tener un impacto positivo en este mundo. Le enviamos todas nuestras bendiciones.");
                    BodyMsg.Add("Respetuosamente,");
                    Title = "¡Cuenta Cerrada!";
                }
                else
                {
                    BodyMsg.Add("We are sending this final notice to let you know that this account you opened in our program has been definitively repossessed by Cotdp.");
                    BodyMsg.Add("We hope you are well and we are asking you to contact us if you suffered from some unexpected event that impaired you from communicating with us.");
                    BodyMsg.Add("We appreciate your intent to make a difference in this world and we are sending you all our blessings.");
                    BodyMsg.Add("Respectfully,");
                    Title = "We Closed Your account!";
                }
                MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SendInsufficientFunds_14(Donor donor)  // _15-Insufficient Funds
        {
            try
            {
                string Title = "";
                List<string> BodyMsg = new List<string>();
                string MessageMail = "";

                if (donor.Language.ToUpper() == "SPANISH")
                {
                    BodyMsg.Add("Gracias por participar en nuestros programas y por usar nuestro sistema de pago automático.");
                    BodyMsg.Add("Por alguna razón, no hubo suficientes fondos en su cartera electrónica/alcancía para poder pagar su renovación mensual durante el proceso automático.");
                    BodyMsg.Add("Por favor, vea cual es la razón que causó esta situación y si es necesario, deposite algún dinero en su cuenta para poder pagar manualmente su donación mensual usando el Centro Financiero> Mandar Donaciones.");
                    BodyMsg.Add("Que disfrute de un día muy bendecido,");
                    BodyMsg.Add("Nos despedimos de usted,");
                    BodyMsg.Add("");
                    Title = "Pago Automático: Fondos Insuficientes.";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                else
                {
                    BodyMsg.Add("Thank you for participating in our programs and for using our automatic payment system.");
                    BodyMsg.Add("For some reason, the funds in your e-wallet do not cover the amount needed to renew your monthly donation.");
                    BodyMsg.Add("Please look into the issue or/and deposit some money into your account to pay manually   your monthly donation using the Financial Manager>Renewal Account.");
                    BodyMsg.Add("Wishing you a blessed day,");
                    BodyMsg.Add("");
                    BodyMsg.Add("Warms Regards,");
                    Title = "Automatic Payment: Insufficient funds";
                    MessageMail = objDUT.GetStandardFormatMail(Title, Title, BodyMsg, donor.regno, donor.Language);
                }
                objDUT.EnviarCorreoLP(donor.regno, donor.LoginId, MessageMail, Title, donor.EmailId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region "Renewal Auxiliar Processes"
        public string CheckIfDonorForgotPayRenewal(Donor donor)
        {
            string LoginID = donor.LoginId;
            int RID = donor.RID;
            long regno = donor.regno;
            WriteLog(LoginID + ": CHEQUEO DE OLVIDO DE PAGO.", 500);

            //Check if the Donor Transaction, previous to this RID number, was the registration payment
            string query = "SELECT * FROM RENEWAL_MAIL_LP WHERE REGNO=" + regno + " AND RID <= " + RID + " ORDER BY RID DESC";
            DataTable DT = objDUT.GetDataTable(query);

            //SI EL PAGO DE LA REGISTRACIÓN YA SE HIZO PERO LA PRÓXIMA REGISTRACIÓN NO (CON TIEMPO YA PARA HACERLA), DEBO ENVIAR EL CORREO 11
            if (DT.Rows[1]["RENEWALTYPE"].ToString() == "REGISTRATION" && DT.Rows[1]["STATUSTRANSACTION"].ToString() == "PAID" && DT.Rows[0]["MAILSENT"].ToString() != "FORGOT YOUR RENEWAL (11)" && Convert.ToDateTime(DT.Rows[1]["REGISTERDATE"]).Date.AddDays(1) == DateTime.Now.Date)
            {
                return "ENVIARCORREO11";
            }
            else
            {
                return "NOENVIARCORREO11";
            }
        }


        public int CheckAdminBoxes33_37(long regno)
        {
            SqlParameter[] arrParam = new SqlParameter[2];
            arrParam[0] = new SqlParameter("@NameProcess", SqlDbType.VarChar, 200);
            arrParam[0].Value = "PROFILE CHANGE: " + regno;
            arrParam[1] = new SqlParameter("@intResult", SqlDbType.Int, 4);
            arrParam[1].Direction = ParameterDirection.Output;
            int result = objDUT.ExecuteSqlSP(arrParam, "SP_COMPROBARCAJA33");
            string query1 = "select TOP 1 Total142630,Total4 from tblcaja33 ORDER BY ID DESC";
            decimal Total142630, Total4; DataTable dt0;
            dt0 = objDUT.GetDataTable(query1);
            Total142630 = Convert.ToDecimal(dt0.Rows[0][0].ToString());
            Total4 = Convert.ToDecimal(dt0.Rows[0][1].ToString());
            query1 = "SELECT LOGINID FROM MEMBER_MASTER WHERE REGNO='" + regno + "'";
            dt0 = objDUT.GetDataTable(query1);
            string LoginID = dt0.Rows[0][0].ToString();
            if (Math.Abs(Total142630 - Total4) > 1)
            {
                //Enviar correo indicando la diferencia en caja33
                EmailAlertaCaja33(regno, "RENEWAL OR REGISTRATION " + LoginID + "/" + regno + " MODIFYING ADMIN BOX # 33");
                return 0;
            }
            return 1;
        }

        public static int EmailAlertaCaja33(long regno, string msg)
        {
            try
            {
                DataUtility objDUT = new DataUtility();
                String ToEmail, TOC;
                DataTable dtM = objDUT.GetDataTable("Select mm.title,mm.memcode,mm.loginid,ml.Password,mm.Emailid,mm.regdate,(isnull(title,'')+' '+fname+' '+isnull(mname,'')+' '+isnull(lname,'')) as name From member_Master mm, member_login ml where mm.regno=ml.regno and mm.regno='" + regno + "'");
                //ToEmail = dtM.Rows[0]["Emailid"].ToString();
                ToEmail = "lisander23@gmail.com";
                String SBody;
                SBody = "<!DOCTYPE html><html lang = \"en\"><head><meta charset = \"utf-8\"><meta http - equiv = \"X-UA-Compatible\" content = \"IE=edge\">" +
               "<meta name = \"viewport\" content = \"width=device-width, initial-scale=1\"><title>Alerta Caja 33</title><link href " +
               "= \"css/bootstrap.min.css\" rel = \"stylesheet\"></head><body><script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\">" +
               "</script><script src = \"js/bootstrap.min.js\"></script><div class=\"row\" style=\"text-align:center;border-bottom:10px solid White;" +
               "background-repeat: no-repeat; background-size: 100% 100%;background-image:url('https://cotdp.org/members/bom/images/opaco.png')\"><br/>" +
               "<div style=\"padding-bottom:20px\"><img src='https://cotdp.org/Members/Images/logo.png' height='100px'  alt='Church Of Divine Prospect'/>" +
               "</div></div><div class=\"row\" style=\"background-repeat: no-repeat; background-size: cover;background-image:url('https://cotdp.org/wp-content/uploads/2017/08/fondo3.png')\">" +
               "<div><table cborder = '0' border='0' cellspacing='0' cellpadding='0' style='Padding-Left:25px;Padding-right:25px;padding-top:0;font-size:" +
               "15px; width: 100%;font-family:Arial'>" +
               "<tr><td>&nbsp;</td></tr>" +
               "<tr><td style=\"color:black;font-size:140%;text-shadow: 2px 2px 5px orange;font-weight:bold\"></td><td colspan = '2' align= 'right' style= \"font-size:medium;font-weight:normal\">" + DateTime.Now.ToShortDateString() + "</td></tr>" +
               "<tr><td>&nbsp;</td></tr><tr><td align = 'left' ><h4  style='font-weight:bold;'>" + dtM.Rows[0]["Name"].ToString() + "</h4></td></tr><tr>" +
               "<td align = 'left' colspan= '2'>Username: " + dtM.Rows[0]["loginid"].ToString() + "</td></tr>" +
               "<td align = 'left' colspan= '2'>ID#: " + dtM.Rows[0]["memcode"].ToString() + "</td></tr>" +
               "<tr><td>&nbsp;</td></tr>" +
               "<tr><td colspan = '3' align='justify'><p style ='text-align=justify'>Se descuadró la caja 33 con " + msg + "</p></td></tr>" +
               "<tr><td>&nbsp;</td></tr>" +
               "<tr><td><div style=\"font-size:xx-small\">This email was automatically generated from a mailbox that is not monitored. If you have any questions, please contact us.</div></td></tr>" +
               "<tr><td>&nbsp;</td></tr>" +
               "</table></div></body></div></html>";

                objDUT.EnviarCorreoLP(0, "", SBody, "Payment: Registration Confirmation ", "lisander23@gmail.com");

                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion


        #region "Payment Request Processes"
        public string CrearSolicitudDePago(decimal Amount, string remark, long regno, string Recurrente = "NO")
        {
            try
            {
                long ACID = 0;

                SqlParameter[] arrParam = new SqlParameter[10];
                arrParam[0] = new SqlParameter("@mRegNo", SqlDbType.BigInt);
                arrParam[0].Value = regno.ToString();
                arrParam[1] = new SqlParameter("@Amount", SqlDbType.Money);
                arrParam[1].Value = (Amount);
                arrParam[2] = new SqlParameter("@BankName", SqlDbType.VarChar, 100);
                arrParam[2].Value = "";
                arrParam[3] = new SqlParameter("@swiftCode", SqlDbType.VarChar, 100);
                arrParam[3].Value = "0000";
                arrParam[4] = new SqlParameter("@accountName", SqlDbType.VarChar, 100);
                arrParam[4].Value = "";
                arrParam[5] = new SqlParameter("@accountNumber", SqlDbType.VarChar, 100);
                arrParam[5].Value = "";
                arrParam[6] = new SqlParameter("@accountType", SqlDbType.VarChar, 100);
                arrParam[6].Value = "";
                arrParam[7] = new SqlParameter("@branch", SqlDbType.VarChar, 100);
                arrParam[7].Value = "";
                arrParam[9] = new SqlParameter("@Remark", SqlDbType.VarChar, 100);
                arrParam[9].Value = remark;
                arrParam[8] = new SqlParameter("@intResult", SqlDbType.Int);
                arrParam[8].Direction = ParameterDirection.Output;
                int res = objDUT.ExecuteSqlSP(arrParam, "SP_RequestWithDrwa");
                if (res == 0)
                {
                    WriteLog("Error tipo res=0 execuiing SP_RequestWithDrwa for regno " + regno);
                }
                if (res == 2)
                {
                    WriteLog("Error: Donor " + regno + " doesn't have enough funds to cover this withdrawal request. Res=2");
                }
                if (res == 3)
                {
                    WriteLog("Error: Donor " + regno + " doesn't have enough funds to cover this withdrawal request. Res=3");
                }
                if (res == 4)
                {
                    WriteLog("Error: Donor " + regno + " Amount Invalid. Res=4");
                }
                if (res == 5)
                {
                    WriteLog("Error: Donor " + regno + " Res=5.");
                }
                if (res == 6)
                {
                    WriteLog("Error: Donor " + regno + " Res=6.");
                }
                if (res == 1)
                {
                    //GET ACID FROM THE PAYMENT REQUEST CREATED
                    string query = "SELECT * FROM MEMBER_ACCOUNT WHERE REGNO=" + regno.ToString() + " AND DEBIT=" + Amount + " ORDER BY ACID DESC";
                    DataTable dt0 = objDUT.GetDataTable(query);
                    ACID = Convert.ToInt32(dt0.Rows[0]["ACID"].ToString());

                    query = "UPDATE MEMBER_ACCOUNT SET RECURRENTPAYMENT='" + Recurrente + "' WHERE ACID=" + ACID;
                    objDUT.ExecuteSql(query);
                }
                return ACID.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }



        public ServiceResponse RunOneTimePaymentRequest()
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                WriteLog("COMIENZA PAGO DE SOLICITUDES ONE TIME", 1000);
                //EXECUTE SCHEDULED WITHDRAWALS REQUEST FOR ONLY ONE TIME (FRECUENCY=FOR)
                DateTime NextExecution;
                string NextExecutionString = "";
                long ID = 0;
                DateTime DateScheduled, LastDateExecution;
                string ACID = "0", remark = "";
                decimal Amount = 0;
                long regno = 0;
                string query = "SELECT * FROM WithdrawalRequestMethods WHERE FRECUENCY='SCHEDULED FOR' AND STATUS='ACTIVE' AND ACID=0 ORDER BY ID";
                DataTable dt = objDUT.GetDataTable(query);
                foreach (DataRow Fila in dt.Rows)
                {
                    ID = Convert.ToInt64(Fila["ID"].ToString());
                    DateScheduled = Convert.ToDateTime(Fila["DateScheduled"].ToString());

                    if (Fila["LastDateExecution"].ToString() == null || Fila["LastDateExecution"].ToString() == "")
                    {
                        if (DateScheduled == DateTime.Today)
                        {
                            regno = Convert.ToInt64(Fila["regno"].ToString());
                            Amount = Convert.ToDecimal(Fila["Amount"].ToString());
                            remark = Fila["remark"].ToString();
                            ACID = CrearSolicitudDePago(Amount, remark, regno, "NO");
                        }


                        if (Fila["Frecuency"].ToString() == "WEEKLY")
                        {
                            NextExecution = Convert.ToDateTime(objDUT.GetTimeLP2()).AddDays(7);
                            NextExecutionString = NextExecution.ToString("yyyy-MM-dd");
                        }
                        else if (Fila["Frecuency"].ToString() == "MONTHLY")
                        {
                            NextExecution = Convert.ToDateTime(objDUT.GetTimeLP2()).AddMonths(1);
                            NextExecutionString = NextExecution.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            NextExecutionString = "";
                        }
                        query = "UPDATE WithdrawalRequestMethods SET DateScheduled='" + NextExecutionString + "', LASTDATEEXECUTION='" + objDUT.GetTimeLP2() + "', ACID=" + Convert.ToInt64(ACID) + ", CONDITION='TRANSFERRED TO ADMIN' WHERE ID=" + ID;
                        objDUT.ExecuteSql(query);
                    }
                }
                WriteLog("FINALIZA PAGO DE SOLICITUDES ONE TIME", 1000);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
            }
            return response;
        }

        public string AgendarNextExecution(string Frecuency)
        {
            DateTime NextExecutionDate;
            string NextExecutionDatestring = "";
            if (Frecuency == "WEEKLY")
            {
                NextExecutionDate = DateTime.Today.AddDays(7);
                NextExecutionDatestring = NextExecutionDate.ToString("yyyy/MM/dd");
            }
            else if (Frecuency == "MONTHLY")
            {
                NextExecutionDate = DateTime.Today.AddMonths(1);
                NextExecutionDatestring = NextExecutionDate.ToString("yyyy/MM/dd");
            }

            return NextExecutionDatestring;
        }

        public void CreateWithdrawalRequestToKeepRecurrency(long IDRequest)
        {

            string query = "SELECT * FROM WithdrawalRequestMethods WHERE ID=" + IDRequest;
            DataTable DT = objDUT.GetDataTable(query);
            string Regno, PaymentMethod, EmailPaypal, XoomName, XoomLastName, XoomAddress, XoomEmail, XoomPhone, XoomBankName,
            XoomAccountNumber, XoomAccountType, Alias, Status, LastDateExecution, RecurrentPayment, DayOfMonthRecurrent,
            Amount, Frecuency, DayOfWeek, Remark, DateCreated, DateScheduled = "";
            Regno = DT.Rows[0]["Regno"].ToString();
            PaymentMethod = DT.Rows[0]["PaymentMethod"].ToString();
            EmailPaypal = DT.Rows[0]["EmailPaypal"].ToString();
            XoomName = DT.Rows[0]["XoomName"].ToString();
            XoomLastName = DT.Rows[0]["XoomLastName"].ToString();
            XoomAddress = DT.Rows[0]["XoomAddress"].ToString();
            XoomEmail = DT.Rows[0]["XoomEmail"].ToString();
            XoomPhone = DT.Rows[0]["XoomPhone"].ToString();
            XoomBankName = DT.Rows[0]["XoomBankName"].ToString();
            XoomAccountNumber = DT.Rows[0]["XoomAccountNumber"].ToString();
            XoomAccountType = DT.Rows[0]["XoomAccountType"].ToString();
            XoomAccountType = DT.Rows[0]["XoomAccountType"].ToString();
            Alias = DT.Rows[0]["Alias"].ToString();
            Status = DT.Rows[0]["Status"].ToString();
            LastDateExecution = DT.Rows[0]["LastDateExecution"].ToString();
            RecurrentPayment = DT.Rows[0]["RecurrentPayment"].ToString().Trim();
            DayOfMonthRecurrent = DT.Rows[0]["DayOfMonthRecurrent"].ToString();
            Amount = DT.Rows[0]["Amount"].ToString();
            Frecuency = DT.Rows[0]["Frecuency"].ToString();
            DayOfWeek = DT.Rows[0]["DayOfWeek"].ToString();
            Remark = DT.Rows[0]["Remark"].ToString();
            DateCreated = DT.Rows[0]["DateCreated"].ToString();
            //DateScheduled = DT.Rows[0]["DateScheduled"].ToString();

            if (Frecuency == "WEEKLY")
            {
                //DateScheduled = Next Week
                DateScheduled = DateTime.Today.AddDays(7).ToString("yyyy/MM/dd");
            }
            else if (Frecuency == "MONTHLY")
            {
                //DateScheduled = Next Month
                DateScheduled = DateTime.Today.AddMonths(1).ToString("yyyy/MM/dd");
            }


            query = "insert into WithdrawalRequestMethods(regno,PaymentMethod,EmailPaypal,XoomName,XoomLastName,XoomAddress," +
                "XoomEmail,XoomPhone,XoomBankName,XoomAccountNumber,XoomAccountType,Alias,Status,LastDateExecution,RecurrentPayment," +
                "DayOfMonthRecurrent,Amount,Frecuency,DayOfWeek,Remark,ACID,DateCreated,DateScheduled) VALUES(" + Regno +
                ",'" + PaymentMethod + "','" + EmailPaypal + "','" + XoomName + "','" + XoomLastName + "','" + XoomAddress +
                "','" + XoomEmail + "','" + XoomPhone + "','" + XoomBankName + "','" + XoomAccountNumber + "','" + XoomAccountType +
                "','" + Alias + "','ACTIVE','" + objDUT.GetTimeLP2() + "','" + RecurrentPayment + "','" + DayOfMonthRecurrent +
                "'," + Amount + ",'" + Frecuency + "','" + DayOfWeek + "','" + Remark + "',0,'" + Convert.ToDateTime(DateCreated) + "','" + Convert.ToDateTime(DateScheduled) + "')";
            objDUT.ExecuteSql(query);
        }

        public void RunRecurrentPaymentRequests()
        {
            //EXECUTE WITHDRAWALS REQUEST RECURRENT
            WriteLog("COMIENZA PAGO DE SOLICITUDES RECURRENTES", 1000);

            WriteLog("FECHA Y HORA ACTUAL:" + DateTime.Now + ". " + DateTime.Now.ToShortDateString(), 1000);


            string query = "SELECT * FROM WithdrawalRequestMethods WHERE RECURRENTPAYMENT='YES' AND STATUS='ACTIVE' AND ACID=0 ORDER BY ID";
            DataTable dt = objDUT.GetDataTable(query);
            string ACID = "", remark = "", NextExecutionString = "";
            DateTime NextExecution;
            foreach (DataRow Fila in dt.Rows)
            {
                long ID = Convert.ToInt64(Fila["ID"].ToString());
                if (Fila["DateScheduled"].ToString() != "" && Fila["DateScheduled"].ToString() != null)
                {
                    if (Fila["RecurrentPayment"].ToString().Trim() == "YES" && Convert.ToDateTime(Fila["DateScheduled"]).ToString("yyyy/MM/dd") == DateTime.Now.ToString("yyyy/MM/dd"))
                    {
                        if (Fila["LastDateExecution"].ToString() != null && Fila["LastDateExecution"].ToString() != "")
                        {
                            if (Convert.ToDateTime(Fila["LastDateExecution"].ToString()).ToString("yyyy/MM/dd") != DateTime.Today.ToString("yyyy/MM/dd"))
                            {
                                ACID = CrearSolicitudDePago(Convert.ToDecimal(Fila["Amount"].ToString()), remark, Convert.ToInt32(Fila["Regno"].ToString()), "YES");
                                WriteLog("WITHDRAWAL ID:" + ID + " PROCESSED. ACID: " + ACID, 1000);

                                if (Fila["Frecuency"].ToString() == "WEEKLY")
                                {
                                    NextExecution = Convert.ToDateTime(objDUT.GetTimeLP2()).AddDays(7);
                                    NextExecutionString = NextExecution.ToString("yyyy-MM-dd");
                                }
                                else if (Fila["Frecuency"].ToString() == "MONTHLY")
                                {
                                    NextExecution = Convert.ToDateTime(objDUT.GetTimeLP2()).AddMonths(1);
                                    NextExecutionString = NextExecution.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    NextExecutionString = "";
                                }
                                //ACTUALIZO LA SOLICITUD COMO TRANSFERIDA A ADMIN Y DEBO CREAR OTRA IGUAL PARA QUE SE MANTENGA LA RECURRENCIA
                                query = "UPDATE WithdrawalRequestMethods SET LASTDATEEXECUTION='" + objDUT.GetTimeLP2() + "', ACID=" + Convert.ToInt64(ACID) + ", CONDITION='TRANSFERRED TO ADMIN' WHERE ID=" + ID;
                                objDUT.ExecuteSql(query);
                                CreateWithdrawalRequestToKeepRecurrency(ID);
                                WriteLog("WITHDRAWAL ID:" + ID + " NEXT EXECUTION SCHEDULED TO " + NextExecutionString, 1000);
                            }
                        }
                        else
                        {
                            ACID = CrearSolicitudDePago(Convert.ToDecimal(Fila["Amount"].ToString()), remark, Convert.ToInt32(Fila["Regno"].ToString()), "YES");
                            WriteLog("WITHDRAWAL ID:" + ID + " PROCESSED. ACID: " + ACID, 1000);
                            NextExecutionString = AgendarNextExecution(Fila["Frecuency"].ToString());
                            query = "UPDATE WithdrawalRequestMethods SET DateScheduled='" + NextExecutionString + "', LASTDATEEXECUTION='" + objDUT.GetTimeLP2() + "', ACID=" + Convert.ToInt64(ACID) + ", CONDITION='TRANSFERRED TO ADMIN' WHERE ID=" + ID;
                            objDUT.ExecuteSql(query);
                            CreateWithdrawalRequestToKeepRecurrency(ID);
                            WriteLog("WITHDRAWAL ID:" + ID + " NEXT EXECUTION SCHEDULED TO " + NextExecutionString, 1000);
                        }
                    }
                    else
                    {
                        WriteLog("WITHDRAWAL ID:" + ID + " NOT PROCESSED. RecurrentPayment field is not YES (" + Fila["RecurrentPayment"].ToString().Trim() + ") OR DateScheduled (" + Convert.ToDateTime(Fila["DateScheduled"]).ToString("yyyy/MM/dd") + ") is different than Today (" + DateTime.Now.ToString("yyyy/MM/dd") + ")." + DateTime.Now, 1000);
                    }
                }
                else
                {
                    WriteLog("WITHDRAWAL ID:" + ID + " NOT PROCESSED. DateScheduled is Null/Empty", 1000);
                }
            }
            WriteLog("FINALIZA PAGO DE SOLICITUDES RECURRENTES", 1000);
        }
        #endregion


        #region "Utilities"
        public void WriteLog(string msg, int waitTime = 0)
        {
            utility.WriteLog(msg, waitTime);
        }
        #endregion
    }
}