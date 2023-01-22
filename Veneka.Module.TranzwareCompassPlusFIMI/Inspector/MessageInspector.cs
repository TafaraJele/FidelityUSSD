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
using System.Configuration;
using Veneka.Module.TranzwareCompassPlusFIMI.Utils;

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
            _log.Warn("AFTER_RECEIVE_REPLY");
            FIMILogger loggers = FIMILogger.GetFimiLoggerInstance();
            FaultException<ResponseCodes.DeclineRp> faultEx = null;
            MessageBuffer buffer;
            Message message;
            if (reply.IsFault)
            {
                _log.Warn("AFTER_RECEIVE_REPLY IS MARKED AS FAULT");
                loggers.Debug("AFTER_RECEIVE_REPLY IS MARKED AS FAULT");
                 buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                 message = buffer.CreateMessage();
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


            //if (_log.IsDebugEnabled)
            //{
             buffer = reply.CreateBufferedCopy(Int32.MaxValue);
             message = buffer.CreateMessage();
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
                var isCardInforRep = IsCardInfoResponse(myStr);
                if (isCardInforRep)
                {
                    var newString = AppendDetailsToAccUID(myStr);
                    var filename = GetAccountUID(myStr);
                    WriteXmlResponseToFile(newString, filename);
                }
                if (_log.IsDebugEnabled)
                    _log.DebugFormat("Response:{0}{1}", Environment.NewLine, myStr.ToString());
                loggers.Debug($"Response:\t {myStr.ToString()}");

                //System.IO.File.WriteAllText(@"C:\veneka\emp\response.txt", myStr.ToString());
            }
            //}

            if (faultEx != null)
                throw faultEx;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            FIMILogger loggers = FIMILogger.GetFimiLoggerInstance();
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Basic Authentication:{0}", _useBasicAuth);
          
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
                }
                else
                {
                    var httpRequestProperty = new HttpRequestMessageProperty();
                    httpRequestProperty.Headers[HttpRequestHeader.Authorization] = header.ToString();
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestProperty);
                }
            }

            //http://pvlerick.github.io/2009/03/messagetostring-returning-stream/
            //if (_log.IsDebugEnabled)
            //{
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

                    //if (_log.IsDebugEnabled)
                    //    _log.DebugFormat("Request:{0}{1}", Environment.NewLine, myStr);
                    loggers.Debug($"Request:\t{myStr}");
                    //System.IO.File.WriteAllText(@"C:\veneka\emp\request.txt", myStr);
                }
            //}

            return null;
        }
        private string GetCardStatus(string xmlResponse)
        {
            int pos1 = xmlResponse.LastIndexOf("<m0:Status>");
            int length = xmlResponse.LastIndexOf("</m0:Status>") - pos1;
            string value = xmlResponse.Substring(pos1, length);
            char[] splitArray = new char[2] { '<', '>' };
            return value.Split(splitArray)[2];
        }

        private string GetCardUID(string xmlResponse)
        {

            var pos1 = xmlResponse.LastIndexOf("<m0:CardUID>");

            var pos2 = xmlResponse.LastIndexOf("</m0:CardUID>");

            var length = pos2 - pos1;

            var value = xmlResponse.Substring(pos1, length);

            char[] splitArray = new char[2] { '<', '>' };

            var valueArray = value.Split(splitArray);

            return valueArray[2];
        }
        private string AppendDetailsToAccUID(string xmlResponse)
        {
            string status = GetCardStatus(xmlResponse);
            var cardUID = GetCardUID(xmlResponse).Trim();
            var cardReferenceNumber = GetCardReferenceNumber(xmlResponse);
            string accountUID = GetAccountUID(xmlResponse).Trim();
            string appendedAccountUID = $"{accountUID};{cardReferenceNumber};{status}";
            xmlResponse = xmlResponse.Replace(accountUID, appendedAccountUID);

            return xmlResponse;
        }
        private string GetCardReferenceNumber(string xmlResponse)
        {
            var pos1 = xmlResponse.IndexOf("<m0:PersonId>");

            var pos2 = xmlResponse.IndexOf("</m0:PersonId>");

            var length = pos2 - pos1;

            var value = xmlResponse.Substring(pos1, length);

            char[] splitArray = new char[2] { '<', '>' };

            var valueArray = value.Split(splitArray);

            return valueArray[2];
        }

        private bool IsCardInfoResponse(string xmlResponse)
        {
            var index = xmlResponse.IndexOf("<m1:GetCardInfoRp>");
            if (index != -1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private void WriteXmlResponseToFile(string xmlResponse, string filename)
        {
            //string filePath = @"C:\Config\IndigoPrepaidUAT\NIResponse";
            string filePath = ConfigurationManager.AppSettings.Get("NIResponsePath");
             filePath = $@"{filePath}";
            try
            {
                string path = System.IO.Path.Combine(filePath, $"{filename}.txt");
                while(File.Exists(path))
                {
                    File.Delete(path);
                }
                StreamWriter writer = new StreamWriter(path);
                writer.WriteLine(xmlResponse);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                _log.Debug($"Failed to write NI xml response to location {filePath} with exception {ex.Message} ");
            }

        }

        private string GetAccountUID(string xmlResponse)
        {
            var pos1 = xmlResponse.LastIndexOf("<m0:AccountUID>");

            var pos2 = xmlResponse.LastIndexOf("</m0:AccountUID>");

            var length = pos2 - pos1;

            var value = xmlResponse.Substring(pos1, length);

            char[] splitArray = new char[2] { '<', '>' };

            var valueArray = value.Split(splitArray);

            return valueArray[2];
        }

        #endregion
    }
}
