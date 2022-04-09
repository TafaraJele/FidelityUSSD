using System;
using System.Web.Http;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Indigo.Utility;
using Veneka.Indigo.Core;
using Veneka.Indigo.Abstractions.Models;
using Veneka.Indigo.Abstractions.Enums;

namespace Indigo.Controllers
{
    /// <summary>
    /// Gets card balance
    /// </summary>
    [Serializable]
    public class GetCardBalanceController : ApiController
    {
        Application application = new Application();
        CommonServices services = new CommonServices();


        /// <summary>
        /// Gets card balance given Balance enquiry details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public BalanceResponse GetCardBalance([FromBody] AccountCard request)
        {
            //Logger
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            fimilogger.Info("***********Get Balance Enquiry controller method*************");

            var response = new BalanceResponse();
            var basicResponse = new BasicResponse();
            //validate balance request
            basicResponse = application.ValidateAccountCard(request);

            response = UpdateBalanceResponse(response, basicResponse);

            if (!response.IsRequestValid)
            {
                return response;
            }

            //get card from indigo
            basicResponse = services.GetAccountCardInIndigo(request, basicResponse, out string cardnumber);

            response = UpdateBalanceResponse(response, basicResponse);

            fimilogger.Info($"Logging basic response {basicResponse.ResponseType} {basicResponse.IsAccountCardFound}");
            fimilogger.Info($"Logging statement response after update {response.ResponseType} {response.IsAccountCardFound}");

            if (basicResponse.ResponseType != ResponseType.SUCCESS || !basicResponse.IsAccountCardFound)
            {
                fimilogger.Info($"Logging basic response {basicResponse.ResponseType} {basicResponse.IsAccountCardFound}");
                return response;
            }
            //get application settings
            var settings = services.GetSettings();

            response = application.GetNICardDetails(request, cardnumber, settings, response);

            return response;

        }

       

        private BalanceResponse UpdateBalanceResponse(BalanceResponse response, BasicResponse basicResponse)
        {
            response.IsRequestValid = basicResponse.IsRequestValid;
            response.Messages = basicResponse.Messages;
            response.ResponseType = basicResponse.ResponseType;
            response.IsAccountCardFound = basicResponse.IsAccountCardFound;
            return response;
        }


    }


}