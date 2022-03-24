using Indigo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Veneka.Indigo.Abstractions.Models;
using Veneka.Indigo.Core;

namespace Indigo.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class DefundPrepaidAccountController : ApiController
    {
        /// <summary>
        /// Defunds account given account number
        /// </summary>
        /// <param name="funds"></param>
        /// <returns></returns>
        [HttpPost]

        public FundLoadResponse Defund([FromBody] FundsLoad funds)
        {
            Application application = new Application();
            CommonServices services = new CommonServices();
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            try
            {
                fimilogger.Info("************Defund load controller**********");
                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };
                var settings = services.GetSettings();

                var response = application.Defund(funds, settings);

                return response;
            }
            catch (Exception ex)
            {
                fimilogger.Info($"Logging Defund load exception {ex.Message}");

                var response = new FundLoadResponse
                {
                    ResponseType = Veneka.Indigo.Abstractions.Enums.ResponseType.EXCEPTION,
                    Messages = new System.Collections.Generic.List<Messages> { new Messages { Message = ex.Message } }
                };

                return response;
            }

        }

    }
}