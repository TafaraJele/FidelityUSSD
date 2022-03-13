using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Veneka.Module.TranzwareCompassPlusFIMI;
using System.IO;
using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;
using Indigo.Utility;
using Veneka.Indigo.Abstractions.Models;
using Veneka.Indigo.Core;

namespace Indigo.Controllers
{
    /// <summary>
    /// Loads funds to prepaid account
    /// </summary>
    [Serializable]
    public class FundsLoadController : ApiController
    {
        /// <summary>
        /// Loads funds to account
        /// </summary>
        /// <param name="funds"></param>
        /// <returns></returns>
        [HttpPost]

        public FundLoadResponse FundsLoad([FromBody] FundsLoad funds)
        {
            //initialising Response
            Response resp = null;
            Application application = new Application();
            CommonServices services = new CommonServices();
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            try
            {
                fimilogger.Info("************Funds load controller**********");
                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };
                var settings = services.GetSettings();

                var response = application.Fundsload(funds, settings);

                return response;
            }
            catch (Exception ex)
            {
                fimilogger.Info($"Logging Funds load exception {ex.Message}");

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



