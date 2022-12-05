using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Veneka.Module.OracleFlexcube;

namespace Veneka.Module.OracleFlexcube.Utils
{
    public class General
    {
        public const string MODULE_LOGGER = "OracleFlexcubeLogger";

        public static BasicHttpBinding BuildBindings(ServicesValidated.Protocol protocol, int? timeoutMilliSeconds)
        {
            //Default timeout is 1 minute.
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliSeconds.GetValueOrDefault(60000));

            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            if (protocol == ServicesValidated.Protocol.HTTPS)
                securityMode = BasicHttpSecurityMode.Transport;

            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.Name = "bankworldBinding";
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
            //TODO need logic to determin if address and path in correct format.


            UriBuilder uri = new UriBuilder();
            uri.Scheme = protocol.ToString();
            uri.Host = address;
            uri.Port = port;
            uri.Path = path;

            return new EndpointAddress(uri.Uri);
        }

        public enum TransactionCode
        {
            GITD,
            GITC,
            GITF,
            MITD,
            MITC,
            MITF
        }

        public static string MaskPAN(string fullPAN)
        {
            if (string.IsNullOrEmpty(fullPAN))
                return string.Empty;
            return fullPAN.Substring(0, 6) + "*".PadRight(6, '*') + fullPAN.Substring(fullPAN.Length - 4, 4);
        }
    }
}
