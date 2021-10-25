using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace COTDP.Services
{
    public class Utility
    {
        DataUtility objDUT = new DataUtility();
        public Utility()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        ///<Summry>
        ///This method check the permission of an user on page
        /// </Summry>
        /// <param name="userId">bigint</param>
        /// <param name="PageName">string</param>
        /// <returns>bool</returns>
        public Boolean isAccessible(Int64 userId, string pageName)
        {
            Boolean Flag = true;
            Int32 result = 0;
            SqlParameter[] arrParam = new SqlParameter[3];
            arrParam[0] = new SqlParameter("@userId", SqlDbType.BigInt); // change to bigint
            arrParam[0].Value = userId;
            arrParam[1] = new SqlParameter("@pageName", SqlDbType.VarChar, 50); // change to bigint
            arrParam[1].Value = pageName;
            arrParam[2] = new SqlParameter("@intResult", SqlDbType.Int);
            arrParam[2].Direction = ParameterDirection.Output;

            result = Convert.ToInt32(objDUT.ExecuteSqlSP(arrParam, "USP_checkPermission").ToString());
            if (result == 0)
                Flag = false;
            else
                Flag = true;
            return Flag;
        }


        /// <summary>
        /// This method check input string is numeric or not.
        /// If numeric return true else false.
        /// Creation Date : 23/10/2007.
        /// </summary>
        /// <param name="strValue">string</param>
        /// <returns>bool</returns>
        public bool IsNumeric(string strValue)
        {
            string strValidChars = "0123456789";
            bool Flag = true;
            for (int i = 0; i < strValue.Length && Flag == true; i++)
            {
                char s = Convert.ToChar(strValue.Substring(i, 1));
                if (strValidChars.IndexOf(s) == -1)
                {
                    Flag = false;
                }
            }
            return Flag;

        }
        /// <summary>
        /// This method is used to get registration number.
        /// Pass loginid or Login Id and Zone Code as argument .
        /// Creation Date : 23/10/2007.
        /// Updation Date : 01/10/2010.
        /// </summary>
        /// <param name="strValue">string</param>
        /// <param name="zone">string</param>
        /// <returns>long, Registration Number</returns>
        public long GetRegistrationNo(string strValue)
        {
            long lngRegNo = 0;
            long lngRegNos = 0;
            bool Flag = IsNumeric(strValue);
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();
            if (Flag)
            {

                string lngRandomid = Convert.ToString(strValue.Trim());
                string strSql = " select regno from Member_master where randomid = @lngRandomid ";
                SqlParameter[] arrParam = new SqlParameter[1];
                arrParam[0] = new SqlParameter("@lngRandomid", SqlDbType.VarChar, 50); // change to bigint
                arrParam[0].Value = lngRandomid;
                SqlDataReader dReader = objDUT.GetDataReader(arrParam, strSql);
                while (dReader.Read())
                {
                    lngRegNo = Convert.ToInt64(dReader["Regno"]);
                }
                return lngRegNo;
            }
            else
            {
                string strLoginId = strValue.Trim();
                string strSqlRegno = " select regno from Member_master where loginid = @strLoginId ";
                SqlParameter[] arrParams = new SqlParameter[1];
                arrParams[0] = new SqlParameter("@strLoginId", SqlDbType.VarChar, 50);
                arrParams[0].Value = strLoginId;
                //arrParams[1] = new SqlParameter("@zoneCode", SqlDbType.VarChar, 50); // change to bigint
                //arrParams[1].Value = zone;
                SqlDataReader dReader = objDUT.GetDataReader(arrParams, strSqlRegno);
                while (dReader.Read())
                {
                    lngRegNos = Convert.ToInt64(dReader["Regno"]);
                }
                return lngRegNos;
            }

        }
        /// <summary>
        /// This method is used to get registration number.
        /// Pass loginid or Login Id and Branch Code as argument.
        /// This return regno if member exist in that branch
        /// Creation Date : 23/10/2007.
        /// Updation Date : 10/11/2010.
        /// </summary>
        /// <param name="strValue">string</param>
        /// <param name="branch">string</param>
        /// <returns>long, Registration Number</returns>
        public long GetRegNoBranch(string strValue, string branch)
        {
            long lngRegNo = 0;
            long lngRegNos = 0;
            bool Flag = IsNumeric(strValue);
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();
            if (Flag)
            {

                //long lngmemcode= Convert.ToInt64(strValue.Trim());
                string strSql = " select regno from Member_master where Active=1 and customerid = @lngmemcode and BranchCode=@branchCode";
                SqlParameter[] arrParam = new SqlParameter[2];
                arrParam[0] = new SqlParameter("@lngmemcode", SqlDbType.VarChar, 50); // change to bigint
                arrParam[0].Value = strValue;
                arrParam[1] = new SqlParameter("@branchCode", SqlDbType.VarChar, 50); // change to bigint
                arrParam[1].Value = branch;
                SqlDataReader dReader = objDUT.GetDataReader(arrParam, strSql);
                while (dReader.Read())
                {
                    lngRegNo = Convert.ToInt64(dReader["Regno"]);
                }
                return lngRegNo;
            }
            else
            {
                string strLoginId = strValue.Trim();
                string strSqlRegno = " select regno from Member_master where Active=1 and customerid = @strLoginId and BranchCode=@branchCode";
                SqlParameter[] arrParams = new SqlParameter[2];
                arrParams[0] = new SqlParameter("@strLoginId", SqlDbType.VarChar, 50);
                arrParams[0].Value = strLoginId;
                arrParams[1] = new SqlParameter("@branchCode", SqlDbType.VarChar, 50); // change to bigint
                arrParams[1].Value = branch;
                SqlDataReader dReader = objDUT.GetDataReader(arrParams, strSqlRegno);
                while (dReader.Read())
                {
                    lngRegNos = Convert.ToInt64(dReader["Regno"]);
                }
                return lngRegNos;
            }

        }
        //This method is used to get registration number.
        //  Pass Only Login Id as argument.
        public long GetRegNo(string strValue)
        {

            long lngRegNos = 0;
            bool Flag = IsNumeric(strValue);
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();

            string strLoginId = strValue.Trim();
            string strSqlRegno = " select regno from Member_master where Active=1 and loginid = @strLoginId";
            SqlParameter[] arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@strLoginId", SqlDbType.VarChar, 50);
            arrParams[0].Value = strLoginId;
            SqlDataReader dReader = objDUT.GetDataReader(arrParams, strSqlRegno);
            while (dReader.Read())
            {
                lngRegNos = Convert.ToInt64(dReader["Regno"]);
            }
            return lngRegNos;


        }


        /// <summary>
        /// This method is used to get Login Id of that registration number or Member Code.
        /// Creation Date : 23/10/2007.
        /// </summary>
        /// <param name="lngRegNo">long, Registration Number or Member Code</param>
        /// <returns>string, Login Id</returns>
        public string GetLoginId(long lngRegNo)
        {
            string strLoginId = String.Empty;
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();
            string strSql = " select loginid from Member_master where regno =@lngRegNo";
            SqlParameter[] arrParam = new SqlParameter[1];
            arrParam[0] = new SqlParameter("@lngRegNo", SqlDbType.Int, 8); // replace with bigint
            arrParam[0].Value = lngRegNo;
            SqlDataReader dReader = objDUT.GetDataReader(arrParam, strSql);
            while (dReader.Read())
            {
                strLoginId = dReader["loginid"].ToString();
            }
            dReader.Close();
            return strLoginId;
        }
        /// <summary>
        /// This method is used to get Multiple Login Id According to multiple Registration Number.
        /// Creation Date : 23/10/2007.
        /// </summary>
        /// <param name="strRegNo">string, Multiple Registration Number </param>
        /// <returns>string, Multiple Login Id</returns>
        public string GetLoginId(string strRegNo)
        {
            string strLoginId = String.Empty;
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();
            string strSql = " select loginid from Member_master where regno in (" + strRegNo + ")"; // =@lngRegNo or loginid=@lngRegNo";
                                                                                                    // SqlParameter[] arrParam = new SqlParameter[1];
                                                                                                    //arrParam[0] = new SqlParameter("@strRegNo", SqlDbType.VarChar);
                                                                                                    //arrParam[0].Value = strRegNo;
            SqlDataReader dReader = objDUT.GetDataReader(strSql);
            while (dReader.Read())
            {
                strLoginId = strLoginId + dReader["loginid"].ToString();
                strLoginId = strLoginId + ",";
            }
            dReader.Close();
            return strLoginId;
        }

        /// <summary>
        /// This method is used to get Member Code of that registration number or loginid.
        /// Creation Date : 23/10/2007.
        /// </summary>
        /// <param name="strValue">string, Registraion Number or LoginId</param>
        /// <returns>long, MemberCode</returns>
        public long GetMemberCode(string strValue)
        {
            long lngMemCode = 0;
            long lngMemCodes = 0;
            bool Flag = IsNumeric(strValue);
            // Create Datautility object.
            DataUtility objDUT = new DataUtility();
            if (Flag)
            {

                long lngRegNo = Convert.ToInt64(strValue.Trim());
                string strSql = " select Memcode from Member_master where regno = @lngRegNo";
                SqlParameter[] arrParam = new SqlParameter[1];
                arrParam[0] = new SqlParameter("@lngRegNo", SqlDbType.Int, 4); // replace with bigint
                arrParam[0].Value = lngRegNo;
                SqlDataReader dReader = objDUT.GetDataReader(arrParam, strSql);
                while (dReader.Read())
                {
                    lngMemCode = Convert.ToInt64(dReader["Memcode"]);
                }
                dReader.Close();
                return lngMemCode;
            }
            else
            {
                string strLoginId = strValue.Trim();
                string strSqlloginid = " select Memcode from Member_master where loginid = @strLoginId";
                SqlParameter[] arrParams = new SqlParameter[1];
                arrParams[0] = new SqlParameter("@strLoginId", SqlDbType.VarChar, 50);
                arrParams[0].Value = strLoginId;
                SqlDataReader dReader = objDUT.GetDataReader(arrParams, strSqlloginid);
                while (dReader.Read())
                {
                    lngMemCodes = Convert.ToInt64(dReader["memCode"]);
                }
                dReader.Close();
                return lngMemCodes;
            }

        }
        public ArrayList getAllMobile(string regno)
        {
            ArrayList a = new ArrayList();
            a.Add(objDUT.GetScalar("Select mobile from member_master where regno=" + regno + ""));
            String[] ob = objDUT.GetScalar("Select More_MobileNo from member_master where regno=" + regno + "").ToString().Split(',');
            if (ob.Length > 0)
            {
                foreach (string s in ob)
                {
                    if (s != "")
                    {
                        a.Add(s);
                    }
                }
            }
            return a;
        }
        public string getAllMobileInComma(string regno)
        {
            string allmobile = string.Empty;
            string number = string.Empty, number1 = string.Empty;
            allmobile = objDUT.GetScalar("Select mobile from member_master where regno=" + regno + "").ToString();
            String moreMob = objDUT.GetScalar("Select More_MobileNo from member_master where regno=" + regno + "").ToString();
            if (moreMob != "")
            {
                string[] arrNum = moreMob.Split(',');
                if (arrNum.Length > 0)
                {
                    for (int i = 0; i < arrNum.Length; i++)
                    {
                        if (arrNum[i] != "")
                        {
                            number = "91" + arrNum[i];
                        }
                        if (i < Convert.ToInt32(arrNum.Length - 1))
                        {
                            number1 += number + ",";
                        }
                        else
                        {
                            number1 += number;
                        }
                    }
                }
                allmobile = allmobile + "," + number1;
            }
            return allmobile;
        }

        public int InsertSmsDetail(string regno, string Msg, string MsgType)
        {
            int intResult = 0;
            string number;
            number = getAllMobileInComma(regno);
            if (number != "")
            {
                string strSPInsert = "Usp_SmsDetail";

                SqlParameter[] SmsDetail = new SqlParameter[5];
                SmsDetail[0] = new SqlParameter("@Regno", SqlDbType.Int, 8);
                SmsDetail[0].Value = Convert.ToInt64(regno);

                SmsDetail[1] = new SqlParameter("@Mobile", SqlDbType.VarChar, 200);
                SmsDetail[1].Value = number;

                SmsDetail[2] = new SqlParameter("@SendMsg", SqlDbType.VarChar);
                SmsDetail[2].Value = Msg;

                SmsDetail[3] = new SqlParameter("@QueryType", SqlDbType.VarChar, 50);
                SmsDetail[3].Value = MsgType;

                SmsDetail[4] = new SqlParameter("@intResult", SqlDbType.Int, 4);
                SmsDetail[4].Direction = ParameterDirection.Output;

                intResult = objDUT.ExecuteSqlSP(SmsDetail, strSPInsert);
                if (intResult > 0)
                {
                    long Smsid = Convert.ToInt64(objDUT.GetScalar("select max(smsID) from sms_details").ToString());
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.txtguru.in/imobile/api.php?username=bpcl&password=91751260&source=BPCLLP&dmobile=91" + number + "&message=" + Msg + "");
                    HttpWebResponse MyResponse = (HttpWebResponse)myReq.GetResponse();
                    Stream dataStream2 = MyResponse.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader2 = new StreamReader(dataStream2);
                    string returnvalue = reader2.ReadToEnd();
                    //string returnvalue =  displayMsg(reader2.ReadToEnd());
                    reader2.Close();
                    string updateMsg = "update sms_details set ReceiveMsg='" + returnvalue + "' where smsID=" + Smsid + "";
                    int ExeRes = objDUT.ExecuteSql(updateMsg);
                }
            }
            else
            {
                intResult = 0;
            }
            return intResult;


        }
        public string displayMsg(string str)
        {
            string[] arr;
            arr = str.Split('|');
            return arr[0];
        }

        ////#region FillLookupDropDownList
        //public static void FillLookupDropDownList(LookupTypes type, DropDownList ddl)
        //{
        //    DataSet dsLookup = new DataSet();
        //    SqlHelper sqlHelper = new SqlHelper();
        //    SqlParameter[] param = new SqlParameter[1];
        //    SqlParameter[] LookupParam = new SqlParameter[2];
        //    string spName = string.Empty;
        //    switch (type)
        //    {
        //        case LookupTypes.Institute:
        //            spName = "[uspInstitute]";
        //            param[0] = new SqlParameter("@QueryType", "BindInstituteList");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, param);
        //            break;
        //        case LookupTypes.State:
        //            spName = "[uspState]";
        //            param[0] = new SqlParameter("@QueryType", "BindStateList");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, param);
        //            break;
        //        case LookupTypes.Course:
        //            spName = "[uSPCourse]";
        //            param[0] = new SqlParameter("@QueryType", "GetCourseList");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, param);
        //            break;
        //        case LookupTypes.Bank:
        //            spName = "[uspBank]";
        //            param[0] = new SqlParameter("@SpQueryType", "GetActiveBank");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, param);
        //            break;
        //        case LookupTypes.User:
        //            spName = "[uspUser]";
        //            param[0] = new SqlParameter("@QueryType", "BindUserList");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, param);
        //            break;
        //        default:
        //            spName = "[uspLookup]";
        //            LookupParam[0] = new SqlParameter("@LookupTypeCode", (int)type);
        //            LookupParam[1] = new SqlParameter("@QueryType", "GetLookupList");
        //            dsLookup = SqlHelper.ExecuteDataset(sqlHelper.conn, CommandType.StoredProcedure, spName, LookupParam);
        //            break;
        //    }
        //    ddl.DataSource = dsLookup.Tables[0];
        //    ddl.DataTextField = "LookupName";
        //    ddl.DataValueField = "LookupCode";
        //    ddl.DataBind();
        //    ddl.Items.Insert(0, new ListItem("Select", "0"));
        //}
        //#endregion

        #region Base64Encode
        public static string Base64Encode(string data)
        {
            byte[] encData_byte = new byte[data.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
            string encodedData = Convert.ToBase64String(encData_byte);
            return encodedData;
        }
        #endregion

        #region Base64Decode
        public static string Base64Decode(string data)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();

            byte[] todecode_byte = Convert.FromBase64String(data);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }
        #endregion

        #region SerializeToXML
        public static string SerializeToXML(object toSerialize)
        {
            string toReturn = String.Empty;
            if (toSerialize != null)
            {
                StringWriter stringWriter = new StringWriter();
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                xmlSerializer.Serialize(stringWriter, toSerialize);
                toReturn = stringWriter.ToString();
                stringWriter.Close();
            }
            return toReturn;
        }
        #endregion

        #region DeSerializeFromXML
        public static object DeSerializeFromXML(string toDeSerialize, Type objectType)
        {
            object toReturn = null;
            if (toDeSerialize != null && toDeSerialize != String.Empty)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(objectType);
                toReturn = xmlSerializer.Deserialize(new StringReader(toDeSerialize));
            }
            return toReturn;
        }
        #endregion

        #region ToSafeString
        public static string ToSafeString(string value, string defaultValue)
        {
            string returnValue;
            if (string.IsNullOrEmpty(value))
            {
                returnValue = defaultValue;
            }
            else
            {
                returnValue = value;

            }
            return returnValue;
        }
        #endregion

        #region ToInt32
        public static int ToInt32(string value, int defaultValue)
        {
            int returnValue;
            if (string.IsNullOrEmpty(value))
            {
                returnValue = defaultValue;
            }
            else
            {
                if (!int.TryParse(value, out returnValue))
                {
                    returnValue = defaultValue;
                }
            }
            return returnValue;
        }
        #endregion

        #region ToBool
        public static bool ToBool(string value, bool defaultValue)
        {
            bool returnValue;
            if (string.IsNullOrEmpty(value))
            {
                returnValue = defaultValue;
            }
            else
            {
                if (!bool.TryParse(value, out returnValue))
                {
                    returnValue = defaultValue;
                }
            }
            return returnValue;
        }
        #endregion

        #region SaveToSession
       
        #endregion

        #region RepeaterCurrentRowCssClass
        private static int rowCounter = 0;
        public static string RepeaterCurrentRowCssClass
        {
            get
            {
                string toReturn = "";
                if (rowCounter % 2 == 0)
                {
                    toReturn = "bg_grey";
                }
                if (rowCounter == Int32.MaxValue)
                {
                    rowCounter = 0;
                }

                else
                {
                    rowCounter++;
                }
                return toReturn;
            }
        }
        #endregion

   

        
        // #region RepeaterCurrentRowCssClass
        //// private static int rowCounter = 0;
        // public static string RepeaterCurrentRowCssClass
        // {
        //     get
        //     {
        //         string toReturn = "";
        //         if (rowCounter % 2 == 0)
        //         {
        //             toReturn = "bg_grey";
        //         }
        //         if (rowCounter == Int32.MaxValue)
        //         {
        //             rowCounter = 0;
        //         }

        //         else
        //         {
        //             rowCounter++;
        //         }
        //         return toReturn;
        //     }
        // }
        // #endregion

        //#region showAlertMessage
        //public static void showAlertMessage(Page page, string msg)
        //{
        //    ScriptManager.RegisterClientScriptBlock(page, page.GetType(), "Key", "<script language='javascript'>alert('" + msg + "');</script>", false);
        //}
        //#endregion

        // Added Reset Function for Reseting controls of Page

        //private void Reset()
        //{
        //    Control crtl;
        //    ClearControl(crtl);
        //}

        ///// <summary>
        ///// used to clear all controls
        ///// </summary>
        ///// <param name="root"></param>
        //public void ClearControl(Control root)
        //{
        //    foreach (Control ctrl in root.Controls)
        //    {
        //        ClearControl(ctrl);
        //        if (ctrl is TextBox)
        //        {
        //            ((TextBox)ctrl).Text = string.Empty;
        //        }
        //        if (ctrl is DropDownList)
        //        {
        //            ((DropDownList)ctrl).ClearSelection();
        //        }
        //        if (ctrl is RadioButtonList)
        //        {
        //            ((RadioButtonList)ctrl).ClearSelection();
        //        }
        //        if (ctrl is CheckBox)
        //        {
        //            ((CheckBox)ctrl).Checked = false;
        //        }


        //    }
        //}

        public static long GetTeamSize(long regno)
        {
            DataUtility objDUT = new DataUtility();
            SqlParameter[] objMemInsert = new SqlParameter[6];
            objMemInsert[0] = new SqlParameter("@mregno", SqlDbType.BigInt);
            objMemInsert[0].Value = Convert.ToInt64(regno);
            objMemInsert[1] = new SqlParameter("@side", SqlDbType.Char);
            objMemInsert[1].Value = "";
            objMemInsert[2] = new SqlParameter("@count", SqlDbType.Int);
            objMemInsert[2].Value = 1;
            objMemInsert[3] = new SqlParameter("@kid", SqlDbType.Int);
            objMemInsert[3].Value = 0;
            objMemInsert[4] = new SqlParameter("@payid", SqlDbType.Int);
            objMemInsert[4].Value = 0;
            objMemInsert[5] = new SqlParameter("@strResult", SqlDbType.VarChar, 1000);
            objMemInsert[5].Direction = ParameterDirection.Output;
            string intResult = objDUT.ExecuteSqlSPS(objMemInsert, "usp_GetdownlineMemberP").ToString();
            long team_size = Convert.ToInt64(intResult);
            return team_size;
        }

        public void WriteToFile(string strPath, ref byte[] Buffer)
        {
            // Create a file
            FileStream newFile = new FileStream(strPath, FileMode.Create);

            // Write data to the file
            newFile.Write(Buffer, 0, Buffer.Length);

            // Close file
            newFile.Close();

        }

        public void WriteLog(string text, int TimeWait = 0)
        {
            try
            {
                int i = objDUT.ExecuteSql("Insert into AutoRenewalLog (Message, Date) values('" + text + "' ,'" + GetTimeLP() + "')");
                if (TimeWait > 0)
                {
                    Esperar(TimeWait);
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
            }
        }

        public string GetTimeLP()
        {
            string SystemTime = "";
            try
            {
                string query = "Select Caption,Default_value from company_settings where  caption ='Time Zone'";
                DataTable dt1 = objDUT.GetDataTable(query);
                if (dt1.Rows.Count > 0)
                {
                    //msglist.Add(sdr["Default_value"].ToString());
                    //msglist.Add(sdr["Caption"].ToString());

                    query = "select DATEADD(hh," + dt1.Rows[0]["Default_value"].ToString() + ", GETUTCDATE())";
                    DataTable hora = objDUT.GetDataTable(query);
                    SystemTime = hora.Rows[0][0].ToString();
                }
            }
            catch (Exception excep)
            {
            }
            return SystemTime;
        }

        public void Esperar(int MiliSeconds)
        {
            System.Threading.Thread.Sleep(MiliSeconds);
        }


        public string GetSpokenLanguages(string MainLanguage, string SpokenLanguages)
        {
            string strLanguages = "";
            string query1 = "select Language + ' (' + NativeName + ') ' as LangComplete from Languages where Language='" + MainLanguage + "'";
            DataTable dt2 = objDUT.GetDataTable(query1);
            strLanguages = dt2.Rows[0][0].ToString();


            string[] ListSpokenLanguages = SpokenLanguages.Split(',');
            foreach (string _language in ListSpokenLanguages)
            {
                query1 = "select Language + ' (' + NativeName + ') ' as LangComplete from Languages where id=" + _language;
                dt2 = objDUT.GetDataTable(query1);
                if (!strLanguages.Contains(dt2.Rows[0][0].ToString().Trim()))
                {
                    strLanguages = strLanguages.Trim() + ", " + dt2.Rows[0][0].ToString();
                }
            }
            strLanguages = strLanguages.Substring(0, strLanguages.Length - 1);
            return strLanguages;
        }


        public double GetBalance(long regno)
        {
            double Balance = 0;
            DataTable dt = objDUT.GetDataTable("select sum(isnull(credit,0)) from member_account where transtype in('LevelIncome', 'ReNewLevelIncome') and trStatus = 1 and Renewalstatus = 0 and regno = " + regno + " and convert(date, transdate,103)<= '" + System.DateTime.UtcNow.AddMinutes(-300).ToShortDateString() + "'");
            if (dt.Rows.Count > 0)
            {
                Balance = Convert.ToDouble(string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? "0" : dt.Rows[0][0].ToString());
            }
            return Balance;
        }

        public List<string> GetJsonListFromApi(string apiUrl)
        {
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(apiUrl);
            WebReq.Method = "GET";
            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            string jsonString;
            using (Stream stream = WebResp.GetResponseStream())   //modified from your code since the using statement disposes the stream automatically when done
            {
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                jsonString = reader.ReadToEnd();
            }

            jsonString = jsonString.Replace("\"", "").Replace("{", "").Replace("}", "");
            string[] valores = jsonString.Split(',');
            List<string> jsonValues = new List<string>(valores);
            return jsonValues;
        }

        public string GetJsonValues(List<string> jsonValues, string key)
        {
            var value = jsonValues.Where(x => x.Contains(key)).FirstOrDefault();
            var values = value.Split(':');
            if (values.Length > 1)
            {
                return values[1].Trim();
            }
            else
            {
                return "";
            }
        }


        public Decimal getBalance(long regno)
        {
            SqlCommand cmd;
            SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
            DataUtility objDUT = new DataUtility();
            //------------- Get account balance
            cmd = new SqlCommand();
            cmd.CommandText = "SP_GetCurrentBalance";
            cmd.Connection = cnn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RegNo", regno);
            SqlParameter strP = new SqlParameter("@Balance", SqlDbType.Money);
            strP.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(strP);
            cnn.Open();
            SqlDataReader sdr = cmd.ExecuteReader();
            Decimal balance = Convert.ToDecimal(strP.Value.ToString());
            cnn.Close();
            return balance;
        }
        
    }

}
