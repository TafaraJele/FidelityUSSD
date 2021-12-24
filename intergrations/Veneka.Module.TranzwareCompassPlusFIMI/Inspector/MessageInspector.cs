using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Common.Logging;
using System.Net;
using System.Xml;
using System.IO;
using Veneka.Module.TranzwareCompassPlusFIMI.CustomFormatter;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes;
using System.Runtime.Serialization.Formatters;
using System.Xml.Linq;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;
using Newtonsoft.Json.Linq;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Inspector
{
    /// <summary>
    /// This is how you should really get the XML request and response for a web service.
    /// </summary>
    public class MessageInspector : IClientMessageInspector
    {
        #region Private Fields
        private static ILog _log = LogManager.GetLogger(Utils.General.MODULE_LOGGER);
        private readonly bool _useBasicAuth;
        private readonly string _username;
        private readonly string _password;
        #endregion

        #region Constructors
        public MessageInspector(bool useBasicAuth, string username, string password, string logger)
        {
            _useBasicAuth = useBasicAuth;
            _username = username;
            _password = password;
            _log = LogManager.GetLogger(logger);
        }

        #endregion

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //_log.Warn("AFTER_RECEIVE_REPLY");

            var msglog = FIMILogger.GetFimiLoggerInstance();
            msglog.Info("AFTER_RECEIVE_REPLY");

            FaultException<ResponseCodes.DeclineRp> faultEx = null;

            if (reply.IsFault)
            {
                // _log.Warn("AFTER_RECEIVE_REPLY IS MARKED AS FAULT");
                msglog.Info("AFTER_RECEIVE_REPLY IS MARKED AS FAULT");

                MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                Message message = buffer.CreateMessage();
                //Assign a copy to the ref received
                reply = new FIMIMessage(buffer.CreateMessage());

                //    //http://stackoverflow.com/questions/16800275/how-to-extract-faultcode-from-a-wcf-message-in-afterreceivereply

                //    //// Create a copy of the original reply to allow default WCF processing
                //    //MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                //    //Message copy = buffer.CreateMessage();  // Create a copy to work with
                //    //reply = buffer.CreateMessage();         // Restore the original message 

                //    //MessageFault faultex = MessageFault.CreateFault(copy, Int32.MaxValue); //Get Fault from Message
                //    //FaultCode codigo = faultex.Code;
                //    ////if (faultex.HasDetail)... //More details                


                var fault = MessageFault.CreateFault(message, int.MaxValue);
                var declineElement = fault.GetDetail<XElement>();

                XmlSerializer serializer = new XmlSerializer(typeof(ResponseCodes.DeclineRp));
                var declineDetail = (ResponseCodes.DeclineRp)serializer.Deserialize(declineElement.CreateReader());

                var fex = new FaultException(fault);

                faultEx = new FaultException<ResponseCodes.DeclineRp>(declineDetail, fex.Reason, fex.Code, fex.Action);
            }
            //MessageBuffer buf = reply.CreateBufferedCopy(int.MaxValue);

            //using (var stream = new MemoryStream())
            //{
            //    buf.WriteMessage(stream);
            //    stream.Position = 0;
            //    SoapFault sf = new SoapFault();
            //    XmlSerializer serializer = new XmlSerializer(typeof(SoapFault));
            //    var fault = serializer.Deserialize(stream);

            //}


            if (msglog != null)
            {
                MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                Message message = buffer.CreateMessage();
                //Assign a copy to the ref received
                reply = new FIMIMessage(buffer.CreateMessage());

                //request = buffer.CreateMessage();         

                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        message.WriteMessage(xmlTextWriter);
                    }

                    String myStr = stringWriter.ToString();



                    if (msglog != null)
                        msglog.Info($"Response:{Environment.NewLine}{myStr.ToString()}");



                    //System.IO.File.WriteAllText(@"C:\veneka\emp\response.txt", myStr.ToString());
                }
            }

            if (faultEx != null)
                throw faultEx;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            //if (_log.IsDebugEnabled)
            //    _log.DebugFormat("Basic Authentication:{0}", _useBasicAuth);
            var msglogger = FIMILogger.GetFimiLoggerInstance();
            try
            {

               
                if (msglogger != null)
                    msglogger.Info($"Basic Authentication:{_useBasicAuth}");

                //Basic auth mean we need to set username and password.
                if (_useBasicAuth)
                {
                    StringBuilder header = new StringBuilder("basic ")
                                            .Append(Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + _password)));




                    HttpRequestMessageProperty httpRequestMessage;
                    object httpRequestMessageObject;
                    if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                    {
                        httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                        if (string.IsNullOrEmpty(httpRequestMessage.Headers[HttpRequestHeader.Authorization]))
                        {
                            httpRequestMessage.Headers[HttpRequestHeader.Authorization] = header.ToString();
                        }


                        msglogger.Info("Soap version " + request.Version);
                        msglogger.Info("Soap Headers" + request.Headers);
                        msglogger.Info("Soap State" + request.State);
                        msglogger.Info("Soap Tostring" + request.ToString());



                    }
                    else
                    {
                        var httpRequestProperty = new HttpRequestMessageProperty();
                        httpRequestProperty.Headers[HttpRequestHeader.Authorization] = header.ToString();
                        request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestProperty);
                    }
                }

                //http://pvlerick.github.io/2009/03/messagetostring-returning-stream/
                if (msglogger != null)
                {
                    MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
                    Message message = buffer.CreateMessage();
                    //Assign a copy to the ref received
                    request = new FIMIMessage(buffer.CreateMessage());
                    //request = buffer.CreateMessage();         

                    using (StringWriter stringWriter = new StringWriter())
                    {
                        using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                        {
                            xmlTextWriter.Formatting = Formatting.Indented;
                            message.WriteMessage(xmlTextWriter);
                        }

                        String myStr = stringWriter.ToString();


                        if (msglogger != null)
                            msglogger.Info($"Request:{Environment.NewLine}{myStr}");

                        //System.IO.File.WriteAllText(@"C:\veneka\emp\request.txt", myStr);
                    }
                }


                return null;

                #endregion

            }

            catch (Exception e)
            {

                msglogger.Info(" Exception " +e.ToString());
                return (JObject)"Exception";
            }
        }
    }
}
