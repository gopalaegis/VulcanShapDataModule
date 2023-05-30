using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class PendingInvoiceViewModel
    {
        public int Id { get; set; }
        public string CoCd { get; set; }
        [Display(Name = "Code")]
        public string Customer { get; set; }
        public string Assignment { get; set; }
        public string Year { get; set; }
        [Display(Name = "Inv. No.")]
        public string DocumentNo { get; set; }
        public Nullable<System.DateTime> Pstng_Date { get; set; }
        public string strPstng_Date { get; set; }
        [Display(Name = "Inv. Date")]
        public Nullable<System.DateTime> Doc_Date { get; set; }
        public string strDoc_Date { get; set; }
        [Display(Name = "Refer.")]
        public string Reference { get; set; }
        public string Doc_Type { get; set; }
        public string Period { get; set; }
        public string D_C { get; set; }
        [Display(Name = "Credit Amount")]
        public string Amount_in_LC { get; set; }
        public string Amount_in_LC1 { get; set; }
        [Display(Name = "Remarks")]
        public string Text { get; set; }
        public Nullable<System.DateTime> Bline_Date { get; set; }
        public string strBline_Date { get; set; }
        [Display(Name = "Pay. Term")]
        public string PayT { get; set; }
        public string Sales_Doc { get; set; }
        public string Item { get; set; }
        public string Payment_reference { get; set; }
        public string Group { get; set; }
        public string Plan_group { get; set; }
        public string G_L_Acct { get; set; }
        [Display(Name = "Name")]
        public string Customer_Number_1 { get; set; }
        public string Document_Type { get; set; }
        public string Debit_Credit_Indicator { get; set; }
        public string Itm { get; set; }
        public string Recon_acct { get; set; }
        public string Group1 { get; set; }
        public string Cl { get; set; }
        public string Customer_classification { get; set; }
        public string Customer_Account_Group { get; set; }
        public string Pers_No { get; set; }
        public string Planning_group { get; set; }
        public string Last_name_First_name { get; set; }
        [Display(Name = "Curr")]
        public string Crcy { get; set; }
        [Display(Name = "Inv. Amount")]
        public string Amount { get; set; }
        public string Amount1 { get; set; }
        [Display(Name = "Date Diff")]
        public string Day1 { get; set; }
        public string Disc1 { get; set; }
        [Display(Name = "Key Manager")]
        public string KEYMANAGER { get; set; }
    }
}