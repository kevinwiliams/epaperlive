using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePaperLive.DBModel;
using ePaperLive.Models;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/UserActivityLog")]
    [Route("action = index")]
    public class UALController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: UAL
        [Route]
        public async Task<ActionResult> Index()
        {
            return View(await db.User_Activity_Logs.ToListAsync());
        }

        // GET: UAL/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user_activity_log user_activity_log = await db.User_Activity_Logs.FindAsync(id);
            if (user_activity_log == null)
            {
                return HttpNotFound();
            }
            return View(user_activity_log);
        }

        // GET: UAL/Create
        [Route("create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: UAL/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LogID,SubscriberID,Role,EmailAddress,IPAddress,LogInformation,SystemInformation,CreatedAt")] user_activity_log user_activity_log)
        {
            if (ModelState.IsValid)
            {
                db.User_Activity_Logs.Add(user_activity_log);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(user_activity_log);
        }

        // GET: UAL/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user_activity_log user_activity_log = await db.User_Activity_Logs.FindAsync(id);
            if (user_activity_log == null)
            {
                return HttpNotFound();
            }
            return View(user_activity_log);
        }

        // POST: UAL/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "LogID,SubscriberID,Role,EmailAddress,IPAddress,LogInformation,SystemInformation,CreatedAt")] user_activity_log user_activity_log)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user_activity_log).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(user_activity_log);
        }

        // GET: UAL/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user_activity_log user_activity_log = await db.User_Activity_Logs.FindAsync(id);
            if (user_activity_log == null)
            {
                return HttpNotFound();
            }
            return View(user_activity_log);
        }

        // POST: UAL/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            user_activity_log user_activity_log = await db.User_Activity_Logs.FindAsync(id);
            db.User_Activity_Logs.Remove(user_activity_log);
            await db.SaveChangesAsync();
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
    }
}
