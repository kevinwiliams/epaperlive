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
using System.IO;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity.Owin;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin, Circulation")]
    [RoutePrefix("Admin/EpaperSub")]
    [Route("action = index")]
    public class EpaperSubController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        ActivityLog _actLog = new ActivityLog();

        // GET: EpaperSub
        [Route]
        public async Task<ActionResult> Index()
        {
            //var subscriber_epaper = db.subscriber_epaper.Include(s => s.Subscriber).AsNoTracking();
            //return View(await subscriber_epaper.ToListAsync());
            await Task.FromResult(0);
            return View();
        }

        [HttpPost]
        [Route]
        public async Task<ActionResult> Index(DataTableParameters dataTableParameters)
        {
            var subscriber_epaper = db.subscriber_epaper.AsQueryable();
            var searchTerm = dataTableParameters.search?.value;
            var filteredData = subscriber_epaper;
            try
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredData = filteredData.Where(x => x.EmailAddress.Contains(searchTerm) || x.PlanDesc.Contains(searchTerm) || x.OrderNumber.Contains(searchTerm));
                }

                // Order the data by the specified column and direction
                if (!string.IsNullOrEmpty(dataTableParameters.order?.FirstOrDefault()?.column.ToString()))
                {
                    var sortColumnIndex = int.Parse(dataTableParameters.order.FirstOrDefault().column.ToString());
                    var sortColumn = dataTableParameters.columns[sortColumnIndex].data;
                    var sortDirection = dataTableParameters.order.FirstOrDefault().dir == "desc" ? "OrderByDescending" : "OrderBy";
                    var property = typeof(Subscriber_Epaper).GetProperty(sortColumn);
                    var parameter = Expression.Parameter(typeof(Subscriber_Epaper), "p");
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    var resultExp = Expression.Call(typeof(Queryable), sortDirection, new Type[] { typeof(Subscriber_Epaper), property.PropertyType }, filteredData.Expression, Expression.Quote(orderByExp));
                    filteredData = filteredData.Provider.CreateQuery<Subscriber_Epaper>(resultExp);
                }

                var filteredCount = await filteredData.CountAsync(); // Get the filtered count
                var pageData = filteredData.Skip(dataTableParameters.start).Take(dataTableParameters.length);

                var pageDataList = await pageData.ToListAsync();

                return Json(new
                {
                    draw = dataTableParameters.draw,
                    recordsTotal = subscriber_epaper.Count(),
                    recordsFiltered = filteredCount,
                    data = pageDataList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
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
            _actLog.SubscriberID = User.Identity.GetUserId();
            _actLog.EmailAddress = subscriber_Epaper.EmailAddress;
            _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Staff");

            if (ModelState.IsValid)
            {
                db.Entry(subscriber_Epaper).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //log
                _actLog.LogInformation = "Modified subscription date (" + User.Identity.Name + ")";
                Util.LogUserActivity(_actLog);

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
            authUser.AddressDetails = new List<AddressDetails>();
            //AddressDetails address = new AddressDetails();
            //authUser.AddressDetails.Add(address);

            authUser.SubscriptionDetails = new List<SubscriptionDetails>();
            //SubscriptionDetails subscriptionDetails = new SubscriptionDetails();

            authUser.PaymentDetails = new List<PaymentDetails>();

            List<SelectListItem> Addressparishes = account.GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = account.GetCountryList();
            ViewBag.GetPaymentList = GetPaymentList();

            return View(authUser);
        }

        [HttpPost]
        [Route("addSubscriber")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSubscriber(FormCollection form, string nextBtn)
        {
            AccountController account = new AccountController();
            account.InitializeController(this.Request.RequestContext);
            AuthSubcriber authUser = new AuthSubcriber();
            //FormCollection form = new FormCollection();
            List<SelectListItem> Addressparishes = account.GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = account.GetCountryList();
            ViewBag.GetPaymentList = GetPaymentList();

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    
                    try
                    {
                        var EmailAddress = form["EmailAddress"];
                        var isExist = account.IsEmailExist(EmailAddress);

                        authUser.SendMail = bool.Parse(form["mailSend"]);
                        
                        authUser.FirstName = form["FirstName"];
                        authUser.LastName = form["LastName"];
                        authUser.EmailAddress = form["EmailAddress"];
                        authUser.Password = form["Password"];
                        authUser.AdminCreated = true;

                        authUser.AddressDetails = new List<AddressDetails>();
                        AddressDetails address = new AddressDetails {
                            AddressType = "M",
                            AddressLine1 = form["AddressLine1"],
                            AddressLine2 = form["AddressLine2"],
                            CityTown = form["CityTown"].Split(',')[0],
                            StateParish = form["StateParish"].Split(',')[0],
                            CountryCode = form["CountryCode"],
                            Phone = form["Phone"]
                        };
                        authUser.AddressDetails.Add(address);

                        authUser.PaymentDetails = new List<PaymentDetails>();

                        DeliveryAddress objDelv = account.GetSubscriberDeliveryAddress();

                        int RateID = Int32.Parse(form["RateID"].TrimStart().TrimEnd());
                        var selectedPlan = db.printandsubrates.AsNoTracking().FirstOrDefault(x => x.Rateid == RateID);
                        var currency = form["Currency"].Trim();
                        var paymentType = form["PaymentType"].Trim();
                        var paddedRateKey = Util.ZeroPadNumber(3, RateID);
                        // 2 Character Sub Type
                        var reSubType = form["SubType"].ToUpper().Substring(0, 2);

                        var OrderNumber = "";

                        switch (paymentType)
                        {
                            case "COMP":
                            case "STAFF":
                                OrderNumber = "COMPLIMENTARY-SUBSCRIPTION";
                                break;
                            case "HC-COMP":
                                OrderNumber = "HC-COMPLIMENTARY-SUBSCRIPTION";
                                break;
                            default:
                                OrderNumber = $"{reSubType}{"-"}{DateTime.Now.ToString("yyyyMMddhhmmssfffff")}{"-"}{currency}{"-"}{paddedRateKey}";
                                break;
                        }

                        PaymentDetails payment = new PaymentDetails
                        {
                            OrderNumber = OrderNumber,
                            RateID = RateID,
                            RateDescription = form["RateDescription"].TrimStart().Trim(),
                            Currency = form["Currency"].Trim(),
                            SubType = form["SubType"].Trim(),
                            CardType = paymentType,
                            CardOwner = authUser.FirstName + " " + authUser.LastName,
                            CardAmount = Decimal.Parse(form["CardAmount"]),
                            ConfirmationNumber = form["ConfirmationNumber"].Trim()
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
                                SubType = form["SubType"].Trim(),
                                RateType = form["RateType"].Trim(),
                                RateDescription = form["RateDescription"].Trim().TrimStart(),
                                RateID = RateID,
                                //NotificationEmail = bool.Parse(form["NotificationEmail"].Trim()),
                                //NewsletterSignUp = bool.Parse(form["NewsletterSignUp"].Trim())
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
                                SubType = form["SubType"].Trim(),
                                RateType = form["RateType"].Trim(),
                                RateDescription = form["RateDescription"].Trim(),
                                RateID = RateID,
                                //NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                //NewsletterSignUp = bool.Parse(form["NewsletterSignUp"]),
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
                                StartDate = DateTime.Parse(form["StartDate"]),
                                EndDate = pEndDate,
                                RateID = RateID,
                                DeliveryInstructions = form["DeliveryInstructions"],
                                RateType = form["RateType"],
                                SubType = "Print",
                                RateDescription = form["RateDescription"].Trim(),
                                //NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                //NewsletterSignUp = bool.Parse(form["NewsletterSignUp"])
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
                                //NotificationEmail = bool.Parse(form["NotificationEmail"]),
                                //NewsletterSignUp = bool.Parse(form["NewsletterSignUp"])
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

                        if (isExist)
                        {
                            ModelState.AddModelError("EmailExist", "Email address is already assigned. Please contact IT to extend the subscription.");
                            return View(authUser);
                        }

                        JOL_UserSession session;
                        var sessionRepository = new SessionRepository();
                        session = sessionRepository.CreateObject(authUser);
                        var isSaved = await sessionRepository.AddOrUpdate(authUser.PaymentDetails.FirstOrDefault().OrderNumber, session, RateID, authUser);

                        var saved = await account.SaveSubscriptionInfoAsync(authUser);
                        if (saved)
                        {
                            return RedirectToAction("index");
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.LogError(ex);
                        return View(authUser);
                    }
                    
                }
            }

            

            return View(authUser);
        }

        [Route("addcorp")]
        public ActionResult AddCorporateRelations()
        {
            return View();
        }

        [HttpPost]
        [Route("addcorp")]
        public async Task<ActionResult> AddCorporateRelations(CorporateAccount corporateAccount)
        {
            var _actLog = new ActivityLog();
            _actLog.SubscriberID = User.Identity.GetUserId();
            _actLog.EmailAddress = corporateAccount.EmailAddress;
            _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Circulation");

            if (ModelState.IsValid)
            {
                try
                {
                    var ac = new AccountController();
                    ac.InitializeController(this.Request.RequestContext);
                    ApplicationDbContext db = new ApplicationDbContext();

                    var parentID = await db.Users.FirstOrDefaultAsync(x => x.UserName == corporateAccount.EmailAddress);

                    if (parentID != null)
                    {
                        var managedAccts = corporateAccount.ManagedAccts.Split(',');

                        foreach (var account in managedAccts)
                        {
                            var email = account.TrimEnd().TrimStart();

                            var sql = @"
                            UPDATE [dbo].[Subscriber_Epaper]
                            SET [ParentId] = @ParentEmail
                            WHERE [EmailAddress] = @ManagedEmail";

                            var parentEmail = new SqlParameter("@ParentEmail", parentID.Id);
                            var managedEmail = new SqlParameter("@ManagedEmail", email);

                            await db.Database.ExecuteSqlCommandAsync(sql, new[] { parentEmail, managedEmail });
                        }

                        //log
                        _actLog.LogInformation = "Added Managed Accounts (" + User.Identity.Name + ") [" + corporateAccount.ManagedAccts.ToString() + "]";
                        Util.LogUserActivity(_actLog);
                        ViewBag.Msg = "Corporate relationship saved";
                        return View(corporateAccount);
                    }
                   
                }
                catch (Exception ex)
                {
                    ViewBag.Msg = "Corporate relationship saved failed";
                    Util.LogError(ex);
                    return View(corporateAccount);
                }

            }
            return View(corporateAccount);
        }

        [HttpPost]
        [Route("sendemail")]
        public async Task<bool> SendConfirmationEmail(string subscriberID, bool send = true)
        {
           

            AuthSubcriber authSubcriber = new AuthSubcriber();
            authSubcriber.SubscriptionDetails = new List<SubscriptionDetails>();
            var sent = false;
            if (send)
            {
                try
                {
                    var UserManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ApplicationUser user = new ApplicationUser();

                    //AspnetUser
                    user = UserManager.FindById(subscriberID);
                    var emailAddress = user.UserName;

                    _actLog.SubscriberID = User.Identity.GetUserId();
                    _actLog.EmailAddress = emailAddress;
                    _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Staff");

                    using (var context = new ApplicationDbContext())
                    {
                        var result = context.subscribers.Include(x => x.Subscriber_Epaper).FirstOrDefault(x => x.EmailAddress == emailAddress);
                        if (result != null)
                        {
                            authSubcriber.FirstName = result.FirstName;
                            authSubcriber.LastName = result.LastName;
                            authSubcriber.EmailAddress = result.EmailAddress;

                            SubscriptionDetails sd = new SubscriptionDetails 
                            { 
                                SubType = "Epaper",
                                EndDate = result.Subscriber_Epaper.FirstOrDefault().EndDate
                            };

                            authSubcriber.SubscriptionDetails.Add(sd);
                        }
                    }
                   
                    //send confirmation email
                    //set up email
                    string subject = "Subscription Renewal Notice";
                    string body = RenderViewToString(this.ControllerContext, "~/Views/Emails/ExtendSubscription.cshtml", authSubcriber);
                    await UserManager.SendEmailAsync(user.Id, subject, body);

                    //log
                    _actLog.LogInformation = "Renewal email sent (" + User.Identity.Name + ")";
                    Util.LogUserActivity(_actLog);
                    sent = true;
                }
                catch (Exception ex)
                {
                    Util.LogError(ex);
                }

            }

            return sent;
        }

        [Route("generatepassword")]
        public JsonResult GeneratePassword()
        {
            var resultData = new Dictionary<string, object>();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmopqrstuvwxyz0123456789";
            var random = new Random();
            var passwordCode = new string(
                Enumerable.Repeat(chars, 12)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            resultData["data"] = passwordCode;

            return Json(resultData);
        }
        public static string RenderViewToString<TModel>(ControllerContext controllerContext, string viewName, TModel model)
        {
            ViewEngineResult viewEngineResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);
            if (viewEngineResult.View == null)
            {
                throw new Exception("Could not find the View file. Searched locations:\r\n" + viewEngineResult.SearchedLocations);
            }
            else
            {
                IView view = viewEngineResult.View;

                using (var stringWriter = new StringWriter())
                {
                    var viewContext = new ViewContext(controllerContext, view, new ViewDataDictionary<TModel>(model), new TempDataDictionary(), stringWriter);
                    view.Render(viewContext, stringWriter);

                    return stringWriter.ToString();
                }
            }
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
        public static List<SelectListItem> GetPaymentList()
        {
           

            List<SelectListItem> paymentList = new List<SelectListItem>();

            paymentList.Add(new SelectListItem { Text = "Hard Copy Complimentary", Value = "HC-COMP" });
            paymentList.Add(new SelectListItem { Text = "Complimentary", Value = "COMP" });
            paymentList.Add(new SelectListItem { Text = "Staff", Value = "STAFF" });
            paymentList.Add(new SelectListItem { Text = "Cash", Value = "CASH" });
            paymentList.Add(new SelectListItem { Text = "Check", Value = "CHECK" });
            paymentList.Add(new SelectListItem { Text = "Bank Transfer", Value = "BANK" });
            
            return paymentList;
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
