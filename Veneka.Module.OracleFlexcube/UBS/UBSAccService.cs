using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Module.OracleFlexcube.UBSAccWebService;
using Common.Logging;
using System.ServiceModel;
using Veneka.Module.OracleFlexcube.Utils;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;

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
    public class UBSAccService : UBSService
    {
        #region Readonly Fields
        private readonly FCUBSAccServiceSEIClient client;
        private static string _username { get; set; }
        private static string _password { get; set; }
        private static string _nonce { get; set; }

        #endregion

        #region Constructors
        public UBSAccService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password, string logger)
        {


            client = new FCUBSAccServiceSEIClient(bindings, endpointAddress);
            X509Certificate2 x509 = new X509Certificate2(@"C:\IndigoIIBCert\fbluatiibcert.pfx", "fidelity");
            client.ClientCredentials.ClientCertificate.Certificate=x509;
            _log.Debug("Certificate ==" + client.ClientCredentials.ClientCertificate.Certificate.FriendlyName);

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password, logger));
                _log = LogManager.GetLogger(logger);
            }
           
            _log.Trace(m => m("Creating Flexcube WebService Client."));

            IgnoreUntrustedSSL = true;

        }

        public UBSAccService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password, string nonce, string logger)
        {


            client = new FCUBSAccServiceSEIClient(bindings, endpointAddress);
            X509Certificate2 x509 = new X509Certificate2(@"C:\IndigoIIBCert\fbluatiibcert.pfx", "fidelity");
            client.ClientCredentials.ClientCertificate.Certificate = x509;
            _log.Debug("Certificate ==" + client.ClientCredentials.ClientCertificate.Certificate.FriendlyName);

            if (String.IsNullOrWhiteSpace(logger))
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password,nonce, Utils.General.MODULE_LOGGER));
            else
            {
                client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(userBasicAuth ?? false, username, password,nonce, logger));
                _log = LogManager.GetLogger(logger);
            }



            _username = username;
            _password = password;
            _nonce = nonce;

            _log.Trace(m => m("Creating Flexcube WebService Client."));


            IgnoreUntrustedSSL = true;
        }

        public UBSAccService(System.ServiceModel.BasicHttpBinding bindings, System.ServiceModel.EndpointAddress endpointAddress,
                              bool? userBasicAuth, string username, string password)
            : this(bindings, endpointAddress, userBasicAuth, username, password, null)
        {
        }
        #endregion

        #region Public Methods
        public QUERYCUSTACC_IOFS_RES QueryCustAcc(QUERYCUSTACC_IOFS_REQ queryCustAccRequest)
        {
            _log.Trace(m => m("Calling WebMethod QueryCustAccIO"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            var response = client.QueryCustAccIO(queryCustAccRequest);
            _log.Trace(m => m("Response Received From QueryCustAccIO"));
            return response;
  
        }

        public QUERYACCBAL_IOFS_RES QueryAccBal(QUERYACCBAL_IOFS_REQ queryAccBalRequest)
        {
            _log.Trace(m => m("Calling WebMethod QueryAccBalIO"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            var response = client.QueryAccBalIO(queryAccBalRequest);
            _log.Trace(m => m("Response Received From QueryAccBalIO"));

            return response;
        }

        public MODIFYCUSTACC_IOPK_RES ModifyCustAccIO(MODIFYCUSTACC_IOPK_REQ modifyCustAccRequest)
        {
            _log.Trace(m => m("Calling WebMethod ModifyCustAccIO"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            var response = client.ModifyCustAccIO(modifyCustAccRequest);
            _log.Trace(m => m("Response Received From ModifyCustAccIO"));

            return response;
        }

        public MODIFYCUSTACC_FSFS_RES ModifyCustAccFS(MODIFYCUSTACC_FSFS_REQ modifyCustAccRequest)
        {
            _log.Trace(m => m("Calling WebMethod ModifyCustAccFS"));

            //Ignore untrusted SSL errror.
            AddUntrustedSSL();
            var response = client.ModifyCustAccFS(modifyCustAccRequest);
            _log.Trace(m => m("Response Received From ModifyCustAccFS"));

            return response;
        }
        #endregion
    }
}
