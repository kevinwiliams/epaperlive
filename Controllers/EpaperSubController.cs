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

namespace ePaperLive.Controllers.Admin.EpaperSub
{
    [Authorize]
    [RoutePrefix("Admin/EpaperSub")]
    [Route("action = index")]
    public class EpaperSubController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EpaperSub
        [Route]
        public async Task<ActionResult> Index()
        {
            var subscriber_epaper = db.subscriber_epaper.Include(s => s.Subscriber);
            return View(await subscriber_epaper.ToListAsync());
        }

        // GET: EpaperSub/Details/5
        [Route("details/{id}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Epaper subscriber_Epaper = await db.subscriber_epaper.FindAsync(id);
            if (subscriber_Epaper == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Epaper);
        }

        // GET: EpaperSub/Create
        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress");
            return View();
        }

        // POST: EpaperSub/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Subscriber_EpaperID,SubscriberID,EmailAddress,RateID,StartDate,EndDate,IsActive,SubType,CreatedAt,NotificationEmail,PlanDesc,OrderNumber")] Subscriber_Epaper subscriber_Epaper)
        {
            if (ModelState.IsValid)
            {
                db.subscriber_epaper.Add(subscriber_Epaper);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Epaper.SubscriberID);
            return View(subscriber_Epaper);
        }

        // GET: EpaperSub/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Epaper subscriber_Epaper = await db.subscriber_epaper.FindAsync(id);
            if (subscriber_Epaper == null)
            {
                return HttpNotFound();
            }
            ViewBag.daysList = GetDaysList();
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Epaper.SubscriberID);
            return View(subscriber_Epaper);
        }

        // POST: EpaperSub/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Subscriber_EpaperID,SubscriberID,EmailAddress,RateID,StartDate,EndDate,IsActive,SubType,CreatedAt,NotificationEmail,PlanDesc,OrderNumber")] Subscriber_Epaper subscriber_Epaper)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber_Epaper).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SubscriberID = new SelectList(db.subscribers, "SubscriberID", "EmailAddress", subscriber_Epaper.SubscriberID);
            return View(subscriber_Epaper);
        }

        // GET: EpaperSub/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber_Epaper subscriber_Epaper = await db.subscriber_epaper.FindAsync(id);
            if (subscriber_Epaper == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_Epaper);
        }

        // POST: EpaperSub/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Subscriber_Epaper subscriber_Epaper = await db.subscriber_epaper.FindAsync(id);
            db.subscriber_epaper.Remove(subscriber_Epaper);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public static List<SelectListItem> GetDaysList()
        {
            List<SelectListItem> daysList = new List<SelectListItem>();

            daysList.Add(new SelectListItem { Text = "7 days", Value = "7" });
            daysList.Add(new SelectListItem { Text = "14 days", Value = "14" });
            daysList.Add(new SelectListItem { Text = "30 days", Value = "30" });
            daysList.Add(new SelectListItem { Text = "60 days", Value = "60" });
            daysList.Add(new SelectListItem { Text = "90 days", Value = "90" });
            daysList.Add(new SelectListItem { Text = "180 days", Value = "180" });
            daysList.Add(new SelectListItem { Text = "360 days", Value = "360" });

            return daysList;
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
