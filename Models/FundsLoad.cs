using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Indigo.Models
{
    public class FundsLoad
    {

        public string phoneNumber { get; set; }
        public string last4Digits { get; set; }
    
        public Decimal amount { get; set; }

        public string token { get; set; }
    }
}