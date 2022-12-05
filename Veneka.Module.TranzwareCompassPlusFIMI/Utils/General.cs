using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Veneka.Module.TranzwareCompassPlusFIMI.CustomEncoder;
using Common.Logging;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Utils
{
    public class General
    {
        #region Constants
        public const string MODULE_LOGGER = "TranzwareCompassPlusFIMILogger";
        #endregion
        #region Fields
        protected static ILog _log = LogManager.GetLogger(Utils.General.MODULE_LOGGER);
        #endregion

        #region Enums

        public enum UpdateFieldMethods
        {
            OnlyTransferredFields = 0, // Only the transferred fields will be updated.
            OptionalFieldsNull = 1 // The optional fields that were not sent will be set to NULL. The FIO, VIP and Sex fields cannot be set to NULL, they will be left unchanged
        }

        public enum AccountTypes
        {
            Checkings = 1,
            Savings = 11,
            Credit = 31,
            Bonus = 91
        }

        public enum AccountStatus
        {
            InactiveAccount = 0,
            Open = 1,
            DepositOnly = 2,
            OpenPrimaryAccount = 3,
            DepositOnlyPrimaryAccount = 4,
            InformationOnly = 5,
            Closed = 9
        }

        #endregion

        public static BasicHttpBinding BuildBasicBindings(ServicesValidated.Protocol protocol, int? timeoutMilliSeconds)
        {
            //Default timeout is 1 minute.
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliSeconds.GetValueOrDefault(60000));

            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            if (protocol == ServicesValidated.Protocol.HTTPS)
                securityMode = BasicHttpSecurityMode.Transport;

            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.Name = "fimiBinding";
            binding.CloseTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = timeout;

            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            return binding;
        }

        public static CustomBinding BuildBindings(ServicesValidated.Protocol protocol,
                                                    System.ServiceModel.Channels.MessageVersion messageVersion,
                                                    int? timeoutMilliSeconds, bool useCustomEncoder)
        {
            //Default timeout is 1 minute.
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliSeconds.GetValueOrDefault(60000));
            ICollection<BindingElement> bindingElements = new List<BindingElement>();

            if (useCustomEncoder)
            {                
                CustomTextMessageBindingElement textBindingElement = new CustomTextMessageBindingElement("UTF-8", "text/xml", messageVersion);
                bindingElements.Add(textBindingElement);
            }
            else
            {
                //Set the message version
                TextMessageEncodingBindingElement TextMessage = new TextMessageEncodingBindingElement()
                {
                    MessageVersion = messageVersion

                };

                bindingElements.Add(TextMessage);
            }

            //Use http or https
            
            if (protocol == ServicesValidated.Protocol.HTTP)
            {
                _log.Debug("Building Bindings --> using http");
                HttpTransportBindingElement httpTransport = new HttpTransportBindingElement()
                {
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue
                };
                bindingElements.Add(httpTransport);
            }
            else
            {
                _log.Debug("Building Bindings --> using https");
                MessageSecurityVersion version = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
                var sec = SecurityBindingElement.CreateCertificateOverTransportBindingElement(version);
                sec.EnableUnsecuredResponse = true;
                bindingElements.Add(sec);

                HttpsTransportBindingElement httpsTransport = new HttpsTransportBindingElement()
                { 
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue,
                    RequireClientCertificate = true,
                    
                };
                bindingElements.Add(httpsTransport);

               
            }

            CustomBinding customBinding = new CustomBinding(bindingElements);

            
            
            customBinding.CloseTimeout =
            customBinding.OpenTimeout =
            customBinding.SendTimeout = timeout;
            customBinding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            
            


            return customBinding;
        }


        public static BasicHttpBinding BuildBindings2(ServicesValidated.Protocol protocol, int? timeoutMilliSeconds)
        {
            //Default timeout is 1 minute.
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliSeconds.GetValueOrDefault(60000));

            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            if (protocol == ServicesValidated.Protocol.HTTPS)
                securityMode = BasicHttpSecurityMode.Transport;

            //CustomBinding customBinding = new CustomBinding()
            
            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            //binding.MessageVersion = MessageVersion.Soap12;
            binding.Name = "fimiBinding";
            binding.CloseTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = timeout;

            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;
           
            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            return binding;
        }

        public static EndpointAddress BuildEndpointAddress(ServicesValidated.Protocol protocol, string address, int port, string path)
        {
            //TODO need logic to determine if address and path in correct format.
            UriBuilder uri = new UriBuilder();
            uri.Scheme = protocol.ToString();
            uri.Host = address;
            uri.Port = port;
            if(!String.IsNullOrWhiteSpace(path))
                uri.Path = path;

            return new EndpointAddress(uri.Uri);
        }
    }
}