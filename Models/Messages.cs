using Indigo.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Indigo.Models
{
    public class Messages
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageType MessageType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        public string Code { get; set; }
    }
}