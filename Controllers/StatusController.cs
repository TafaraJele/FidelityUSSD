//using System;
//using System.Net;
//using System.Web.Http;
//using Indigo.CardReq;
//using Indigo.Models;
//using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
//using Veneka.Module.TranzwareCompassPlusFIMI;
//using Veneka.Module.TranzwareCompassPlusFIMI.Models;
//using System.IO;
//using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Configuration;
//using Indigo.Utility;

//namespace Indigo.Controllers
//{
//    [Serializable]
//    public class StatusController : ApiController
//    {


//        // GET: CardActivating
//        [HttpPost]

//        public List<Status> CardStatus([FromBody] Models.CardStatus cardParameters)
//        {
//            //initialising Response
//            Response resp = null;

//            try
//            {


//                String phoneNumber = cardParameters.phoneNumber;
//                String last4Digits = cardParameters.last4Digits;

//                String token = cardParameters.token;
//                String preferedPAN = "";
//                int preferedMBR = 0;
//                int id = 0;
//                var pan = "";
//                int mbr = 0;


//                // List<Status> returnedCardDetails = new List<Status>();
//                List<Status> returnedCardDetails = new List<Status>()
//                                            {
//                                                new Status(){ Code=0 , Message = ""},

//                                            };

//                //by passing certificates
//                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//                ServicePointManager.ServerCertificateValidationCallback +=
//                    (sender, cert, chain, sslPolicyErrors) => { return true; };

//                var fimilogger = FIMILogger.GetFimiLoggerInstance();

//                fimilogger.Info("**********Calling card status API**********");


//                //Sending to wsdl for card request
//                fimilogger.Info("Calling card Request");

//                CardRequestClient list = new CardRequestClient();
//                fimilogger.Info("Last 4 digits length " + last4Digits.Length);
//                if (phoneNumber != null && last4Digits.Length == 4)
//                {
//                    resp = list.GetCardbyContactNumberExt(phoneNumber, last4Digits, token);
                           



//                    //if (resp.Value != null)
//                    if (resp.Value != null)
//                    {
//                        fimilogger.Info("Indigo ID  using  GetCardbyContactNumberExt " + resp.Value);

//                        preferedPAN = resp.Value;
//                        //preferedMBR = resp.MBR;


//                        //string str = resp.Value;
//                        //string[] uorsList = str.Split(',');
//                        //var idList = uorsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

//                        string username = ConfigurationManager.AppSettings["username"].ToString();
//                        string password = ConfigurationManager.AppSettings["password"].ToString();
//                        string address = ConfigurationManager.AppSettings["address"].ToString();
//                        string path = ConfigurationManager.AppSettings["path"].ToString();
//                        string instName = ConfigurationManager.AppSettings["instName"].ToString();
//                        int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
//                        int timeoutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());


//                        string logger = "";
//                        string sessionKey = "";
//                        string nextChallenge = "";
//                        int sessionId = 0;

//                        FileInfo file = new FileInfo("FIMI");
//                        bool UseCustomEncoder = false;

//                        fimilogger.Info("**********Passing Parameters**********");
//                        fimilogger.Info("phoneNumber : " + phoneNumber);
//                        fimilogger.Info("path :" + path);
//                        fimilogger.Info("timeoutMilliSeconds : " + timeoutMilliSeconds);
//                        fimilogger.Info("username :" + username);
//                        fimilogger.Info("password :" + password);
//                        fimilogger.Info("logger :" + logger);
//                        fimilogger.Info("port :" + port);
//                        fimilogger.Info("address :" + address);
//                        var protocol = ServicesValidated.Protocol.HTTPS;
//                        fimilogger.Info("protocol :" + protocol);
//                        var authentication = ServicesValidated.Authentication.NONE;
//                        fimilogger.Info("authentication :" + authentication);
//                        fimilogger.Info("Service Validated");

//                        fimilogger.Info("Fimi validated");

//                        fimilogger.Info("Initialising service validated");



//                        FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path,
//                            timeoutMilliSeconds, ServicesValidated.Authentication.NONE,
//                            file, username, password, logger, UseCustomEncoder);

//                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
//                        var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);



//                        if (initResponse)
//                        {
//                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession success");
//                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
//                            var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

//                            if (logonResponse)
//                            {
//                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI logon success");
//                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getCard");

//                                GetCardInfo cardInfo = new GetCardInfo();
//                                cardInfo.PAN = preferedPAN;
//                                cardInfo.MBR = preferedMBR;


//                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
//                                var cardinfo = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
//                                if (cardinfo.Count > 0)
//                                {
//                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                    returnedCardDetails = cardinfo;
//                                }
//                                else
//                                {
//                                    GetCardInfo cardInf = new GetCardInfo();
//                                    cardInf.PAN = preferedPAN;
//                                    cardInf.MBR = 1;
//                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI second card request");

//                                    var cardinf = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInf);
//                                    if (cardinfo.Count > 0)
//                                    {
//                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                        returnedCardDetails = cardinfo;
//                                    }
//                                    else
//                                    {

//                                        GetCardInfo cards = new GetCardInfo();
//                                        cardInf.PAN = preferedPAN;
//                                        cardInf.MBR = 2;
//                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI third card request");

//                                        var cardins = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cards);
//                                        if (cardinf.Count > 0)
//                                        {
//                                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                            returnedCardDetails = cardins;
//                                        }
//                                        else
//                                        {
//                                            return returnedCardDetails;
//                                        }



//                                    }


//                                }



//                            }
//                        }
//                    }
//                }

//                return returnedCardDetails;

//            }
//            catch (IndexOutOfRangeException)
//            {


//                return new List<Status>();
//            }
//        }
//    }


//}