using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
    public class getTransInfo
    {
     

        public int MBR { get; set; }
        public string PAN { get; set; }
        public int Count { get; set; }
        public string IssInstName { get; set; }
        public string CardNumber { get; set; }
        public DateTime FromTime { get; set; }

        public DateTime ToTime { get; set; }
    }
}
