using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter
{
    class FIMIMessageBodyWriter : BodyWriter
    {
        string body;

        public FIMIMessageBodyWriter(string strData)
            : base(true)
        {
            body = strData;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteRaw(body);
        }
    }
}
