using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
   public class CardDetails
    {
        public string AcctNo { get; set; }
        public int Status { get; set; }
        public string AccountUID { get; set; }
        public Decimal AvailBalance { get; set; }
        public int Currency { get; internal set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public long CardReferenceNumber { get; set; }

    }
}
