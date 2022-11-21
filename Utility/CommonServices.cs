using Indigo.CardReq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using Veneka.Indigo.Abstractions.Enums;
using Veneka.Indigo.Abstractions.Models;


namespace Indigo.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ApplicationSettings GetSettings()
        {
            ApplicationSettings settings = new ApplicationSettings();
            settings.UserName = ConfigurationManager.AppSettings["username"].ToString();
            settings.Password = ConfigurationManager.AppSettings["password"].ToString();
            settings.Address = ConfigurationManager.AppSettings["address"].ToString();
            settings.Path = ConfigurationManager.AppSettings["path"].ToString();
            settings.InstName = ConfigurationManager.AppSettings["instName"].ToString(); 
            settings.Port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
            settings.TimeOutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());
            settings.EndpointConfigurationName = ConfigurationManager.AppSettings["EndpointConfigurationName"].ToString();
            settings.IndigoAddress = ConfigurationManager.AppSettings["IndigoAddress"].ToString();
            settings.CIFConnectionString = ConfigurationManager.ConnectionStrings["CIFConnectionString"].ConnectionString;

            return settings;
        }
        /// <summary>
        /// 
        /// </summary>
        public BasicResponse GetAccountCardInIndigo(AccountCard request, BasicResponse basicResponse, out string cardnumber)
        {
            var fimilogger = FIMILogger.GetFimiLoggerInstance();

            cardnumber = string.Empty;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => { return true; };

            CardRequestClient IndigoClient = new CardRequestClient();
            fimilogger.Info($"Calling Indigo get card by prepaid account endpoint configuration name URL {IndigoClient.Endpoint.Address} ");
            var response = IndigoClient.GetCardsByPrepaidAccount(request.CardLastFourDigits, request.AccountNumber, request.Token);
            //Sending to wsdl for card request
            fimilogger.Info($"Logging Response for get card by prepaid account CARD NUMBER : {response.Value}");

            if (response.ResponseType == ResponseCode._00)
            {
                basicResponse.ResponseType = ResponseType.SUCCESS;
                if (string.IsNullOrEmpty(response.Value))
                {
                    fimilogger.Info($"Card was not found");
                    basicResponse.IsAccountCardFound = false;
                    basicResponse.Messages.Add(new Messages { Code = "99", Message = $"Card for the account{request.AccountNumber} and last four digits {request.CardLastFourDigits} was not found" });
                }
                else
                {
                    fimilogger.Info($"Card was found");
                    basicResponse.IsAccountCardFound = true;
                    cardnumber = response.Value;
                    fimilogger.Info($"Logging basic response {basicResponse.ResponseType} {basicResponse.IsAccountCardFound}");
                }

            }
            else
            {
                fimilogger.Info($"There was an error requesting the card from Indigo Message {response.ResponseMessage}, ");
                basicResponse.ResponseType = ResponseType.ERROR;
                basicResponse.Messages.Add(new Messages { Code = "99", MessageType = MessageType.ERROR, Message = response.ResponseMessage, Exception = response.ResponseException });
            }

            return basicResponse;
        }

    }
}