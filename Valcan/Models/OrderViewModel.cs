using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        [Display(Name = "SO NO.")]
        public string Sales_Doc { get; set; }
        [Display(Name = "Line")]
        public string Item { get; set; }
        [Display(Name = "SLine")]
        public string Sch_Line { get; set; }
        [Display(Name = "MRP")]
        public string MRP_Controller { get; set; }
        [Display(Name = "Div")]
        public string Dv { get; set; }
        [Display(Name = "Cust Code")]
        public string Sold_to_pt { get; set; }
        [Display(Name = "Customer Name")]
        public string Sold_to_party { get; set; }

        [Display(Name = "Material Code")]
        public string Material { get; set; }
        [Display(Name = "Material Description")]
        public string Description { get; set; }
        [Display(Name = "Order Qty")]
        public string Order_Qty { get; set; }
        [Display(Name = "Open Qty")]
        public string Open_Qty { get; set; }
        [Display(Name = "Base Price")]
        public string Unit_Basic_Price { get; set; }
        [Display(Name = "Curr")]
        public string Curr { get; set; }
        [Display(Name = "Total Price")]
        public string Total_Price_IRS { get; set; }
        [Display(Name = "PRD Remark")]
        public string Remarks_from_Production { get; set; }
        
        [Display(Name = "PO No.")]
        public string Purchase_order_no { get; set; }
        [Display(Name = "PO Date")]
        public Nullable<System.DateTime> PO_date { get; set; }
        public string strPO_date { get; set; }
        [Display(Name = "Dispatch Lot")]
        public string Your_Ref { get; set; }
        [Display(Name = "NPD/REPEAT Order")]
        public string Material_group_1 { get; set; }
        [Display(Name = "STD/SPL Marking")]
        public string Material_group_2 { get; set; }
        [Display(Name = "BD/LD/Normal")]
        public string Customer_group_5 { get; set; }
        [Display(Name = "SO Created")]
        public Nullable<System.DateTime> Created_on { get; set; }
        public string strCreated_on { get; set; }
        [Display(Name = "Cust Req.Dt.")]
        public Nullable<System.DateTime> Req_dlv_dt { get; set; }
        public string strReq_dlv_dt { get; set; }
        [Display(Name = "Cust Req. Week")]
        public string Cust_Req_Week { get; set; }
        [Display(Name = "Cust Req.Yar")]
        public string Cust_Req_Yar { get; set; }
        [Display(Name = "Incoterms")]
        public string IncoT { get; set; }
        [Display(Name = "Incoterms Des")]
        public string Inco_2 { get; set; }
        [Display(Name = "PRD Week")]
        public string Week_Changed { get; set; }
        [Display(Name = "MKT Remark")]
        public string Remarks_from_Marketing { get; set; }
        [Display(Name = "Mkt Sales Week")]
        public string Marketing_Sales_Week { get; set; }
        [Display(Name = "Pre Dispatch Inspection")]
        public string Pre_Dispatch_Inspection { get; set; }
        [Display(Name = "Key Manager")]
        public string KEYMANAGER { get; set; }
    }
}