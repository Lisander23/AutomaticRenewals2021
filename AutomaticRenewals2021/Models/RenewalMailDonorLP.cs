using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COTDP.Models
{
    public class RenewalMailLPDonor
    {
        public long regno { get; set; }
        public string LoginId { get; set; }
        public DateTime RenewalDate { get; set; }
        public string MailSent { get; set; }
        public string RenewalType { get; set; }
        public string StatusTransaction { get; set; }
        public string Comments { get; set; }
        public DateTime RegisterDate { get; set; }
        public double Amount { get; set; }
        public double Donation { get; set; }
        public int CID { get; set; }
        public double LastRenewalAmount { get; set; }
        public DateTime? LastMailSentDate { get; set; }
        public bool AutomaticRenewal { get; set; } = false;
        public int RID { get; set; }
        public String Language { get; set; }
        public string PaymentMode { get; set; }
        public string EmailId { get; set; }
    }
}