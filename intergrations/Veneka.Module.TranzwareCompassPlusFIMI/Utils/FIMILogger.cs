using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Utils
{
    public class FIMILogger : ILogger
    {

        private static FIMILogger fimiLogger;

        private static Logger logger;

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

        public void Debug(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Debug(message);
            else
                GetLogger("FIMILoggerRules").Debug(message);
        }

        public void Error(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Error(message);
            else
                GetLogger("FIMILoggerRules").Error(message);
        }

        public void Info(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Info(message);
            else
                GetLogger("FIMILoggerRules").Info(message);
        }

        public void Warning(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("FIMILoggerRules").Warn(message);
            else
                GetLogger("FIMILoggerRules").Warn(message);
        }
    }
}
