//using System;
//using System.Net;
//using System.Web.Http;
//using Indigo.CardReq;
//using Indigo.Models;
//using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
//using Veneka.Module.TranzwareCompassPlusFIMI;
//using System.IO;
//using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
//using System.Linq;
//using System.Text.RegularExpressions;
//using Newtonsoft.Json.Linq;
//using System.Configuration;
//using Veneka.Module.TranzwareCompassPlusFIMI.Models;
//using Indigo.Utility;

//namespace Indigo.Controllers
//{
//    [Serializable]
//    public class FundsLoadController : ApiController
//    {


//        // GET: CardActivating
//        [HttpPost]

//        public JObject FundsLoad([FromBody] FundsLoad funds)
//        {
//            //initialising Response
//            Response resp = null;

//            try
//            {

//                string obj =
//                           @"{
//                          Message: '',
//                          Code:0
                         
//                        }";
//                JObject accRequest = JObject.Parse(obj);

//                String phoneNumber = funds.phoneNumber;
//                String last4Digits = funds.last4Digits;

//                Decimal amount = funds.amount;
//                String token = funds.token;
//                int id = 0;
//                var pan = "";
//                int mbr = 0;

//                string preferedPAN = "";
//                int preferedMBR = 0;


//                var fimilogger = FIMILogger.GetFimiLoggerInstance();

//                fimilogger.Info("************Calling Funds API**********");
//                //by passing certificates
//                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//                ServicePointManager.ServerCertificateValidationCallback +=
//                    (sender, cert, chain, sslPolicyErrors) => { return true; };


//                //Sending to wsdl for card request

//                CardRequestClient list = new CardRequestClient();
//                fimilogger.Info("Calling Card Request");
//                fimilogger.Info("Last 4 digits length " + last4Digits.Length);

//                if (phoneNumber != null && last4Digits.Length == 4 && amount > 0)
//                {
//                    //resp = list.GetCardbyContactNumber(phoneNumber,token);

//                    //resp = list.GetCardbyContactNumberExt(phoneNumber, token, last4Digits);
//                    resp = list.GetCardbyContactNumberExt(phoneNumber, last4Digits, token);

//                    if (resp.Value != null)
//                    {
//                        fimilogger.Info("RESPONSE " + resp.Value);

//                        preferedPAN = resp.Value;
//                        //preferedMBR = resp.MBR;
//                        //string str = resp.Value;
//                        //string[] uorsList = str.Split(',');
//                        //var idList = uorsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();


//                        fimilogger.Info("Calling Card Request success " + resp.Value);

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
//                        FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path, timeoutMilliSeconds, ServicesValidated.Authentication.NONE, file, username, password, logger, UseCustomEncoder);


//                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
//                        var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);

//                        if (initResponse)
//                        {

//                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
//                            var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

//                            if (logonResponse)
//                            {

//                                GetCardInfo cardInfo = new GetCardInfo();
//                                cardInfo.PAN = preferedPAN;
//                                cardInfo.MBR = preferedMBR;




//                                //call card information
//                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
//                                var cardinfo = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardInfo);

//                                if (cardinfo.Count > 0)
//                                {


//                                    //call account credit
//                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                    var cardA = cardinfo.Select(x => x.AcctNo).ToList();

//                                    string AcctNo = cardA[0];
//                                    AcctCredit acc = new AcctCredit();
//                                    acc.Account = AcctNo;
//                                    acc.Amount = amount;
//                                    acc.IgnoreImpact = 1;

//                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transaction information");
//                                    accRequest = serviceValidated.CreditPrepaidAccount(sessionId, sessionKey, ref nextChallenge, acc);



//                                }

//                                else
//                                {

//                                    GetCardInfo cardInf = new GetCardInfo();
//                                    cardInfo.PAN = preferedPAN;
//                                    cardInfo.MBR = 1;

//                                    //call card information
//                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Second card Request ");
//                                    var cardinf = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardInf);

//                                    if (cardinf.Count > 0)
//                                    {
//                                        //make a call
//                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                        var cardA = cardinfo.Select(x => x.AcctNo).ToList();

//                                        string AcctNo = cardA[0];
//                                        AcctCredit acc = new AcctCredit();
//                                        acc.Account = AcctNo;
//                                        acc.Amount = amount;
//                                        acc.IgnoreImpact = 1;

//                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transaction information");
//                                        accRequest = serviceValidated.CreditPrepaidAccount(sessionId, sessionKey, ref nextChallenge, acc);
//                                    }

//                                    else
//                                    {
//                                        GetCardInfo cardIn = new GetCardInfo();
//                                        cardInfo.PAN = preferedPAN;
//                                        cardInfo.MBR = 2;

//                                        //call card information
//                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI third card Request ");
//                                        var cardin = serviceValidated.GetCardInfo(sessionId, sessionKey, ref nextChallenge, cardIn);


//                                        if (cardin.Count > 0)
//                                        {
//                                            //make a call
//                                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
//                                            var cardA = cardinfo.Select(x => x.AcctNo).ToList();

//                                            string AcctNo = cardA[0];
//                                            AcctCredit acc = new AcctCredit();
//                                            acc.Account = AcctNo;
//                                            acc.Amount = amount;
//                                            acc.IgnoreImpact = 1;

//                                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transaction information");
//                                            accRequest = serviceValidated.CreditPrepaidAccount(sessionId, sessionKey, ref nextChallenge, acc);
//                                        }
//                                        else
//                                        {
//                                            return accRequest;
//                                        }



//                                    }

//                                }
//                            }
//                        }
//                    }


//                }




//                return accRequest;

//            }
//            catch (IndexOutOfRangeException)
//            {


//                return (JObject)"Exception";
//            }
//        }
//    }


//}