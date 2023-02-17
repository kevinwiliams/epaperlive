﻿using System;
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

            AuthSubcriber authUser = new AuthSubcriber();
            //authUser.AddressDetails = new List<AddressDetails>();
            //AddressDetails address = new AddressDetails();
            //authUser.AddressDetails.Add(address);

            authUser.SubscriptionDetails = new List<SubscriptionDetails>();
            //SubscriptionDetails subscriptionDetails = new SubscriptionDetails();

            //authUser.PaymentDetails = new List<PaymentDetails>();

            List<SelectListItem> Addressparishes = account.GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = account.GetCountryList();

            return View();
        }

        [HttpPost]
        [Route("addSubscriber")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSubscriber(FormCollection form, string nextBtn)
        {
            AccountController account = new AccountController();
            account.InitializeController(this.Request.RequestContext);

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {

                    var EmailAddress = form["EmailAddress"];
                    var isExist = account.IsEmailExist(EmailAddress);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                        return View(form);
                    }

                    try
                    {


                        AuthSubcriber authUser = new AuthSubcriber();
                        authUser.FirstName = form["FirstName"];
                        authUser.LastName = form["LastName"];
                        authUser.Password = form["Password"];

                        authUser.AddressDetails = new List<AddressDetails>();
                        AddressDetails address = new AddressDetails {
                            AddressType = "M",
                            AddressLine1 = form["AddressLine1"],
                            AddressLine2 = form["AddressLine2"],
                            CityTown = form["CityTown"],
                            StateParish = form["StateParish"],
                            CountryCode = form["CountryCode"]
                        };
                        authUser.AddressDetails.Add(address);


                        authUser.PaymentDetails = new List<PaymentDetails>();

                        DeliveryAddress objDelv = account.GetSubscriberDeliveryAddress();

                        int RateID = Int32.Parse(form["RateID"]);
                        var selectedPlan = db.printandsubrates.FirstOrDefault(x => x.Rateid == RateID);
                        var currency = form["Currency"];

                        var paddedRateKey = Util.ZeroPadNumber(3, RateID);
                        // 2 Character Sub Type
                        var reSubType = form["SubType"].ToUpper().Substring(0, 2);
                        PaymentDetails payment = new PaymentDetails
                        {
                            OrderNumber = $"{reSubType}{"-"}{DateTime.Now.ToString("yyyyMMddhhmmssfffff")}{"-"}{currency}{"-"}{paddedRateKey}",
                            RateID = RateID,
                            RateDescription = form["RateDescription"].Trim(),
                            Currency = form["Currency"],
                            SubType = form["SubType"],
                            CardType = "N/A",
                            CardOwner = authUser.FirstName + " " + authUser.LastName,
                            CardAmount = Decimal.Parse(form["CardAmount"]) // (decimal)selectedPlan.Rate
                        };
                        authUser.PaymentDetails.Add(payment);


                        authUser.SubscriptionDetails = new List<SubscriptionDetails>();

                        if (selectedPlan.Type == "Epaper")
                        {
                            var endDate = DateTime.Parse(form["StartDate"]).AddDays((double)selectedPlan.ETerm);
                            SubscriptionDetails subscription = new SubscriptionDetails
                            {
                                StartDate = DateTime.Now,
                                EndDate = endDate,
                                SubType = form["SubType"],
                                RateType = form["RateType"],
                                RateDescription = form["RateDescription"].Trim(),
                                isActive = true,
                                NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                NewsletterSignUp = bool.Parse(form["NewsletterSignUp"])
                            };
                            authUser.SubscriptionDetails.Add(subscription);

                        }

                        if (selectedPlan.Type == "Print")
                        {
                            var endDate = DateTime.Parse(form["StartDate"]).AddDays((double)selectedPlan.PrintTerm * 7);
                            SubscriptionDetails subscription = new SubscriptionDetails
                            {
                                StartDate = DateTime.Now,
                                EndDate = endDate,
                                SubType = form["SubType"],
                                RateType = form["RateType"],
                                RateDescription = form["RateDescription"].Trim(),
                                isActive = true,
                                NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                NewsletterSignUp = bool.Parse(form["NewsletterSignUp"]),
                                DeliveryInstructions = form["DeliveryInstructions"]
                            };
                            authUser.SubscriptionDetails.Add(subscription);

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
                            var pEndDate = DateTime.Parse(form["StartDate"]).AddDays((double)selectedPlan.PrintTerm * 7);
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = authUser.SubscriptionDetails.FirstOrDefault().StartDate,
                                EndDate = pEndDate,
                                RateID = RateID,
                                DeliveryInstructions = form["DeliveryInstructions"],
                                RateType = form["RateType"],
                                SubType = "Print",
                                RateDescription = form["RateDescription"].Trim(),
                                NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                NewsletterSignUp = bool.Parse(form["NewsletterSignUp"])
                            };
                            //print subscription
                            authUser.SubscriptionDetails.Add(printSubscription);

                            var eEndDate = DateTime.Now.AddDays((double)selectedPlan.ETerm);
                            SubscriptionDetails subscription = new SubscriptionDetails
                            {
                                StartDate = DateTime.Now,
                                RateID = RateID,
                                EndDate = eEndDate,
                                SubType = form["RateType"],
                                RateType = "Epaper",
                                RateDescription = form["RateDescription"],
                                isActive = true,
                                NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                NewsletterSignUp = bool.Parse(form["NewsletterSignUp"])
                            };
                            authUser.SubscriptionDetails.Add(subscription);

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