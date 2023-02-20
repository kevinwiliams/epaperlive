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

namespace ePaperLive.Views.Admin.PrintSub
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/PrintSub")]
    [Route("action = index")]
    public class PrintSubController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PrintSub
        [Route]
        public async Task<ActionResult> Index()
        {
            var subscriber_print = db.subscriber_print
                                    .Include(s => s.Subscriber)
                                    .Where(x => x.IsActive == true);
            return View(await subscriber_print.ToListAsync());
        }

        // GET: PrintSub/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Print subscriber_Print = await db.subscriber_print.FindAsync(id);
            if (subscriber_Print == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Print);
        }

        // GET: PrintSub/Create
        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress");
            return View();
        }

        // POST: PrintSub/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Subscriber_PrintID,SubscriberID,EmailAddress,RateID,AddressID,StartDate,EndDate,IsActive,DeliveryInstructions,Circprosubid,CreatedAt")] Subscriber_Print subscriber_Print)
        {
            if (ModelState.IsValid)
            {
                db.subscriber_print.Add(subscriber_Print);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Print.SubscriberID);
            return View(subscriber_Print);
        }

        // GET: PrintSub/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Print subscriber_Print = await db.subscriber_print.FindAsync(id);
            if (subscriber_Print == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Print.SubscriberID);
            Subscriber_Address subadd = await db.subscriber_address.FirstOrDefaultAsync(x => x.AddressID == subscriber_Print.AddressID);
            ViewBag.AddressDetails = subadd;
            return View(subscriber_Print);
        }

        // POST: PrintSub/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Subscriber_PrintID,SubscriberID,EmailAddress,RateID,AddressID,StartDate,EndDate,IsActive,DeliveryInstructions,Circprosubid,CreatedAt")] Subscriber_Print subscriber_Print)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber_Print).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Print.SubscriberID);
            return View(subscriber_Print);
        }

        // GET: PrintSub/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Print subscriber_Print = await db.subscriber_print.FindAsync(id);
            if (subscriber_Print == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Print);
        }

        // POST: PrintSub/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Subscriber_Print subscriber_Print = await db.subscriber_print.FindAsync(id);
            db.subscriber_print.Remove(subscriber_Print);
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
