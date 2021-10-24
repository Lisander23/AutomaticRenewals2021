using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace COTDP.Services
{
    public class DataUtility
    {
        #region All Constructors
        /// <summary>
        /// This default or no argument constructor.
        /// </summary>
        public DataUtility()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// This parametrized constructor
        /// </summary>
        /// <param name="Connection">string</param>
        public DataUtility(string Connection)
        {
            this._mCon = new SqlConnection(Connection);
        }
        #endregion

        #region All Propreties
        private SqlConnection _mCon;

        public SqlConnection mCon
        {
            get { return _mCon; }
            set { _mCon = value; }
        }


        private SqlCommand _mDataCom;

        public SqlCommand mDataCom
        {
            get { return _mDataCom; }
            set { _mDataCom = value; }
        }

        private SqlDataAdapter _mDa;

        public SqlDataAdapter mDa
        {
            get
            {
                if (_mDa == null)
                {
                    _mDa = new SqlDataAdapter();
                }
                return _mDa;
            }
            set { _mDa = value; }
        }

        private DataTable _DataTable;

        public DataTable DataTable
        {
            get
            {
                if (_DataTable == null)
                {
                    _DataTable = new DataTable();
                }
                return _DataTable;
            }
            set { _DataTable = value; }
        }

        private DataSet _DataSet;

        public DataSet DataSet
        {
            get
            {
                if (_DataSet == null)
                {
                    _DataSet = new DataSet();
                }
                return _DataSet;
            }
            set { _DataSet = value; }
        }


        #endregion

        #region All private methods
        /// <summary>
        ///  1. Initialize Connection object with parameterize constructor.
        ///  2. Initialize Command object wiht default or no argument constructor.
        ///  3. Set active connectin with Command object.
        /// </summary>
        private void OpenConnection()
        {
            // Check Connection object for null.
            if (_mCon == null)
            {
                _mCon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
            }
            // Check Connection State.
            if (_mCon.State == ConnectionState.Closed)
            {
                _mCon.Close();
                _mCon.Open();

                // Initialize Command object.
                _mDataCom = new SqlCommand();

                // Set active connection with Command object.
                _mDataCom.Connection = _mCon;
            }

        }
        /// <summary>
        /// This method  is used for close the connection.
        /// </summary>
        private void CloseConnection()
        {
            // Check Connection is open.
            if (_mCon.State == ConnectionState.Open)
            {
                _mCon.Close();
            }
        }
        /// <summary>
        /// This method is used for Dispose the connection object.
        /// </summary>
        private void DisposeConnection()
        {
            if (_mCon != null)
            {
                _mCon.Dispose();
                // Initialize Connection object with null.
                _mCon = null;
            }
        }
        #endregion

        #region All public methods
        /// <summary>
        /// This method is used to execute DML  using SQL as text.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>int, no of rows affected</returns>
        public int ExecuteSql(string strSql)
        {
            // Open the connection.
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            // Execute the method.
            int intResult = _mDataCom.ExecuteNonQuery();
            // Close the connection.
            CloseConnection();
            // Release the resources.
            DisposeConnection();
            return intResult;
        }
        /// <summary>
        /// This method is used to execute DML using parameterized SQL query.
        /// Passing SqlParameter array and SQL query.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSql">string</param>
        /// <returns>int, no of rows affected</returns>
        public int ExecuteSql(SqlParameter[] arrParam, string strSql)
        {

            OpenConnection();
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            _mDataCom.Parameters.Clear();
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            int intResult = _mDataCom.ExecuteNonQuery();
            CloseConnection();
            DisposeConnection();
            return intResult;
        }

        /// <summary>
        /// This method is used to execute DML using stored procedure.
        /// Passing SqlParameter array and procedure name.  
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSPName">string</param>
        /// <returns>string, no of records affected</returns>
        public string ExecuteSqlSPS(SqlParameter[] arrParam, string strSPName)
        {
            OpenConnection();
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 600;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }

            _mDataCom.ExecuteNonQuery();
            string strResult = (_mDataCom.Parameters["@strResult"].Value.ToString());
            CloseConnection();
            DisposeConnection();
            return strResult;
        }

        /// <summary>
        /// This method is used to execute DML using stored procedure.
        /// Passing SqlParameter array and procedure name.  
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSPName">string</param>
        /// <returns>int, no of records affected</returns>
        public int ExecuteSqlSP(SqlParameter[] arrParam, string strSPName)
        {
            try
            {
                OpenConnection();
                _mDataCom.CommandType = CommandType.StoredProcedure;
                _mDataCom.CommandText = strSPName;
                _mDataCom.CommandTimeout = 18000;
                for (int i = 0; i < arrParam.Length; i++)
                {
                    _mDataCom.Parameters.Add(arrParam[i]);
                }

                _mDataCom.ExecuteNonQuery();
                int intResult = Int32.Parse(_mDataCom.Parameters["@intResult"].Value.ToString());
                CloseConnection();
                DisposeConnection();
                return intResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int ExecuteSql(SqlParameter parametros, string str)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is to validate a record if its already there in the table.
        /// If record exist return true else return false.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>bool</returns>
        public bool IsExist(string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            // Execute the method
            int intResult = (int)_mDataCom.ExecuteScalar(); // typecasting because ExecuteScalar return object.
            CloseConnection();
            DisposeConnection();

            // Check the result.
            if (intResult > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This method is used to return frist column of the selected record.
        /// Pass SQL query as text.
        /// Return object so type cast it.
        /// Created Date : 31/10/2005
        /// Created By   : Dipak Sinha.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>object</returns>
        public object GetScalar(string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            // Execute the method
            object intResult = _mDataCom.ExecuteScalar();
            CloseConnection();
            DisposeConnection();
            return intResult;
        }

        /// <summary>
        /// This method check whether record already exist in the table.
        /// Passing SqlParameter array and SQL query as text.
        /// If exist return true else return false.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSql">string</param>
        /// <returns>bool</returns>
        public bool IsExist(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;

            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Execute the method
            int intResult = (int)_mDataCom.ExecuteScalar(); // typecasting because ExecuteScalar return object.
            CloseConnection();
            DisposeConnection();

            // Check the result.
            if (intResult > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This method is used to execute DML using stored procedure.
        /// Passing SqlParameter array and procedure name.  
        /// If exist return true else return false.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSPName">string</param>
        /// <returns>bool</returns>
        public bool IsExistSP(SqlParameter[] arrParam, string strSPName)
        {
            OpenConnection();
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }

            _mDataCom.ExecuteScalar();
            int intResult = Int32.Parse(_mDataCom.Parameters["@intResult"].Value.ToString());
            CloseConnection();
            DisposeConnection();
            if (intResult > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void WriteLog(string text, int TimeWait = 0)
        {
            try
            {
                int i = ExecuteSql("Insert into AutoRenewalLog (Message, Date) values('" + text + "' ,'" + GetTimeLP() + "')");
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
                DataTable dt1 = GetDataTable(query);
                if (dt1.Rows.Count > 0)
                {
                    //msglist.Add(sdr["Default_value"].ToString());
                    //msglist.Add(sdr["Caption"].ToString());

                    query = "select DATEADD(hh," + dt1.Rows[0]["Default_value"].ToString() + ", GETUTCDATE())";
                    DataTable hora = GetDataTable(query);
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
        /// <summary>
        /// This method is used to execute DML using stored procedure.
        /// Passing procedure name.  
        /// If exist return true else return false.
        /// </summary>
        /// <param name="strSPName">string stored procedure name</param>
        /// <returns>bool</returns>
        public bool IsExistSP(string strSPName)
        {
            OpenConnection();
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            _mDataCom.ExecuteScalar();
            int intResult = Int32.Parse(_mDataCom.Parameters["@intResult"].Value.ToString());
            CloseConnection();
            DisposeConnection();
            if (intResult > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This is to read data in Disconnected mode using SQL as text.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;

            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;

            // Initialize DataTable object.
            _DataTable = new DataTable();
            _mDa.Fill(_DataTable);
            CloseConnection();
            DisposeConnection();
            return _DataTable;
        }

        /// <summary>
        /// This is to read data in Disconnected mode using parameterized SQL.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSql">string</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 0;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataTable object.
            _DataTable = new DataTable();
            _mDa.Fill(_DataTable);
            CloseConnection();
            DisposeConnection();
            return _DataTable;

        }
        /// <summary>
        /// This is to read data using Disconnected mode using stored procedure.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSPName">string</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTableSP(SqlParameter[] arrParam, string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataTable object.
            _DataTable = new DataTable();
            _mDa.Fill(_DataTable);
            CloseConnection();
            DisposeConnection();
            return _DataTable;

        }
        /// <summary>
        /// This is to read data using Disconnected mode using stored procedure.
        /// Passing procedure name as parameter.
        /// </summary>
        /// <param name="strSPName">string stored procedure name</param>
        /// <returns>DataTable object</returns>
        public DataTable GetDataTableSP(string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataTable object.
            _DataTable = new DataTable();
            _mDa.Fill(_DataTable);
            CloseConnection();
            DisposeConnection();
            return _DataTable;

        }
        /// <summary>
        /// This is to read data in Disconnected mode using SQL as text.
        /// </summary>
        /// <param name="strSql">string </param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            // Initailize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataSet object.
            _DataSet = new DataSet();
            _mDa.Fill(_DataSet);
            CloseConnection();
            DisposeConnection();
            return _DataSet;

        }
        /// <summary>
        /// This is to read data in Disconnected mode using parameterized SQL as text.
        /// </summary>
        /// <param name="arrParam">SqlParameter[]</param>
        /// <param name="strSql">string</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataSet object.
            _DataSet = new DataSet();
            _mDa.Fill(_DataSet);
            CloseConnection();
            DisposeConnection();
            return _DataSet;
        }

        /// <summary>
        /// This is to read data in Disconnected mode using stored procedure.
        /// Passing stored procedure name.
        /// </summary>
        /// <param name="strSPName">string stored procedure name</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSetSP(string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataSet object.
            _DataSet = new DataSet();
            _mDa.Fill(_DataSet);
            CloseConnection();
            DisposeConnection();
            return _DataSet;
        }

        /// <summary>
        /// This is to read data in Disconnected mode using parameterized stored procedure.
        /// Passing SqlParameter collection stored procedure name.
        /// </summary>
        /// <param name="arrParam">SqlParameter[]</param>
        /// <param name="strSPName">string</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSetSP(SqlParameter[] arrParam, string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Initialize SqlDataAdapter object.
            _mDa = new SqlDataAdapter();
            // Set the Command object in DataAdapter.
            _mDa.SelectCommand = _mDataCom;
            // Initialize DataSet object.
            _DataSet = new DataSet();
            _mDa.Fill(_DataSet);
            CloseConnection();
            DisposeConnection();
            return _DataSet;

        }

        /// <summary>
        /// This is to read data using Connected mode using SQL as text.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader GetDataReader(string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;

            // Create SqlDataReader object.
            SqlDataReader dReader;
            dReader = _mDataCom.ExecuteReader(CommandBehavior.CloseConnection);
            return dReader;
        }
        /// <summary>
        /// This is to read data using Connected mode using parameterized SQL.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSql">string</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader GetDataReader(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;

            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Create SqlDataReader object.
            SqlDataReader dReader;
            dReader = _mDataCom.ExecuteReader(CommandBehavior.CloseConnection);
            return dReader;

        }

        /// <summary>
        /// This is to read data in Connected mode using stored procedure.
        /// Passing SqlParameter and procedure name as parameter.
        /// </summary>
        /// <param name="arrParam">SqlParameter</param>
        /// <param name="strSql">string</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader GetDataReaderSP(SqlParameter[] arrParam, string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;

            for (int i = 0; i < arrParam.Length; i++)
            {
                _mDataCom.Parameters.Add(arrParam[i]);
            }
            // Create SqlDataReader object.
            SqlDataReader dReader;
            dReader = _mDataCom.ExecuteReader(CommandBehavior.CloseConnection);
            return dReader;

        }
        /// <summary>
        /// This is to read data in Connected mode using stored procedure.
        /// Passing procedure name as parameter.
        /// </summary>
        /// <param name="strSPName">string stored procedure name</param>
        /// <returns>SqlDataReader object</returns>
        public SqlDataReader GetDataReaderSP(string strSPName)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSPName;
            _mDataCom.CommandTimeout = 18000;
            // Create SqlDataReader object.
            SqlDataReader dReader;
            dReader = _mDataCom.ExecuteReader(CommandBehavior.CloseConnection);
            return dReader;
        }

        /// <summary>
        /// This is to read Single data using Connected mode using SQL as text.
        /// </summary>
        /// <param name="strSql">string</param>
        /// <returns>Object</returns>
        public object GetScaler(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.Parameters.Clear();
            _mDataCom.CommandType = CommandType.Text;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            _mDataCom.Parameters.Add(arrParam[0]);

            // Create SqlDataReader object.
            //SqlDataReader dReader;
            Object oObject;
            oObject = _mDataCom.ExecuteScalar();
            return oObject;
        }

        public int LineNumber(Exception e)
        {
            int linenum = 0;
            try
            {
                linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(' ')));
            }
            catch
            {
                //Stack trace is not available!
            }
            return linenum;
        }


        public string GetTimeLP2()
        {
            string SystemTime = "";
            try
            {
                string query = "Select Caption,Default_value from company_settings where  caption ='Time Zone'";
                DataTable dt1 = GetDataTable(query);

                query = "select DATEADD(hh," + dt1.Rows[0]["Default_value"].ToString() + ", GETUTCDATE())";
                DataTable hora = GetDataTable(query);
                SystemTime = hora.Rows[0][0].ToString();
            }
            catch (Exception excep)
            {
            }
            return SystemTime;
        }

        public int EnviarCorreoLP(long mxregno = 0, string LoginID = "", string Message = "", string subject = "", string ToEmail = "", Attachment Adjunto = null, string AdjuntoName = "", string Origen = "")
        {
            try
            {
                //string MailServer = "";
                //Origen = "noreply@cotdp.org";
                //MailServer = "webmail.cotdp.org";

                //System.Net.Mail.MailMessage Email;
                //Email = new System.Net.Mail.MailMessage(Origen, ToEmail);
                //Email.IsBodyHtml = true;
                //Email.Subject = subject;
                //Email.Priority = System.Net.Mail.MailPriority.High;
                //Email.Body = Message;
                //   if (Adjunto != null)
                //{
                //    Adjunto.Name = AdjuntoName;
                //    Email.Attachments.Add(Adjunto);
                //}

                //System.Net.NetworkCredential webmail = new System.Net.NetworkCredential(Origen, "Christian23$1");//("noreply@cotdp.org", "Christian23$1");
                //System.Net.Mail.SmtpClient ms = new SmtpClient(MailServer);
                //ms.Credentials = webmail;
                //ms.Send(Email);
                //return 1;


                SmtpClient smtpClient = new SmtpClient();
                NetworkCredential smtpCredentials = new NetworkCredential("noreply@cotdp.org", "Christian23$1");

                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("noreply@cotdp.org");
                MailAddress toAddress = new MailAddress(string.IsNullOrEmpty(ToEmail) ? "lisander23@gmail.com" : ToEmail);

                smtpClient.Host = "hgws23.win.hostgator.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = smtpCredentials;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 20000;

                message.From = fromAddress;
                message.To.Add(toAddress);
                message.IsBodyHtml = true;
                message.Subject = subject;
                message.Priority = MailPriority.High;
                message.Body = Message;
                if (Adjunto != null)
                {
                    Adjunto.Name = AdjuntoName;
                    message.Attachments.Add(Adjunto);
                }

                smtpClient.Send(message);
                return 1;
            }
            catch (Exception ex)
            {
                throw ex;
                //return 0;
                // Response.Write(ex.Message);
            }
            return 0;
        }

        public DataTable CalculateBalanceDonorLP(long regno)
        {
            try
            {
                SqlParameter[] sqlp = new SqlParameter[1];
                sqlp[0] = new SqlParameter("@mregno", SqlDbType.BigInt, 8);
                sqlp[0].Value = regno;

                string query = "select * from member_master where regno=" + regno;
                DataTable dt0 = GetDataTable(query);
                string MasterAccountName = dt0.Rows[0]["LoginID"].ToString();

                long regnoAux = regno;
                DataTable dtt = GetDataTableSP(sqlp, "[usp_DownlineList]");
                DataTable Tabla;
                Tabla = dtt.Copy();
                Tabla.Columns.Add("Level");
                Tabla.Columns.Add("EmailID");
                Tabla.Columns.Add("Balance", typeof(decimal));
                Tabla.Columns.Add("ViewDetail");
                var col = new DataColumn("regDate", typeof(DateTime));
                Tabla.Columns.Add("MasterSubAccount");
                Tabla.Columns.Add(col);
                if (Tabla.Rows.Count > 0)
                {
                    for (int i = 0; i < Tabla.Rows.Count; i++)
                    {
                        string username = Tabla.Rows[i]["LoginID"].ToString();
                        string consulta = "WITH MyTest as (SELECT P.regno, P.parentRegNoM, p.loginid, CAST(P.regno AS VarChar(Max)) as Level," +
                             "p.FName + ' ' + p.LName Name,CONVERT(VARCHAR, p.regdate, 101) as regdate,CID FROM member_master P WHERE " +
                             "P.parentRegNoM = 0 UNION ALL SELECT P1.regno, P1.parentRegNoM, p1.loginid, CAST(P1.regno AS VarChar(Max)) + '," +
                             "' + M.Level,p1.FName + ' ' + p1.LName Name,CONVERT(VARCHAR, p1.regdate, 101) as regdate, p1.CID FROM" +
                             " MEMBER_Master P1 INNER JOIN MyTest M ON M.regno = P1.parentRegNoM) SELECT DISTINCT A.REGNO,parentRegNoM," +
                             " a.LoginId,a.Level,a.Name,c.countryName,a.REGDATE,D.Balance,'" + MasterAccountName + "' as MasterSubAccount From MyTest A, MEMBER_ACCOUNT B, country c cross " +
                             " apply (select Null as regno, NULL as parentRegno, NULL as loginid, NULL as Level, NULL Name, NULL as " +
                             " countryname, NULL as Regdate,TotalCredit - TotalDebit - LockAmt - UnlockedAmount Balance,'" + MasterAccountName + "' as MasterSubAccount from(select regno, " +
                             "sum(TotalCredit)TotalCredit, sum(TotalDebit) TotalDebit, sum(LockAmt) LockAmt, sum(unLockedAmount) " +
                             " UnlockedAmount from(Select a.regno, Sum(credit) TotalCredit, sum(Debit)TotalDebit, 0 as 'LockAmt', 0 as " +
                             " 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID = '" + username + "' " +
                             " and trStatus = 1 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, sum(Debit) " +
                             " LockAmt, 0 as 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID " +
                             " = '" + username + "' and trStatus = 0 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, " +
                             " 0 as 'LockAmt', sum(Debit) unLockedAmount from member_account a, member_master b where a.regno = b.regno and " +
                             " b.LOGINID = '" + username + "' and trStatus = 2 group by a.regno) as tabla group by regno) as tabla2, member_master m," +
                             " country c where tabla2.regno = m.regno and m.cid = c.CID) as d where A.REGNO = B.REGNO And a.cid = c.cid " +
                             " AND a.loginid = '" + username + "' ";

                        DataTable LevelDonor = GetDataTable(consulta);
                        string[] levels = LevelDonor.Rows[0]["level"].ToString().Split(',');
                        int LevelNumber = 0;
                        if (levels.Length > 9)  // IF LEVEL IS > 6 THEN EXIT, 9 because there are 3 previous levels in database: root, cotdp and chris1
                        {
                            Tabla.Rows[i].Delete();
                        }
                        else
                        {
                            for (int j = 0; j < levels.Length - 1; j++)
                            {
                                if (levels[j].ToString().Trim() == regnoAux.ToString())
                                {
                                    LevelNumber = j;
                                }
                            }
                            Tabla.Rows[i]["level"] = "Level" + LevelNumber;
                            Tabla.Rows[i]["regDate"] = Convert.ToDateTime(LevelDonor.Rows[0]["regDate"].ToString()).ToString("MM/dd/yyyy");
                            Tabla.Rows[i]["Balance"] = Math.Round(Convert.ToDecimal(LevelDonor.Rows[0]["Balance"].ToString()), 2).ToString();
                            Tabla.Rows[i]["MasterSubAccount"] = LevelDonor.Rows[0]["MasterSubAccount"].ToString();
                            Tabla.Rows[i]["ViewDetail"] = "<a href = \"TotalBalanceMasterAccount.aspx?id='" + username + "'\" target=\"_blank\">View</a>";
                        }
                    }
                }
                else
                {
                    query = "SELECT * FROM MEMBER_MASTER A, Country C WHERE A.CID=C.CID AND REGNO=" + regno;
                    DataTable DT1 = GetDataTable(query);
                    string LoginID = DT1.Rows[0]["LoginID"].ToString();

                    DataRow Fila = Tabla.NewRow();
                    Tabla.Columns.Add("Regno");
                    Fila["regno"] = DT1.Rows[0]["regno"].ToString();
                    Tabla.Columns.Add("ParentRegnoM");
                    Fila["ParentRegnoM"] = DT1.Rows[0]["ParentRegnoM"].ToString();
                    Tabla.Columns.Add("LoginID");
                    Fila["LoginID"] = LoginID;
                    Tabla.Columns.Add("Name");
                    Fila["Name"] = DT1.Rows[0]["FName"].ToString() + " " + DT1.Rows[0]["LName"].ToString();
                    Tabla.Columns.Add("Countryname");
                    Fila["Countryname"] = DT1.Rows[0]["Countryname"].ToString();

                    //This line is nedded to include the main donor in datatable because the previouse process only inlude their childs
                    query = "WITH MyTest as (SELECT P.regno, P.parentRegNoM, p.loginid, CAST(P.regno AS VarChar(Max)) as Level," +
                             "p.FName + ' ' + p.LName Name,CONVERT(VARCHAR, p.regdate, 101) as regdate,CID FROM member_master P WHERE " +
                             "P.parentRegNoM = 0 UNION ALL SELECT P1.regno, P1.parentRegNoM, p1.loginid, CAST(P1.regno AS VarChar(Max)) + '," +
                             "' + M.Level,p1.FName + ' ' + p1.LName Name,CONVERT(VARCHAR, p1.regdate, 101) as regdate, p1.CID FROM" +
                             " MEMBER_Master P1 INNER JOIN MyTest M ON M.regno = P1.parentRegNoM) SELECT DISTINCT A.REGNO,parentRegNoM," +
                             " a.LoginId,a.Level,a.Name,c.countryName,a.REGDATE,D.Balance,'" + MasterAccountName + "' as MasterSubAccount  From MyTest A, MEMBER_ACCOUNT B, country c cross " +
                             " apply (select Null as regno, NULL as parentRegno, NULL as loginid, NULL as Level, NULL Name, NULL as " +
                             " countryname, NULL as Regdate,TotalCredit - TotalDebit - LockAmt - UnlockedAmount Balance,'" + MasterAccountName + "' as MasterSubAccount  from(select regno, " +
                             "sum(TotalCredit)TotalCredit, sum(TotalDebit) TotalDebit, sum(LockAmt) LockAmt, sum(unLockedAmount) " +
                             " UnlockedAmount from(Select a.regno, Sum(credit) TotalCredit, sum(Debit)TotalDebit, 0 as 'LockAmt', 0 as " +
                             " 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID = '" + LoginID + "' " +
                             " and trStatus = 1 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, sum(Debit) " +
                             " LockAmt, 0 as 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID " +
                             " = '" + LoginID + "' and trStatus = 0 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, " +
                             " 0 as 'LockAmt', sum(Debit) unLockedAmount from member_account a, member_master b where a.regno = b.regno and " +
                             " b.LOGINID = '" + LoginID + "' and trStatus = 2 group by a.regno) as tabla group by regno) as tabla2, member_master m," +
                             " country c where tabla2.regno = m.regno and m.cid = c.CID) as d where A.REGNO = B.REGNO And a.cid = c.cid " +
                             " AND a.loginid = '" + LoginID + "' ";
                    DT1 = GetDataTable(query);
                    Fila["Level"] = "Level0";
                    Fila["Regdate"] = DT1.Rows[0]["Regdate"].ToString();
                    Fila["Balance"] = Math.Round(Convert.ToDecimal(DT1.Rows[0]["Balance"].ToString()), 2).ToString();
                    Fila["MasterSubAccount"] = DT1.Rows[0]["MasterSubAccount"].ToString();
                    Tabla.Rows.Add(Fila);
                }
                DataView dv = Tabla.DefaultView;
                dv.Sort = "regdate DESC";
                DataTable sortedDT = dv.ToTable();
                sortedDT.Columns.Add("DateStr");
                foreach (DataRow dr in sortedDT.Rows)
                {
                    dr["DateStr"] = string.Format("{0:MM/dd/yyyy}", dr["regdate"]);
                }
                //Add the member master Account to DataTable
                string consulta2 = "SELECT LOGINID FROM MEMBER_MASTER WHERE REGNO=" + regno;
                DataTable DT2 = GetDataTable(consulta2);
                string username2 = DT2.Rows[0][0].ToString();
                consulta2 = "WITH MyTest as (SELECT P.regno, P.parentRegNoM, p.loginid, CAST(P.regno AS VarChar(Max)) as Level," +
                             "p.FName + ' ' + p.LName Name,CONVERT(VARCHAR, p.regdate, 101) as regdate,CID FROM member_master P WHERE " +
                             "P.parentRegNoM = 0 UNION ALL SELECT P1.regno, P1.parentRegNoM, p1.loginid, CAST(P1.regno AS VarChar(Max)) + '," +
                             "' + M.Level,p1.FName + ' ' + p1.LName Name,CONVERT(VARCHAR, p1.regdate, 101) as regdate, p1.CID FROM" +
                             " MEMBER_Master P1 INNER JOIN MyTest M ON M.regno = P1.parentRegNoM) SELECT DISTINCT A.REGNO,parentRegNoM," +
                             " a.LoginId,a.Level,a.Name,c.countryName,a.REGDATE,D.Balance,'" + MasterAccountName + "' as MasterSubAccount From MyTest A, MEMBER_ACCOUNT B, country c cross " +
                             " apply (select Null as regno, NULL as parentRegno, NULL as loginid, NULL as Level, NULL Name, NULL as " +
                             " countryname, NULL as Regdate,TotalCredit - TotalDebit - LockAmt - UnlockedAmount Balance,'" + MasterAccountName + "' as MasterSubAccount from(select regno, " +
                             "sum(TotalCredit)TotalCredit, sum(TotalDebit) TotalDebit, sum(LockAmt) LockAmt, sum(unLockedAmount) " +
                             " UnlockedAmount from(Select a.regno, Sum(credit) TotalCredit, sum(Debit)TotalDebit, 0 as 'LockAmt', 0 as " +
                             " 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID = '" + username2 + "' " +
                             " and trStatus = 1 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, sum(Debit) " +
                             " LockAmt, 0 as 'unLockedAmount' from member_account a, member_master b where a.regno = b.regno and b.LOGINID " +
                             " = '" + username2 + "' and trStatus = 0 group by a.regno union all select a.regno, 0 as totalCredit, 0 as TotalDebit, " +
                             " 0 as 'LockAmt', sum(Debit) unLockedAmount from member_account a, member_master b where a.regno = b.regno and " +
                             " b.LOGINID = '" + username2 + "' and trStatus = 2 group by a.regno) as tabla group by regno) as tabla2, member_master m," +
                             " country c where tabla2.regno = m.regno and m.cid = c.CID) as d where A.REGNO = B.REGNO And a.cid = c.cid " +
                             " AND a.loginid = '" + username2 + "'";
                DT2 = GetDataTable(consulta2);
                DataRow Fila2 = sortedDT.NewRow();
                int l = 0;
                while (l < Tabla.Columns.Count)
                {
                    Fila2[l] = DBNull.Value;
                    l++;
                }
                Fila2["memcode"] = DT2.Rows[0]["Regno"].ToString();
                Fila2["LoginID"] = DT2.Rows[0]["LoginID"].ToString();
                Fila2["Name"] = DT2.Rows[0]["Name"].ToString();
                Fila2["Country"] = DT2.Rows[0]["CountryName"].ToString();
                Fila2["Regdate"] = DT2.Rows[0]["Regdate"].ToString();
                Fila2["Balance"] = Math.Round(Convert.ToDecimal(DT2.Rows[0]["Balance"].ToString()), 2);
                Fila2["MasterSubAccount"] = DT2.Rows[0]["MasterSubAccount"].ToString();
                sortedDT.Rows.InsertAt(Fila2, 0);
                return sortedDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public object GetScalerSP(SqlParameter[] arrParam, string strSql)
        {
            OpenConnection();
            // Set Command object properties.
            _mDataCom.Parameters.Clear();
            _mDataCom.CommandType = CommandType.StoredProcedure;
            _mDataCom.CommandText = strSql;
            _mDataCom.CommandTimeout = 18000;
            _mDataCom.Parameters.Add(arrParam[0]);

            // Create SqlDataReader object.
            //SqlDataReader dReader;
            Object oObject;
            oObject = _mDataCom.ExecuteScalar();
            return oObject;
        }
        #endregion

        public decimal GetDonorBalance(long regno)
        {
            string query = "select sum (debit) Debit, Sum(Credit) Credit, sum(Credit)- sum(debit) Balance from (" +
            " Select 0 as 'Debit', isnull(sum(credit), 0) as 'Credit' from Member_account where regno = " + regno + " and transtype" +
            "='primaryBoard' and crdb = 'C' and debit <= 0 and credit > 0 and trstatus = 1 UNION ALL Select 0 as 'Debit'," +
            " isnull(sum(credit), 0) as 'Credit' from Member_account where regno = " + regno + " and transtype = 'ReNewLevelIncome'" +
            " and crdb = 'C' and debit <= 0 and credit > 0 and trstatus = 1 UNION ALL Select 0 as 'Debit',isnull(Sum(credit), 0) as " +
            "'Credit'  from member_Account where regno = " + regno + " and crdb = 'C' and credit > 0  and debit <= 0 and transtype=" +
            "'Deposit' and trstatus = 1 and trAppBy = 1 UNION ALL select 0 as 'Debit',isnull(sum(tlIncome), 0) as 'Credit' from " +
            " MEMBER_Level where regno = " + regno + " UNION ALL select 0 as 'Debit',isnull(SUM(CREDIT), 0) as 'Credit' from " +
            " member_account where transtype in('Vouchercancel') and trstatus = 1 and regno = " + regno + "union all Select isnull" +
            " (Sum(debit), 0) as 'Debit',0 as 'Credit'  from member_Account where regno = " + regno + "  and crdb = 'D' and debit >" +
            " 0 and transtype in('payment','withdraw') and trstatus< 2 UNION ALL Select isnull(Sum(Debit), 0) as 'Debit',0 as " +
            " 'Credit'  from member_Account where regno = 3  and crdb = 'D' and debit > 0 and transtype = 'Deduct' and trstatus< 2" +
            " UNION ALL Select isnull(Sum(Debit), 0) as 'Debit',0 as 'Credit'  from member_Account where regno = " + regno + " and " +
            " crdb = 'D' and debit > 0 and transtype in ('Renewal_Income','Registration_Payment') and trstatus< 2 UNION ALL  Select " +
            " isnull(Sum(Debit), 0) as 'Debit', 0 as 'Credit'  from member_Account where regno =" + regno + " and crdb = 'D' " +
            " and debit > 0 and transtype in('voucher') and trstatus< 2) as t1 ";
            DataTable dt = GetDataTable(query);
            return Convert.ToDecimal(dt.Rows[0]["Balance"].ToString());
        }

        public void DetectPaymentOrigen(long regno, decimal Amount, string transtype, string remark, string Transdate, long ACID)
        {
            try
            {
                //Detect if the payment comes from Deposits Wallet or Commissions acumulated   27/08/2018
                //Detecting if the purchase was made with DepositWallet Money before this renewal/registration
                decimal DonorBalance = GetDonorBalance(regno) + Amount;
                string query = "select sum(credit) credit, sum(debit) debit,sum(credit)-sum(debit) Balance from DepositsWallet where regno=" + regno;
                DataTable dt = GetDataTable(query);
                decimal DepositsWalletBalance = 0;
                if (dt.Rows[0][0].ToString() == "")
                {
                    DepositsWalletBalance = 0;
                }
                else
                {
                    DepositsWalletBalance = Convert.ToDecimal(dt.Rows[0]["Balance"].ToString());
                }

                //if (DepositsWalletBalance > 0) // Deposit Balance is positive
                //{
                // This means that DepositWallet Balance > Renewal / Registration, then I can pay the Renewal/Registration with this balance
                if (DepositsWalletBalance > Amount)
                {
                    //All AmountRegistration can be paid with DepositsWallet
                    query = "INSERT INTO DepositsWallet (TransType,CREDIT,DEBIT,BALANCE,DESCRIPTION,Transdate,REGNO,ACID) VALUES " +
                   "('" + transtype + "',0," + Amount + "," + (DepositsWalletBalance - Amount) + ",'" + remark + "','" + Transdate + "'," + regno + "," + ACID + ") ";
                    ExecuteSql(query);
                }
                else if (DepositsWalletBalance == 0)
                {
                    //LA SOLICITUD DEBE SALIR DE LAS COMISIONES YA QUE NO HAY BALANCE EN LA TABLA ESCONDIDA
                    query = "INSERT INTO DepositsWallet (TransType,CREDIT,DEBIT,BALANCE,DESCRIPTION,Transdate,REGNO,ACID) VALUES " +
                 "('" + transtype + "',0," + DepositsWalletBalance + ",0,'" + remark + " - NO BALANCE IN DEPOSIT WALLET, THIS WITHDRAW WILL BE DEDUCTED FROM COMMISSIONS TOTALLY','" + Transdate + "'," + regno + "," + ACID + ") ";
                    ExecuteSql(query);
                }
                else
                {
                    //In this case only will be deducted from Deposits Wallet the amount Available, the rest will be deducted from Commissions
                    query = "INSERT INTO DepositsWallet (TransType,CREDIT,DEBIT,BALANCE,DESCRIPTION,Transdate,REGNO,ACID) VALUES " +
                    "('" + transtype + "',0," + DepositsWalletBalance + ",0,'" + remark + " - PART DEPOSITWALLETS AND ANOTHER PART COMMISSIONS','" + Transdate + "'," + regno + "," + ACID + ") ";
                    ExecuteSql(query);
                }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public decimal GetDepositsWalletBalance(long regno)
        {
            string query = "select sum(credit) credit, sum(debit) debit, sum(credit) - sum(debit) Balance  from DepositsWallet where " +
                " regno=" + regno + " and transtype like 'Deposit%'";
            DataTable dt = GetDataTable(query);
            return Convert.ToDecimal(dt.Rows[0]["Balance"].ToString());
        }

        public bool ValidateInmate(long regno)
        {
            string query = "SELECT STATEINMATE, COUNTYINMATE, FACILITYINMATE,NUMBERINMATE FROM MEMBER_MASTER WHERE REGNO=" + regno;
            DataTable dt0 = GetDataTable(query);
            string StateInmate, CountyInmate, FacilityInmate, NumberInmate;
            StateInmate = dt0.Rows[0]["StateInmate"].ToString();
            CountyInmate = dt0.Rows[0]["CountyInmate"].ToString();
            FacilityInmate = dt0.Rows[0]["FacilityInmate"].ToString();
            NumberInmate = dt0.Rows[0]["NumberInmate"].ToString();
            if ((StateInmate != null && StateInmate != "") && (CountyInmate != null && CountyInmate != "") && (FacilityInmate != null && FacilityInmate != "") && (NumberInmate != null && NumberInmate != ""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ValidateDonorSponsored(long regno)
        {
            string query = "SELECT ISBENEFACTOR FROM MEMBER_MASTER WHERE REGNO=" + regno;
            DataTable dt0 = GetDataTable(query);
            string IsBenefactor = dt0.Rows[0][0].ToString();
            return IsBenefactor;

        }

        public string GetFileNameDocumentID(string LongName)
        {
            int j = LongName.IndexOf('-');
            string ShortName = LongName.Substring(j + 1);
            j = ShortName.IndexOf('-');
            ShortName = ShortName.Substring(j + 1);
            j = ShortName.IndexOf('-');
            ShortName = ShortName.Substring(j + 1);
            j = ShortName.IndexOf('-');
            ShortName = ShortName.Substring(j + 1);
            return ShortName;
        }

        public void InsertarTablaBenefactor(long RegnoBenefactor, long RegnoSponsoredDonor, decimal Credit, decimal Debit, string TransType, string Description, string ShortDescription = "")
        {
            //INSERTAR REGISTRO EN TABLA AUXILIAR DE CONTROL DE DEUDAS ENTRE EL DONOR SPONSORED Y EL BENEFACTOR
            if (ShortDescription == "")
            {
                ShortDescription = Description;
            }
            string LoginBenefactor = getLoginID(RegnoBenefactor);
            string LogindIDSponsoredDonor = getLoginID(RegnoSponsoredDonor);

            //Guardar en una tabla auxiliar esta deuda que está adquiriendo el donante con el benefactor
            string query = "insert into SponsoredBenefactorAccount(regnoDonorSponsored,regnoBenefactor,Credit,Debit,Description," +
           "ShortDescription,Transtype,Transdate) values(" + RegnoSponsoredDonor + "," + RegnoBenefactor + "," + Credit +
           "," + Debit + ",'" + Description + "','" + ShortDescription + "','" + TransType + "','" + GetTimeLP2() + "')";
            ExecuteSql(query);
        }

        public string getLoginID(long regno)
        {
            string LoginID = "";
            string query = "SELECT LOGINID FROM MEMBER_MASTER WHERE REGNO=" + regno;
            DataTable dt = GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                LoginID = dt.Rows[0][0].ToString();
            }
            return LoginID;
        }

        public long getRegno(string LoginID)
        {
            long Regno = 0;
            string query = "SELECT regno FROM MEMBER_MASTER WHERE LOGINID='" + LoginID + "'";
            DataTable dt = GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                Regno = Convert.ToInt64(dt.Rows[0][0].ToString());
            }
            return Regno;
        }

        public string getCounty(int IDCounty)
        {
            string CountyName = "";
            string query = "select CountyName from county where IDCounty=" + IDCounty;
            DataTable dt = GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                CountyName = dt.Rows[0][0].ToString();
            }
            return CountyName;
        }

        public string getCause(int IDCause)
        {
            string Cause = "";
            string query = "select Cause from Causes where ID=" + IDCause;
            DataTable dt = GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                Cause = dt.Rows[0][0].ToString();
            }
            return Cause;
        }

        public bool IsInmateSponsor(string LoginID)
        {
            string query = "SELECT ISINMATESPONSOR FROM MEMBER_MASTER WHERE LOGINID='" + LoginID + "'";
            DataTable dt0 = GetDataTable(query);
            if (dt0.Rows[0][0].ToString() == "YES")
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        public decimal getDebtBetweenDonorBenefactor(long regnoSponsored)
        {
            string LoginIDSponsored = getLoginID(regnoSponsored);
            string query = "SELECT * FROM MEMBER_MASTER WHERE REGNO=" + regnoSponsored;
            DataTable DT = GetDataTable(query);
            string LoginIDBenefactor = getLoginID(Convert.ToInt64(DT.Rows[0]["ParentRegnoM"]));
            decimal Balance = 0;

            query = "select regno, sum(isnull(credit,0)) credit, sum(isnull(debit,0)) debit from (select row_number() over(order by" +
                " Transdate desc) ID, regno, TransDate, CONVERT(DECIMAL(10, 2), credit)  credit, CONVERT(DECIMAL(10, 2), debit) debit," +
                " transType, Remark from(select regno, TransDate, credit, debit, transType, Remark from member_account where transtype" +
                " in ('Deposit', 'Debit Internal Transfer','Credit Internal Transfer') and  regno = " + regnoSponsored + "union all" +
                " select regnoDonorSponsored regno, Transdate, Credit, Debit, Transtype, Description Remark from" +
                " SponsoredBenefactorAccount where regnoDonorSponsored = " + regnoSponsored + ") as t1  where remark like" +
                " '%Deposit%from%' or remark like '%Payment Debt%' or remark like '%Registration Payment%' or remark like '%Renewal" +
                " Payment%' or Remark like '%Forgiven%') as t2 group by regno";

            DT = GetDataTable(query);
            decimal Credit = 0;
            decimal Debit = 0;
            if (DT.Rows.Count > 0)
            {
                Debit = Convert.ToDecimal(DT.Rows[0]["Credit"].ToString());
                Credit = Convert.ToDecimal(DT.Rows[0]["Debit"].ToString());
            }
            Balance = Debit - Credit;
            return Balance;
        }

        public string GetRealCountryName(string CountryName)
        {
            string FinalCountryname = "";
            if (CountryName.Contains("-"))
            {
                FinalCountryname = CountryName.Substring(0, CountryName.IndexOf('-'));
            }
            else
            {
                FinalCountryname = CountryName;
            }
            return FinalCountryname;
        }

        public string ConvertDataTableToHTMLString(System.Data.DataTable dt, string filter, string sort, string fontsize, string border, bool headers, bool useCaptionForHeaders, string language)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<table width='100%' border='" + border + "'b>");
                if (headers)
                {
                    sb.Append("<tr>");
                    foreach (System.Data.DataColumn dc in dt.Columns)
                    {
                        if (useCaptionForHeaders)
                            sb.Append("<td width='30%'><b><font face=Arial size=2>" + dc.Caption + "</font></b></td>");
                        else
                        if (dc.ColumnName == "eVoucher No")
                        {
                            dc.ColumnName = language == "ENGLISH" ? "E-voucher #" : "N° E-voucher";
                        }
                        if (dc.ColumnName == "eVoucher Code")
                        {
                            dc.ColumnName = language == "ENGLISH" ? "E-voucher Code" : "Código E-voucher";
                        }
                        if (dc.ColumnName == "Sold To")
                        {
                            dc.ColumnName = language == "ENGLISH" ? dc.ColumnName : "Vendido a";
                        }
                        sb.Append("<td width='30%'><b><font face=Arial size=2>" + dc.ColumnName + "</font></b></td>");
                    }
                    sb.Append("</tr>");
                }
                //write table data
                foreach (System.Data.DataRow dr in dt.Rows) //foreach (System.Data.DataRow dr in dt.Select(filter, sort))
                {
                    sb.Append("<tr>");
                    foreach (System.Data.DataColumn dc in dt.Columns)
                    {
                        if (dc.ColumnName == "mrp" || dc.ColumnName == "dp" || dc.ColumnName == "bv" || dc.ColumnName == "totalbv" || dc.ColumnName == "totalamount")
                        {
                            sb.Append("<td align='right'><font face=Arial size=" + fontsize + ">" + string.Format("{0:f}", Convert.ToDouble(dr[dc].ToString())) + "</font></td>");
                        }
                        else
                        {
                            sb.Append("<td height='30'><font face=Arial size=" + fontsize + ">" + dr[dc].ToString() + "</font></td>");
                        }
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</table>");
                return sb.ToString();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetBodyPurchaseEvoucherMail(string reg, string EvoucherName, decimal PriceEvoucher, string Language, DataTable table, int memcode = 0)
        {
            string fName, loginid, email, phone, subject;
            try
            {
                subject = "Thank you For Purchase Evoucher";
                DataTable dt = GetDataTable("select  loginid, mobile,  isnull(fname,'')+' '+isnull(mname,'')+' '+isnull(lname,'')" +
                    " as fname, isnull(emailid,'') emailId, isnull(title,'') totle from member_master where regno=" + reg);

                loginid = dt.Rows[0]["loginId"].ToString();
                phone = dt.Rows[0]["mobile"].ToString();
                fName = dt.Rows[0]["fname"].ToString();
                email = dt.Rows[0]["emailId"].ToString();
                string Mr = dt.Rows[0]["totle"].ToString();

                string Title = Language == "SPANISH" ? "Bienvenido a Ayudaos" : "Welcome to HelpFundYou";
                string LinkLogin = Language == "SPANISH" ? "https://cotdp.org/App/BOM/loginSP.aspx" : "https://cotdp.org/App/BOM/login.aspx";
                string Logo = Language == "SPANISH" ? "https://i.postimg.cc/kgLQmbnZ/Logo-Ayudaos.png" : "https://i.postimg.cc/zXXSSNtd/Logo-Help-Fund-You.png";
                string MsgWelcome = Language == "SPANISH" ? "B i e n v e n i d o . . . !" : "W e l c o m e . . . !";
                string User = Language == "SPANISH" ? "Usuario" : "Username";
                string Line1 = Language == "SPANISH" ? "Ayudaos.org le da la bienvenida así como a toda su familia." : "HelpFundYou.com welcomes you and your family.";
                string Line2 = Language == "SPANISH" ? "Muchas gracias por participar en nuestros programas y por formar parte de un movimiento que cambiará el modo de vida de muchas personas." : "We thank you very much for participating in our programs and for being a part of a movement that will change the way of life for a lot of people.";
                string Line3 = Language == "SPANISH" ? "Usted no puede imaginar cuánto se aprecian sus donaciones. Le deseamos todo lo mejor y le enviamos todas nuestras bendiciones." : "You cannot imagine how much your donations are appreciated. We wish you all the best and we send you all our blessings.";
                string Line4 = Language == "SPANISH" ? "Nombre del E-voucher : " : "E-voucher Name :";
                string Line5 = Language == "SPANISH" ? "Precio del E-voucher : $" : "E-voucher Price : $";
                string Line6 = Language == "SPANISH" ? "Ud. puede copiar y pegar este código y número de cupón electrónico para luego reenviarlo a la persona que lo necesita para registrarse o para depositar dinero en su cuenta." : "You can forward this email or copy and paste this E-voucher code and number to the person that needs it to register or to deposit some money into his or her account.";
                string Line7 = Language == "SPANISH" ? "Que Dios le bendiga." : "God bless you!";
                string MailContact = Language == "SPANISH" ? "contact@ayudaos.org" : "contact@helpfundyou.org";
                string GoLogin = Language == "SPANISH" ? "Ir a Inicio de Sesión" : "Go to Login";

                string SBody = "";
                SBody = "<!DOCTYPE html><html lang=\"en\">";
                SBody = SBody + "<head><meta charset =\"utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">";
                SBody = SBody + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">";
                SBody = SBody + "<title>" + Title + "</title>";
                SBody = SBody + "<link href=\"css/bootstrap.min.css\" rel=\"stylesheet\"></head>";
                SBody = SBody + "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\"></script>";
                SBody = SBody + "<script src=\"js/bootstrap.min.js\"></script>";
                SBody = SBody + "</head>";
                SBody = SBody + "<body>";

                SBody = SBody + "<table cborder='0' border='0' cellspacing='0' cellpadding='0' style='Padding-Left:25px;Padding-right:25px;padding-top:0;font-size:15px;width:100%;font-family:Arial;color:black'>";
                //TAMAÑO DE LAS COLUMNAS
                SBody = SBody + "<tr>";
                SBody = SBody + "<td style=\"width:5%;\"></td>";
                SBody = SBody + "<td style=\"width:30%;\"></td>";
                SBody = SBody + "<td style=\"width:30%;\"></td>";
                SBody = SBody + "<td style=\"width:30%;\"</td>";
                SBody = SBody + "<td style=\"width:5%;\"></td>";
                SBody = SBody + "</tr>";

                //LOGO
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"width:30%;text-align:right\">";
                SBody = SBody + "<a href=" + LinkLogin + "><img style=\"display: block;\" width=\"100%\" height=\"90px\" src=" + Logo + " alt=Logo-Ayudaos/></a> ";
                SBody = SBody + "</td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";


                //FECHA
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"color:black;text-align:right\"><h4>" + DateTime.Now.ToString("MM/dd/yyyy") + "</h4></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";


                //NOMBRE DONANTE
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                if (Language == "SPANISH")
                {
                    SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + Mr.ToString().Replace("Mr.", "Sr.").Replace("Mrs.", "Sra.").Replace("Ms.", "Srta.") + " " + fName.ToString() + " </h3></td>";
                }
                else
                {
                    SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + Mr.ToString() + " " + fName.ToString() + " </h3></td>";
                }
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                //USUARIO
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"color:black;font-weight:bold;font-family:Arial\"><h4>" + User + ": " + loginid.ToString() + " </h4></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                //ID
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style='color:black;padding-top:3px;font-family:Arial'>ID#: " + memcode + " </td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                //EVOUCHER NAME
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"color:black;padding-top:3px;font-family:Arial\">" + Line4 + EvoucherName + " </td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                //EVOUCHER PRICE
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"color:black;padding-top:3px;font-family:Arial\">" + Line5 + Convert.ToString(PriceEvoucher) + " </td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                //CUERPO DEL MENSAJE 
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td colspan=3 width='100%' align='center' style='padding-top:30px'>" + ConvertDataTableToHTMLString(table, "", "", "2pt", "1", true, false, Language) + "</td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td colspan=3><p style='font-family:Arial;padding-top:3px;color:black'>" + Line6 + "</p></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td><p style='font-family:Arial;padding-top:3px;color:black'>" + Line7 + "</p></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td style=\"font-weight:bold\"><a href=\"mailto:" + MailContact + " target=\"_top\">" + MailContact + "</a></br></br></br></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td colspan=\"2\"style=\"text-align:right\"><a href=" + LinkLogin + "><button style=\"background-color:#42a50d;color:#ffffff;border-radius:7px;height:30px\">" + GoLogin + "</button></a></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";

                SBody = SBody + "</table>";
                SBody = SBody + "</body>";
                return SBody;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetBodyWelcomeMail(DataTable dtM, string Language)
        {
            string Title = Language == "SPANISH" ? "Bienvenido a Ayudaos" : "Welcome to HelpFundYou";
            string LinkLogin = Language == "SPANISH" ? "https://cotdp.org/App/BOM/loginSP.aspx" : "https://cotdp.org/App/BOM/login.aspx";
            string Logo = Language == "SPANISH" ? "https://i.postimg.cc/kgLQmbnZ/Logo-Ayudaos.png" : "https://i.postimg.cc/zXXSSNtd/Logo-Help-Fund-You.png";
            string MsgWelcome = Language == "SPANISH" ? "B i e n v e n i d o . . . !" : "W e l c o m e . . . !";
            string User = Language == "SPANISH" ? "Usuario" : "Username";

            string Line1 = Language == "SPANISH" ? "Ayudaos.org le da la bienvenida así como a toda su familia." : "HelpFundYou.com welcomes you and your family.";
            string Line2 = Language == "SPANISH" ? "Muchas gracias por participar en nuestros programas y por formar parte de un movimiento que cambiará el modo de vida de muchas personas." : "We thank you very much for participating in our programs and for being a part of a movement that will change the way of life for a lot of people.";
            string Line3 = Language == "SPANISH" ? "Usted no puede imaginar cuánto se aprecian sus donaciones. Le deseamos todo lo mejor y le enviamos todas nuestras bendiciones." : "You cannot imagine how much your donations are appreciated. We wish you all the best and we send you all our blessings.";
            string Line4 = Language == "SPANISH" ? "Le aconsejamos que guarde en un lugar muy seguro, las informaciones de su “Carta de bienvenida” que adjuntamos a este email, así como de la copia que puede haber imprimido o bajado a su computadora durante el proceso de registración. Toda la información confidencial de su cuenta está en este documento." : "We remind you to keep a copy of your 'Welcome Letter' that you copied or downloaded to your computer during the registering process, in a very safe place. All of your account’s sensitive information is in this document.";
            string Line5 = Language == "SPANISH" ? "Le invitamos a que usted realice muchas obras de Misericordia. No deje que la codicia sea el motivo de sus acciones." : "We invite you to practice a lot of works of Mercy. Do not let the greed be the motive of your actions.";
            string Line6 = Language == "SPANISH" ? "Desde el fondo de nuestros corazones," : "From the bottom of our heart,";
            string Line7 = Language == "SPANISH" ? "Que Dios le bendiga." : "God bless you!";
            string MailContact = Language == "SPANISH" ? "contact@ayudaos.org" : "contact@helpfundyou.org";
            string GoLogin = Language == "SPANISH" ? "Ir a Inicio de Sesión" : "Go to Login";

            string SBody = "";
            SBody = "<!DOCTYPE html><html lang=\"en\">";
            SBody = SBody + "<head><meta charset =\"utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">";
            SBody = SBody + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">";
            SBody = SBody + "<title>" + Title + "</title>";
            SBody = SBody + "<link href=\"css/bootstrap.min.css\" rel=\"stylesheet\"></head>";
            SBody = SBody + "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\"></script>";
            SBody = SBody + "<script src=\"js/bootstrap.min.js\"></script>";
            SBody = SBody + "</head>";
            SBody = SBody + "<body>";

            SBody = SBody + "<table cborder='0' border='0' cellspacing='0' cellpadding='0' style='Padding-Left:25px;Padding-right:25px;padding-top:0;font-size:15px;width:100%;font-family:Arial;color:black'>";
            //TAMAÑO DE LAS COLUMNAS
            SBody = SBody + "<tr>";
            SBody = SBody + "<td style=\"width:5%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"</td>";
            SBody = SBody + "<td style=\"width:5%;\"></td>";
            SBody = SBody + "</tr>";

            //LOGO
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"width:30%;text-align:right\">";
            SBody = SBody + "<a href=" + LinkLogin + "><img style=\"display: block;\" width=\"100%\" height=\"90px\" src=" + Logo + " alt=Logo-Ayudaos/></a> ";
            SBody = SBody + "</td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //MENSAJE DE BIENVENIDA
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><h3 style=\"color:black;font-size:140%;text-shadow: 2px 2px 5px orange;font-weight:bold\"> " + MsgWelcome + "</h3></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //FECHA
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"color:black;text-align:right\"><h4>" + Convert.ToDateTime(dtM.Rows[0]["regdate"]).ToString("MM/dd/yyyy") + "</h4></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";


            //NOMBRE DONANTE
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            if (Language == "SPANISH")
            {
                SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + dtM.Rows[0]["title"].ToString().Replace("Mr.", "Sr.").Replace("Mrs.", "Sra.").Replace("Ms.", "Srta.") + " " + dtM.Rows[0]["Name"].ToString() + " </h3></td>";
            }
            else
            {
                SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + dtM.Rows[0]["title"].ToString() + " " + dtM.Rows[0]["Name"].ToString() + " </h3></td>";
            }
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //USUARIO
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"color:black;font-weight:bold;font-family:Arial\"><h4>" + User + ": " + dtM.Rows[0]["loginid"].ToString() + " </h4></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //CUERPO DEL MENSAJE
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            if (Language == "SPANISH")
            {
                SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line1 + "</p></td>";
            }
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line2 + "</p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line3 + "</br></p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line4 + "</p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line5 + "</br></p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line6 + "</br></br></p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line7 + "</p></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\" style=\"font-weight:bold\"><a href=\"mailto:" + MailContact + " target=\"_top\">" + MailContact + "</a></br></br></br></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"2\"style=\"text-align:right\"><a href=" + LinkLogin + "><button style=\"background-color:#42a50d;color:#ffffff;border-radius:7px;height:30px\">" + GoLogin + "</button></a></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "</table>";
            SBody = SBody + "</body>";
            return SBody;
        }


        public string GetStandardFormatMail(string Title, string BodyTitle, List<string> BodyMsg, long regno, string Language)
        {
            DataTable dtM = GetDataTable("SELECT * FROM MEMBER_MASTER WHERE REGNO = " + regno);

            string LinkLogin = Language == "SPANISH" ? "https://cotdp.org/App/BOM/loginSP.aspx" : "https://cotdp.org/App/BOM/login.aspx";
            string Logo = Language == "SPANISH" ? "https://i.postimg.cc/kgLQmbnZ/Logo-Ayudaos.png" : "https://i.postimg.cc/zXXSSNtd/Logo-Help-Fund-You.png";
            string User = Language == "SPANISH" ? "Usuario" : "Username";
            string MailContact = Language == "SPANISH" ? "contact@ayudaos.org" : "contact@helpfundyou.org";
            string GoLogin = Language == "SPANISH" ? "Ir a Inicio de Sesión" : "Go to Login";

            string SBody = "";
            SBody = "<!DOCTYPE html><html lang=\"en\">";
            SBody = SBody + "<head><meta charset =\"utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">";
            SBody = SBody + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">";
            SBody = SBody + "<title>" + Title + "</title>";
            SBody = SBody + "<link href=\"css/bootstrap.min.css\" rel=\"stylesheet\"></head>";
            SBody = SBody + "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\"></script>";
            SBody = SBody + "<script src=\"js/bootstrap.min.js\"></script>";
            SBody = SBody + "</head>";
            SBody = SBody + "<body>";

            SBody = SBody + "<table cborder='0' border='0' cellspacing='0' cellpadding='0' style='Padding-Left:25px;Padding-right:25px;padding-top:0;font-size:15px;width:100%;font-family:Arial;color:black'>";
            //TAMAÑO DE LAS COLUMNAS
            SBody = SBody + "<tr>";
            SBody = SBody + "<td style=\"width:5%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"></td>";
            SBody = SBody + "<td style=\"width:30%;\"</td>";
            SBody = SBody + "<td style=\"width:5%;\"></td>";
            SBody = SBody + "</tr>";

            //LOGO
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"width:30%;text-align:right\">";
            SBody = SBody + "<a href=" + LinkLogin + "><img style=\"display: block;\" width=\"100%\" height=\"90px\" src=" + Logo + " alt=Logo-Ayudaos/></a> ";
            SBody = SBody + "</td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //MENSAJE DE BIENVENIDA
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\"><h3 style=\"color:black;font-size:140%;text-shadow: 2px 2px 5px orange;font-weight:bold\"> " + BodyTitle + "</h3></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //FECHA
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"color:black;text-align:right\"><h4>" + DateTime.Now.ToString("MM/dd/yyyy") + "</h4></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";


            //NOMBRE DONANTE
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            string donorTitle = string.IsNullOrEmpty(dtM.Rows[0]["title"].ToString()) ? "" : dtM.Rows[0]["title"].ToString();
            string donorName = dtM.Rows[0]["FName"] + " " + dtM.Rows[0]["LName"];
            string donorLogin = string.IsNullOrEmpty(dtM.Rows[0]["loginid"].ToString()) ? "" : dtM.Rows[0]["loginid"].ToString();

            if (Language.ToUpper() == "SPANISH")
            {

                SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + donorTitle.Replace("Mr.", "Sr.").Replace("Mrs.", "Sra.").Replace("Ms.", "Srta.") + " " + donorName + " </h3></td>";
            }
            else
            {
                SBody = SBody + "<td><h3 style=\"color:black:font-weight:bold\">" + donorTitle + " " + donorName + " </h3></td>";
            }
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //USUARIO
            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td style=\"color:black;font-weight:bold;font-family:Arial\"><h4>" + User + ": " + donorLogin + " </h4></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            //CUERPO DEL MENSAJE
            foreach (string Line in BodyMsg)
            {
                SBody = SBody + "<tr>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "<td colspan=\"3\"><p style=\"font-family:Arial;color:black\">" + Line + "</p></td>";
                SBody = SBody + "<td></td>";
                SBody = SBody + "</tr>";
            }

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"3\" style=\"font-weight:bold\"><a href=\"mailto:" + MailContact + " target=\"_top\">" + MailContact + "</a></br></br></br></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "<tr>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "<td colspan=\"2\"style=\"text-align:right\"><a href=" + LinkLogin + "><button style=\"background-color:#42a50d;color:#ffffff;border-radius:7px;height:30px\">" + GoLogin + "</button></a></td>";
            SBody = SBody + "<td></td>";
            SBody = SBody + "</tr>";

            SBody = SBody + "</table>";
            SBody = SBody + "</body>";
            return SBody;
        }




    }
}