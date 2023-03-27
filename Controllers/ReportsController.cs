using ePaperLive.Models;
using System;
using System.Collections.Generic;
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
                var sql = @"
                    SELECT LEFT([TranxDate], 11) as SignUpDate, COUNT(*) Total FROM [dbo].[Subscriber_Tranx]  
                    GROUP BY LEFT([TranxDate], 11)";

                var result = await context.Database.SqlQuery<SignUpsList>(sql).ToListAsync();
                return View(result);
            }
        }


        [HttpPost]
        [Route("signups")]
        public async Task<ActionResult> SignUps(string orderNumber, DateTime startDate, DateTime endDate)
        {
            using (var context = new ApplicationDbContext())
            {
                var sql = @"
                    SELECT LEFT([TranxDate], 11) as SignUpDate, COUNT(*) Total FROM [dbo].[Subscriber_Tranx] 
                    WHERE [TranxDate] >= @startDate AND [TranxDate] <= @endDate 
                    AND ([OrderID] like '%' + @orderNumber + '%') 
                    GROUP BY LEFT([TranxDate], 11)";

                var sDate = new SqlParameter("startDate", startDate);
                var eDate = new SqlParameter("endDate", endDate);
                var orderNum = new SqlParameter("orderNumber", orderNumber);

                var result = await context.Database.SqlQuery<SignUpsList>(sql, sDate, eDate, orderNum).ToListAsync();
                return View(result);
            }
        }
    }
}