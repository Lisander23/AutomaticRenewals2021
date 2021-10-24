using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace COTDP.Services
{

    public class Donor
    {

        public long regno { get; set; }
        public string LoginId { get; set; }
        public string EmailId { get; set; }
        public decimal Donation { get; set; }
        public DateTime RenewalDate { get; set; }
        public bool AutomaticRenewal { get; set; }
        public int RID { get; set; }
        public string MailSent { get; set; }
        public string Language { get; set; }
        public string StatusTransaction { get; set; }
        public string Comments { get; set; }
        public string RenewalType { get; set; }
        public int CountryId { get; set; }
        public double RenewalAmount { get; set; } = 0;
        public double Balance { get; set; } = 0;
        public string PaymentMode { get; set; }
        public DateTime? LastMailSentDate { get; set; }

    }
}