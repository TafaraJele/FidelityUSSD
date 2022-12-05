using Common.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.IntegrationDataControl.DAL;
using Veneka.Module.OracleFlexcube.UBS;
using Veneka.Module.OracleFlexcube.UBSRTWebService;
using Veneka.Module.OracleFlexcube.Utils;

namespace Veneka.Module.OracleFlexcube
{
	public sealed class RTServicesValidated : ServicesValidated
	{
		#region Constants
		private const string INTEGRATION_NAME = "FCUBSRTService";
		#endregion

		#region Readonly Fields
		private readonly UBSRTService _rtService;
		#endregion

		#region Properties
		/// <summary>
		/// Set to true if the SSL Certificate is untrusted and you want to service to not throw an exception.
		/// </summary>
		public override bool IgnoreUntrustedSSL 
		{
			get
			{
				return _rtService.IgnoreUntrustedSSL;
			}
			set
			{
				_rtService.IgnoreUntrustedSSL = value;
			}
		}
		#endregion

		#region Constructors
		public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString)
			: this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, null)
		{
		}

		public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString, string logger)
			: this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, logger)
		{
		}
        public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,string username,string password,string nonce, string connectionString, string logger)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, username,password,nonce, connectionString, logger)
        {
        }
        public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, 
								Authentication authentication, string username, string password,
								string connectionString, string logger)
			: base(protocol, address, port, path, timeoutMilliSeconds,
						authentication, username, password, connectionString, logger)
		{
			_rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
											General.BuildEndpointAddress(protocol, address, port, path),
											authentication == Authentication.BASIC ? true : false,
											username, password, logger);

		}
        public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,string nonce,
                                string connectionString, string logger)
            : base(protocol, address, port, path, timeoutMilliSeconds,
                        authentication, username, password, nonce,connectionString, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password,nonce, logger);

        }

        public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                              Authentication authentication, string username, string password,
                              IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
          : base(protocol, address, port, path, timeoutMilliSeconds,
                      authentication, username, password, defaultDataDAL,validationDAL, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);

        }
        public RTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                               IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
            : base(protocol, address, port, path, timeoutMilliSeconds,
                        authentication, username, password, nonce, defaultDataDAL,validationDAL, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);

        }
        #endregion

        #region Public Methods
        public bool CreateTransactionIO(string accountNumber, string branchCode, string transactionReference, string ccy, decimal txnAmount,string  MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
		{
            AddUntrustedSSL();

            _log.Trace(m => m("Call To QueryCustomerAccountValidated()"));

			referenceNumber = String.Empty;
			
			responseMessage = new List<Tuple<string, string>>();

			//Build the Request
			CREATETRANSACTION_IOPK_REQ request = new CREATETRANSACTION_IOPK_REQ();

			//Header Section			
			FCUBS_HEADERType header = new FCUBS_HEADERType();

			//Load Defaults for the header
			_defaultVal.LoadDefaults(INTEGRATION_NAME, header);
            header.OPERATION = "CreateTransaction";
            header.SOURCE_OPERATION = "CreateTransaction";
            header.MSGID = MessageId;
            header.CORRELID = MessageId;
            header.SOURCE_USERID = "ICIUSER";
            header.BRANCH = branchCode;
			request.FCUBS_HEADER = header;

			//Body Section
			RetailTellerTypeIO retailTellerType = new RetailTellerTypeIO();

			//Load Defaults for Body				
			_defaultVal.LoadDefaults(INTEGRATION_NAME, retailTellerType);

           
			retailTellerType.XREF = transactionReference;
			retailTellerType.BRN = branchCode;
            retailTellerType.PRD = "VICI";
            retailTellerType.TXNAMTSpecified = true;
			retailTellerType.TXNBRN = branchCode;
			retailTellerType.TXNACC = accountNumber;
			retailTellerType.TXNCCY = ccy;
			retailTellerType.TXNAMT = txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			
			request.FCUBS_BODY = new CREATETRANSACTION_IOPK_REQFCUBS_BODY
			{
				TransactionDetailsIO = retailTellerType		
			};
			
			var response = _rtService.CreateTransactionIO(request);

			bool validResponse = true;
			List<Tuple<string, string>> messages = new List<Tuple<string,string>>();

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

			////Check Errors
			_log.Trace(m => m("Checking Response Errors"));
            if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
			{
				ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
			}

            if (response.FCUBS_BODY != null && response.FCUBS_BODY.TransactionDetailsPK != null &&
				!String.IsNullOrWhiteSpace(response.FCUBS_BODY.TransactionDetailsPK.FCCREF))
			{
                if (!_validateData.Validate(INTEGRATION_NAME, response.FCUBS_BODY.TransactionDetailsPK, LanguageId, out messages))
                    validResponse = false;	

				referenceNumber = response.FCUBS_BODY.TransactionDetailsPK.FCCREF;
			}

            responseMessage.AddRange(messages);

			_log.Trace(m => m("QueryCustomerAccountValidated() Done"));

			return validResponse;
		}

        public bool CreateTransactionFS(string accountNumber, string branchCode,string domicilebranch, string transactionReference, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To QueryCustomerAccountValidated()"));

            referenceNumber = String.Empty;

            responseMessage = new List<Tuple<string, string>>();

            //Build the Request
            CREATETRANSACTION_FSFS_REQ request = new CREATETRANSACTION_FSFS_REQ();

            //Header Section			
            FCUBS_HEADERType header = new FCUBS_HEADERType();

            //Load Defaults for the header
            _defaultVal.LoadDefaults(INTEGRATION_NAME, header);
            header.OPERATION = "CreateTransaction";
            header.SOURCE_OPERATION = "CreateTransaction";
            header.MSGID = MessageId;
            header.CORRELID = MessageId;
            header.SOURCE_USERID = "ICIUSER";
            header.BRANCH = branchCode;
            request.FCUBS_HEADER = header;

            //Body Section
            RetailTellerTypeFull retailTellerType = new RetailTellerTypeFull();

            //Load Defaults for Body				
            _defaultVal.LoadDefaults(INTEGRATION_NAME, retailTellerType);


            retailTellerType.XREF = transactionReference;
            retailTellerType.BRN = branchCode;
            retailTellerType.PRD = "VICI";
            retailTellerType.TXNAMTSpecified = true;
            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_BODY == null)
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_BODY", "Empty response received. Flexcube may be unavailabe, please contact support."));
                validResponse = false;
            }

            //Check Warnings
            //_log.Trace(m => m("Checking Response Warnings"));
            //if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_WARNING_RESP != null)
            //{
            //    ValidateWarnings(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_WARNING_RESP, ref validResponse, ref responseMessage);
            //}

            ////Check Errors
            //_log.Trace(m => m("Checking Response Errors"));
            //if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
            //{
            //    ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
            //}

            if (response.FCUBS_BODY != null && response.FCUBS_BODY.TransactionDetails != null &&
                !String.IsNullOrWhiteSpace(response.FCUBS_BODY.TransactionDetails.FCCREF))
            {
                //if (!_validateData.Validate(INTEGRATION_NAME, response.FCUBS_BODY.TransactionDetails, LanguageId, out messages))
                //    validResponse = false;

                referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryCustomerAccountValidated() Done"));

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
                ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls12;

            }
            //  if (_log.IsDebugEnabled)
            _log.Debug("Protocol used is =" + ServicePointManager.SecurityProtocol);

        }
    }
}
