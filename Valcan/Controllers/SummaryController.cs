using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Valcan.Models;

namespace Valcan.Controllers
{
    public class SummaryController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SummaryController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        // GET: Summary
        public ActionResult Index()
        {
            List<SummaryViewModel> orderlst = new List<SummaryViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                var fystart = DateTime.Now;
                var fyend = DateTime.Now;
                if (DateTime.Now.Month >= 4)
                {
                    fystart = new DateTime(DateTime.Now.Year, 4, 1);
                    fyend = new DateTime(DateTime.Now.Year + 1, 3, 31);
                }
                else
                {
                    fystart = new DateTime(DateTime.Now.Year - 1, 4, 1);
                    fyend = new DateTime(DateTime.Now.Year, 3, 31);
                }
                var dd = db.GetSummaryData(uid, fystart.Date, fyend.Date);


                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                //var data = (from om in db.Summary_Master
                //            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Sold_to_party equals cm.CUSTOMERNAME
                //            join km in db.KeyManagerMasters on cm.KEYMANAGER equals km.KeyManager
                //            join um in db.UserInKeyManagerMasters on km.ID equals um.KeyManagerID
                //            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                //            //group cm by cm.KEYMANAGER into pg
                //            select new
                //            {
                //                cm.KEYMANAGER,
                //                om.Sold_to_party,
                //                om.Annual_sales,
                //                om.Incoming_orders,
                //                om.Sales,
                //                om.Open_orders,
                //                om.Sold_to_pt,
                //                om.Date,
                //                um.UserID,
                //                um.KeyManagerMaster.KeyManager_Name,
                //                //om.Offer_Value_INR,
                //                //om.Order_Expected,
                //            }).ToList().Distinct();
                


                //var dd = data.GroupBy(x => x.KeyManager_Name).Select(
                //    x => new SummaryViewModel
                //    {
                //        KEYMANAGER = x.First().KeyManager_Name,
                //        Incoming_orders = x.Where(y => y.Date != null && y.Date.Value.Year == DateTime.Now.Year).Sum(c => Convert.ToDecimal(c.Incoming_orders)).ToString(),
                //        Sales = x.Where(y => y.Date != null && y.Date.Value.Date >= fystart.Date
                //        && y.Date.Value.Date <= fyend.Date
                //        ).Sum(c => Convert.ToDecimal(c.Sales)).ToString(),
                //        KEYMANAGER_id = x.First().KEYMANAGER,
                //        Open_orders = x.Sum(c => Convert.ToDecimal(c.Open_orders)).ToString(),
                //        //Sold_to_party = x.Count().ToString()
                //    }).ToList();

                //Incoming_orders = x.Where(y => y.Date != null && y.Date.Value.Year == DateTime.Now.Year).Sum(c => Convert.ToDecimal(c.Incoming_orders)).ToString(),
                //        Sales = x.Where(y => y.Date != null && y.Date.Value.Day >= fystart.Day && y.Date.Value.Year >= fystart.Year && y.Date.Value.Month >= fystart.Month
                //        && y.Date.Value.Day <= fyend.Day && y.Date.Value.Month <= fyend.Month && y.Date.Value.Year <= fyend.Year
                //        ).Sum(c => Convert.ToDecimal(c.Sales)).ToString(),

                //Incoming_orders = x.Where(y => y.Date != null && y.Date.Value.Year >= DateTime.Now.Year).Sum(c => Convert.ToDecimal(c.Incoming_orders)).ToString(),
                //        Sales = x.Where(y => y.Date != null && y.Date.Value.Day >= 1 && y.Date.Value.Month >= 4 && y.Date.Value.Year == DateTime.Now.Year
                //        && y.Date.Value.Day <= 31 && y.Date.Value.Month <= 3 && y.Date.Value.Year <= DateTime.Now.AddYears(1).Year
                //        ).Sum(c => Convert.ToDecimal(c.Sales)).ToString()

                foreach (var item in dd)
                {
                    SummaryViewModel order = new SummaryViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    //order.Sold_to_party = item.Sold_to_party;
                    //if (!string.IsNullOrEmpty(item.Annual_sales))
                    //{
                    //    var Annual_sales = item.Annual_sales.Split('.');
                    //    order.Annual_sales = Annual_sales[0];
                    //    order.Annual_sales_cast = Convert.ToDouble(order.Annual_sales);
                    //}
                    //order.Annual_sales = Convert.ToInt64(item.Annual_sales);
                    if (item.OrderIntake != null)
                    {
                        var iodata = item.OrderIntake.ToString().Split('.');
                        order.Incoming_orders = iodata[0];
                        order.Incoming_orders_cast = Convert.ToDouble(order.Incoming_orders);
                    }
                    //decimal IOval = Convert.ToDecimal(item.Incoming_orders);

                    //order.Incoming_orders = Convert.ToDecimal(decimal.Truncate(IOval));

                    if (item.sales !=null)
                    {
                        var Sales = item.sales.ToString().Split('.');
                        order.Sales = Sales[0];
                        order.Sales_cast = Convert.ToDouble(order.Sales);
                    }
                    //decimal Sval = Convert.ToDecimal(item.Sales);
                    //order.Sales = Convert.ToInt64(decimal.Truncate(Sval));
                    if (item.Open_orders != null)
                    {
                        var Open_orders = item.Open_orders.ToString().Split('.');
                        order.Open_orders = Open_orders[0];
                        order.Open_orders_cast = Convert.ToDouble(order.Open_orders);

                    }

                    //decimal Oval = Convert.ToDecimal(item.Open_orders);
                    //order.Open_orders = Convert.ToInt64(decimal.Truncate(Oval));

                    //order.Sold_to_pt = item.Sold_to_pt;
                    //order.KEYMANAGER_id = item.KEYMANAGER;
                    order.KEYMANAGER_id = item.keymanager_id.ToString();
                    orderlst.Add(order);
                    //foreach (var item in keyM)
                    //{
                    //    SummaryViewModel order = new SummaryViewModel();
                    //    order.KEYMANAGER = item.KeyManager_Name;
                    //    order.Sold_to_party = item.Sold_to_party;
                    //    order.Annual_sales = item.Annual_sales;
                    //    order.Incoming_orders = item.Incoming_orders;
                    //    order.Sales = item.Sales;
                    //    order.Open_orders = item.Open_orders;
                    //    order.Sold_to_pt = item.Sold_to_pt;
                    //    order.KEYMANAGER_id = item.KEYMANAGER;
                    //    orderlst.Add(order);
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(orderlst.OrderByDescending(x => x.Incoming_orders_cast).ThenByDescending(x => x.Sales_cast));
        }
        public ActionResult SearchFilter(SummarySearchViewModel model)
        {
            List<SummaryViewModel> summarylst = new List<SummaryViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.Summary_Master
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Sold_to_party equals cm.CUSTOMERNAME
                            join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals um.KeyManagerMaster.KeyManager
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Sold_to_party,
                                om.Annual_sales,
                                om.Incoming_orders,
                                om.Sales,
                                om.Open_orders,
                                om.Sold_to_pt,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();

                if (!string.IsNullOrEmpty(model.SCustName))
                {
                    model.SCustName = model.SCustName.ToLower();
                    if (model.SCustNameddl == 0)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower() == model.SCustName).ToList();
                    }
                    if (model.SCustNameddl == 1)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower() != model.SCustName).ToList();
                    }
                    if (model.SCustNameddl == 2)
                    {
                        string[] ids = model.SCustName.Split(',');
                        data = data.Where(x => ids.Contains(x.Sold_to_party.ToString())).ToList();
                    }
                    if (model.SCustNameddl == 3)
                    {
                        string[] ids = model.SCustName.Split(',');
                        data = data.Where(x => !ids.Contains(x.Sold_to_party.ToLower().ToString())).ToList();
                    }
                    if (model.SCustNameddl == 4)
                    {
                        data = data.Where(x => x.Sold_to_party.ToLower().Contains(model.SCustName)).ToList();
                    }
                    if (model.SCustNameddl == 5)
                    {
                        data = data.Where(x => !x.Sold_to_party.ToLower().Contains(model.SCustName)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.SCustCode))
                {
                    model.SCustCode = model.SCustCode.ToLower();
                    if (model.SCustCodeddl == 0)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() == model.SCustCode).ToList();
                    }
                    if (model.SCustCodeddl == 1)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() != model.SCustCode).ToList();
                    }
                    if (model.SCustCodeddl == 2)
                    {
                        string[] ids = model.SCustCode.Split(',');
                        data = data.Where(x => ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.SCustCodeddl == 3)
                    {
                        string[] ids = model.SCustCode.Split(',');
                        data = data.Where(x => !ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.SCustCodeddl == 4)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower().Contains(model.SCustCode)).ToList();
                    }
                    if (model.SCustCodeddl == 5)
                    {
                        data = data.Where(x => !x.Sold_to_pt.ToLower().Contains(model.SCustCode)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.SKeyManager))
                {
                    model.SKeyManager = model.SKeyManager.ToLower();
                    if (model.SKeyManagerddl == 0)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() == model.SKeyManager).ToList();
                    }
                    if (model.SKeyManagerddl == 1)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() != model.SKeyManager).ToList();
                    }
                    if (model.SKeyManagerddl == 2)
                    {
                        string[] ids = model.SKeyManager.Split(',');
                        data = data.Where(x => ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.SKeyManagerddl == 3)
                    {
                        string[] ids = model.SKeyManager.Split(',');
                        data = data.Where(x => !ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.SKeyManagerddl == 4)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower().Contains(model.SKeyManager)).ToList();
                    }
                    if (model.SKeyManagerddl == 5)
                    {
                        data = data.Where(x => !x.KeyManager_Name.ToLower().Contains(model.SKeyManager)).ToList();
                    }
                }

                foreach (var item in data)
                {
                    SummaryViewModel order = new SummaryViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    order.Sold_to_party = item.Sold_to_party;
                    if (!string.IsNullOrEmpty(item.Annual_sales))
                    {
                        var Annual_sales = item.Annual_sales.Split('.');
                        order.Annual_sales = Annual_sales[0];
                        order.Annual_sales_cast = Convert.ToDouble(order.Annual_sales);
                    }
                    //order.Annual_sales = Convert.ToDecimal(item.Annual_sales);
                    //order.Incoming_orders = Convert.ToDecimal(item.Incoming_orders);
                    if (!string.IsNullOrEmpty(item.Incoming_orders))
                    {
                        var io = item.Incoming_orders.Split('.');
                        order.Incoming_orders = io[0];
                        order.Incoming_orders_cast = Convert.ToDouble(order.Incoming_orders);
                    }
                    if (!string.IsNullOrEmpty(item.Sales))
                    {
                        var Sales = item.Sales.Split('.');
                        order.Sales = Sales[0];
                        order.Sales_cast = Convert.ToDouble(order.Sales);
                    }
                    //order.Sales = Convert.ToDecimal(item.Sales);
                    if (!string.IsNullOrEmpty(item.Open_orders))
                    {
                        var Open_orders = item.Open_orders.Split('.');
                        order.Open_orders = Open_orders[0];
                        order.Open_orders_cast = Convert.ToDouble(order.Open_orders);
                    }
                    //order.Open_orders = Convert.ToDecimal(item.Open_orders);
                    order.Sold_to_pt = item.Sold_to_pt;

                    summarylst.Add(order);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return PartialView("~/Views/Summary/_PartialTableData.cshtml", summarylst);
        }

        public ActionResult GetCustomerDetailsbyKeyManager(string KAMId, string KAMName)
        {
            var uid = (int)Session["UserID"];
            var fystart = DateTime.Now;
            var fyend = DateTime.Now;
            if (DateTime.Now.Month >= 4)
            {
                fystart = new DateTime(DateTime.Now.Year, 4, 1);
                fyend = new DateTime(DateTime.Now.Year + 1, 3, 31);
            }
            else
            {
                fystart = new DateTime(DateTime.Now.Year - 1, 4, 1);
                fyend = new DateTime(DateTime.Now.Year, 3, 31);
            }
            List<SummaryViewModel> orderlst = new List<SummaryViewModel>();

            var dd = db.GetClientwiseSummaryData(uid, fystart.Date, fyend.Date, KAMId);
            //var data = (from om in db.Summary_Master
            //            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Sold_to_party equals cm.CUSTOMERNAME
            //            join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals KAMId
            //            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
            //            //group cm by cm.KEYMANAGER into pg
            //            select new
            //            {
            //                cm.KEYMANAGER,
            //                om.Sold_to_party,
            //                om.Annual_sales,
            //                om.Incoming_orders,
            //                om.Sales,
            //                om.Open_orders,
            //                om.Sold_to_pt,
            //                om.Date,
            //                um.UserID,
            //                //um.KeyManagerMaster.KeyManager_Name,
            //                //om.Offer_Value_INR,
            //                //om.Order_Expected,
            //            }).ToList().Distinct();

            //var dd = data.GroupBy(x => x.Sold_to_party).Select(
            //        x => new SummaryViewModel
            //        {
            //            KEYMANAGER = x.First().KEYMANAGER,
            //            Sold_to_party = x.First().Sold_to_party,
            //            Sold_to_pt = x.First().Sold_to_pt,
            //            Incoming_orders = x.Where(y => y.Date != null && y.Date.Value.Year == DateTime.Now.Year).Sum(c => Convert.ToDecimal(c.Incoming_orders)).ToString(),
            //            Sales = x.Where(y => y.Date != null && y.Date.Value >= fystart
            //            && y.Date.Value <= fyend
            //            ).Sum(c => Convert.ToDecimal(c.Sales)).ToString(),
            //            KEYMANAGER_id = x.First().KEYMANAGER,
            //            Open_orders = x.Sum(c => Convert.ToDecimal(c.Open_orders)).ToString(),
            //            //Sold_to_party = x.Count().ToString()
            //        }).ToList();

            foreach (var item in dd)
            {
                SummaryViewModel order = new SummaryViewModel();
                order.KEYMANAGER = KAMName;
                order.Sold_to_party = item.Sold_to_party;
                if (item.Annual_sales !=null)
                {
                    var Annual_sales = item.Annual_sales.ToString().Split('.');
                    order.Annual_sales = Annual_sales[0];
                    order.Annual_sales_cast = Convert.ToDouble(order.Annual_sales);
                }
               // order.Annual_sales = Convert.ToDecimal(item.Annual_sales);

                if (item.OrderIntake != null)
                {
                    var io = item.OrderIntake.ToString().Split('.');
                    order.Incoming_orders = io[0];
                    order.Incoming_orders_cast = Convert.ToDouble(order.Incoming_orders);
                }
                if (item.sales !=null)
                {
                    var Sales = item.sales.ToString().Split('.');
                    order.Sales = Sales[0];
                    order.Sales_cast = Convert.ToDouble(order.Sales);
                }
                //order.Sales = Convert.ToDecimal(item.Sales);
                if (item.Open_orders != null)
                {
                    var Open_orders = item.Open_orders.ToString().Split('.');
                    order.Open_orders = Open_orders[0];
                    order.Open_orders_cast = Convert.ToDouble(order.Open_orders);
                }
                //order.Open_orders = Convert.ToDecimal(item.Open_orders);
                order.Sold_to_pt = item.Sold_to_pt;
                order.KEYMANAGER_id = item.KEYMANAGER;
                orderlst.Add(order);
            }
            return PartialView("~/Views/Summary/_PartialCustomerData.cshtml", orderlst.OrderByDescending(x => x.Incoming_orders_cast).ThenByDescending(x => x.Sales_cast));
        }
    }
}