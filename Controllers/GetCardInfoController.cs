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
    public class GetCardInfoController : ApiController
    {
        Application application = new Application();
        CommonServices services = new CommonServices();

        /// <summary>
        /// Gets card info given card number
        /// </summary>
        /// <param name="cardnumber"></param>
        /// <returns></returns>
        [HttpGet]
        public BalanceResponse GetCardInfo(string cardnumber)
        {
            //Logger
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            fimilogger.Info("***********Get card info*************");

            var response = new BalanceResponse();

            //get application settings
            var settings = services.GetSettings();
            var request = new AccountCard { };

            response = application.GetNICardDetails(request, cardnumber, settings, response);

            return response;

        }
    }
}
