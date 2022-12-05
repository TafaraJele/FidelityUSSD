
using NLog;


namespace Indigo.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class FIMILogger : ILogger
    {

        private static FIMILogger fimiLogger;

        private static Logger logger;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FIMILogger GetFimiLoggerInstance()
        {
            if (fimiLogger == null)
            {
                fimiLogger = new FIMILogger();
            }
            return fimiLogger;
        }
        private Logger GetLogger(string log)
        {
            if (FIMILogger.logger == null)
                FIMILogger.logger = LogManager.GetLogger(log);

            return FIMILogger.logger;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public void Debug(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Debug(message);
            else
                GetLogger("FIMILoggerRules").Debug(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public void Error(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Error(message);
            else
                GetLogger("FIMILoggerRules").Error(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>

        public void Info(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Info(message);
            else
                GetLogger("FIMILoggerRules").Info(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public void Warning(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Warn(message);
            else
                GetLogger("FIMILoggerRules").Warn(message);
        }
    }
}