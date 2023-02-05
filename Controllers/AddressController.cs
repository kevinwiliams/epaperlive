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

namespace ePaperLive.Views.Admin.Address
{
    [Authorize]
    [RoutePrefix("Admin/Address")]
    [Route("action = index")]
    public class AddressController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Address
        [Route]
        public async Task<ActionResult> Index()
        {
            var subscriber_address = db.subscriber_address.Include(s => s.Subscriber);
            return View(await subscriber_address.ToListAsync());
        }

        // GET: Address/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Address subscriber_Address = await db.subscriber_address.FindAsync(id);
            if (subscriber_Address == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Address);
        }

        // GET: Address/Create
        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress");
            return View();
        }

        // POST: Address/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "AddressID,SubscriberID,AddressType,EmailAddress,AddressLine1,AddressLine2,CityTown,StateParish,ZipCode,CountryCode,CreatedAt")] Subscriber_Address subscriber_Address)
        {
            if (ModelState.IsValid)
            {
                db.subscriber_address.Add(subscriber_Address);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Address.SubscriberID);
            return View(subscriber_Address);
        }

        // GET: Address/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Address subscriber_Address = await db.subscriber_address.FindAsync(id);
            if (subscriber_Address == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Address.SubscriberID);
            return View(subscriber_Address);
        }

        // POST: Address/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AddressID,SubscriberID,AddressType,EmailAddress,AddressLine1,AddressLine2,CityTown,StateParish,ZipCode,CountryCode,CreatedAt")] Subscriber_Address subscriber_Address)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber_Address).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Address.SubscriberID);
            return View(subscriber_Address);
        }

        // GET: Address/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Address subscriber_Address = await db.subscriber_address.FindAsync(id);
            if (subscriber_Address == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Address);
        }

        // POST: Address/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Subscriber_Address subscriber_Address = await db.subscriber_address.FindAsync(id);
            db.subscriber_address.Remove(subscriber_Address);
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
