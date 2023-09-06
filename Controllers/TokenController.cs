using System;
using System.Net;
using System.Web.Http;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Newtonsoft.Json.Linq;

using Veneka.Indigo.Abstractions.Models;
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
            JObject myCleanJsonObject = null;

            try
            {
                
                string resul = "Success";
               
                                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                AuthenticationClient prof = new AuthenticationClient();


                if (!string.IsNullOrEmpty(login.UserName) && !string.IsNullOrEmpty(login.UserName) )
                {
                    resp = prof.Login(login.UserName, login.Password);

                    if (resp.AuthToken != null)
                    {

                        var result = new
                        {
                            Message = "Success",
                            Code = 200,
                            AuthToken = resp.AuthToken,

                        };


                        System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                        var rt = js.Serialize(result);
                       
                        rt = rt.Replace(@"\\", "");

                        myCleanJsonObject = JObject.Parse(rt);

                    }

                }
                else
                {
                    var result = new
                    {
                        Message = "Failure UserName and Password are required",
                        Code = 400
                       
                    };
                    System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                    var rt = js.Serialize(result);

                    rt = rt.Replace(@"\\", "");

                    myCleanJsonObject = JObject.Parse(rt);
                }

                return myCleanJsonObject;


            }
            catch (Exception exec)
            {
                var result = new
                {
                    Message = $"{exec}",
                    Code = 500

                };
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                var rt = js.Serialize(result);

                rt = rt.Replace(@"\\", "");

                myCleanJsonObject = JObject.Parse(rt);

                return myCleanJsonObject = JObject.Parse(rt);
                
            }
        }
    }
}