﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Models
{
    public class AcctCredit
    {
        public string Account { get; set; }

        public Decimal Amount { get; set; }

        public int IgnoreImpact { get; set; }


    }
}
