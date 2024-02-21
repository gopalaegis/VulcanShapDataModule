using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class OfferViewModel
    {
        public int Id { get; set; }
        public string Dv { get; set; }
        public string SaTy { get; set; }
        [Display(Name = "Code")]
        public string Sold_to_pt { get; set; }
        [Display(Name = "Customer Name")]
        public string Name_1 { get; set; }
        [Display(Name = "Inquiry No.")]
        public string Purchase_order_number { get; set; }
        [Display(Name = "Offer No.")]
        public string Document { get; set; }
        [Display(Name = "Line")]
        public string Item { get; set; }

        [Display(Name = "Offer Date")]
        public Nullable<System.DateTime> Doc_Date { get; set; }
        public string strDoc_Date { get; set; }
        [Display(Name = "Material code")]
        public string Material { get; set; }
        [Display(Name = "Material Description")]
        public string Material_Description { get; set; }
        [Display(Name = "Exch. Rate")]
        public string Exch_Rate { get; set; }
        [Display(Name = "Curr.")]
        public string Curr { get; set; }
        [Display(Name = "Qty")]
        public string ConfirmQty { get; set; }
        public string Net_value1 { get; set; }
        [Display(Name = "Unit Price")]
        public string Net_price { get; set; }
        
        [Display(Name = "Total Price")]
        public string Net_Value { get; set; }
        public string Prb { get; set; }
        public Nullable<System.DateTime> Dlv_Date { get; set; }
        public string strDlv_Date { get; set; }
        public string Prod_hier { get; set; }
        public string Prod_Description { get; set; }
        public string CGrp { get; set; }
        public string Key_Managr { get; set; }
        public string SDst { get; set; }
        [Display(Name = "KeyManager")]
        public string Customer_Z { get; set; }
        public string Vendor_Name { get; set; }
        public string Matl_Group { get; set; }
        public string Mat_Grp_Descr { get; set; }
        public string Cust_Ind_C { get; set; }
        public string Cust_Ind_D { get; set; }
        public string OrdRs { get; set; }
        [Display(Name = "Total Price in INR")]
        public string Offer_Value_INR     { get; set; }
        [Display(Name = "Expected order value in INR")]
        public string Order_Expected  { get; set; }
    }
}