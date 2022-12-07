using ePaperLive.DBModel;
using ePaperLive.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ePaperLive.Filters;
using BotDetect.Web.Mvc;

namespace ePaperLive.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(ipAddress.Split(':').FirstOrDefault()))
            {
                IPHostEntry Host = default;
                string Hostname = null;
                Hostname = System.Environment.MachineName;
                Host = Dns.GetHostEntry(Hostname);
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddress = Convert.ToString(IP);
                    }
                }
            }

            UserLocation location = new UserLocation();
            string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", ipAddress, "0d4f60641cd9b95ff5ac9b4d866a0655");
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(url);
                location = new JavaScriptSerializer().Deserialize<UserLocation>(json);

                if (Util.IsLocaIP(location.IP))
                {
                    location.Country_Code = "JM";
                }
            }

            LoginDetails login = new LoginDetails
            {
                location = location
            };

            Session["subscriber_location"] = location;

            if(Request.IsAuthenticated) {
                return RedirectToAction("Dashboard", "Account");
            }

            return View();
        }

        [HttpPost]
        [CaptchaValidationActionFilter("CaptchaCode", "FeedbackCaptcha", "Incorrect!")]
        public ActionResult Index(FeedbackFormModel model)
        {
            //TODO: Send Mail
            MvcCaptcha.ResetCaptcha("FeedbackCaptcha");
            return View(model);
        }

        public ActionResult GetPackages() 
        {
            PrintSubRates model = GetRatesList();

            return PartialView("_PackagesPartial", model);
        }

        public PrintSubRates GetRatesList()
        {
            PrintSubRates model = new PrintSubRates();
         
            try
            {
                UserLocation objLoc = GetSubscriberLocation();
                var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                ApplicationDbContext db = new ApplicationDbContext();
                List<printandsubrate> ratesList = db.printandsubrates.AsNoTracking()
                                    .Where(x => x.Market == market)
                                    .Where(x => x.Active == true).ToList();

                model.Rates = ratesList;

                return model;
            }
            catch (Exception e)
            {
                //handle exception
                Util.LogError(e);
            }
         

            return model;
        }

        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ServiceLevelAgreement()
        {
            return View();
        }

        public ActionResult PrivacyPolicy() 
        {
            return View();
        }

        public ActionResult TermsCondition()
        {
            return View();
        }

        private UserLocation GetSubscriberLocation()
        {
            if (Session["subscriber_location"] == null)
            {
                Session["subscriber_location"] = new UserLocation();
            }
            return (UserLocation)Session["subscriber_location"];
        }

        private Subscriber GetSubscriber()
        {
            if (Session["subscriber"] == null)
            {
                Session["subscriber"] = new Subscriber();
            }
            return (Subscriber)Session["subscriber"];
        }

        
    }
}