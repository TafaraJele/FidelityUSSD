using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Veneka.Module.IntegrationDataControl.DAL;
using Veneka.Module.OracleFlexcube.UBS;
using Veneka.Module.OracleFlexcube.UBSRTWebService;
using Veneka.Module.OracleFlexcube.Utils;


namespace Veneka.Module.OracleFlexcube
{
    public sealed class MelcomRTServicesValidated : ServicesValidated
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
        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, null)
        {
        }

        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string connectionString, string logger)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, String.Empty, String.Empty, connectionString, logger)
        {
        }
        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, string username, string password, string nonce, string connectionString, string logger)
            : this(protocol, address, port, path, timeoutMilliSeconds, Authentication.NONE, username, password, nonce, connectionString, logger)
        {
        }
        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
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
        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                string connectionString, string logger)
            : base(protocol, address, port, path, timeoutMilliSeconds,
                        authentication, username, password, nonce, connectionString, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);

        }

        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                              Authentication authentication, string username, string password,
                              IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
          : base(protocol, address, port, path, timeoutMilliSeconds,
                      authentication, username, password, defaultDataDAL, validationDAL, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, logger);

        }
        public MelcomRTServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                               IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
            : base(protocol, address, port, path, timeoutMilliSeconds,
                        authentication, username, password, nonce, defaultDataDAL, validationDAL, logger)
        {
            _rtService = new UBSRTService(General.BuildBindings(protocol, timeoutMilliSeconds),
                                            General.BuildEndpointAddress(protocol, address, port, path),
                                            authentication == Authentication.BASIC ? true : false,
                                            username, password, nonce, logger);

        }
        #endregion

        #region Public Methods
        public bool CreateDebitTransactionFS(General.TransactionCode txnPostCode,string username,string lookUpAccNumber, string customerName, string cardNumber, string cardRef, string accountNumber, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To CreateDebitTransactionFS()"));

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
            retailTellerType.PRD = txnPostCode.ToString(); //"GITD";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE= "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName + " " + cardNumber;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " REF:" + transactionReference + Narration + customerName;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + lookUpAccNumber + " "+ username  + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;

            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount, 2);//txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if(response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while debiting the ecash account"));
               
                validResponse = false;
                    
            }

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

                _log.Trace(m => m("Checking Response Errors"));
                if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "eCash account debit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
                referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("CreateDebitTransactionFS() Done"));

            return validResponse;
        }
        public bool CreateDebitTransactionFS(General.TransactionCode txnPostCode,string tellerId, string lookUpAccNumber,string denominations,string customerName, string cardNumber, string cardRef, string accountNumber, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To CreateDebitTransactionFS()"));
            _log.Trace(m => m("CreateDebitTransactionFS --> TellerID" + tellerId));
            _log.Trace(m => m("CreateDebitTransactionFS --> BranchCode" + branchCode));
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
            header.USERID = tellerId.ToUpper();
            header.BRANCH = branchCode;
            header.MODULEID = "RT";
            //FCUBS_HEADERTypePARAM[] param = new FCUBS_HEADERTypePARAM[1];
            //param[0].NAME = "SERVERSTAT";
            //param[0].VALUE = "HOST";
            //header.ADDL = param;

           // request.FCUBS_HEADER = header;


            //Body Section
            RetailTellerTypeFull retailTellerType = new RetailTellerTypeFull();
            _log.Trace(m => m("CreateDebitTransactionFS --> Build transaction details "));

            //Load Defaults for Body				
            _defaultVal.LoadDefaults(INTEGRATION_NAME, retailTellerType);


            retailTellerType.XREF = transactionReference;
            retailTellerType.BRN = branchCode;
            retailTellerType.PRD = txnPostCode.ToString(); //"GITD";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName + " " + cardNumber;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + Narration + customerName + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber);
           // retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " REF:" + transactionReference + Narration + customerName;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + lookUpAccNumber + " " + tellerId + " "  + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference + " ";

            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount,2);
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);


            //code:ccy:value:units|code:ccy:value:units
            string[] strTemp = denominations.Split('|');
           
            foreach (string item in strTemp)
            {
                string[] strTemp2 = item.Split(':');
              
            }

            int numRec = 0;
            for (int i = 0; i < strTemp.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(strTemp[i]))
                {
                    string[] strTemp2 = strTemp[i].Split(':');
                    if (int.Parse(strTemp2[3]) > 0)
                    {
                        numRec++;
                    }
                }
            }
            _log.Trace(m => m("CreateDebitTransactionFS --> num denom to post:" + numRec));
            retailTellerType.DenominationDetails = new DenomDetailsType[numRec];
            bool updateTill = false;

            //DGHS3:10:1|DGHS5:2:1|DGHS6:1:1|
            for (int i = 0; i < strTemp.Length - 1; i++)
            {
                if(!string.IsNullOrEmpty(strTemp[i]))
                {
                    _log.Trace(string.Format("CreateDebitTransactionFS --> next item {0}", strTemp[i]));
                    string[] strTemp2 = strTemp[i].Split(':');
                    _log.Trace(string.Format("CreateDebitTransactionFS --> after splitting {0}:{1}:{2}:{3}", strTemp2[0], strTemp2[1], strTemp2[2], strTemp2[3]));
                    if (int.Parse(strTemp2[3]) > 0)
                    {
                        _log.Debug("found denoms with values");
                        DenomDetailsType tempObj = new DenomDetailsType()
                        {
                            CDDENOM = strTemp2[0],
                            CDCCY = strTemp2[1],
                            VALUE = decimal.Parse(strTemp2[2]),
                            VALUESpecified = true,
                            UNITS = int.Parse(strTemp2[3]),
                            UNITSSpecified = true,
                            DENMVAL = decimal.Parse(strTemp2[2]) * int.Parse(strTemp2[3]),
                            DENMVALSpecified = true,

                        };
                        retailTellerType.DenominationDetails[i] = tempObj;
                        updateTill = true;
                    }
                }
                
            }

            if(updateTill) //ONLY UPDATE THIS VALUE IF ITS A TILL RELATED TRANSACTION
            {
                header.SOURCE = "CMS";
            }

            request.FCUBS_HEADER = header;

            _log.Trace(m => m("CreateDebitTransactionFS --> after preping denominations for transaction"));
            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);
            _log.Trace(m => m("CreateDebitTransactionFS --> after calling _rtService.CreateTransactionFS(request)"));
            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while debiting the ecash account"));

                validResponse = false;

            }

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

            //if (response.FCUBS_BODY != null && response.FCUBS_BODY.TransactionDetails != null &&
            //    !String.IsNullOrWhiteSpace(response.FCUBS_BODY.TransactionDetails.FCCREF))
            if (response.FCUBS_BODY != null)
            {

                _log.Trace(m => m("Checking Response Errors"));

                if(response.FCUBS_BODY.TransactionDetails != null &&
                !String.IsNullOrWhiteSpace(response.FCUBS_BODY.TransactionDetails.FCCREF))
                {
                    referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
                }

                if (response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "Account debit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
                
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("CreateDebitTransactionFS() Done"));

            return validResponse;
        }

        public bool CreateCreditTransactionFS(General.TransactionCode txnPostCode, string customerName, string cardNumber, string accountNumber, string cardRef, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To CreateCreditTransactionFS()"));

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
            retailTellerType.PRD = txnPostCode.ToString();//"GITC";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName +" " + cardNumber;  
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + Narration + customerName + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber); ;
           // retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " REF:" + transactionReference + Narration + customerName;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber)  + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;
            _log.Trace(m => m("inside CreateCreditTransactionFS() --> accountNumber : " + accountNumber));
            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount, 2); //txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while crediting the account"));

                validResponse = false;

            }

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
                _log.Trace(m => m("Checking Response Errors"));
                if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "Fidelity Income account credit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
                referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryCustomerAccountValidated() Done"));

            return validResponse;
        }

        public bool CreateCreditTransactionFS(General.TransactionCode txnPostCode, string username, string lookUpAccNumber, string customerName, string cardNumber, string accountNumber, string cardRef, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
        {
            AddUntrustedSSL();
            _log.Trace(m => m("Call To CreateCreditTransactionFS()"));

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
            retailTellerType.PRD = txnPostCode.ToString();//"GITC";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName +" " + cardNumber;  
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + Narration + customerName + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber); ;
            // retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " REF:" + transactionReference + Narration + customerName;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + lookUpAccNumber + " " + username + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;
            _log.Trace(m => m("inside CreateCreditTransactionFS() --> accountNumber : " + accountNumber));
            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount, 2); //txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while crediting the account"));

                validResponse = false;

            }

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
                _log.Trace(m => m("Checking Response Errors"));
                if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "Fidelity Income account credit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
                referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryCustomerAccountValidated() Done"));

            return validResponse;
        }

        public bool CreateCreditTransactionFS(General.TransactionCode txnPostCode, string denominations,string customerName, string cardNumber,string cardRef, string accountNumber, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
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
            retailTellerType.PRD = txnPostCode.ToString();//"GITC";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName + " " + cardNumber;
           // retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + Narration + customerName + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber); ;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber)  + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;
            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount, 2);//txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while crediting the account"));

                validResponse = false;

            }

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
                _log.Trace(m => m("Checking Response Errors"));
                if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "Fidelity Income account credit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
                referenceNumber = response.FCUBS_BODY.TransactionDetails.FCCREF;
            }

            responseMessage.AddRange(messages);

            _log.Trace(m => m("QueryCustomerAccountValidated() Done"));

            return validResponse;
        }
        public bool CreateCreditTransactionFS(General.TransactionCode txnPostCode, string tellerId, string lookUpAccNumber, string denominations, string customerName, string cardNumber, string cardRef, string accountNumber, string branchCode, string domicilebranch, string transactionReference, string Narration, string ccy, decimal txnAmount, string MessageId, out string referenceNumber, out List<Tuple<string, string>> responseMessage)
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
            retailTellerType.PRD = txnPostCode.ToString();//"GITC";
            retailTellerType.TXNAMTSpecified = true;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + "VISA PREPAID CARD LOAD B/O" + customerName + " " + cardNumber;
            // retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + "REF:" + transactionReference + Narration + customerName + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber); ;
            //retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference;
            retailTellerType.NARRATIVE = "DD" + DateTime.Now.ToString() + " " + Veneka.Module.OracleFlexcube.Utils.General.MaskPAN(cardNumber) + " " + lookUpAccNumber + " " + tellerId + " " + Narration + " " + customerName + " " + cardRef + " REF:" + transactionReference + " ";
            retailTellerType.TXNBRN = domicilebranch;
            retailTellerType.TXNACC = accountNumber;
            retailTellerType.TXNCCY = ccy;
            retailTellerType.TXNAMT = Math.Round(txnAmount, 2);//txnAmount;
            retailTellerType.TXNDATE = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            request.FCUBS_BODY = new CREATETRANSACTION_FSFS_REQFCUBS_BODY
            {
                TransactionDetails = retailTellerType
            };

            var response = _rtService.CreateTransactionFS(request);

            bool validResponse = true;
            List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

            if (response.FCUBS_HEADER.MSGSTAT.ToString().ToUpper().Equals("FAILURE"))
            {
                messages.Add(Tuple.Create<string, string>("FCUBS_HEADER", "Failure response received while crediting the account"));

                validResponse = false;

            }

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
                _log.Trace(m => m("Checking Response Errors"));
                if (response.FCUBS_BODY != null && response.FCUBS_BODY.FCUBS_ERROR_RESP != null)
                {
                    messages.Add(Tuple.Create<string, string>("FCUBS_ERROR_RESP", "Fidelity Income account credit failed, please contact support."));
                    validResponse = false;
                    //ValidateErrors(INTEGRATION_NAME, response.FCUBS_BODY.FCUBS_ERROR_RESP, ref validResponse, ref responseMessage);
                }
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
