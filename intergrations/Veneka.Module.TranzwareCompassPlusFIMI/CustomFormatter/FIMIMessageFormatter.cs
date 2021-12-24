using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter
{
    public class FIMIMessageFormatter : IClientMessageFormatter
    {
        private readonly IClientMessageFormatter formatter;

        public FIMIMessageFormatter(IClientMessageFormatter formatter)
        {
            this.formatter = formatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            return this.formatter.DeserializeReply(message, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            var message = this.formatter.SerializeRequest(messageVersion, parameters);
            return new FIMIMessage(message);
        }
    }
}
