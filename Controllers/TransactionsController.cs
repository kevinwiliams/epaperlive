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
    [RoutePrefix("Admin/Transactions")]
    [Route("action = index")]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        [Route]
        public async Task<ActionResult> Index()
        {
            var subscriber_tranx = db.subscriber_tranx.Include(s => s.Subscriber);
            return View(await subscriber_tranx.AsNoTracking().ToListAsync());
        }

        // GET: Transactions/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Tranx subscriber_Tranx = await db.subscriber_tranx.FindAsync(id);
            if (subscriber_Tranx == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Tranx);
        }

        // GET: Transactions/Create
        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Subscriber_TranxID,SubscriberID,EmailAddress,CardOwner,CardType,CardExp,CardLastFour,PromoCode,TranxAmount,TranxDate,RateID,TranxType,OrderID,TranxNotes,IpAddress,IsMadeLiveSuccessful,EnrolledIn3DSecure,AuthCode,ConfirmationNo")] Subscriber_Tranx subscriber_Tranx)
        {
            if (ModelState.IsValid)
            {
                db.subscriber_tranx.Add(subscriber_Tranx);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Tranx.SubscriberID);
            return View(subscriber_Tranx);
        }

        // GET: Transactions/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Tranx subscriber_Tranx = await db.subscriber_tranx.FindAsync(id);
            if (subscriber_Tranx == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Tranx.SubscriberID);
            return View(subscriber_Tranx);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Subscriber_TranxID,SubscriberID,EmailAddress,CardOwner,CardType,CardExp,CardLastFour,PromoCode,TranxAmount,TranxDate,RateID,TranxType,OrderID,TranxNotes,IpAddress,IsMadeLiveSuccessful,EnrolledIn3DSecure,AuthCode,ConfirmationNo")] Subscriber_Tranx subscriber_Tranx)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber_Tranx).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Tranx.SubscriberID);
            return View(subscriber_Tranx);
        }

        // GET: Transactions/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Tranx subscriber_Tranx = await db.subscriber_tranx.FindAsync(id);
            if (subscriber_Tranx == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Tranx);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Subscriber_Tranx subscriber_Tranx = await db.subscriber_tranx.FindAsync(id);
            db.subscriber_tranx.Remove(subscriber_Tranx);
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
