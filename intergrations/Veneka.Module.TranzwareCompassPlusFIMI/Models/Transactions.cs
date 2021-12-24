using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
    public class Transactions
    {

        public long Id { get; set; }
        public int Type { get; set; }
        public DateTime Time { get; set; }
        public DateTime ExpDate { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Fee { get; set; }
        public int Currency { get; set; }
    
        public string TranNumber { get; set; }
        public string CardNumber { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }


    }
}
