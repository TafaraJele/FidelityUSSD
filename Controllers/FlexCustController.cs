
//using System.Web.Http;
//using Veneka.Indigo.Abstractions.Models;


//namespace Indigo.Controllers
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class FlexCustController : ApiController
//    {

//        /// <summary>
//        /// /
//        /// </summary>
//        /// <param name="config"></param>
//        /// <param name="customer"></param>
//        /// <param name="msgid"></param>
//        /// <param name="protocol"></param>
//        /// <param name="address"></param>
//        /// <param name="port"></param>
//        /// <param name="path"></param>
//        /// <param name="timeOutMilliseconds"></param>
//        /// <param name="connectionString"></param>
//        /// <returns></returns>
//        [HttpPost]

//        public CREATECUSTOMER_FSFS_RES PostCreateCustomerAccount(FlexcubeCreateCustomerConfig config, CREATECUSTOMER_FSFS_REQ customer, string msgid, Protocol protocol, string address, int port, string path, int? timeOutMilliseconds, string connectionString)
//        {
//            var customerValidation = new CustomerServiceValidated(protocol, address, port, path, timeOutMilliseconds, connectionString);
//            var response = customerValidation.CreateCustomer(customer);

//            return response;
//        }

//    }
//}
