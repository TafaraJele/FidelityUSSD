using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Module.TranzwareCompassPlusFIMI.Services;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;

using System.ServiceModel;
using Veneka.Module.TranzwareCompassPlusFIMI.Cryptography;
using System.IO;
using Veneka.Module.IntegrationDataControl.DAL;
using System.Configuration;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;

namespace Veneka.Module.TranzwareCompassPlusFIMI
{
    public enum IdentificationType
    {
        Passport = 1,
        DriversLicense = 2,
        InsuranceNumber = 3,
        TPN = 4,
        NationalID = 5
    }


    public class FIMIServicesValidated : ServicesValidated
    {
        #region Constants
        private const string INTEGRATION_NAME = "FIMIService";
        private const string HEX_SPACES = "2020202020202020";
        #endregion

        #region Readonly Fields
        private readonly FIMIService _fimiService;
        private readonly string _clerk = String.Empty;
        private readonly string _password = String.Empty;
        #endregion

        #region Properties

        /// <summary>
        /// Set to true if the SSL Certificate is untrusted and you want to service to not throw an exception.
        /// </summary>
        public override bool IgnoreUntrustedSSL
        {
            get
            {
                return _fimiService.IgnoreUntrustedSSL;
            }
            set
            {
                _fimiService.IgnoreUntrustedSSL = value;
            }
        }

        #endregion

        #region Constructors

        public FIMIServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, Authentication authentication,
                                        FileInfo fileInfo, string username = null, string password = null, string logger = null, bool useCustomEncoder = false)
                                    : base(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, fileInfo, logger)
        {
            _fimiService = Init(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger, useCustomEncoder);
            this._clerk = username;
            this._password = password;
        }

        public FIMIServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string connectionString, string username = null, string password = null, string logger = null, bool useCustomEncoder = false)
                                : base(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, connectionString, logger)
        {
            _fimiService = Init(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger, useCustomEncoder);
            this._clerk = username;
            this._password = password;
        }
        public FIMIServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                              Authentication authentication, IDefaultDataDAL dal, string username = null, string password = null, string logger = null, bool useCustomEncoder = false)
                              : base(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, dal, logger)
        {
            _fimiService = Init(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger, useCustomEncoder);
            this._clerk = username;
            this._password = password;
        }

        private FIMIService Init(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string logger, bool useCustomEncoder)
        {
            //var service = new FIMIService(General.BuildBindings(protocol,
            //                                System.ServiceModel.Channels.MessageVersion.Soap12, timeoutMilliSeconds, useCustomEncoder),
            //                                General.BuildEndpointAddress(protocol, address, port, path),
            //                                authentication == Authentication.BASIC ? true : false,
            //                                username, password, logger);
            var service = new FIMIService(General.BuildBindings(protocol,
                                           System.ServiceModel.Channels.MessageVersion.Soap12, timeoutMilliSeconds, useCustomEncoder),
                                           General.BuildEndpointAddress(protocol, address, port, path),
                                           authentication == Authentication.BASIC ? true : false,
                                           username, password, logger);

            if (password.Length > 8)
                throw new ArgumentException("FIMI password is greater than 8 characters.", "password");

            return service;
        }
        #endregion

        #region Public Methods

        public bool InitSession(out int sessionId, out string sessionKey, out string nextChallenge)
        {
            sessionId = 0;
            bool validResponse = false;
            FIMILogger logger = FIMILogger.GetFimiLoggerInstance();
            logger.Info("**********Calling InitSession**********");
            string hexPassword = Utility.StringToHex(_password.ToUpper().Trim());
            string result1 = TripleDes.Encrypt(hexPassword, hexPassword);
            string result2 = TripleDes.Decrypt("2020202020202020", result1);
            sessionKey = TripleDes.Encrypt(hexPassword, result2);
            nextChallenge = "";
            logger?.Info("HexPass:" + hexPassword + ", Result1:" + result1 + ", Result:" + result2 + ", SessionKey:" + sessionKey);
            InitSessionRq initSessionRq = new InitSessionRq
            {
                Product = "FIMI",
                Ver = 13.6m,
                NeedDictsSpecified = true,
                NeedDicts = 0,
                Clerk = _clerk
            };
            InitSessionRp1 response = _fimiService.InitSession(new InitSessionRq1
            {
                Request = initSessionRq
            });
            logger.Info("Checking InitSession() response errors");
            if (response == null || response.Response == null || response.Response.Response != 1)
            {
                logger.Info("InitSession() response is null or false");
                validResponse = false;
            }
            else
            {
                logger.Info("InitSession() response valid.");
                validResponse = true;
                sessionId = response.Response.Id;
                nextChallenge = response.Response.NextChallenge;
            }
            logger?.Info($"SessionId={sessionId}, SessionKey={sessionKey}");
            return validResponse;
        }
        public bool Logon(int sessionId, string sessionKey, ref string nextChallenge)
        {

            bool validResponse = false;
            FIMILogger log = FIMILogger.GetFimiLoggerInstance();
            log.Info("**********Calling Logon**********");
            string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            LogonRq logonRq = new LogonRq
            {
                Product = "FIMI",
                Ver = 13.6m,
                Clerk = _clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId
            };
            log?.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            LogonRp1 response = _fimiService.Logon(new LogonRq1
            {
                Request = logonRq
            });
            log.Info("Checking Logon() response errors");
            if (response == null || response == null || response.Response == null || response.Response.Response != 1)
            {
                log.Info("Logon() response is null or false");
                validResponse = false;
            }
            else
            {
                log.Info("Logon() response is valid");
                validResponse = true;
            }
            nextChallenge = response.Response.NextChallenge;
            return validResponse;
        }
        public List<Cards> GetPersonInfo(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, GetPersonInfo personInfo)
        {
            string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            FIMILogger loggs = FIMILogger.GetFimiLoggerInstance();
            loggs.Info("**********Calling GetPersonInfo Information**********");
            List<Cards> cards = new List<Cards>();
            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6m,
                Clerk = _clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = personInfo.InstName,
                IdSpecified = true,
                Id = personInfo.Id,
                Identity = personInfo.identity,
                IdentType = personInfo.identityType
            };
            loggs?.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            GetPersonInfoRp1 response = _fimiService.GetPersonInfo(new GetPersonInfoRq1
            {
                Request = personInfoReq
            });
            loggs.Info("Checking GetPersonInfo() response errors");
            if (response == null || response.Response == null)
            {
                loggs.Info("GetPersonInfo() response is null or false");
            }
            else
            {
                loggs.Info("GetPersonInfo() response valid");
                nextChallenge = response.Response.NextChallenge;
                GetPersonInfoRpCardsRow[] row2 = response.Response.Cards.Row;
                foreach (GetPersonInfoRpCardsRow row in row2)
                {
                    Cards card = new Cards
                    {
                        CardNumber = row.PAN,
                        ExpDate = row.ExpDate,
                        PersonId = row.PersonId,
                        Code = 200,
                        Message = "Success"
                    };
                    cards.Add(card);
                }
                loggs.Info("GetPerson Cards List " + cards);
            }
            return cards;
        }
        public int GetPersonInfo(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, out GetPersonInfoRp1 personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = instName,
                IdSpecified = true,
                Id = id
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });

            _log.Trace(m => m("Checking GetPersonInfo() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetPersonInfo() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetPersonInfo() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            personInfo = response;
            return response.Response.Response;
        }

        public int GetPersonInfo(string customerId, IdentificationType idType, string instName, int sessionId, string sessionKey, ref string nextChallenge, out GetPersonInfoRp1 personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = instName,
                Identity = customerId,
                IdentTypeSpecified = true,
                IdentType = (int)idType
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });

            _log.Trace(m => m("Checking GetPersonInfo() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetPersonInfo() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetPersonInfo() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            personInfo = response;
            return response.Response.Response;
        }

        public int UpdatePerson(string inst, int personId, string fullNames, string idNumber, DateTime dob, string address, string postcode, string branchCode, int sessionId, string sessionKey, ref string nextChallenge)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            UpdatePersonRq updatePersonRq = new UpdatePersonRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                UpdateFieldsMethodSpecified = true,
                UpdateFieldsMethod = (int)General.UpdateFieldMethods.OnlyTransferredFields,
                FIO = fullNames,
                Id = personId,
                InstName = inst,
                IdentType = 1,
                IdentTypeSpecified = true,
                Identity = idNumber,
                Birthday = dob,
                BirthdaySpecified = true,
                ResidentCountry = 288,
                ResidentCountrySpecified = true,
                ResidentState = "Accra",
                ResidentStateExternalId = "05",
                AddressInLatin = address,
                PostalCode = postcode,
                BranchId = branchCode
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, updatePersonRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.UpdatePerson(new UpdatePersonRq1 { Request = updatePersonRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking UpdatePerson() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("UpdatePerson() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("UpdatePerson() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response.Response.Response;
        }

        public int SetCardPerson(long personId, string cardUID, int sessionId, string sessionKey, ref string nextChallenge)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            SetCardPersonRq updatePersonRq = new SetCardPersonRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                NewPersonId = personId,
                CardUID = cardUID
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, updatePersonRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.SetCardPerson(new SetCardPersonRq1 { Request = updatePersonRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking UpdatePerson() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("UpdatePerson() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("UpdatePerson() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response.Response.Response;
        }

        public int CreateAccount(string accountNumber, int currency, decimal ledgerBalance, decimal availableBalance, long personId,
            General.AccountTypes accountType, General.AccountStatus accountStatus, string comment, int sessionId, string sessionKey, ref string nextChallenge)
        {

            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            CreateAccountRq createAccountRq = new CreateAccountRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountNumber,
                Currency = currency, // ISO Code
                LedgerBalance = ledgerBalance,
                AvailBalance = availableBalance,
                PersonIDSpecified = true,
                PersonID = personId,
                Type = (int)accountType,
                Status = (int)accountStatus,
                Comment = comment//"3~101" //TODO: Supplied by EMP per bank
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, createAccountRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.CreateAccount(new CreateAccountRq1 { Request = createAccountRq });


            _log.Trace(m => m("Checking CreateAccount() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("CreateAccount() response is null"));
            }
            else
            {
                _log.Trace(m => m("CreateAccount() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response.Response.Response;
        }

        public int ResetCard2AcctLinkRq(string accountNumber, long personId, string cardUUID, int sessionId, string sessionKey, ref string nextChallenge)
        {
            //string nextPwd = "";
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            ResetCard2AcctLinkRq resetCard2AcctLinkRq = new ResetCard2AcctLinkRq
            {
                Clerk = this._clerk,
                Password = nextPwd,
                Ver = 13.6M,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountNumber,
                //PersonIdSpecified = true,
                //PersonId = personId,
                CardUID = cardUUID
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, resetCard2AcctLinkRq);

            //General.BuildBasicBindings(Protocol.HTTP,
            //                                System.ServiceModel.Channels.MessageVersion.Soap12, 60000)
            //"213.131.67.214"
            //TODO: Hack for testing
            //http://192.168.4.123:9998/ResetCardToAcctLink
            //FIMIService _fimiService2 = new FIMIService(General.BuildBindings(Protocol.HTTPS,
            //                                System.ServiceModel.Channels.MessageVersion.Soap12, 60000),
            //                                General.BuildEndpointAddress(Protocol.HTTPS, "213.131.67.214", 9998, "ResetCardToAcctLink"),
            //                                false,
            //                                "", "", "");

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.CardAccountLinkReset(new ResetCard2AcctLinkRq1 { Request = resetCard2AcctLinkRq });

            _log.Trace(m => m("Checking ResetCardToAccountLink() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("ResetCardToAccountLink() response is null"));
            }
            else
            {
                _log.Trace(m => m("ResetCardToAccountLink() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response.Response.Response;
        }

        public int SetCardStatus(string cardUUID, int cardStatus, int sessionId, string sessionKey, ref string nextChallenge)
        {
            //string nextPwd = "";
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            SetCardStatusRq setCardStatusRq1 = new SetCardStatusRq
            {
                Clerk = this._clerk,
                Password = nextPwd,
                Ver = 13.6M,
                SessionSpecified = true,
                Session = sessionId,
                Status = cardStatus,
                CardUID = cardUUID,
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, setCardStatusRq1);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.SetCardStatus(new SetCardStatusRq1 { Request = setCardStatusRq1 });

            _log.Trace(m => m("Checking SetCardStatusRq() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("SetCardStatusRq() response is null"));
            }
            else
            {
                _log.Trace(m => m("SetCardStatusRq() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response.Response.Response;
        }

        public ChangeECStatusRp1 ChangeECStatus(string cardUUID, int cardStatus, int sessionId, string sessionKey, ref string nextChallenge)
        {
            //string nextPwd = "";
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            ChangeECStatusRq changeECStatusRq = new ChangeECStatusRq
            {
                Clerk = this._clerk,
                Password = nextPwd,
                Ver = 13.6M,
                SessionSpecified = true,
                Session = sessionId,
                Status = cardStatus,
                Type = 1,
                TypeSpecified = true,
                CardUID = cardUUID,
                ChangeReason = "InDIGO Process"
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, changeECStatusRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.ChangeECStatus(new ChangeECStatusRq1 { Request = changeECStatusRq });

            _log.Trace(m => m("Checking SetCardStatusRq() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("SetCardStatusRq() response is null"));
            }
            else
            {
                _log.Trace(m => m("SetCardStatusRq() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;

            return response;
        }

        public JObject CreditPrepaidAccount(int sessionId, string sessionKey, ref string nextChallenge, AcctCredit accountInfo)
        {
            string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            FIMILogger loge = FIMILogger.GetFimiLoggerInstance();
            loge.Info("**********Calling CreditPrepaidAccount Information**********");
            new List<AccResponse>();
            var result = new
            {
                Message = "",
                Code = 0
            };
            AcctCreditRq accreq = new AcctCreditRq
            {
                Ver = 13.6m,
                Clerk = _clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountInfo.Account,
                Amount = accountInfo.Amount,
                IgnoreImpact = 1,
                IgnoreImpactSpecified = true
            };
            loge?.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            AcctCreditRp1 response = _fimiService.CreditPrepaidAccount(new AcctCreditRq1
            {
                Request = accreq
            });
            loge.Info("Checking AcctCreditRq1() response errors");
            if (response == null || response.Response == null)
            {
                loge.Info("Checking AcctCreditRq1() response is null or false");
            }
            else
            {
                loge.Info("Checking AcctDebitRq1() response valid");
                if (response.Response.Response != 1)
                {
                    loge.Debug("Response code was not equal to 1 failed");
                    result = new
                    {
                        Message = Response.GetResponse(response.Response.Response),
                        Code = response.Response.Response
                    };
                }
                else
                {
                    loge.Debug("Response code was not equal 1 success");
                    result = new
                    {
                        Message = "Success",
                        Code = 200
                    };
                }
            }
            return JObject.Parse(new JavaScriptSerializer().Serialize(result).Replace("\\\\", ""));
        }

        public JObject DebitPrepaidAccount(int sessionId, string sessionKey, ref string nextChallenge, AcctCredit accountInfo)
        {
            string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            FIMILogger loge = FIMILogger.GetFimiLoggerInstance();
            loge.Info("**********Calling DebitPrepaidAccount Information**********");
            new List<AccResponse>();
            var result = new
            {
                Message = "Failed",
                Code = 99
            };
            AcctDebitRq accreq = new AcctDebitRq
            {
                Ver = 13.6m,
                Clerk = _clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountInfo.Account,
                Amount = accountInfo.Amount,
                IgnoreImpact = 1,
                IgnoreImpactSpecified = true
            };
            loge?.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            AcctDebitRp1 response = _fimiService.DebitPrepaidAccount(new AcctDebitRq1
            {
                Request = accreq
            });
            loge.Info("Checking AcctDebitRq1() response errors");
            if (response == null || response.Response == null)
            {
                loge.Info("Checking AcctDebitRq1() response is null or false");
            }
            else
            {
                loge.Info("Checking AcctDebitRq1() response valid");
                if (response.Response.Response != 1)
                {
                    loge.Debug("Response code was not equal to 1 failed");
                    result = new
                    {
                        Message = Response.GetResponse(response.Response.Response),
                        Code = response.Response.Response
                    };
                }
                else
                {
                    loge.Debug("Response code was not equal 1 success");
                    result = new
                    {
                        Message = "Success",
                        Code = 200
                    };
                }
            }
            return JObject.Parse(new JavaScriptSerializer().Serialize(result).Replace("\\\\", ""));
        }

        public int GetPrepaidAccountDetail(int sessionId, string sessionKey, ref string nextChallenge, string pan, int mbr, out GetCardInfoRp1 cardInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetCardInfoRq prepaidReqInfo = new GetCardInfoRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = mbr,
                PAN = pan,
                MBRSpecified = true
            };

            _defaultVal.LoadDefaults(INTEGRATION_NAME, prepaidReqInfo);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetPrepaidCardInfo(new GetCardInfoRq1 { Request = prepaidReqInfo });

            _log.Trace(m => m("Checking GetPrepaidAccountDetail() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetPrepaidAccountDetail() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetPrepaidAccountDetail() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            cardInfo = response;
            return response.Response.Response;
        }

        public List<CardDetails> GetCardInfo(int sessionId, string sessionKey, ref string nextChallenge, GetCardInfo cardInfo)
        {
            string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            FIMILogger logga = FIMILogger.GetFimiLoggerInstance();
            logga.Info("**********Calling GetPersonInfo Information**********");
            List<CardDetails> cardDetails = new List<CardDetails>();
            GetCardInfoRq CardInfo = new GetCardInfoRq
            {
                Ver = 13.6m,
                Clerk = _clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = cardInfo.MBR,
                PAN = cardInfo.PAN
            };
            logga?.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            GetCardInfoRp1 response = _fimiService.GetCardInfo(new GetCardInfoRq1
            {
                Request = CardInfo
            });
            logga.Info("Checking GetCardInfo() response errors");
            if (response == null || response.Response == null)
            {
                logga.Info("GetCardInfo() response is null or false");
            }
            else
            {
                logga.Info("GetCardInfo() response valid");
                nextChallenge = response.Response.NextChallenge;
                GetCardInfoRpAccountsRow[] row2 = response.Response.Accounts.Row;
                foreach (GetCardInfoRpAccountsRow row in row2)
                {
                    if (row.AcctNo.Contains("GoG"))
                    {     
                        logga.Info($"Account is a GoG Account");

                        string xmlResponse = ReadNIResponse(row.AccountUID);
                        string accountUID = GetAccountUID(xmlResponse);
                        CardDetails cardDet = new CardDetails
                        {
                            AcctNo = row.AcctNo,
                            Status = int.Parse(GetCardStatus(accountUID)),
                            Currency = row.Currency,
                            AvailBalance = row.AvailBalance,
                            Code = 200,
                            Message = "Success",
                            CardReferenceNumber = long.Parse(GetCardReferenceNumber(accountUID))
                        };
                        cardDetails.Add(cardDet);
                    }
                    else
                    {
                        logga.Info($"Account Number does not contain GoG prefix and is not a GoG account number {row.AcctNo}");
                    }
                    
                }
            }
            return cardDetails;
        }


        public int GetWorkingKey(int sessionId, string sessionKey, ref string nextChallenge, out GetWorkingKeyRp1 workingKeyResp)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetWorkingKeyRq getWorkingKeyRq = new GetWorkingKeyRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, getWorkingKeyRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetWorkingKey(new GetWorkingKeyRq1 { Request = getWorkingKeyRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking GetWorkingKey() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetWorkingKey() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetWorkingKey() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            workingKeyResp = response;
            return response.Response.Response;
        }

        public int GetPVVPinOffSet(int keyID, int isPVV, string pan, int mbr, int pinBlockFormat, string newPinBlock, int sessionId, string sessionKey, ref string nextChallenge, out GetPVV_PINOffsetRp1 pvvPinOffSetResp)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetPVV_PINOffsetRq getPVVOffSetRq = new GetPVV_PINOffsetRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = mbr,
                MBRSpecified = true,
                KeyId = keyID,
                IsPVV = isPVV,
                PAN = pan,
                PINBlockFormat = pinBlockFormat,
                PINBlockFormatSpecified = true,
                NewPINBlock = newPinBlock
            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, getPVVOffSetRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetPVV_PinOffset(new GetPVV_PINOffsetRq1 { Request = getPVVOffSetRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking GetPVVPinOffSet() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetPVVPinOffSet() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetPVVPinOffSet() response valid"));
                _log.Trace($"PVV Offset - {response.Response.NewPVV_PINOffset}");
                _log.Trace($"New PinBlock - {response.Response.NewPINBlock}");
                _log.Trace($"Resp Code - {response.Response.Response}");
                _log.DebugFormat($"Transaction ID  - {TransactionId}");
            }

            nextChallenge = response.Response.NextChallenge;
            pvvPinOffSetResp = response;
            return response.Response.Response;
        }

        public int SetCardPVV(int mbr, int newPVV, string pan, string changeReason, int sessionId, string sessionKey, ref string nextChallenge, out SetCardPVVRp1 setCardPVVRp1)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            SetCardPVVRq setPVVRq = new SetCardPVVRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = mbr,
                MBRSpecified = true,
                PAN = pan,
                NewPVV = newPVV,
                NewPVVSpecified = true,
                ChangeReason = changeReason


            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, setPVVRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.SetCardPVV(new SetCardPVVRq1 { Request = setPVVRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking SetCardPVV() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("SetCardPVV() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("SetCardPVV() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            setCardPVVRp1 = response;
            return response.Response.Response;
        }

        public int AddEMVScript(int keyId, int mbr, string pan, string pinBlockVal, int sessionId, string sessionKey, ref string nextChallenge, out AddEMVScriptRp1 addEMVScriptRp1)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            AddEMVScriptRq addEMVScriptRq = new AddEMVScriptRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = mbr,
                MBRSpecified = true,
                PAN = pan,
                Value = pinBlockVal,
                KeyId = keyId,
                KeyIdSpecified = true,
                KeyType = 1,
                KeyTypeSpecified = true,
                Type = 7


            };

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, addEMVScriptRq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.AddEMVScript(new AddEMVScriptRq1 { Request = addEMVScriptRq });

            var TransactionId = response.Response.TranId;

            _log.Trace(m => m("Checking AddEMVScript() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("AddEMVScript() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("AddEMVScript() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            addEMVScriptRp1 = response;
            return response.Response.Response;
        }
        private string ReadNIResponse(string accountUID)
        {
            var xmlresponse = string.Empty;
            //string filePath = @"C:\Config\IndigoPrepaidUAT\NIResponse";
            string filePath = ConfigurationManager.AppSettings.Get("NIResponsePath");
            filePath = $@"{filePath}";
            try
            {
                string path = System.IO.Path.Combine(filePath, $"{accountUID}.txt");
                if (File.Exists(path))
                {
                    StreamReader reader = new StreamReader(path);
                    xmlresponse = reader.ReadToEnd();
                    reader.Close();
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                _log.Debug($"Failed to write NI xml response to location {filePath} with exception {ex.Message} ");
            }

            return xmlresponse;

        }

        private string GetCardUID(string accountUID)
        {
            var accountUIDArray = accountUID.Split(';');

            return accountUIDArray[1];
        }
        private string GetAccountUID(string xmlResponse)
        {
            var pos1 = xmlResponse.LastIndexOf("<m0:AccountUID>");

            var pos2 = xmlResponse.LastIndexOf("</m0:AccountUID>");

            var length = pos2 - pos1;

            var value = xmlResponse.Substring(pos1, length);

            char[] splitArray = new char[2] { '<', '>' };

            var valueArray = value.Split(splitArray);

            return valueArray[2];
        }
        private string GetCardStatus(string accountUID)
        {
            return accountUID.Split(';')[2];
        }
        private string GetCardReferenceNumber(string accountUID)
        {
            return accountUID.Split(';')[1];
        }
        public GetCardStatementRp1 GetCardStatement(int sessionId, string sessionKey, ref string nextChallenge, string pan, int mbr)
        {
            //string nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string nextPwd = "jdfdfhkdasdhb";
            FIMILogger log = FIMILogger.GetFimiLoggerInstance();
            log.Info("**********Calling GetCard Information**********");
            GetCardStatementRq1 statementRequest = new GetCardStatementRq1
            {
                Request = new GetCardStatementRq
                {
                    PAN = pan,
                    MBR = mbr,
                    Clerk = _clerk,
                    Password = nextPwd,
                    Product = "FIMI",
                    Session = sessionId,
                    Ver = 16.2m,
                    MBRSpecified = true,
                    SessionSpecified = true
                }
            };
            log.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");
            var response = _fimiService.GetCardStatement(statementRequest);

            return response;
        }

        #endregion
    }
}
