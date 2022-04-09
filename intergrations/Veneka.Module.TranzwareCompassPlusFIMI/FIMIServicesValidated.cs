using System;
using System.Collections.Generic;
using Veneka.Module.TranzwareCompassPlusFIMI.Services;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;
using Veneka.Module.TranzwareCompassPlusFIMI.Cryptography;
using System.IO;
using Veneka.Module.IntegrationDataControl.DAL;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;
using Newtonsoft.Json.Linq;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;
using System.Xml;
using NUnit.Framework.Interfaces;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Configuration;

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
            var logger = FIMILogger.GetFimiLoggerInstance();
            logger.Info("**********Calling InitSession**********");
            var hexPassword = Utility.StringToHex(this._password.ToUpper().Trim());
            var result1 = TripleDes.Encrypt(hexPassword, hexPassword);
            var result2 = TripleDes.Decrypt(HEX_SPACES, result1);
            sessionKey = TripleDes.Encrypt(hexPassword, result2);
            nextChallenge = "";


            //if (_log.IsDebugEnabled)
            //_log.DebugFormat("HexPass:{0}, Result1:{1}, Result:{2}, SessionKey:{3}", hexPassword, result1, result2, sessionKey);
            if (logger != null)
                logger.Info($"HexPass:{hexPassword}, Result1:{result1}, Result:{result2}, SessionKey:{sessionKey}");

            InitSessionRq initSessionRq = new InitSessionRq
            {
                Product = "FIMI",
                Ver = 13.6M,
                NeedDictsSpecified = true,
                NeedDicts = 0,
                Clerk = this._clerk
            };

            //Load Defaults for the header
            //_defaultVal.LoadDefaults(INTEGRATION_NAME, initSessionRq);

            var response = _fimiService.InitSession(new InitSessionRq1 { Request = initSessionRq });

            //_log.Trace(m => m("Checking InitSession() response errors"));
            logger.Info("Checking InitSession() response errors");
            if ((response == null || response.Response == null) || (response.Response.Response != 1))
            {
                //_log.Trace(m => m("InitSession() response is null or false"));
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

            //Need session ID for later



            //if (_log.IsDebugEnabled)
            //    _log.DebugFormat("SessionId={0}, SessionKey={1}", sessionId, sessionKey);

            if (logger != null)
                logger.Info($"SessionId={sessionId}, SessionKey={sessionKey}");

            return validResponse;
        }

        public string Mask(string CardPan)
        {
            string Mask = "";

            var maskedPan = CardPan.Aggregate(string.Empty, (value, next) =>
            {
                if (value.Length >= 6 && value.Length < CardPan.Length - 4)
                {
                    next = '*';
                }

                Mask = value + next;
                return Mask;
            });
            return Mask;
        }

        public bool Logon(int sessionId, string sessionKey, ref string nextChallenge)
        {
            bool validResponse = false;
            var log = FIMILogger.GetFimiLoggerInstance();
            log.Info("**********Calling Logon**********");

            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            LogonRq logonRq = new LogonRq
            {
                Product = "FIMI",
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId
            };

            //Load Defaults for the header
            //_defaultVal.LoadDefaults(INTEGRATION_NAME, logonRq);

            //if (_log.IsDebugEnabled)
            //    _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);

            if (log != null)
                log.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.Logon(new LogonRq1 { Request = logonRq });


            log.Info("Checking Logon() response errors");
            if ((response == null || response == null || response.Response == null) || (response.Response.Response != 1))
            {

                log.Info("Logon() response is null or false");
                validResponse = false;
            }
            else
            {
                log.Info("Logon() response is null or false");
                validResponse = true;
            }

            nextChallenge = response.Response.NextChallenge;

            return validResponse;
        }


        public List<Transactions> GetTransactionInfo(int sessionId, string sessionKey, ref string nextChallenge, getTransInfo getTransInfo)
        {
            // var logger = FIMILogger.GetFimiLoggerInstance();
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string Currency = "";
            string CardUID = "";
            string CardNumber = "";
            string TranNumber = "";
            string Type = "";
            int Id = 0;

            var logs = FIMILogger.GetFimiLoggerInstance();
            logs.Info("**********Calling Transaction Information**********");

            Decimal Amount = 0;
            Decimal Fee = 0;
            DateTime Time = new DateTime();

            DateTime FromTime = new DateTime();
            DateTime ToTime = new DateTime();



            List<Transactions> trans = new List<Transactions>();

            // int[] rowValue = new int[] { 100, 200 }

            //int[] rowValue = new int[2];
            //rowValue[0] = 100;
            //rowValue[1] = 200;

            GetTransInfoRqTypeRow[] getTransInfoRqTypeRows = new GetTransInfoRqTypeRow[4];

            GetTransInfoRqTranCodeRow[] getTransInfoRqTranCodeRows = new GetTransInfoRqTranCodeRow[198];


            //foreach (var RowValue in rowValue)
            //{
            // for (int i = 0; i < rowValue.Length; i++)
            //{
            getTransInfoRqTypeRows[0] = new GetTransInfoRqTypeRow { Value = 200 };
            getTransInfoRqTypeRows[1] = new GetTransInfoRqTypeRow { Value = 420 };
            getTransInfoRqTypeRows[2] = new GetTransInfoRqTypeRow { Value = 999 };
            // }
            // }

            getTransInfoRqTranCodeRows[0] = new GetTransInfoRqTranCodeRow { Value = 10 };
            getTransInfoRqTranCodeRows[1] = new GetTransInfoRqTranCodeRow { Value = 15 };
            getTransInfoRqTranCodeRows[2] = new GetTransInfoRqTranCodeRow { Value = 20 };
            getTransInfoRqTranCodeRows[3] = new GetTransInfoRqTranCodeRow { Value = 21 };
            getTransInfoRqTranCodeRows[4] = new GetTransInfoRqTranCodeRow { Value = 22 };
            getTransInfoRqTranCodeRows[5] = new GetTransInfoRqTranCodeRow { Value = 23 };
            getTransInfoRqTranCodeRows[6] = new GetTransInfoRqTranCodeRow { Value = 26 };
            getTransInfoRqTranCodeRows[7] = new GetTransInfoRqTranCodeRow { Value = 27 };
            getTransInfoRqTranCodeRows[8] = new GetTransInfoRqTranCodeRow { Value = 30 };
            getTransInfoRqTranCodeRows[9] = new GetTransInfoRqTranCodeRow { Value = 31 };
            getTransInfoRqTranCodeRows[10] = new GetTransInfoRqTranCodeRow { Value = 40 };
            getTransInfoRqTranCodeRows[11] = new GetTransInfoRqTranCodeRow { Value = 41 };
            getTransInfoRqTranCodeRows[12] = new GetTransInfoRqTranCodeRow { Value = 42 };
            getTransInfoRqTranCodeRows[13] = new GetTransInfoRqTranCodeRow { Value = 43 };
            getTransInfoRqTranCodeRows[14] = new GetTransInfoRqTranCodeRow { Value = 46 };
            getTransInfoRqTranCodeRows[15] = new GetTransInfoRqTranCodeRow { Value = 47 };
            getTransInfoRqTranCodeRows[16] = new GetTransInfoRqTranCodeRow { Value = 48 };
            getTransInfoRqTranCodeRows[18] = new GetTransInfoRqTranCodeRow { Value = 50 };
            getTransInfoRqTranCodeRows[19] = new GetTransInfoRqTranCodeRow { Value = 51 };
            getTransInfoRqTranCodeRows[20] = new GetTransInfoRqTranCodeRow { Value = 52 };
            getTransInfoRqTranCodeRows[21] = new GetTransInfoRqTranCodeRow { Value = 53 };
            getTransInfoRqTranCodeRows[22] = new GetTransInfoRqTranCodeRow { Value = 54 };
            getTransInfoRqTranCodeRows[23] = new GetTransInfoRqTranCodeRow { Value = 55 };
            getTransInfoRqTranCodeRows[24] = new GetTransInfoRqTranCodeRow { Value = 56 };
            getTransInfoRqTranCodeRows[25] = new GetTransInfoRqTranCodeRow { Value = 57 };
            getTransInfoRqTranCodeRows[26] = new GetTransInfoRqTranCodeRow { Value = 58 };
            getTransInfoRqTranCodeRows[27] = new GetTransInfoRqTranCodeRow { Value = 59 };
            getTransInfoRqTranCodeRows[28] = new GetTransInfoRqTranCodeRow { Value = 60 };
            getTransInfoRqTranCodeRows[29] = new GetTransInfoRqTranCodeRow { Value = 61 };
            getTransInfoRqTranCodeRows[30] = new GetTransInfoRqTranCodeRow { Value = 62 };
            getTransInfoRqTranCodeRows[31] = new GetTransInfoRqTranCodeRow { Value = 63 };
            getTransInfoRqTranCodeRows[32] = new GetTransInfoRqTranCodeRow { Value = 64 };
            getTransInfoRqTranCodeRows[33] = new GetTransInfoRqTranCodeRow { Value = 65 };
            getTransInfoRqTranCodeRows[34] = new GetTransInfoRqTranCodeRow { Value = 66 };
            getTransInfoRqTranCodeRows[35] = new GetTransInfoRqTranCodeRow { Value = 67 };
            getTransInfoRqTranCodeRows[36] = new GetTransInfoRqTranCodeRow { Value = 68 };





            getTransInfoRqTranCodeRows[37] = new GetTransInfoRqTranCodeRow { Value = 69 };
            getTransInfoRqTranCodeRows[38] = new GetTransInfoRqTranCodeRow { Value = 70 };
            getTransInfoRqTranCodeRows[39] = new GetTransInfoRqTranCodeRow { Value = 71 };
            getTransInfoRqTranCodeRows[40] = new GetTransInfoRqTranCodeRow { Value = 72 };
            getTransInfoRqTranCodeRows[41] = new GetTransInfoRqTranCodeRow { Value = 73 };
            getTransInfoRqTranCodeRows[42] = new GetTransInfoRqTranCodeRow { Value = 74 };
            getTransInfoRqTranCodeRows[43] = new GetTransInfoRqTranCodeRow { Value = 75 };
            getTransInfoRqTranCodeRows[44] = new GetTransInfoRqTranCodeRow { Value = 76 };
            getTransInfoRqTranCodeRows[45] = new GetTransInfoRqTranCodeRow { Value = 77 };
            getTransInfoRqTranCodeRows[46] = new GetTransInfoRqTranCodeRow { Value = 78 };
            getTransInfoRqTranCodeRows[47] = new GetTransInfoRqTranCodeRow { Value = 79 };
            getTransInfoRqTranCodeRows[48] = new GetTransInfoRqTranCodeRow { Value = 80 };
            getTransInfoRqTranCodeRows[49] = new GetTransInfoRqTranCodeRow { Value = 81 };
            getTransInfoRqTranCodeRows[50] = new GetTransInfoRqTranCodeRow { Value = 82 };
            getTransInfoRqTranCodeRows[51] = new GetTransInfoRqTranCodeRow { Value = 83 };
            getTransInfoRqTranCodeRows[52] = new GetTransInfoRqTranCodeRow { Value = 84 };

            getTransInfoRqTranCodeRows[53] = new GetTransInfoRqTranCodeRow { Value = 85 };
            getTransInfoRqTranCodeRows[54] = new GetTransInfoRqTranCodeRow { Value = 86 };
            getTransInfoRqTranCodeRows[55] = new GetTransInfoRqTranCodeRow { Value = 87 };
            getTransInfoRqTranCodeRows[56] = new GetTransInfoRqTranCodeRow { Value = 88 };
            getTransInfoRqTranCodeRows[57] = new GetTransInfoRqTranCodeRow { Value = 89 };
            getTransInfoRqTranCodeRows[58] = new GetTransInfoRqTranCodeRow { Value = 90 };
            getTransInfoRqTranCodeRows[59] = new GetTransInfoRqTranCodeRow { Value = 91 };
            getTransInfoRqTranCodeRows[60] = new GetTransInfoRqTranCodeRow { Value = 92 };
            getTransInfoRqTranCodeRows[61] = new GetTransInfoRqTranCodeRow { Value = 93 };
            getTransInfoRqTranCodeRows[62] = new GetTransInfoRqTranCodeRow { Value = 94 };
            getTransInfoRqTranCodeRows[63] = new GetTransInfoRqTranCodeRow { Value = 95 };
            getTransInfoRqTranCodeRows[64] = new GetTransInfoRqTranCodeRow { Value = 96 };
            getTransInfoRqTranCodeRows[65] = new GetTransInfoRqTranCodeRow { Value = 97 };
            getTransInfoRqTranCodeRows[66] = new GetTransInfoRqTranCodeRow { Value = 98 };
            getTransInfoRqTranCodeRows[67] = new GetTransInfoRqTranCodeRow { Value = 99 };
            getTransInfoRqTranCodeRows[68] = new GetTransInfoRqTranCodeRow { Value = 100 };
            getTransInfoRqTranCodeRows[69] = new GetTransInfoRqTranCodeRow { Value = 101 };
            getTransInfoRqTranCodeRows[70] = new GetTransInfoRqTranCodeRow { Value = 102 };
            getTransInfoRqTranCodeRows[71] = new GetTransInfoRqTranCodeRow { Value = 103 };
            getTransInfoRqTranCodeRows[72] = new GetTransInfoRqTranCodeRow { Value = 104 };
            getTransInfoRqTranCodeRows[73] = new GetTransInfoRqTranCodeRow { Value = 105 };


            getTransInfoRqTranCodeRows[74] = new GetTransInfoRqTranCodeRow { Value = 106 };
            getTransInfoRqTranCodeRows[75] = new GetTransInfoRqTranCodeRow { Value = 107 };
            getTransInfoRqTranCodeRows[76] = new GetTransInfoRqTranCodeRow { Value = 108 };
            getTransInfoRqTranCodeRows[76] = new GetTransInfoRqTranCodeRow { Value = 109 };
            getTransInfoRqTranCodeRows[77] = new GetTransInfoRqTranCodeRow { Value = 110 };
            getTransInfoRqTranCodeRows[78] = new GetTransInfoRqTranCodeRow { Value = 111 };
            getTransInfoRqTranCodeRows[79] = new GetTransInfoRqTranCodeRow { Value = 112 };
            getTransInfoRqTranCodeRows[80] = new GetTransInfoRqTranCodeRow { Value = 113 };
            getTransInfoRqTranCodeRows[81] = new GetTransInfoRqTranCodeRow { Value = 114 };
            getTransInfoRqTranCodeRows[82] = new GetTransInfoRqTranCodeRow { Value = 115 };
            getTransInfoRqTranCodeRows[83] = new GetTransInfoRqTranCodeRow { Value = 116 };
            getTransInfoRqTranCodeRows[84] = new GetTransInfoRqTranCodeRow { Value = 117 };
            getTransInfoRqTranCodeRows[85] = new GetTransInfoRqTranCodeRow { Value = 118 };
            getTransInfoRqTranCodeRows[86] = new GetTransInfoRqTranCodeRow { Value = 119 };
            getTransInfoRqTranCodeRows[87] = new GetTransInfoRqTranCodeRow { Value = 120 };
            getTransInfoRqTranCodeRows[88] = new GetTransInfoRqTranCodeRow { Value = 121 };

            getTransInfoRqTranCodeRows[89] = new GetTransInfoRqTranCodeRow { Value = 122 };
            getTransInfoRqTranCodeRows[90] = new GetTransInfoRqTranCodeRow { Value = 123 };
            getTransInfoRqTranCodeRows[91] = new GetTransInfoRqTranCodeRow { Value = 124 };
            getTransInfoRqTranCodeRows[92] = new GetTransInfoRqTranCodeRow { Value = 125 };
            getTransInfoRqTranCodeRows[93] = new GetTransInfoRqTranCodeRow { Value = 126 };
            getTransInfoRqTranCodeRows[94] = new GetTransInfoRqTranCodeRow { Value = 127 };
            getTransInfoRqTranCodeRows[95] = new GetTransInfoRqTranCodeRow { Value = 128 };
            getTransInfoRqTranCodeRows[96] = new GetTransInfoRqTranCodeRow { Value = 129 };
            getTransInfoRqTranCodeRows[97] = new GetTransInfoRqTranCodeRow { Value = 130 };
            getTransInfoRqTranCodeRows[98] = new GetTransInfoRqTranCodeRow { Value = 131 };
            getTransInfoRqTranCodeRows[99] = new GetTransInfoRqTranCodeRow { Value = 132 };
            getTransInfoRqTranCodeRows[100] = new GetTransInfoRqTranCodeRow { Value = 133 };
            getTransInfoRqTranCodeRows[101] = new GetTransInfoRqTranCodeRow { Value = 134 };
            getTransInfoRqTranCodeRows[102] = new GetTransInfoRqTranCodeRow { Value = 135 };
            getTransInfoRqTranCodeRows[103] = new GetTransInfoRqTranCodeRow { Value = 136 };
            getTransInfoRqTranCodeRows[104] = new GetTransInfoRqTranCodeRow { Value = 137 };
            getTransInfoRqTranCodeRows[105] = new GetTransInfoRqTranCodeRow { Value = 138 };
            getTransInfoRqTranCodeRows[106] = new GetTransInfoRqTranCodeRow { Value = 139 };
            getTransInfoRqTranCodeRows[107] = new GetTransInfoRqTranCodeRow { Value = 140 };
            getTransInfoRqTranCodeRows[108] = new GetTransInfoRqTranCodeRow { Value = 141 };
            getTransInfoRqTranCodeRows[109] = new GetTransInfoRqTranCodeRow { Value = 142 };

            getTransInfoRqTranCodeRows[110] = new GetTransInfoRqTranCodeRow { Value = 143 };
            getTransInfoRqTranCodeRows[111] = new GetTransInfoRqTranCodeRow { Value = 144 };
            getTransInfoRqTranCodeRows[112] = new GetTransInfoRqTranCodeRow { Value = 145 };
            getTransInfoRqTranCodeRows[113] = new GetTransInfoRqTranCodeRow { Value = 146 };
            getTransInfoRqTranCodeRows[114] = new GetTransInfoRqTranCodeRow { Value = 147 };
            getTransInfoRqTranCodeRows[115] = new GetTransInfoRqTranCodeRow { Value = 148 };
            getTransInfoRqTranCodeRows[116] = new GetTransInfoRqTranCodeRow { Value = 149 };
            getTransInfoRqTranCodeRows[117] = new GetTransInfoRqTranCodeRow { Value = 150 };
            getTransInfoRqTranCodeRows[118] = new GetTransInfoRqTranCodeRow { Value = 151 };
            getTransInfoRqTranCodeRows[119] = new GetTransInfoRqTranCodeRow { Value = 152 };
            getTransInfoRqTranCodeRows[120] = new GetTransInfoRqTranCodeRow { Value = 153 };
            getTransInfoRqTranCodeRows[121] = new GetTransInfoRqTranCodeRow { Value = 154 };
            getTransInfoRqTranCodeRows[122] = new GetTransInfoRqTranCodeRow { Value = 155 };
            getTransInfoRqTranCodeRows[123] = new GetTransInfoRqTranCodeRow { Value = 156 };
            getTransInfoRqTranCodeRows[124] = new GetTransInfoRqTranCodeRow { Value = 157 };
            getTransInfoRqTranCodeRows[125] = new GetTransInfoRqTranCodeRow { Value = 158 };

            getTransInfoRqTranCodeRows[126] = new GetTransInfoRqTranCodeRow { Value = 159 };
            getTransInfoRqTranCodeRows[127] = new GetTransInfoRqTranCodeRow { Value = 160 };
            getTransInfoRqTranCodeRows[128] = new GetTransInfoRqTranCodeRow { Value = 161 };
            getTransInfoRqTranCodeRows[129] = new GetTransInfoRqTranCodeRow { Value = 162 };
            getTransInfoRqTranCodeRows[130] = new GetTransInfoRqTranCodeRow { Value = 163 };
            getTransInfoRqTranCodeRows[131] = new GetTransInfoRqTranCodeRow { Value = 164 };
            getTransInfoRqTranCodeRows[132] = new GetTransInfoRqTranCodeRow { Value = 165 };
            getTransInfoRqTranCodeRows[133] = new GetTransInfoRqTranCodeRow { Value = 166 };
            getTransInfoRqTranCodeRows[134] = new GetTransInfoRqTranCodeRow { Value = 167 };
            getTransInfoRqTranCodeRows[135] = new GetTransInfoRqTranCodeRow { Value = 168 };
            getTransInfoRqTranCodeRows[136] = new GetTransInfoRqTranCodeRow { Value = 169 };
            getTransInfoRqTranCodeRows[137] = new GetTransInfoRqTranCodeRow { Value = 170 };
            getTransInfoRqTranCodeRows[138] = new GetTransInfoRqTranCodeRow { Value = 171 };
            getTransInfoRqTranCodeRows[139] = new GetTransInfoRqTranCodeRow { Value = 172 };
            getTransInfoRqTranCodeRows[140] = new GetTransInfoRqTranCodeRow { Value = 173 };
            getTransInfoRqTranCodeRows[141] = new GetTransInfoRqTranCodeRow { Value = 174 };
            getTransInfoRqTranCodeRows[142] = new GetTransInfoRqTranCodeRow { Value = 175 };
            getTransInfoRqTranCodeRows[143] = new GetTransInfoRqTranCodeRow { Value = 176 };
            getTransInfoRqTranCodeRows[144] = new GetTransInfoRqTranCodeRow { Value = 177 };
            getTransInfoRqTranCodeRows[145] = new GetTransInfoRqTranCodeRow { Value = 178 };
            getTransInfoRqTranCodeRows[146] = new GetTransInfoRqTranCodeRow { Value = 179 };


            getTransInfoRqTranCodeRows[147] = new GetTransInfoRqTranCodeRow { Value = 180 };
            getTransInfoRqTranCodeRows[148] = new GetTransInfoRqTranCodeRow { Value = 181 };
            getTransInfoRqTranCodeRows[149] = new GetTransInfoRqTranCodeRow { Value = 182 };
            getTransInfoRqTranCodeRows[150] = new GetTransInfoRqTranCodeRow { Value = 183 };
            getTransInfoRqTranCodeRows[151] = new GetTransInfoRqTranCodeRow { Value = 184 };
            getTransInfoRqTranCodeRows[152] = new GetTransInfoRqTranCodeRow { Value = 185 };
            getTransInfoRqTranCodeRows[153] = new GetTransInfoRqTranCodeRow { Value = 186 };
            getTransInfoRqTranCodeRows[154] = new GetTransInfoRqTranCodeRow { Value = 187 };
            getTransInfoRqTranCodeRows[155] = new GetTransInfoRqTranCodeRow { Value = 188 };
            getTransInfoRqTranCodeRows[156] = new GetTransInfoRqTranCodeRow { Value = 189 };
            getTransInfoRqTranCodeRows[157] = new GetTransInfoRqTranCodeRow { Value = 190 };
            getTransInfoRqTranCodeRows[158] = new GetTransInfoRqTranCodeRow { Value = 191 };
            getTransInfoRqTranCodeRows[159] = new GetTransInfoRqTranCodeRow { Value = 192 };
            getTransInfoRqTranCodeRows[160] = new GetTransInfoRqTranCodeRow { Value = 193 };
            getTransInfoRqTranCodeRows[161] = new GetTransInfoRqTranCodeRow { Value = 194 };
            getTransInfoRqTranCodeRows[162] = new GetTransInfoRqTranCodeRow { Value = 195 };
            getTransInfoRqTranCodeRows[163] = new GetTransInfoRqTranCodeRow { Value = 196 };
            getTransInfoRqTranCodeRows[164] = new GetTransInfoRqTranCodeRow { Value = 197 };

            getTransInfoRqTranCodeRows[165] = new GetTransInfoRqTranCodeRow { Value = 201 };
            getTransInfoRqTranCodeRows[166] = new GetTransInfoRqTranCodeRow { Value = 202 };
            getTransInfoRqTranCodeRows[167] = new GetTransInfoRqTranCodeRow { Value = 203 };
            getTransInfoRqTranCodeRows[168] = new GetTransInfoRqTranCodeRow { Value = 204 };
            getTransInfoRqTranCodeRows[169] = new GetTransInfoRqTranCodeRow { Value = 205 };
            getTransInfoRqTranCodeRows[170] = new GetTransInfoRqTranCodeRow { Value = 206 };
            getTransInfoRqTranCodeRows[171] = new GetTransInfoRqTranCodeRow { Value = 207 };
            getTransInfoRqTranCodeRows[172] = new GetTransInfoRqTranCodeRow { Value = 208 };
            getTransInfoRqTranCodeRows[173] = new GetTransInfoRqTranCodeRow { Value = 209 };
            getTransInfoRqTranCodeRows[174] = new GetTransInfoRqTranCodeRow { Value = 210 };
            getTransInfoRqTranCodeRows[175] = new GetTransInfoRqTranCodeRow { Value = 211 };
            getTransInfoRqTranCodeRows[176] = new GetTransInfoRqTranCodeRow { Value = 220 };
            getTransInfoRqTranCodeRows[177] = new GetTransInfoRqTranCodeRow { Value = 221 };
            getTransInfoRqTranCodeRows[178] = new GetTransInfoRqTranCodeRow { Value = 222 };
            getTransInfoRqTranCodeRows[179] = new GetTransInfoRqTranCodeRow { Value = 223 };
            getTransInfoRqTranCodeRows[180] = new GetTransInfoRqTranCodeRow { Value = 224 };
            getTransInfoRqTranCodeRows[181] = new GetTransInfoRqTranCodeRow { Value = 225 };
            getTransInfoRqTranCodeRows[182] = new GetTransInfoRqTranCodeRow { Value = 226 };
            getTransInfoRqTranCodeRows[183] = new GetTransInfoRqTranCodeRow { Value = 230 };

            getTransInfoRqTranCodeRows[184] = new GetTransInfoRqTranCodeRow { Value = 231 };
            getTransInfoRqTranCodeRows[185] = new GetTransInfoRqTranCodeRow { Value = 290 };
            getTransInfoRqTranCodeRows[186] = new GetTransInfoRqTranCodeRow { Value = 291 };
            getTransInfoRqTranCodeRows[187] = new GetTransInfoRqTranCodeRow { Value = 292 };
            getTransInfoRqTranCodeRows[188] = new GetTransInfoRqTranCodeRow { Value = 293 };
            getTransInfoRqTranCodeRows[189] = new GetTransInfoRqTranCodeRow { Value = 294 };
            getTransInfoRqTranCodeRows[190] = new GetTransInfoRqTranCodeRow { Value = 295 };
            getTransInfoRqTranCodeRows[191] = new GetTransInfoRqTranCodeRow { Value = 296 };
            getTransInfoRqTranCodeRows[192] = new GetTransInfoRqTranCodeRow { Value = 297 };
            getTransInfoRqTranCodeRows[193] = new GetTransInfoRqTranCodeRow { Value = 298 };
            getTransInfoRqTranCodeRows[194] = new GetTransInfoRqTranCodeRow { Value = 299 };
            getTransInfoRqTranCodeRows[195] = new GetTransInfoRqTranCodeRow { Value = 529 };
            getTransInfoRqTranCodeRows[196] = new GetTransInfoRqTranCodeRow { Value = 528 };



            GetTransInfoRq transInfo = new GetTransInfoRq()
            {
                Type = new GetTransInfoRqType
                {
                    Row = getTransInfoRqTypeRows
                },

                TranCode = new GetTransInfoRqTranCode
                {
                    Row = getTransInfoRqTranCodeRows
                },

                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                //Count = getTransInfo.Count,
                MBR = getTransInfo.MBR,
                PAN = getTransInfo.PAN,
                FromTime = getTransInfo.FromTime,
                ToTime = getTransInfo.ToTime,
                FromTimeSpecified = true,
                ToTimeSpecified = true,
                NewerTran = 1,
                NewerTranSpecified = true,




                //CountSpecified = true,
                //IssInstName = getTransInfo.IssInstName


            };

            // _defaultVal.LoadDefaults(INTEGRATION_NAME, prepaidReqInfo);


            if (logs != null)
                logs.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            //GetTransInfoRp response = new GetTransInfoRp1();
            //response.Response.TranList.Row = null;
            var response = _fimiService.GetTransactionInfo(new GetTransInfoRq1 { Request = transInfo });


            //var xml = response.ToString();
            //XmlDocument xy = new XmlDocument();
            //xy.LoadXml(xml);

            ////XmlNode isIt =  xy.SelectSingleNode("descendant::Row[Id]");
            //XmlNode isIt = xy.SelectSingleNode("descendant::TranList::Row[Id]");

            //if (isIt!= null) {
            if (response.Response.TranList == null)
            {
                return new List<Transactions>();

                //return new List<Transactions>()
                //                              {
                //                                  new Transactions(){ Code= 0 , Message = ""},

                //                           };
            }
            if (response.Response.TranList.Row != null || (response.Response.TranList.Row.Length > 0))
            {

                logs.Info("AccountDetail() response valid");
                nextChallenge = response.Response.NextChallenge;
                foreach (var row in response.Response.TranList.Row)
                {
                    Transactions tran = new Transactions
                    {

                        CardNumber = Mask(row.PAN),
                        Currency = row.Currency,
                        TranNumber = row.TranNumber,
                        Type = row.Type,
                        Amount = row.Amount,
                        Fee = row.Fee,
                        Time = row.Time,
                        Code = 200,
                        Message = "Success"

                    };
                    if (tran.Amount != 0)
                    {
                        trans.Add(tran);
                    }
                }
            }



            return trans;
        }

        public List<Cards> GetPersonInfo(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, GetPersonInfo personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string cardNumber = "";
            string PAN = "";
            int MBR = 0;
            int PersonId = 0;
            DateTime ExpDate = new DateTime();

            var loggs = FIMILogger.GetFimiLoggerInstance();
            loggs.Info("**********Calling GetPersonInfo Information**********");

            List<Cards> cards = new List<Cards>();

            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = personInfo.InstName,
                IdSpecified = true,
                Id = personInfo.Id,
                Identity = personInfo.identity,
                IdentType = personInfo.identityType,

            };

            //Load Defaults for the header
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);
            if (loggs != null)
                loggs.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });

            //_log.Trace(m => m("Checking GetPersonInfo() response errors"));
            loggs.Info("Checking GetPersonInfo() response errors");


            if (response == null || response.Response == null)
            {
                // _log.Trace(m => m("GetPersonInfo() response is null or false"));
                loggs.Info("GetPersonInfo() response is null or false");
            }
            else
            {
                loggs.Info("GetPersonInfo() response valid");

                nextChallenge = response.Response.NextChallenge;
                foreach (var row in response.Response.Cards.Row)
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

            //personInfo = response;

        }




        //list
        public List<Cards> GetPersonList(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, GetPersonInfo personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string cardNumber = "";
            string PAN = "";
            int MBR = 0;
            int PersonId = 0;
            DateTime ExpDate = new DateTime();

            var loggs = FIMILogger.GetFimiLoggerInstance();
            loggs.Info("**********Calling GetPersonInfo Information**********");

            List<Cards> cards = new List<Cards>();

            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = personInfo.InstName,
                IdSpecified = true,
                Id = personInfo.Id,
                Identity = personInfo.identity,
                IdentType = personInfo.identityType,

            };

            //Load Defaults for the header
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);
            if (loggs != null)
                loggs.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });

            //_log.Trace(m => m("Checking GetPersonInfo() response errors"));
            loggs.Info("Checking GetPersonInfo() response errors");


            if (response == null || response.Response == null)
            {
                // _log.Trace(m => m("GetPersonInfo() response is null or false"));
                loggs.Info("GetPersonInfo() response is null or false");
            }
            else
            {
                loggs.Info("GetPersonInfo() response valid");

                nextChallenge = response.Response.NextChallenge;
                foreach (var row in response.Response.Cards.Row)
                {
                    Cards card = new Cards
                    {
                        CardNumber = Mask(row.PAN),
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

            //personInfo = response;

        }


        public List<Cards> GetPersonListCard(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, GetPersonInfo personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string cardNumber = "";
            string PAN = "";
            int MBR = 0;
            int PersonId = 0;
            DateTime ExpDate = new DateTime();

            var loggs = FIMILogger.GetFimiLoggerInstance();
            loggs.Info("**********Calling GetPersonInfo Information**********");

            List<Cards> cards = new List<Cards>();

            GetPersonInfoRq personInfoReq = new GetPersonInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                InstName = personInfo.InstName,
                IdSpecified = true,
                Id = personInfo.Id,
                Identity = personInfo.identity,
                IdentType = personInfo.identityType,

            };

            //Load Defaults for the header
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);
            if (loggs != null)
                loggs.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });

            //_log.Trace(m => m("Checking GetPersonInfo() response errors"));
            loggs.Info("Checking GetPersonInfo() response errors");


            if (response == null || response.Response == null)
            {
                // _log.Trace(m => m("GetPersonInfo() response is null or false"));
                loggs.Info("GetPersonInfo() response is null or false");
            }
            else
            {
                loggs.Info("GetPersonInfo() response valid");

                nextChallenge = response.Response.NextChallenge;

                //if(response.Response.TranList.Row != null || (response.Response.TranList.Row.Length > 0))
                foreach (var row in response.Response.Cards.Row)
                {
                    Cards card = new Cards
                    {
                        CardNumber = Mask(row.PAN),
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

            //personInfo = response;

        }


        public List<CardDetails> GetCardInfo(int sessionId, string sessionKey, ref string nextChallenge, GetCardInfo cardInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            var logga = FIMILogger.GetFimiLoggerInstance();
            logga.Info("**********Calling GetPersonInfo Information**********");


            string AcctNo = "";
            string AccountUID = "";
            int Status = 0;
            int Currency = 0;
            Decimal AvailBalance = 0;
            String Code = "";

            List<CardDetails> cardDetails = new List<CardDetails>();
            // List<CardDetails> cardDetails = null;

            GetCardInfoRq CardInfo = new GetCardInfoRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = cardInfo.MBR,
                PAN = cardInfo.PAN,

            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, GetCardInforeq);
            if (logga != null)
                logga.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");


            var response = _fimiService.GetCardInfo(new GetCardInfoRq1 { Request = CardInfo });

            logga.Info("Checking GetCardInfo() response errors");
            if (response == null || response.Response == null)
            {

                logga.Info("GetCardInfo() response is null or false");
            }
            else
            {

                logga.Info("GetCardInfo() response valid");
                nextChallenge = response.Response.NextChallenge;
                foreach (var row in response.Response.Accounts.Row)

                {
                    logga.Info($"Person Id before true {response.Response.PersonId}");
                    //response.Response.PersonIdSpecified = true;
                    logga.Info($"Person Id {response.Response.PersonId}");

                    var xmlResponse = ReadNIResponse(row.AccountUID);
                    var accountUID = GetAccountUID(xmlResponse);
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
            }

            return cardDetails;
        }

        private string ReadNIResponse(string accountUID)
        {
            var xmlresponse = string.Empty;
            string filePath = ConfigurationManager.AppSettings.Get("FilePath");
            string path = System.IO.Path.Combine(filePath, $"{accountUID}.txt");
            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                 xmlresponse = reader.ReadToEnd();
                reader.Close();  
                File.Delete(path);
            }
           
            return xmlresponse;

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

        private string GetCardReferenceNumber(string accountUID)
        {
            var accountUIDArray = accountUID.Split(';');

            return accountUIDArray[1];
        }
        private string GetCardStatus(string accountUID)
        {
            var accountUIDArray = accountUID.Split(';');

            return accountUIDArray[2];
        }

        public List<CardDetails> GetCardInfoList(int sessionId, string sessionKey, ref string nextChallenge, GetCardInfo cardInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            var logga = FIMILogger.GetFimiLoggerInstance();
            logga.Info("**********Calling GetPersonInfo Information**********");


            string AcctNo = "";
            string AccountUID = "";
            int Status = 0;
            int Currency = 0;
            Decimal AvailBalance = 0;
            String Code = "";

            //List<CardDetails> cardDetails = new List<CardDetails>();
            List<CardDetails> cardDetails = null;

            GetCardInfoRq CardInfo = new GetCardInfoRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = cardInfo.MBR,
                PAN = cardInfo.PAN,

            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, GetCardInforeq);
            if (logga != null)
                logga.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");


            var response = _fimiService.GetCardInfo(new GetCardInfoRq1 { Request = CardInfo });



            logga.Info("Checking GetCardInfo() response errors");
            if (response == null || response.Response == null)
            {

                logga.Info("GetCardInfo() response is null or false");
            }
            else
            {

                logga.Info("GetCardInfo() response valid");
                nextChallenge = response.Response.NextChallenge;
                foreach (var row in response.Response.Accounts.Row)

                {
                    CardDetails cardDet = new CardDetails
                    {
                        AcctNo = row.AcctNo,

                        Status = row.Status,
                        Currency = row.Currency,
                        AvailBalance = row.AvailBalance,
                        Code = 200,
                        Message = "Success"
                    };
                    cardDetails.Add(cardDet);

                }
            }


            return cardDetails;
        }


        public JObject CreditPrepaidAccount(int sessionId, string sessionKey, ref string nextChallenge, AcctCredit accountInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string Account = "";
            Decimal Amount = 0;

            var loge = FIMILogger.GetFimiLoggerInstance();
            loge.Info("**********Calling CreditPrepaidAccount Information**********");

            List<AccResponse> cardDetails = new List<AccResponse>();

            var result = new
            {

                // ApprovalCode = response.Response.ApprovalCode,
                //AvailBalance = response.Response.AvailBalance,
                Message = "",
                Code = 0,


            };


            AcctCreditRq accreq = new AcctCreditRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountInfo.Account,
                Amount = accountInfo.Amount,
                IgnoreImpact = 1,
                IgnoreImpactSpecified = true

            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, accreq);
            if (loge != null)
                loge.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.CreditPrepaidAccount(new AcctCreditRq1 { Request = accreq });


            loge.Info("Checking AcctCreditRq1() response errors");
            if (response == null || response.Response == null)
            {


                loge.Info("Checking AcctCreditRq1() response is null or false");

            }
            else
            {


                loge.Info("Checking AcctCreditRq1() response valid");

                result = new
                {

                    // ApprovalCode = response.Response.ApprovalCode,
                    //AvailBalance = response.Response.AvailBalance,
                    Message = "Success",
                    Code = 200


                };


            }

            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

            var rt = js.Serialize(result);

            //rt = JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.None });
            rt = rt.Replace(@"\\", "");

            var myCleanJsonObject = JObject.Parse(rt);

            return myCleanJsonObject;
        }

        public JObject DebitPrepaidAccount(int sessionId, string sessionKey, ref string nextChallenge, AcctCredit accountInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            string Account = "";
            Decimal Amount = 0;

            var loge = FIMILogger.GetFimiLoggerInstance();
            loge.Info("**********Calling DebitPrepaidAccount Information**********");

            List<AccResponse> cardDetails = new List<AccResponse>();

            var result = new
            {

                // ApprovalCode = response.Response.ApprovalCode,
                //AvailBalance = response.Response.AvailBalance,
                Message = "",
                Code = 0,


            };


            AcctDebitRq accreq = new AcctDebitRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = accountInfo.Account,
                Amount = accountInfo.Amount,
                IgnoreImpact = 1,
                IgnoreImpactSpecified = true

            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, accreq);
            if (loge != null)
                loge.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.DebitPrepaidAccount(new AcctDebitRq1 { Request = accreq });


            loge.Info("Checking AcctDebitRq1() response errors");
            if (response == null || response.Response == null)
            {


                loge.Info("Checking AcctDebitRq1() response is null or false");

            }
            else
            {


                loge.Info("Checking AcctDebitRq1() response valid");

                result = new
                {

                    // ApprovalCode = response.Response.ApprovalCode,
                    //AvailBalance = response.Response.AvailBalance,
                    Message = "Success",
                    Code = 200


                };


            }

            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

            var rt = js.Serialize(result);

            //rt = JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.None });
            rt = rt.Replace(@"\\", "");

            var myCleanJsonObject = JObject.Parse(rt);

            return myCleanJsonObject;
        }

        public List<Status> GetCardStatus(int sessionId, string sessionKey, ref string nextChallenge, GetCardInfo cardInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));
            var lo = FIMILogger.GetFimiLoggerInstance();
            lo.Info("**********Calling GetCardStatus Information**********");


            string AccountStatus = "";
            string AccountUID = "";
            string AcctNo = "";
            int Status = 0;


            List<Status> cardDetails = new List<Status>();


            GetCardInfoRq CardInfo = new GetCardInfoRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                MBR = cardInfo.MBR,
                PAN = cardInfo.PAN,

            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, GetCardInforeq);

            if (lo != null)
                lo.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPrepaidCardInfo(new GetCardInfoRq1 { Request = CardInfo });



            lo.Info("Checking GetCardInfo() response errors");
            if (response == null || response.Response == null)
            {
                lo.Info("GetCardInfo() response is null or false");

            }
            else
            {

                lo.Info("GetCardInfo() response is valid");

                nextChallenge = response.Response.NextChallenge;


                foreach (var row in response.Response.Accounts.Row)



                {
                    Status cardStat = new Status
                    {
                        AcctNo = row.AcctNo,

                        status = row.Status,

                        Code = 200,

                        Message = "Success",



                    };
                    cardDetails.Add(cardStat);

                }
            }


            return cardDetails;
        }







        public int GetPersonInfo(int id, string instName, int sessionId, string sessionKey, ref string nextChallenge, out GetPersonInfoRp1 personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            var log = FIMILogger.GetFimiLoggerInstance();
            log.Info("**********Calling GetPersonInfo**********");

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
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);


            if (log != null)
                log.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });


            log.Info("Checking GetPersonInfo() response errors");
            if (response == null || response.Response == null)
            {

                log.Info("Checking GetPersonInfo() response is null or false");
            }
            else
            {

                log.Info("GetPersonInfo() response valid");
            }

            nextChallenge = response.Response.NextChallenge;
            personInfo = response;
            return response.Response.Response;
        }

        public int GetPersonInfo(string customerId, IdentificationType idType, string instName, int sessionId, string sessionKey, ref string nextChallenge, out GetPersonInfoRp1 personInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            var loggers = FIMILogger.GetFimiLoggerInstance();
            loggers.Info("**********Calling GetPersonInfo**********");

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
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, personInfoReq);


            if (loggers != null)
                loggers.Info($"SessionId={sessionId}, SessionKey={sessionKey}, NextChallenge={nextChallenge}, Password={nextPwd}");

            var response = _fimiService.GetPersonInfo(new GetPersonInfoRq1 { Request = personInfoReq });


            loggers.Info("Checking GetPersonInfo() response errors");
            if (response == null || response.Response == null)
            {


                loggers.Info("GetPersonInfo() response is null or false");
            }
            else
            {

                loggers.Info("GetPersonInfo() response is valid");
            }

            nextChallenge = response.Response.NextChallenge;
            personInfo = response;
            return response.Response.Response;
        }

        public int UpdatePerson(string inst, int personId, string fullNames, string idNumber, DateTime dob, string address, string postcode, int sessionId, string sessionKey, ref string nextChallenge)
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
                PostalCode = postcode
            };

            //Load Defaults for the header
            //  _defaultVal.LoadDefaults(INTEGRATION_NAME, updatePersonRq);

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

        public int CreditPrepaidAccount(int sessionId, string sessionKey, ref string nextChallenge, decimal amount, string destinationAccountNumber, out AcctCreditRp1 creditInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            AcctCreditRq prepaidReqInfo = new AcctCreditRq()
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                Account = destinationAccountNumber,
                Amount = amount,
                IgnoreImpact = 1,
                IgnoreImpactSpecified = true
            };

            //_defaultVal.LoadDefaults(INTEGRATION_NAME, prepaidReqInfo);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.CreditPrepaidAccount(new AcctCreditRq1 { Request = prepaidReqInfo });

            _log.Trace(m => m("Checking CreditPrepaidAccount() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("CreditPrepaidAccount() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("CreditPrepaidAccount() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            creditInfo = response;
            return response.Response.Response;
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

            // _defaultVal.LoadDefaults(INTEGRATION_NAME, prepaidReqInfo);

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

        public int GetCardInfo(string pan, int mbr, int sessionId, string sessionKey, ref string nextChallenge, out GetCardInfoRp1 cardInfo)
        {
            var nextPwd = TripleDes.Encrypt(sessionKey, Utility.StringToHex(nextChallenge));

            GetCardInfoRq cardInfoReq = new GetCardInfoRq
            {
                Ver = 13.6M,
                Clerk = this._clerk,
                Password = nextPwd,
                SessionSpecified = true,
                Session = sessionId,
                PAN = pan,
                MBR = mbr,
                MBRSpecified = true
            };

            //Load Defaults for the header
            // _defaultVal.LoadDefaults(INTEGRATION_NAME, cardInfoReq);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("SessionId={0}, SessionKey={1}, NextChallenge={2}, Password={3}", sessionId, sessionKey, nextChallenge, nextPwd);
            var response = _fimiService.GetCardInfo(new GetCardInfoRq1 { Request = cardInfoReq });

            _log.Trace(m => m("Checking GetCardInfo() response errors"));
            if (response == null || response.Response == null)
            {
                _log.Trace(m => m("GetCardInfo() response is null or false"));
            }
            else
            {
                _log.Trace(m => m("GetCardInfo() response valid"));
            }

            nextChallenge = response.Response.NextChallenge;
            cardInfo = response;
            return response.Response.Response;
        }
       

        #endregion
    }

    
}
