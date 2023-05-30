using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class SummaryViewModel
    {
        public int ID { get; set; }
        public string Dv { get; set; }
        public string Division { get; set; }
        public string Sold_to_pt { get; set; }
        public string Sold_to_party { get; set; }
        public string Material { get; set; }
        public string Material_Number { get; set; }
        public string Product_hierarchy { get; set; }
        public string Pers_No { get; set; }
        public string Last_name_First_name { get; set; }
        public string Group { get; set; }
        public string Group_1 { get; set; }
        public string Customer_Account_Group { get; set; }
        public string Cl { get; set; }
        public string Customer_classification { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Indus { get; set; }
        public string Industry_key { get; set; }
        public string Matl_Group { get; set; }
        public string Product_hierarchy_2 { get; set; }
        public string Plan_group { get; set; }
        public string Material_Group { get; set; }
        public string Material_Type { get; set; }
        public string Annual_sales { get; set; }
        public double Annual_sales_cast { get; set; }
        public string Annual_sales_cur { get; set; }
        public string Incoming_orders { get; set; }
        public double Incoming_orders_cast { get; set; }
        public string Incoming_orders_cur { get; set; }
        public string Incoming_orders_quantity { get; set; }
        public string Incoming_orders_quantity_unit { get; set; }
        public string Sales { get; set; }
        public double Sales_cast { get; set; }
        public string Sales_cur { get; set; }
        public string Invoiced_Quantity { get; set; }
        public string Invoiced_Quantity_unit { get; set; }
        public string Open_orders { get; set; }
        public double Open_orders_cast { get; set; }
        public string Open_orders_cur { get; set; }
        public string Open_orders_quantity { get; set; }
        public string Open_orders_quantity_unit { get; set; }
        public string City { get; set; }
        public string Region_State_Province_Count { get; set; }
        public string Country_Key { get; set; }
        public string Ind_Code_1 { get; set; }
        public string Ind_code_2 { get; set; }
        public string Ind_code_3 { get; set; }
        public string MRPC { get; set; }
        public string MRP_Controller_Materials_Plan { get; set; }
        public string Product_hierarchy_3 { get; set; }
        public string Number_of_the_level_in_the_pro { get; set; }
        public string KEYMANAGER { get; set; }
        public string KEYMANAGER_id { get; set; }
        //public CustomerSummaryViewModel CustSummaryViewModel { get; set; }
    }

    //public class CustomerSummaryViewModel
    //{
    //    public string KEYMANAGER { get; set; }
    //    public Int64 Annual_sales { get; set; }
    //    public Int64 Incoming_orders { get; set; }
    //    public Int64 Sales { get; set; }
    //    public Int64 Open_orders { get; set; }
    //    public string Sold_to_pt { get; set; }
    //    public string Sold_to_party { get; set; }
    //    public string KEYMANAGER_id { get; set; }
    //}
}