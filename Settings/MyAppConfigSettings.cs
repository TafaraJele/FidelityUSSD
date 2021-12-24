using System;
using System.Configuration;
using System.Web.Http.Cors;
using Veneka.Module.TranzwareCompassPlusFIMI;
using System.IO;
using static Veneka.Module.TranzwareCompassPlusFIMI.ServicesValidated;
using Veneka.Module.TranzwareCompassPlusFIMI.FIMI;
using System.Configuration;

namespace Indigo.Settings
{

    public static class MyAppConfigSettings
    {


        public static readonly string username = ConfigurationManager.AppSettings["username"].ToString();
        public static readonly string password = ConfigurationManager.AppSettings["password"].ToString();
        public static readonly string address = ConfigurationManager.AppSettings["address"].ToString();
        public static readonly string path = ConfigurationManager.AppSettings["path"].ToString();
        public static readonly string instName = ConfigurationManager.AppSettings["instName"].ToString();
        public static readonly int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
        public static readonly int timeoutMilliSeconds = int.Parse(ConfigurationManager.AppSettings["timeoutMilliSeconds"].ToString());

        public static readonly string logger = "";
        public static readonly string sessionKey = "";
        public static readonly string nextChallenge = "";
        public static readonly int sessionId = 0;

        public static readonly FileInfo file = new FileInfo("FIMI");
        public static readonly bool UseCustomEncoder = false;


    }
}
    
