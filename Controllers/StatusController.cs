using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Veneka.Module.TranzwareCompassPlusFIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;
using System.IO;
using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Indigo.Utility;
using System.Reflection;
using Veneka.Indigo.Abstractions.Models;
using System.Data.SqlClient;
using System.Data;

namespace Indigo.Controllers
{
    [Serializable]
    public class StatusController : ApiController
    {


        // GET: CardActivating
        //[HttpPost]
        //[Route("GetStatus")]
        //public List<Status> CardStatus([FromBody] Veneka.Indigo.Abstractions.Models.CardStatus cardParameters)
        //{
        //    //initialising Response
        //    Response resp = null;

        //    try
        //    {


        //        String phoneNumber = cardParameters.phoneNumber;
        //        String last4Digits = cardParameters.last4Digits;

        //        String token = cardParameters.token;
        //        String preferedPAN = "";
        //        int preferedMBR = 0;
        //        int id = 0;
        //        var pan = "";
        //        int mbr = 0;


        //        // List<Status> returnedCardDetails = new List<Status>();
        //        List<Status> returnedCardDetails = new List<Status>()
        //                                    {
        //                                        new Status(){ Code=0 , Message = ""},

        //                                    };

        //        //by passing certificates
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        ServicePointManager.ServerCertificateValidationCallback +=
        //            (sender, cert, chain, sslPolicyErrors) => { return true; };

        //        var fimilogger = FIMILogger.GetFimiLoggerInstance();

        //        fimilogger.Info("**********Calling card status API**********");


        //        //Sending to wsdl for card request
        //        fimilogger.Info("Calling card Request");

        //        CardRequestClient list = new CardRequestClient();
        //        fimilogger.Info("Last 4 digits length " + last4Digits.Length);
        //        if (phoneNumber != null && last4Digits.Length == 4)
        //        {
        //            resp = list.GetCardbyContactNumberExt(phoneNumber, last4Digits, token);




        //            //if (resp.Value != null)
        //            if (resp.Value != null)
        //            {
        //                fimilogger.Info("Indigo ID  using  GetCardbyContactNumberExt " + resp.Value);

        //                preferedPAN = resp.Value;
        //                //preferedMBR = resp.MBR;


        //                //string str = resp.Value;
        //                //string[] uorsList = str.Split(',');
        //                //var idList = uorsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        //                string username = ConfigurationManager.AppSettings["username"].ToString();
        //                string password = ConfigurationManager.AppSettings["password"].ToString();
        //                string address = ConfigurationManager.AppSettings["address"].ToString();
        //                string path = ConfigurationManager.AppSettings["path"].ToString();
        //                string instName = ConfigurationManager.AppSettings["instName"].ToString();
        //                int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
        //                int timeoutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());


        //                string logger = "";
        //                string sessionKey = "";
        //                string nextChallenge = "";
        //                int sessionId = 0;

        //                FileInfo file = new FileInfo("FIMI");
        //                bool UseCustomEncoder = false;

        //                fimilogger.Info("**********Passing Parameters**********");
        //                fimilogger.Info("phoneNumber : " + phoneNumber);
        //                fimilogger.Info("path :" + path);
        //                fimilogger.Info("timeoutMilliSeconds : " + timeoutMilliSeconds);
        //                fimilogger.Info("username :" + username);
        //                fimilogger.Info("password :" + password);
        //                fimilogger.Info("logger :" + logger);
        //                fimilogger.Info("port :" + port);
        //                fimilogger.Info("address :" + address);
        //                var protocol = ServicesValidated.Protocol.HTTPS;
        //                fimilogger.Info("protocol :" + protocol);
        //                var authentication = ServicesValidated.Authentication.NONE;
        //                fimilogger.Info("authentication :" + authentication);
        //                fimilogger.Info("Service Validated");

        //                fimilogger.Info("Fimi validated");

        //                fimilogger.Info("Initialising service validated");



        //                FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path,
        //                    timeoutMilliSeconds, ServicesValidated.Authentication.NONE,
        //                    file, username, password, logger, UseCustomEncoder);

        //                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
        //                var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);



        //                if (initResponse)
        //                {
        //                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession success");
        //                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
        //                    var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

        //                    if (logonResponse)
        //                    {
        //                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI logon success");
        //                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getCard");

        //                        GetCardInfo cardInfo = new GetCardInfo();
        //                        cardInfo.PAN = preferedPAN;
        //                        cardInfo.MBR = preferedMBR;


        //                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
        //                        var cardinfo = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
        //                        if (cardinfo.Count > 0)
        //                        {
        //                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
        //                            returnedCardDetails = cardinfo;
        //                        }
        //                        else
        //                        {
        //                            GetCardInfo cardInf = new GetCardInfo();
        //                            cardInf.PAN = preferedPAN;
        //                            cardInf.MBR = 1;
        //                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI second card request");

        //                            var cardinf = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInf);
        //                            if (cardinfo.Count > 0)
        //                            {
        //                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
        //                                returnedCardDetails = cardinfo;
        //                            }
        //                            else
        //                            {

        //                                GetCardInfo cards = new GetCardInfo();
        //                                cardInf.PAN = preferedPAN;
        //                                cardInf.MBR = 2;
        //                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI third card request");

        //                                var cardins = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cards);
        //                                if (cardinf.Count > 0)
        //                                {
        //                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
        //                                    returnedCardDetails = cardins;
        //                                }
        //                                else
        //                                {
        //                                    return returnedCardDetails;
        //                                }



        //                            }


        //                        }



        //                    }
        //                }
        //            }
        //        }

        //        return returnedCardDetails;

        //    }
        //    catch (IndexOutOfRangeException)
        //    {


        //        return new List<Status>();
        //    }
        //}

        // GET: CardActivating
        [HttpPost]
        [Route("api/get-card-status/account-number")]
        public List<Status> CardStatus([FromBody] Veneka.Indigo.Abstractions.Models.AccountCardStatus cardParameters)
        {
            //initialising Response
            Response resp = null;

            try
            {
                String preferedPAN = "";
                int preferedMBR = 0;
                int id = 0;
                var pan = "";
                int mbr = 0;


                // List<Status> returnedCardDetails = new List<Status>();
                List<Status> returnedCardDetails = new List<Status>()
                                            {
                                                new Status(){ Code=0 , Message = ""},

                                            };

                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var fimilogger = FIMILogger.GetFimiLoggerInstance();

                fimilogger.Info("**********Calling card status API**********");


                //Sending to wsdl for card request
                fimilogger.Info("Calling card Request");

                CardRequestClient list = new CardRequestClient();
                fimilogger.Info("Last 4 digits length " + cardParameters.last4Digits.Length);
                if (cardParameters.accountNumber != null && cardParameters.last4Digits.Length == 4)
                {
                    resp = list.GetCardsByPrepaidAccount(cardParameters.accountNumber, cardParameters.last4Digits, cardParameters.token);

                    //if (resp.Value != null)
                    if (resp.Value != null)
                    {
                        fimilogger.Info("Indigo ID  using  GetCardsByPrepaidAccount " + resp.Value);

                        preferedPAN = resp.Value;
                        //preferedMBR = resp.MBR;


                        //string str = resp.Value;
                        //string[] uorsList = str.Split(',');
                        //var idList = uorsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                        string username = ConfigurationManager.AppSettings["username"].ToString();
                        string password = ConfigurationManager.AppSettings["password"].ToString();
                        string address = ConfigurationManager.AppSettings["address"].ToString();
                        string path = ConfigurationManager.AppSettings["path"].ToString();
                        string instName = ConfigurationManager.AppSettings["instName"].ToString();
                        int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
                        int timeoutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());


                        string logger = "";
                        string sessionKey = "";
                        string nextChallenge = "";
                        int sessionId = 0;

                        FileInfo file = new FileInfo("FIMI");
                        bool UseCustomEncoder = false;

                        fimilogger.Info("**********Passing Parameters**********");
                        fimilogger.Info("accountNumber : " + cardParameters.accountNumber);
                        fimilogger.Info("path :" + path);
                        fimilogger.Info("timeoutMilliSeconds : " + timeoutMilliSeconds);
                        fimilogger.Info("username :" + username);
                        fimilogger.Info("password :" + password);
                        fimilogger.Info("logger :" + logger);
                        fimilogger.Info("port :" + port);
                        fimilogger.Info("address :" + address);
                        var protocol = ServicesValidated.Protocol.HTTPS;
                        fimilogger.Info("protocol :" + protocol);
                        var authentication = ServicesValidated.Authentication.NONE;
                        fimilogger.Info("authentication :" + authentication);
                        fimilogger.Info("Service Validated");

                        fimilogger.Info("Fimi validated");

                        fimilogger.Info("Initialising service validated");



                        FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path,
                            timeoutMilliSeconds, ServicesValidated.Authentication.NONE,
                            file, username, password, logger, UseCustomEncoder);

                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
                        var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);



                        if (initResponse)
                        {
                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession success");
                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
                            var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

                            if (logonResponse)
                            {
                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI logon success");
                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getCard");

                                GetCardInfo cardInfo = new GetCardInfo();
                                cardInfo.PAN = preferedPAN;
                                cardInfo.MBR = preferedMBR;


                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
                                var cardinfo = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
                                if (cardinfo.Count > 0)
                                {
                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
                                    returnedCardDetails = cardinfo;
                                }
                                else
                                {
                                    GetCardInfo cardInf = new GetCardInfo();
                                    cardInf.PAN = preferedPAN;
                                    cardInf.MBR = 1;
                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI second card request");

                                    var cardinf = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInf);
                                    if (cardinfo.Count > 0)
                                    {
                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
                                        returnedCardDetails = cardinfo;
                                    }
                                    else
                                    {

                                        GetCardInfo cards = new GetCardInfo();
                                        cardInf.PAN = preferedPAN;
                                        cardInf.MBR = 2;
                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI third card request");

                                        var cardins = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cards);
                                        if (cardinf.Count > 0)
                                        {
                                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
                                            returnedCardDetails = cardins;
                                        }
                                        else
                                        {
                                            return returnedCardDetails;
                                        }

                                    }
                                }

                            }
                        }
                    }
                }

                return returnedCardDetails;

            }
            catch (IndexOutOfRangeException)
            {


                return new List<Status>();
            }
        }


        //blockcard
        [HttpPost]
        [Route("api/block-card/account-number")]

        public List<BlockCardResponse> BlockCard([FromBody] Veneka.Indigo.Abstractions.Models.CardStatusDTO cardParameters)
        {
            //initialising Response
            Response resp = null;

            try
            {

                List<Int32> lis = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 12, 15 };
                if (!lis.Contains(cardParameters.status))
                {
                    //...

                    return new List<BlockCardResponse>()
                        {
                            new BlockCardResponse(){
                            Code = 404,
                            Message ="Card status invalid",
                            CardStatus  = cardParameters.status,
                            }

                        };
                }

                //Mapp status Fidelity request
                cardParameters.status = StatusIDRequestMapping(cardParameters.status);

                // List<Status> returnedCardDetails = new List<Status>();
                List<BlockCardResponse> returnedCardDetails = new List<BlockCardResponse>()
                {
                    //new Status(){
                    //    Message = "Failed to process",
                    //    Code = 200

                    //},

                };

                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var fimilogger = FIMILogger.GetFimiLoggerInstance();

                fimilogger.Info("**********Calling BlockCard**********");


                //Sending to wsdl for card request
                fimilogger.Info("Calling card Request");

                CardRequestClient list = new CardRequestClient();
                fimilogger.Info("Last 4 digits length " + cardParameters.last4Digits.Length);
                if (cardParameters.accountNumber != null && cardParameters.last4Digits.Length == 4)
                {


                    resp = list.GetCardsByPrepaidAccount(cardParameters.accountNumber, cardParameters.last4Digits, cardParameters.token);



                    if (resp.Value == null || resp.Value == "")
                    {

                        return new List<BlockCardResponse>()
                        {
                            new BlockCardResponse(){
                            Code = 404,
                            Message ="Card not found in Indigo or Token expired"

                            }

                        };


                    }


                    fimilogger.Info("Indigo Response " + resp.Value);

                    string indigoPAN = resp.Value;

                    string username = ConfigurationManager.AppSettings["username"].ToString();
                    string password = ConfigurationManager.AppSettings["password"].ToString();
                    string address = ConfigurationManager.AppSettings["address"].ToString();
                    string path = ConfigurationManager.AppSettings["path"].ToString();
                    string instName = ConfigurationManager.AppSettings["instName"].ToString();
                    int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
                    int timeoutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());
                    string indigoConnectionString = ConfigurationManager.ConnectionStrings["IndigoConnectionString"].ConnectionString;

                    string logger = "";
                    string sessionKey = "";
                    string nextChallenge = "";
                    int sessionId = 0;
                    var protocol = ServicesValidated.Protocol.HTTPS;
                    var authentication = ServicesValidated.Authentication.NONE;
                    FileInfo file = new FileInfo("FIMI");
                    bool UseCustomEncoder = false;

                    fimilogger.Info("**********Passing Parameters**********");
                    fimilogger.Info("Account number : " + cardParameters.accountNumber + " \n path :" + path + "\n timeoutMilliSeconds : " + timeoutMilliSeconds);
                    fimilogger.Info($"username : {username} \n password : {password} \n logger : {logger} \n port : {port} \n address :{address} \n protocol :{protocol}\n authentication : authentication");

                    fimilogger.Info("Initialising service validated");

                    FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, address, port, path,
                        timeoutMilliSeconds, ServicesValidated.Authentication.NONE,
                        file, username, password, logger, UseCustomEncoder);

                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
                    var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);

                    if (initResponse)
                    {
                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession success");
                       //fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon");
                        var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

                        if (logonResponse)
                        {
                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI logon success");

                            CardBlockRequest cardInfo = new CardBlockRequest();

                            cardInfo.PAN = indigoPAN;
                            cardInfo.cardStatus = cardParameters.status;

                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card");
                            //var cardinfo = serviceValidated.GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);

                            var cardinfo = serviceValidated.SetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
                            //GetCardStatus(sessionId, sessionKey, ref nextChallenge, cardInfo);
                            if (cardinfo)
                            {
                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI card success");
                                List<BlockCardResponse> returnedCard = new List<BlockCardResponse>()
                                            {
                                                new BlockCardResponse(){
                                                    Message = "Card status altered",
                                                    CardStatus  = cardParameters.status,
                                                    Code = 200

                                                },
                                            };
                                //CMS card status updated block card in Indigo
                                fimilogger.Info($"Card status update in CMS successful. Update Indigo card status indigoConnectionString {indigoConnectionString}");
                                var isStatusUpdated = BlockCardInIndigoByPAN(indigoPAN, indigoConnectionString);

                                return returnedCard;
                            }

                        }
                    }

                }

                return returnedCardDetails;

            }
            catch (Exception e)
            {

                return new List<BlockCardResponse>() {
                                                new BlockCardResponse(){
                                                    Message = e.Message,
                                                    Code = 400

                                                }, };
            }
        }
        //Fidelity request to map stolen 3 and lost 2 status to not active 0 and closed 9
        //Reference Tawedzerwa
        private int StatusIDRequestMapping(int status)
        {
            if (status == 3)
            {
                return 9;
            }
            else if (status == 2)
            {
                return 0;
            }
            else
            {
                return status;
            }

        }


        private bool BlockCardInIndigoByPAN(string cardNumber, string connectionString)
        {
            var fimilogger = FIMILogger.GetFimiLoggerInstance();          

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("usp_update_card_status", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Define input parameters
                    command.Parameters.Add(new SqlParameter("@card_number", SqlDbType.VarChar, 50)).Value = cardNumber;
                    command.Parameters.Add(new SqlParameter("@new_status_id", SqlDbType.Int)).Value = 20; // Replace with your desired status ID
                    command.Parameters.Add(new SqlParameter("@audit_user_id", SqlDbType.BigInt)).Value = -1; // Replace with your audit user ID
                    command.Parameters.Add(new SqlParameter("@audit_workstation", SqlDbType.VarChar, 100)).Value = "System requests";

                    // Define the output parameter
                    SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int);
                    resultCodeParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(resultCodeParam);

                    try
                    {
                        command.ExecuteNonQuery();

                        // Retrieve the output parameter value
                        int resultCode = (int)command.Parameters["@ResultCode"].Value;
                        if (resultCode != 0)
                        {
                            fimilogger.Debug($"Card status update failed in Indigo result code {resultCode}");
                            return false;
                        }

                    }
                    catch (SqlException ex)
                    {
                        fimilogger.Debug("SQL Error: " + ex.Message);
                    }
                }

                return true;
            }
        }

    }
}