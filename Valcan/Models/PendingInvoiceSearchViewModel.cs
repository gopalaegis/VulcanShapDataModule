using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class PendingInvoiceSearchViewModel
    {
        public string InvoiceNo { get; set; }
        public int Invoiceddl { get; set; }
        public string PICustName { get; set; }
        public int PICustNameddl { get; set; }
        public string PICustCode { get; set; }
        public int PICustCodeddl { get; set; }
        public string PIKeyManager { get; set; }
        public int PIKeyManagerddl { get; set; }
    }
}