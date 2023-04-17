using ePaperLive.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

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

        [Route("locations")]
        public async Task<ActionResult> Locations()
        {
            var ipApiProKey = ConfigurationManager.AppSettings["ipApiProKey"];
            var ip2LocationKey = ConfigurationManager.AppSettings["ip2LocationKey"];

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var result = await (from t in context.subscriber_tranx
                                        join u in context.Users on t.SubscriberID equals u.Id into uj
                                        from u in uj.DefaultIfEmpty()
                                        join s in context.subscribers on t.SubscriberID equals s.SubscriberID into sj
                                        from s in sj.DefaultIfEmpty()
                                        where t.IpAddress != null
                                        orderby t.TranxDate descending
                                        select new LocationReport
                                        {
                                            FirstName = s.FirstName,
                                            LastName = s.LastName,
                                            EmailAddress = s.EmailAddress,
                                            PhoneNumber = u.PhoneNumber,
                                            PlanDesc = t.PlanDesc,
                                            Region = "",
                                            Country = "",
                                            TransactionDate = t.TranxDate,
                                            IpAddress = t.IpAddress,
                                            OrderNumber = t.OrderID
                                        }).Take(25).OrderByDescending(x => x.TransactionDate).ToListAsync();

                    foreach (var location in result)
                    {
                        if (Util.IsLocaIP(location.IpAddress))
                        {
                            location.Country = "JM";
                        }
                        else
                        {
                            string apiUrl = String.Format("https://pro.ip-api.com/json/{0}?key={1}", location.IpAddress, ipApiProKey);
                            using (WebClient client = new WebClient())
                            {
                                string json = client.DownloadString(apiUrl);

                                dynamic jsonResult = new JavaScriptSerializer().Deserialize<dynamic>(json);
                                if (jsonResult["status"] == "success")
                                {
                                    location.Region = jsonResult["regionName"];
                                    location.Country = jsonResult["country"];
                                }
                                else
                                {
                                    try
                                    {
                                        string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", location.IpAddress, ip2LocationKey); //
                                        using (WebClient webClient = new WebClient())
                                        {
                                            string jsonRes = webClient.DownloadString(url);
                                            var userlocation = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);

                                            location.Region = userlocation.Region_Name;
                                            location.Country = userlocation.Country_Name;

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.LogError(ex);
                                        location.Region = "NF";
                                        location.Country = "NF";
                                    }
                                }

                            }
                        }

                           
                    }

                    return View(result);
                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return View("locations");
            }
            
        }

        [HttpPost]
        [Route("locations")]
        public async Task<ActionResult> Locations(DateTime startDate, DateTime endDate)
        {
            var ipApiProKey = ConfigurationManager.AppSettings["ipApiProKey"];
            var ip2LocationKey = ConfigurationManager.AppSettings["ip2LocationKey"];
            var endDatePlusOneDay = endDate.AddDays(1);
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var result = await (from t in context.subscriber_tranx
                                        join u in context.Users on t.SubscriberID equals u.Id into uj
                                        from u in uj.DefaultIfEmpty()
                                        join s in context.subscribers on t.SubscriberID equals s.SubscriberID into sj
                                        from s in sj.DefaultIfEmpty()
                                        where t.IpAddress != null && t.TranxDate >= startDate && t.TranxDate < endDatePlusOneDay
                                        orderby t.TranxDate descending
                                        select new LocationReport
                                        {
                                            FirstName = s.FirstName,
                                            LastName = s.LastName,
                                            EmailAddress = s.EmailAddress,
                                            PhoneNumber = u.PhoneNumber,
                                            PlanDesc = t.PlanDesc,
                                            Region = "",
                                            Country = "",
                                            TransactionDate = t.TranxDate,
                                            IpAddress = t.IpAddress,
                                            OrderNumber = t.OrderID
                                        }).OrderByDescending(x => x.TransactionDate).ToListAsync();

                    foreach (var location in result)
                    {
                        if (Util.IsLocaIP(location.IpAddress))
                        {
                            location.Country = "JM";
                        }
                        else
                        {
                            string apiUrl = String.Format("https://pro.ip-api.com/json/{0}?key={1}", location.IpAddress, ipApiProKey);
                            using (WebClient client = new WebClient())
                            {
                                string json = client.DownloadString(apiUrl);

                                dynamic jsonResult = new JavaScriptSerializer().Deserialize<dynamic>(json);
                                if (jsonResult["status"] == "success")
                                {
                                    location.Region = jsonResult["regionName"];
                                    location.Country = jsonResult["country"];
                                }
                                else
                                {
                                    try
                                    {
                                        string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", location.IpAddress, ip2LocationKey); //
                                        using (WebClient webClient = new WebClient())
                                        {
                                            string jsonRes = webClient.DownloadString(url);
                                            var userlocation = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);

                                            location.Region = userlocation.Region_Name;
                                            location.Country = userlocation.Country_Name;

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.LogError(ex);
                                        location.Region = "NF";
                                        location.Country = "NF";
                                    }
                                }

                            }
                        }


                    }

                    return View(result);
                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return View("locations");
            }
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

        [Route("epapersublist")]
        public async Task<ActionResult> SubListEpaper()
        {
            try
            {
                var ipApiProKey = ConfigurationManager.AppSettings["ipApiProKey"];
                var ip2LocationKey = ConfigurationManager.AppSettings["ip2LocationKey"];

                using (var context = new ApplicationDbContext())
                {
                    var sql = @"
                            SELECT c.*, b.oldesttranxdate,	a.StartDate, a.EndDate,	a.SubType,	a.PlanDesc, u.PhoneNumber,
                                CASE WHEN OrderID LIKE '%coupon%' THEN  'Complimentary'
	                            WHEN OrderID LIKE 'free%' THEN  'Complimentary'
	                            WHEN OrderID LIKE '%comp%' THEN  'Complimentary'
		                        WHEN OrderID = 'to_be_added' THEN  'Not sure'
	                            WHEN OrderID IN ('check', 'cheque') THEN  'Paid' ELSE 'Paid' 
                                END AS initialsubtype, orderid AS initialorderid, d.TranxDate AS initialcreated, d.PlanDesc AS initialplan, d.IpAddress, f.firstname, f.lastname, h.AddressLine1, h.addressline2, h.CityTown, h.stateparish, h.countrycode
                            FROM Subscriber_Epaper AS a
                            INNER JOIN ( SELECT EmailAddress, MIN(tranxdate) AS oldesttranxdate FROM Subscriber_Tranx GROUP BY EmailAddress) AS b ON a.EmailAddress = b.EmailAddress
                            INNER JOIN ( SELECT emailaddress, PlanDesc, OrderID, TranxDate, IpAddress FROM Subscriber_Tranx) AS d ON b.EmailAddress = d.EmailAddress AND b.oldesttranxdate = d.TranxDate
                            INNER JOIN subscribers AS f ON a.EmailAddress = f.EmailAddress
                            LEFT JOIN Subscriber_Address AS h ON f.AddressID = h.AddressID
                            LEFT JOIN ( SELECT emailaddress, SubType, OrderNumber, MAX(PlanDesc) AS LatestPlanDesc FROM Subscriber_Epaper WHERE Subscriber_EpaperID >= 1 GROUP BY emailaddress, SubType, OrderNumber) AS c ON a.EmailAddress = c.emailaddress AND a.PlanDesc = c.LatestPlanDesc AND a.SubType = c.SubType            
	                        LEFT JOIN AspNetUsers AS u ON a.EmailAddress = u.Email";

                    var result = await context.Database.SqlQuery<EPaperSubscriberResult>(sql).ToListAsync();

                    foreach (var location in result)
                    {
                       
                        string apiUrl = String.Format("https://pro.ip-api.com/json/{0}?key={1}", location.IpAddress, ipApiProKey);
                        if (String.IsNullOrWhiteSpace(location.CountryCode))
                        {
                            using (WebClient client = new WebClient())
                            {
                                string json = client.DownloadString(apiUrl);

                                dynamic jsonResult = new JavaScriptSerializer().Deserialize<dynamic>(json);
                                if (jsonResult["status"] == "success")
                                {
                                    location.CityTown = jsonResult["regionName"];
                                    location.CountryCode = jsonResult["country"];
                                }
                                else
                                {
                                    try
                                    {
                                        string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", location.IpAddress, ip2LocationKey); //
                                        using (WebClient webClient = new WebClient())
                                        {
                                            string jsonRes = webClient.DownloadString(url);
                                            var userlocation = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);

                                            location.CityTown = userlocation.Region_Name;
                                            location.CountryCode = userlocation.Country_Name;

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.LogError(ex);
                                        location.CityTown = "NF";
                                        location.CountryCode = "NF";
                                    }
                                }

                            }
                        }
                    }

                    return View(result);

                }


            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }
           
        }

        [HttpPost]
        [Route("epapersublist")]
        public async Task<ActionResult> SubListEpaper(string subType, DateTime startDate, DateTime endDate)
        {
            try
            {
                var ipApiProKey = ConfigurationManager.AppSettings["ipApiProKey"];
                var ip2LocationKey = ConfigurationManager.AppSettings["ip2LocationKey"];

                using (var context = new ApplicationDbContext())
                {
                    var sql = @"
                    SELECT c.*, b.oldesttranxdate,	a.StartDate, a.EndDate,	a.SubType,	a.PlanDesc, u.PhoneNumber,
                        CASE WHEN OrderID LIKE '%coupon%' THEN  'Complimentary'
	                    WHEN OrderID LIKE 'free%' THEN  'Complimentary'
	                    WHEN OrderID LIKE '%comp%' THEN  'Complimentary'
	                    WHEN OrderID = 'to_be_added' THEN  'Not sure'
	                    WHEN OrderID IN ('check', 'cheque') THEN  'Paid' ELSE 'Paid' 
                        END AS initialsubtype, orderid AS initialorderid, d.TranxDate AS initialcreated, d.PlanDesc AS initialplan, d.IpAddress, f.firstname, f.lastname, h.AddressLine1, h.addressline2, h.CityTown, h.stateparish, h.countrycode
                    FROM Subscriber_Epaper AS a
                    INNER JOIN ( SELECT EmailAddress, MIN(tranxdate) AS oldesttranxdate FROM Subscriber_Tranx GROUP BY EmailAddress) AS b ON a.EmailAddress = b.EmailAddress
                    INNER JOIN ( SELECT emailaddress, PlanDesc, OrderID, TranxDate, IpAddress FROM Subscriber_Tranx) AS d ON b.EmailAddress = d.EmailAddress AND b.oldesttranxdate = d.TranxDate
                    INNER JOIN subscribers AS f ON a.EmailAddress = f.EmailAddress
                    LEFT JOIN Subscriber_Address AS h ON f.AddressID = h.AddressID
                    LEFT JOIN ( SELECT emailaddress, SubType, OrderNumber, MAX(PlanDesc) AS LatestPlanDesc FROM Subscriber_Epaper WHERE Subscriber_EpaperID >= 1 GROUP BY emailaddress, SubType, OrderNumber) AS c ON a.EmailAddress = c.emailaddress AND a.PlanDesc = c.LatestPlanDesc AND a.SubType = c.SubType            
                    LEFT JOIN AspNetUsers AS u ON a.EmailAddress = u.Email
                    WHERE d.TranxDate BETWEEN @startDate AND @endDate AND (a.SubType LIKE @subType +'%')";

                    var sDate = new SqlParameter("startDate", startDate);
                    var eDate = new SqlParameter("endDate", endDate.AddDays(1));
                    var orderNum = new SqlParameter("subType", subType);

                    var result = await context.Database.SqlQuery<EPaperSubscriberResult>(sql, sDate, eDate, orderNum).ToListAsync();
                    foreach (var location in result)
                    {

                        string apiUrl = String.Format("https://pro.ip-api.com/json/{0}?key={1}", location.IpAddress, ipApiProKey);
                        if (String.IsNullOrWhiteSpace(location.CountryCode))
                        {
                            using (WebClient client = new WebClient())
                            {
                                string json = client.DownloadString(apiUrl);

                                dynamic jsonResult = new JavaScriptSerializer().Deserialize<dynamic>(json);
                                if (jsonResult["status"] == "success")
                                {
                                    location.CityTown = jsonResult["regionName"];
                                    location.CountryCode = jsonResult["country"];
                                }
                                else
                                {
                                    try
                                    {
                                        string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", location.IpAddress, ip2LocationKey); //
                                        using (WebClient webClient = new WebClient())
                                        {
                                            string jsonRes = webClient.DownloadString(url);
                                            var userlocation = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);

                                            location.CityTown = userlocation.Region_Name;
                                            location.CountryCode = userlocation.Country_Name;

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.LogError(ex);
                                        location.CityTown = "NF";
                                        location.CountryCode = "NF";
                                    }
                                }

                            }
                        }
                    }
                    return View(result);

                }

            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }

        }
    }
}