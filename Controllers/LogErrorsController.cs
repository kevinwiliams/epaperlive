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

namespace ePaperLive.Views
{
    [Authorize (Roles ="Admin")]
    [RoutePrefix("Admin/Logs")]
    [Route("action = index")]
    public class LogErrorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: LogErrors
        [Route]
        public async Task<ActionResult> Index()
        {
            return View(await db.log_errors.AsNoTracking().ToListAsync());
        }

        // GET: LogErrors/Details/5
        [Route("details/{id}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log_errors log_errors = await db.log_errors.FindAsync(id);
            if (log_errors == null)
            {
                return HttpNotFound();
            }
            return View(log_errors);
        }

        // GET: LogErrors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogErrors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "errID,err_msg,err_date,err_time,err_name,stacktrace")] log_errors log_errors)
        {
            if (ModelState.IsValid)
            {
                db.log_errors.Add(log_errors);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(log_errors);
        }

        // GET: LogErrors/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log_errors log_errors = await db.log_errors.FindAsync(id);
            if (log_errors == null)
            {
                return HttpNotFound();
            }
            return View(log_errors);
        }

        // POST: LogErrors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "errID,err_msg,err_date,err_time,err_name,stacktrace")] log_errors log_errors)
        {
            if (ModelState.IsValid)
            {
                db.Entry(log_errors).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(log_errors);
        }

        // GET: LogErrors/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log_errors log_errors = await db.log_errors.FindAsync(id);
            if (log_errors == null)
            {
                return HttpNotFound();
            }
            return View(log_errors);
        }

        // POST: LogErrors/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            log_errors log_errors = await db.log_errors.FindAsync(id);
            db.log_errors.Remove(log_errors);
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
