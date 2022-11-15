using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
    public class Transactions
    {
        public long FontId { get; set; }
        public string BackId { get; set; }
        public int Origin { get; set; }        
        public int Type { get; set; }
        public int OperCode { get; set; }
        public string AnotherTitle { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }       
        public DateTime OperDate { get; set; }
        public DateTime TranTime { get; set; }       
        public decimal OrigAmount { get; set; }
        public string Currency { get; set; }    
        public string TranNumber { get; set; }
        public string CardNumber { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public string OrigCurrency { get; set; }
        public string PAN { get; set; }
        public int MBR { get; set; }      
        public int TermClass { get; set; }
        public string TermName { get; set; }
        public string TermRetailerName { get; set; }
        public int TermSIC { get; set; }        
        public string TermLocation { get; set; }
        public string BackAcct { get; set; }
        public decimal BackAmount { get; set; }        
        public decimal Remain { get; set; }        
        public string ApprovalCode { get; set; }
        public int CurrencyISOCode { get; set; }
        public int OrigCurrencyISOCode { get; set; }        
        public int SeqNo { get; set; }

    }
}
