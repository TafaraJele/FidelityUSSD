using Indigo.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;

namespace Indigo.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StatementResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Transactions> Transactions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public StatementResponse()
        {
            Transactions = new List<Transactions>();
        }
        /// <summary>
        /// Alist of messages in the response
        /// </summary>
        public List<Messages> Messages{ get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ResponseType ResponseType { get; set; }
    }
}