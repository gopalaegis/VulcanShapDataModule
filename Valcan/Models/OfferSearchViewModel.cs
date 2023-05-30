using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class OfferSearchViewModel
    {
        public string OfferNo { get; set; }
        public int Offerddl { get; set; }
        public string CustName { get; set; }
        public int CustNameddl { get; set; }
        public string CustCode { get; set; }
        public int CustCodeddl { get; set; }
        public string MaterialCode { get; set; }
        public int MaterialCodeddl { get; set; }
        public string MaterialDesc { get; set; }
        public int MaterialDescddl { get; set; }
        public string KeyManager { get; set; }
        public int KeyManagerddl { get; set; }
        public string InquiryRef { get; set; }
        public int InquiryRefddl { get; set; }
    }
}