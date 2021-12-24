using System;
using System.Net;
using System.Web.Http;
using Indigo.CardReq;
using Indigo.Models;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Veneka.Module.TranzwareCompassPlusFIMI;
using System.IO;
using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Veneka.Module.TranzwareCompassPlusFIMI.Models;
using Indigo.Utility;
using Indigo.Auth;

namespace Indigo.Controllers
{
    [Serializable]
    public class TokenController : ApiController
    {
            // GET: Login
            [HttpPost]

            public JObject Auth([FromBody] Token login)
            {

               AuthenticationResponse resp = null;

                try
                {
                    //string  Fidelity_API = new Fidelity_API();
                    string username = login.username;
                    string password = login.password;
                    string resul = "Success";
                     JObject myCleanJsonObject = null;





                //Sending to wsdl card request fuction

                //Sending to wsdl card request fuction
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => { return true; };

                    AuthenticationClient prof = new AuthenticationClient();


                if (username != null && password != null)
                {
                    resp = prof.Login(username, password);

                    if (resp.AuthToken != null) {

                        var result = new
                        {
                            Message = "Success",
                            Code = 200,
                            AuthToken = resp.AuthToken,

                        };


                        System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                        var rt = js.Serialize(result);

                        //rt = JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.None });
                        rt = rt.Replace(@"\\", "");

                        myCleanJsonObject = JObject.Parse(rt);

                    }

                }

                   



                    return myCleanJsonObject;



                }
                catch (IndexOutOfRangeException)
                {


                    return (JObject)"Exception";
                }
            }
        }
    }