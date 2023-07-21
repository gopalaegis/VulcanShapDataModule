using ClosedXML.Excel;
using DAL;
using DocumentManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Valcan.CommandClass;
using Valcan.Models;

namespace Valcan.Controllers
{
    public class HomeController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HomeController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        public string Key = ConfigurationManager.AppSettings["passwordkey"].ToString();
        //public string IsLive = ConfigurationManager.AppSettings["IsLive"].ToString();
        public int ChangePasswordInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ChangePasswordInterval"].ToString().Trim());

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        public ActionResult Login(string returnUrl)
        {
            
            ViewBag.ReturnUrl = returnUrl;
            Session.Clear();
            Session.Abandon();
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                logger.Error("Current time is: " + DateTime.Now);
                if (ModelState.IsValid)
                {
                    CommonMethod cm = new CommonMethod();
                    model.Password = cm.EncryptData(model.Password, Key);
                    var a = cm.DecryptData("qyIxscsHmmSioOjZLY2BCQ==", Key);

                    var IsValidUser = db.UserMasters
                  .Where(u => u.EmailID.ToLower() == model.EmailID.ToLower() && u.Password == model.Password && u.IsActive == true).SingleOrDefault();
                    if (IsValidUser != null)
                    {
                        var userID = IsValidUser.ID;
                        var userMail = IsValidUser.EmailID;
                        var password_period = (from d in db.UserPasswordHistories where d.UserID == userID orderby d.CreatedOn descending select new { lastChangeOn = d.CreatedOn }).Take(1);

                        //var admin_assigned_Period = IsValidUser.ChangePasswordInterval;
                        var admin_assigned_Period = ChangePasswordInterval;

                        //var dmin_assigned_Period_todate = DateTime.ParseExact(admin_assigned_Period, "dd", System.Globalization.CultureInfo.InvariantCulture);
                        TimeSpan difference = new TimeSpan();
                        foreach (var i in password_period)
                        {
                            difference = (TimeSpan)(DateTime.Now - i.lastChangeOn);
                        }
                        if (difference.Days > admin_assigned_Period)
                        {
                            ViewData["error"] = "Password has been reset";
                            Session["UserMail"] = IsValidUser.EmailID;
                            return RedirectToAction("ResetPassword");
                        }
                        if (!User.IsInRole("SuperAdmin"))
                        {
                            if (IsValidUser.IsFirstLogin == true)
                            {
                                Session["UserMail"] = IsValidUser.EmailID;
                                return RedirectToAction("ResetPassword");

                            }

                        }
                        FormsAuthentication.SetAuthCookie(model.EmailID, false);
                        Session["UserID"] = IsValidUser.ID;
                        Session["UserMail"] = IsValidUser.EmailID;
                        Session["UserName"] = IsValidUser.FirstName;
                        if (!String.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToLocal(returnUrl);
                        }
                        return RedirectToAction("Index", "Offer");
                    }
                    logger.Error("Invalid Email ID or Password");
                    ViewData["LoginFlag"] = "Invalid Email ID or Password";
                    ViewData["error"] = " Invalid ID or Password";
                    return View();

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }



            return View();
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            //HttpContext.Current.Session.Clear();
            //HttpContext.Current.Session.Abandon();
            //HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
            return RedirectToAction("Login", "Home");
        }
        [Authorize]
        public ActionResult ShowProfile()
        {
            //var user_id = Convert.ToInt32(Session["UserID"]);
            //var userdetails = db.UserMasters.Where(x => x.ID == user_id).FirstOrDefault();
            //UserMaster userdata = new UserMaster
            //{
            //    FirstName = userdetails.FirstName,
            //    EmailID = userdetails.EmailID,
            //    LastName = userdetails.LastName

            //};
            //return View(userdata);  
            var user_id = Convert.ToInt32(Session["UserID"]);
            if (user_id == 0)
            {
                return RedirectToAction("Logout", "Home");

            }
            var data = db.UserMasters.Where(x => x.ID == user_id).FirstOrDefault();
            return View(data);
        }
        [Authorize]
        public ActionResult EditProfile()
        {
            var user_id = Convert.ToInt32(Session["UserID"]);
            if (user_id == 0)
            {
                return RedirectToAction("Logout", "Home");

            }
            var userdetails = db.UserMasters.Where(x => x.ID == user_id).FirstOrDefault();
            UserMasterViewModel userdata = new UserMasterViewModel
            {
                ID = userdetails.ID,
                FirstName = userdetails.FirstName,
                EmailID = userdetails.EmailID,
                LastName = userdetails.LastName

            };
            return View(userdata);

        }
        [Authorize]
        [HttpPost]
        public ActionResult EditProfile(UserMasterViewModel userMasterViewModel)
        {
            var user_id = Convert.ToInt32(Session["UserID"]);
            if (user_id == 0)
            {
                return RedirectToAction("Logout", "Home");

            }
            if (ModelState.IsValid)
            {
                var isexist = db.UserMasters.Where(x => x.ID == userMasterViewModel.ID).FirstOrDefault();
                if (isexist != null)
                {
                    isexist.ID = isexist.ID;
                    isexist.FirstName = userMasterViewModel.FirstName;
                    isexist.LastName = "+" + userMasterViewModel.countryflag + userMasterViewModel.LastName;
                    isexist.EmailID = userMasterViewModel.EmailID;
                    isexist.LastModifiedBy = userMasterViewModel.ID;
                    isexist.LastModifiedOn = DateTime.Now;
                    db.Entry(isexist).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewData["success"] = "Profile Updated Successfully";
                    return RedirectToAction("ShowProfile");
                }
                ViewData["error"] = "User ID Not Found";
                return View(userMasterViewModel);
            }

            ViewData["error"] = "Please Fill The Required Field";
            return View(userMasterViewModel);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userID = Convert.ToInt32(Session["UserID"]);
                var userMail = Convert.ToString(Session["UserMail"]);
                CommonMethod cm = new CommonMethod();
                model.password = cm.EncryptData(model.password, Key);
                var IsValidUser = db.UserMasters
                    .Where(u => u.Password == model.password && u.EmailID == userMail && u.IsActive == true).SingleOrDefault();
                if (IsValidUser != null)
                {

                    model.NewPassword = cm.EncryptData(model.NewPassword, Key);

                    var lastpassword = (from d in db.UserPasswordHistories where d.UserID == userID orderby d.CreatedOn descending select new { Password = d.Password }).Take(2);
                    foreach (var i in lastpassword)
                    {
                        if (i.Password == model.NewPassword)
                        {
                            ViewData["error"] = "Password must be different from last two passwords";
                            return View(model);

                        }
                    }
                    IsValidUser.Password = model.NewPassword;
                    IsValidUser.LastModifiedBy = userID;
                    IsValidUser.LastModifiedOn = DateTime.Now;
                    UserPasswordHistory userPasswordHistory = new UserPasswordHistory()
                    {
                        UserID = userID,
                        CreatedBy = userID,
                        Password = model.NewPassword,
                        CreatedOn = DateTime.Now,
                    };
                    db.UserPasswordHistories.Add(userPasswordHistory);
                    db.SaveChanges();
                    return RedirectToAction("Login");

                }
                ViewData["error"] = "Invalid current password";
                return View(model);
            }
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var userID = Convert.ToInt32(Session["UserID"]);
                var userMail = Convert.ToString(Session["UserMail"]);
                CommonMethod cm = new CommonMethod();
                model.password = cm.EncryptData(model.password, Key);
                var IsValidUser = db.UserMasters
                    .Where(u => u.Password == model.password && u.EmailID == userMail && u.IsActive == true).SingleOrDefault();
                if (IsValidUser != null)
                {

                    model.NewPassword = cm.EncryptData(model.NewPassword, Key);

                    var lastpassword = (from d in db.UserPasswordHistories where d.UserID == IsValidUser.ID orderby d.CreatedOn descending select new { Password = d.Password }).Take(2);
                    foreach (var i in lastpassword)
                    {
                        if (i.Password == model.NewPassword)
                        {
                            ViewData["error"] = "Password must be different from last two passwords";
                            return View();

                        }
                    }
                    IsValidUser.IsFirstLogin = false;
                    IsValidUser.Password = model.NewPassword;
                    IsValidUser.LastModifiedBy = IsValidUser.ID;
                    IsValidUser.LastModifiedOn = DateTime.Now;
                    UserPasswordHistory userPasswordHistory = new UserPasswordHistory()
                    {
                        UserID = IsValidUser.ID,
                        CreatedBy = IsValidUser.ID,
                        Password = model.NewPassword,
                        CreatedOn = DateTime.Now,
                    };
                    db.UserPasswordHistories.Add(userPasswordHistory);
                    db.SaveChanges();
                    FormsAuthentication.SignOut();
                    Session.Clear();
                    Session.Abandon();
                    return RedirectToAction("Index");

                }
                Session.Clear();
                Session.Abandon();
                ViewData["error"] = " Invalid old password";
                return View();
            }
            return View();
        }
        public async Task<JsonResult> UserIDAlreadyExistsAsync(string EmailID)
        {
            var userID = Convert.ToInt32(Session["UserID"]);
            var result = await db.UserMasters.AnyAsync(x => x.EmailID == EmailID && x.IsActive == true && x.ID != userID);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
        // GET: ClonePanel
        //public ActionResult ExcelToDatabase()
        //{
        //    return View();
        //}

        //[HttpPost]
        public ActionResult ExcelToDatabase()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            bool result = false;
            ViewBag.data = null;

            //if (Request.Files["FileUpload1"].ContentLength > 0)
            //{
            //string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();

            string query = null;
            string connString = "";
            string filename = "ZCUSTOMER.xlsx";
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            string[] validFileTypes = { ".xls", ".xlsx" };

            //string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), Request.Files["FileUpload1"].FileName);
            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Document"), filename);
            //if (!Directory.Exists(path1))
            //{
            //    Directory.CreateDirectory(Server.MapPath("~/Document"));
            //}
            if (validFileTypes.Contains(extension))
            {
                //if (System.IO.File.Exists(path1))
                //{ System.IO.File.Delete(path1); }
                //Request.Files["FileUpload1"].SaveAs(path1);

                //Connection String to Excel Workbook
                if (extension.Trim() == ".xls")
                {
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    result = ImportExceltoDatabase(path1);
                }
                else if (extension.Trim() == ".xlsx")
                {
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    result = ImportExceltoDatabase(path1);
                }
            }
            else
            {
                ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
            }
            // }
            if (result)
            {
                ViewBag.data = "Customer Data Import Successfully from excel to database.";
            }
            else
            {
                logger.Error(result);
                ViewBag.data = "there is some issue while importing the Data.";
            }
            return View("Index");
        }

        #region old code
        //public bool ImportExceltoDatabase(string strFilePath, string connString)
        //{
        //    bool result = false;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (XLWorkbook workBook = new XLWorkbook(strFilePath))
        //        {
        //            //Read the first Sheet from Excel file.
        //            IXLWorksheet workSheet = workBook.Worksheet(1);



        //            //Loop through the Worksheet rows.
        //            bool firstRow = true;
        //            foreach (IXLRow row in workSheet.Rows())
        //            {
        //                //Use the first row to add columns to DataTable.
        //                if (firstRow)
        //                {
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {
        //                        if (count < 26)
        //                        {
        //                            count = count + 1;
        //                            dt.Columns.Add(cell.Value.ToString());
        //                        }

        //                    }
        //                    firstRow = false;
        //                }
        //                else
        //                {
        //                    //Add rows to DataTable.
        //                    dt.Rows.Add();
        //                    int i = 0;
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {

        //                        if (count < 26)
        //                        {
        //                            count = count + 1;
        //                            if (string.IsNullOrEmpty(cell.Value.ToString()))
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
        //                            }
        //                            else
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                            }
        //                            i++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (dt.Rows.Count > 0)
        //        {
        //            SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //            string s = "Truncate Table customermaster";
        //            con.Open();
        //            SqlCommand Com = new SqlCommand(s, con);
        //            Com.ExecuteNonQuery();
        //            con.Close();
        //            CustomerMaster tblObj = new CustomerMaster();
        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (!string.IsNullOrEmpty(row[1].ToString()))
        //                {
        //                    tblObj.COMPANYCODE = row[0].ToString();
        //                    tblObj.CUSTOMER = row[1].ToString();
        //                    tblObj.CUSTOMERNAME = row[2].ToString();
        //                    tblObj.CUSTOMERGRP = row[3].ToString();
        //                    tblObj.SEARCHTERM = row[4].ToString();
        //                    tblObj.STREET = row[5].ToString();
        //                    tblObj.CITY = row[6].ToString();
        //                    tblObj.POSTALCODE = row[7].ToString();
        //                    tblObj.REGION = row[8].ToString();
        //                    tblObj.EMAILADDR = row[9].ToString();
        //                    tblObj.PHONENO = row[10].ToString();
        //                    tblObj.FAXNO = row[11].ToString();
        //                    tblObj.PAYMENTTERMS = row[12].ToString();
        //                    tblObj.TEXT = row[13].ToString();
        //                    tblObj.CreatedBy = row[14].ToString();
        //                    if (!string.IsNullOrEmpty(row[15].ToString()))
        //                    {
        //                        if (row[15].ToString().Contains("."))
        //                        {
        //                            row[15] = row[15].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.CreatedON = DateTime.ParseExact(row["Created_ON"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.CreatedON = Convert.ToDateTime(row[15].ToString());
        //                    }
        //                    //tblObj.CreatedON = (DateTime)row["Created_ON"];
        //                    tblObj.ECCNO = row[16].ToString();
        //                    tblObj.CSTNO = row[17].ToString();
        //                    tblObj.LSTNO = row[18].ToString();
        //                    tblObj.PANNO = row[19].ToString();
        //                    tblObj.SERVICETAXREGNNO = row[20].ToString();
        //                    tblObj.MARKETINGGROUP = row[21].ToString();
        //                    tblObj.KEYMANAGER = row[22].ToString();
        //                    tblObj.AGENT = row[23].ToString();
        //                    tblObj.TARGET = row[24].ToString();
        //                    tblObj.EMPLOYEE = row[25].ToString();
        //                    //tblObj.Name = row["Address"].ToString();
        //                    //tblObj.Salary = (int)row["Salary"];
        //                    //tblObj.Age = (int)row["Age"];

        //                    db.CustomerMasters.Add(tblObj);
        //                    db.SaveChanges();
        //                }
        //            }

        //            //foreach (DataRow row in dt.Rows)
        //            //{
        //            //    if (!string.IsNullOrEmpty(row["CUSTOMER"].ToString()))
        //            //    {
        //            //        tblObj.COMPANYCODE = row["COMPANY_CODE"].ToString();
        //            //        tblObj.CUSTOMER = row["CUSTOMER"].ToString();
        //            //        tblObj.CUSTOMERNAME = row["CUSTOMER_NAME"].ToString();
        //            //        tblObj.CUSTOMERGRP = row["CUSTOMER_GRP"].ToString();
        //            //        tblObj.SEARCHTERM = row["SEARCH_TERM"].ToString();
        //            //        tblObj.STREET = row["STREET"].ToString();
        //            //        tblObj.CITY = row["CITY"].ToString();
        //            //        tblObj.POSTALCODE = row["POSTALCODE"].ToString();
        //            //        tblObj.REGION = row["REGION"].ToString();
        //            //        tblObj.EMAILADDR = row["EMAIL_ADDR"].ToString();
        //            //        tblObj.PHONENO = row["PHONE_NO"].ToString();
        //            //        tblObj.FAXNO = row["FAX_NO"].ToString();
        //            //        tblObj.PAYMENTTERMS = row["PAYMENT_TERMS"].ToString();
        //            //        tblObj.TEXT = row["TEXT"].ToString();
        //            //        tblObj.CreatedBy = row["Created_By"].ToString();
        //            //        if (!string.IsNullOrEmpty(row["Created_ON"].ToString()))
        //            //        {
        //            //            if (row["Created_ON"].ToString().Contains("."))
        //            //            {
        //            //                row["Created_ON"] = row["Created_ON"].ToString().Replace(".", "-");
        //            //            }
        //            //            //tblObj.CreatedON = DateTime.ParseExact(row["Created_ON"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //            //            tblObj.CreatedON = Convert.ToDateTime(row["Created_ON"].ToString());
        //            //        }
        //            //        //tblObj.CreatedON = (DateTime)row["Created_ON"];
        //            //        tblObj.ECCNO = row["ECC_NO"].ToString();
        //            //        tblObj.CSTNO = row["CST_NO"].ToString();
        //            //        tblObj.LSTNO = row["LST_NO"].ToString();
        //            //        tblObj.PANNO = row["PAN_NO"].ToString();
        //            //        tblObj.SERVICETAXREGNNO = row["SERVICE_TAX_REGN_NO"].ToString();
        //            //        tblObj.MARKETINGGROUP = row["MARKETING_GROUP"].ToString();
        //            //        tblObj.KEYMANAGER = row["KEY_MANAGER"].ToString();
        //            //        tblObj.AGENT = row["AGENT"].ToString();
        //            //        tblObj.TARGET = row["TARGET"].ToString();
        //            //        tblObj.EMPLOYEE = row["EMPLOYEE"].ToString();
        //            //        //tblObj.Name = row["Address"].ToString();
        //            //        //tblObj.Salary = (int)row["Salary"];
        //            //        //tblObj.Age = (int)row["Age"];

        //            //        db.CustomerMasters.Add(tblObj);
        //            //        db.SaveChanges();
        //            //    }
        //            //}

        //        }

        //        //oledbConn.Open();
        //        //using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
        //        //{
        //        //    OleDbDataAdapter oleda = new OleDbDataAdapter();
        //        //    oleda.SelectCommand = cmd;
        //        //    DataSet ds = new DataSet();
        //        //    oleda.Fill(ds);

        //        //    dt = ds.Tables[0];

        //        //    if (dt.Rows.Count > 0)
        //        //    {
        //        //        SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //        //        string s = "Truncate Table customermaster";
        //        //        con.Open();
        //        //        SqlCommand Com = new SqlCommand(s, con);
        //        //        Com.ExecuteNonQuery();
        //        //        con.Close();
        //        //        CustomerMaster tblObj = new CustomerMaster();
        //        //        foreach (DataRow row in dt.Rows)
        //        //        {
        //        //            if (!string.IsNullOrEmpty(row["CUSTOMER"].ToString()))
        //        //            {
        //        //                tblObj.COMPANYCODE = row["COMPANY_CODE"].ToString();
        //        //                tblObj.CUSTOMER = row["CUSTOMER"].ToString();
        //        //                tblObj.CUSTOMERNAME = row["CUSTOMER_NAME"].ToString();
        //        //                tblObj.CUSTOMERGRP = row["CUSTOMER_GRP"].ToString();
        //        //                tblObj.SEARCHTERM = row["SEARCH_TERM"].ToString();
        //        //                tblObj.STREET = row["STREET"].ToString();
        //        //                tblObj.CITY = row["CITY"].ToString();
        //        //                tblObj.POSTALCODE = row["POSTALCODE"].ToString();
        //        //                tblObj.REGION = row["REGION"].ToString();
        //        //                tblObj.EMAILADDR = row["EMAIL_ADDR"].ToString();
        //        //                tblObj.PHONENO = row["PHONE_NO"].ToString();
        //        //                tblObj.FAXNO = row["FAX_NO"].ToString();
        //        //                tblObj.PAYMENTTERMS = row["PAYMENT_TERMS"].ToString();
        //        //                tblObj.TEXT = row["TEXT"].ToString();
        //        //                tblObj.CreatedBy = row["Created_By"].ToString();
        //        //                tblObj.CreatedON = (DateTime)row["Created_ON"];
        //        //                tblObj.ECCNO = row["ECC_NO"].ToString();
        //        //                tblObj.CSTNO = row["CST_NO"].ToString();
        //        //                tblObj.LSTNO = row["LST_NO"].ToString();
        //        //                tblObj.PANNO = row["PAN_NO"].ToString();
        //        //                tblObj.SERVICETAXREGNNO = row["SERVICE_TAX_REGN_NO"].ToString();
        //        //                tblObj.MARKETINGGROUP = row["MARKETING_GROUP"].ToString();
        //        //                tblObj.KEYMANAGER = row["KEY_MANAGER"].ToString();
        //        //                tblObj.AGENT = row["AGENT"].ToString();
        //        //                tblObj.TARGET = row["TARGET"].ToString();
        //        //                tblObj.EMPLOYEE = row["EMPLOYEE"].ToString();
        //        //                //tblObj.Name = row["Address"].ToString();
        //        //                //tblObj.Salary = (int)row["Salary"];
        //        //                //tblObj.Age = (int)row["Age"];

        //        //                db.CustomerMasters.Add(tblObj);
        //        //                db.SaveChanges();
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //    }
        //    finally
        //    {
        //        //oledbConn.Close();
        //    }
        //    return result;
        //}
        #endregion
        public bool ImportExceltoDatabase(string strFilePath)
        {
            bool result = false;
            DataTable dt = new DataTable();
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(strFilePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);



                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (count < 26)
                                {
                                    count = count + 1;
                                    dt.Columns.Add(cell.Value.ToString());
                                }

                            }
                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {

                                if (count < 26)
                                {
                                    count = count + 1;
                                    if (string.IsNullOrEmpty(cell.Value.ToString()))
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
                                    }
                                    else
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                    string s = "Truncate Table customermaster";
                    con.Open();
                    SqlCommand Com = new SqlCommand(s, con);
                    Com.ExecuteNonQuery();
                    con.Close();
                    CustomerMaster tblObj = new CustomerMaster();
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row[1].ToString()))
                        {
                            tblObj.COMPANYCODE = row[0].ToString();
                            tblObj.CUSTOMER = row[1].ToString();
                            tblObj.CUSTOMERNAME = row[2].ToString();
                            tblObj.CUSTOMERGRP = row[3].ToString();
                            tblObj.SEARCHTERM = row[4].ToString();
                            tblObj.STREET = row[5].ToString();
                            tblObj.CITY = row[6].ToString();
                            tblObj.POSTALCODE = row[7].ToString();
                            tblObj.REGION = row[8].ToString();
                            tblObj.EMAILADDR = row[9].ToString();
                            tblObj.PHONENO = row[10].ToString();
                            tblObj.FAXNO = row[11].ToString();
                            tblObj.PAYMENTTERMS = row[12].ToString();
                            tblObj.TEXT = row[13].ToString();
                            tblObj.CreatedBy = row[14].ToString();
                            if (!string.IsNullOrEmpty(row[15].ToString()))
                            {
                                if (row[15].ToString().Contains("."))
                                {
                                    row[15] = row[15].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.CreatedON = DateTime.ParseExact(row[15].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[15].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.CreatedON = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.CreatedON = Convert.ToDateTime(row[15].ToString());
                            }
                            //tblObj.CreatedON = (DateTime)row["Created_ON"];
                            tblObj.ECCNO = row[16].ToString();
                            tblObj.CSTNO = row[17].ToString();
                            tblObj.LSTNO = row[18].ToString();
                            tblObj.PANNO = row[19].ToString();
                            tblObj.SERVICETAXREGNNO = row[20].ToString();
                            tblObj.MARKETINGGROUP = row[21].ToString();
                            tblObj.KEYMANAGER = row[22].ToString();
                            tblObj.AGENT = row[23].ToString();
                            tblObj.TARGET = row[24].ToString();
                            tblObj.EMPLOYEE = row[25].ToString();
                            //tblObj.Name = row["Address"].ToString();
                            //tblObj.Salary = (int)row["Salary"];
                            //tblObj.Age = (int)row["Age"];

                            db.CustomerMasters.Add(tblObj);
                            db.SaveChanges();
                        }
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = false;
                throw;
            }
            finally
            {
                //oledbConn.Close();
            }
            return result;
        }
        public ActionResult OfferExcelToDatabase()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            bool result = false;
            ViewBag.data = null;

            //if (Request.Files["FileUpload1"].ContentLength > 0)
            //{
            //string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();

            string query = null;
            string connString = "";
            string filename = "Offers.xlsx";
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            string[] validFileTypes = { ".xls", ".xlsx" };

            //string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), Request.Files["FileUpload1"].FileName);
            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Document"), filename);
            //if (!Directory.Exists(path1))
            //{
            //    Directory.CreateDirectory(Server.MapPath("~/Document"));
            //}
            if (validFileTypes.Contains(extension))
            {
                //if (System.IO.File.Exists(path1))
                //{ System.IO.File.Delete(path1); }
                //Request.Files["FileUpload1"].SaveAs(path1);

                //Connection String to Excel Workbook
                if (extension.Trim() == ".xls")
                {
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    result = ImportOfferExceltoDatabase(path1);
                }
                else if (extension.Trim() == ".xlsx")
                {
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    result = ImportOfferExceltoDatabase(path1);
                }
            }
            else
            {
                ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
            }
            // }
            if (result)
            {
                ViewBag.data = "Offers Import Successfully from excel to database.";
            }
            else
            {
                logger.Error(result);
                ViewBag.data = "there is some issue while importing the Data.";
            }
            return View("Index");
        }

        #region old code
        //public bool ImportOfferExceltoDatabase(string strFilePath, string connString)
        //{
        //    bool result = false;
        //    //OleDbConnection oledbConn = new OleDbConnection(connString);
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (XLWorkbook workBook = new XLWorkbook(strFilePath))
        //        {
        //            //Read the first Sheet from Excel file.
        //            IXLWorksheet workSheet = workBook.Worksheet(1);



        //            //Loop through the Worksheet rows.
        //            bool firstRow = true;
        //            foreach (IXLRow row in workSheet.Rows())
        //            {
        //                //Use the first row to add columns to DataTable.
        //                if (firstRow)
        //                {
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {
        //                        if (count < 30)
        //                        {
        //                            count = count + 1;
        //                            dt.Columns.Add(cell.Value.ToString());
        //                        }

        //                    }
        //                    firstRow = false;
        //                }
        //                else
        //                {
        //                    //Add rows to DataTable.
        //                    dt.Rows.Add();
        //                    int i = 0;
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {

        //                        if (count < 30)
        //                        {
        //                            count = count + 1;
        //                            if (string.IsNullOrEmpty(cell.Value.ToString()))
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
        //                            }
        //                            else
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                            }
        //                            i++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (dt.Rows.Count > 0)
        //        {
        //            SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //            string s = "Truncate Table offermaster";
        //            con.Open();
        //            SqlCommand Com = new SqlCommand(s, con);
        //            Com.ExecuteNonQuery();
        //            con.Close();
        //            OfferMaster tblObj = new OfferMaster();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (!string.IsNullOrEmpty(row[0].ToString()))
        //                {
        //                    tblObj.Dv = row[0].ToString();
        //                    tblObj.SaTy = row[1].ToString();
        //                    tblObj.Sold_to_pt = row[2].ToString();
        //                    tblObj.Name_1 = row[3].ToString();
        //                    tblObj.Purchase_order_number = row[4].ToString();
        //                    tblObj.Document = row[5].ToString();
        //                    tblObj.Item = row[6].ToString();
        //                    if (!string.IsNullOrEmpty(row[7].ToString()))
        //                    {
        //                        if (row[7].ToString().Contains("."))
        //                        {
        //                            row[7] = row[7].ToString().Replace(".", "-");
        //                        }
        //                        tblObj.Doc_Date = DateTime.ParseExact(row[7].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                    }
        //                    tblObj.Material = row[8].ToString();
        //                    tblObj.Material_Description = row[9].ToString();
        //                    tblObj.Exch_Rate = row[10].ToString();
        //                    tblObj.Curr = row[11].ToString();
        //                    tblObj.ConfirmQty = row[12].ToString();
        //                    tblObj.Net_value1 = row[13].ToString();
        //                    tblObj.Net_price = row[14].ToString();
        //                    tblObj.Net_Value = row[15].ToString();
        //                    tblObj.Prb = row[16].ToString();
        //                    if (!string.IsNullOrEmpty(row[17].ToString()))
        //                    {
        //                        if (row[17].ToString().Contains("."))
        //                        {
        //                            row[17] = row[17].ToString().Replace(".", "-");
        //                        }
        //                        tblObj.Dlv_Date = DateTime.ParseExact(row[17].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                    }
        //                    tblObj.Prod_hier = row[18].ToString();
        //                    tblObj.Prod_Description = row[19].ToString();
        //                    tblObj.CGrp = row[20].ToString();
        //                    tblObj.Key_Managr = row[21].ToString();
        //                    tblObj.SDst = row[22].ToString();
        //                    tblObj.Customer_Z = row[23].ToString();
        //                    tblObj.Vendor_Name = row[24].ToString();
        //                    tblObj.Matl_Group = row[25].ToString();
        //                    tblObj.Mat_Grp_Descr = row[26].ToString();
        //                    tblObj.Cust_Ind_C = row[27].ToString();
        //                    tblObj.Cust_Ind_D = row[28].ToString();
        //                    tblObj.OrdRs = row[29].ToString();
        //                    //tblObj.Name = row["Address"].ToString();
        //                    //tblObj.Salary = (int)row["Salary"];
        //                    //tblObj.Age = (int)row["Age"];

        //                    db.OfferMasters.Add(tblObj);
        //                    db.SaveChanges();
        //                }
        //            }
        //            //foreach (DataRow row in dt.Rows)
        //            //{
        //            //    if (!string.IsNullOrEmpty(row["Dv"].ToString()))
        //            //    {
        //            //        tblObj.Dv = row["Dv"].ToString();
        //            //        tblObj.SaTy = row["SaTy"].ToString();
        //            //        tblObj.Sold_to_pt = row["Sold_to_pt"].ToString();
        //            //        tblObj.Name_1 = row["Name_1"].ToString();
        //            //        tblObj.Purchase_order_number = row["Purchase_order_number"].ToString();
        //            //        tblObj.Document = row["Document"].ToString();
        //            //        tblObj.Item = row["Item"].ToString();
        //            //        if (!string.IsNullOrEmpty(row["Doc_Date"].ToString()))
        //            //        {
        //            //            if (row["Doc_Date"].ToString().Contains("."))
        //            //            {
        //            //                row["Doc_Date"] = row["Doc_Date"].ToString().Replace(".", "-");
        //            //            }
        //            //            tblObj.Doc_Date = DateTime.ParseExact(row["Doc_Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //            //        }
        //            //        tblObj.Material = row["Material"].ToString();
        //            //        tblObj.Material_Description = row["Material_Description"].ToString();
        //            //        tblObj.Exch_Rate = row["Exch_Rate"].ToString();
        //            //        tblObj.Curr = row["Curr"].ToString();
        //            //        tblObj.ConfirmQty = row["ConfirmQty"].ToString();
        //            //        tblObj.Net_value1 = row["Net_value1"].ToString();
        //            //        tblObj.Net_price = row["Net_price"].ToString();
        //            //        tblObj.Net_Value = row["Net_Value"].ToString();
        //            //        tblObj.Prb = row["Prb"].ToString();
        //            //        if (!string.IsNullOrEmpty(row["Dlv_Date"].ToString()))
        //            //        {
        //            //            if (row["Dlv_Date"].ToString().Contains("."))
        //            //            {
        //            //                row["Dlv_Date"] = row["Dlv_Date"].ToString().Replace(".", "-");
        //            //            }
        //            //            tblObj.Dlv_Date = DateTime.ParseExact(row["Dlv_Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //            //        }
        //            //        tblObj.Prod_hier = row["Prod_hier"].ToString();
        //            //        tblObj.Prod_Description = row["Prod_Description"].ToString();
        //            //        tblObj.CGrp = row["CGrp"].ToString();
        //            //        tblObj.Key_Managr = row["Key_Managr"].ToString();
        //            //        tblObj.SDst = row["SDst"].ToString();
        //            //        tblObj.Customer_Z = row["Customer_Z"].ToString();
        //            //        tblObj.Vendor_Name = row["Vendor_Name"].ToString();
        //            //        tblObj.Matl_Group = row["Matl_Group"].ToString();
        //            //        tblObj.Mat_Grp_Descr = row["Mat_Grp_Descr"].ToString();
        //            //        tblObj.Cust_Ind_C = row["Cust_Ind_C"].ToString();
        //            //        tblObj.Cust_Ind_D = row["Cust_Ind_D"].ToString();
        //            //        tblObj.OrdRs = row["OrdRs"].ToString();
        //            //        //tblObj.Name = row["Address"].ToString();
        //            //        //tblObj.Salary = (int)row["Salary"];
        //            //        //tblObj.Age = (int)row["Age"];

        //            //        db.OfferMasters.Add(tblObj);
        //            //        db.SaveChanges();
        //            //    }
        //            //}

        //        }



        //        //oledbConn.Open();
        //        //using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
        //        //{
        //        //    OleDbDataAdapter oleda = new OleDbDataAdapter();
        //        //    oleda.SelectCommand = cmd;
        //        //    DataSet ds = new DataSet();
        //        //    oleda.Fill(ds);

        //        //    dt = ds.Tables[0];

        //        //    if (dt.Rows.Count > 0)
        //        //    {
        //        //        SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //        //        string s = "Truncate Table offermaster";
        //        //        con.Open();
        //        //        SqlCommand Com = new SqlCommand(s, con);
        //        //        Com.ExecuteNonQuery();
        //        //        con.Close();
        //        //        OfferMaster tblObj = new OfferMaster();
        //        //        foreach (DataRow row in dt.Rows)
        //        //        {
        //        //            if (!string.IsNullOrEmpty(row["Dv"].ToString()))
        //        //            {
        //        //                tblObj.Dv = row["Dv"].ToString();
        //        //                tblObj.SaTy = row["SaTy"].ToString();
        //        //                tblObj.Sold_to_pt = row["Sold_to_pt"].ToString();
        //        //                tblObj.Name_1 = row["Name_1"].ToString();
        //        //                tblObj.Purchase_order_number = row["Purchase_order_number"].ToString();
        //        //                tblObj.Document = row["Document"].ToString();
        //        //                tblObj.Item = row["Item"].ToString();
        //        //                if (!string.IsNullOrEmpty(row["Doc_Date"].ToString()))
        //        //                {
        //        //                    if (row["Doc_Date"].ToString().Contains("."))
        //        //                    {
        //        //                        row["Doc_Date"] = row["Doc_Date"].ToString().Replace(".", "-");
        //        //                    }
        //        //                    tblObj.Doc_Date = Convert.ToDateTime(row["Doc_Date"].ToString());
        //        //                }
        //        //                tblObj.Material = row["Material"].ToString();
        //        //                tblObj.Material_Description = row["Material_Description"].ToString();
        //        //                tblObj.Exch_Rate = row["Exch_Rate"].ToString();
        //        //                tblObj.Curr = row["Curr"].ToString();
        //        //                tblObj.ConfirmQty = row["ConfirmQty"].ToString();
        //        //                tblObj.Net_value1 = row["Net_value1"].ToString();
        //        //                tblObj.Net_price = row["Net_price"].ToString();
        //        //                tblObj.Net_Value = row["Net_Value"].ToString();
        //        //                tblObj.Prb = row["Prb"].ToString();
        //        //                if (!string.IsNullOrEmpty(row["Dlv_Date"].ToString()))
        //        //                {
        //        //                    if (row["Dlv_Date"].ToString().Contains("."))
        //        //                    {
        //        //                        row["Dlv_Date"] = row["Dlv_Date"].ToString().Replace(".", "-");
        //        //                    }
        //        //                    tblObj.Dlv_Date = Convert.ToDateTime(row["Dlv_Date"].ToString());
        //        //                }
        //        //                tblObj.Prod_hier = row["Prod_hier"].ToString();
        //        //                tblObj.Prod_Description = row["Prod_Description"].ToString();
        //        //                tblObj.CGrp = row["CGrp"].ToString();
        //        //                tblObj.Key_Managr = row["Key_Managr"].ToString();
        //        //                tblObj.SDst = row["SDst"].ToString();
        //        //                tblObj.Customer_Z = row["Customer_Z"].ToString();
        //        //                tblObj.Vendor_Name = row["Vendor_Name"].ToString();
        //        //                tblObj.Matl_Group = row["Matl_Group"].ToString();
        //        //                tblObj.Mat_Grp_Descr = row["Mat_Grp_Descr"].ToString();
        //        //                tblObj.Cust_Ind_C = row["Cust_Ind_C"].ToString();
        //        //                tblObj.Cust_Ind_D = row["Cust_Ind_D"].ToString();
        //        //                //tblObj.OrdRs = row["OrdRs"].ToString();
        //        //                //tblObj.Name = row["Address"].ToString();
        //        //                //tblObj.Salary = (int)row["Salary"];
        //        //                //tblObj.Age = (int)row["Age"];

        //        //                db.OfferMasters.Add(tblObj);
        //        //                db.SaveChanges();
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.Message);
        //        result = false;
        //    }
        //    finally
        //    {
        //        //oledbConn.Close();
        //    }
        //    return result;
        //}
        //private DateTime ConvertToDateTime(string st)
        //{
        //    try
        //    {
        //        if (IsLive == "True")
        //        {
        //            var day = string.Empty;
        //            var month = string.Empty;
        //            var dat = st.Split('-');
        //            if (dat[0].Length == 1)
        //                month = "0" + dat[0];
        //            else
        //                month = dat[0];
        //            if (dat[1].Length == 1)
        //                day = "0" + dat[1];
        //            else
        //                day = dat[1];
        //            st = month + "-" + day + "-" + dat[2];
        //            DateTime dt1 = DateTime.ParseExact(st, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //            return dt1;
        //        }
        //        else
        //        {
        //            DateTime dt1 = DateTime.ParseExact(st, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //            return dt1;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        if (!string.IsNullOrEmpty(st))
        //        {
        //            return Convert.ToDateTime(st);
        //        }
        //        else
        //        {
        //            return DateTime.Now;
        //        }
        //    }

        //}

        #endregion
        public bool ImportOfferExceltoDatabase(string strFilePath)
        {
            bool result = false;
            //OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(strFilePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);



                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (count < 30)
                                {
                                    count = count + 1;
                                    dt.Columns.Add(cell.Value.ToString());
                                }

                            }
                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {

                                if (count < 30)
                                {
                                    count = count + 1;
                                    if (string.IsNullOrEmpty(cell.Value.ToString()))
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
                                    }
                                    else
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                    string s = "Truncate Table offermaster";
                    con.Open();
                    SqlCommand Com = new SqlCommand(s, con);
                    Com.ExecuteNonQuery();
                    con.Close();
                    OfferMaster tblObj = new OfferMaster();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row[0].ToString()))
                        {
                            tblObj.Dv = row[0].ToString();
                            tblObj.SaTy = row[1].ToString();
                            tblObj.Sold_to_pt = row[2].ToString();
                            tblObj.Name_1 = row[3].ToString();
                            tblObj.Purchase_order_number = row[4].ToString();
                            tblObj.Document = row[5].ToString();
                            tblObj.Item = row[6].ToString();
                            if (!string.IsNullOrEmpty(row[7].ToString()))
                            {
                                if (row[7].ToString().Contains("."))
                                {
                                    row[7] = row[7].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Doc_Date = DateTime.ParseExact(row[7].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[7].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Doc_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Doc_Date = Convert.ToDateTime(row[7].ToString());
                            }
                            tblObj.Material = row[8].ToString();
                            tblObj.Material_Description = row[9].ToString();
                            tblObj.Exch_Rate = row[10].ToString();
                            tblObj.Curr = row[11].ToString();
                            tblObj.ConfirmQty = row[12].ToString();
                            tblObj.Net_value1 = row[13].ToString();
                            tblObj.Net_price = row[14].ToString();
                            tblObj.Net_Value = row[15].ToString();
                            tblObj.Prb = row[16].ToString();
                            if (!string.IsNullOrEmpty(row[17].ToString()))
                            {
                                if (row[17].ToString().Contains("."))
                                {
                                    row[17] = row[17].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Dlv_Date = DateTime.ParseExact(row[17].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[17].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Dlv_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Dlv_Date = Convert.ToDateTime(row[17].ToString());
                            }
                            tblObj.Prod_hier = row[18].ToString();
                            tblObj.Prod_Description = row[19].ToString();
                            tblObj.CGrp = row[20].ToString();
                            tblObj.Key_Managr = row[21].ToString();
                            tblObj.SDst = row[22].ToString();
                            tblObj.Customer_Z = row[23].ToString();
                            tblObj.Vendor_Name = row[24].ToString();
                            tblObj.Matl_Group = row[25].ToString();
                            tblObj.Mat_Grp_Descr = row[26].ToString();
                            tblObj.Cust_Ind_C = row[27].ToString();
                            tblObj.Cust_Ind_D = row[28].ToString();
                            tblObj.OrdRs = row[29].ToString();
                            //tblObj.Name = row["Address"].ToString();
                            //tblObj.Salary = (int)row["Salary"];
                            //tblObj.Age = (int)row["Age"];

                            db.OfferMasters.Add(tblObj);
                            db.SaveChanges();
                        }
                    }
                    //foreach (DataRow row in dt.Rows)
                    //{
                    //    if (!string.IsNullOrEmpty(row["Dv"].ToString()))
                    //    {
                    //        tblObj.Dv = row["Dv"].ToString();
                    //        tblObj.SaTy = row["SaTy"].ToString();
                    //        tblObj.Sold_to_pt = row["Sold_to_pt"].ToString();
                    //        tblObj.Name_1 = row["Name_1"].ToString();
                    //        tblObj.Purchase_order_number = row["Purchase_order_number"].ToString();
                    //        tblObj.Document = row["Document"].ToString();
                    //        tblObj.Item = row["Item"].ToString();
                    //        if (!string.IsNullOrEmpty(row["Doc_Date"].ToString()))
                    //        {
                    //            if (row["Doc_Date"].ToString().Contains("."))
                    //            {
                    //                row["Doc_Date"] = row["Doc_Date"].ToString().Replace(".", "-");
                    //            }
                    //            tblObj.Doc_Date = DateTime.ParseExact(row["Doc_Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    //        }
                    //        tblObj.Material = row["Material"].ToString();
                    //        tblObj.Material_Description = row["Material_Description"].ToString();
                    //        tblObj.Exch_Rate = row["Exch_Rate"].ToString();
                    //        tblObj.Curr = row["Curr"].ToString();
                    //        tblObj.ConfirmQty = row["ConfirmQty"].ToString();
                    //        tblObj.Net_value1 = row["Net_value1"].ToString();
                    //        tblObj.Net_price = row["Net_price"].ToString();
                    //        tblObj.Net_Value = row["Net_Value"].ToString();
                    //        tblObj.Prb = row["Prb"].ToString();
                    //        if (!string.IsNullOrEmpty(row["Dlv_Date"].ToString()))
                    //        {
                    //            if (row["Dlv_Date"].ToString().Contains("."))
                    //            {
                    //                row["Dlv_Date"] = row["Dlv_Date"].ToString().Replace(".", "-");
                    //            }
                    //            tblObj.Dlv_Date = DateTime.ParseExact(row["Dlv_Date"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    //        }
                    //        tblObj.Prod_hier = row["Prod_hier"].ToString();
                    //        tblObj.Prod_Description = row["Prod_Description"].ToString();
                    //        tblObj.CGrp = row["CGrp"].ToString();
                    //        tblObj.Key_Managr = row["Key_Managr"].ToString();
                    //        tblObj.SDst = row["SDst"].ToString();
                    //        tblObj.Customer_Z = row["Customer_Z"].ToString();
                    //        tblObj.Vendor_Name = row["Vendor_Name"].ToString();
                    //        tblObj.Matl_Group = row["Matl_Group"].ToString();
                    //        tblObj.Mat_Grp_Descr = row["Mat_Grp_Descr"].ToString();
                    //        tblObj.Cust_Ind_C = row["Cust_Ind_C"].ToString();
                    //        tblObj.Cust_Ind_D = row["Cust_Ind_D"].ToString();
                    //        tblObj.OrdRs = row["OrdRs"].ToString();
                    //        //tblObj.Name = row["Address"].ToString();
                    //        //tblObj.Salary = (int)row["Salary"];
                    //        //tblObj.Age = (int)row["Age"];

                    //        db.OfferMasters.Add(tblObj);
                    //        db.SaveChanges();
                    //    }
                    //}

                }



                //oledbConn.Open();
                //using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
                //{
                //    OleDbDataAdapter oleda = new OleDbDataAdapter();
                //    oleda.SelectCommand = cmd;
                //    DataSet ds = new DataSet();
                //    oleda.Fill(ds);

                //    dt = ds.Tables[0];

                //    if (dt.Rows.Count > 0)
                //    {
                //        SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                //        string s = "Truncate Table offermaster";
                //        con.Open();
                //        SqlCommand Com = new SqlCommand(s, con);
                //        Com.ExecuteNonQuery();
                //        con.Close();
                //        OfferMaster tblObj = new OfferMaster();
                //        foreach (DataRow row in dt.Rows)
                //        {
                //            if (!string.IsNullOrEmpty(row["Dv"].ToString()))
                //            {
                //                tblObj.Dv = row["Dv"].ToString();
                //                tblObj.SaTy = row["SaTy"].ToString();
                //                tblObj.Sold_to_pt = row["Sold_to_pt"].ToString();
                //                tblObj.Name_1 = row["Name_1"].ToString();
                //                tblObj.Purchase_order_number = row["Purchase_order_number"].ToString();
                //                tblObj.Document = row["Document"].ToString();
                //                tblObj.Item = row["Item"].ToString();
                //                if (!string.IsNullOrEmpty(row["Doc_Date"].ToString()))
                //                {
                //                    if (row["Doc_Date"].ToString().Contains("."))
                //                    {
                //                        row["Doc_Date"] = row["Doc_Date"].ToString().Replace(".", "-");
                //                    }
                //                    tblObj.Doc_Date = Convert.ToDateTime(row["Doc_Date"].ToString());
                //                }
                //                tblObj.Material = row["Material"].ToString();
                //                tblObj.Material_Description = row["Material_Description"].ToString();
                //                tblObj.Exch_Rate = row["Exch_Rate"].ToString();
                //                tblObj.Curr = row["Curr"].ToString();
                //                tblObj.ConfirmQty = row["ConfirmQty"].ToString();
                //                tblObj.Net_value1 = row["Net_value1"].ToString();
                //                tblObj.Net_price = row["Net_price"].ToString();
                //                tblObj.Net_Value = row["Net_Value"].ToString();
                //                tblObj.Prb = row["Prb"].ToString();
                //                if (!string.IsNullOrEmpty(row["Dlv_Date"].ToString()))
                //                {
                //                    if (row["Dlv_Date"].ToString().Contains("."))
                //                    {
                //                        row["Dlv_Date"] = row["Dlv_Date"].ToString().Replace(".", "-");
                //                    }
                //                    tblObj.Dlv_Date = Convert.ToDateTime(row["Dlv_Date"].ToString());
                //                }
                //                tblObj.Prod_hier = row["Prod_hier"].ToString();
                //                tblObj.Prod_Description = row["Prod_Description"].ToString();
                //                tblObj.CGrp = row["CGrp"].ToString();
                //                tblObj.Key_Managr = row["Key_Managr"].ToString();
                //                tblObj.SDst = row["SDst"].ToString();
                //                tblObj.Customer_Z = row["Customer_Z"].ToString();
                //                tblObj.Vendor_Name = row["Vendor_Name"].ToString();
                //                tblObj.Matl_Group = row["Matl_Group"].ToString();
                //                tblObj.Mat_Grp_Descr = row["Mat_Grp_Descr"].ToString();
                //                tblObj.Cust_Ind_C = row["Cust_Ind_C"].ToString();
                //                tblObj.Cust_Ind_D = row["Cust_Ind_D"].ToString();
                //                //tblObj.OrdRs = row["OrdRs"].ToString();
                //                //tblObj.Name = row["Address"].ToString();
                //                //tblObj.Salary = (int)row["Salary"];
                //                //tblObj.Age = (int)row["Age"];

                //                db.OfferMasters.Add(tblObj);
                //                db.SaveChanges();
                //            }
                //        }
                //    }
                //}
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
            finally
            {
                //oledbConn.Close();
            }
            return result;
        }

        public ActionResult PendingInvExcelToDatabase()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            bool result = false;
            ViewBag.data = null;

            string query = null;
            string connString = "";
            string filename = "Pending invoice.xlsx";
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            string[] validFileTypes = { ".xls", ".xlsx" };

            
            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Document"), filename);
            
            if (validFileTypes.Contains(extension))
            {
                //Connection String to Excel Workbook
                if (extension.Trim() == ".xls")
                {
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    result = ImportPendingInvExceltoDatabase(path1);
                }
                else if (extension.Trim() == ".xlsx")
                {
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    result = ImportPendingInvExceltoDatabase(path1);
                }
            }
            else
            {
                ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
            }
            // }
            if (result)
            {
                ViewBag.data = "Pending invoice Import Successfully from excel to database.";
            }
            else
            {
                logger.Error(result);
                ViewBag.data = "there is some issue while importing the Data.";
            }
            return View("Index");
        }

        #region old code
        //public bool ImportPendingInvExceltoDatabase(string strFilePath, string connString)
        //{
        //    bool result = false;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (XLWorkbook workBook = new XLWorkbook(strFilePath))
        //        {
        //            //Read the first Sheet from Excel file.
        //            IXLWorksheet workSheet = workBook.Worksheet(1);



        //            //Loop through the Worksheet rows.
        //            bool firstRow = true;
        //            foreach (IXLRow row in workSheet.Rows())
        //            {
        //                //Use the first row to add columns to DataTable.
        //                if (firstRow)
        //                {
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {
        //                        if (count < 39)
        //                        {
        //                            count = count + 1;
        //                            dt.Columns.Add(cell.Address.ToString());
        //                        }

        //                    }
        //                    firstRow = false;
        //                }
        //                else
        //                {
        //                    //Add rows to DataTable.
        //                    dt.Rows.Add();
        //                    int i = 0;
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {

        //                        if (count < 39)
        //                        {
        //                            count = count + 1;
        //                            if (string.IsNullOrEmpty(cell.Value.ToString()))
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
        //                            }
        //                            else
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                            }
        //                            i++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (dt.Rows.Count > 0)
        //        {
        //            SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //            string s = "Truncate Table pending_invoice_Master";
        //            con.Open();
        //            SqlCommand Com = new SqlCommand(s, con);
        //            Com.ExecuteNonQuery();
        //            con.Close();
        //            Pending_invoice_Master tblObj = new Pending_invoice_Master();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (!string.IsNullOrEmpty(row[0].ToString()))
        //                {
        //                    tblObj.CoCd = row[0].ToString();
        //                    tblObj.Customer = row[1].ToString();
        //                    tblObj.Assignment = row[2].ToString();
        //                    tblObj.Year = row[3].ToString();
        //                    tblObj.DocumentNo = row[4].ToString();
        //                    if (!string.IsNullOrEmpty(row[5].ToString()))
        //                    {
        //                        if (row[5].ToString().Contains("."))
        //                        {
        //                            row[5] = row[5].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Pstng_Date = DateTime.ParseExact(row[5].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Pstng_Date = Convert.ToDateTime(row[5].ToString());
        //                    }
        //                    //tblObj.Pstng_Date = row[5].ToString();
        //                    if (!string.IsNullOrEmpty(row[6].ToString()))
        //                    {
        //                        if (row[6].ToString().Contains("."))
        //                        {
        //                            row[6] = row[6].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Doc_Date = DateTime.ParseExact(row[6].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Doc_Date = Convert.ToDateTime(row[6].ToString());
        //                    }
        //                    //tblObj.Doc_Date = row[6].ToString();
        //                    tblObj.Reference = row[7].ToString();
        //                    tblObj.Doc_Type = row[8].ToString();
        //                    tblObj.Period = row[9].ToString();
        //                    tblObj.D_C = row[10].ToString();
        //                    tblObj.Amount_in_LC = row[11].ToString();
        //                    tblObj.Amount_in_LC1 = row[12].ToString();
        //                    tblObj.Text = row[13].ToString();
        //                    if (!string.IsNullOrEmpty(row[14].ToString()))
        //                    {
        //                        if (row[14].ToString().Contains("."))
        //                        {
        //                            row[14] = row[14].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Bline_Date = Convert.ToDateTime(row[14].ToString());
        //                    }
        //                    //tblObj.Bline_Date = row[14].ToString();
        //                    tblObj.PayT = row[15].ToString();
        //                    tblObj.Sales_Doc = row[16].ToString();
        //                    tblObj.Item = row[17].ToString();
        //                    tblObj.Payment_reference = row[18].ToString();
        //                    tblObj.Group = row[19].ToString();
        //                    tblObj.Plan_group = row[20].ToString();
        //                    tblObj.G_L_Acct = row[21].ToString();
        //                    tblObj.Customer_Number_1 = row[22].ToString();
        //                    tblObj.Document_Type = row[23].ToString();
        //                    tblObj.Debit_Credit_Indicator = row[24].ToString();
        //                    tblObj.Itm = row[25].ToString();
        //                    tblObj.Recon_acct = row[26].ToString();
        //                    tblObj.Group1 = row[27].ToString();
        //                    tblObj.Cl = row[28].ToString();
        //                    tblObj.Customer_classification = row[29].ToString();
        //                    tblObj.Customer_Account_Group = row[30].ToString();
        //                    tblObj.Pers_No = row[31].ToString();
        //                    tblObj.Planning_group = row[32].ToString();
        //                    tblObj.Last_name_First_name = row[33].ToString();
        //                    tblObj.Crcy = row[34].ToString();
        //                    tblObj.Amount = row[35].ToString();
        //                    tblObj.Amount1 = row[36].ToString();
        //                    tblObj.Day1 = row[37].ToString();
        //                    tblObj.Disc1 = row[38].ToString();


        //                    db.Pending_invoice_Master.Add(tblObj);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.Message);
        //        result = false;
        //    }
        //    finally
        //    {
        //        //oledbConn.Close();
        //    }
        //    return result;
        //}

        #endregion
        public bool ImportPendingInvExceltoDatabase(string strFilePath)
        {
            bool result = false;
            DataTable dt = new DataTable();
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(strFilePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);



                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (count < 39)
                                {
                                    count = count + 1;
                                    dt.Columns.Add(cell.Address.ToString());
                                }

                            }
                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {

                                if (count < 39)
                                {
                                    count = count + 1;
                                    if (string.IsNullOrEmpty(cell.Value.ToString()))
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
                                    }
                                    else
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                    string s = "Truncate Table pending_invoice_Master";
                    con.Open();
                    SqlCommand Com = new SqlCommand(s, con);
                    Com.ExecuteNonQuery();
                    con.Close();
                    Pending_invoice_Master tblObj = new Pending_invoice_Master();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row[0].ToString()))
                        {
                            tblObj.CoCd = row[0].ToString();
                            tblObj.Customer = row[1].ToString();
                            tblObj.Assignment = row[2].ToString();
                            tblObj.Year = row[3].ToString();
                            tblObj.DocumentNo = row[4].ToString();
                            if (!string.IsNullOrEmpty(row[5].ToString()))
                            {
                                if (row[5].ToString().Contains("."))
                                {
                                    row[5] = row[5].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Pstng_Date = DateTime.ParseExact(row[5].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[5].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Pstng_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);
                                }
                                //tblObj.Pstng_Date = Convert.ToDateTime(row[5].ToString());
                            }
                            //tblObj.Pstng_Date = row[5].ToString();
                            if (!string.IsNullOrEmpty(row[6].ToString()))
                            {
                                if (row[6].ToString().Contains("."))
                                {
                                    row[6] = row[6].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Doc_Date = DateTime.ParseExact(row[6].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[6].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Doc_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }

                                //tblObj.Doc_Date = Convert.ToDateTime(row[6].ToString());
                            }
                            //tblObj.Doc_Date = row[6].ToString();
                            tblObj.Reference = row[7].ToString();
                            tblObj.Doc_Type = row[8].ToString();
                            tblObj.Period = row[9].ToString();
                            tblObj.D_C = row[10].ToString();
                            tblObj.Amount_in_LC = row[11].ToString();
                            tblObj.Amount_in_LC1 = row[12].ToString();
                            tblObj.Text = row[13].ToString();
                            if (!string.IsNullOrEmpty(row[14].ToString()))
                            {
                                if (row[14].ToString().Contains("."))
                                {
                                    row[14] = row[14].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[14].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Bline_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Bline_Date = Convert.ToDateTime(row[14].ToString());
                            }
                            //tblObj.Bline_Date = row[14].ToString();
                            tblObj.PayT = row[15].ToString();
                            tblObj.Sales_Doc = row[16].ToString();
                            tblObj.Item = row[17].ToString();
                            tblObj.Payment_reference = row[18].ToString();
                            tblObj.Group = row[19].ToString();
                            tblObj.Plan_group = row[20].ToString();
                            tblObj.G_L_Acct = row[21].ToString();
                            tblObj.Customer_Number_1 = row[22].ToString();
                            tblObj.Document_Type = row[23].ToString();
                            tblObj.Debit_Credit_Indicator = row[24].ToString();
                            tblObj.Itm = row[25].ToString();
                            tblObj.Recon_acct = row[26].ToString();
                            tblObj.Group1 = row[27].ToString();
                            tblObj.Cl = row[28].ToString();
                            tblObj.Customer_classification = row[29].ToString();
                            tblObj.Customer_Account_Group = row[30].ToString();
                            tblObj.Pers_No = row[31].ToString();
                            tblObj.Planning_group = row[32].ToString();
                            tblObj.Last_name_First_name = row[33].ToString();
                            tblObj.Crcy = row[34].ToString();
                            tblObj.Amount = row[35].ToString();
                            tblObj.Amount1 = row[36].ToString();
                            tblObj.Day1 = row[37].ToString();
                            tblObj.Disc1 = row[38].ToString();


                            db.Pending_invoice_Master.Add(tblObj);
                            db.SaveChanges();
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = false;
                throw;
            }
            finally
            {
                //oledbConn.Close();
            }
            return result;
        }
        public ActionResult SummaryExcelToDatabase()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            bool result = false;
            ViewBag.data = null;

            string query = null;
            string connString = "";
            string filename = "Summary.xlsx";
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            string[] validFileTypes = { ".xls", ".xlsx" };


            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Document"), filename);

            if (validFileTypes.Contains(extension))
            {
                //Connection String to Excel Workbook
                if (extension.Trim() == ".xls")
                {
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    result = ImportSummaryExceltoDatabase(path1);
                }
                else if (extension.Trim() == ".xlsx")
                {
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    result = ImportSummaryExceltoDatabase(path1);
                }
            }
            else
            {
                ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
            }
            // }
            if (result)
            {
                ViewBag.data = "Summary Import Successfully from excel to database.";
            }
            else
            {
                logger.Error(result);
                ViewBag.data = "there is some issue while importing the Data.";
            }
            return View("Index");
        }

        #region old code
        //public bool ImportSummaryExceltoDatabase(string strFilePath, string connString)
        //{
        //    bool result = false;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (XLWorkbook workBook = new XLWorkbook(strFilePath))
        //        {
        //            //Read the first Sheet from Excel file.
        //            IXLWorksheet workSheet = workBook.Worksheet(1);



        //            //Loop through the Worksheet rows.
        //            bool firstRow = true;
        //            foreach (IXLRow row in workSheet.Rows())
        //            {
        //                //Use the first row to add columns to DataTable.
        //                if (firstRow)
        //                {
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {
        //                        if (count < 46)
        //                        {
        //                            count = count + 1;
        //                            dt.Columns.Add(cell.Address.ToString());
        //                        }

        //                    }
        //                    firstRow = false;
        //                }
        //                else
        //                {
        //                    //Add rows to DataTable.
        //                    dt.Rows.Add();
        //                    int i = 0;
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {

        //                        if (count < 46)
        //                        {
        //                            count = count + 1;
        //                            if (string.IsNullOrEmpty(cell.Value.ToString()))
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
        //                            }
        //                            else
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                            }
        //                            i++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (dt.Rows.Count > 0)
        //        {
        //            SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //            string s = "Truncate Table summary_master";
        //            con.Open();
        //            SqlCommand Com = new SqlCommand(s, con);
        //            Com.ExecuteNonQuery();
        //            con.Close();
        //            Summary_Master tblObj = new Summary_Master();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (!string.IsNullOrEmpty(row[0].ToString()))
        //                {
        //                    tblObj.Dv = row[0].ToString();
        //                    tblObj.Division = row[1].ToString();
        //                    tblObj.Sold_to_pt = row[2].ToString();
        //                    tblObj.Sold_to_party = row[3].ToString();
        //                    tblObj.Material = row[4].ToString();
        //                    tblObj.Material_Number = row[5].ToString();
        //                    tblObj.Product_hierarchy = row[6].ToString();
        //                    tblObj.Pers_No = row[7].ToString();
        //                    tblObj.Last_name_First_name = row[8].ToString();
        //                    tblObj.Group = row[9].ToString();
        //                    tblObj.Group_1 = row[10].ToString();
        //                    tblObj.Customer_Account_Group = row[11].ToString();
        //                    tblObj.Cl = row[12].ToString();
        //                    tblObj.Customer_classification = row[13].ToString();
        //                    if (!string.IsNullOrEmpty(row[14].ToString()))
        //                    {
        //                        if (row[14].ToString().Contains("."))
        //                        {
        //                            row[14] = row[14].ToString().Replace(".", "-");
        //                        }
        //                        tblObj.Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        //tblObj.Date = Convert.ToDateTime(row[14].ToString());
        //                    }
        //                    //tblObj.Date = row[0].ToString();
        //                    tblObj.Indus = row[15].ToString();
        //                    tblObj.Industry_key = row[16].ToString();
        //                    tblObj.Matl_Group = row[17].ToString();
        //                    tblObj.Product_hierarchy_2 = row[18].ToString();
        //                    tblObj.Plan_group = row[19].ToString();
        //                    tblObj.Material_Group = row[20].ToString();
        //                    tblObj.Material_Type = row[21].ToString();
        //                    tblObj.Annual_sales = row[22].ToString();
        //                    tblObj.Annual_sales_cur = row[23].ToString();
        //                    tblObj.Incoming_orders = row[24].ToString();
        //                    tblObj.Incoming_orders_cur = row[25].ToString();
        //                    tblObj.Incoming_orders_quantity = row[26].ToString();
        //                    tblObj.Incoming_orders_quantity_unit = row[27].ToString();
        //                    tblObj.Sales = row[28].ToString();
        //                    tblObj.Sales_cur = row[29].ToString();
        //                    tblObj.Invoiced_Quantity = row[30].ToString();
        //                    tblObj.Invoiced_Quantity_unit = row[31].ToString();
        //                    tblObj.Open_orders = row[32].ToString();
        //                    tblObj.Open_orders_cur = row[33].ToString();
        //                    tblObj.Open_orders_quantity = row[34].ToString();
        //                    tblObj.Open_orders_quantity_unit = row[35].ToString();
        //                    tblObj.City = row[36].ToString();
        //                    tblObj.Region_State_Province_Count = row[37].ToString();
        //                    tblObj.Country_Key = row[38].ToString();
        //                    tblObj.Ind_Code_1 = row[39].ToString();
        //                    tblObj.Ind_code_2 = row[40].ToString();
        //                    tblObj.Ind_code_3 = row[41].ToString();
        //                    tblObj.MRPC = row[42].ToString();
        //                    tblObj.MRP_Controller_Materials_Plan = row[43].ToString();
        //                    tblObj.Product_hierarchy_3 = row[44].ToString();
        //                    tblObj.Number_of_the_level_in_the_pro = row[45].ToString();


        //                    db.Summary_Master.Add(tblObj);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.Message);
        //        result = false;
        //    }
        //    finally
        //    {
        //        //oledbConn.Close();
        //    }
        //    return result;
        //}

        #endregion
        public bool ImportSummaryExceltoDatabase(string strFilePath)
        {
            bool result = false;
            DataTable dt = new DataTable();
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(strFilePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);



                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (count < 46)
                                {
                                    count = count + 1;
                                    dt.Columns.Add(cell.Address.ToString());
                                }

                            }
                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {

                                if (count < 46)
                                {
                                    count = count + 1;
                                    if (string.IsNullOrEmpty(cell.Value.ToString()))
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
                                    }
                                    else
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                    string s = "Truncate Table summary_master";
                    con.Open();
                    SqlCommand Com = new SqlCommand(s, con);
                    Com.ExecuteNonQuery();
                    con.Close();
                    Summary_Master tblObj = new Summary_Master();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row[0].ToString()))
                        {
                            tblObj.Dv = row[0].ToString();
                            tblObj.Division = row[1].ToString();
                            tblObj.Sold_to_pt = row[2].ToString();
                            tblObj.Sold_to_party = row[3].ToString();
                            tblObj.Material = row[4].ToString();
                            tblObj.Material_Number = row[5].ToString();
                            tblObj.Product_hierarchy = row[6].ToString();
                            tblObj.Pers_No = row[7].ToString();
                            tblObj.Last_name_First_name = row[8].ToString();
                            tblObj.Group = row[9].ToString();
                            tblObj.Group_1 = row[10].ToString();
                            tblObj.Customer_Account_Group = row[11].ToString();
                            tblObj.Cl = row[12].ToString();
                            tblObj.Customer_classification = row[13].ToString();
                            if (!string.IsNullOrEmpty(row[14].ToString()))
                            {
                                if (row[14].ToString().Contains("."))
                                {
                                    row[14] = row[14].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Date = DateTime.ParseExact(row[14].ToString(), "MM-dd-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[14].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Date = Convert.ToDateTime(row[14].ToString());
                            }
                            //tblObj.Date = row[0].ToString();
                            tblObj.Indus = row[15].ToString();
                            tblObj.Industry_key = row[16].ToString();
                            tblObj.Matl_Group = row[17].ToString();
                            tblObj.Product_hierarchy_2 = row[18].ToString();
                            tblObj.Plan_group = row[19].ToString();
                            tblObj.Material_Group = row[20].ToString();
                            tblObj.Material_Type = row[21].ToString();
                            tblObj.Annual_sales = row[22].ToString();
                            tblObj.Annual_sales_cur = row[23].ToString();
                            tblObj.Incoming_orders = row[24].ToString();
                            tblObj.Incoming_orders_cur = row[25].ToString();
                            tblObj.Incoming_orders_quantity = row[26].ToString();
                            tblObj.Incoming_orders_quantity_unit = row[27].ToString();
                            tblObj.Sales = row[28].ToString();
                            tblObj.Sales_cur = row[29].ToString();
                            tblObj.Invoiced_Quantity = row[30].ToString();
                            tblObj.Invoiced_Quantity_unit = row[31].ToString();
                            tblObj.Open_orders = row[32].ToString();
                            tblObj.Open_orders_cur = row[33].ToString();
                            tblObj.Open_orders_quantity = row[34].ToString();
                            tblObj.Open_orders_quantity_unit = row[35].ToString();
                            tblObj.City = row[36].ToString();
                            tblObj.Region_State_Province_Count = row[37].ToString();
                            tblObj.Country_Key = row[38].ToString();
                            tblObj.Ind_Code_1 = row[39].ToString();
                            tblObj.Ind_code_2 = row[40].ToString();
                            tblObj.Ind_code_3 = row[41].ToString();
                            tblObj.MRPC = row[42].ToString();
                            tblObj.MRP_Controller_Materials_Plan = row[43].ToString();
                            tblObj.Product_hierarchy_3 = row[44].ToString();
                            tblObj.Number_of_the_level_in_the_pro = row[45].ToString();


                            db.Summary_Master.Add(tblObj);
                            db.SaveChanges();
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = false;
                throw;
            }
            finally
            {
                //oledbConn.Close();
            }
            return result;
        }

        public ActionResult OrderExcelToDatabase()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            bool result = false;
            ViewBag.data = null;

            string query = null;
            string connString = "";
            string filename = "Order list.xlsx";
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            string[] validFileTypes = { ".xls", ".xlsx" };


            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Document"), filename);

            if (validFileTypes.Contains(extension))
            {
                //Connection String to Excel Workbook
                if (extension.Trim() == ".xls")
                {
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    result = ImportOrderExceltoDatabase(path1);
                }
                else if (extension.Trim() == ".xlsx")
                {
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    result = ImportOrderExceltoDatabase(path1);
                }
            }
            else
            {
                ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
            }
            // }
            if (result)
            {
                ViewBag.data = "Order list Import Successfully from excel to database.";
            }
            else
            {
                logger.Error(result);
                ViewBag.data = "there is some issue while importing the Data.";
            }
            return View("Index");
        }

        #region old code
        //public bool ImportOrderExceltoDatabase(string strFilePath, string connString)
        //{
        //    bool result = false;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (XLWorkbook workBook = new XLWorkbook(strFilePath))
        //        {
        //            //Read the first Sheet from Excel file.
        //            IXLWorksheet workSheet = workBook.Worksheet(1);



        //            //Loop through the Worksheet rows.
        //            bool firstRow = true;
        //            foreach (IXLRow row in workSheet.Rows())
        //            {
        //                //Use the first row to add columns to DataTable.
        //                if (firstRow)
        //                {
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {
        //                        if (count < 62)
        //                        {
        //                            count = count + 1;
        //                            dt.Columns.Add(cell.Address.ToString());
        //                        }

        //                    }
        //                    firstRow = false;
        //                }
        //                else
        //                {
        //                    //Add rows to DataTable.
        //                    dt.Rows.Add();
        //                    int i = 0;
        //                    int count = 0;
        //                    foreach (IXLCell cell in row.Cells())
        //                    {

        //                        if (count < 62)
        //                        {
        //                            count = count + 1;
        //                            if (string.IsNullOrEmpty(cell.Value.ToString()))
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
        //                            }
        //                            else
        //                            {
        //                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
        //                            }
        //                            i++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (dt.Rows.Count > 0)
        //        {
        //            SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
        //            string s = "Truncate Table ordermaster";
        //            con.Open();
        //            SqlCommand Com = new SqlCommand(s, con);
        //            Com.ExecuteNonQuery();
        //            con.Close();
        //            OrderMaster tblObj = new OrderMaster();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                if (!string.IsNullOrEmpty(row[0].ToString()))
        //                {
        //                    tblObj.Match = row[0].ToString();
        //                    tblObj.MRP_Controller = row[1].ToString();
        //                    tblObj.Dv = row[2].ToString();
        //                    tblObj.SaTy = row[3].ToString();
        //                    tblObj.Sales_Doc = row[4].ToString();
        //                    tblObj.Item = row[5].ToString();
        //                    tblObj.Sch_Line = row[6].ToString();
        //                    if (!string.IsNullOrEmpty(row[7].ToString()))
        //                    {
        //                        if (row[7].ToString().Contains("."))
        //                        {
        //                            row[7] = row[7].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Created_on = Convert.ToDateTime(row[7].ToString());
        //                    }
        //                    //tblObj.Created_on = row[7].ToString();
        //                    tblObj.Sold_to_party = row[8].ToString();
        //                    tblObj.Material = row[9].ToString();
        //                    tblObj.Description = row[10].ToString();
        //                    tblObj.Order_Qty = row[11].ToString();
        //                    tblObj.Open_Qty = row[12].ToString();
        //                    tblObj.Unit_Basic_Price = row[13].ToString();
        //                    tblObj.Curr = row[14].ToString();
        //                    tblObj.Total_Price_IRS = row[15].ToString();
        //                    tblObj.Purchase_order_no = row[16].ToString();
        //                    if (!string.IsNullOrEmpty(row[17].ToString()))
        //                    {
        //                        if (row[17].ToString().Contains("."))
        //                        {
        //                            row[17] = row[17].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.PO_date = Convert.ToDateTime(row[17].ToString());
        //                    }
        //                    //tblObj.PO_date = row[17].ToString();
        //                    tblObj.Usage = row[18].ToString();
        //                    tblObj.Old_material_no = row[19].ToString();
        //                    tblObj.Sold_to_pt = row[20].ToString();
        //                    if (!string.IsNullOrEmpty(row[21].ToString()))
        //                    {
        //                        if (row[21].ToString().Contains("."))
        //                        {
        //                            row[21] = row[21].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Req_dlv_dt = Convert.ToDateTime(row[21].ToString());
        //                    }
        //                    tblObj.Customer_group_1 = row[22].ToString();
        //                    tblObj.Product_hierarchy = row[23].ToString();
        //                    tblObj.Exchange_Rate = row[24].ToString();
        //                    tblObj.IncoT = row[25].ToString();
        //                    tblObj.Inco_2 = row[26].ToString();
        //                    tblObj.Payment_terms = row[27].ToString();
        //                    tblObj.Filter = row[28].ToString();
        //                    if (!string.IsNullOrEmpty(row[29].ToString()))
        //                    {
        //                        if (row[29].ToString().Contains("."))
        //                        {
        //                            row[29] = row[29].ToString().Replace(".", "-");
        //                        }
        //                        //tblObj.Bline_Date = DateTime.ParseExact(row[14].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //                        tblObj.Production_Date = Convert.ToDateTime(row[29].ToString());
        //                    }
        //                    //tblObj.Production_Date = row[29].ToString();
        //                    tblObj.Week_Required = row[30].ToString();
        //                    tblObj.Required_Year = row[31].ToString();
        //                    tblObj.Production_Week = row[32].ToString();
        //                    tblObj.Production_Year = row[33].ToString();
        //                    tblObj.Week_Scheduled = row[34].ToString();
        //                    tblObj.Week_Changed = row[35].ToString();
        //                    tblObj.Remarks_from_Production = row[36].ToString();
        //                    tblObj.Remarks_from_Marketing = row[37].ToString();
        //                    tblObj.Logistics_Remarks = row[38].ToString();
        //                    tblObj.Adv_Received = row[39].ToString();
        //                    tblObj.Marketing_Sales_Week = row[40].ToString();
        //                    tblObj.Credit_In_Week = row[41].ToString();
        //                    tblObj.Payment_Collection_Week = row[42].ToString();
        //                    tblObj.For_new_SO_entry_in_Remarks_Sheet = row[43].ToString();
        //                    tblObj.Pre_Dispatch_Inspection = row[44].ToString();
        //                    tblObj.Approved_Transporter = row[45].ToString();
        //                    tblObj.PI_Payment = row[46].ToString();
        //                    tblObj.Tax_amount = row[47].ToString();
        //                    tblObj.Grp2 = row[48].ToString();
        //                    tblObj.Grp3 = row[49].ToString();
        //                    tblObj.Grp4 = row[50].ToString();
        //                    tblObj.MG1 = row[51].ToString();
        //                    tblObj.Product_hierarchy1 = row[52].ToString();
        //                    tblObj.Material_group_1 = row[53].ToString();
        //                    tblObj.Product_hierarchy2 = row[54].ToString();
        //                    tblObj.Your_Ref = row[55].ToString();
        //                    tblObj.MG2 = row[56].ToString();
        //                    tblObj.Material_group_2 = row[57].ToString();
        //                    tblObj.Grp5 = row[58].ToString();
        //                    tblObj.Customer_group_5 = row[59].ToString();
        //                    tblObj.ConvFactor = row[60].ToString();
        //                    tblObj.Mat_Group = row[61].ToString();


        //                    db.OrderMasters.Add(tblObj);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.Message);
        //        result = false;
        //    }
        //    finally
        //    {
        //        //oledbConn.Close();
        //    }
        //    return result;
        //}
        #endregion

        public bool ImportOrderExceltoDatabase(string strFilePath)
        {
            bool result = false;
            DataTable dt = new DataTable();
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(strFilePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);



                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (count < 62)
                                {
                                    count = count + 1;
                                    dt.Columns.Add(cell.Address.ToString());
                                }

                            }
                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            int count = 0;
                            foreach (IXLCell cell in row.Cells())
                            {

                                if (count < 62)
                                {
                                    count = count + 1;
                                    if (string.IsNullOrEmpty(cell.Value.ToString()))
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = string.Empty;
                                    }
                                    else
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    SqlConnection con = new SqlConnection("Data Source=72.52.128.82;Initial Catalog=admin_vulcan2dev;User ID=admin_vulcan2dev;Password=Rd~g4f6t0m2LHuchr");
                    string s = "Truncate Table ordermaster";
                    con.Open();
                    SqlCommand Com = new SqlCommand(s, con);
                    Com.ExecuteNonQuery();
                    con.Close();
                    OrderMaster tblObj = new OrderMaster();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row[0].ToString()))
                        {
                            tblObj.Match = row[0].ToString();
                            tblObj.MRP_Controller = row[1].ToString();
                            tblObj.Dv = row[2].ToString();
                            tblObj.SaTy = row[3].ToString();
                            tblObj.Sales_Doc = row[4].ToString();
                            tblObj.Item = row[5].ToString();
                            tblObj.Sch_Line = row[6].ToString();
                            if (!string.IsNullOrEmpty(row[7].ToString()))
                            {
                                if (row[7].ToString().Contains("."))
                                {
                                    row[7] = row[7].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Created_on = DateTime.ParseExact(row[7].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[7].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Created_on = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);
                                }


                                //tblObj.Created_on = Convert.ToDateTime(row[7].ToString());
                            }
                            //tblObj.Created_on = row[7].ToString();
                            tblObj.Sold_to_party = row[8].ToString();
                            tblObj.Material = row[9].ToString();
                            tblObj.Description = row[10].ToString();
                            tblObj.Order_Qty = row[11].ToString();
                            tblObj.Open_Qty = row[12].ToString();
                            tblObj.Unit_Basic_Price = row[13].ToString();
                            tblObj.Curr = row[14].ToString();
                            tblObj.Total_Price_IRS = row[15].ToString();
                            tblObj.Purchase_order_no = row[16].ToString();
                            if (!string.IsNullOrEmpty(row[17].ToString()))
                            {
                                if (row[17].ToString().Contains("."))
                                {
                                    row[17] = row[17].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.PO_date = DateTime.ParseExact(row[17].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[17].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.PO_date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);
                                }
                                //tblObj.PO_date = Convert.ToDateTime(row[17].ToString());
                            }
                            //tblObj.PO_date = row[17].ToString();
                            tblObj.Usage = row[18].ToString();
                            tblObj.Old_material_no = row[19].ToString();
                            tblObj.Sold_to_pt = row[20].ToString();
                            if (!string.IsNullOrEmpty(row[21].ToString()))
                            {
                                if (row[21].ToString().Contains("."))
                                {
                                    row[21] = row[21].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Req_dlv_dt = DateTime.ParseExact(row[21].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[21].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Req_dlv_dt = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Req_dlv_dt = Convert.ToDateTime(row[21].ToString());
                            }
                            tblObj.Customer_group_1 = row[22].ToString();
                            tblObj.Product_hierarchy = row[23].ToString();
                            tblObj.Exchange_Rate = row[24].ToString();
                            tblObj.IncoT = row[25].ToString();
                            tblObj.Inco_2 = row[26].ToString();
                            tblObj.Payment_terms = row[27].ToString();
                            tblObj.Filter = row[28].ToString();
                            if (!string.IsNullOrEmpty(row[29].ToString()))
                            {
                                if (row[29].ToString().Contains("."))
                                {
                                    row[29] = row[29].ToString().Replace(".", "-");
                                }
                                try
                                {
                                    tblObj.Production_Date = DateTime.ParseExact(row[29].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    double d = double.Parse(row[29].ToString());
                                    DateTime conv = DateTime.FromOADate(d);
                                    var strconv = conv.ToShortDateString().Replace("/", "-");
                                    ////DateTime dt1 = DateTime.FromOADate((int)row[7]);
                                    tblObj.Production_Date = DateTime.ParseExact(strconv, "M-d-yyyy", CultureInfo.InvariantCulture);

                                }
                                //tblObj.Production_Date = Convert.ToDateTime(row[29].ToString());
                            }
                            //tblObj.Production_Date = row[29].ToString();
                            tblObj.Week_Required = row[30].ToString();
                            tblObj.Required_Year = row[31].ToString();
                            tblObj.Production_Week = row[32].ToString();
                            tblObj.Production_Year = row[33].ToString();
                            tblObj.Week_Scheduled = row[34].ToString();
                            tblObj.Week_Changed = row[35].ToString();
                            tblObj.Remarks_from_Production = row[36].ToString();
                            tblObj.Remarks_from_Marketing = row[37].ToString();
                            tblObj.Logistics_Remarks = row[38].ToString();
                            tblObj.Adv_Received = row[39].ToString();
                            tblObj.Marketing_Sales_Week = row[40].ToString();
                            tblObj.Credit_In_Week = row[41].ToString();
                            tblObj.Payment_Collection_Week = row[42].ToString();
                            tblObj.For_new_SO_entry_in_Remarks_Sheet = row[43].ToString();
                            tblObj.Pre_Dispatch_Inspection = row[44].ToString();
                            tblObj.Approved_Transporter = row[45].ToString();
                            tblObj.PI_Payment = row[46].ToString();
                            tblObj.Tax_amount = row[47].ToString();
                            tblObj.Grp2 = row[48].ToString();
                            tblObj.Grp3 = row[49].ToString();
                            tblObj.Grp4 = row[50].ToString();
                            tblObj.MG1 = row[51].ToString();
                            tblObj.Product_hierarchy1 = row[52].ToString();
                            tblObj.Material_group_1 = row[53].ToString();
                            tblObj.Product_hierarchy2 = row[54].ToString();
                            tblObj.Your_Ref = row[55].ToString();
                            tblObj.MG2 = row[56].ToString();
                            tblObj.Material_group_2 = row[57].ToString();
                            tblObj.Grp5 = row[58].ToString();
                            tblObj.Customer_group_5 = row[59].ToString();
                            tblObj.ConvFactor = row[60].ToString();
                            tblObj.Mat_Group = row[61].ToString();


                            db.OrderMasters.Add(tblObj);
                            db.SaveChanges();
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = false;
                throw;
            }
            finally
            {
                //oledbConn.Close();
            }
            return result;
        }
    }
}