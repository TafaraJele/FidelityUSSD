
using Indigo.Utility;
using System.Web.Http;
using Veneka.Indigo.Abstractions.Models;
using Veneka.Indigo.Core;
namespace Indigo.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FlexAccController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost]

        public FlexCustomerAccResp CreateFlexAccount([FromBody] FlexiCustomer customer)
        {
            var fimilogger = FIMILogger.GetFimiLoggerInstance();
            fimilogger.Debug("Create flex customer account");
            Application application = new Application();

            return application.CreateCustomer(customer); ;

            }
        }
}
