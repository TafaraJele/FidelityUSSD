using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.IntegrationDataControl.DAL;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;

namespace Veneka.Module.TranzwareCompassPlusFIMI
{
	public abstract class ServicesValidated
	{
		#region Static Enumerations
		
        public enum Protocol { HTTP, HTTPS}
		public enum Authentication { NONE, BASIC }
		
        #endregion

		#region Readonly Fields
		
        protected readonly DefaultDataControl _defaultVal;

        #endregion

        #region Private Fields

        protected static ILog _log = LogManager.GetLogger(Utils.General.MODULE_LOGGER);
       
        #endregion

        #region Properties

        /// <summary>
        /// Set to true if the SSL Certificate is untrusted and you want to service to not throw an exception.
        /// </summary>
        public abstract bool IgnoreUntrustedSSL { get; set; }

		/// <summary>
		/// Set the language ID that you want the response message to be in. 
		/// This ID will be the same as what was configured in the database.
		/// </summary>
		public int LanguageId { get; set; }

		#endregion

		#region Constructors
        private ServicesValidated (Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                    Authentication authentication, string username, string password, string logger)
        {
            if (!String.IsNullOrWhiteSpace(logger))
                _log = LogManager.GetLogger(logger);

            _log.Trace(m => m("AccountService Starting"));
            if (_log.IsDebugEnabled)
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.Append("Connection information...")
                         .Append(Environment.NewLine)
                         .AppendFormat("Protocol:{0}", protocol)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Address:{0}", address)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Port:{0}", port)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Path:{0}", path)
                         .Append(Environment.NewLine)
                         .AppendFormat(" TimeoutMilliSeconds:{0}ms", timeoutMilliSeconds)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Authentication:{0}", authentication)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Username:{0}", username)
                         .Append(Environment.NewLine)
                         //.AppendFormat(" Password:\t{0}\n", password.su)
                         //.AppendFormat(" ConnectionString:{0}", connectionString)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Logger:{0}", logger);

                _log.Debug(debugInfo.ToString());
            }
        }
        
        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, 
								Authentication authentication, string username, string password,
								string connectionString, string logger) :this(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger)
		{
			_defaultVal = new DefaultDataControl(connectionString, null);
		}
        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                               Authentication authentication, string username, string password,
                               IDefaultDataDAL dal, string logger) : this(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger)
        {
            _defaultVal = new DefaultDataControl(dal, null);
        }

        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,
                                FileInfo fileInfo, string logger) : this(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password, logger)
        {
            _defaultVal = new DefaultDataControl(fileInfo, null);
        }

        #endregion

        #region Protected Methods

        //protected void ValidateStatusObj(StatusObj statusObj, string integrationName, ref bool validResponse, ref List<Tuple<string, string>> responseMessages)
        //{
        //    List<Tuple<string, string>> messages;
        //    if (!_validateData.Validate(integrationName, statusObj, LanguageId, out messages))
        //        validResponse = false;

        //    responseMessages.AddRange(messages);
        //}

        /// <summary>
        /// Validates the Errors contained in the response from flexcube.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="errors"></param>
        /// <param name="validResponse"></param>
        /// <param name="messages"></param>
        //protected void ValidateErrors(string integrationName, FIMIService.ERRORType[] errors, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
        //{
        //    for (int x = 0; x < errors.Length; x++)
        //    {
        //        for (int y = 0; y < errors[x].ERROR.Length; y++)
        //        {
        //            List<Tuple<string, string>> messages;
        //            if (!_validateData.Validate(integrationName, errors[x].ERROR[y], LanguageId, out messages))
        //                validResponse = false;
        //            responseMessage.AddRange(messages);
        //        }				
        //    }
        //}

        //protected void ValidateErrors(string integrationName, UBSRTWebService.ERRORDETAILSType[] errors, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
        //{           
        //    foreach (var error in errors)
        //    {
        //        List<Tuple<string, string>> messages;
        //        if (!_validateData.Validate(integrationName, error, LanguageId, out messages))
        //            validResponse = false;
        //        responseMessage.AddRange(messages);
        //    }
        //}

        /// <summary>
        /// Validates Warning Messages contained in the resonse from flexcube
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="warnings"></param>
        /// <param name="validResponse"></param>
        /// <param name="messages"></param>
        //protected void ValidateWarnings(string integrationName, UBSAccWebService.WARNINGType[] warnings, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
        //{
        //    for (int x = 0; x < warnings.Length; x++)
        //    {
        //        for (int y = 0; y < warnings[x].WARNING.Length; y++)
        //        {
        //            List<Tuple<string, string>> messages;

        //            if (!_validateData.Validate(integrationName, warnings[x].WARNING[y], LanguageId, out messages))
        //                validResponse = false;
        //            responseMessage.AddRange(messages);
        //        }				
        //    }
        //}

        //protected void ValidateWarnings(string integrationName, UBSRTWebService.WARNINGDETAILSType[] warnings, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
        //{
        //    foreach(var warning in warnings)
        //    {
        //        List<Tuple<string, string>> messages;

        //        if (!_validateData.Validate(integrationName, warning, LanguageId, out messages))
        //            validResponse = false;
        //        responseMessage.AddRange(messages);
        //    }
        //}

        #endregion
    }
}
