using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veneka.Module.TranzwareCompassPlusFIMI.ResponseCodes
{
    public sealed class Response
    {
        private const string UNKNOWN_RESPONSE_CODE = "Uknown Error";
        private static readonly string[] _englishResponses = new string[]
        {
            /*0*/   UNKNOWN_RESPONSE_CODE,
            /*1*/   "Approved",
            /*2*/   "Format Error",
            /*3*/   UNKNOWN_RESPONSE_CODE,
            /*4*/   "Invalid Key ID",
            /*5*/   "Invalid Clerk",
            /*6*/   "Invalid Password",
            /*7*/   "Cleak Expired",
            /*8*/   "System Malfunction",
            /*9*/   "Clerk Blocked",
            /*10*/  "Invalid Terminal",
            /*11*/  "Invalid Transaction",
            /*12*/  "Invalid Card",
            /*13*/  "Invalid Account",
            /*14*/  UNKNOWN_RESPONSE_CODE,
            /*15*/  "Account Already Exists",
            /*16*/  UNKNOWN_RESPONSE_CODE,
            /*17*/  UNKNOWN_RESPONSE_CODE,
            /*18*/  "Invalid Session",
            /*19*/  UNKNOWN_RESPONSE_CODE,
            /*20*/  UNKNOWN_RESPONSE_CODE,
            /*21*/  UNKNOWN_RESPONSE_CODE,
            /*22*/  UNKNOWN_RESPONSE_CODE,
            /*23*/  UNKNOWN_RESPONSE_CODE,
            /*24*/  "Invalid Currency"
        };

        public static string GetResponse(int responseCode)
        {
            if (responseCode > _englishResponses.Length -1 || responseCode < 0)
                return UNKNOWN_RESPONSE_CODE;

            return _englishResponses[responseCode];
        }
    }
}
