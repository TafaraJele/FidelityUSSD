using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using System.Configuration;
using Indigo.Utility;
using Veneka.Indigo.Abstractions.Enums;
using Veneka.Indigo.Abstractions.Models;
using Veneka.Indigo.Core;

namespace Indigo.Controllers
{
    /// <summary>
    /// Queries card statements 
    /// </summary>
    [Serializable]
    public class StatementsController : ApiController
    {

        Application application = new Application();
        CommonServices services = new CommonServices();
        /// <summary>
        /// Returns a statement given the statement request details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]

        public StatementResponse GetStatement([FromBody] AccountCard request)
        {
            //Logger
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            fimilogger.Info("***********Get statement controller method*************");
            var response = new StatementResponse();
            var basicResponse = new BasicResponse();
            //get application settings
            var settings = services.GetSettings();
            response = application.GetCardStatement(request, "4890332376859510", settings, response);
            //validate statement request
            basicResponse = application.ValidateAccountCard(request);

            response = UpdateStatementResponse(response, basicResponse);

            if (!response.IsRequestValid)
            {
                return response;
            }
            //get card from indigo
            basicResponse = services.GetAccountCardInIndigo(request, basicResponse, out string cardnumber);
            response = UpdateStatementResponse(response, basicResponse);

            fimilogger.Info($"Logging basic response {basicResponse.ResponseType} {basicResponse.IsAccountCardFound}");
        
            if (basicResponse.ResponseType != ResponseType.SUCCESS || !basicResponse.IsAccountCardFound)
            {
                return response;
            }
           

            //get statement from FIMI by card number
            response = application.GetCardStatement(request, cardnumber, settings, response);

            return response;

        }
        

        private StatementResponse UpdateStatementResponse(StatementResponse response, BasicResponse basicResponse)
        {
            var fimilogger = FIMILogger.GetFimiLoggerInstance();

            response.IsRequestValid = basicResponse.IsRequestValid;
            response.Messages = basicResponse.Messages;
            response.ResponseType = basicResponse.ResponseType;
            response.IsAccountCardFound = basicResponse.IsAccountCardFound;

            return response;
        }


    }
}