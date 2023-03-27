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
using System.Linq.Expressions;
using System.Data.SqlClient;


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
            //var subscriber_tranx = db.subscriber_tranx.Include(s => s.Subscriber);
            //return View(await subscriber_tranx.AsNoTracking().ToListAsync());
            await Task.FromResult(0);
            return View();
        }

        [HttpPost]
        [Route]
        public async Task<ActionResult> Index(DataTableParameters dataTableParameters)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var subscriber_tranx = db.subscriber_tranx.AsQueryable();
            var searchTerm = dataTableParameters.search?.value;
            var filteredData = subscriber_tranx;
            try
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredData = filteredData.Where(x => x.EmailAddress.Contains(searchTerm) || x.PlanDesc.Contains(searchTerm) || x.OrderID.Contains(searchTerm));
                }

                // Order the data by the specified column and direction
                if (!string.IsNullOrEmpty(dataTableParameters.order?.FirstOrDefault()?.column.ToString()))
                {
                    var sortColumnIndex = int.Parse(dataTableParameters.order.FirstOrDefault().column.ToString());
                    var sortColumn = dataTableParameters.columns[sortColumnIndex].data;
                    var sortDirection = dataTableParameters.order.FirstOrDefault().dir == "desc" ? "OrderByDescending" : "OrderBy";
                    var property = typeof(Subscriber_Tranx).GetProperty(sortColumn);
                    var parameter = Expression.Parameter(typeof(Subscriber_Tranx), "p");
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    var resultExp = Expression.Call(typeof(Queryable), sortDirection, new Type[] { typeof(Subscriber_Tranx), property.PropertyType }, filteredData.Expression, Expression.Quote(orderByExp));
                    filteredData = filteredData.Provider.CreateQuery<Subscriber_Tranx>(resultExp);
                }

                filteredData = filteredData
                    //.OrderBy(x => x.EmailAddress)
                    .Skip(dataTableParameters.start)
                    .Take(dataTableParameters.length == 0 ? 25 : dataTableParameters.length);

                var filteredDataList = await filteredData.ToListAsync();

                return Json(new
                {
                    draw = dataTableParameters.draw,
                    recordsTotal = subscriber_tranx.Count(),
                    recordsFiltered = filteredData.Count(),
                    data = filteredDataList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

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
