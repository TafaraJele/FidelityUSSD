using System;
using System.Xml.Serialization;

namespace Veneka.Module.OracleFlexcube.Utils
{
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    public class UsernameToken
    {
        public UsernameToken()
        {
        }

        public UsernameToken(string id, string username, string password,string nonce)
        {
            Id = id;
            Username = username;
            Password = new Password() { Value = password };
            Nonce = new Nonce() {Value=nonce };
            Created = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss:sssZ");
        }

        [XmlAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public string Id { get; set; }

        [XmlElement]
        public string Username { get; set; }

        [XmlElement]
        public Password Password { get; set; }

        [XmlElement]
        public Nonce Nonce { get; set; }

        [XmlElement]
        public string Created { get; set; }
    }
    public class Nonce
    {
        public Nonce()
        {
            EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
        }

        [XmlAttribute]
        public string EncodingType { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
    public class Password
    {
        public Password()
        {
            Type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
        }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}