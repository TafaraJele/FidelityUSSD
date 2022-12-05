using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter
{
    //http://stackoverflow.com/questions/31595770/c-sharp-wcf-namespaces-move-to-header-use-ns-prefix/31597758#31597758
    class FIMIMessage : Message
    {
        private readonly Message message;

        public FIMIMessage(Message message)
        {
            this.message = message;
        }

        public override MessageHeaders Headers
        {
            get
            {
                return this.message.Headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                return this.message.Properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                return this.message.Version;
            }
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("Body", "http://www.w3.org/2003/05/soap-envelope");            
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.message.WriteBodyContents(writer);
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            //xmlns:fimi1="http://schemas.compassplus.com/two/1.0/fimi_types.xsd"
            //xmlns:fimi="http://schemas.compassplus.com/two/1.0/fimi.xsd" 
            //xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
            //xmlns:m="http://schemas.compassplus.com/two/1.0/fimi.xsd"
            writer.WriteStartElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
            writer.WriteAttributeString("xmlns", "fimi1", null, "http://schemas.compassplus.com/two/1.0/fimi_types.xsd");
            writer.WriteAttributeString("xmlns", "fimi", null, "http://schemas.compassplus.com/two/1.0/fimi.xsd");
            writer.WriteAttributeString("xmlns", "m", null, "http://schemas.compassplus.com/two/1.0/fimi.xsd");
            //writer.WriteAttributeString("xmlns", "soap", null, "http://www.w3.org/2003/05/soap-envelope");


        }
    }
}
