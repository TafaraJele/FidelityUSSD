using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Services
{
	public abstract class Service
	{
        #region Constants
        private const string INTEGRATION_NAME = "TranzwareCompassPlusFIMIServices";
        #endregion
         
        #region Fields
        protected static ILog _log = LogManager.GetLogger(Utils.General.MODULE_LOGGER);
        #endregion

        #region Properties
        public bool IgnoreUntrustedSSL { get; set; }
        #endregion

        #region Protected Methods

        protected void AddUntrustedSSL()
        {
            //if (_log.IsDebugEnabled)
            //    _log.DebugFormat("Ignore Untrusted SSL:\t", IgnoreUntrustedSSL);

            var loggs = FIMILogger.GetFimiLoggerInstance();

            if (loggs != null)
                loggs.Info("Ignore Untrusted SSL " + IgnoreUntrustedSSL);

            if (IgnoreUntrustedSSL)
            {

                ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                                                        X509Chain chain,
                                                        SslPolicyErrors sslPolicyErrors) => true;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            }
        }

        #endregion
	}
}
