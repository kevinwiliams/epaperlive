using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePaperLive.DBModel;
using ePaperLive.Models;

namespace ePaperLive.Views.Admin.Rates
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/Rates")]
    [Route("action = index")]
    public class RatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Rates
        [Route]
        public ActionResult Index()
        {
            return View(db.printandsubrates.ToList());
        }

        // GET: Rates/Details/5
        [Route("details/{id:int}")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            printandsubrate printandsubrate = db.printandsubrates.Find(id);
            if (printandsubrate == null)
            {
                return HttpNotFound();
            }
            return View(printandsubrate);
        }

        // GET: Rates/Create
        [Route("create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Rates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Rateid,Market,Type,RateDescr,PrintDayPattern,PrintTerm,PrintTermUnit,EDayPattern,ETerm,ETermUnit,Curr,Rate,SortOrder,Active,IntroRate,OfferIntroRate,IsFeatured,IsCTA,FeatureList,BestDealFlag,IsRenewalRate")] printandsubrate printandsubrate)
        {
            if (ModelState.IsValid)
            {
                db.printandsubrates.Add(printandsubrate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printandsubrate);
        }

        // GET: Rates/Edit/5
        [Route("edit/{id:int}")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            printandsubrate printandsubrate = db.printandsubrates.Find(id);
            if (printandsubrate == null)
            {
                return HttpNotFound();
            }
            return View(printandsubrate);
        }

        // POST: Rates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Rateid,Market,Type,RateDescr,PrintDayPattern,PrintTerm,PrintTermUnit,EDayPattern,ETerm,ETermUnit,Curr,Rate,SortOrder,Active,IntroRate,OfferIntroRate,IsFeatured,IsCTA,FeatureList,BestDealFlag,IsRenewalRate")] printandsubrate printandsubrate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printandsubrate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printandsubrate);
        }

        // GET: Rates/Delete/5
        [Route("delete/{id:int}")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            printandsubrate printandsubrate = db.printandsubrates.Find(id);
            if (printandsubrate == null)
            {
                return HttpNotFound();
            }
            return View(printandsubrate);
        }

        // POST: Rates/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            printandsubrate printandsubrate = db.printandsubrates.Find(id);
            db.printandsubrates.Remove(printandsubrate);
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
    }
}
