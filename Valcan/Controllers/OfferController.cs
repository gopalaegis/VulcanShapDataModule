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
    public class OfferController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(OfferController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        // GET: Offer
        public ActionResult Index()
        {
            List<OfferViewModel> offerlst = new List<OfferViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                //    var data = (from om in db.OfferMasters
                //                join cm in db.CustomerMasters on om.Name_1 equals cm.CUSTOMERNAME
                //                join km in db.KeyManagerMasters on cm.KEYMANAGER equals km.KeyManager
                //                join um in db.UserInKeyManagerMasters on km.ID equals um.KeyManagerID
                //                where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                //                select new
                //                {
                //                    cm.KEYMANAGER,
                //                    om.Sold_to_pt,
                //                    om.Name_1,
                //                    om.Document,
                //                    om.Item,
                //                    om.Doc_Date,
                //                    om.Purchase_order_number,
                //                    om.Material,
                //                    om.Material_Description,
                //                    om.ConfirmQty,
                //                    om.Net_price,
                //                    om.Net_Value,
                //                    om.Curr,
                //                    om.Exch_Rate,
                //                    om.Prb,
                //                    um.UserID,
                //                   //um.KeyManagerMaster.KeyManager_Name,
                //                   km.KeyManager_Name
                //                    //om.Offer_Value_INR,
                //                    //om.Order_Expected,
                //                }).ToList().Distinct();

                //    foreach (var item in data)
                //    {
                //        var Offer_Value_INR = Convert.ToDecimal(1);
                //        var Order_Expected = Convert.ToDecimal(1);
                //        try
                //        {
                //            if (string.IsNullOrEmpty(item.Exch_Rate))
                //            {
                //                if (!string.IsNullOrEmpty(item.ConfirmQty))
                //                {
                //                    Offer_Value_INR = Convert.ToDecimal(item.Net_price.ToString()) * Convert.ToDecimal(1) * Convert.ToDecimal(item.ConfirmQty);
                //                }
                //                else
                //                {
                //                    Offer_Value_INR = Convert.ToDecimal(item.Net_price.ToString()) * Convert.ToDecimal(1);
                //                }

                //                Order_Expected = (Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(1) * Convert.ToDecimal(item.Prb))/100;
                //            }
                //            else
                //            {
                //                if (!string.IsNullOrEmpty(item.ConfirmQty))
                //                {
                //                    Offer_Value_INR = Convert.ToDecimal(item.Net_price.ToString()) * Convert.ToDecimal(item.Exch_Rate.ToString()) * Convert.ToDecimal(item.ConfirmQty);
                //                }
                //                else
                //                {
                //                    Offer_Value_INR = Convert.ToDecimal(item.Net_price.ToString()) * Convert.ToDecimal(item.Exch_Rate.ToString());
                //                }
                //                Order_Expected = (Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(item.Exch_Rate.ToString()) * Convert.ToDecimal(item.Prb))/100;
                //            }
                //        }
                //        catch (Exception ex)
                //        {

                //            logger.Error("Offer_Value_INR or Order_Expected not valid error is : " + ex.Message);
                //        }
                //        OfferViewModel offer = new OfferViewModel();
                //        offer.Key_Managr = item.KeyManager_Name;
                //        offer.Sold_to_pt = item.Sold_to_pt;
                //        offer.Name_1 = item.Name_1;
                //        offer.Document = item.Document;
                //        offer.Item = item.Item;
                //        offer.Doc_Date = item.Doc_Date;
                //        offer.strDoc_Date = item.Doc_Date.HasValue ? item.Doc_Date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                //        offer.Purchase_order_number = item.Purchase_order_number;
                //        offer.Material = item.Material;
                //        offer.Material_Description = item.Material_Description;
                //        offer.ConfirmQty = item.ConfirmQty;
                //        offer.Net_price = item.Net_price;
                //        offer.Net_Value = item.Net_Value;
                //        offer.Curr = item.Curr;
                //        offer.Exch_Rate = item.Exch_Rate;
                //        offer.Offer_Value_INR = Offer_Value_INR.ToString();
                //        offer.Order_Expected = Order_Expected.ToString();

                //        offerlst.Add(offer);
                //    }
                //}
                var data = (from om in db.OfferMasters
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Name_1 equals cm.CUSTOMERNAME
                            join km in db.KeyManagerMasters on cm.KEYMANAGER equals km.KeyManager
                            join um in db.UserInKeyManagerMasters on km.ID equals um.KeyManagerID
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Sold_to_pt,
                                om.Name_1,
                                om.Document,
                                om.Item,
                                om.Doc_Date,
                                om.Purchase_order_number,
                                om.Material,
                                om.Material_Description,
                                om.ConfirmQty,
                                om.Net_price,
                                om.Net_Value,
                                om.Prb,
                                om.Curr,
                                om.Exch_Rate,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();

                foreach (var item in data)
                {
                    var Offer_Value_INR = Convert.ToDecimal(1);
                    var Order_Expected = Convert.ToDecimal(1);
                    try
                    {
                        string prb = item.Prb;
                        string exrate = item.Exch_Rate;
                        if (string.IsNullOrEmpty(prb))
                            prb = "1";
                        if (string.IsNullOrEmpty(exrate))
                            exrate = "1";

                        Offer_Value_INR = Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(exrate);
                        Order_Expected = (Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(exrate) * Convert.ToDecimal(prb)) / 100;

                    }
                    catch (Exception ex)
                    {

                        logger.Error("Offer_Value_INR or Order_Expected not valid error is : " + ex.Message);
                    }
                    OfferViewModel offer = new OfferViewModel();
                    offer.Key_Managr = item.KeyManager_Name;
                    offer.Sold_to_pt = item.Sold_to_pt;
                    offer.Name_1 = item.Name_1;
                    offer.Document = item.Document;
                    offer.Item = item.Item;
                    offer.Doc_Date = item.Doc_Date;
                    offer.strDoc_Date = item.Doc_Date.HasValue ? item.Doc_Date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    offer.Purchase_order_number = item.Purchase_order_number;
                    offer.Material = item.Material;
                    offer.Material_Description = item.Material_Description;
                    offer.ConfirmQty = item.ConfirmQty;
                    offer.Net_price = item.Net_price;
                    offer.Net_Value = item.Net_Value;
                    offer.Curr = item.Curr;
                    offer.Exch_Rate = item.Exch_Rate;
                    offer.Offer_Value_INR = Offer_Value_INR.ToString();
                    offer.Order_Expected = Order_Expected.ToString();

                    offerlst.Add(offer);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(offerlst);
        }
        public ActionResult SearchFilter(OfferSearchViewModel model)
        {
            List<OfferViewModel> offerlst = new List<OfferViewModel>();
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var uid = (int)Session["UserID"];
                //var keymagrlst = db.UserInKeyManagerMasters.Where(x => x.UserID == (int)Session["UserID"]).Select(x => x.KeyManagerID).ToList();

                //var offerlst = db.OfferMasters.Where(x => x.Key_Managr.Contains()).ToListAsync();

                var data = (from om in db.OfferMasters
                            join cm in db.CustomerMasters.Where(x => x.KEYMANAGER != "") on om.Name_1 equals cm.CUSTOMERNAME
                            join km in db.KeyManagerMasters on cm.KEYMANAGER equals km.KeyManager
                            join um in db.UserInKeyManagerMasters on km.ID equals um.KeyManagerID
                            where um.UserID == uid && cm.CUSTOMER.EndsWith(om.Sold_to_pt)
                            select new
                            {
                                cm.KEYMANAGER,
                                om.Sold_to_pt,
                                om.Name_1,
                                om.Document,
                                om.Item,
                                om.Doc_Date,
                                om.Purchase_order_number,
                                om.Material,
                                om.Material_Description,
                                om.ConfirmQty,
                                om.Net_price,
                                om.Net_Value,
                                om.Prb,
                                om.Curr,
                                om.Exch_Rate,
                                um.UserID,
                                um.KeyManagerMaster.KeyManager_Name,
                                //om.Offer_Value_INR,
                                //om.Order_Expected,
                            }).ToList().Distinct();

                if (!string.IsNullOrEmpty(model.OfferNo))
                {
                    if (model.Offerddl == 0)
                    {
                        data = data.Where(x => x.Document.ToLower() == model.OfferNo.ToLower()).ToList();
                    }
                    if (model.Offerddl == 1)
                    {
                        data = data.Where(x => x.Document.ToLower() != model.OfferNo.ToLower()).ToList();
                    }
                    if (model.Offerddl == 2)
                    {
                        string[] ids = model.OfferNo.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Document.ToLower().ToString())).ToList();
                    }
                    if (model.Offerddl == 3)
                    {
                        string[] ids = model.OfferNo.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Document.ToLower().ToString())).ToList();
                    }
                    if (model.Offerddl == 4)
                    {
                        data = data.Where(x => x.Document.ToLower().Contains(model.OfferNo.ToLower())).ToList();
                    }
                    if (model.Offerddl == 5)
                    {
                        data = data.Where(x => !x.Document.ToLower().Contains(model.OfferNo.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.CustName))
                {
                    if (model.CustNameddl == 0)
                    {
                        data = data.Where(x => x.Name_1.ToLower() == model.CustName.ToLower()).ToList();
                    }
                    if (model.CustNameddl == 1)
                    {
                        data = data.Where(x => x.Name_1.ToLower() != model.CustName.ToLower()).ToList();
                    }
                    if (model.CustNameddl == 2)
                    {
                        string[] ids = model.CustName.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Name_1.ToLower().ToString())).ToList();
                    }
                    if (model.CustNameddl == 3)
                    {
                        string[] ids = model.CustName.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Name_1.ToLower().ToString())).ToList();
                    }
                    if (model.CustNameddl == 4)
                    {
                        data = data.Where(x => x.Name_1.ToLower().Contains(model.CustName.ToLower())).ToList();
                    }
                    if (model.CustNameddl == 5)
                    {
                        data = data.Where(x => !x.Name_1.ToLower().Contains(model.CustName.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.CustCode))
                {
                    if (model.CustCodeddl == 0)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() == model.CustCode.ToLower()).ToList();
                    }
                    if (model.CustCodeddl == 1)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower() != model.CustCode.ToLower()).ToList();
                    }
                    if (model.CustCodeddl == 2)
                    {
                        string[] ids = model.CustCode.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.CustCodeddl == 3)
                    {
                        string[] ids = model.CustCode.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Sold_to_pt.ToLower().ToString())).ToList();
                    }
                    if (model.CustCodeddl == 4)
                    {
                        data = data.Where(x => x.Sold_to_pt.ToLower().Contains(model.CustCode.ToLower())).ToList();
                    }
                    if (model.CustCodeddl == 5)
                    {
                        data = data.Where(x => !x.Sold_to_pt.ToLower().Contains(model.CustCode.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.MaterialCode))
                {
                    if (model.MaterialCodeddl == 0)
                    {
                        data = data.Where(x => x.Material.ToLower() == model.MaterialCode.ToLower()).ToList();
                    }
                    if (model.MaterialCodeddl == 1)
                    {
                        data = data.Where(x => x.Material.ToLower() != model.MaterialCode.ToLower()).ToList();
                    }
                    if (model.MaterialCodeddl == 2)
                    {
                        string[] ids = model.MaterialCode.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Material.ToLower().ToString())).ToList();
                    }
                    if (model.MaterialCodeddl == 3)
                    {
                        string[] ids = model.MaterialCode.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Material.ToLower().ToString())).ToList();
                    }
                    if (model.MaterialCodeddl == 4)
                    {
                        data = data.Where(x => x.Material.ToLower().Contains(model.MaterialCode.ToLower())).ToList();
                    }
                    if (model.MaterialCodeddl == 5)
                    {
                        data = data.Where(x => !x.Material.ToLower().Contains(model.MaterialCode.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.MaterialDesc))
                {
                    if (model.MaterialDescddl == 0)
                    {
                        data = data.Where(x => x.Material_Description.ToLower() == model.MaterialDesc.ToLower()).ToList();
                    }
                    if (model.MaterialDescddl == 1)
                    {
                        data = data.Where(x => x.Material_Description.ToLower() != model.MaterialDesc.ToLower()).ToList();
                    }
                    if (model.MaterialDescddl == 2)
                    {
                        string[] ids = model.MaterialDesc.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Material_Description.ToLower().ToString())).ToList();
                    }
                    if (model.MaterialDescddl == 3)
                    {
                        string[] ids = model.MaterialDesc.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Material_Description.ToLower().ToString())).ToList();
                    }
                    if (model.MaterialDescddl == 4)
                    {
                        data = data.Where(x => x.Material_Description.ToLower().Contains(model.MaterialDesc.ToLower())).ToList();
                    }
                    if (model.MaterialDescddl == 5)
                    {
                        data = data.Where(x => !x.Material_Description.ToLower().Contains(model.MaterialDesc.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.KeyManager))
                {
                    if (model.KeyManagerddl == 0)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() == model.KeyManager.ToLower()).ToList();
                    }
                    if (model.KeyManagerddl == 1)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower() != model.KeyManager.ToLower()).ToList();
                    }
                    if (model.KeyManagerddl == 2)
                    {
                        string[] ids = model.KeyManager.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.KeyManagerddl == 3)
                    {
                        string[] ids = model.KeyManager.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.KeyManager_Name.ToLower().ToString())).ToList();
                    }
                    if (model.KeyManagerddl == 4)
                    {
                        data = data.Where(x => x.KeyManager_Name.ToLower().Contains(model.KeyManager.ToLower())).ToList();
                    }
                    if (model.KeyManagerddl == 5)
                    {
                        data = data.Where(x => !x.KeyManager_Name.ToLower().Contains(model.KeyManager.ToLower())).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(model.InquiryRef))
                {
                    if (model.InquiryRefddl == 0)
                    {
                        data = data.Where(x => x.Purchase_order_number.ToLower() == model.InquiryRef.ToLower()).ToList();
                    }
                    if (model.InquiryRefddl == 1)
                    {
                        data = data.Where(x => x.Purchase_order_number.ToLower() != model.InquiryRef.ToLower()).ToList();
                    }
                    if (model.InquiryRefddl == 2)
                    {
                        string[] ids = model.InquiryRef.ToLower().Split(',');
                        data = data.Where(x => ids.Contains(x.Purchase_order_number.ToLower().ToString())).ToList();
                    }
                    if (model.InquiryRefddl == 3)
                    {
                        string[] ids = model.InquiryRef.ToLower().Split(',');
                        data = data.Where(x => !ids.Contains(x.Purchase_order_number.ToLower().ToString())).ToList();
                    }
                    if (model.InquiryRefddl == 4)
                    {
                        data = data.Where(x => x.Purchase_order_number.ToLower().Contains(model.InquiryRef.ToLower())).ToList();
                    }
                    if (model.InquiryRefddl == 5)
                    {
                        data = data.Where(x => !x.Purchase_order_number.ToLower().Contains(model.InquiryRef.ToLower())).ToList();
                    }
                }

                foreach (var item in data)
                {
                    var Offer_Value_INR = Convert.ToDecimal(1);
                    var Order_Expected = Convert.ToDecimal(1);
                    try
                    {
                        string prb = item.Prb;
                        string exrate = item.Exch_Rate;
                        if (string.IsNullOrEmpty(prb))
                            prb = "1";
                        if (string.IsNullOrEmpty(exrate))
                            exrate = "1";

                        Offer_Value_INR = Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(exrate);
                        Order_Expected = (Convert.ToDecimal(item.Net_Value.ToString()) * Convert.ToDecimal(exrate) * Convert.ToDecimal(prb)) / 100;

                    }
                    catch (Exception ex)
                    {

                        logger.Error("Offer_Value_INR or Order_Expected not valid error is : " + ex.Message);
                    }
                    OfferViewModel offer = new OfferViewModel();
                    offer.Key_Managr = item.KeyManager_Name;
                    offer.Sold_to_pt = item.Sold_to_pt;
                    offer.Name_1 = item.Name_1;
                    offer.Document = item.Document;
                    offer.Item = item.Item;
                    offer.Doc_Date = item.Doc_Date;
                    offer.strDoc_Date = item.Doc_Date.HasValue ? item.Doc_Date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : string.Empty;
                    offer.Purchase_order_number = item.Purchase_order_number;
                    offer.Material = item.Material;
                    offer.Material_Description = item.Material_Description;
                    offer.ConfirmQty = item.ConfirmQty;
                    offer.Net_price = item.Net_price;
                    offer.Net_Value = item.Net_Value;
                    offer.Curr = item.Curr;
                    offer.Exch_Rate = item.Exch_Rate;
                    offer.Offer_Value_INR = Offer_Value_INR.ToString();
                    offer.Order_Expected = Order_Expected.ToString();

                    offerlst.Add(offer);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return PartialView("~/Views/Offer/_PartialTableData.cshtml", offerlst);
        }
    }
}