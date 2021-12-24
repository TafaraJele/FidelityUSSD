//using System;
//using System.Net;
//using System.Web.Http;
//using Indigo.CardReq;
//using Indigo.Models;
//using Newtonsoft.Json.Linq;
//using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
//using System.Web.Http.Cors;
//using Veneka.Module.TranzwareCompassPlusFIMI;
//using Veneka.Module.TranzwareCompassPlusFIMI.Models;
//using System.IO;
//using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
//using System.Collections.Generic;
//using System.Configuration;
//using Indigo.Utility;
//using System.Linq;
//using System.Web.Script.Serialization;


//namespace Indigo.Controllers
//{
//    [Serializable]
//    public class getCardListController : ApiController
//    {
//        [HttpPost]
       
//        public List<List> CardList([FromBody] CardList cardli)
//        {
//            //initialising Response
//            Response resp = null;

//            try
//            {
//                String phoneNumber = cardli.phoneNumber;
//                String token = cardli.token;
//                String preferedPAN = "";
//                int preferedMBR = 0;
//                int id = 0;
//                var pan = "";
//                int mbr = 0;


//                // List<Status> returnedCardDetails = new List<Status>();
//                List<List> returnedCardDetails = new List<List>();
                                            

//                //by passing certificates
//                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//                ServicePointManager.ServerCertificateValidationCallback +=
//                    (sender, cert, chain, sslPolicyErrors) => { return true; };

//                var fimilogger = FIMILogger.GetFimiLoggerInstance();

//                fimilogger.Info("**********Calling card status API**********");


//                //Sending to wsdl for card request
//                fimilogger.Info("Calling card Request");

//                CardRequestClient list = new CardRequestClient();

//                if (phoneNumber != null)
//                {
//                    // resp = list.GetCardbyContactNumber(phoneNumber, token);

//                    resp = list.GetCardListbyContactNumberExt(phoneNumber, token);



//                    //if (resp.Value != null)
//                    if (resp.Value != null)
//                    {
//                        fimilogger.Info("Indigo ID " + resp.Value);

//                        string str = resp.Value;
//                        string[] uorsList = str.Split(',');
//                        var idList = uorsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

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



//                        FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path, timeoutMilliSeconds, ServicesValidated.Authentication.NONE, file, username, password, logger, UseCustomEncoder);
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
//                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getPerson");

//                                //start of foreach
//                                foreach (string m in idList)
//                                { 

//                                    //GetPersonInfo personInfo = new GetPersonInfo();
//                                    //personInfo.Id = int.Parse(m);
//                                    //personInfo.InstName = instName;

//                                    //fimilogger.Info("getperson ID " + personInfo.Id);
//                                    //fimilogger.Info("getperson InstName " + personInfo.InstName);

//                                    //var cards = serviceValidated.GetPersonListCard(id, instName, sessionId, sessionKey, ref nextChallenge, personInfo);

//                                    //if (cards.Count > 0)
//                                    //{
//                                    //    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI person success");


//                                        //foreach (var car in cards)
//                                        //{
//                                            pan = Mask(m);
//                                            //mbr = car.MBR;

//                                            fimilogger.Info("CardNumber "+pan);
                                         
//                                            //GetCardInfo cardInfo = new GetCardInfo


//                                            //cardInfo.PAN = pan;
//                                            //cardInfo.MBR = mbr;

//                                            //fimilogger.Info("Pan "+ pan);


//                                            //fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
//                                            //var cardinfo = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
//                                           // var cardinfo = serviceValidated.GetCardInfoList(sessionId, sessionKey, ref nextChallenge, cardInfo);
//                                           // if (cardinfo.Count > 0)
//                                           // {
//                                                //foreach (var cu in cardinfo)
//                                                //{

//                                                    List cardmas = new List
//                                                    {
//                                                        CardNumber = pan,
//                                                        Code = 200,
//                                                        Message = "Success"
//                                                    };
//                                                    returnedCardDetails.Add(cardmas);
//                                               // }

//                                                // returnedCardDetails.Add(cardinfo);
//                                            //}



//                                        //}
//                                   // }
//                                }
//                            }
//                        }
//                    }
//                }

//                return returnedCardDetails;
//            }
//            catch (IndexOutOfRangeException)
//            {

//                return new List<List>();
//            }
//        }

//        public static string Mask(string CardPan)
//        {
//            string Mask = "";

//            var maskedPan = CardPan.Aggregate(string.Empty, (value, next) =>
//            {
//                if (value.Length >= 6 && value.Length < CardPan.Length - 4)
//                {
//                    next = '*';
//                }

//                Mask = value + next;
//                return Mask;
//            });
//            return Mask;
//        }
//    }

 
//}