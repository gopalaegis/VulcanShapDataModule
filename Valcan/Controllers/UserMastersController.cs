using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DAL;
using Valcan.Models;
using PagedList;
using Valcan.CommandClass;

namespace Valcan.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserMastersController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(UserMastersController));  //Declaring Log4Net
        public int ChangePasswordInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ChangePasswordInterval"].ToString().Trim());

        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();
        public string Key = ConfigurationManager.AppSettings["passwordkey"].ToString();
        // GET: UserMasters
        public ActionResult Index()
        {
            ViewBag.CurrentSort = "Name";
            ViewBag.CurrentPageSize = 10;


            var users = from s in db.UserMasters
                        select s;

            users = users.OrderBy(s => s.FirstName);
            var lastUpload = db.UploadExcel_Audit.OrderByDescending(x => x.Id).FirstOrDefault();
            if (lastUpload != null)
            {
                ViewBag.lastUpload = lastUpload.UploadDate;
            }
            return View(users.ToPagedList(1, 10));

        }

        public PartialViewResult GetUserRecord(string sortOrder, int pagesize, int? pagenumber)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentPageSize = pagesize;


            var users = from s in db.UserMasters
                        select s;
            switch (sortOrder)
            {

                case "Id":
                    users = users.OrderBy(s => s.ID);
                    break;

                default:
                    users = users.OrderBy(s => s.FirstName);
                    break;
            }

            int pageno = (pagenumber ?? 1);

            return PartialView("_UserListing", users.ToPagedList(pageno, pagesize));
        }

        // GET: UserMasters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMaster userMaster = db.UserMasters.Find(id);
            if (userMaster == null)
            {
                return HttpNotFound();
            }
            return View(userMaster);
        }
        [HttpPost]
        public ActionResult GetUserDetails(string UserID)
        {
            string[] UserID1 = UserID.Trim().Split('_');
            int uid = Convert.ToInt32(UserID1[1]);
            UserMaster userMaster = db.UserMasters.Find(uid);




            return PartialView("_UserDetailsPopup", userMaster);
        }
        // GET: UserMasters/Create
        public ActionResult Create()
        {
            CreateUserViewModel collectionVM = new CreateUserViewModel();
            List<KMChoiceViewModel> choiceList = new List<KMChoiceViewModel>();
            var KeyManager = db.KeyManagerMasters.Where(x => x.IsActive == true).ToList();
            foreach (var i in KeyManager)
            {
                choiceList.Add(new KMChoiceViewModel() { SNo = i.ID, Text = i.KeyManager_Name });
            }
            //choiceList.Add(new ChoiceViewModel() { SNo = 1, Text = "Objective Choice 1" });
            List<ChoiceViewModel> choiceRoleList = new List<ChoiceViewModel>();
            var roles = db.RoleMasters.Where(x => x.IsActive == true).ToList();
            foreach (var i in roles)
            {
                choiceRoleList.Add(new ChoiceViewModel() { SNo = i.ID, Text = i.RoleName });
            }
            //choiceList.Add(new ChoiceViewModel() { SNo = 1, Text = "Objective Choice 1" });

            collectionVM.ChoicesVM = choiceRoleList;
            collectionVM.SelectedRoleChoices = new List<long>();
            collectionVM.ID = 0;
            collectionVM.KMChoicesVM = choiceList;
            collectionVM.SelectedChoices = new List<long>();
            return View(collectionVM);

            //return View();
        }

        // POST: UserMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel createUserViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CreateUserViewModel collectionVM = new CreateUserViewModel();
                    List<KMChoiceViewModel> choiceKMList = new List<KMChoiceViewModel>();
                    List<ChoiceViewModel> choiceList = new List<ChoiceViewModel>();
                    var KeyManager = db.KeyManagerMasters.Where(x => x.IsActive == true).ToList();
                    foreach (var i in KeyManager)
                    {
                        choiceKMList.Add(new KMChoiceViewModel() { SNo = i.ID, Text = i.KeyManager_Name });
                    }
                    collectionVM.KMChoicesVM = choiceKMList;
                    collectionVM.SelectedChoices = createUserViewModel.SelectedChoices;

                    var roles = db.RoleMasters.Where(x => x.IsActive == true).ToList();
                    foreach (var i in roles)
                    {
                        choiceList.Add(new ChoiceViewModel() { SNo = i.ID, Text = i.RoleName });
                    }
                    collectionVM.ChoicesVM = choiceList;
                    collectionVM.SelectedRoleChoices = createUserViewModel.SelectedRoleChoices;

                    if (createUserViewModel.SelectedChoices != null && createUserViewModel.SelectedRoleChoices !=null)
                    {
                        var isexist = db.UserMasters.Any(x => x.EmailID == createUserViewModel.EmailID && x.IsActive == true);
                        if (isexist)
                        {
                            ViewData["error"] = "Email Already Exists";



                            return View(collectionVM);

                        }
                        CommonMethod cm = new CommonMethod();
                        UserMaster userMaster = new UserMaster
                        {
                            FirstName = createUserViewModel.FirstName,
                            LastName = "+" + createUserViewModel.countryflag + createUserViewModel.LastName,
                            Password = cm.EncryptData(createUserViewModel.Password, Key),
                            CreatedBy = Convert.ToInt32(Session["UserID"]),
                            CreatedOn = DateTime.Now,
                            LastModifiedBy = Convert.ToInt32(Session["UserID"]),
                            EmailID = createUserViewModel.EmailID,
                            LastModifiedOn = DateTime.Now,
                            ChangePasswordInterval = ChangePasswordInterval,
                            IsActive = true,
                            IsFirstLogin = true,

                        };
                        db.UserMasters.Add(userMaster);
                        db.SaveChanges();
                        int uid = userMaster.ID;
                        foreach (int i in createUserViewModel.SelectedChoices)
                        {
                            UserInKeyManagerMaster userInRoleMaster = new UserInKeyManagerMaster
                            {
                                UserID = uid,
                                KeyManagerID = i,
                                CreatedBy = Convert.ToInt32(Session["UserID"]),
                                CreatedOn = DateTime.Now,

                            };
                            db.UserInKeyManagerMasters.Add(userInRoleMaster);
                            db.SaveChanges();

                        }
                        foreach (int i in createUserViewModel.SelectedRoleChoices)
                        {
                            UserInRoleMaster userInRoleMaster = new UserInRoleMaster
                            {
                                UserID = uid,
                                RoleID = i,
                                CreatedBy = Convert.ToInt32(Session["UserID"]),
                                CreatedOn = DateTime.Now,

                            };
                            db.UserInRoleMasters.Add(userInRoleMaster);
                            db.SaveChanges();

                        }
                        UserPasswordHistory userPasswordHistory = new UserPasswordHistory()
                        {
                            UserID = uid,
                            CreatedBy = Convert.ToInt32(Session["UserID"]),
                            Password = cm.EncryptData(createUserViewModel.Password, Key),
                            CreatedOn = DateTime.Now,
                        };
                        db.UserPasswordHistories.Add(userPasswordHistory);
                        db.SaveChanges();

                        ViewData["success"] = "User Addedd Successfully";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        if (createUserViewModel.SelectedChoices == null)
                        {
                            collectionVM.KMChoicesVM = choiceKMList;
                            collectionVM.SelectedChoices = createUserViewModel.SelectedChoices;
                            createUserViewModel.LastName = "+" + createUserViewModel.countryflag + createUserViewModel.LastName;

                            createUserViewModel.KMChoicesVM = choiceKMList;
                            createUserViewModel.SelectedChoices = new List<long>();
                            ViewData["error"] = "Please Select Atleast One Key Manager";
                            return View(createUserViewModel);
                        }
                        else
                        {
                            collectionVM.ChoicesVM = choiceList;
                            collectionVM.SelectedRoleChoices = createUserViewModel.SelectedRoleChoices;
                            createUserViewModel.LastName = "+" + createUserViewModel.countryflag + createUserViewModel.LastName;

                            createUserViewModel.ChoicesVM = choiceList;
                            createUserViewModel.SelectedRoleChoices = new List<long>();
                            ViewData["error"] = "Please Select Atleast One Role";
                            return View(createUserViewModel);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(createUserViewModel);
        }
        // GET: UserMasters/Edit/5
        public async Task<ActionResult> Edit(string UserID)
        {
            //For getting userid form client and convert to integer.
            string[] UserID1 = UserID.Trim().Split('_');
            int uid = Convert.ToInt32(UserID1[1]);

            //Checking if user exist or not.
            UserMaster userMaster = await db.UserMasters.FindAsync(uid);
            CreateUserViewModel createUserViewModel = new CreateUserViewModel();

            //List of Roles
            List<KMChoiceViewModel> choiceList = new List<KMChoiceViewModel>();
            var roles = await db.KeyManagerMasters.Where(x => x.IsActive == true).ToListAsync();
            foreach (var i in roles)
            {
                choiceList.Add(new KMChoiceViewModel() { SNo = i.ID, Text = i.KeyManager_Name });
            }
            createUserViewModel.Password = userMaster.Password;
            createUserViewModel.ID = userMaster.ID;
            createUserViewModel.FirstName = userMaster.FirstName;
            createUserViewModel.LastName = userMaster.LastName;
            createUserViewModel.EmailID = userMaster.EmailID;
            createUserViewModel.KMChoicesVM = choiceList;
            // List of assigned roles to user to display on edit.
            List<long> selectedroles = new List<long>();
            var userInKM = await db.UserInKeyManagerMasters.Where(x => x.UserID == uid).ToListAsync();
            foreach (var i in userInKM)
            {
                selectedroles.Add(i.KeyManagerID);
            }

            //List of Roles
            List<ChoiceViewModel> choiceRoleList = new List<ChoiceViewModel>();
            var role = await db.RoleMasters.Where(x => x.IsActive == true).ToListAsync();
            foreach (var i in role)
            {
                choiceRoleList.Add(new ChoiceViewModel() { SNo = i.ID, Text = i.RoleName });
            }
            createUserViewModel.Password = userMaster.Password;
            createUserViewModel.ID = userMaster.ID;
            createUserViewModel.FirstName = userMaster.FirstName;
            createUserViewModel.LastName = userMaster.LastName;
            createUserViewModel.EmailID = userMaster.EmailID;
            createUserViewModel.ChoicesVM = choiceRoleList;
            // List of assigned roles to user to display on edit.
            List<long> selectedchoiceroles = new List<long>();
            var userInRoles = await db.UserInRoleMasters.Where(x => x.UserID == uid).ToListAsync();
            foreach (var i in userInRoles)
            {
                selectedchoiceroles.Add(i.RoleID);
            }

            //createUserViewModel.SelectedChoices = new List<long>();
            createUserViewModel.SelectedRoleChoices = selectedchoiceroles;
            createUserViewModel.SelectedChoices = selectedroles;
            return PartialView("_EditUserPopup", createUserViewModel);
        }

        // POST: UserMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreateUserViewModel createUserViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CreateUserViewModel collectionVM = new CreateUserViewModel();
                    List<KMChoiceViewModel> choiceList = new List<KMChoiceViewModel>();
                    collectionVM.KMChoicesVM = choiceList;
                    collectionVM.SelectedChoices = createUserViewModel.SelectedChoices;
                    if (createUserViewModel.SelectedChoices != null)
                    {
                        var is_email = await db.UserMasters.AnyAsync(x => x.EmailID == createUserViewModel.EmailID && x.IsActive == true && x.ID != createUserViewModel.ID);
                        if (is_email == true)
                        {
                            TempData["error"] = "Email Already Exists";
                            return RedirectToAction("Index");
                        }
                        var idexist = await db.UserMasters.Where(x => x.ID == createUserViewModel.ID).FirstOrDefaultAsync();
                        if (idexist != null)
                        {

                            idexist.FirstName = createUserViewModel.FirstName;
                            idexist.LastName = "+" + createUserViewModel.countryflag + createUserViewModel.LastName;
                            idexist.EmailID = createUserViewModel.EmailID;
                            idexist.LastModifiedBy = Convert.ToInt32(Session["UserID"]);
                            idexist.LastModifiedOn = DateTime.Now;
                            db.Entry(idexist).State = EntityState.Modified;

                            //for delete old roles
                            var oldKM = await db.UserInKeyManagerMasters.Where(x => x.UserID == createUserViewModel.ID).ToListAsync();
                            foreach (var i in oldKM)
                            {
                                db.UserInKeyManagerMasters.Remove(i);
                            }

                            foreach (int i in createUserViewModel.SelectedChoices)
                            {
                                UserInKeyManagerMaster userInRoleMaster = new UserInKeyManagerMaster
                                {
                                    UserID = createUserViewModel.ID,
                                    KeyManagerID = i,
                                    CreatedBy = Convert.ToInt32(Session["UserID"]),
                                    CreatedOn = DateTime.Now,

                                };
                                db.UserInKeyManagerMasters.Add(userInRoleMaster);
                            }

                            //for delete old roles
                            var oldroles = await db.UserInRoleMasters.Where(x => x.UserID == createUserViewModel.ID).ToListAsync();
                            foreach (var i in oldroles)
                            {
                                db.UserInRoleMasters.Remove(i);
                            }

                            foreach (int i in createUserViewModel.SelectedRoleChoices)
                            {
                                UserInRoleMaster userInRoleMaster = new UserInRoleMaster
                                {
                                    UserID = createUserViewModel.ID,
                                    RoleID = i,
                                    CreatedBy = Convert.ToInt32(Session["UserID"]),
                                    CreatedOn = DateTime.Now,

                                };
                                db.UserInRoleMasters.Add(userInRoleMaster);
                            }

                            try
                            {
                                await db.SaveChangesAsync();
                                TempData["success"] = "User Updated Successfully";
                                return RedirectToAction("Index");
                            }
                            catch
                            {
                                TempData["error"] = "Please Select Atleast One Role";
                                return RedirectToAction("Index");
                            }




                        }



                        TempData["error"] = "User user does not exist";
                        return RedirectToAction("Index");
                    }
                    else
                    {

                        TempData["error"] = "Please Select Atleast One Role";
                        return RedirectToAction("Index");
                    }
                }
                TempData["error"] = "Please Select Atleast One Role";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return RedirectToAction("Index");
        }
        //public ActionResult Edit([Bind(Include = "ID,FirstName,LastName,EmailID,Password,ChangePasswordInterval,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsActive")] UserMaster userMaster)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var isexist = db.UserMasters.Any(x => x.EmailID == userMaster.EmailID && x.ID != userMaster.ID);
        //        if (isexist)
        //        {
        //            ViewData["error"] = "Email Already Exists";


        //            return View(userMaster);

        //        }
        //        userMaster.LastModifiedBy = Convert.ToInt32(Session["UserID"]);
        //        userMaster.LastModifiedOn = DateTime.Now;
        //        db.Entry(userMaster).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(userMaster);
        //}

        // GET: UserMasters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMaster userMaster = db.UserMasters.Find(id);
            if (userMaster == null)
            {
                return HttpNotFound();
            }
            return View(userMaster);
        }

        // POST: UserMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserMaster userMaster = db.UserMasters.Find(id);
            //db.UserMasters.Remove(userMaster);
            userMaster.IsActive = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }  

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        public async Task<JsonResult> UserAlreadyExistsAsync(string EmailID, int ID)
        {
            if (ID == 0)
            {
                var result = await db.UserMasters.AnyAsync(x => x.EmailID == EmailID && x.IsActive == true);
                return Json(!result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = await db.UserMasters.AnyAsync(x => x.EmailID == EmailID && x.IsActive == true && x.ID != ID);
                return Json(!result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult EnableOrDisableUser(string UserID, int Flag)
        {
            ResponseResult responseResult = new ResponseResult();
            try
            {

                string[] UserID1 = UserID.Trim().Split('_');
                int uid = Convert.ToInt32(UserID1[1]);
                var userMaster = db.UserMasters.Where(x => x.ID == uid).FirstOrDefault();

                if (userMaster != null)
                {
                    userMaster.IsActive = Convert.ToBoolean(Flag);
                    userMaster.LastModifiedBy = Convert.ToInt32(Session["UserID"]);
                    userMaster.LastModifiedOn = DateTime.Now;
                    db.Entry(userMaster).State = EntityState.Modified;
                    db.SaveChanges();


                    return Json(responseResult.GetResponse(true, userMaster.FirstName, string.Format("User {0} Successfully", Flag == 1 ? "Enable" : "Disable")));
                }
                else
                {
                    return Json(responseResult.GetResponse(false, UserID, "Invalid User"));
                }
            }
            catch (Exception ex)
            {
                return Json(responseResult.GetResponse(false, UserID, ex.Message));
            }

            //return PartialView("_Notification");
        }
        [HttpPost]
        public ActionResult GeneratePassword(string UserID, string NewPassword)
        {
            ResponseResult responseResult = new ResponseResult();
            try
            {

                string[] UserID1 = UserID.Trim().Split('_');
                int uid = Convert.ToInt32(UserID1[1]);
                var userMaster = db.UserMasters.Where(x => x.ID == uid).FirstOrDefault();
                if (userMaster != null)
                {
                    CommonMethod cm = new CommonMethod();
                    NewPassword = cm.EncryptData(NewPassword, Key);
                    var lastpassword = (from d in db.UserPasswordHistories where d.UserID == uid orderby d.CreatedOn descending select new { Password = d.Password }).Take(2);
                    foreach (var i in lastpassword)
                    {
                        if (i.Password == NewPassword)
                        {
                            return Json(responseResult.GetResponse(false, UserID, "Password must not be from last two passwords!"));
                        }
                    }
                    userMaster.Password = NewPassword;
                    userMaster.LastModifiedBy = Convert.ToInt32(Session["UserID"]);
                    userMaster.LastModifiedOn = DateTime.Now;
                    db.Entry(userMaster).State = EntityState.Modified;
                    db.SaveChanges();
                    UserPasswordHistory userPasswordHistory = new UserPasswordHistory()
                    {
                        UserID = uid,
                        CreatedBy = Convert.ToInt32(Session["UserID"]),
                        Password = NewPassword,
                        CreatedOn = DateTime.Now,
                    };
                    db.UserPasswordHistories.Add(userPasswordHistory);
                    db.SaveChanges();
                    return Json(responseResult.GetResponse(true, userMaster.FirstName, "Password Changed Successfully"));
                }
                else
                {
                    return Json(responseResult.GetResponse(false, UserID, "Invalid User"));
                }
            }
            catch (Exception ex)
            {
                return Json(responseResult.GetResponse(false, UserID, ex.Message));
            }
        }
    }
}