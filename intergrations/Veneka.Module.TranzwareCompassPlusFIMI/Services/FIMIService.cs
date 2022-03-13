using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Services
{
    public class FIMIService : Service
    {
        #region Readonly Fields

        private readonly fimiClient client;

        #endregion

        #region Contructors
        public FIMIService(BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password, string logger)
        {
            client = new fimiClient(bindings, endpointAddress);

           

            var log = FIMILogger.GetFimiLoggerInstance();

     

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, logger));
                _log = LogManager.GetLogger(logger);
            }

            //foreach (OperationDescription od in client.Endpoint.Contract.Operations)
            //{
            //    od.Behaviors.Add(new FIMIIEndpointBehavior());
            //}

           // _log.Trace(m => m("Creating FIMI Client Service."));

            log.Info("Creating FIMI Client Service.");

            //15/12/2020
            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                                        X509Chain chain,
                                        SslPolicyErrors sslPolicyErrors) => true;

            //ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            foreach (X509Certificate2 x509 in collection)
            {
                if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")
                //TEST - "16454F0B70C07171FB271CC2A9F859A191C74828")
                //Prod - "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB"
                {
                    client.ClientCredentials.ClientCertificate.SetCertificate(
                     x509.SubjectName.Name, store.Location, StoreName.My);

                }
            }

            log.Info("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer);
            log.Info("Certificate Subject :" + client.ClientCredentials.ClientCertificate.Certificate.Subject);
            log.Info("Certificate Thumb print" + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint);


            //end
            IgnoreUntrustedSSL = true;
        }

        public FIMIService(System.ServiceModel.Channels.CustomBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password, string logger)
        {
            client = new fimiClient(bindings, endpointAddress);

            var lo = FIMILogger.GetFimiLoggerInstance();

            //lo.Info("Endpoint " + client.Endpoint.Address.Uri.ToString());

            //client.pro

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, logger));
                _log = LogManager.GetLogger(logger);
            }

            foreach (OperationDescription od in client.Endpoint.Contract.Operations)
            {
                od.Behaviors.Add(new FIMIIEndpointBehavior());
            }

            //_log.Trace(m => m("Creating FIMI Client Service."));
            lo.Info("Creating FIMI Client Service.");

            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                                        X509Chain chain,
                                        SslPolicyErrors sslPolicyErrors) => true;

            //ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            foreach (X509Certificate2 x509 in collection)
            {
                //if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")//PROD
               if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")//TEST
                {
                        client.ClientCredentials.ClientCertificate.SetCertificate(
                     x509.SubjectName.Name, store.Location, StoreName.My);

                }
            }

            lo.Info("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer);
            lo.Info("Certificate Subject :" + client.ClientCredentials.ClientCertificate.Certificate.Subject);
            lo.Info("Certificate Thumb print" + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint);

            IgnoreUntrustedSSL = true;
        }

        public FIMIService(System.ServiceModel.Channels.CustomBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password)
            : this(bindings, endpointAddress, userBasicAuth, username, password, null)
        {
            var loggers = FIMILogger.GetFimiLoggerInstance();

            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                                        X509Chain chain,
                                        SslPolicyErrors sslPolicyErrors) => true;

            //ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            foreach (X509Certificate2 x509 in collection)
            {
                //if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
                    //TEST - "16454F0B70C07171FB271CC2A9F859A191C74828")
                if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")
                {
                     client.ClientCredentials.ClientCertificate.SetCertificate(
                     x509.SubjectName.Name, store.Location, StoreName.My);

                }
            }

            //client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(certPath, "Ni_123", X509KeyStorageFlags.UserKeySet);

            //_log.Trace(m => m("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer));
            //_log.Trace(m => m("Certificate Subject : " + client.ClientCredentials.ClientCertificate.Certificate.Subject));
            //_log.Trace(m => m("Certificate Thumb print : " + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint));

            loggers.Info("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer);
            loggers.Info("Certificate Subject :" + client.ClientCredentials.ClientCertificate.Certificate.Subject);
            loggers.Info("Certificate Thumb print" + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint);

            IgnoreUntrustedSSL = true;

        }

        #endregion

        #region Public Methods

        public GetPersonInfoRp1 GetPersonInfo(GetPersonInfoRq1 person)
        {
            _log.Trace(m => m("Call To GetPersonInfo()"));

            AddUntrustedSSL();

            GetPersonInfoRp1 response = null;

            try
            {
                response = client.GetPersonInfoRq(person);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetPersonInfoRp1()
                {
                    Response = new GetPersonInfoRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public GetCardInfoRp1 GetCardInfo(GetCardInfoRq1 card)
        {
            _log.Trace(m => m("Call To GetCardInfo()"));
            _log.Trace($"Card Info:{card.Request.PAN}, {card.Request.MBR}");
            AddUntrustedSSL();

            GetCardInfoRp1 response = null;

            try
            {
                response = client.GetCardInfoRq(card);
                _log.Trace($"carduid:={response.Response.CardUID}");
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetCardInfoRp1()
                {
                    Response = new GetCardInfoRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public UpdatePersonRp1 UpdatePerson(UpdatePersonRq1 person)
        {
            _log.Trace(m => m("Call To UpdatePerson()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();

            UpdatePersonRp1 response = null;

            try
            {
                response = client.UpdatePersonRq(person);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new UpdatePersonRp1()
                {
                    Response = new UpdatePersonRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public SetCardPersonRp1 SetCardPerson(SetCardPersonRq1 person)
        {
            _log.Trace(m => m("Call To SetCardPerson()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();

            SetCardPersonRp1 response = null;

            try
            {
                response = client.SetCardPersonRq(person);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new SetCardPersonRp1()
                {
                    Response = new SetCardPersonRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public CreateAccountRp1 CreateAccount(CreateAccountRq1 account)
        {
            _log.Trace(m => m("Call To CreateAccount()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();

            CreateAccountRp1 response = null;

            try
            {
                response = client.CreateAccountRq(account);
                _log.DebugFormat("RESPONSE IS: {0}", response);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new CreateAccountRp1()
                {
                    Response = new CreateAccountRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public ResetCard2AcctLinkRp1 CardAccountLinkReset(ResetCard2AcctLinkRq1 cardReset)
        {
            _log.Trace(m => m("Call To ResetCard2AcctLink()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();

            ResetCard2AcctLinkRp1 response = null;

            try
            {
                response = client.ResetCard2AcctLinkRq(cardReset);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new ResetCard2AcctLinkRp1()
                {
                    Response = new ResetCard2AcctLinkRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public SetCardStatusRp1 SetCardStatus(SetCardStatusRq1 cardStatus)
        {
            _log.Trace(m => m("Call To CardAccountLinkReset()"));
            _log.Trace($"CardUID :{cardStatus.Request.CardUID}");
            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            SetCardStatusRp1 response = null;

            try
            {
                response = client.SetCardStatusRq(cardStatus);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new SetCardStatusRp1()
                {
                    Response = new SetCardStatusRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public ChangeECStatusRp1 ChangeECStatus(ChangeECStatusRq1 changeECStatus)
        {
            _log.Trace(m => m("Call To CardAccountLinkReset()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            ChangeECStatusRp1 response = null;

            try
            {
                response = client.ChangeECStatusRq(changeECStatus);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new ChangeECStatusRp1()
                {
                    Response = new ChangeECStatusRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product,
                        Echo = fex.Message
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public GetCardInfoRp1 GetPrepaidCardInfo(GetCardInfoRq1 getCardInfoRq)
        {
            _log.Trace(m => m("Call To GetCardInfoRq()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            GetCardInfoRp1 response = null;

            try
            {
                response = client.GetCardInfoRq(getCardInfoRq);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetCardInfoRp1()
                {
                    Response = new GetCardInfoRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product,
                        Echo = fex.Message
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public AcctCreditRp1 CreditPrepaidAccount(AcctCreditRq1 acctCreditRp)
        {
            _log.Trace(m => m("Call To AcctCreditRq()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            AcctCreditRp1 response = null;

            try
            {
                response = client.AcctCreditRq(acctCreditRp);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new AcctCreditRp1()
                {
                    Response = new AcctCreditRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product,
                        Echo = fex.Message
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }
        #endregion

        #region Session Specific Methods

        public InitSessionRp1 InitSession(InitSessionRq1 request)
        {

            InitSessionRp1 response = null;
            var logger = FIMILogger.GetFimiLoggerInstance();

           // _log.Trace(m => m("Call To InitSession()"));
            logger.Info("Call To InitSession()");

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            #region

            //if (IgnoreUntrustedSSL)
            //{

            //    //ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
            //    //                                        X509Chain chain,
            //    //                                        SslPolicyErrors sslPolicyErrors) => true;

            //    ////ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            //    //X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            //    //store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            //    //X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            //    //foreach (X509Certificate2 x509 in collection)
            //    //{
            //    //    if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
            //    //        //TEST - "16454F0B70C07171FB271CC2A9F859A191C74828")
            //    //    {
            //    //        //if(client.ClientCredentials.ClientCertificate  == null) 
            //    //        {
            //    //            client.ClientCredentials.ClientCertificate.SetCertificate(
            //    //         x509.SubjectName.Name, store.Location, StoreName.My);
            //    //        }
                        
            //    //    }
            //    //}

            //    ////client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(certPath, "Ni_123", X509KeyStorageFlags.UserKeySet);

            //    //_log.Trace(m => m("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer));
            //    //_log.Trace(m => m("Certificate Subject : " + client.ClientCredentials.ClientCertificate.Certificate.Subject));
            //    //_log.Trace(m => m("Certificate Thumb print : " + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint));


            //}

            #endregion
          
           
            try
            {
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (client.State == CommunicationState.Opened)
                {
                 
                    logger.Info("Comms object already in opened state");
                }
                else
                {
                 
                    logger.Info("Comms object not open, lets open");
                    client.Open();
                }

                logger.Info("Endpoint Address "+client.Endpoint.Address);
                response = client.InitSessionRq(request);
            }
            //catch (FaultException<ResponseCodes.DeclineRp> fex)
            //{
            //    _log.Warn(fex);
            //    logger.Info("Fex "+fex);
            //    var messageFault = fex.CreateMessageFault();
            //    var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

            //    response = new InitSessionRp1()
            //    {
            //        Response = new InitSessionRp
            //        {
            //            NextChallenge = decline.Response.NextChallenge,
            //            Response = decline.Response.Response,
            //            Ver = decline.Response.Ver,
            //            Product = decline.Response.Product
            //        }
            //    };
            // }
            //catch (CommunicationException commEx)
            //{
            //    _log.Warn(commEx);
            //    _log.Warn(commEx.InnerException);
            //    _log.Warn(commEx.StackTrace);
            //    logger.Info(" commexx"+commEx);
            //    logger.Info("innerexception "+commEx.InnerException);
            //    logger.Info("stacktrace "+commEx.StackTrace);


            //}
            catch (Exception e)
            {

                logger.Info("Exception "+e);
            }

            return response;
        }

        public LogonRp1 Logon(LogonRq1 request)
        {
            _log.Trace(m => m("Call To Logon()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();

            LogonRp1 response = null;

            try
            {
                response = client.LogonRq(request);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new LogonRp1()
                {
                    Response = new LogonRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                _log.Warn(commEx);
            }

            return response;
        }

        public GetTransInfoRp1 GetTransactionInfo(GetTransInfoRq1 transactionRequest)
        {

            _log.Trace("Call To GetTransInfoRq1()");

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            GetTransInfoRp1 response = null;

            try
            {
                response = client.GetTransInfoRq(transactionRequest);
                //response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetTransInfoRp1()
                {
                    Response = new GetTransInfoRp
                    {
                        NextChallenge = decline.Response.NextChallenge,
                        Response = decline.Response.Response,
                        Ver = decline.Response.Ver,
                        Product = decline.Response.Product,
                        Echo = fex.Message
                    }
                };
            }
            catch (CommunicationException commEx)
            {

                _log.Warn(commEx);
            }

            return response;

        
        }

          #endregion
    }
}