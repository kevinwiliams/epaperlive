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
    [RoutePrefix("Admin/SchoolGovRates")]
    [Route("action = index")]
    public class SchoolGovRatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SchoolGovRates
        [Route]
        public async Task<ActionResult> Index()
        {
            return View(await db.school_govt_rates.ToListAsync());
        }

        // GET: SchoolGovRates/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            school_govt_rates school_govt_rates = await db.school_govt_rates.FindAsync(id);
            if (school_govt_rates == null)
            {
                return HttpNotFound();
            }
            return View(school_govt_rates);
        }

        // GET: SchoolGovRates/Create
        [Route("create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: SchoolGovRates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SchGovtID,ParentRateID,Domains,Category,RateDescr,Curr,Rate,Term,Units,UpdatedAt,Active")] school_govt_rates school_govt_rates)
        {
            if (ModelState.IsValid)
            {
                db.school_govt_rates.Add(school_govt_rates);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(school_govt_rates);
        }

        // GET: SchoolGovRates/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            school_govt_rates school_govt_rates = await db.school_govt_rates.FindAsync(id);
            if (school_govt_rates == null)
            {
                return HttpNotFound();
            }
            return View(school_govt_rates);
        }

        // POST: SchoolGovRates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SchGovtID,ParentRateID,Domains,Category,RateDescr,Curr,Rate,Term,Units,UpdatedAt,Active")] school_govt_rates school_govt_rates)
        {
            if (ModelState.IsValid)
            {
                db.Entry(school_govt_rates).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(school_govt_rates);
        }

        // GET: SchoolGovRates/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            school_govt_rates school_govt_rates = await db.school_govt_rates.FindAsync(id);
            if (school_govt_rates == null)
            {
                return HttpNotFound();
            }
            return View(school_govt_rates);
        }

        // POST: SchoolGovRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            school_govt_rates school_govt_rates = await db.school_govt_rates.FindAsync(id);
            db.school_govt_rates.Remove(school_govt_rates);
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
