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
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin,Circulation")]
    [RoutePrefix("Admin/PrintSub")]
    [Route("action = index")]
    public class PrintSubController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PrintSub
        [Route]
        public async Task<ActionResult> Index()
        {
            //var subscriber_print = db.subscriber_print
            //                        .Include(s => s.Subscriber)
            //                        .Where(x => x.IsActive == true);
            //return View(await subscriber_print.ToListAsync());
            using (var context = new ApplicationDbContext())
            {
                var sql = @"
                    SELECT s.SubscriberID, s.EmailAddress, s.FirstName, s.LastName, sp.OrderNumber, sp.Subscriber_PrintID as AddressID,
                    sa. AddressLine1, sa.AddressLine2, sa.CityTown, sa.StateParish , anu.PhoneNumber, sp.StartDate, sp.EndDate, sp.IsActive, sp.Circprosubid
                    FROM Subscribers s with(nolock)
                    JOIN AspNetUsers anu ON s.SubscriberID = anu.Id 
                    JOIN Subscriber_Print sp ON s.SubscriberID = sp.SubscriberID 
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = sp.AddressID";

                var result = await context.Database.SqlQuery<PrintSubscribers>(sql).ToListAsync();
                return View(result);
            }
        }

        // GET: PrintSub/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sql = @"
                    SELECT s.SubscriberID, s.EmailAddress, s.FirstName, s.LastName, sp.OrderNumber, sp.Subscriber_PrintID as AddressID,
                    sa. AddressLine1, sa.AddressLine2, sa.CityTown, sa.StateParish , anu.PhoneNumber, sp.StartDate, sp.EndDate, sp.IsActive, sp.Circprosubid
                    FROM Subscribers s with(nolock)
                    JOIN AspNetUsers anu ON s.SubscriberID = anu.Id 
                    JOIN Subscriber_Print sp ON s.SubscriberID = sp.SubscriberID 
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = sp.AddressID
                    WHERE sp.Subscriber_PrintID = @Id";
            var idParam = new SqlParameter("Id", id);

            PrintSubscribers result = await db.Database.SqlQuery<PrintSubscribers>(sql, idParam).FirstOrDefaultAsync();
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
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
            var sql = @"
                    SELECT s.SubscriberID, s.EmailAddress, s.FirstName, s.LastName, sp.OrderNumber, sp.Subscriber_PrintID as AddressID,
                    sa. AddressLine1, sa.AddressLine2, sa.CityTown, sa.StateParish , anu.PhoneNumber, sp.StartDate, sp.EndDate, sp.IsActive, sp.Circprosubid
                    FROM Subscribers s with(nolock)
                    JOIN AspNetUsers anu ON s.SubscriberID = anu.Id 
                    JOIN Subscriber_Print sp ON s.SubscriberID = sp.SubscriberID 
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = sp.AddressID
                    WHERE sp.Subscriber_PrintID = @Id";
            var idParam = new SqlParameter("Id", id);

            PrintSubscribers result = await db.Database.SqlQuery<PrintSubscribers>(sql, idParam).FirstOrDefaultAsync();
           
                return View(result);
        }

        // POST: PrintSub/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PrintSubscribers printSubscribers)
        {
            var _actLog = new ActivityLog();
            _actLog.SubscriberID = User.Identity.GetUserId();
            _actLog.EmailAddress = printSubscribers.EmailAddress;
            _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Circulation");

            if (ModelState.IsValid)
            {
                var sql = @"
                            UPDATE sp 
                            SET sp.Circprosubid = @circProID
                            FROM Subscribers s with(nolock)
                            JOIN Subscriber_Print sp ON s.SubscriberID = sp.SubscriberID
                            LEFT JOIN Subscriber_Address sa ON sa.AddressID = sp.AddressID
                            WHERE sp.Subscriber_PrintID = @Id";

                var newCircProID = new SqlParameter("@circProID", printSubscribers.Circprosubid);
                var Id = new SqlParameter("@Id", printSubscribers.AddressID);

                await db.Database.ExecuteSqlCommandAsync(sql, new[] { newCircProID, Id });
                
                //log
                _actLog.LogInformation = "Updated CircPro ID (" + User.Identity.Name + ")";
                Util.LogUserActivity(_actLog);
                return RedirectToAction("Index");
            }
            return View(printSubscribers);
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
