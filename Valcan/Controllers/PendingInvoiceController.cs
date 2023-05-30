using DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Valcan.Models;

namespace Valcan.Controllers
{
    public class PendingInvoiceController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PendingInvoiceController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        // GET: Summary
        public ActionResult Index()
        {
            List<PendingInvoiceViewModel> orderlst = new List<PendingInvoiceViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.Pending_invoice_Master
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Customer_Number_1 equals cm.CUSTOMERNAME
                             join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals um.KeyManagerMaster.KeyManager
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Customer)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Customer,
                                om.Customer_Number_1,
                                om.PayT,
                                om.DocumentNo,
                                om.Text,
                                om.Crcy,
                                om.Doc_Date,
                                om.Reference,
                                om.Amount,
                                om.Amount_in_LC,
                                om.Day1,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();
                
                foreach (var item in data)
                {
                    PendingInvoiceViewModel order = new PendingInvoiceViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    order.Customer = item.Customer;
                    order.Customer_Number_1 = item.Customer_Number_1;
                    order.PayT = item.PayT;
                    order.DocumentNo = item.DocumentNo;
                    order.Text = item.Text;
                    order.Doc_Date = item.Doc_Date;
                    order.strDoc_Date = item.Doc_Date.HasValue ? item.Doc_Date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Reference = item.Reference;
                    order.Amount = item.Amount;
                    order.Amount_in_LC = item.Amount_in_LC;
                    order.Day1 = item.Day1;

                    orderlst.Add(order);
                }
                //var s = orderlst.ToList().Sum(x=>x.Amount_in_LC)
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(orderlst);
        }
        public ActionResult SearchFilter(PendingInvoiceSearchViewModel model)
        {
            List<PendingInvoiceViewModel> offerlst = new List<PendingInvoiceViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.Pending_invoice_Master
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Customer_Number_1 equals cm.CUSTOMERNAME
                            join um in db.UserInKeyManagerMasters on cm.KEYMANAGER equals um.KeyManagerMaster.KeyManager
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Customer)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Customer,
                                om.Customer_Number_1,
                                om.PayT,
                                om.DocumentNo,
                                om.Text,
                                om.Crcy,
                                om.Doc_Date,
                                om.Reference,
                                om.Amount,
                                om.Amount_in_LC,
                                om.Day1,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();

                if (!string.IsNullOrEmpty(model.InvoiceNo))
                {
                    model.InvoiceNo = model.InvoiceNo.ToLower();
                    if (model.Invoiceddl == 0)
                    {
                        data = data.Where(x => x.DocumentNo.ToLower() == model.InvoiceNo).ToList();
                    }
                    if (model.Invoiceddl == 1)
                    {
                        data = data.Where(x => x.DocumentNo.ToLower() != model.InvoiceNo).ToList();
                    }
                    if (model.Invoiceddl == 2)
                    {
                        string[] ids = model.InvoiceNo.Split(',');
                        data = data.Where(x => ids.Contains(x.DocumentNo.ToLower().ToString())).ToList();
                    }
                    if (model.Invoiceddl == 3)
                    {
                        string[] ids = model.InvoiceNo.Split(',');
                        data = data.Where(x => !ids.Contains(x.DocumentNo.ToLower().ToString())).ToList();
                    }
                    if (model.Invoiceddl == 4)
                    {
                        data = data.Where(x => x.DocumentNo.ToLower().Contains(model.InvoiceNo)).ToList();
                    }
                    if (model.Invoiceddl == 5)
                    {
                        data = data.Where(x => !x.DocumentNo.ToLower().Contains(model.InvoiceNo)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.PICustName))
                {
                    model.PICustName = model.PICustName.ToLower();
                    if (model.PICustNameddl == 0)
                    {
                        data = data.Where(x => x.Customer_Number_1.ToLower() == model.PICustName).ToList();
                    }
                    if (model.PICustNameddl == 1)
                    {
                        data = data.Where(x => x.Customer_Number_1.ToLower() != model.PICustName).ToList();
                    }
                    if (model.PICustNameddl == 2)
                    {
                        string[] ids = model.PICustName.Split(',');
                        data = data.Where(x => ids.Contains(x.Customer_Number_1.ToLower().ToString())).ToList();
                    }
                    if (model.PICustNameddl == 3)
                    {
                        string[] ids = model.PICustName.Split(',');
                        data = data.Where(x => !ids.Contains(x.Customer_Number_1.ToLower().ToString())).ToList();
                    }
                    if (model.PICustNameddl == 4)
                    {
                        data = data.Where(x => x.Customer_Number_1.ToLower().Contains(model.PICustName)).ToList();
                    }
                    if (model.PICustNameddl == 5)
                    {
                        data = data.Where(x => !x.Customer_Number_1.ToLower().Contains(model.PICustName)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.PICustCode))
                {
                    model.PICustCode = model.PICustCode.ToLower();
                    if (model.PICustCodeddl == 0)
                    {
                        data = data.Where(x => x.Customer.ToLower() == model.PICustCode).ToList();
                    }
                    if (model.PICustCodeddl == 1)
                    {
                        data = data.Where(x => x.Customer.ToLower() != model.PICustCode).ToList();
                    }
                    if (model.PICustCodeddl == 2)
                    {
                        string[] ids = model.PICustCode.Split(',');
                        data = data.Where(x => ids.Contains(x.Customer.ToLower().ToString())).ToList();
                    }
                    if (model.PICustCodeddl == 3)
                    {
                        string[] ids = model.PICustCode.Split(',');
                        data = data.Where(x => !ids.Contains(x.Customer.ToLower().ToString())).ToList();
                    }
                    if (model.PICustCodeddl == 4)
                    {
                        data = data.Where(x => x.Customer.ToLower().Contains(model.PICustCode)).ToList();
                    }
                    if (model.PICustCodeddl == 5)
                    {
                        data = data.Where(x => !x.Customer.ToLower().Contains(model.PICustCode)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.PIKeyManager))
                {
                    model.PIKeyManager = model.PIKeyManager.ToLower();
                    if (model.PIKeyManagerddl == 0)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() == model.PIKeyManager).ToList();
                    }
                    if (model.PIKeyManagerddl == 1)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() != model.PIKeyManager).ToList();
                    }
                    if (model.PIKeyManagerddl == 2)
                    {
                        string[] ids = model.PIKeyManager.Split(',');
                        data = data.Where(x => ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.PIKeyManagerddl == 3)
                    {
                        string[] ids = model.PIKeyManager.Split(',');
                        data = data.Where(x => !ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.PIKeyManagerddl == 4)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower().Contains(model.PIKeyManager)).ToList();
                    }
                    if (model.PIKeyManagerddl == 5)
                    {
                        data = data.Where(x => !x.KeyManager_Name.ToLower().Contains(model.PIKeyManager)).ToList();
                    }
                }

                foreach (var item in data)
                {
                    PendingInvoiceViewModel order = new PendingInvoiceViewModel();
                    order.KEYMANAGER = item.KeyManager_Name;
                    order.Customer = item.Customer;
                    order.Customer_Number_1 = item.Customer_Number_1;
                    order.PayT = item.PayT;
                    order.DocumentNo = item.DocumentNo;
                    order.Text = item.Text;
                    order.Doc_Date = item.Doc_Date;
                    order.strDoc_Date = item.Doc_Date.HasValue ? item.Doc_Date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    order.Reference = item.Reference;
                    order.Amount = item.Amount;
                    order.Amount_in_LC = item.Amount_in_LC;
                    order.Day1 = item.Day1;

                    offerlst.Add(order);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return PartialView("~/Views/PendingInvoice/_PartialTableData.cshtml", offerlst);
        }
    }
}