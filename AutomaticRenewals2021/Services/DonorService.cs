using COTDP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace COTDP.Services
{
    public class DonorService
    {
        DataUtility objDUT = new DataUtility();


        public Donor GetDonorDetailsModel(long regno)
        {
            string query = "SELECT * FROM MEMBER_Master WHERE REGNO=" + regno;
            DataTable dt = objDUT.GetDataTable(query);

            Donor donor = ConvertDataRowToDonor(dt.Rows[0]);
            return donor;
        }

        public List<Donor> ConvertDTtoDonorList(DataTable dt)
        {
            List<Donor> donorList = new List<Donor>();
            foreach (DataRow line in dt.Rows)
            {
                Donor _donor = ConvertDataRowToDonor(line);
                donorList.Add(_donor);
            }
            return donorList;
        }

        public Donor ConvertDataRowToDonor(DataRow line)
        {
            Donor donor = new Donor();
            try
            {
                donor.regno = Convert.ToInt16(line["Regno"].ToString());
                donor.LoginId = Convert.ToString(line["LOGINID"]);
                donor.EmailId = line["EmailID"].ToString();
                if (line.Table.Columns.Contains("DONATION"))
                {
                    donor.Donation = Convert.ToDecimal(line["DONATION"].ToString());
                }
                if (line.Table.Columns.Contains("RENEWALDATE"))
                {
                    donor.RenewalDate = Convert.ToDateTime(line["RENEWALDATE"].ToString());
                }
                if (line.Table.Columns.Contains("AUTOMATICRENEWAL"))
                {
                    donor.AutomaticRenewal = line["AUTOMATICRENEWAL"].ToString() == "0" ? false : true;
                }
                if (line.Table.Columns.Contains("RID"))
                {
                    donor.RID = Convert.ToInt16(line["RID"].ToString());
                }
                if (line.Table.Columns.Contains("MailSent"))
                {
                    donor.MailSent = line["MailSent"].ToString();
                }
                donor.Language = line["Language"].ToString();
                if (line.Table.Columns.Contains("RenewalType"))
                {
                    donor.RenewalType = line["RenewalType"].ToString();
                }
                if (line.Table.Columns.Contains("StatusTransaction"))
                {
                    donor.StatusTransaction = line["StatusTransaction"].ToString();
                }
                if (line.Table.Columns.Contains("Comments"))
                {
                    donor.Comments = line["Comments"].ToString();
                }
                if (line.Table.Columns.Contains("CID"))
                {
                    donor.CountryId = Convert.ToInt16(line["CID"].ToString());
                }
                if (line.Table.Columns.Contains("PaymentMode"))
                {
                    donor.PaymentMode = line["PaymentMode"].ToString();
                }
            }
            catch (Exception ex)
            {
                donor = null;
            }
            return donor;
        }



        public RenewalMailLPDonor ConvertDataRowToRenewalMailLPDonor(DataRow line)
        {
            RenewalMailLPDonor donor = new RenewalMailLPDonor();
            donor.regno = Convert.ToInt16(line["Regno"].ToString());
            donor.LoginId = Convert.ToString(line["LOGINID"]);
            if (line.Table.Columns.Contains("RENEWALDATE"))
            {
                donor.RenewalDate = Convert.ToDateTime(line["RENEWALDATE"].ToString());
            }
            if (line.Table.Columns.Contains("MAILSENT"))
            {
                donor.MailSent = line["MAILSENT"].ToString();
            }
            if (line.Table.Columns.Contains("RENEWALTYPE"))
            {
                donor.RenewalType = line["RENEWALTYPE"].ToString();
            }
            if (line.Table.Columns.Contains("STATUSTRANSACTION"))
            {
                donor.StatusTransaction = line["STATUSTRANSACTION"].ToString();
            }
            if (line.Table.Columns.Contains("COMMENTS"))
            {
                donor.Comments = line["COMMENTS"].ToString();
            }
            if (line.Table.Columns.Contains("REGISTERDATE"))
            {
                donor.RegisterDate = Convert.ToDateTime(line["REGISTERDATE"].ToString());
            }
            if (line.Table.Columns.Contains("AMOUNT"))
            {
                donor.Amount = Convert.ToDouble(line["AMOUNT"].ToString());
            }
            if (line.Table.Columns.Contains("DONATION"))
            {
                donor.Donation = Convert.ToDouble(line["DONATION"].ToString());
            }
            if (line.Table.Columns.Contains("CID"))
            {
                donor.CID = Convert.ToInt16(line["CID"].ToString());
            }
            return donor;
        }


        public List<RenewalMailLPDonor> ConvertDataRowToRenewalMailLPDonorList(DataTable dt)
        {
            List<RenewalMailLPDonor> DonorList = new List<RenewalMailLPDonor>();
            foreach (DataRow line in dt.Rows)
            {
                RenewalMailLPDonor donor = new RenewalMailLPDonor();
                donor.regno = Convert.ToInt16(line["Regno"].ToString());
                donor.LoginId = Convert.ToString(line["LOGINID"]);
                if (line.Table.Columns.Contains("RENEWALDATE"))
                {
                    donor.RenewalDate = Convert.ToDateTime(line["RENEWALDATE"].ToString());
                }
                if (line.Table.Columns.Contains("MAILSENT"))
                {
                    donor.MailSent = line["MAILSENT"].ToString();
                }
                if (line.Table.Columns.Contains("RENEWALTYPE"))
                {
                    donor.RenewalType = line["RENEWALTYPE"].ToString();
                }
                if (line.Table.Columns.Contains("STATUSTRANSACTION"))
                {
                    donor.StatusTransaction = line["STATUSTRANSACTION"].ToString();
                }
                if (line.Table.Columns.Contains("COMMENTS"))
                {
                    donor.Comments = line["COMMENTS"].ToString();
                }
                if (line.Table.Columns.Contains("REGISTERDATE"))
                {
                    donor.RegisterDate = Convert.ToDateTime(line["REGISTERDATE"].ToString());
                }
                if (line.Table.Columns.Contains("AMOUNT"))
                {
                    donor.Amount = Convert.ToDouble(line["AMOUNT"].ToString());
                }
                if (line.Table.Columns.Contains("DONATION"))
                {
                    donor.Donation = Convert.ToDouble(line["DONATION"].ToString());
                }
                if (line.Table.Columns.Contains("CID"))
                {
                    donor.CID = Convert.ToInt16(line["CID"].ToString());
                }
                if (line.Table.Columns.Contains("LASTMAILSENTDATE"))
                {
                    donor.RenewalDate = Convert.ToDateTime(line["LASTMAILSENTDATE"].ToString());
                }
                if (line.Table.Columns.Contains("AutomaticRenewal"))
                {
                    donor.AutomaticRenewal = line["AutomaticRenewal"].ToString() == "0" ? false : true;
                }
                if (line.Table.Columns.Contains("RID"))
                {
                    donor.RID = Convert.ToInt16(line["RID"].ToString());
                }
                if (line.Table.Columns.Contains("LANGUAGE"))
                {
                    donor.Language = line["LANGUAGE"].ToString();
                }
                if (line.Table.Columns.Contains("PAYMENTMODE"))
                {
                    donor.PaymentMode = line["PAYMENTMODE"].ToString();
                }
                if (line.Table.Columns.Contains("EMAILID"))
                {
                    donor.EmailId = line["EMAILID"].ToString();
                }
                DonorList.Add(donor);
            }
            return DonorList;
        }

        public RenewalDonor ConvertDataRowToRenewalDonor(DataRow line)
        {
            RenewalDonor donor = new RenewalDonor();
            try
            {
                donor.regno = Convert.ToInt16(line["Regno"].ToString());
                donor.LoginId = Convert.ToString(line["LOGINID"]);
                if (line.Table.Columns.Contains("RID"))
                {
                    donor.RID = Convert.ToInt32(line["RID"].ToString());
                }
                if (line.Table.Columns.Contains("AnniversaryDate"))
                {
                    donor.AnniversaryDate = Convert.ToDateTime(line["AnniversaryDate"].ToString());
                }
                if (line.Table.Columns.Contains("MailStatus"))
                {
                    donor.MailStatus = line["MailStatus"].ToString() == "0" ? false : true;
                }
                if (line.Table.Columns.Contains("Solved"))
                {
                    donor.MailStatus = line["Solved"].ToString() == "0" ? false : true;
                }
                if (line.Table.Columns.Contains("RenewalType"))
                {
                    donor.RenewalType = line["RenewalType"].ToString();
                }
                if (line.Table.Columns.Contains("MailNextRenewal"))
                {
                    donor.MailNextRenewal = line["MailNextRenewal"].ToString();
                }
                if (line.Table.Columns.Contains("LastRenewalAmount"))
                {
                    donor.LastRenewalAmount = Convert.ToDouble(line["LastRenewalAmount"].ToString());
                }
                if (line.Table.Columns.Contains("KitPrice"))
                {
                    donor.KitPrice = Convert.ToDouble(line["KitPrice"].ToString());
                }
                if (line.Table.Columns.Contains("CID"))
                {
                    donor.CID = Convert.ToInt16(line["CID"].ToString());
                }
            }
            catch (Exception ex)
            {
                donor = null;
            }
            return donor;
        }
    }
}