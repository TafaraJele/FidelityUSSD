using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
	public class Cards
	{
		public string CardNumber { get; set; }
		public string PAN { get; set; }
		public int MBR { get; set; }
		public DateTime ExpDate { get; set; }
		public long PersonId { get; set; }
		public int Code { get; set; }
		public string Message { get; set; }
	}
}
