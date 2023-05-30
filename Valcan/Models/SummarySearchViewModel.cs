using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class SummarySearchViewModel
    {
        public string SCustName { get; set; }
        public int SCustNameddl { get; set; }
        public string SCustCode { get; set; }
        public int SCustCodeddl { get; set; }
        public string SKeyManager { get; set; }
        public int SKeyManagerddl { get; set; }
        public string SDivision { get; set; }
        public int SDivisionddl { get; set; }
        public string SMonths { get; set; }
        public int SMonthsddl { get; set; }
    }
}