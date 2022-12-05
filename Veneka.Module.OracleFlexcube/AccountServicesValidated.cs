using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.OracleFlexcube.UBS;
using Veneka.Module.OracleFlexcube.UBSAccWebService;
using Veneka.Module.OracleFlexcube.Utils;
using Common.Logging;
using Veneka.Module.IntegrationDataControl.DAL;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace Veneka.Module.OracleFlexcube
{
    /// <summary>
    /// This class provides methods which will populate any default values and check for a valid response from Flexcube.
    /// </summary>
    public sealed class AccountServicesValidated : ServicesValidated
    {
        #region Constants
        private const string INTEGRATION_NAME = "FCUBSAccService";
        private const string FLEX_ACC_SUCCESS = "ST-SAVE-023";
        #endregion

        #region Readonly Fields
        private readonly UBSAccService _accService;
        #endregion

        #region Properties
        /// <summary>
        /// Set to true if the SSL Certificate is untrusted and you want to service to not throw an exception.
        /// </summary>
        public override bool IgnoreUntrustedSSL
        {
            get
            {
                return _accService.IgnoreUntrustedSSL;
            }
            set
            {
                _accService.IgnoreUntrustedSSL = value;
            }
        }
        #endregion

        #region Constructors
        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, null)
        {
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString, string logger)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, logger)
        {
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string username, string password, string nonce, string connectionString, string logger)
        : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, username, password, nonce, connectionString, logger)
        {
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,
                                string connectionString, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, connectionString, logger)
        {
            _accService = new UBSAccService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                string connectionString, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, nonce, connectionString, logger)
        {
            _accService = new UBSAccService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                               Authentication authentication, string username, string password,
                                IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
                               : base(protocol, address, port, path, timeoutMilliSeconds,
                                       authentication, username, password, defaultDataDAL, validationDAL, logger)
        {
            _accService = new UBSAccService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);
        }

        public AccountServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, nonce, defaultDataDAL, validationDAL, logger)
        {
            _accService = new UBSAccService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Query a customer account.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="branchCode"></param>
        /// <param name="custAccount"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public bool QueryCustomerAccount(string accountNumber, string branchCode, string refNumber, out CustAccountFullType custAccount, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
           
            _log.Trace(m => m("Call To QueryCustomerAccountValidated()"));

            responseMessage = new List<Tuple<string, string>>();
            custAccount = null;

            //Build the Request
            QUERYCUSTACC_IOFS_REQ request = new QUERYCUSTACC_IOFS_REQ();
            
            //Header Section			
            FCUBS_HEADERType header = new FCUBS_HEADERType();

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, header);

            header.BRANCH = branchCode;
            header.OPERATION = "QueryCustAcc";
            header.SOURCE_OPERATION = "QueryCustAcc";
            header.MSGID = refNumber;
            header.CORRELID = refNumber;
            header.SOURCE_USERID = "ICIUSER";

            request.FCUBS_HEADER = header;

            //Body Section
            request.FCUBS_BODY = new QUERYCUSTACC_IOFS_REQFCUBS_BODY
            {
                CustAccountIO = new CustAccountQueryIOType
                {
                    ACC = accountNumber,
                    BRN = branchCode
                }
            };

            var response = _accService.QueryCustAcc(request);
            
            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_BODY == null)
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_BODY", "Empty response received. Flexcube may be unavailabe, please contact support."));
                validResponse = false;
            }

            //Check Warnings
            _log.Trace(m => m("Checking Response Warnings"));
            //         if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_WARNING_RESP != null)
            //{
            //             //_log.Debug("Integration Name=" + INTEGRATION_NAME);
            //             // ValidateWarnings(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_WARNING_RESP, ref validResponse, ref responseMessage);

            //         }

            //Check Errors
            _log.Trace(m => m("Checking Response Errors"));
            //         if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
            //{
            //	ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
            //}

            //Check Account Values
            _log.Trace(m => m("Checking Response Body"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.CustAccountFull != null)
            {
                //if (!_validateData.Validate(INTEGRATION_NAME, response.FCUBS_BODY.CustAccountFull, LanguageId, out messages))
                //	validResponse = false;
                //if (response.FCUBS_BODY.CustAccountFull.ADDR1 == null)
                //{
                //    throw new Exception("Address is not found");

                //}
                //if (response.FCUBS_BODY.CustAccountFull.ADDR2 == null)
                //{
                //    throw new Exception("Address2 is not found");

                //}
                //if (response.FCUBS_BODY.CustAccountFull.ADDR3 == null)
                //{
                //    throw new Exception("Address3 is not found");

                //}
                if (response.FCUBS_BODY.CustAccountFull.CUSTNAME == null)
                {
                    throw new Exception("customer name is not found");

                }
                if (response.FCUBS_BODY.CustAccountFull.CCY == null)
                {
                    throw new Exception("currecny is empty");

                }

                custAccount = response.FCUBS_BODY.CustAccountFull;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryCustomerAccountValidated() Done"));

            return validResponse;
        }

        /// <summary>
        /// Query a customer account for latest account balances.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="branchCode"></param>
        /// <param name="accountBalances"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public bool QueryAccountBalance(string accountNumber, string branchCode,string refNumber, out AccBalRestypeACC_BAL accountBalances, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
           
            _log.Trace(m => m("Call To QueryAccountBalanceValidated()"));

            responseMessage = new List<Tuple<string, string>>();
            accountBalances = null;

            //Build the Request
            QUERYACCBAL_IOFS_REQ request = new QUERYACCBAL_IOFS_REQ();

            //Header Section			
            FCUBS_HEADERType header = new FCUBS_HEADERType();

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, header);

            header.BRANCH = branchCode;
            header.OPERATION = "QueryAccBal";
            header.SOURCE_OPERATION = "QueryAccBal";
            header.MSGID = refNumber;
            header.CORRELID = refNumber;
            header.SOURCE_USERID = "ICIUSER";

            request.FCUBS_HEADER = header;

            //Body Section
            request.FCUBS_BODY = new QUERYACCBAL_IOFS_REQFCUBS_BODY
            {
                ACCBalance = new AccBalReqtype
                {
                    ACC_BAL = new AccBalReqtypeACC_BAL
                    {
                        BRANCH_CODE = branchCode,
                        CUST_AC_NO = accountNumber
                    }
                }
            };

            var response = _accService.QueryAccBal(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_BODY == null)
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_BODY", "Empty response received. Flexcube may be unavailabe, please contact support."));
                validResponse = false;
            }

            //Check Warnings
            //_log.Trace(m => m("Checking Response Warnings"));
            //         if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_WARNING_RESP != null) 
            //{
            //	ValidateWarnings(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_WARNING_RESP, ref validResponse, ref responseMessage);
            //}

            ////Check Errors
            //_log.Trace(m => m("Checking Response Errors"));
            //         if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
            //{
            //	ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
            //}

            //NOTE: ACCBalance should return only 1 item in the array. Never more.

            //Check Account Values
            _log.Trace(m => m("Checking Response Body"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.ACCBalance != null && response.FCUBS_BODY.ACCBalance.Length > 0)
            {
                //if (!_validateData.Validate(INTEGRATION_NAME, response.FCUBS_BODY.ACCBalance[0], LanguageId, out messages))
                //	validResponse = false;				

                accountBalances = response.FCUBS_BODY.ACCBalance[0];
            }


            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryAccountBalanceValidated() Done"));

            return validResponse;
        }

        public bool ModifyCustomerAccount(CustAccountFullType custAccount, string branchCode, string MessageId, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To ModifyCustomerAccount()"));

            responseMessage = new List<Tuple<string, string>>();

            //Build the Request
            MODIFYCUSTACC_FSFS_REQ request = new MODIFYCUSTACC_FSFS_REQ();

            //Header Section			
            FCUBS_HEADERType header = new FCUBS_HEADERType();

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, header);

            header.BRANCH = branchCode;
            header.OPERATION = "ModifyCustAcc";
            header.SOURCE_OPERATION = "ModifyCustAcc";
            header.MSGID = MessageId;
            header.CORRELID = MessageId;
            header.SOURCE_USERID = "ICIUSER";
            List<FCUBS_HEADERTypePARAM> _listparam = new List<FCUBS_HEADERTypePARAM>();
            FCUBS_HEADERTypePARAM param = new FCUBS_HEADERTypePARAM();
            param.NAME = "HOST";
            param.VALUE = "SERVERSTAT";
            header.ADDL = _listparam.ToArray();
            header.MODULEID = "RT";
            request.FCUBS_HEADER = header;

            //Body Section
            request.FCUBS_BODY = new MODIFYCUSTACC_FSFS_REQFCUBS_BODY
            {
                CustAccountFull = custAccount
            };

            var response = _accService.ModifyCustAccFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_BODY == null)
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_BODY", "Empty response received. Flexcube may be unavailabe, please contact support."));
                validResponse = false;
            }

            //Check Warnings
            _log.Trace(m => m("Checking Response Warnings"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_WARNING_RESP != null)
            {
                ValidateWarnings(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_WARNING_RESP, ref validResponse, ref responseMessage);
            }

            //Check Errors
            _log.Trace(m => m("Checking Response Errors"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
            {
                ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("ModifyCustomerAccount() Done"));

            return validResponse;
        }
        #endregion

        public void AddUntrustedSSL()
        {


            //  if (IgnoreUntrustedSSL)
            {

                ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
                                                        X509Chain chain,
                                                        SslPolicyErrors sslPolicyErrors) => true;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            }
            //  if (_log.IsDebugEnabled)
            _log.Debug("Protocol used is =" + ServicePointManager.SecurityProtocol);

        }
    }
}
