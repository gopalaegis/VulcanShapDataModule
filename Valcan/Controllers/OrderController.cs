using DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Valcan.Models;

namespace Valcan.Controllers
{
    public class OrderController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(OrderController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        // GET: Offer
        public ActionResult Index()
        {
            List<OrderViewModel> orderlst = new List<OrderViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.OrderMasters
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Sold_to_party equals cm.CUSTOMERNAME
                            join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals um.KeyManagerMaster.KeyManager
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Sales_Doc,
                                om.Item,
                                om.Sch_Line,
                                om.MRP_Controller,
                                om.Dv,
                                om.Sold_to_pt,
                                om.Sold_to_party,
                                om.Material,
                                om.Description,
                                om.Order_Qty,
                                om.Open_Qty,
                                om.Unit_Basic_Price,
                                om.Curr,
                                om.Total_Price_IRS,
                                om.Remarks_from_Production,
                                om.Purchase_order_no,
                                om.PO_date,
                                om.Your_Ref,
                                om.Material_group_1,
                                om.Material_group_2,
                                om.Customer_group_5,
                                om.Created_on,
                                om.Req_dlv_dt,
                                om.IncoT,
                                om.Inco_2,
                                om.Week_Changed,
                                om.Remarks_from_Marketing,
                                om.Marketing_Sales_Week,
                                om.Pre_Dispatch_Inspection,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();
                CultureInfo cul = CultureInfo.CurrentCulture;
                foreach (var item in data)
                {
                    //var firstDayWeek = cul.Calendar.GetWeekOfYear(
                    //    item.Req_dlv_dt,
                    //    CalendarWeekRule.FirstDay,
                    //    DayOfWeek.Monday);
                    var Req_dlv_dt =  Convert.ToDateTime(item.Req_dlv_dt);
                    int weekNum = cul.Calendar.GetWeekOfYear(
                       Req_dlv_dt,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday);

                    int year = weekNum == 52 && Req_dlv_dt.Month == 1 ? Req_dlv_dt.Year - 1 : Req_dlv_dt.Year;

                    OrderViewModel order = new OrderViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    order.Sales_Doc = item.Sales_Doc;
                    order.Item = item.Item;
                    order.Sch_Line = item.Sch_Line;
                    order.MRP_Controller = item.MRP_Controller;
                    order.Dv = item.Dv;
                    order.Sold_to_pt = item.Sold_to_pt;
                    order.Sold_to_party = item.Sold_to_party;
                    order.Material = item.Material;
                    order.Description = item.Description;
                    order.Order_Qty = item.Order_Qty;
                    order.Open_Qty = item.Open_Qty;
                    order.Unit_Basic_Price = item.Unit_Basic_Price;
                    order.Curr = item.Curr;
                    order.Total_Price_IRS = item.Total_Price_IRS;
                    order.Remarks_from_Production = item.Remarks_from_Production;
                    order.Purchase_order_no = item.Purchase_order_no;
                    order.PO_date = item.PO_date;
                    order.strPO_date = item.PO_date.HasValue ? item.PO_date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Your_Ref = item.Your_Ref;
                    order.Material_group_1 = item.Material_group_1;
                    order.Material_group_2 = item.Material_group_2;
                    order.Customer_group_5 = item.Customer_group_5;
                    order.Created_on = item.Created_on;
                    order.strCreated_on = item.Created_on.HasValue ? item.Created_on.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Req_dlv_dt = item.Req_dlv_dt;
                    order.strReq_dlv_dt = item.Req_dlv_dt.HasValue ? item.Req_dlv_dt.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Cust_Req_Week = weekNum.ToString();
                    order.Cust_Req_Yar = year.ToString();
                    order.IncoT = item.IncoT;
                    order.Inco_2 = item.Inco_2;
                    order.Week_Changed = item.Week_Changed;
                    order.Remarks_from_Marketing = item.Remarks_from_Marketing;
                    order.Marketing_Sales_Week = item.Marketing_Sales_Week;
                    order.Pre_Dispatch_Inspection = item.Pre_Dispatch_Inspection;

                    orderlst.Add(order);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(orderlst);
        }
        public ActionResult SearchFilter(OrderSearchViewModel model)
        {
            List<OrderViewModel> orderlst = new List<OrderViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.OrderMasters
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Sold_to_party equals cm.CUSTOMERNAME
                            join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals um.KeyManagerMaster.KeyManager
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Sales_Doc,
                                om.Item,
                                om.Sch_Line,
                                om.MRP_Controller,
                                om.Dv,
                                om.Sold_to_pt,
                                om.Sold_to_party,
                                om.Material,
                                om.Description,
                                om.Order_Qty,
                                om.Open_Qty,
                                om.Unit_Basic_Price,
                                om.Curr,
                                om.Total_Price_IRS,
                                om.Remarks_from_Production,
                                om.Purchase_order_no,
                                om.PO_date,
                                om.Your_Ref,
                                om.Material_group_1,
                                om.Material_group_2,
                                om.Customer_group_5,
                                om.Created_on,
                                om.Req_dlv_dt,
                                om.IncoT,
                                om.Inco_2,
                                om.Week_Changed,
                                om.Remarks_from_Marketing,
                                om.Marketing_Sales_Week,
                                om.Pre_Dispatch_Inspection,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();

                if (!string.IsNullOrEmpty(model.SoNo))
                {
                    model.SoNo = model.SoNo.ToLower();
                    if (model.SoNoddl == 0)
                    {
                        data = data.Where(x => x.Sales_Doc.ToLower() == model.SoNo).ToList();
                    }
                    if (model.SoNoddl == 1)
                    {
                        data = data.Where(x => x.Sales_Doc.ToLower() != model.SoNo).ToList();
                    }
                    if (model.SoNoddl == 2)
                    {
                        string[] ids = model.SoNo.Split(',');
                        data = data.Where(x => ids.Contains(x.Sales_Doc.ToLower().ToString())).ToList();
                    }
                    if (model.SoNoddl == 3)
                    {
                        string[] ids = model.SoNo.Split(',');
                        data = data.Where(x => !ids.Contains(x.Sales_Doc.ToLower().ToString())).ToList();
                    }
                    if (model.SoNoddl == 4)
                    {
                        data = data.Where(x => x.Sales_Doc.ToLower().Contains(model.SoNo)).ToList();
                    }
                    if (model.SoNoddl == 5)
                    {
                        data = data.Where(x => !x.Sales_Doc.ToLower().Contains(model.SoNo)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.OCustName))
                {
                    model.OCustName = model.OCustName.ToLower();
                    if (model.OCustNameddl == 0)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower() == model.OCustName).ToList();
                    }
                    if (model.OCustNameddl == 1)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower() != model.OCustName).ToList();
                    }
                    if (model.OCustNameddl == 2)
                    {
                        string[] ids = model.OCustName.Split(',');
                        data = data.Where(x => ids.Contains(x.Sold_to_party.ToLower().ToString())).ToList();
                    }
                    if (model.OCustNameddl == 3)
                    {
                        string[] ids = model.OCustName.Split(',');
                        data = data.Where(x => !ids.Contains(x.Sold_to_party.ToLower().ToString())).ToList();
                    }
                    if (model.OCustNameddl == 4)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower().Contains(model.OCustName)).ToList();
                    }
                    if (model.OCustNameddl == 5)
                    {
                        data = data.Where(x => !x.Sold_to_party.ToLower().Contains(model.OCustName)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.OCustCode))
                {
                    model.OCustCode = model.OCustCode.ToLower();
                    if (model.OCustCodeddl == 0)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() == model.OCustCode).ToList();
                    }
                    if (model.OCustCodeddl == 1)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() != model.OCustCode).ToList();
                    }
                    if (model.OCustCodeddl == 2)
                    {
                        string[] ids = model.OCustCode.Split(',');
                        data = data.Where(x => ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.OCustCodeddl == 3)
                    {
                        string[] ids = model.OCustCode.Split(',');
                        data = data.Where(x => !ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.OCustCodeddl == 4)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower().Contains(model.OCustCode)).ToList();
                    }
                    if (model.OCustCodeddl == 5)
                    {
                        data = data.Where(x => !x.Sold_to_pt.ToLower().Contains(model.OCustCode)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.OMaterialCode))
                {
                    model.OMaterialCode = model.OMaterialCode.ToLower();
                    if (model.OMaterialCodeddl == 0)
                    {
                        data = data.Where(x => x.Material.ToLower() == model.OMaterialCode).ToList();
                    }
                    if (model.OMaterialCodeddl == 1)
                    {
                        data = data.Where(x => x.Material.ToLower() != model.OMaterialCode).ToList();
                    }
                    if (model.OMaterialCodeddl == 2)
                    {
                        string[] ids = model.OMaterialCode.Split(',');
                        data = data.Where(x => ids.Contains(x.Material.ToLower().ToString())).ToList();
                    }
                    if (model.OMaterialCodeddl == 3)
                    {
                        string[] ids = model.OMaterialCode.Split(',');
                        data = data.Where(x => !ids.Contains(x.Material.ToLower().ToString())).ToList();
                    }
                    if (model.OMaterialCodeddl == 4)
                    {
                        data = data.Where(x => x.Material.ToLower().Contains(model.OMaterialCode)).ToList();
                    }
                    if (model.OMaterialCodeddl == 5)
                    {
                        data = data.Where(x => !x.Material.ToLower().Contains(model.OMaterialCode)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.OMaterialDesc))
                {
                    model.OMaterialDesc = model.OMaterialDesc.ToLower();
                    if (model.OMaterialDescddl == 0)
                    {
                        data = data.Where(x => x.Description.ToLower() == model.OMaterialDesc).ToList();
                    }
                    if (model.OMaterialDescddl == 1)
                    {
                        data = data.Where(x => x.Description.ToLower() != model.OMaterialDesc).ToList();
                    }
                    if (model.OMaterialDescddl == 2)
                    {
                        string[] ids = model.OMaterialDesc.Split(',');
                        data = data.Where(x => ids.Contains(x.Description.ToLower().ToString())).ToList();
                    }
                    if (model.OMaterialDescddl == 3)
                    {
                        string[] ids = model.OMaterialDesc.Split(',');
                        data = data.Where(x => !ids.Contains(x.Description.ToLower().ToString())).ToList();
                    }
                    if (model.OMaterialDescddl == 4)
                    {
                        data = data.Where(x => x.Description.ToLower().Contains(model.OMaterialDesc)).ToList();
                    }
                    if (model.OMaterialDescddl == 5)
                    {
                        data = data.Where(x => !x.Description.ToLower().Contains(model.OMaterialDesc)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.OKeyManager))
                {
                    model.OKeyManager = model.OKeyManager.ToLower();
                    if (model.OKeyManagerddl == 0)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() == model.OKeyManager).ToList();
                    }
                    if (model.OKeyManagerddl == 1)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() != model.OKeyManager).ToList();
                    }
                    if (model.OKeyManagerddl == 2)
                    {
                        string[] ids = model.OKeyManager.Split(',');
                        data = data.Where(x => ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.OKeyManagerddl == 3)
                    {
                        string[] ids = model.OKeyManager.Split(',');
                        data = data.Where(x => !ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.OKeyManagerddl == 4)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower().Contains(model.OKeyManager)).ToList();
                    }
                    if (model.OKeyManagerddl == 5)
                    {
                        data = data.Where(x => !x.KeyManager_Name.ToLower().Contains(model.OKeyManager)).ToList();
                    }
                }

                CultureInfo cul = CultureInfo.CurrentCulture;
                foreach (var item in data)
                {
                    //var firstDayWeek = cul.Calendar.GetWeekOfYear(
                    //    item.Req_dlv_dt,
                    //    CalendarWeekRule.FirstDay,
                    //    DayOfWeek.Monday);
                    var Req_dlv_dt = Convert.ToDateTime(item.Req_dlv_dt);
                    int weekNum = cul.Calendar.GetWeekOfYear(
                       Req_dlv_dt,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday);

                    int year = weekNum == 52 && Req_dlv_dt.Month == 1 ? Req_dlv_dt.Year - 1 : Req_dlv_dt.Year;

                    OrderViewModel order = new OrderViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    order.Sales_Doc = item.Sales_Doc;
                    order.Item = item.Item;
                    order.Sch_Line = item.Sch_Line;
                    order.MRP_Controller = item.MRP_Controller;
                    order.Dv = item.Dv;
                    order.Sold_to_pt = item.Sold_to_pt;
                    order.Sold_to_party = item.Sold_to_party;
                    order.Material = item.Material;
                    order.Description = item.Description;
                    order.Order_Qty = item.Order_Qty;
                    order.Open_Qty = item.Open_Qty;
                    order.Unit_Basic_Price = item.Unit_Basic_Price;
                    order.Curr = item.Curr;
                    order.Total_Price_IRS = item.Total_Price_IRS;
                    order.Remarks_from_Production = item.Remarks_from_Production;
                    order.Purchase_order_no = item.Purchase_order_no;
                    order.PO_date = item.PO_date;
                    order.strPO_date = item.PO_date.HasValue ? item.PO_date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Your_Ref = item.Your_Ref;
                    order.Material_group_1 = item.Material_group_1;
                    order.Material_group_2 = item.Material_group_2;
                    order.Customer_group_5 = item.Customer_group_5;
                    order.Created_on = item.Created_on;
                    order.strCreated_on = item.Created_on.HasValue ? item.Created_on.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Req_dlv_dt = item.Req_dlv_dt;
                    order.strReq_dlv_dt = item.Req_dlv_dt.HasValue ? item.Req_dlv_dt.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Cust_Req_Week = weekNum.ToString();
                    order.Cust_Req_Yar = year.ToString();
                    order.IncoT = item.IncoT;
                    order.Inco_2 = item.Inco_2;
                    order.Week_Changed = item.Week_Changed;
                    order.Remarks_from_Marketing = item.Remarks_from_Marketing;
                    order.Marketing_Sales_Week = item.Marketing_Sales_Week;
                    order.Pre_Dispatch_Inspection = item.Pre_Dispatch_Inspection;

                    orderlst.Add(order);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return PartialView("~/Views/Order/_PartialTableData.cshtml", orderlst);
        }
    }
}