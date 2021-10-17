using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticRenewals2021.Models
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long Regno { get; set; }
        public int Kid { get; set; }
    }
}
