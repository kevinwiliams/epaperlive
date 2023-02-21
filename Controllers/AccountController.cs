using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ePaperLive.Models;
using System.Web.Security;
using ePaperLive.DBModel;
using static ePaperLive.Util;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Configuration;
using FACGatewayService;
using FACGatewayService.FACPG;
using System.Runtime.Caching;
using System.IO;
using Newtonsoft.Json.Linq;
using ePaperLive.Filters;
using System.Net.Mail;
using System.Net;
using System.Web.Routing;
using System.Data.SqlClient;

namespace ePaperLive.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationDbContext _db;

        private static SessionRepository sessionRepository = new SessionRepository();
        private readonly ObjectCache cache = MemoryCache.Default;
        private readonly CacheItemPolicy cachePolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(120) };
        private readonly TempData tempData = new TempData();

        public AccountController()
        {
            _db = new ApplicationDbContext();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    using (var context = new ApplicationDbContext())
                    {
                        var subscriber = context.subscribers.FirstOrDefault(e => e.EmailAddress == model.Email);

                        if (subscriber != null) {

                            subscriber.LastLogin = DateTime.Now;
                            await context.SaveChangesAsync();
                        }
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Dashboard", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                model.CallBackUrl = callbackUrl;

                string subject = "Reset Password";
                string body = RenderViewToString(this.ControllerContext, "~/Views/Emails/PasswordReset.cshtml", model);
                await UserManager.SendEmailAsync(user.Id, subject, body);

               
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            AuthSubcriber authSubcriber = GetAuthSubscriber();
            Subscriber obj = GetSubscriber();

            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                    if (info == null)
                    {
                        return View("ExternalLoginFailure");
                    }
                    
                    authSubcriber.Login = info.Login;

                    var lastName = loginInfo.ExternalIdentity.Claims.FirstOrDefault(x => x.Type.Contains("surname")).Value;
                    var firstName = loginInfo.ExternalIdentity.Claims.FirstOrDefault(x => x.Type.Contains("givenname")).Value;
                    authSubcriber.FirstName = obj.FirstName = firstName;
                    authSubcriber.LastName = obj.LastName = lastName;
                    authSubcriber.EmailAddress = obj.EmailAddress = loginInfo.Email;
                    obj.CreatedAt = DateTime.Now;
                    obj.IpAddress = Request.UserHostAddress;

                    var isExist = IsEmailExist(loginInfo.Email);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                        LoginDetails loginDetails = new LoginDetails 
                        { 
                            EmailAddress = loginInfo.Email,
                            FirstName = firstName,
                            LastName = lastName
                        };
                        return View("LoginDetails", loginDetails);
                    }

                    UserLocation objLoc = GetSubscriberLocation();

                    var countryCode = (objLoc.Country_Code == "JM") ? "JAM" : "";

                    //load parishes
                    List<SelectListItem> parishes = GetParishes();
                    ViewBag.Parishes = new SelectList(parishes, "Value", "Text");
                    ViewBag.CountryList = GetCountryList();
                    ViewBag.Name = firstName;

                    //Test Data
                    AddressDetails ad = new AddressDetails
                    {
                        CountryCode = countryCode,
                        CountryList = GetCountryList(),
                    };

                    ViewData["preloadSub"] = GetPreloadSub();

                    return View("AddressDetails", ad);
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Clear();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
            }

            base.Dispose(disposing);
        }

        [HttpGet]
        public async Task<ActionResult> ReadPaper()
        {
            if (User.Identity.IsAuthenticated)
            {
                string token = "";
                var applicationUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                token = applicationUser.SecurityStamp;
                string pressReaderURL = "https://jamaicaobserver.pressreader.com/?token=";

                return Redirect(pressReaderURL + token);
            }
            else 
            {
                return View("dashboard");
            }
            
        }

        [AllowAnonymous]
        public ActionResult LoginModal()
        {
            return PartialView("_LoginModal");
        }

        [AllowAnonymous]
        public ActionResult DelAddressModal()
        {
            return PartialView("_DeliveryAddressModal");
        }

        public async Task<ActionResult> Dashboard()
        {
            try
            {
                string authUser = User.Identity.GetUserId();

                DateTime today = DateTime.Now;
                
                //check / update subscription
                var activeEpaperSubscription = await _db.subscriber_epaper.FirstOrDefaultAsync(x => x.SubscriberID == authUser && x.IsActive == true);
                if (activeEpaperSubscription != null)
                {
                    TimeSpan t = activeEpaperSubscription.EndDate - today;
                    var endDate = activeEpaperSubscription.EndDate;
                    double daysLeft = t.TotalDays;

                    if (daysLeft <= 1)
                    {
                        activeEpaperSubscription.IsActive = false;
                        await _db.SaveChangesAsync();
                    }
                }
                //check / update print subscription
                var activePrintSubscription = await _db.subscriber_print.FirstOrDefaultAsync(x => x.SubscriberID == authUser && x.IsActive == true);
                if (activePrintSubscription != null)
                {
                    TimeSpan t = activePrintSubscription.EndDate - today;
                    var endDate = activePrintSubscription.EndDate;
                    double daysLeft = t.TotalDays;

                    if (daysLeft <= 1)
                    {
                        activePrintSubscription.IsActive = false;
                        await _db.SaveChangesAsync();
                    }
                }
                AuthSubcriber authSubcriber = GetAuthSubscriber();

                if (authSubcriber.SubscriptionDetails == null)
                {
                    authSubcriber.SubscriberID = authUser;

                    using (var context = new ApplicationDbContext())
                    {
                        //load data and join via foriegn keys
                        var tableData = context.subscribers.AsNoTracking()
                            .Include(x => x.Subscriber_Address)
                            .Include(x => x.Subscriber_Epaper)
                            .Include(x => x.Subscriber_Print)
                            .Include(x => x.Subscriber_Tranx)
                            .FirstOrDefault(u => u.SubscriberID == authUser);

                        List<printandsubrate> ratesList = context.printandsubrates
                                                .Where(x => x.Active == true).ToList();


                        AuthSubcriber obj = new AuthSubcriber();
                        List<SubscriptionDetails> SubscriptionList = authSubcriber.SubscriptionDetails = new List<SubscriptionDetails>();
                        List<AddressDetails> AddressList = authSubcriber.AddressDetails = new List<AddressDetails>();
                        List<PaymentDetails> PaymentsList = authSubcriber.PaymentDetails = new List<PaymentDetails>();

                        if (tableData != null)
                        {
                            authSubcriber.FirstName = tableData.FirstName;
                            authSubcriber.LastName = tableData.LastName;
                            authSubcriber.EmailAddress = tableData.EmailAddress;

                            //Epaper subscriptions
                            if (tableData.Subscriber_Epaper.Count() > 0)
                            {
                                foreach (var epaper in tableData.Subscriber_Epaper)
                                {
                                    SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                                    {
                                        RateID = epaper.RateID,
                                        StartDate = epaper.StartDate,
                                        EndDate = epaper.EndDate,
                                        RateDescription = epaper.PlanDesc,
                                        SubType = (ratesList.Where(X => X.Rateid == epaper.RateID).Count() > 0) ? ratesList.FirstOrDefault(X => X.Rateid == epaper.RateID).Type : "Epaper",
                                        isActive = epaper.IsActive,
                                        SubscriptionID = epaper.Subscriber_EpaperID,
                                        RateType = "Epaper",
                                        OrderNumber = epaper.OrderNumber
                                    };
                                    SubscriptionList.Add(subscriptionDetails);
                                }
                            }
                            //Print Subscriptions
                            if (tableData.Subscriber_Print.Count() > 0)
                            {
                                foreach (var print in tableData.Subscriber_Print)
                                {
                                    if (SubscriptionList.FirstOrDefault(X => X.RateID == print.RateID) == null)
                                    {
                                        SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                                        {
                                            RateID = print.RateID,
                                            StartDate = print.StartDate,
                                            EndDate = print.EndDate,
                                            RateDescription = print.PlanDesc,
                                            isActive = print.IsActive,
                                            SubscriptionID = print.Subscriber_PrintID,
                                            RateType = "Print",
                                            OrderNumber = print.OrderNumber
                                        };
                                        SubscriptionList.Add(subscriptionDetails);
                                    }

                                }

                            }
                            //Addresses
                            if (tableData.Subscriber_Address.Count() > 0)
                            {
                                foreach (var address in tableData.Subscriber_Address)
                                {
                                    AddressDetails addressDetails = new AddressDetails
                                    {
                                        AddressLine1 = address.AddressLine1,
                                        AddressLine2 = address.AddressLine2,
                                        AddressType = address.AddressType,
                                        CityTown = address.CityTown,
                                        StateParish = address.StateParish,
                                        CountryCode = address.CountryCode,
                                        ZipCode = address.ZipCode,
                                        AddressID = address.AddressID
                                    };
                                    AddressList.Add(addressDetails);
                                }
                            }
                            //Transactions
                            if (tableData.Subscriber_Tranx.Count() > 0)
                            {
                                foreach (var payments in tableData.Subscriber_Tranx)
                                {
                                    PaymentDetails paymentDetails = new PaymentDetails
                                    {
                                        CardAmount = (decimal)payments.TranxAmount,
                                        CardNumber = payments.CardLastFour,
                                        CardExp = payments.CardExp,
                                        CardOwner = payments.CardOwner,
                                        CardType = payments.CardType,
                                        TranxDate = payments.TranxDate,
                                        RateDescription = payments.PlanDesc,
                                        TransactionID = payments.Subscriber_TranxID,
                                        OrderNumber = payments.OrderID
                                    };
                                    PaymentsList.Add(paymentDetails);

                                    //Update subscription if user requested refund
                                    SubscriptionList.FirstOrDefault(x => x.OrderNumber == payments.OrderID).RefundRequested = payments.RefundRequested;
                                }
                            }

                        }

                    }
                }

                ViewData["userFirstName"] = authSubcriber.FirstName;

                if (authSubcriber.AddressDetails != null)
                    ViewBag.address = authSubcriber.AddressDetails.ToList();
                if (authSubcriber.PaymentDetails != null)
                    ViewBag.payments = authSubcriber.PaymentDetails.ToList();
                if (authSubcriber.SubscriptionDetails != null)
                {
                    ViewBag.plans = authSubcriber.SubscriptionDetails;
                    if (authSubcriber.SubscriptionDetails.Where(x => x.isActive == true).Count() > 0)
                    {
                        var startDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).StartDate;
                        var endDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate;
                        ViewBag.dates = startDate.GetWeekdayInRange(endDate, DayOfWeek.Monday);
                    }

                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return View("index", "home");
            }
            
                
            return View();
        }

        public ActionResult Orders()
        {
            ViewData["Title"] = "Orders";
            try
            {
                AuthSubcriber authSubcriber = GetAuthSubscriber();
                List<SubscriptionDetails> subscriptionDetails = authSubcriber.SubscriptionDetails;
                if (subscriptionDetails != null)
                {

                    ViewData["FeedBackFormModelData"] = new FeedbackFormModel { 
                        Name = authSubcriber.FirstName + " " + authSubcriber.LastName,
                        Email = authSubcriber.EmailAddress
                    };

                    return View(subscriptionDetails);
                }
                else 
                {
                    return View("dashboard");
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return View("dashboard");
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        public async Task<ActionResult> Orders(FeedbackFormModel model)
        {

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

            if (ModelState.IsValid)
            {
                //TODO: Send Mail
                var user = model.Email;
                string subject = model.Subject;
                string body = model.Message;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtp_host;
                smtp.Port = (int.TryParse(portNumber, out port) ? port : 25);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = true;

                var newMsg = new MailMessage();
                var mailSubject = "Refund Request: " + subject;
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

               
                var orderNumber = subject.Split(':')[1].Trim();

                using (var context = new ApplicationDbContext())
                {
                    var result = context.subscriber_tranx.FirstOrDefault(x => x.EmailAddress == user && x.OrderID == orderNumber);

                    if (result != null)
                    {
                        result.RefundRequested = true;
                        await context.SaveChangesAsync();
                    }

                    var type = orderNumber.Split('-')[0].Trim();
                    /*
                    switch (type)
                    {

                        case "EP":
                            var epaperSub = context.subscriber_epaper.FirstOrDefault(x => x.EmailAddress == user && x.OrderNumber == orderNumber);
                            if (epaperSub != null)
                            {
                                epaperSub.IsActive = false;
                                await context.SaveChangesAsync();
                            }
                            break;
                        case "PR":
                            var printSub = context.subscriber_print.FirstOrDefault(x => x.EmailAddress == user && x.OrderNumber == orderNumber);
                            if (printSub != null)
                            {
                                printSub.IsActive = false;
                                await context.SaveChangesAsync();
                            }
                            break;
                        case "BU":
                            var printSubB = context.subscriber_print.FirstOrDefault(x => x.EmailAddress == user && x.OrderNumber == orderNumber);
                            if (printSubB != null)
                            {
                                printSubB.IsActive = false;
                                await context.SaveChangesAsync();
                            }
                            var epaperSubB = context.subscriber_epaper.FirstOrDefault(x => x.EmailAddress == user && x.OrderNumber == orderNumber);
                            if (epaperSubB != null)
                            {
                                epaperSubB.IsActive = false;
                                await context.SaveChangesAsync();
                            }
                            break;
                        default:
                            break;
                    }

                    */
                }

                return RedirectToAction("orders");
                //below code to be implemented if business decides to automatically deactivate after request
            }

            return View(model);
        }

        public async Task<ActionResult> UserProfile()
        {
            try
            {
                string authUser = User.Identity.GetUserName();

                using (var context = new ApplicationDbContext())
                {
                    var sql = @"
                    SELECT s.SubscriberID, s.EmailAddress, s.FirstName, s.LastName, s.DateOfBirth, s.IsActive,
                    sa.AddressID, sa. AddressLine1, sa.AddressLine2, sa.CityTown, sa.StateParish, sa.CountryCode
                    FROM Subscribers s with(nolock)
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = s.AddressID
                    WHERE s.EmailAddress = @EmailAddress";

                    var idParam = new SqlParameter("EmailAddress", authUser);

                    var result = await context.Database.SqlQuery<UserProfile>(sql, idParam).FirstOrDefaultAsync();

                    List<SelectListItem> Addressparishes = GetParishes();
                    ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
                    ViewBag.CountryList = GetCountryList();

                    return View(result);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return RedirectToAction("dashboard");
            }
            
        }
        [HttpPost]
        public async Task<ActionResult> UserProfile(UserProfile userProfile)
        {
            try
            {
                AuthSubcriber authSubcriber = GetAuthSubscriber();
                ViewBag.plans = authSubcriber.SubscriptionDetails;

                string authUser = User.Identity.GetUserId();

                using (var context = new ApplicationDbContext())
                {
                    //load data and join via foriegn keys
                    var tableData = context.subscribers
                        .Include(x => x.Subscriber_Address)
                        .FirstOrDefault(u => u.SubscriberID == authUser);

                    if (tableData != null)
                    {

                        try
                        {
                            AuthSubcriber localAuthSubcriber = GetAuthSubscriber();
                            var newAddress = new Subscriber_Address();
                            int oldAddressID = (tableData.Subscriber_Address.FirstOrDefault(i => i.AddressType == "M") != null) ? tableData.Subscriber_Address.FirstOrDefault(i => i.AddressType == "M").AddressID : 0;
                            var oldAddress = context.subscriber_address.FirstOrDefault(x => x.AddressID == oldAddressID);
                            if (oldAddress != null)
                            {

                                oldAddress.AddressLine1 = userProfile.AddressLine1;
                                oldAddress.AddressLine2 = userProfile.AddressLine2;
                                oldAddress.CityTown = userProfile.CityTown;
                                oldAddress.StateParish = userProfile.StateParish;
                                oldAddress.ZipCode = userProfile.ZipCode;
                                oldAddress.CountryCode = userProfile.CountryCode;

                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").AddressLine1 = userProfile.AddressLine1;
                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").AddressLine2 = userProfile.AddressLine2;
                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").CityTown = userProfile.CityTown;
                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").StateParish = userProfile.StateParish;
                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").ZipCode = userProfile.ZipCode;
                                //tableData.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").CountryCode = userProfile.CountryCode;
                                await context.SaveChangesAsync();
                                newAddress = oldAddress;
                            }
                            else 
                            {
                                newAddress = new Subscriber_Address
                                {
                                    SubscriberID = authUser,
                                    EmailAddress = User.Identity.GetUserName(),
                                    AddressLine1 = userProfile.AddressLine1,
                                    AddressLine2 = userProfile.AddressLine2,
                                    CityTown = userProfile.CityTown,
                                    StateParish = userProfile.StateParish,
                                    ZipCode = userProfile.ZipCode,
                                    CountryCode = userProfile.CountryCode,
                                    AddressType = "M",
                                    CreatedAt = DateTime.Now
                                };
                                context.subscriber_address.Add(newAddress);
                                await context.SaveChangesAsync();
                            }


                            //update tables
                            tableData.FirstName = userProfile.FirstName;
                            tableData.LastName = userProfile.LastName;
                            tableData.AddressID = newAddress.AddressID;
                            await context.SaveChangesAsync();

                            if (localAuthSubcriber.AddressDetails != null)
                            {
                                var dbAddress = localAuthSubcriber.AddressDetails.FirstOrDefault(x => x.AddressType == "M");
                                if (dbAddress != null)
                                {
                                    localAuthSubcriber.AddressDetails.Remove(dbAddress);
                                }

                                AddressDetails address = new AddressDetails
                                {
                                    AddressID = newAddress.AddressID,
                                    AddressLine1 = newAddress.AddressLine1,
                                    AddressLine2 = newAddress.AddressLine2,
                                    CityTown = newAddress.CityTown,
                                    StateParish = newAddress.StateParish,
                                    ZipCode = newAddress.ZipCode,
                                    CountryCode = newAddress.CountryCode,
                                    AddressType = newAddress.AddressType,
                                };

                                localAuthSubcriber.AddressDetails.Add(address);
                            }
                            

                            ViewBag.msg = "Profile updated successfully";
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                            return RedirectToAction("UserProfile");

                        }


                    }
                }
                return RedirectToAction("UserProfile");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return View("dashboard");
            }
            
        }
        public ActionResult UpdateProfile() 
        {
            AuthSubcriber authSubcriber = GetAuthSubscriber();
            ViewBag.plans = authSubcriber.SubscriptionDetails;
            ViewData["preloadSub"] = GetPreloadSub();

            List<SelectListItem> Addressparishes = GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();

            return View(authSubcriber);
        }
        public ActionResult ExtendSubscription()
        {
            AuthSubcriber authSubcriber = GetAuthSubscriber();
            ViewBag.plans = authSubcriber.SubscriptionDetails;
            SubscriptionDetails subscription = new SubscriptionDetails();
            subscription.StartDate = DateTime.Now;

            if (authSubcriber.AddressDetails != null)
            {
                var mailingAddress = authSubcriber.AddressDetails.FirstOrDefault(x => x.AddressType == "M");
                if (mailingAddress != null)
                {
                    ViewData["savedAddress"] = true;
                    ViewData["savedAddressData"] = JsonConvert.SerializeObject(mailingAddress);
                }
            }

            if (authSubcriber.SubscriptionDetails != null)
            {
                ViewBag.plans = authSubcriber.SubscriptionDetails;

                if (authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true) != null)
                {
                    var startDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).StartDate;
                    var endDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate;
                    ViewBag.dates = startDate.GetWeekdayInRange(endDate, DayOfWeek.Monday);

                    subscription = new SubscriptionDetails
                    {
                        StartDate = (authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate != null) ? authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate : DateTime.Now,
                        RateType = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).RateType,
                        RateID = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).RateID
                    };
                }
                
            }
            //load parishes
            List<SelectListItem> parishes = GetParishes();
            ViewBag.Parishes = new SelectList(parishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();
            

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExtendSubscription(SubscriptionDetails data, string nextBtn)
        {
            //load parishes
            List<SelectListItem> parishes = GetParishes();
            ViewBag.Parishes = new SelectList(parishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {

                    try
                    {
                        ApplicationDbContext db = new ApplicationDbContext();
                        Subscriber objSub = GetSubscriber();
                        DeliveryAddress objDelv = GetSubscriberDeliveryAddress();
                        Subscriber_Epaper objEp = GetEpaperDetails();
                        Subscriber_Print objPr = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        AuthSubcriber authUser = GetAuthSubscriber();
                        List<SubscriptionDetails> subscriptionDetails = new List<SubscriptionDetails>();
                        List<PaymentDetails> paymentDetailsList = new List<PaymentDetails>();
                        //List<AddressDetails> addressDetails = new List<AddressDetails>();
                        authUser.SubscriptionDetails.RemoveAll(x => x.SubscriptionID == 0);
                        objTran.RateID = data.RateID;

                        var selectedPlan = db.printandsubrates.FirstOrDefault(x => x.Rateid == data.RateID);

                        if (selectedPlan.Type == "Print")
                        {

                            var existingPrintPlan = new SubscriptionDetails();
                            var endDate = DateTime.Now;
                            if (authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.isActive == true) != null)
                            {
                                existingPrintPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.isActive == true);
                                data.StartDate = existingPrintPlan.EndDate;
                                endDate = data.EndDate = existingPrintPlan.EndDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            }
                            else
                            {
                                endDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            }

                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = endDate,
                                RateID = data.RateID,
                                DeliveryInstructions = data.DeliveryInstructions,
                                RateType = selectedPlan.Type
                            };

                            subscriptionDetails.Add(printSubscription);
                            authUser.SubscriptionDetails.Add(printSubscription);

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
                        if (selectedPlan.Type == "Epaper")
                        {
                            var existingEpaperPlan = new SubscriptionDetails();
                            var endDat = DateTime.Now;

                            var startDate = (existingEpaperPlan.EndDate > DateTime.Now) ? existingEpaperPlan.EndDate : DateTime.Now;
                            if (authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.isActive == true) != null)
                            {
                                existingEpaperPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.isActive == true);
                                data.StartDate = startDate;
                                endDat = data.EndDate = existingEpaperPlan.EndDate.AddDays((double)selectedPlan.ETerm);
                            }
                            else
                            {
                                endDat = data.StartDate.AddDays((double)selectedPlan.ETerm);
                            }
                            
                            SubscriptionDetails epaperSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = endDat,
                                RateID = data.RateID,
                                SubType = selectedPlan.Type,
                                RateType = selectedPlan.Type
                            };

                            subscriptionDetails.Add(epaperSubscription);
                            authUser.SubscriptionDetails.Add(epaperSubscription);


                        }
                        if (selectedPlan.Type == "Bundle")
                        {
                            var existingPrintPlan = new SubscriptionDetails();
                            var pEndDate = DateTime.Now;
                            if (authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.isActive == true) != null)
                            {

                                existingPrintPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.isActive == true);
                                data.StartDate = existingPrintPlan.EndDate;
                                pEndDate = data.EndDate = existingPrintPlan.EndDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            }
                            else
                            {
                                pEndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            }
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = pEndDate,
                                RateID = data.RateID,
                                DeliveryInstructions = data.DeliveryInstructions,
                            };
                            //print subscription

                            subscriptionDetails.Add(printSubscription);
                            var existingEpaperPlan = new SubscriptionDetails();
                            var eEndDate = DateTime.Now;
                            if (authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.isActive == true) != null)
                            {
                                var startDate = (existingEpaperPlan.EndDate > DateTime.Now) ? existingEpaperPlan.EndDate : DateTime.Now;
                                existingEpaperPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.isActive == true);
                                data.StartDate = startDate;
                                eEndDate = data.EndDate = existingEpaperPlan.EndDate.AddDays((double)selectedPlan.ETerm);
                            }
                            else
                            {
                                eEndDate = data.StartDate.AddDays((double)selectedPlan.ETerm);
                            }
                            SubscriptionDetails epaperSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = eEndDate,
                                RateID = data.RateID,
                                SubType = data.SubType,
                                NotificationEmail = data.NotificationEmail,
                                RateType = selectedPlan.Type

                            };
                            //Epaper subscription
                            subscriptionDetails.Add(epaperSubscription);
                            authUser.SubscriptionDetails.Add(epaperSubscription);


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


                        List<SelectListItem> billparishes = GetParishes();
                        ViewBag.Parishes = new SelectList(billparishes, "Value", "Text");
                        ViewBag.CountryList = GetCountryList();

                        AddressDetails billingAddress = new AddressDetails
                        {
                            CountryList = GetCountryList()
                        };


                        PaymentDetails pd = new PaymentDetails
                        {
                            RateID = data.RateID,
                            RateDescription = selectedPlan.RateDescr,
                            Currency = selectedPlan.Curr,
                            CardAmount = (decimal)selectedPlan.Rate,
                            SubType = selectedPlan.Type,
                            BillingAddress = billingAddress,
                            IsExtension = true
                        };

                        paymentDetailsList.Add(pd);
                        authUser.PaymentDetails.Add(pd);

                        if (authUser.SubscriptionDetails != null)
                        {
                            ViewBag.plans = authUser.SubscriptionDetails;

                            if (authUser.SubscriptionDetails.FirstOrDefault(x => x.isActive == true) != null)
                            {
                                var startDate = authUser.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).StartDate;
                                var endDate = authUser.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate;
                                ViewBag.dates = startDate.GetWeekdayInRange(endDate, DayOfWeek.Monday);
                            }
                        }

                        return View("ExtendPayment", pd);
                    }
                    catch (Exception ex)
                    {

                        LogError(ex);
                        return View("dashboard");

                    }

                }
            }

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExtendPayment(PaymentDetails data, string prevBtn, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();
            AuthSubcriber authSubcriber = GetAuthSubscriber();

            var cardType = data.CardType;

            if (prevBtn != null)
            {

                try
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    Subscriber objSub = GetSubscriber();
                    Subscriber_Epaper objEp = GetEpaperDetails();
                    Subscriber_Print objPr = GetPrintDetails();
                    UserLocation objLoc = GetSubscriberLocation();
                    var market = (objLoc.Country_Code == "JM") ? "Local" : "International";


                    SubscriptionDetails sd = new SubscriptionDetails
                    {
                        StartDate = objEp.StartDate,
                        RateID = objEp.RateID,
                        DeliveryInstructions = objPr.DeliveryInstructions,
                        SubType = objEp.SubType,
                        RatesList = db.printandsubrates.Where(x => x.Market == market).Where(x => x.Active == true).ToList(),
                        Market = market
                    };

                    return View("ExtendSubscription", sd);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return View(data);
                }

            }

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {

                    try
                    {
                        //get all session variables
                        AuthSubcriber authUser = GetAuthSubscriber();
                        Subscriber objSub = GetSubscriber();
                        Subscriber_Address objAdd = GetSubscriberAddress();
                        Subscriber_Epaper objE = GetEpaperDetails();
                        Subscriber_Print objP = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        ApplicationUser user = GetAppUser();

                        objTran.CardType = data.CardType;
                        objTran.CardOwner = data.CardOwner;
                        objTran.TranxAmount = (double)data.CardAmount;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        return View(data);
                    }

                    dynamic summary = await ChargeCard(data);
                    return Json(data);

                }
            }

            AddressDetails mailingAddress = authSubcriber.AddressDetails.FirstOrDefault(x => x.AddressType == "M");
            ViewData["savedAddress"] = true;
            ViewData["savedAddressData"] = JsonConvert.SerializeObject(mailingAddress);

            if (authSubcriber.SubscriptionDetails != null)
            {
                ViewBag.plans = authSubcriber.SubscriptionDetails;

                if (authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true) != null)
                {
                    var startDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).StartDate;
                    var endDate = authSubcriber.SubscriptionDetails.FirstOrDefault(x => x.isActive == true).EndDate;
                    ViewBag.dates = startDate.GetWeekdayInRange(endDate, DayOfWeek.Monday);
                }
             }

            ViewBag.CountryList = GetCountryList();
            return View(data);
        }
        [AllowAnonymous]
        public ActionResult Subscribe(string pkgType, string term, decimal price = 0)
        {
           
            ViewData["preloadSub"] = GetPreloadSub();
            //Test Data
            LoginDetails ld = new LoginDetails
            {
                //FirstName = "Dwayne",
                //LastName = "Mendez",
                //EmailAddress = "dwayne.mendez@live.net",
            };
            return View("LoginDetails", ld);
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult LoginDetails(LoginDetails data, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var isExist = IsEmailExist(data.EmailAddress);
                        if (isExist)
                        {
                            ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                            return View(data);
                        }
                        AuthSubcriber authUser = GetAuthSubscriber();
                        Subscriber obj = GetSubscriber();
                        ApplicationUser user = GetAppUser();


                        authUser.FirstName = obj.FirstName = data.FirstName;
                        authUser.LastName = obj.LastName = data.LastName;
                        authUser.EmailAddress = obj.EmailAddress = data.EmailAddress;
                        authUser.Password = data.Password;
                        //obj.passwordHash = PasswordHash(data.Password);
                        obj.CreatedAt = DateTime.Now;
                        obj.IpAddress = Request.UserHostAddress;
                        
                        user.Email = data.EmailAddress;
                        user.PasswordHash = data.Password;

                        UserLocation objLoc = GetSubscriberLocation();

                        var countryCode = (objLoc.Country_Code == "JM") ? "JAM" : "";

                        //load parishes
                        List<SelectListItem> parishes = GetParishes();
                        ViewBag.Parishes = new SelectList(parishes, "Value", "Text");
                        ViewBag.CountryList = GetCountryList();
                        ViewBag.Name = data.FirstName;

                        //Test Data
                        AddressDetails ad = new AddressDetails
                        {
                            //AddressLine1 = "Lot 876 Scheme Steet",
                            //CityTown = "Hope Bay",
                            //StateParish = "Portland",
                            //ZipCode = "JAMWI",
                            //Phone = "876-875-8651",
                            CountryCode = countryCode,
                            CountryList = GetCountryList(),
                        };

                        return View("AddressDetails", ad);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        return View(data);

                    }

                }
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AddressDetails(AddressDetails data, string prevBtn, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();

            List<SelectListItem> Addressparishes = GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();

            if (prevBtn != null)
            {
                try
                {
                    Subscriber obj = GetSubscriber();
                    LoginDetails ld = new LoginDetails
                    {
                        FirstName = obj.FirstName,
                        LastName = obj.LastName,
                        EmailAddress = obj.EmailAddress
                    };

                    return View("LoginDetails", ld);
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        Subscriber objSub = GetSubscriber();
                        Subscriber_Address objAdd = GetSubscriberAddress();
                        UserLocation objLoc = GetSubscriberLocation();
                        ApplicationUser user = GetAppUser();
                        AuthSubcriber authUser = GetAuthSubscriber();
                        List<AddressDetails> AddressList = new List<AddressDetails>();

                        var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                        //use for 2-step auth
                        user.PhoneNumber = data.Phone;
                        //add email from subscriber
                        objAdd.EmailAddress = objSub.EmailAddress;
                        //address type M - Mailing --- B - Billing
                        objAdd.AddressType = data.AddressType = "M";
                        objAdd.AddressLine1 = data.AddressLine1;
                        objAdd.AddressLine2 = data.AddressLine2;
                        objAdd.CityTown = data.CityTown;
                        objAdd.StateParish = data.StateParish;
                        objAdd.ZipCode = data.ZipCode;
                        objAdd.CountryCode = data.CountryCode;
                        objAdd.CreatedAt = DateTime.Now;

                        AddressList.Add(data);
                        authUser.AddressDetails = AddressList;
                        //load rates on the next (subscription) page
                        //GetParishes();
                        List<SelectListItem> parishes = GetParishes();
                        ViewBag.Parishes = new SelectList(parishes, "Value", "Text");
                        ViewBag.CountryList = GetCountryList();
                        ViewBag.Name = authUser.FirstName;

                        DeliveryAddress delAddressDetails = new DeliveryAddress
                        { 
                            CountryList = GetCountryList()
                        };

                        SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                        {
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(30),
                            Market = market,
                            DeliveryAddress = delAddressDetails

                        };


                        return View("SubscriptionInfo", subscriptionDetails);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        return View(data);
                    }

                }
            }
            return View(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult DistrictList(string Id)
        {
            var parish = Id.Replace("-", ". ").Replace("_", " & ");
            var district = from s in District.GetDistrict()
                           where s.ParishName == parish
                           select s;
            return Json(new SelectList(district.ToArray(), "ParishName", "TownName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetRatesList()
        {

            return PartialView("_RatesFormPartial");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRatesList(string rateType, bool isRenewal = false)
        {
            //rateType = (rateType != null) ? rateType : "Epaper";
            PrintSubRates model = new PrintSubRates();
            /*return View(model);*/

            if (rateType != null)
            {
                try
                {
                    UserLocation objLoc = GetSubscriberLocation();
                    var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                    ApplicationDbContext db = new ApplicationDbContext();
                    List<printandsubrate> ratesList = db.printandsubrates
                                        .Where(x => x.Market == market)
                                        .Where(x => x.Active == true).ToList();


                    var nratesList = (rateType != null) ? ratesList.Where(x => x.Type == rateType).ToList() : ratesList;
     
                    foreach (var item in nratesList.Where(x => x.PrintDayPattern != null))
                    {
                        item.PrintDayPattern = DeliveryFreqToDate(item.PrintDayPattern);
                    }

                    model.Rates = nratesList;
                    model.RateType = rateType;
                    model.IsRenewal = isRenewal;

                    return PartialView("_RatesPartial", model);
                }
                catch (Exception e)
                {
                    //handle exception
                    LogError(e);
                }
            }

            return PartialView("_RatesPartial", model);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRatesListAdmin(string rateType, bool isRenewal = false)
        {
            //rateType = (rateType != null) ? rateType : "Epaper";
            PrintSubRates model = new PrintSubRates();
            /*return View(model);*/

            if (rateType != null)
            {
                try
                {
                    UserLocation objLoc = GetSubscriberLocation();
                    var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                    ApplicationDbContext db = new ApplicationDbContext();
                    List<printandsubrate> ratesList = db.printandsubrates.ToList();
                                        //.Where(x => x.Market == market)
                                        //.Where(x => x.Active == true).ToList();


                    var nratesList = (rateType != null) ? ratesList.Where(x => x.Type == rateType).ToList() : ratesList;

                    foreach (var item in nratesList.Where(x => x.PrintDayPattern != null))
                    {
                        item.PrintDayPattern = DeliveryFreqToDate(item.PrintDayPattern);
                    }

                    model.Rates = nratesList;
                    model.RateType = rateType;
                    model.IsRenewal = isRenewal;

                    return PartialView("_RatesPartial", model);
                }
                catch (Exception e)
                {
                    //handle exception
                    LogError(e);
                }
            }

            return PartialView("_RatesPartial", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SubscriptionInfo(SubscriptionDetails data, string prevBtn, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();
            List<SelectListItem> parishes = GetParishes();
            ViewBag.Parishes = new SelectList(parishes, "Value", "Text");

            ApplicationDbContext db = new ApplicationDbContext();

            if (prevBtn != null)
            {
                try
                {
                    Subscriber_Address objAdd = GetSubscriberAddress();
                    AddressDetails ad = new AddressDetails
                    {
                        AddressLine1 = objAdd.AddressLine1,
                        AddressLine2 = objAdd.AddressLine2,
                        CityTown = objAdd.CityTown,
                        StateParish = objAdd.StateParish,
                        ZipCode = objAdd.ZipCode,
                        CountryCode = objAdd.CountryCode,
                        CountryList = GetCountryList()
                    };

                    List<SelectListItem> prevparishes = GetParishes();
                    ViewBag.Parishes = new SelectList(prevparishes, "Value", "Text");
                    ViewBag.CountryList = GetCountryList();

                    return View("AddressDetails", ad);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }


            }

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {

                    try
                    {
                        Subscriber objSub = GetSubscriber();
                        DeliveryAddress objDelv = GetSubscriberDeliveryAddress();
                        Subscriber_Epaper objEp = GetEpaperDetails();
                        Subscriber_Print objPr = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        AuthSubcriber authUser = GetAuthSubscriber();
                        List<SubscriptionDetails> subscriptionDetails = authUser.SubscriptionDetails = new List<SubscriptionDetails>();
                        List<PaymentDetails> paymentDetailsList = authUser.PaymentDetails = new List<PaymentDetails>();
                        //List<AddressDetails> addressDetails = new List<AddressDetails>();

                        objSub.Newsletter = data.NewsletterSignUp;
                        objTran.RateID = data.RateID;

                        var selectedPlan = db.printandsubrates.FirstOrDefault(x => x.Rateid == data.RateID);

                        if (selectedPlan.Type == "Print")
                        {
                            var endDate = data.EndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = endDate,
                                RateID = data.RateID,
                                DeliveryInstructions = data.DeliveryInstructions,
                                RateType = selectedPlan.Type,
                                SubType = selectedPlan.Type,
                                RateDescription = selectedPlan.RateDescr
                            };

                            subscriptionDetails.Add(printSubscription);

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
                        if (selectedPlan.Type == "Epaper")
                        {
                            var endDate = data.EndDate = data.StartDate.AddDays((double)selectedPlan.ETerm);
                            SubscriptionDetails epaperSubscription = new SubscriptionDetails
                            {
                                StartDate = DateTime.Now,
                                EndDate = endDate,
                                RateID = data.RateID,
                                SubType = selectedPlan.Type,
                                NotificationEmail = data.NotificationEmail,
                                RateType = selectedPlan.Type,
                                RateDescription = selectedPlan.RateDescr
                            };

                            subscriptionDetails.Add(epaperSubscription);
                        }
                        if (selectedPlan.Type == "Bundle")
                        {
                            var pEndDate = data.EndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = pEndDate,
                                RateID = data.RateID,
                                DeliveryInstructions = data.DeliveryInstructions,
                                RateType = selectedPlan.Type,
                                SubType = "Print",
                                RateDescription = selectedPlan.RateDescr
                            };
                            //print subscription
                            subscriptionDetails.Add(printSubscription);

                            var eEndDate = data.EndDate = data.StartDate.AddDays((double)selectedPlan.ETerm);
                            SubscriptionDetails epaperSubscription = new SubscriptionDetails
                            {
                                StartDate = DateTime.Now,
                                EndDate = eEndDate,
                                RateID = data.RateID,
                                SubType = "Epaper",
                                NotificationEmail = data.NotificationEmail,
                                RateType = selectedPlan.Type,
                                RateDescription = selectedPlan.RateDescr
                            };
                            //Epaper subscription
                            subscriptionDetails.Add(epaperSubscription);

                            AddressDetails deliveryAddress = new AddressDetails
                            {
                                AddressLine1 = objDelv.AddressLine1,
                                AddressLine2 = objDelv.AddressLine2,
                                AddressType = "D",
                                CityTown = objDelv.CityTown,
                                StateParish = objDelv.StateParish,
                                CountryCode= objDelv.CountryCode
                            };

                            authUser.AddressDetails.Add(deliveryAddress);
                        }


                        List<SelectListItem> billparishes = GetParishes();
                        ViewBag.Parishes = new SelectList(billparishes, "Value", "Text");
                        ViewBag.CountryList = GetCountryList();

                        AddressDetails billingAddress = new AddressDetails {
                            //Test data
                            //AddressLine1 = "Lot 876 Scheme Steet",
                            //CityTown = "Bay Town",
                            //StateParish = "Portland",
                            //CountryCode = "JAM",
                            //ZipCode = "JAMWI",
                            //end 
                            CountryList = GetCountryList()
                        };

                        var printTerm = selectedPlan.PrintTerm + " " + selectedPlan.PrintTermUnit;
                        var epaperTerm = selectedPlan.ETerm + " " + selectedPlan.ETermUnit;
                        var rateTerm = (selectedPlan.Type == "Print") ? printTerm : epaperTerm;
                        var ratePrice = (selectedPlan.OfferIntroRate) ? selectedPlan.IntroRate : selectedPlan.Rate;
                        PaymentDetails pd = new PaymentDetails
                        {
                            RateID = data.RateID,
                            RateDescription = selectedPlan.RateDescr,
                            RateTerm = rateTerm,
                            Currency = selectedPlan.Curr,
                            CardAmount = (decimal)ratePrice,
                            SubType = selectedPlan.Type,
                            BillingAddress = billingAddress,
                            //test data
                            //CardOwner = "Dwayne Mendez",
                        };

                        paymentDetailsList.Add(pd);
                        //authUser.PaymentDetails.Add(pd);
                        if (!String.IsNullOrEmpty(epaperTerm.Trim()))
                            ViewData["ePaperStartDate"] = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.SubscriptionID == 0).StartDate;

                        if(!String.IsNullOrEmpty(printTerm.Trim()))
                            ViewData["printStartDate"] = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.SubscriptionID == 0).StartDate;


                        Subscriber_Address mailingAddress = GetSubscriberAddress();
                        ViewData["savedAddress"] = true;
                        ViewData["savedAddressData"] = JsonConvert.SerializeObject(mailingAddress);

                        return View("PaymentDetails", pd);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }

                }
            }

            return View(data);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveDeliveryAddress(FormCollection form)
        {
            DeliveryAddress deliveryAddress = GetSubscriberDeliveryAddress();

            deliveryAddress.AddressType = "D";
            //deliveryAddress.CreatedAt = DateTime.Now;


            if (!(bool.Parse(form["SameAsMailing"].Split(',').FirstOrDefault())))
            {
                deliveryAddress.AddressLine1 = form["DeliveryAddress.AddressLine1"];
                deliveryAddress.AddressLine2 = form["DeliveryAddress.AddressLine2"];
                deliveryAddress.CityTown = form["DeliveryAddress.CityTown"].Replace("-", ". ").Replace("_", " & ");
                deliveryAddress.StateParish = form["DeliveryAddress.StateParish"].Replace("-", ". ").Replace("_", " & ");
                deliveryAddress.CountryCode = form["DeliveryAddress.CountryCode"];
            }
            else 
            {
                Subscriber_Address mailingAddress = GetSubscriberAddress();
                deliveryAddress.AddressLine1 = mailingAddress.AddressLine1;
                deliveryAddress.AddressLine2 = mailingAddress.AddressLine2;
                deliveryAddress.CityTown = mailingAddress.CityTown;
                deliveryAddress.StateParish = mailingAddress.StateParish;
                deliveryAddress.CountryCode = mailingAddress.CountryCode;
            }

            ViewData["savedAddressData"] = deliveryAddress;
            ViewData["savedAddress"] = true;
            var result = new JsonResult();
            result.Data = deliveryAddress;
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PaymentDetails(PaymentDetails data, string prevBtn, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();

            var cardType = data.CardType;

            if (prevBtn != null)
            {

                try
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    Subscriber objSub = GetSubscriber();
                    Subscriber_Epaper objEp = GetEpaperDetails();
                    Subscriber_Print objPr = GetPrintDetails();
                    UserLocation objLoc = GetSubscriberLocation();
                    var market = (objLoc.Country_Code == "JM") ? "Local" : "International";


                    SubscriptionDetails sd = new SubscriptionDetails
                    {
                        StartDate = objEp.StartDate,
                        RateID = objEp.RateID,
                        DeliveryInstructions = objPr.DeliveryInstructions,
                        NewsletterSignUp = objSub.Newsletter ?? false,
                        NotificationEmail = objEp.NotificationEmail,
                        SubType = objEp.SubType,
                        RatesList = db.printandsubrates.Where(x => x.Market == market).Where(x => x.Active == true).ToList(),
                        Market = market
                    };

                    return View("SubscriptionInfo", sd);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }

            }

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {

                    try
                    {
                        //get all session variables
                        //AuthSubcriber authUser = GetAuthSubscriber();
                        Subscriber objSub = GetSubscriber();
                        //Subscriber_Address objAdd = GetSubscriberAddress();
                        //Subscriber_Epaper objE = GetEpaperDetails();
                        //Subscriber_Print objP = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        //ApplicationUser user = GetAppUser();
                        
                        objTran.CardType = data.CardType;
                        objTran.CardOwner = data.CardOwner;
                        objTran.TranxAmount = (double)data.CardAmount;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }

                    dynamic summary = await ChargeCard(data);
                    return Json(data);

                }
            }

            ViewBag.CountryList = GetCountryList();
            return View(data);
        }

        public async Task<bool> SaveSubscriptionInfoAsync(AuthSubcriber authUser)
        {

            //AuthSubcriber authUser = new AuthSubcriber();
            var emailAddress = authUser.EmailAddress;
            //get all session variables
            Subscriber objSub = new Subscriber { 
                FirstName = authUser.FirstName,
                LastName = authUser.LastName,
                EmailAddress = emailAddress,
                CreatedAt = DateTime.Now,
                IpAddress = Request.UserHostAddress,
                IsActive = true
            };

            try
            {
                AddressDetails mailingAddress = null;

                if (authUser.AddressDetails != null)
                    mailingAddress = authUser.AddressDetails.FirstOrDefault(x => x.AddressType == "M" && x.AddressID == 0);
                
                Subscriber_Address objAdd = new Subscriber_Address();
                if (mailingAddress != null)
                {
                    objAdd = new Subscriber_Address
                    {
                        EmailAddress = emailAddress,
                        //address type M - Mailing --- B - Billing
                        AddressType = "M",
                        AddressLine1 = mailingAddress.AddressLine1,
                        AddressLine2 = mailingAddress.AddressLine2,
                        CityTown = mailingAddress.CityTown,
                        StateParish = mailingAddress.StateParish,
                        ZipCode = mailingAddress.ZipCode,
                        CountryCode = mailingAddress.CountryCode,
                        CreatedAt = DateTime.Now
                    };
                }
                AddressDetails deliveryAddress = null;
                if (authUser.AddressDetails != null)
                    deliveryAddress = authUser.AddressDetails.FirstOrDefault(x => x.AddressType == "D" && x.AddressID == 0);

                Subscriber_Address objDelAdd = new Subscriber_Address();
                if (deliveryAddress != null)
                {
                    objDelAdd = new Subscriber_Address
                    {
                        EmailAddress = emailAddress,
                        //address type M - Mailing --- B - Billing
                        AddressType = "D",
                        AddressLine1 = deliveryAddress.AddressLine1,
                        AddressLine2 = deliveryAddress.AddressLine2,
                        CityTown = deliveryAddress.CityTown,
                        StateParish = deliveryAddress.StateParish,
                        CountryCode = deliveryAddress.CountryCode,
                        CreatedAt = DateTime.Now
                    };
                }

                SubscriptionDetails epaperSub = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubscriptionID == 0 && (x.RateType == "Epaper" || x.RateType == "Bundle" && x.SubType == "Epaper"));
                Subscriber_Epaper objE = new Subscriber_Epaper();
                if (epaperSub != null)
                {
                    objE = new Subscriber_Epaper
                    {
                        CreatedAt = DateTime.Now,
                        StartDate = epaperSub.StartDate,
                        EndDate = epaperSub.EndDate,
                        RateID = epaperSub.RateID,
                        SubType = epaperSub.SubType,
                        IsActive = false,
                        EmailAddress = emailAddress,
                        NotificationEmail = epaperSub.NotificationEmail,
                        PlanDesc = epaperSub.RateDescription
                    };
                }

                SubscriptionDetails printSub = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubscriptionID == 0 && (x.RateType == "Print" || x.RateType == "Bundle" && x.SubType == "Print"));
                Subscriber_Print objP = new Subscriber_Print();
                if (printSub != null)
                {
                    objP = new Subscriber_Print
                    {
                        StartDate = printSub.StartDate,
                        EndDate = printSub.EndDate,
                        RateID = printSub.RateID,
                        IsActive = false,
                        EmailAddress = emailAddress,
                        DeliveryInstructions = printSub.DeliveryInstructions,
                        CreatedAt = DateTime.Now,
                        PlanDesc = printSub.RateDescription
                    };
                }

                PaymentDetails trxDetails = authUser.PaymentDetails.Where(x => x.TransactionID == 0).OrderByDescending(t => t.TranxDate).FirstOrDefault();
                Subscriber_Tranx objTran = new Subscriber_Tranx();
                if (trxDetails != null)
                {
                    objTran = new Subscriber_Tranx
                    {
                        //save transaction
                        EmailAddress = emailAddress,
                        TranxDate = DateTime.Now,
                        RateID = trxDetails.RateID,
                        IpAddress = Request.UserHostAddress,
                        CardOwner = trxDetails.CardOwner,
                        CardType = trxDetails.CardType,
                        CardExp = trxDetails.CardExp,
                        CardLastFour = trxDetails.CardNumberLastFour,
                        TranxAmount = (double)trxDetails.CardAmount,
                        OrderID = trxDetails.OrderNumber,
                        EnrolledIn3DSecure = true,
                        PlanDesc = trxDetails.RateDescription
                    };
                }


                string SubscriberID = authUser.SubscriberID;
                int addressID = 0;
                var rateID = trxDetails.RateID;

                IdentityResult createAccount = new IdentityResult();
                ApplicationUser newAccount = new ApplicationUser();
                //save subscribers
               
                if (!trxDetails.IsExtension)
                {
                    newAccount = new ApplicationUser
                    {
                        UserName = emailAddress,
                        Email = emailAddress,
                        Subscriber = objSub
                    };

                    //create application user
                    createAccount = (authUser.Login != null) ? await UserManager.CreateAsync(newAccount) : await UserManager.CreateAsync(newAccount, authUser.Password);

                    //
                    //create oauthuser login
                    if (authUser.Login != null)
                        await UserManager.AddLoginAsync(newAccount.Id, authUser.Login);
                }   
                //get Subscriber ID
                SubscriberID = (!String.IsNullOrEmpty(SubscriberID)) ? SubscriberID : newAccount.Id;

                if (createAccount.Succeeded)
                {
                    var userRole = (newAccount.Email.Contains("jamaicaobserver.com")) ? "Staff" : "Subscriber";
                    //assign User Role
                    createAccount = await UserManager.AddToRoleAsync(SubscriberID, userRole);
                }
                AddErrors(createAccount);

                //save to DB
                using (var context = new ApplicationDbContext())
                {
                    //save address
                    if(!trxDetails.IsExtension)
                    {
                        if (mailingAddress != null)
                        {
                            objAdd.SubscriberID = SubscriberID;
                            context.subscriber_address.Add(objAdd);
                            await context.SaveChangesAsync();
                        }
                        
                    }

                    //save delivery address
                    if (deliveryAddress != null)
                    {
                        objDelAdd.SubscriberID = SubscriberID;
                        context.subscriber_address.Add(objDelAdd);
                        await context.SaveChangesAsync();
                    }

                    //get Address ID
                    addressID = objAdd.AddressID;

                    //update subscribers table w/ address ID
                    if (!trxDetails.IsExtension) 
                    {
                        var result = context.subscribers.SingleOrDefault(b => b.SubscriberID == SubscriberID);
                        if (result != null)
                        {
                            result.AddressID = addressID;
                            await context.SaveChangesAsync();
                        }
                    }

                    var selectedPlan = context.printandsubrates.SingleOrDefault(b => b.Rateid == rateID);

                    //save transaction
                    objTran.SubscriberID = SubscriberID;
                    context.subscriber_tranx.Add(objTran);
                    await context.SaveChangesAsync();

                    var existingEpaperPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Epaper" && x.isActive == true);
                    var existingPrintPlan = authUser.SubscriptionDetails.FirstOrDefault(x => x.SubType == "Print" && x.isActive == true);

                    //save based on subscription
                    if (selectedPlan != null)
                    {
                        switch (selectedPlan.Type)
                        {
                            case "Print":
                                if (existingPrintPlan != null)
                                {
                                    //update existing subscription
                                    var result = context.subscriber_print.FirstOrDefault(x => x.SubscriberID == SubscriberID && x.IsActive == true);
                                    if (result != null)
                                    {
                                        //result.DeliveryInstructions = objP.DeliveryInstructions;
                                        //result.RateID = objP.RateID;
                                        result.IsActive = false;
                                        await context.SaveChangesAsync();
                                    }

                                    //save print subscription
                                    objP.AddressID = objDelAdd.AddressID;
                                    objP.SubscriberID = SubscriberID;
                                    objP.OrderNumber = objTran.OrderID;
                                    objP.PlanDesc = selectedPlan.RateDescr;
                                    objP.StartDate = result.StartDate;
                                    objP.EndDate = result.EndDate.AddDays((double)selectedPlan.PrintTerm * 7);
                                    context.subscriber_print.Add(objP);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    //save print subscription
                                    objP.AddressID = objDelAdd.AddressID;
                                    objP.SubscriberID = SubscriberID;
                                    objP.PlanDesc = selectedPlan.RateDescr;
                                    objP.OrderNumber = objTran.OrderID;
                                    context.subscriber_print.Add(objP);
                                    await context.SaveChangesAsync();
                                }
                                break;

                            case "Epaper":
                                if (existingEpaperPlan != null)
                                {
                                    //update existing subscription
                                    var result = context.subscriber_epaper.FirstOrDefault(x => x.SubscriberID == SubscriberID && x.IsActive == true);
                                    if (result != null)
                                    {
                                        //result.RateID = (int)rateID;
                                        //result.EndDate = objE.EndDate.AddDays((double)selectedPlan.ETerm);
                                        result.IsActive = false;
                                        await context.SaveChangesAsync();
                                    }
                                    //save epaper subscription
                                    var subType = (selectedPlan.RateDescr.Contains("Coupon") || selectedPlan.RateDescr.Contains("Free")) ? SubscriptionType.Complimentary.ToString() : SubscriptionType.Paid.ToString();
                                    objE.SubType = subType;
                                    objE.SubscriberID = SubscriberID;
                                    objE.OrderNumber = objTran.OrderID;
                                    objE.PlanDesc = selectedPlan.RateDescr;
                                    objE.StartDate = result.StartDate;
                                    objE.EndDate = result.EndDate.AddDays((double)selectedPlan.ETerm);
                                    context.subscriber_epaper.Add(objE);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    //save epaper subscription
                                    var subType = (selectedPlan.RateDescr.Contains("Coupon") || selectedPlan.RateDescr.Contains("Free")) ? SubscriptionType.Complimentary.ToString() : SubscriptionType.Paid.ToString();
                                    objE.SubType = subType;
                                    objE.SubscriberID = SubscriberID;
                                    objE.PlanDesc = selectedPlan.RateDescr;
                                    objE.OrderNumber = objTran.OrderID;
                                    context.subscriber_epaper.Add(objE);
                                    await context.SaveChangesAsync();
                                }
                                break;

                            case "Bundle":
                                if (existingPrintPlan != null)
                                {
                                    //update existing subscription
                                    var result = context.subscriber_print.FirstOrDefault(x => x.SubscriberID == SubscriberID && x.IsActive == true);
                                    if (result != null)
                                    {
                                        //result.DeliveryInstructions = objP.DeliveryInstructions;
                                        //result.RateID = objP.RateID;
                                        //result.EndDate = objP.EndDate.AddDays((double)selectedPlan.PrintTerm * 7);
                                        result.IsActive = false;
                                        await context.SaveChangesAsync();
                                    }
                                    //save print subscription
                                    objP.AddressID = objDelAdd.AddressID;
                                    objP.SubscriberID = SubscriberID;
                                    objP.PlanDesc = selectedPlan.RateDescr;
                                    objP.OrderNumber = objTran.OrderID;
                                    objP.StartDate = result.StartDate;
                                    objP.EndDate = result.EndDate.AddDays((double)selectedPlan.PrintTerm * 7);
                                    context.subscriber_print.Add(objP);
                                    await context.SaveChangesAsync();

                                }
                                else
                                {
                                    //save print subscription
                                    objP.AddressID = objDelAdd.AddressID;
                                    objP.SubscriberID = SubscriberID;
                                    objP.PlanDesc = selectedPlan.RateDescr;
                                    objP.OrderNumber = objTran.OrderID;
                                    context.subscriber_print.Add(objP);
                                    await context.SaveChangesAsync();
                                }

                                if (existingEpaperPlan != null)
                                {
                                    //update existing subscription
                                    var result = context.subscriber_epaper.FirstOrDefault(x => x.SubscriberID == SubscriberID && x.IsActive == true);
                                    if (result != null)
                                    {
                                        //result.RateID = (int)rateID;
                                        //result.EndDate = objE.EndDate.AddDays((double)selectedPlan.ETerm);
                                        result.IsActive = false;
                                        await context.SaveChangesAsync();
                                    }
                                    //save epaper subscription
                                    var subType = (selectedPlan.RateDescr.Contains("Coupon") || selectedPlan.RateDescr.Contains("Free")) ? SubscriptionType.Complimentary.ToString() : SubscriptionType.Paid.ToString();
                                    objE.SubType = subType;
                                    objE.SubscriberID = SubscriberID;
                                    objE.PlanDesc = selectedPlan.RateDescr;
                                    objE.OrderNumber = objTran.OrderID;
                                    objE.StartDate = result.StartDate;
                                    objE.EndDate = result.EndDate.AddDays((double)selectedPlan.ETerm);
                                    context.subscriber_epaper.Add(objE);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    //save epaper subscription
                                    var subType = (selectedPlan.RateDescr.Contains("Coupon") || selectedPlan.RateDescr.Contains("Free")) ? SubscriptionType.Complimentary.ToString() : SubscriptionType.Paid.ToString();
                                    objE.SubType = subType;
                                    objE.SubscriberID = SubscriberID;
                                    objE.PlanDesc = selectedPlan.RateDescr;
                                    objE.OrderNumber = objTran.OrderID;
                                    context.subscriber_epaper.Add(objE);
                                    await context.SaveChangesAsync();
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }

                await CompleteTransactionProcess(int.Parse(rateID.ToString()), objTran.OrderID, emailAddress);


                return true;
                    //RemoveSubscriber();
                //}

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            
            return false;
        }

        [AllowAnonymous]
        public ActionResult RedeemCoupon()
        {

            List<SelectListItem> Addressparishes = GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RedeemCoupon(AuthSubcriber authUser, string nextBtn)
        {
            authUser.SubscriptionDetails = new List<SubscriptionDetails>();
            authUser.PaymentDetails = new List<PaymentDetails>();

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var isExist = IsEmailExist(authUser.EmailAddress);
                        if (isExist)
                        {
                            ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                            return View(authUser);
                        }

                        authUser.AddressDetails.FirstOrDefault().AddressType = "M";
                        using (var context = new ApplicationDbContext())
                        {
                            var result = context.coupons.FirstOrDefault(x => x.CouponCode == authUser.RedeemCode && x.UsedDate == null && x.ExpiryDate >= DateTime.Now);
                            if (result != null)
                            {
                                var endDate = DateTime.Now.AddDays((double)result.SubDays);

                                SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                                {
                                    StartDate = DateTime.Now,
                                    EndDate = endDate,
                                    RateID = 61,
                                    SubType = "Epaper",
                                    NotificationEmail = false,
                                    RateType = "Epaper",
                                    RateDescription = "ePaper Coupon",
                                    isActive = true

                                };
                                authUser.SubscriptionDetails.Add(subscriptionDetails);

                                PaymentDetails pd = new PaymentDetails
                                {
                                    RateID = 61,
                                    RateDescription = "ePaper Coupon",
                                    RateTerm = "",
                                    Currency = "JMD",
                                    CardAmount = 0,
                                    SubType = "Epaper",
                                    CardType = "N/A",
                                    //test data
                                    CardOwner = authUser.FirstName + " " + authUser.LastName,
                                    OrderNumber = "Complimentary : Coupon",
                                };
                                authUser.PaymentDetails.Add(pd);


                                JOL_UserSession session;
                                var sessionRepository = new SessionRepository();
                                session = sessionRepository.CreateObject(authUser);
                                var isSaved = await sessionRepository.AddOrUpdate(pd.OrderNumber, session, 61, authUser);

                                var saved = await SaveSubscriptionInfoAsync(authUser);
                                if (saved)
                                {
                                    //set coupon to used
                                    var closeCoupon = context.coupons.FirstOrDefault(x => x.CouponCode == authUser.RedeemCode);
                                    if (closeCoupon != null)
                                    {
                                        closeCoupon.UsedDate = DateTime.Now;
                                        await context.SaveChangesAsync();
                                    }

                                    //await CompleteTransactionProcess(pd.RateID, pd.OrderNumber, authUser.EmailAddress);
                                    return View("PaymentSuccess", authUser);
                                }
                            }
                            ModelState.AddModelError("InvalidCode", "Invalid Code");

                        }
                    }
                    catch (Exception ex)
                    {

                        LogError(ex);
                    }
                    
                }
            }

            List<SelectListItem> Addressparishes = GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            ViewBag.CountryList = GetCountryList();

            return View();
        }

        [AllowAnonymous]
        public ActionResult FreeMonth()
        {
            AuthSubcriber authUser = new AuthSubcriber();

            authUser.RedeemCode = "READFORFREE30DAYS";
            return View(authUser);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FreeMonth(AuthSubcriber authUser, string nextBtn)
        {
            authUser.SubscriptionDetails = new List<SubscriptionDetails>();
            authUser.PaymentDetails = new List<PaymentDetails>();

            if (nextBtn != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var isExist = IsEmailExist(authUser.EmailAddress);
                        if (isExist)
                        {
                            ModelState.AddModelError("EmailExist", "Email address is already assigned. Please use forget password option to log in");
                            return View(authUser);
                        }

                        using (var context = new ApplicationDbContext())
                        {
                            var result = context.coupons.FirstOrDefault(x => x.CouponCode == authUser.RedeemCode && x.UsedDate == null && x.ExpiryDate >= DateTime.Now);
                            if (result != null)
                            {
                                var market = GetUserLocation().Country_Code;
                                var rateID = (market == "JM") ? 32 : 2;
                                var currency = (market == "JM") ? "JMD" : "USD";

                                var selectedPlan = context.printandsubrates.FirstOrDefault(x => x.Rateid == rateID);


                                var endDate = DateTime.Now.AddDays((double)result.SubDays);

                                SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                                {
                                    StartDate = DateTime.Now,
                                    EndDate = endDate,
                                    RateID = rateID,
                                    SubType = "Epaper",
                                    NotificationEmail = false,
                                    RateType = "Epaper",
                                    RateDescription = "1 Month (30 Days) - Free"

                                };
                                authUser.SubscriptionDetails.Add(subscriptionDetails);

                                PaymentDetails pd = new PaymentDetails
                                {
                                    RateID = rateID,
                                    RateDescription = "1 Month (30 Days) - Free",
                                    RateTerm = "",
                                    Currency = currency,
                                    CardAmount = 0,
                                    SubType = "Epaper",
                                    CardType = "N/A",
                                    //test data
                                    CardOwner = authUser.FirstName + " " + authUser.LastName,
                                    OrderNumber = "FreeTrial:Coupon",
                                };
                                authUser.PaymentDetails.Add(pd);


                                JOL_UserSession session;
                                var sessionRepository = new SessionRepository();
                                session = sessionRepository.CreateObject(authUser);
                                var isSaved = await sessionRepository.AddOrUpdate(pd.OrderNumber, session, rateID, authUser);
                                if (isSaved)
                                {
                                    var saved = await SaveSubscriptionInfoAsync(authUser);
                                    if (saved)
                                    {
                                        return View("PaymentSuccess", authUser);
                                    }
                                }
                            }

                            ModelState.AddModelError("InvalidCode", "Invalid Code");

                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        return View(authUser);
                    }
                   
                }
            }
         
            return View(authUser);
        }

        [AllowAnonymous]
        public ActionResult PaymentSuccess()
        {
            AuthSubcriber authSubcriber = GetAuthSubscriber();
            RemoveSubscriber();
            Session["auth_subscriber"] = authSubcriber;

            return View("PaymentSuccess", authSubcriber);
        }

        public async Task<JsonResult> ChargeCard(PaymentDetails paymentDetails)
        {
            TransactionSummary summary = new TransactionSummary();
            TransactionSummary encrypted = new TransactionSummary();

            try
            {
                AuthSubcriber clientData = GetAuthSubscriber(); //pull exisiting data
                var processedClientData = new AuthSubcriber();
                var deliveryAddress = clientData.AddressDetails.FirstOrDefault(x => x.AddressType == "D");
                List<PaymentDetails> paymentDetailsList = new List<PaymentDetails>();
               
                // Setup card processor.
                var cardProcessor = new CardProcessor();
                var transactionDetails = new TransactionDetails();
                var cardDetails = new CardDetails();
                var billingDetails = new BillingDetails();
                var shippingDetails = new ShippingDetails();
                //TODO: Recurring Payments Setup
                transactionDetails.TransactionCode = 8; //2048 – Subsequent Recurring – future recurring payments : 4096 – Initial Recurring – First Payment in a recurring cycle : 8192 - **HOST SPECIFIC – Initial Recurring for “Free - Trials”
                var recurringDetails = new RecurringDetails
                {
                    IsRecurring = true,
                    ExecutionDate = "20221217", //jan 5, 2023
                    Frequency = "D", // “D” – Daily : “W” – Weekly : “F” – Fortnightly / Every 2 weeks : “M” – Monthly : “E” – Bi - Monthly, “Q” – Quarterly, “Y” – Yearly
                    NumberOfRecurrences = 3
                };

                paymentDetails.TranxDate = DateTime.Now;

                var cardNumber = Regex.Replace(paymentDetails.CardNumber, @"\s+", "");
                // Save the stripped card numbers
                string sCardType = Util.GetCreditCardType(cardNumber);
                Dictionary<string, string> result = Util.StripCardNumber(cardNumber);
                if (result != null)
                {
                    paymentDetails.CardNumberLastFour = result["lastFour"];
                }

                cardDetails.CardCVV2 = paymentDetails.CardCVV;
                cardDetails.CardNumber = cardNumber;
                var cardExpiry = paymentDetails.CardExp.Split('/');
                var Year = DateTime.Now.Year.ToString();
                var cardExpiryYear = Year.Substring(0,2) + cardExpiry[1];
                cardDetails.CardExpiryDate = CardUtils.FormatExpiryDate(cardExpiry[0], cardExpiryYear);

                transactionDetails.Amount = CardUtils.ZeroPadAmount(paymentDetails.CardAmount);
                transactionDetails.Currency = paymentDetails.Currency;

                //Setup Order Number
                string milli = Util.ZeroPadNumber(3, DateTime.Now.Millisecond); 
                // Pad to 10 digits.
                var paddedRateKey = Util.ZeroPadNumber(3, paymentDetails.RateID);
                // 2 Character Sub Type
                var reSubType = paymentDetails.SubType.ToUpper().Substring(0,2);

                transactionDetails.OrderNumber = $"{reSubType}{"-"}{DateTime.Now.ToString("yyyyMMddhhmmssfffff")}{"-"}{paymentDetails.Currency}{"-"}{paddedRateKey}";

                paymentDetails.OrderNumber = transactionDetails.OrderNumber;
                paymentDetailsList.Add(paymentDetails);
                processedClientData.PaymentDetails = paymentDetailsList;
                clientData.PaymentDetails.RemoveAll(x => x.TransactionID == 0);
                clientData.PaymentDetails.Add(paymentDetails);

                // Update Billing Details
                billingDetails.BillToAddress = paymentDetails.BillingAddress.AddressLine1;
                billingDetails.BillToAddress2 = paymentDetails.BillingAddress.AddressLine2;
                billingDetails.BillToCity = paymentDetails.BillingAddress.CityTown;

                // Update Shipping Details
                if (deliveryAddress != null)
                {
                    shippingDetails.ShipToFirstName = clientData.FirstName;
                    shippingDetails.ShipToLastName = clientData.LastName;
                    shippingDetails.ShipToAddress = deliveryAddress.AddressLine1;
                    shippingDetails.ShipToAddress2 = deliveryAddress.AddressLine2;
                    shippingDetails.ShipToCity = deliveryAddress.CityTown;
                }

                shippingDetails = (deliveryAddress != null) ? shippingDetails : null;

                if (!await CardUtils.IsCardCharged(transactionDetails.OrderNumber))
                {
                    // Clear sensitive data and save for later retrieval.
                    paymentDetails.CardCVV = "";
                    paymentDetails.CardNumber = "";

                    JOL_UserSession session;
                    var sessionRepository = new SessionRepository();
                    session = sessionRepository.CreateObject(clientData);
                    var isSaved = await sessionRepository.AddOrUpdate(transactionDetails.OrderNumber, session, paymentDetails.RateID, clientData);

                    summary = await cardProcessor.ChargeCard(cardDetails, transactionDetails, billingDetails, shippingDetails, null);

                    // 3D Secure (Visa/MasterCard) Flow
                    if (!string.IsNullOrWhiteSpace(summary.Merchant3DSResponseHtml))
                    {
                        // Return the payment object.
                        //encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(summary), "E");
                        int cacheExpiryDuration = int.Parse(ConfigurationManager.AppSettings["cacheExpiryDuration"] ?? "15");
                        tempData.Cache.Add(transactionDetails.OrderNumber, $"{clientData.SubscriberID}|{paymentDetails.RateID}|{transactionDetails.Amount}|{clientData.EmailAddress}", new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheExpiryDuration) });

                        encrypted = summary;
                        //return view
                        paymentDetails.TransactionSummary = summary;
                        // await SaveSubscriptionAsync(paymentDetails);
                        //return View("PaymentDetails", paymentDetails);
                        return Json(paymentDetails);
                    }
                    else
                    {
                        // KeyCard Flow
                        encrypted = summary;

                        var transStatus = encrypted.TransactionStatus;
                        switch (transStatus.Status)
                        {
                            case PaymentStatus.Successful:
                                //await SaveSubscriptionAsync();
                                paymentDetails.IsMadeLiveSuccessful = true;
                                paymentDetails.TransactionSummary = summary;
                                paymentDetails.OrderNumber = transactionDetails.OrderNumber;
                                paymentDetails.ConfirmationNumber = summary.ReferenceNo;
                                paymentDetails.AuthorizationCode = summary.AuthCode;
                                paymentDetails.IsMadeLiveSuccessful = true;

                                await SaveSubscriptionInfoAsync(clientData);
                                RemoveSubscriber();
                                // return View("PaymentSuccess");
                                return Json(paymentDetails);


                            case PaymentStatus.Failed:
                                // TODO: Add Friendly Message property to Card Processor to display to user.
                                //return View("PaymentDetails", paymentDetails);
                                return Json(paymentDetails);

                            case PaymentStatus.GatewayError:
                            case PaymentStatus.InternalError:
                                break;
                        }
                        //TODO: Remove only cookies?
                        RemoveSubscriber();

                        paymentDetails.TransactionSummary = summary;
                        await SaveSubscriptionInfoAsync(clientData);
                        //return View("PaymentDetails", paymentDetails);
                        return Json(paymentDetails);


                    }
                }
                else
                {
                    // Clear sensitive data and save for later retrieval.
                    // Save data of previous charge to our database for later processing.
                    paymentDetails.CardCVV = "";
                    paymentDetails.CardNumber = "";
                    var transactionResponse = await cardProcessor.GetGatewayTransactionStatus(transactionDetails.OrderNumber);
                    var transSummary = cardProcessor.GetTransactionSummary(transactionResponse);
                    paymentDetails.AuthorizationCode = transSummary.AuthCode;
                    paymentDetails.ConfirmationNumber = transSummary.ReferenceNo;
                    //TODO: Not sure if 
                    paymentDetails.TransactionSummary = summary;
                    await SaveSubscriptionInfoAsync(clientData);
                    return Json(paymentDetails);
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
                //return View("PaymentDetails");
            }
            // Something went wrong to get here.
            // return Ok();
            return Json(paymentDetails);


        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CompleteTransaction(FormCollection form)
        {
            try
            {

                var sessionRepository = new SessionRepository();
                JOL_UserSession existing = new JOL_UserSession();
                // Retrieve data that was saved before 3DS processing.
                await Task.FromResult(0);
                var subscriberID = "";
                var rateID = "";
                //var 
                var threedsparams = new ThreeDSParams();
                var cardProcessor = new CardProcessor();

                threedsparams.MerID = form["MerID"];
                threedsparams.AcqID = form["AcqID"];
                threedsparams.OrderID = form["OrderID"];
                threedsparams.ResponseCode = form["ResponseCode"];
                threedsparams.ReasonCode = form["ReasonCode"];
                threedsparams.ReasonCodeDesc = form["ReasonCodeDesc"];
                threedsparams.ReferenceNo = form["ReferenceNo"];
                threedsparams.PaddedCardNo = form["PaddedCardNo"];
                threedsparams.AuthCode = form["AuthCode"];
                threedsparams.CVV2Result = form["CVV2Result"];
                threedsparams.AuthenticationResult = form["AuthenticationResult"];
                threedsparams.CAVVValue = form["CAVVValue"];
                threedsparams.ECIIndicator = form["ECIIndicator"];
                threedsparams.TransactionStain = form["TransactionStain"];
                threedsparams.OriginalResponseCode = form["OriginalResponseCode"];
                threedsparams.Signature = form["Signature"];
                threedsparams.SignatureMethod = form["SignatureMethod"];

                var savedKeys = tempData.Cache.Get(threedsparams.OrderID);


                string[] keys = savedKeys.ToString().Split('|');
                subscriberID = (keys[0]);
                rateID = (keys[1]);
                var email = keys[3];
                Session["userEmail"] = email;

                var originalAmount = CardUtils.GetAmountFromString(keys[2]);
                threedsparams.Amount = originalAmount;


                // Free up cache
                tempData.Cache.Remove(threedsparams.OrderID);
                var summary = CardProcessor.GetGateway3DSecureResponse(threedsparams);
                var existingSession = await sessionRepository.Get(email);
                var customerData = JsonConvert.DeserializeObject<AuthSubcriber>(existingSession.RootObject);
                Session["auth_subscriber"] = customerData;
                
                PaymentDetails paymentDetails = customerData.PaymentDetails.FirstOrDefault();
                paymentDetails.TransactionSummary = summary;

                var errMsg = ConfigurationManager.AppSettings["CreditCardError"];
                // Setup messages for failure and passes.
                switch (summary.TransactionStatus.Status)
                {
                    case PaymentStatus.Successful:
                        //save subscription
                        await SaveSubscriptionInfoAsync(customerData);
                        
                        // Set to 15 minutes by default if not found
                        int cacheExpiryDuration = int.Parse(ConfigurationManager.AppSettings["cacheExpiryDuration"] ?? "15");
                        // Repopulate cache for new flow.
                        tempData.Cache.Add(summary.OrderId, $"{email}|{rateID}", new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheExpiryDuration) });
                        return RedirectToAction("PaymentSuccess");

                    case PaymentStatus.Failed:
                        summary.FriendlyErrorMessages.Add(errMsg);
                        ViewBag.CountryList = GetCountryList();
                        ClearDBSession(email);
                        return View("PaymentDetails", paymentDetails);

                    case PaymentStatus.InternalError:
                    case PaymentStatus.GatewayError:
                        summary.FriendlyErrorMessages.Add(errMsg);
                        ViewBag.CountryList = GetCountryList();
                        ClearDBSession(email);
                        return View("PaymentDetails", paymentDetails);

                    default:
                        summary.FriendlyErrorMessages.Add(errMsg);
                        ViewBag.CountryList = GetCountryList();
                        ClearDBSession(email);
                        return View("PaymentDetails", paymentDetails);
                        
                }
                // return Ok();
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }

            return View("PaymentDetails");

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> CompleteTransactionProcess(int rateID, string orderNumber, string emailAddress)
        {
            var appDbOther = new ApplicationDbContext();
            var cardProcessor = new CardProcessor();

            try
            {
                var savedTransaction = await appDbOther.subscriber_tranx.FirstOrDefaultAsync(t => t.OrderID == orderNumber && t.EmailAddress == emailAddress);
                Session["userEmail"] = emailAddress;

                if (savedTransaction != null)
                {
                    if (!savedTransaction.IsMadeLiveSuccessful)
                    {
                        string email = emailAddress;
                        var processedClientData = new AuthSubcriber();

                        // Retrieve data that was saved before 3DS processing.
                        var sessionRepository = new SessionRepository();
                        var existing = await sessionRepository.Get(email);
                        var customerData = JsonConvert.DeserializeObject<AuthSubcriber>(existing.RootObject);
                        var currentPolicy = customerData.SubscriptionDetails.FirstOrDefault(p => p.RateID == rateID);

                        PaymentDetails currentTransaction = null;
                        var previousTransactions = customerData.PaymentDetails.Where(p => p.RateID == rateID && p.OrderNumber == orderNumber);
                        if (previousTransactions.Any())
                        {
                            var mostRecent = previousTransactions.Max(tr => tr.TranxDate);
                            currentTransaction = previousTransactions.FirstOrDefault(t => t.TranxDate == mostRecent);
                        }
                        else
                        {
                            currentTransaction = new PaymentDetails();
                        }

                        TransactionSummary transSummary = new TransactionSummary();
                        if (customerData.RedeemCode == null && customerData.AdminCreated == false)
                        {
                            var transactionResponse = await cardProcessor.GetGatewayTransactionStatus(orderNumber);
                            transSummary = cardProcessor.GetTransactionSummary(transactionResponse);

                            currentTransaction.ConfirmationNumber = savedTransaction.ConfirmationNo;
                            //currentTransaction.OrderID = orderID;
                            currentTransaction.AuthorizationCode = savedTransaction.AuthCode;
                        }
                       

                        using (var context = new ApplicationDbContext())
                        {

                            var clientData = (from s in context.subscribers
                                              join st in context.subscriber_tranx on s.SubscriberID equals st.SubscriberID
                                              join se in context.subscriber_epaper on st.OrderID equals se.OrderNumber into seGroup
                                              from se in seGroup.DefaultIfEmpty()
                                              join sp in context.subscriber_print on st.OrderID equals sp.OrderNumber into spGroup
                                              from sp in spGroup.DefaultIfEmpty()
                                              select new { subscriber = s, eSubscription = se, pSubscription = sp, tranasction = st })
                                        .FirstOrDefault(x => x.tranasction.OrderID == orderNumber);

                            //var clientData = context.subscribers
                            //    .Include(s => s.Subscriber_Tranx)
                            //    .Include(s => s.Subscriber_Epaper)
                            //    .Include(s => s.Subscriber_Print)
                            //    //.FirstOrDefault(s => s.Subscriber_Tranx.FirstOrDefault(x => x.EmailAddress == emailAddress).OrderID == orderNumber);
                            //    .FirstOrDefault(u => u.EmailAddress == emailAddress && u.Subscriber_Tranx.FirstOrDefault(x => x.IsMadeLiveSuccessful == false).OrderID == orderNumber);

                            if (clientData != null)
                            {
                                if (customerData.RedeemCode == null && customerData.AdminCreated == false)
                                {

                                    //update transaction table with authcode and confirmation no.
                                    Subscriber_Tranx curTransaction = context.subscriber_tranx.FirstOrDefault(x => x.OrderID == orderNumber);
                                    context.Entry(curTransaction).State = EntityState.Modified;

                                    if (curTransaction != null)
                                    {
                                        curTransaction.AuthCode = transSummary.AuthCode;
                                        curTransaction.ConfirmationNo = transSummary.ReferenceNo;
                                        curTransaction.IsMadeLiveSuccessful = true;
                                        await context.SaveChangesAsync();
                                    }
                                }
                                
                                //make subscription active
                                if (currentTransaction.SubType == "Epaper" || currentTransaction.SubType == "Bundle") 
                                { 
                                    Subscriber_Epaper curESubscription = context.subscriber_epaper.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID && x.OrderNumber == orderNumber);
                                    context.Entry(curESubscription).State = EntityState.Modified;

                                    if (curESubscription != null)
                                    {
                                        curESubscription.IsActive = true;
                                        await context.SaveChangesAsync();
                                    }

                                }
                                if (currentTransaction.SubType == "Print" || currentTransaction.SubType == "Bundle") 
                                {
                                    Subscriber_Print curPSubscription = context.subscriber_print.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID && x.OrderNumber == orderNumber);
                                    context.Entry(curPSubscription).State = EntityState.Modified;

                                    if (curPSubscription != null)
                                    {
                                        curPSubscription.IsActive = true;
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }

                            //send confirmation email
                            await SendConfirmationEmail(customerData, currentTransaction.SubType, false);
                            ClearDBSession(emailAddress);
                            return Json(true);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return View("PaymentSuccess");
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult CheckPromoCode(string promoCode)
        {
            var code = promoCode.ToUpper();
            var result = new Dictionary<string, object>();

            AuthSubcriber authUser = GetAuthSubscriber();
            PaymentDetails paymentDetails = authUser.PaymentDetails.FirstOrDefault();
            decimal originalAmount = paymentDetails.CardAmount;

            if (code != paymentDetails.PromoCode) 
            {
                using (var context = new ApplicationDbContext())
                {
                    var discount = context.promocodes.FirstOrDefault(q => q.PromoCode == promoCode.ToUpper());

                    if (discount != null)
                    {
                        if (discount.EndDate > DateTime.Now && discount.Active == true)
                        {
                            originalAmount -= (originalAmount * (decimal)discount.Discount);
                            paymentDetails.PromoCode = discount.PromoCode;
                            paymentDetails.CardAmount = originalAmount;
                            result["msg"] = "Discount Applied";
                            result["data"] = paymentDetails;
                            result["applied"] = true;
                        }
                        else 
                        {
                            result["msg"] = "Expired promo code";
                            result["data"] = paymentDetails;
                            result["applied"] = false;
                        }
                    }
                    else 
                    {
                        result["msg"] = "Invalid Promo Code";
                        result["data"] = paymentDetails;
                        result["applied"] = false;
                    }
                }
            }
            else
            {
                result["msg"] = "Already applied this promo code";
                result["data"] = paymentDetails;
                result["applied"] = true;
            }

            //var result = new JsonResult();
            //result.Data = paymentDetails;
            return Json(result);
            //return RedirectToAction("PaymentDetails", paymentDetails);
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

        [NonAction]
        public bool IsEmailExist(string emailAddress)
        {

            try
            {
                using (ApplicationDbContext dc = new ApplicationDbContext())
                {
                    var v = dc.subscribers.Where(a => a.EmailAddress == emailAddress).FirstOrDefault();
                    return v != null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [NonAction]
        public async Task<bool> SendConfirmationEmail(AuthSubcriber authSubcriber, string SubType, bool send = true) 
        {
            var sent = false;
            if (send)
            {
                var emailAddress = authSubcriber.EmailAddress;
                //send confirmation email
                var user = await UserManager.FindByNameAsync(emailAddress);
                string subject = "Subscription Confirmation (" + SubType + ")";
                string body = RenderViewToString(this.ControllerContext, "~/Views/Emails/ConfirmSubscription.cshtml", authSubcriber);
                await UserManager.SendEmailAsync(user.Id, subject, body);
                sent = true;
            }

            return sent;
        }

        [AllowAnonymous]
        public async Task<bool> MigrateUsers()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var sql = @"SELECT * FROM eusers";

                    var result = await context.Database.SqlQuery<Eusers>(sql).ToListAsync();

                    foreach (var item in result)
                    {

                        var isExist = IsEmailExist(item.Email);
                        if (!isExist) {
                            TimeSpan difference = item.SubscriptionEnd - item.SubscriptionStart;
                            var days = difference.TotalDays;
                            var rateID = 0;

                            //1   International Epaper  1 Month(30 Days)
                            //2   International Epaper  1 Month(30 Days) - Free
                            //3   International Epaper  12 Months(360 Days)
                            //4   International Epaper  6 Months(180 Days)
                            //31  Local Epaper  1 Month(30 Days)
                            //32  Local Epaper  1 Month(30 Days) - Free
                            //33  Local Epaper  1 Year(360 Days)
                            //34  Local Epaper  6 Months(180 Days)
                            if (days >= 360)
                            {
                                if (item.CardAmount < 300)
                                {
                                    rateID = 3;
                                }
                                else
                                {
                                    rateID = 33;
                                }

                            }
                            if (days < 360 && days > 30)
                            {
                                if (item.CardAmount < 300)
                                {
                                    rateID = 4;
                                }
                                else
                                {
                                    rateID = 34;
                                }
                            }
                            if (days <= 30)
                            {
                                if (item.CardAmount < 300 && item.CardAmount > 0)
                                {
                                    rateID = 1;
                                }
                                else
                                {
                                    rateID = 32;
                                }

                                if (item.OrderId.ToLower().Contains("free30"))
                                {
                                    if (item.Country == "jm")
                                        rateID = 32;
                                    else
                                        rateID = 2;
                                }
                            }
                           

                            string SubscriberID = "";
                            int addressID = 0;
                            string planDesc = "";

                            var selectedPlan = context.printandsubrates.SingleOrDefault(b => b.Rateid == rateID);
                            planDesc = selectedPlan.RateDescr;

                            var emailAddress = item.Email;

                            Subscriber objSub = new Subscriber
                            {
                                FirstName = item.Fname,
                                LastName = item.Lname,
                                EmailAddress = emailAddress,
                                CreatedAt = DateTime.Now,
                                IpAddress = item.Ip,
                                IsActive = (item.Active == "yes") ? true : false,
                                Secretquestion = item.SecretQuestion,
                                Secretans = item.SecretAnswer
                            };

                            Subscriber_Address objAdd = new Subscriber_Address
                            {
                                EmailAddress = emailAddress,
                                //address type M - Mailing --- B - Billing
                                AddressType = "M",
                                AddressLine1 = item.Address,
                                //AddressLine2 = mailingAddress.AddressLine2,
                                CityTown = item.City,
                                StateParish = item.State,
                                ZipCode = item.Zip.ToString(),
                                CountryCode = (item.Country != null) ? ConvertTwoLetterNameToThreeLetterName(item.Country) : "JAM",
                                CreatedAt = item.SubscriptionStart
                            };

                            Subscriber_Epaper objE = new Subscriber_Epaper
                            {
                                CreatedAt = item.SubscriptionStart,
                                StartDate = item.SubscriptionStart,
                                EndDate = item.SubscriptionEnd,
                                RateID = rateID, //TODO
                                SubType = (item.Country !=null && item.Country.Contains("complimentary")) ? SubscriptionType.Complimentary.ToString() : SubscriptionType.Paid.ToString(),
                                IsActive = (item.SubscriptionEnd > DateTime.Now && item.Active == "yes") ? true : false,
                                EmailAddress = emailAddress,
                                NotificationEmail = (item.Newsletter == "yes") ? true : false,
                                PlanDesc = planDesc,
                            };

                            Subscriber_Tranx objTran = new Subscriber_Tranx
                            {
                                //save transaction
                                EmailAddress = emailAddress,
                                TranxDate = item.TransactionDate,
                                RateID = rateID,
                                IpAddress = item.Ip,
                                CardOwner = item.CardOwnerName,
                                CardType = (item.CardType.Contains("comp")) ? "COMP" : item.CardType.ToUpper().Trim(),
                                CardExp = "00/00",
                                CardLastFour = "0000",
                                TranxAmount = (double)item.CardAmount,
                                OrderID = item.OrderId,
                                EnrolledIn3DSecure = false,
                                PlanDesc = planDesc
                            };

                            var newAccount = new ApplicationUser
                            {
                                UserName = emailAddress,
                                Email = emailAddress,
                                Subscriber = objSub
                            };
                            //create application user
                            var createAccount = await UserManager.CreateAsync(newAccount, item.Password);
                            if (createAccount.Succeeded)
                            {
                                SubscriberID = newAccount.Id;

                                var userRole = (newAccount.Email.Contains("jamaicaobserver.com")) ? "Staff" : "Subscriber";
                                //assign User Role
                                createAccount = await UserManager.AddToRoleAsync(SubscriberID, userRole);
                                //save address
                                objAdd.SubscriberID = SubscriberID;
                                context.subscriber_address.Add(objAdd);
                                await context.SaveChangesAsync();

                                //get Address ID
                                addressID = objAdd.AddressID;

                                //update subscribers table w/ address ID
                                var sub = context.subscribers.SingleOrDefault(b => b.SubscriberID == SubscriberID);
                                if (sub != null)
                                {
                                    sub.AddressID = addressID;
                                    await context.SaveChangesAsync();
                                }

                                //save epaper
                                objE.SubscriberID = SubscriberID;
                                context.subscriber_epaper.Add(objE);
                                await context.SaveChangesAsync();

                                //save transaction
                                objTran.SubscriberID = SubscriberID;
                                context.subscriber_tranx.Add(objTran);
                                await context.SaveChangesAsync();

                                AddErrors(createAccount);

                            }
                        }
                        

                    }
                    //return true;

                }


                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;

            }

        }
        [AllowAnonymous]
        public ActionResult DeliveryZones()
        {
            List<SelectListItem> Addressparishes = GetParishes();
            ViewBag.Parishes = new SelectList(Addressparishes, "Value", "Text");
            return View();
        }

        private Subscriber GetSubscriber()
        {
            if (Session["subscriber"] == null)
            {
                Session["subscriber"] = new Subscriber();
            }
            return (Subscriber)Session["subscriber"];
        }

        private Subscriber_Address GetSubscriberAddress()
        {
            if (Session["subscriber_address"] == null)
            {
                Session["subscriber_address"] = new Subscriber_Address();
            }
            return (Subscriber_Address)Session["subscriber_address"];
        }

        public DeliveryAddress GetSubscriberDeliveryAddress()
        {
            if (Session["subscriber_del_address"] == null)
            {
                Session["subscriber_del_address"] = new DeliveryAddress();
            }
            return (DeliveryAddress)Session["subscriber_del_address"];
        }

        private Subscriber_Epaper GetEpaperDetails()
        {
            if (Session["subscriber_epaper"] == null)
            {
                Session["subscriber_epaper"] = new Subscriber_Epaper();
            }
            return (Subscriber_Epaper)Session["subscriber_epaper"];
        }

        private Subscriber_Print GetPrintDetails()
        {
            if (Session["subscriber_print"] == null)
            {
                Session["subscriber_print"] = new Subscriber_Print();
            }
            return (Subscriber_Print)Session["subscriber_print"];
        }

        private Subscriber_Tranx GetTransaction()
        {
            if (Session["subscriber_tranx"] == null)
            {
                Session["subscriber_tranx"] = new Subscriber_Tranx();
            }
            return (Subscriber_Tranx)Session["subscriber_tranx"];
        }

        private UserLocation GetSubscriberLocation()
        {
            if (Session["subscriber_location"] == null)
            {
                Session["subscriber_location"] = Util.GetUserLocation();
            }
            return (UserLocation)Session["subscriber_location"];
        }

        private ApplicationUser GetAppUser()
        {
            if (Session["app_user"] == null)
            {
                Session["app_user"] = new ApplicationUser();
            }
            return (ApplicationUser)Session["app_user"];
        }

        private AuthSubcriber GetAuthSubscriber()
        {
            if (Session["auth_subscriber"] == null)
            {
                Session["auth_subscriber"] = new AuthSubcriber();
            }
            return (AuthSubcriber)Session["auth_subscriber"];
        }
        private Dictionary<string, int> GetPreloadSub()
        {
            if (Session["preloadSub"] == null)
            {
                Session["preloadSub"] = new Dictionary<string, int>();
            }
            return (Dictionary<string, int>)Session["preloadSub"];
        }

        public List<SelectListItem> GetCountryList()
        {
       
            if (Session["CountryList"] == null || Session == null)
            {
                //Generate Country list
                List<SelectListItem> CountryList = new List<SelectListItem>();
                CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                foreach (CultureInfo CInfo in CInfoList)
                {
                    RegionInfo R = new RegionInfo(CInfo.LCID);
                    var country = new SelectListItem
                    {
                        Value = R.ThreeLetterISORegionName.ToString(),
                        Text = R.EnglishName
                    };

                    if (!(CountryList.Select(i => i.Value).Contains(country.Value)) && Regex.IsMatch(country.Value, @"^[a-zA-Z]+$"))
                    {
                        CountryList.Add(country);
                    }
                }

                CountryList = CountryList.OrderBy(x => x.Text).ToList();

                Session["CountryList"] = CountryList;
            }

            return (List<SelectListItem>)Session["CountryList"];
        }

        public List<SelectListItem> GetParishes()
        {
            if(Session["Parishes"] == null || Session == null ) 
            {
                var parishes = new List<SelectListItem>();
                var parishList = new List<SelectListItem>();

                using (var context = new ApplicationDbContext())
                {
                    var result = context.delivery_zones.Select(x => x.Parish).Distinct();

                    if (result != null)
                    {
                        foreach (var parish in result)
                        {
                            parishes.Add(new SelectListItem { Text = parish, Value = parish.Replace(". ", "-").Replace(" & ", "_") });
                        }

                        Session["Parishes"] = parishes;

                    }
                }
            }

            return (List<SelectListItem>)Session["Parishes"]; ;
        }
        private void RemoveSubscriber()
        {

            Session.Remove("subscriber");
            Session.Remove("subscriber_address");
            Session.Remove("subscriber_epaper");
            Session.Remove("subscriber_print");
            Session.Remove("subscriber_tranx");
            //Session.Clear();
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }

            using (var context = new ApplicationDbContext())
            {
                var email = Session["userEmail"];
                if (email !=null)
                {
                    var remove = context.JOL_UserSession.Where(x => x.Email == email.ToString()).FirstOrDefault();
                    if (remove != null)
                    {
                        context.JOL_UserSession.Remove(remove);
                        context.SaveChanges();
                    }
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                }

            }
            Dispose(true);
        }

        private void ClearDBSession(string email) {

            using (var context = new ApplicationDbContext())
            {
                var remove = context.JOL_UserSession.Where(x => x.Email == email).FirstOrDefault();
                if (remove != null)
                {
                    context.JOL_UserSession.Remove(remove);
                    context.SaveChanges();
                }
            }
        }

        public void InitializeController(RequestContext context)
        {
            base.Initialize(context);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}