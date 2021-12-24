using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using Indigo.Models;
using Newtonsoft.Json.Linq;
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

namespace Indigo.Controllers
{
    /// <summary>
    /// Queries card statements 
    /// </summary>
    [Serializable]
    public class StatementsController : ApiController
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
        /// <summary>
        /// Returns a statement given the statement request details
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost]

        public List<Transactions> GetStatement([FromBody] StatementRequest account)
        {
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            //initialising Response
            Response resp = null;

            try
            {
               //get applicaton settings
                GetSettings();
                string logger = "";
                string sessionKey = "";
                string nextChallenge = "";
                int sessionId = 0;
                int id = 0;

                int preferedMBR = 0;


                int mbr = 0;

                var requestData = JsonConvert.SerializeObject(account);
                fimilogger.Info("Logging request data :" + requestData);


                bool UseCustomEncoder = false;
                fimilogger.Info("UseCustomEncoder :" + UseCustomEncoder);
                var protocol = ServicesValidated.Protocol.HTTPS;
                fimilogger.Info("protocol :" + protocol);
                var authentication = ServicesValidated.Authentication.NONE;
                fimilogger.Info("authentication :" + authentication);
                FileInfo file = new FileInfo("FIMI");


                fimilogger.Info("Calling get statement API");


                List<string> PanList = new List<string>();
                List<Transactions> returnedTransactions = new List<Transactions>();


                //by passing certificates
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };


                CardRequestClient IndigoClient = new CardRequestClient();
                fimilogger.Info($"Calling Indigo get card by prepaid account endpoint configuration name");
                var response = IndigoClient.GetCardsByPrepaidAccount(account.CardLastFourDigits, account.AccountNumber, account.Token);
                //Sending to wsdl for card request
                fimilogger.Info($"Logging Response for get card by prepaid account CARD NUMBER : {response.Value}");
                //Service1SoapClient service = new Service1SoapClient();

                //var response = service.GetCardsByPrepaidAccount(account.CardLastFourDigits, account.AccountNumber, account.Token);
                //fimilogger.Info($"response ....{response}");


                if (!string.IsNullOrEmpty(response.Value))
                {
                    //var responseData = JsonConvert.SerializeObject(response.Value.FirstOrDefault());

                    PAN = response.Value;

                    fimilogger.Info("Calling fimi validated");
                    FIMIServicesValidated serviceValidated = new FIMIServicesValidated(ServicesValidated.Protocol.HTTPS, Address, Port, Path, TimeOutMilliSeconds, ServicesValidated.Authentication.NONE, file, UserName, Password, logger, UseCustomEncoder);

                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI InitSession");
                    var initResponse = serviceValidated.InitSession(out sessionId, out sessionKey, out nextChallenge);


                    if (initResponse)
                    {
                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI Logon ");
                        var logonResponse = serviceValidated.Logon(sessionId, sessionKey, ref nextChallenge);

                        if (logonResponse)
                        {

                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI getstatement");

                            //end for each
                            getTransInfo TransInfo = new getTransInfo();
                            TransInfo.MBR = mbr;
                            TransInfo.PAN = PAN;
                            TransInfo.ToTime = account.ToTime;
                            TransInfo.FromTime = account.FromTime;

                            fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo First attempt");

                            var transactions = serviceValidated.GetTransactionInfo(sessionId, sessionKey, ref nextChallenge, TransInfo);

                            if (transactions.Count > 0)
                            {
                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo success");

                                returnedTransactions = transactions;



                                returnedTransactions = returnedTransactions.ToList();
                                if (returnedTransactions.Count > 10)
                                {
                                    returnedTransactions = returnedTransactions.GetRange(0, 10);
                                }

                            }
                            else
                            {

                                getTransInfo TransInf = new getTransInfo();
                                TransInf.MBR = 1;
                                TransInf.PAN = PAN;
                                TransInf.ToTime = account.ToTime;
                                TransInf.FromTime = account.FromTime;

                                fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo First attempt");

                                var transaction = serviceValidated.GetTransactionInfo(sessionId, sessionKey, ref nextChallenge, TransInf);
                                if (transaction.Count > 0)
                                {
                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo success");

                                    returnedTransactions = transaction;

                                    returnedTransactions = returnedTransactions.ToList();
                                    if (returnedTransactions.Count > 10)
                                    {
                                        returnedTransactions = returnedTransactions.GetRange(0, 10);
                                    }


                                }
                                else
                                {

                                    getTransInfo TransIn = new getTransInfo();
                                    TransInf.MBR = 2;
                                    TransInf.PAN = PAN;
                                    TransInf.ToTime = account.ToTime;
                                    TransInf.FromTime = account.FromTime;

                                    fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo second attempt");


                                    var transact = serviceValidated.GetTransactionInfo(sessionId, sessionKey, ref nextChallenge, TransIn);
                                    if (transaction.Count > 0)
                                    {
                                        fimilogger.Info("Calling Intergration TranzwareCompassPlusFIMI transactioninfo success");

                                        returnedTransactions = transaction;

                                        returnedTransactions = returnedTransactions.ToList();
                                        if (returnedTransactions.Count > 10)
                                        {
                                            returnedTransactions = returnedTransactions.GetRange(0, 10);
                                        }
                                    }
                                    else
                                    {
                                        return returnedTransactions;
                                    }



                                }
                            }

                        }
                        
                    }

                }
                else
                {
                    fimilogger.Info($"response ....{response} no card was found for account {requestData}");
                }


                return returnedTransactions;


            }
            catch (Exception exec)
            {
                fimilogger.Info($"Logging.....Error {exec}");
                return new List<Transactions>();
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