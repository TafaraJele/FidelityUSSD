using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using Indigo.Models;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using System.Web.Http.Cors;
using Veneka.Module.TranzwareCompassPlusFIMI;
using System.IO;
using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Indigo.Utility;
using Indigo.IndigoWebService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Indigo.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class getCardBalanceController : ApiController
    {
        private string UserName;
        private string Password;
        private string Address;
        private string Path;
        private string InstName;
        private int Port;
        private int TimeOutMilliSeconds;
        private string PAN;
        private string EndpointConfigurationName;
        private string IndigoAddress;

        [HttpPost]


        /*
            1) Receive mobile number
            2) Check reference from mobile  - Indigo
            3) Call intisession - NI
            4) Call Logon - NI
            5) get Personifo
            6) get cardinfo    

            */

        public IActionResult BalanceEnquiry([System.Web.Http.FromBody] BalanceEnquiry cardParameters)
        {
            //initialising Response
            Response resp = null;
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            var requestData = JsonConvert.SerializeObject(cardParameters);
            fimilogger.Info($"Logging request deatils {requestData}");

            try
            {
                //List<CardDetails> cardinfoReturned = null;

                List<Veneka.Module.TranzwareCompassPlusFIMI.Models.CardDetails> cardinfoReturned = new List<Veneka.Module.TranzwareCompassPlusFIMI.Models.CardDetails>();

                int id = 0;
                string preferedPAN = "";
                int preferedMBR = 0;
                var pan = "";
                int mbr = 0;


                fimilogger.Info("**********Calling Balance API**********");
                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                GetSettings();
                //Sending to wsdl for card request

                CardRequestClient list = new CardRequestClient();
                fimilogger.Info("logging card request in indigo");

                //fimilogger.Info($"Calling Indigo get card by prepaid account endpoint configuration name{EndpointConfigurationName}, {IndigoAddress}");
                //Service1SoapClient service = new Service1SoapClient();                
                //var response = service.GetCardsByPrepaidAccount(cardParameters.CardLast4FourDigits, cardParameters.AccountNumber, cardParameters.Token);

                CardRequestClient IndigoClient = new CardRequestClient();
                fimilogger.Info($"Calling Indigo get card by prepaid account endpoint configuration name");
                var response = IndigoClient.GetCardsByPrepaidAccount(cardParameters.CardLast4FourDigits, cardParameters.AccountNumber, cardParameters.Token);
                //Sending to wsdl for card request
                fimilogger.Info($"Logging Response for get card by prepaid account CARD NUMBER : {response.Value}");
                var responseData = JsonConvert.SerializeObject(response);
                fimilogger.Info("Get card by prepaid account number response " + responseData);

                if (!string.IsNullOrEmpty(response.Value))
                {
                    preferedPAN = response.Value;
                    // preferedMBR = resp.MBR;

                    string logger = "";
                    string sessionKey = "";
                    string nextChallenge = "";
                    int sessionId = 0;

                    FileInfo file = new FileInfo("FIMI");
                    bool UseCustomEncoder = false;


                    fimilogger.Info("**********Passing Parameters**********");
                    fimilogger.Info("account number : " + cardParameters.AccountNumber);
                    fimilogger.Info("path :" + Path);
                    fimilogger.Info("timeoutMilliSeconds : " + TimeOutMilliSeconds);
                    fimilogger.Info("username :" + UserName);
                    fimilogger.Info("password :" + Password);
                    fimilogger.Info("logger :" + logger);
                    fimilogger.Info("port :" + Port);
                    fimilogger.Info("address :" + Address);
                    var protocol = ServicesValidated.Protocol.HTTPS;
                    fimilogger.Info("protocol :" + protocol);
                    var authentication = ServicesValidated.Authentication.NONE;
                    fimilogger.Info("authentication :" + authentication);
                    fimilogger.Info("Service Validated");

                    fimilogger.Info("Fimi validated");

                    FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, Address, Port, Path, TimeOutMilliSeconds, ServicesValidated.Authentication.NONE, file, UserName, Password, logger, UseCustomEncoder);

                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
                    var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);

                    if (initResponse)
                    {
                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
                        var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

                        if (logonResponse)
                        {

                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI logon success");
                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getPerson");

                            GetCardInfo cardInfo = new GetCardInfo();
                            cardInfo.PAN = preferedPAN;
                            cardInfo.MBR = preferedMBR;

                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card information");
                            var cardinfo = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardInfo);

                            if (cardinfo.Count > 0)
                            {
                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");

                                cardinfoReturned = cardinfo;
                            }
                            else
                            {

                                GetCardInfo cardInf = new GetCardInfo();
                                cardInf.PAN = preferedPAN;
                                cardInf.MBR = 1;


                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card information");
                                var cardinf = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardInf);
                                if (cardinf.Count > 0)
                                {
                                    GetCardInfo cardIn = new GetCardInfo();
                                    cardIn.PAN = preferedPAN;
                                    cardIn.MBR = 2;

                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card information");
                                    var cardin = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardIn);
                                    cardinfoReturned = cardin;
                                }
                                else
                                {
                                    return (IActionResult)Ok(cardinfoReturned);

                                }
                            }

                        }
                    }
                }
                else
                {
                    fimilogger.Info($"no cards found for details {requestData}");
                    return (IActionResult)NotFound();
                }

                return (IActionResult)Ok(cardinfoReturned);


            }
            catch (Exception exec)
            {
                fimilogger.Info($"Logging error exception {exec}");
                return (IActionResult)BadRequest(exec.Message);
            }
        }

        private void GetSettings()
        {
            UserName = ConfigurationManager.AppSettings["username"].ToString();
            Password = ConfigurationManager.AppSettings["password"].ToString();
            Address = ConfigurationManager.AppSettings["address"].ToString();
            Path = ConfigurationManager.AppSettings["path"].ToString();
            InstName = ConfigurationManager.AppSettings["instName"].ToString();
            Port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
            TimeOutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());
            EndpointConfigurationName = ConfigurationManager.AppSettings["EndpointConfigurationName"].ToString();
            IndigoAddress = ConfigurationManager.AppSettings["IndigoAddress"].ToString();
        }
    }


}