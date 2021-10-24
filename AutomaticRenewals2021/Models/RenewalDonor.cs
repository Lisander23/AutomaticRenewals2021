using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COTDP.Models
{
    public class RenewalDonor
    {
        public long regno { get; set; }
        public string LoginId { get; set; }
        public int RID { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public bool MailStatus { get; set; }
        public bool Solved { get; set; }
        public string RenewalType { get; set; }
        public string MailNextRenewal { get; set; }
        public double LastRenewalAmount { get; set; }
        public double KitPrice { get; set; }
        public int CID { get; set; }


    }
}