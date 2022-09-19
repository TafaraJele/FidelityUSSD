
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

        public FlexCustomerAccResp CreateFlexAccount([FromBody]FlexiCustomer customer)
        {
            Application application = new Application();                        

            return application.CreateCustomer(customer); ;

        }
    }
}
