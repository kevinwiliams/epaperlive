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
        
        [Route("addSubscriber")]
        public ActionResult AddSubscriber()
        {
            AccountController account = new AccountController();
            account.InitializeController(this.Request.RequestContext);

            List<SelectListItem> Addressparishes = account.GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = account.GetCountryList();

            return View();
        }

        [HttpPost]
        [Route("addSubscriber")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSubscriber(AuthSubcriber authUser, string nextBtn)
        {
            authUser.SubscriptionDetails = new List<SubscriptionDetails>();
            authUser.PaymentDetails = new List<PaymentDetails>();
            AccountController account = new AccountController();


            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    var isExist = account.IsEmailExist(authUser.EmailAddress);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                        return View(authUser);
                    }

                    try
                    {
                        DeliveryAddress objDelv = account.GetSubscriberDeliveryAddress();

                        var RateID = authUser.SubscriptionDetails.FirstOrDefault().RateID;
                        authUser.AddressDetails.FirstOrDefault().AddressType = "M";

                        var paddedRateKey = Util.ZeroPadNumber(3, authUser.PaymentDetails.FirstOrDefault().RateID);
                        // 2 Character Sub Type
                        var reSubType = authUser.PaymentDetails.FirstOrDefault().SubType.ToUpper().Substring(0, 2);

                        authUser.PaymentDetails.FirstOrDefault().OrderNumber = $"{reSubType}{"-"}{DateTime.Now.ToString("yyyyMMddhhmmssfffff")}{"-"}{authUser.PaymentDetails.FirstOrDefault().Currency}{"-"}{paddedRateKey}";

                        var selectedPlan = db.printandsubrates.FirstOrDefault(x => x.Rateid == RateID);

                        if (selectedPlan.Type == "Epaper")
                        {
                            var endDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate.AddDays((double)selectedPlan.ETerm);
                            authUser.SubscriptionDetails.FirstOrDefault().StartDate = DateTime.Now;
                            authUser.SubscriptionDetails.FirstOrDefault().EndDate = endDate;
                            authUser.SubscriptionDetails.FirstOrDefault().SubType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateDescription = selectedPlan.RateDescr;
                            authUser.SubscriptionDetails.FirstOrDefault().isActive = true;

                        }

                        if (selectedPlan.Type == "Print")
                        {
                            var endDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            authUser.SubscriptionDetails.FirstOrDefault().StartDate = DateTime.Now;
                            authUser.SubscriptionDetails.FirstOrDefault().EndDate = endDate;
                            authUser.SubscriptionDetails.FirstOrDefault().SubType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateDescription = selectedPlan.RateDescr;
                            authUser.SubscriptionDetails.FirstOrDefault().isActive = true;

                            AddressDetails deliveryAddress = new AddressDetails
                            {
                                AddressLine1 = objDelv.AddressLine1,
                                AddressLine2 = objDelv.AddressLine2,
                                AddressType = "D",
                                CityTown = objDelv.CityTown,
                                StateParish = objDelv.StateParish,
                                CountryCode = objDelv.CountryCode
                            };

                            authUser.AddressDetails.Add(deliveryAddress);
                        }

                        if (selectedPlan.Type == "Bundle")
                        {
                            var pEndDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate,
                                EndDate = pEndDate,
                                RateID = authUser.SubscriptionDetails.FirstOrDefault().RateID,
                                DeliveryInstructions = authUser.SubscriptionDetails.FirstOrDefault().DeliveryInstructions,
                                RateType = selectedPlan.Type,
                                SubType = "Print",
                                RateDescription = selectedPlan.RateDescr
                            };
                            //print subscription
                            authUser.SubscriptionDetails.Add(printSubscription);

                            var eEndDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate.AddDays((double)selectedPlan.ETerm);
                            authUser.SubscriptionDetails.FirstOrDefault().StartDate = DateTime.Now;
                            authUser.SubscriptionDetails.FirstOrDefault().EndDate = eEndDate;
                            authUser.SubscriptionDetails.FirstOrDefault().SubType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateType = selectedPlan.Type;
                            authUser.SubscriptionDetails.FirstOrDefault().RateDescription = selectedPlan.RateDescr;
                            authUser.SubscriptionDetails.FirstOrDefault().isActive = true;

                            AddressDetails deliveryAddress = new AddressDetails
                            {
                                AddressLine1 = objDelv.AddressLine1,
                                AddressLine2 = objDelv.AddressLine2,
                                AddressType = "D",
                                CityTown = objDelv.CityTown,
                                StateParish = objDelv.StateParish,
                                CountryCode = objDelv.CountryCode
                            };

                            authUser.AddressDetails.Add(deliveryAddress);
                        }

                        authUser.PaymentDetails.FirstOrDefault().RateID = RateID;
                        authUser.PaymentDetails.FirstOrDefault().RateDescription = selectedPlan.RateDescr;
                        authUser.PaymentDetails.FirstOrDefault().Currency = selectedPlan.Curr;
                        authUser.PaymentDetails.FirstOrDefault().SubType = selectedPlan.Type;
                        authUser.PaymentDetails.FirstOrDefault().CardType = "N/A";
                        authUser.PaymentDetails.FirstOrDefault().CardOwner = authUser.FirstName + " " + authUser.LastName;

                        JOL_UserSession session;
                        var sessionRepository = new SessionRepository();
                        session = sessionRepository.CreateObject(authUser);
                        var isSaved = await sessionRepository.AddOrUpdate(authUser.PaymentDetails.FirstOrDefault().OrderNumber, session, RateID, authUser);

                        var saved = await account.SaveSubscriptionInfoAsync(authUser);
                        if (saved)
                        {
                            return View("index");
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.LogError(ex);
                        return View();
                    }
                    


                    
                }
            }

            List<SelectListItem> Addressparishes = account.GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = account.GetCountryList();

            return View();
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
