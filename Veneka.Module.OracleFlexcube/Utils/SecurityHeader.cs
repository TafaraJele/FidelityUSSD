using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Veneka.Module.OracleFlexcube.Utils
{
    public class SecurityHeader : MessageHeader
    {
        private readonly string _password, _username, _nonce;
        private readonly DateTime _createdDate;

        public SecurityHeader(string id, string username, string password, string nonce)
        {
            _password = password;
            _username = username;
            _nonce = nonce;
            _createdDate = DateTime.Now;
            this.Id = id;
        }

        public string Id { get; set; }

        public override string Name
        {
            get { return "Security"; }
        }

        public override string Namespace
        {
            get { return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"; }
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement("wsse", Name, Namespace);
            writer.WriteXmlnsAttribute("wsse", Namespace);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement("wsse", "UsernameToken", Namespace);
            writer.WriteAttributeString("Id", Id);
            writer.WriteAttributeString("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            writer.WriteStartElement("wsse", "Username", Namespace);
            writer.WriteValue(_username);
            writer.WriteEndElement();

            writer.WriteStartElement("wsse", "Password", Namespace);
            writer.WriteAttributeString("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText");
            writer.WriteValue(_password);
            writer.WriteEndElement();

            writer.WriteStartElement("wsse", "Nonce", Namespace);
            writer.WriteAttributeString("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
            writer.WriteValue(_nonce);
            writer.WriteEndElement();

            writer.WriteStartElement("wsse", "Created", Namespace);
            writer.WriteValue(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss:sssZ"));
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
   
}
