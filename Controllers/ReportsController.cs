using ePaperLive.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/Reports")]
    [Route("action = index")]
    public class ReportsController : Controller
    {
        // GET: Reports
        [Route]
        public ActionResult Index()
        {
            return View();
        }

        [Route("signups")]
        public async Task<ActionResult> SignUps()
        {
            using (var context = new ApplicationDbContext())
            {
                var result = await context.subscriber_tranx
                   .GroupBy(t => DbFunctions.TruncateTime(t.TranxDate))
                   .Select(g => new SignUpsList
                   {
                       SignUpDate = g.Key ?? DateTime.Now,
                       Total = g.Count()
                   })
                   .ToListAsync();
                return View(result);
            }
        }


        [HttpPost]
        [Route("signups")]
        public async Task<ActionResult> SignUps(string orderNumber, DateTime startDate, DateTime endDate)
        {
            using (var context = new ApplicationDbContext())
            {
                var endDatePlusOneDay = endDate.AddDays(1);
                var result = await context.subscriber_tranx
                    .Where(t => t.TranxDate >= startDate && t.TranxDate < endDatePlusOneDay && t.OrderID.Contains(orderNumber))
                    .GroupBy(t => DbFunctions.TruncateTime(t.TranxDate))
                                .Select(g => new SignUpsList
                                {
                                    SignUpDate = g.Key ?? DateTime.Now,
                                    Total = g.Count()
                                })
                                .ToListAsync();

                return View(result);
            }
        }
    }
}