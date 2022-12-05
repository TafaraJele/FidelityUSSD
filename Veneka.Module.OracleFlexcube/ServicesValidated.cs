using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.IntegrationDataControl.DAL;

namespace Veneka.Module.OracleFlexcube
{
	public abstract class ServicesValidated
	{
		#region Static Enumerations
		public enum Protocol { HTTP, HTTPS }
		public enum Authentication { NONE, BASIC }
		#endregion

		#region Readonly Fields
		protected readonly DefaultDataControl _defaultVal;
		protected readonly ValidateDataControl _validateData;
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
		protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, 
								Authentication authentication, string username, string password,
								string connectionString, string logger)
		{
			if(!String.IsNullOrWhiteSpace(logger))
				_log = LogManager.GetLogger(logger);

			_log.Trace(m => m("AccountService Starting"));
			if (_log.IsDebugEnabled)
			{
				StringBuilder debugInfo = new StringBuilder();
                debugInfo.AppendFormat("Protocol:\t{0}", protocol)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Address:\t{0}", address)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Port:\t{0}", port)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Path:\t{0}", path)
                         .Append(Environment.NewLine)
						 .AppendFormat(" TimeoutMilliSeconds:\t{0}", timeoutMilliSeconds)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Authentication:\t{0}", authentication)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Username:\t{0}", username)
                         .Append(Environment.NewLine)
						 //.AppendFormat(" Password:\t{0}\n", password.su)
						 .AppendFormat(" ConnectionString:\t{0}", connectionString)
                         .Append(Environment.NewLine)
						 .AppendFormat(" Logger:\t{0}", logger);

				_log.Debug(debugInfo.ToString());
			}

			_defaultVal = new DefaultDataControl(connectionString,null);
			_validateData = new ValidateDataControl(connectionString);
		}
        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,
                                IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
        {
            if (!String.IsNullOrWhiteSpace(logger))
                _log = LogManager.GetLogger(logger);

            _log.Trace(m => m("AccountService Starting"));
            if (_log.IsDebugEnabled)
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.AppendFormat("Protocol:\t{0}", protocol)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Address:\t{0}", address)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Port:\t{0}", port)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Path:\t{0}", path)
                         .Append(Environment.NewLine)
                         .AppendFormat(" TimeoutMilliSeconds:\t{0}", timeoutMilliSeconds)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Authentication:\t{0}", authentication)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Username:\t{0}", username)
                         .Append(Environment.NewLine)
                         //.AppendFormat(" Password:\t{0}\n", password.su)
                         // .AppendFormat(" ConnectionString:\t{0}", connectionString)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Logger:\t{0}", logger);

                _log.Debug(debugInfo.ToString());
            }

            _defaultVal = new DefaultDataControl(defaultDataDAL, null);
            _validateData = new ValidateDataControl(validationDAL);
        }
        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password,string nonce,
                                 IDefaultDataDAL defaultDataDAL, IValidationDAL validationDAL, string logger)
        {
            if (!String.IsNullOrWhiteSpace(logger))
                _log = LogManager.GetLogger(logger);

            _log.Trace(m => m("AccountService Starting"));
            if (_log.IsDebugEnabled)
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.AppendFormat("Protocol:\t{0}", protocol)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Address:\t{0}", address)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Port:\t{0}", port)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Path:\t{0}", path)
                         .Append(Environment.NewLine)
                         .AppendFormat(" TimeoutMilliSeconds:\t{0}", timeoutMilliSeconds)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Authentication:\t{0}", authentication)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Username:\t{0}", username)
                         .Append(Environment.NewLine)
                         //.AppendFormat(" Password:\t{0}\n", password.su)
                        // .AppendFormat(" ConnectionString:\t{0}", connectionString)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Logger:\t{0}", logger);

                _log.Debug(debugInfo.ToString());
            }

            _defaultVal = new DefaultDataControl(defaultDataDAL, null);
            _validateData = new ValidateDataControl(validationDAL);
        }


        protected ServicesValidated(Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds,
                                Authentication authentication, string username, string password, string nonce,
                                string connectionString, string logger)
        {
            if (!String.IsNullOrWhiteSpace(logger))
                _log = LogManager.GetLogger(logger);

            _log.Trace(m => m("AccountService Starting"));
            if (_log.IsDebugEnabled)
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.AppendFormat("Protocol:\t{0}", protocol)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Address:\t{0}", address)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Port:\t{0}", port)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Path:\t{0}", path)
                         .Append(Environment.NewLine)
                         .AppendFormat(" TimeoutMilliSeconds:\t{0}", timeoutMilliSeconds)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Authentication:\t{0}", authentication)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Username:\t{0}", username)
                         .Append(Environment.NewLine)
                         //.AppendFormat(" Password:\t{0}\n", password.su)
                         .AppendFormat(" ConnectionString:\t{0}", connectionString)
                         .Append(Environment.NewLine)
                         .AppendFormat(" Logger:\t{0}", logger);

                _log.Debug(debugInfo.ToString());
            }

            _defaultVal = new DefaultDataControl(connectionString, null);
            _validateData = new ValidateDataControl(connectionString);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Validates the Errors contained in the response from flexcube.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="errors"></param>
        /// <param name="validResponse"></param>
        /// <param name="messages"></param>
        protected void ValidateErrors(string integrationName, UBSAccWebService.ERRORType[] errors, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
		{

            for (int x = 0; x < errors.Length; x++)
            {
                for (int y = 0; y < errors[x].ERROR.Length; y++)
                {
                    List<Tuple<string, string>> messages;
                    if (!_validateData.Validate(integrationName, errors[x].ERROR[y], LanguageId, out messages))
                        validResponse = false;
                    responseMessage.AddRange(messages);
                }
            }
    
        }
      

        protected void ValidateErrors(string integrationName, UBSRTWebService.ERRORType errors, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
		{
            //foreach (var error in errors)
            //{
            List<Tuple<string, string>> messages;
            if (!_validateData.Validate(integrationName, errors, LanguageId, out messages))
					validResponse = false;
				responseMessage.AddRange(messages);
			//}
		}

		/// <summary>
		/// Validates Warning Messages contained in the resonse from flexcube
		/// </summary>
		/// <param name="responseMessage"></param>
		/// <param name="warnings"></param>
		/// <param name="validResponse"></param>
		/// <param name="messages"></param>
		protected void ValidateWarnings(string integrationName, UBSAccWebService.WARNINGType[] warnings, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
		{
            _log.Debug("service validates class integration name =" + integrationName);
            for (int x = 0; x < warnings.Length; x++)
            {
                for (int y = 0; y < warnings[x].WARNING.Length; y++)
                {
                    List<Tuple<string, string>> messages;
                    
                    if (!_validateData.Validate(integrationName, warnings[x].WARNING[y], LanguageId, out messages))
                        validResponse = false;
                    responseMessage.AddRange(messages);
                }
            }
        }

        protected void ValidateWarnings(string integrationName, UBSRTWebService.WARNINGType warnings, ref bool validResponse, ref List<Tuple<string, string>> responseMessage)
		{
			//foreach(var warning in warnings)
			//{
				List<Tuple<string, string>> messages;

				if (!_validateData.Validate(integrationName, warnings, LanguageId, out messages))
					validResponse = false;
				responseMessage.AddRange(messages);
			//}
		}

		#endregion
	}
}
