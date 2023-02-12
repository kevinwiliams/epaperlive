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
using Microsoft.AspNet.Identity;

using System.Net.Mail;
using System.Net.Mime;
using System.Web.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ePaperLive.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Market = GetSubscriberLocation().Country_Code;

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
        public async System.Threading.Tasks.Task<ActionResult> Index(FeedbackFormModel model)
        {
            //TODO: Send Mail
            //var accCtrl = new AccountController();
            var user = model.Email;
            string subject = model.Subject;
            string body = model.Message;
            //await accCtrl.UserManager.SendEmailAsync(user, subject, body);

            MvcCaptcha.ResetCaptcha("FeedbackCaptcha");

            var jsonFile = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/email_settings.json"));
            var settings = JObject.Parse(jsonFile);
            // Credentials
            string userName = (string)settings["email_address_username"],
                pwd = (string)settings["email_password"],
                smtp_host = (string)settings["smtp_host"],
                ssl_enabled = (string)settings["ssl_enabled"],
                password = (string)settings["email_password"],
                domain = (string)settings["email_address_domain"],
                portNumber = (string)settings["email_port_number"],
                feedBackEmails = (string)settings["feedback_email"];

            int port;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = smtp_host;
            smtp.Port = (int.TryParse(portNumber, out port) ? port : 25);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;

            var newMsg = new MailMessage();
            var mailSubject = subject;
            newMsg.To.Add(feedBackEmails);

           
            newMsg.From = new MailAddress(user, model.Name);
            newMsg.Subject = mailSubject;
            newMsg.Body = body;
            newMsg.IsBodyHtml = true;

            var credentials = new NetworkCredential(userName, pwd);
            smtp.Credentials = credentials;
            smtp.EnableSsl = bool.Parse(ssl_enabled);

            // Send
            await smtp.SendMailAsync(newMsg);


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

        public ActionResult Newsletter()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Newsletter(string email)
        {
            ViewData["newsletterEmail"] = email;
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