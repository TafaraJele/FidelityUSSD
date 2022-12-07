using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Inspector;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Services
{
    public class FIMIService : Service
    {
        #region Readonly Fields

        private readonly fimiClient client;

        #endregion

        #region Contructors
        public FIMIService(BasicHttpBinding bindings, EndpointAddress endpointAddress, bool? userBasicAuth, string username, string password, string logger)
        {
            client = new fimiClient(bindings, endpointAddress);
            FIMILogger log = FIMILogger.GetFimiLoggerInstance();
            if (string.IsNullOrWhiteSpace(logger))
            {
                client.Endpoint.Behaviors.Add(new LogClientBehaviour(userBasicAuth.GetValueOrDefault(), username, password, "TranzwareCompassPlusFIMILogger"));
            }
            else
            {
                client.Endpoint.Behaviors.Add(new LogClientBehaviour(userBasicAuth.GetValueOrDefault(), username, password, logger));
                Service._log = LogManager.GetLogger(logger);
            }
            log.Info("Creating FIMI Client Service.");
            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 x509 = enumerator.Current;
                if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")
                //if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
                {
                    client.ClientCredentials.ClientCertificate.SetCertificate(x509.SubjectName.Name, store.Location, StoreName.My);
                }
            }
            log.Info("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer);
            log.Info("Certificate Subject :" + client.ClientCredentials.ClientCertificate.Certificate.Subject);
            log.Info("Certificate Thumb print" + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint);
            base.IgnoreUntrustedSSL = true;
        }

        public FIMIService(CustomBinding bindings, EndpointAddress endpointAddress, bool? userBasicAuth, string username, string password, string logger)
        {
            client = new fimiClient(bindings, endpointAddress);
            FIMILogger lo = FIMILogger.GetFimiLoggerInstance();
            if (string.IsNullOrWhiteSpace(logger))
            {
                client.Endpoint.Behaviors.Add(new LogClientBehaviour(userBasicAuth.GetValueOrDefault(), username, password, "TranzwareCompassPlusFIMILogger"));
            }
            else
            {
                client.Endpoint.Behaviors.Add(new LogClientBehaviour(userBasicAuth.GetValueOrDefault(), username, password, logger));
                Service._log = LogManager.GetLogger(logger);
            }
            foreach (OperationDescription operation in client.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Add(new FIMIIEndpointBehavior());
            }
            lo.Info("Creating FIMI Client Service.");
            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Enumerator enumerator2 = store.Certificates.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                X509Certificate2 x509 = enumerator2.Current;
                if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")
                //if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
                {
                    client.ClientCredentials.ClientCertificate.SetCertificate(x509.SubjectName.Name, store.Location, StoreName.My);
                }
            }
            base.IgnoreUntrustedSSL = false;
        }

        public FIMIService(CustomBinding bindings, EndpointAddress endpointAddress, bool? userBasicAuth, string username, string password)
            : this(bindings, endpointAddress, userBasicAuth, username, password, null)
        {
            FIMILogger loggers = FIMILogger.GetFimiLoggerInstance();
            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 x509 = enumerator.Current;
                if (x509.Thumbprint == "16454F0B70C07171FB271CC2A9F859A191C74828")
                //if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
                {
                    client.ClientCredentials.ClientCertificate.SetCertificate(x509.SubjectName.Name, store.Location, StoreName.My);
                }
            }
            loggers.Info("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer);
            loggers.Info("Certificate Subject :" + client.ClientCredentials.ClientCertificate.Certificate.Subject);
            loggers.Info("Certificate Thumb print" + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint);
            base.IgnoreUntrustedSSL = true;
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
        public AcctDebitRp1 DebitPrepaidAccount(AcctDebitRq1 acctDebitRp)
        {
            Service._log.Trace(delegate (FormatMessageHandler m)
            {
                m("Call To DebitCreditRq()");
            });
            AddUntrustedSSL();
            try
            {
                AcctDebitRp1 acctDebitRp2 = client.AcctDebitRq(acctDebitRp);
                acctDebitRp2.Response.Echo = string.Empty;
                string responseData = JsonConvert.SerializeObject(acctDebitRp2);
                Service._log.Trace("Log response " + responseData);
                return acctDebitRp2;
            }
            catch (FaultException<Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes.DeclineRp> fex)
            {
                Service._log.Warn(fex);
                Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes.DeclineRp decline = fex.CreateMessageFault().GetDetail<Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes.DeclineRp>();
                return new AcctDebitRp1
                {
                    Response = new AcctDebitRp
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
                Service._log.Warn(commEx);
            }
            return null;
        }


        public GetWorkingKeyRp1 GetWorkingKey(GetWorkingKeyRq1 workingKeyRq1)
        {
            _log.Trace(m => m("Call To GetWorkingKeyRq()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            GetWorkingKeyRp1 response = null;

            try
            {
                response = client.GetWorkingKeyRq(workingKeyRq1);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetWorkingKeyRp1()
                {
                    Response = new GetWorkingKeyRp
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

        public GetPVV_PINOffsetRp1 GetPVV_PinOffset(GetPVV_PINOffsetRq1 getPVV_PINOffsetRq1)
        {
            _log.Trace(m => m("Call To GetWorkingKeyRq()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            GetPVV_PINOffsetRp1 response = null;

            try
            {
                response = client.GetPVV_PINOffsetRq(getPVV_PINOffsetRq1);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new GetPVV_PINOffsetRp1()
                {
                    Response = new GetPVV_PINOffsetRp
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

        public SetCardPVVRp1 SetCardPVV(SetCardPVVRq1 setCardPVVRq1)
        {
            _log.Trace(m => m("Call To SetCardPVV()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            SetCardPVVRp1 response = null;

            try
            {
                response = client.SetCardPVVRq(setCardPVVRq1);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new SetCardPVVRp1()
                {
                    Response = new SetCardPVVRp
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

        public AddEMVScriptRp1 AddEMVScript(AddEMVScriptRq1 setCardPVVRq1)
        {
            _log.Trace(m => m("Call To AddEMVScript()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            AddEMVScriptRp1 response = null;

            try
            {
                response = client.AddEMVScriptRq(setCardPVVRq1);
                response.Response.Echo = String.Empty;
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new AddEMVScriptRp1()
                {
                    Response = new AddEMVScriptRp
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
            _log.Trace(m => m("Call To InitSession()"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            #region old code for ref

            if (IgnoreUntrustedSSL)
            {

                //ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                //                                        X509Chain chain,
                //                                        SslPolicyErrors sslPolicyErrors) => true;

                ////ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

                //X509Store store = new X509Store("My", StoreLocation.LocalMachine);
                //store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                //X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                //foreach (X509Certificate2 x509 in collection)
                //{
                //    if (x509.Thumbprint == "86A52D26EA1FC3CA06557EAE1C13C4874BAF54EB")
                //        //TEST - "16454F0B70C07171FB271CC2A9F859A191C74828")
                //    {
                //        //if(client.ClientCredentials.ClientCertificate  == null) 
                //        {
                //            client.ClientCredentials.ClientCertificate.SetCertificate(
                //         x509.SubjectName.Name, store.Location, StoreName.My);
                //        }

                //    }
                //}

                ////client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(certPath, "Ni_123", X509KeyStorageFlags.UserKeySet);

                //_log.Trace(m => m("Certificate Issuer : " + client.ClientCredentials.ClientCertificate.Certificate.Issuer));
                //_log.Trace(m => m("Certificate Subject : " + client.ClientCredentials.ClientCertificate.Certificate.Subject));
                //_log.Trace(m => m("Certificate Thumb print : " + client.ClientCredentials.ClientCertificate.Certificate.Thumbprint));


            }

            #endregion
            InitSessionRp1 response = null;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (client.State == CommunicationState.Opened)
                {
                    _log.Debug("Comms object already in opened state");
                }
                else
                {
                    _log.Debug("Comms object not open, lets open");
                    client.Open();
                }

                response = client.InitSessionRq(request);
            }
            catch (FaultException<ResponseCodes.DeclineRp> fex)
            {
                _log.Warn(fex);
                var messageFault = fex.CreateMessageFault();
                var decline = messageFault.GetDetail<ResponseCodes.DeclineRp>();

                response = new InitSessionRp1()
                {
                    Response = new InitSessionRp
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
                _log.Warn(commEx.InnerException);
                _log.Warn(commEx.StackTrace);

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

        public GetCardStatementRp1 GetCardStatement(GetCardStatementRq1 statementRequest)
        {
            FIMILogger log = FIMILogger.GetFimiLoggerInstance();
            log.Info("Call To GetCardStatement()");
            AddUntrustedSSL();
            GetCardStatementRp1 response = null;
            try
            {
                log.Info("Try Call To GetCardStatement()");
                return client.GetCardStatementRq(statementRequest);
            }
            catch (FaultException<Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes.DeclineRp> fex)
            {
                log.Error(fex.Message);
                fex.CreateMessageFault().GetDetail<Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes.DeclineRp>();
                return new GetCardStatementRp1
                {
                    Response = new GetCardStatementRp
                    {
                        Response = 0,
                        Echo = fex.Message
                    }
                };
            }
            catch (CommunicationException commEx)
            {
                response = new GetCardStatementRp1
                {
                    Response = new GetCardStatementRp
                    {
                        Response = 0,
                        Echo = commEx.Message
                    }
                };
                log.Error(commEx.Message);
                return response;
            }
        }

        #endregion
    }
}