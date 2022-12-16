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
           
            LoginDetails login = new LoginDetails
            {
                location = GetSubscriberLocation()
            };

            if (Request.IsAuthenticated) {
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

        public ActionResult GetFeaturedPackages()
        {
            PrintSubRates model = GetRatesList();

            return PartialView("_FeaturedPackagesPartial", model);
        }

        public PrintSubRates GetRatesList()
        {
            PrintSubRates model = new PrintSubRates();

            Dictionary<string, int> preloadSub = GetPreloadSub();
            preloadSub.Clear();

            try
            {
                UserLocation objLoc = GetSubscriberLocation();
                var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                ApplicationDbContext db = new ApplicationDbContext();
                List<printandsubrate> ratesList = db.printandsubrates.AsNoTracking()
                                    .Where(x => x.Market == market && x.Active.Value).ToList();

                //update frequency pattern to text
                ratesList.ForEach(item =>
                {
                    if (item.PrintDayPattern != null)
                    {
                        item.PrintDayPattern = Util.DeliveryFreqToDate(item.PrintDayPattern);
                    }
                });

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

        public ActionResult PreloadSubscription(string rateType, int rateID) 
        {
            Dictionary<string, int> preloadSub = GetPreloadSub();
            preloadSub.Clear();
            preloadSub.Add(rateType, rateID);
            
            var responseData = new Dictionary<string, object>()
            {
                ["saved"] = true,
                ["data"] = preloadSub
            };

            return Json(responseData);
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
                Session["subscriber_location"] = Util.GetUserLocation();
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

        private Dictionary<string, int> GetPreloadSub()
        {
            if (Session["preloadSub"] == null)
            {
                Session["preloadSub"] = new Dictionary<string, int>();
            }
            return (Dictionary<string, int>)Session["preloadSub"];
        }


    }
}