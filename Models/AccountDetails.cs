using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Indigo.Models
{
    /// <summary>
    /// Account Details Request contains the statement request details
    /// </summary>
    [DataContract]
    public class StatementRequest
    {
        /// <summary>
        /// Account number is a required field
        /// </summary>
        [Required] [DataMember]
        public string AccountNumber { get; set; }
        /// <summary>
        /// Card last four digits is required
        /// </summary>
        [Required][DataMember]
        public string CardLastFourDigits { get; set; }
        /// <summary>
        /// Token is required
        /// </summary>
        [Required][DataMember]
        public string Token { get; set; }
        /// <summary>
        /// From Time defines statement start time and is required
        /// </summary>
        [Required] [DataMember]
        public DateTime FromTime { get; set; }
        /// <summary>
        /// To Time defines the end time of the statement and is required
        /// </summary>
        [Required][DataMember]
        public DateTime ToTime { get; set; }
      
    }
}