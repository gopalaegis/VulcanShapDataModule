using DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Valcan.Models;

namespace Valcan.Controllers
{
    public class KeyManagerMasterController : Controller
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(KeyManagerMasterController));  //Declaring Log4Net
        private admin_vulcan2devEntities db = new admin_vulcan2devEntities();

        // GET: ReasonManagementMasters
        public async Task<ActionResult> Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View(await db.KeyManagerMasters.Where(x => x.IsActive == true).ToListAsync());
        }


        // GET: ReasonManagementMasters/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KeyManagerMaster reasonManagementMaster = await db.KeyManagerMasters.FindAsync(id);
            if (reasonManagementMaster == null)
            {
                return HttpNotFound();
            }
            return View(reasonManagementMaster);
        }

        // GET: ReasonManagementMasters/Create
        public ActionResult Create()
        {
            KeyManagerViewModel rm = new KeyManagerViewModel
            {
                ID = 0
            };
            return View(rm);
        }

        // POST: ReasonManagementMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,KeyManager,KeyManager_Name,IsActive")] KeyManagerViewModel reasonManagementMasterVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    KeyManagerMaster reasonManagement = new KeyManagerMaster
                    {
                        KeyManager = reasonManagementMasterVM.KeyManager,
                        KeyManager_Name = reasonManagementMasterVM.KeyManager_Name,
                        IsActive = true,
                        CreatedBy = Convert.ToInt32(Session["UserID"]),
                        CreatedOn = DateTime.Now,
                        LastModifiedBy = Convert.ToInt32(Session["UserID"]),
                        LastModifiedOn = DateTime.Now



                    };

                    db.KeyManagerMasters.Add(reasonManagement);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(reasonManagementMasterVM);
        }

        // GET: ReasonManagementMasters/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KeyManagerMaster reasonManagementMaster = await db.KeyManagerMasters.FindAsync(id);
            KeyManagerViewModel reasonManagementViewModel = new KeyManagerViewModel
            {
                ID = reasonManagementMaster.ID,
                KeyManager = reasonManagementMaster.KeyManager,
                KeyManager_Name = reasonManagementMaster.KeyManager_Name,
                IsActive = reasonManagementMaster.IsActive





            };
            if (reasonManagementMaster == null)
            {
                return HttpNotFound();
            }
            return View(reasonManagementViewModel);
        }

        // POST: ReasonManagementMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,KeyManager,KeyManager_Name,IsActive")] KeyManagerViewModel reasonManagementVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    KeyManagerMaster reasonManagementMaster = await db.KeyManagerMasters.FindAsync(reasonManagementVM.ID);
                    reasonManagementMaster.KeyManager = reasonManagementVM.KeyManager;
                    reasonManagementMaster.KeyManager_Name = reasonManagementVM.KeyManager_Name;
                    reasonManagementMaster.LastModifiedOn = DateTime.Now;
                    reasonManagementMaster.LastModifiedBy = Convert.ToInt32(Session["UserID"]);
                    db.Entry(reasonManagementMaster).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(reasonManagementVM);
        }

        // GET: ReasonManagementMasters/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            KeyManagerViewModel reasonManagementViewModel = new KeyManagerViewModel();
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                KeyManagerMaster reasonManagementMaster = await db.KeyManagerMasters.FindAsync(id);
                if (reasonManagementMaster == null)
                {
                    return HttpNotFound();
                }


                reasonManagementViewModel.ID = reasonManagementMaster.ID;
                reasonManagementViewModel.KeyManager = reasonManagementMaster.KeyManager;
                reasonManagementViewModel.KeyManager_Name = reasonManagementMaster.KeyManager_Name;
                reasonManagementViewModel.IsActive = reasonManagementMaster.IsActive;
                reasonManagementViewModel.msg = "";
                var counttemp = db.UserInKeyManagerMasters.Where(r => r.KeyManagerID == id).AsQueryable();

                var count = counttemp.Count();

                if (count > 0)
                {
                    reasonManagementViewModel.msg = "This record is already mapped with Download Documemt Logs, You can not delete it.";

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View(reasonManagementViewModel);
        }

        // POST: ReasonManagementMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            KeyManagerMaster reasonManagementMaster = await db.KeyManagerMasters.FindAsync(id);
            reasonManagementMaster.IsActive = false;
            db.Entry(reasonManagementMaster).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");

        }
        [AllowAnonymous]
        public async Task<JsonResult> KeyManagerAlreadyExists(int ID, string KeyManager)
        {
            var result = await db.KeyManagerMasters.AnyAsync(x => x.KeyManager.ToUpper() == KeyManager.ToUpper() && x.ID != ID && x.IsActive == true);
            return Json(!result, JsonRequestBehavior.AllowGet);

            //else
            //{
            //    var result = await db.ReasonManagementMasters.AnyAsync(x => x.Reason.ToUpper() == Reason.ToUpper() && x.ID != ID && x.IsActive == true);
            //    return Json(!result, JsonRequestBehavior.AllowGet);
            //}

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}