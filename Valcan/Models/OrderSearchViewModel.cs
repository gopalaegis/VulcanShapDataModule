using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class OrderSearchViewModel
    {
        public string SoNo { get; set; }
        public int SoNoddl { get; set; }
        public string OCustName { get; set; }
        public int OCustNameddl { get; set; }
        public string OCustCode { get; set; }
        public int OCustCodeddl { get; set; }
        public string OMaterialCode { get; set; }
        public int OMaterialCodeddl { get; set; }
        public string OMaterialDesc { get; set; }
        public int OMaterialDescddl { get; set; }
        public string OKeyManager { get; set; }
        public int OKeyManagerddl { get; set; }
        public string OPORef { get; set; }
        public int OPoRefddl { get; set; }
        public string ODivision { get; set; }
        public int ODivisionddl { get; set; }
    }
}