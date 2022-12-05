using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Indigo.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        void Debug(string message, string arg = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        void Info(string message, string arg = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        void Warning(string message, string arg = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        void Error(string message, string arg = null);
    }
}