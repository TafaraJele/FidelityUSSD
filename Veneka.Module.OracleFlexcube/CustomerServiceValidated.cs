using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Veneka.Module.IntegrationDataControl.DAL;
using Veneka.Module.OracleFlexcube.UBS;
using Veneka.Module.OracleFlexcube.UBSCustWebService;
using Veneka.Module.OracleFlexcube.Utils;

namespace Veneka.Module.OracleFlexcube
{
    public sealed class CustomerServiceValidated : ServicesValidated
    {
        #region Constants
        private const string INTEGRATION_NAME = "FCUBSCustService";
        private const string FLEX_ACC_SUCCESS = "ST-SAVE-023";
        #endregion

        #region Readonly Fields
        private readonly UBSCustService _custService;
        #endregion

        #region Properties
        /// <summary>
        /// Set to true if the SSL Certificate is untrusted and you want to service to not throw an exception.
        /// </summary>
        public override bool IgnoreUntrustedSSL
        {
            get
            {
                return _custService.IgnoreUntrustedSSL;
            }
            set
            {
                _custService.IgnoreUntrustedSSL = value;
            }

        }
        #endregion

        #region Constructors
        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString)
              : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, null)
        {
        }
        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString, string logger)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, logger)
        {
        }

        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string username, string password, string nonce, string connectionString, string logger)
        : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, username, password, nonce, connectionString, logger)
        {
        }

        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,
                                string connectionString, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, connectionString, logger)
        {
            _custService = new UBSCustService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);
        }

        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                string connectionString, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, nonce, connectionString, logger)
        {
            _custService = new UBSCustService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);
        }

        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                               Authentication authentication, string username, string password,
                                IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
                               : base(protocol, address, port, path, timeoutMilliSeconds,
                                       authentication, username, password, defaultDataDAL, validationDAL, logger)
        {
            _custService = new UBSCustService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);
        }

        public CustomerServiceValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
                                : base(protocol, address, port, path, timeoutMilliSeconds,
                                        authentication, username, password, nonce, defaultDataDAL, validationDAL, logger)
        {
            _custService = new UBSCustService(General.BuildBindings(protocol, timeoutMilliSeconds),
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

        public bool QueryCustomerDetails(string customerNo, string branchCode,string refNumber, out CustomerFullType custDetails, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To QueryCustomerDetails()"));

            responseMessage = new List<Tuple<string, string>>();
            custDetails = null;
            //Build the Request
            QUERYCUSTOMER_IOFS_REQ request = new QUERYCUSTOMER_IOFS_REQ();

            //Header Section			
            FCUBS_HEADERType header = new FCUBS_HEADERType();

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, header);
            header.MSGID = "CMSN7v1476793681";
            header.CORRELID = "CMSN7v1476793681";
            header.USERID = "ICIUSER";
            header.MODULEID = "CO";
            header.OPERATION = "QueryCustomer";
            header.SOURCE_OPERATION = "QueryCustomer";
            header.SERVICE = "FCUBSCustomerService";
            header.SOURCE_USERID = "ICIUSER";
            header.BRANCH = branchCode;
            header.SOURCE = "FLEXICI";
            

            FCUBS_HEADERTypePARAM[] add = new FCUBS_HEADERTypePARAM[]
            {
                new FCUBS_HEADERTypePARAM
                {
                    NAME="SERVERSTAT",
                    VALUE="HOST"

                }
            };

            header.ADDL = add;
            request.FCUBS_HEADER = header;

            request.FCUBS_BODY = new QUERYCUSTOMER_IOFS_REQFCUBS_BODY
            {
                CustomerIO = new CustomerQueryIOType()
                {
                    CUSTNO = customerNo,

                }
            };
         
            var response = _custService.QueryCustDetails(request);
            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();
      
            if (response.FCUBS_BODY == null)
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_BODY", "Empty response received. Flexcube may be unavailabe, please contact support."));
                validResponse = false;
            }

            //Check Warnings
            _log.Trace(m => m("Checking Response Warnings"));
        
          

            //Check Account Values
            _log.Trace(m => m("Checking Response Body"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.CustomerFull.Custpersonal != null)
            {
                if (response.FCUBS_BODY.CustomerFull.Custpersonal.DOB == null)
                {
                    if (response.FCUBS_BODY.CustomerFull.Custcorp.INCORPDT ==null)
                    {
                        throw new Exception("Date of Birth/Date incorporate not found");
                    }
                  
                }
                custDetails = response.FCUBS_BODY.CustomerFull;
            }
            responseMessage.AddRange(messages);

            _log.Trace(m => m(" QueryCustomerDetails() Done"));

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
