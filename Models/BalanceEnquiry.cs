using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Indigo.Models
{
    [DataContract]
    public class BalanceEnquiry
    {
        [Required][DataMember]
        public string AccountNumber { get; set; }
        [Required][DataMember]
        public string CardLast4FourDigits { get; set; }
        [Required][DataMember]
        public string Token { get; set; }

    }
}