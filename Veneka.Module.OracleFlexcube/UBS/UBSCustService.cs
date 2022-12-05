using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Veneka.Module.OracleFlexcube.UBSCustWebService;


namespace Veneka.Module.OracleFlexcube.UBS
{
    /*
     * There is a problem with XMLSerialiser and Nested Loops. If you update this service and get
     * weird CS00029 and CS00030 messages Example:
     * 
     * Have a look here for the solution: http://stackoverflow.com/questions/7581440/cs0030unable-to-generate-a-temporary-class
     * And here: https://connect.microsoft.com/VisualStudio/feedback/details/471297
     * 
     * 
     * The error occurs when a complex type in the wsdl contains exactly one element with 
     * unbounded occurrence. The workaround, taken from this forum discussion (credit to Elena Kharitidi), 
     * is to add dummy attributes to such types:

        <xs:sequence maxOccurs="unbounded">
            <xs:element ../>
        <xs:sequence>
        <xs:attribute name="tmp" type="xs:string" />      <-- add this
     
        and

        <xs:sequence>
            <xs:element maxOccurs="unbounded"/>
        <xs:sequence>
        <xs:attribute name="tmp" type="xs:string" />      <-- add this
     * 
     * You are welcome! 
     * 
     */

    /// <summary>
    /// Class that interacts with the web service, it doesn't populate default values or validates response
    /// </summary>
    public class UBSCustService: UBSService
    {
        #region Readonly Fields
        private readonly FCUBSCustomerServiceSEIClient client;
        private static string _username { get; set; }
        private static string _password { get; set; }
        private static string _nonce { get; set; }

        #endregion
        #region Constructors
        public UBSCustService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                            bool? userBasicAuth, string username, string password, string logger)
        {

            client = new FCUBSCustomerServiceSEIClient(bindings, endpointAddress);
            X509Certificate2 x509 = new X509Certificate2(@"C:\IndigoIIBCert\fbluatiibcert.pfx", "fidelity");
            client.ClientCredentials.ClientCertificate.Certificate = x509;
            _log.Debug("Certificate ==" + client.ClientCredentials.ClientCertificate.Certificate.FriendlyName);

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, logger));
                _log = LogManager.GetLogger(logger);
            }

            _log.Trace(m => m("Creating Flexcube Customer WebService  Client."));

            IgnoreUntrustedSSL = true;

        }

        public UBSCustService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                             bool? userBasicAuth, string username, string password, string nonce, string logger)
        {


            client = new FCUBSCustomerServiceSEIClient(bindings, endpointAddress);
            X509Certificate2 x509 = new X509Certificate2(@"C:\IndigoIIBCert\fbluatiibcert.pfx", "fidelity");
            client.ClientCredentials.ClientCertificate.Certificate = x509;
            _log.Debug("Certificate ==" + client.ClientCredentials.ClientCertificate.Certificate.FriendlyName);

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, nonce, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, nonce, logger));
                _log = LogManager.GetLogger(logger);
            }



            _username = username;
            _password = password;
            _nonce = nonce;

            _log.Trace(m => m("Creating Flexcube WebService Client."));


            IgnoreUntrustedSSL = true;
        }

        //public UBSCustService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
        //                   bool? userBasicAuth, string username, string password)
        // : this(bindings, endpointAddress, userBasicAuth, username, password, null)
        //{
        //}
        #endregion

        #region Public Methods
        public QUERYCUSTOMER_IOFS_RES QueryCustDetails (QUERYCUSTOMER_IOFS_REQ  queryCustDetailsRequest)
        {
            _log.Trace(m => m("Calling WebMethod QueryCustomerIO"));
            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            var response = client.QueryCustomerIO (queryCustDetailsRequest);
            _log.Trace(m => m("Response Received From QueryCustAccIO"));
            return response;
        }
        #endregion
    }
}
