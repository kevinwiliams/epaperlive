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

namespace ePaperLive.Views.Admin.Coupon
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/Coupons")]
    [Route("action = index")]
    public class CouponsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Coupons
        [Route]
        public async Task<ActionResult> Index()
        {
            return View(await db.coupons.AsNoTracking().ToListAsync());
        }
        // generate coupon
        [Route("generatecoupon")]
        public JsonResult GenerateCoupon() {
            var resultData = new Dictionary<string, object>();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var couponCode = new string(
                Enumerable.Repeat(chars, 13)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            resultData["data"] = couponCode;

            return Json(resultData);
        }

        // GET: Coupons/Details/5
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupons coupons = await db.coupons.FindAsync(id);
            if (coupons == null)
            {
                return HttpNotFound();
            }
            return View(coupons);
        }

        // GET: Coupons/Create
        [Route("create")]
        public ActionResult Create()
        {
            var values = new List<int> { 7, 30, 180, 360 };
            var items = values.Select(v => new SelectListItem
            {
                Value = v.ToString(),
                Text = v.ToString()
            }).ToList();

            ViewBag.SubDaysList = items;
            return View();
        }

        // POST: Coupons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CouponID,CouponCode,SubDays,ExpiryDate")] Coupons coupons)
        {
            if (ModelState.IsValid)
            {
                coupons.CreatedAt = DateTime.Now;
                db.coupons.Add(coupons);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(coupons);
        }

        // GET: Coupons/Edit/5
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupons coupons = await db.coupons.FindAsync(id);
            if (coupons == null)
            {
                return HttpNotFound();
            }
            return View(coupons);
        }

        // POST: Coupons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CouponID,CouponCode,SubDays,ExpiryDate,UsedDate,CreatedAt")] Coupons coupons)
        {
            if (ModelState.IsValid)
            {
                db.Entry(coupons).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(coupons);
        }

        // GET: Coupons/Delete/5
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coupons coupons = await db.coupons.FindAsync(id);
            if (coupons == null)
            {
                return HttpNotFound();
            }
            return View(coupons);
        }

        // POST: Coupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Coupons coupons = await db.coupons.FindAsync(id);
            db.coupons.Remove(coupons);
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
