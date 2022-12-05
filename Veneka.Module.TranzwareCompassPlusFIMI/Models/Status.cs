using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
	public class Status
	{
		public string AcctNo { get; set; }
		public int status { get; set; }
		public int Code { get; set; }
		public string Message { get; set; }
	}
}
