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

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
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
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
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
                return View("Dashboard");
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

        public ActionResult Dashboard()
        {

            string authUser = User.Identity.GetUserId();

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
                    List<SubscriptionDetails> SubscriptionList = new List<SubscriptionDetails>();
                    List<AddressDetails> AddressList = new List<AddressDetails>();
                    List<PaymentDetails> PaymentsList = new List<PaymentDetails>();

                    if (tableData != null)
                    {
                        authSubcriber.FirstName = tableData.FirstName;
                        authSubcriber.LastName = tableData.LastName;

                            //Epaper subscriptions
                            if (tableData.Subscriber_Epaper.Count() > 0)
                            {
                                foreach (var epaper in tableData.Subscriber_Epaper)
                                {
                                    SubscriptionDetails subscriptionDetails = new SubscriptionDetails();
                                    subscriptionDetails.RateID = epaper.RateID;
                                    subscriptionDetails.StartDate = epaper.StartDate;
                                    subscriptionDetails.EndDate = epaper.EndDate;
                                    subscriptionDetails.RateDescription = ratesList.FirstOrDefault(X => X.Rateid == epaper.RateID).RateDescr;
                                    subscriptionDetails.SubType = ratesList.FirstOrDefault(X => X.Rateid == epaper.RateID).Type;
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
                                        SubscriptionDetails subscriptionDetails = new SubscriptionDetails();
                                        subscriptionDetails.RateID = print.RateID;
                                        subscriptionDetails.StartDate = print.StartDate;
                                        subscriptionDetails.EndDate = print.EndDate;
                                        subscriptionDetails.RateDescription = ratesList.FirstOrDefault(X => X.Rateid == print.RateID).RateDescr;
                                        SubscriptionList.Add(subscriptionDetails);
                                    }

                                }

                            }
                            //Addresses
                            if (tableData.Subscriber_Address.Count() > 0)
                            {
                                foreach (var address in tableData.Subscriber_Address)
                                {
                                    AddressDetails addressDetails = new AddressDetails();
                                    addressDetails.AddressLine1 = address.AddressLine1;
                                    addressDetails.AddressLine2 = address.AddressLine2;
                                    addressDetails.AddressType = address.AddressType;
                                    addressDetails.CityTown = address.CityTown;
                                    addressDetails.StateParish = address.StateParish;
                                    addressDetails.CountryCode = address.CountryCode;
                                    addressDetails.ZipCode = address.ZipCode;
                                    AddressList.Add(addressDetails);
                                }
                            }
                            //Transactions
                            if (tableData.Subscriber_Tranx.Count() > 0)
                            {
                                foreach (var payments in tableData.Subscriber_Tranx)
                                {
                                    PaymentDetails paymentDetails = new PaymentDetails();
                                    paymentDetails.CardAmount = (decimal)payments.TranxAmount;
                                    paymentDetails.CardNumber = payments.CardLastFour;
                                    paymentDetails.CardExp = payments.CardExp;
                                    paymentDetails.CardOwner = payments.CardOwner;
                                    paymentDetails.CardType = payments.CardType;
                                    paymentDetails.TranxDate = payments.TranxDate;
                                    paymentDetails.RateDescription = ratesList.FirstOrDefault(X => X.Rateid == payments.RateID).RateDescr;
                                    PaymentsList.Add(paymentDetails);

                                }
                            }

                        authSubcriber.SubscriptionDetails = SubscriptionList;
                        authSubcriber.PaymentDetails = PaymentsList;
                        authSubcriber.AddressDetails = AddressList;
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
                var startDate = authSubcriber.SubscriptionDetails.FirstOrDefault().StartDate;
                var endDate = authSubcriber.SubscriptionDetails.FirstOrDefault().EndDate;
                ViewBag.plans = authSubcriber.SubscriptionDetails;
                ViewBag.dates = startDate.GetWeekdayInRange(endDate, DayOfWeek.Monday);
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
                return View(subscriptionDetails);
            }
            catch (Exception)
            {

                return View("dashboard");
            }
            
        }

        public ActionResult UserProfile()
        {
            AuthSubcriber authSubcriber = GetAuthSubscriber();
            ViewBag.plans = authSubcriber.SubscriptionDetails;


            return View(authSubcriber);
        }
        [HttpPost]
        public ActionResult UserProfile(AuthSubcriber authSubcriber)
        {

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

                        Subscriber_Address oldAddress = tableData.Subscriber_Address.FirstOrDefault();
                        tableData.Subscriber_Address.Remove(oldAddress);

                        Subscriber_Address newAddress = new Subscriber_Address();
                        newAddress = oldAddress;

                        newAddress.AddressLine1 = authSubcriber.AddressDetails.FirstOrDefault().AddressLine1;
                        newAddress.AddressLine2 = authSubcriber.AddressDetails.FirstOrDefault().AddressLine2;
                        newAddress.CityTown = authSubcriber.AddressDetails.FirstOrDefault().CityTown;
                        newAddress.StateParish = authSubcriber.AddressDetails.FirstOrDefault().StateParish;
                        newAddress.ZipCode = authSubcriber.AddressDetails.FirstOrDefault().ZipCode;
                        newAddress.CountryCode = authSubcriber.AddressDetails.FirstOrDefault().CountryCode;
                        newAddress.AddressType = authSubcriber.AddressDetails.FirstOrDefault().AddressType;

                        //update tables
                        tableData.FirstName = authSubcriber.FirstName;
                        tableData.LastName = authSubcriber.LastName;
                        tableData.Subscriber_Address.Add(newAddress);
                        context.SaveChanges();

                        var dbAddress = localAuthSubcriber.AddressDetails.FirstOrDefault(x => x.AddressType == "M");
                        localAuthSubcriber.AddressDetails.Remove(dbAddress);

                        AddressDetails address = new AddressDetails
                        {
                            AddressLine1 = newAddress.AddressLine1,
                            AddressLine2 = newAddress.AddressLine2,
                            CityTown = newAddress.CityTown,
                            StateParish = newAddress.StateParish,
                            ZipCode = newAddress.ZipCode,
                            CountryCode = newAddress.CountryCode,
                            AddressType = newAddress.AddressType,
                        };

                        localAuthSubcriber.AddressDetails.Add(address);

                        ViewBag.msg = "Profile updated successfully";
                    }
                    catch (Exception)
                    {

                        return RedirectToAction("UserProfile");

                    }
                    
                    
                }
            }
            return RedirectToAction("UserProfile");
        }
       
        [AllowAnonymous]
        public ActionResult Subscribe(string pkgType, string term, decimal price = 0)
        {
            ViewData["pkgType"] = pkgType;
            ViewData["price"] = price;
            ViewData["term"] = term;
            ViewData["preloadSub"] = GetPreloadSub();
            //Test Data
            LoginDetails ld = new LoginDetails
            {
                FirstName = "Dwayne",
                LastName = "Mendez",
                EmailAddress = "dwayne.mendez@live.net",
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
                        authUser.PasswordKey = data.Password;
                        //obj.passwordHash = PasswordHash(data.Password);
                        obj.CreatedAt = DateTime.Now;
                        obj.IpAddress = Request.UserHostAddress;
                        
                        user.Email = data.EmailAddress;
                        user.PasswordHash = data.Password;

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

                        //Test Data
                        AddressDetails ad = new AddressDetails
                        {
                            CountryList = CountryList,
                            AddressLine1 = "Lot 876 Scheme Steet",
                            CityTown = "Bay Town",
                            StateParish = "Portland",
                            CountryCode = "JAM",
                            ZipCode = "JAMWI",
                            Phone = "876-875-8651"
                        };

                        return View("AddressDetails", ad);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
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
                        ApplicationDbContext db = new ApplicationDbContext();
                        DeliveryAddress delAddressDetails = new DeliveryAddress
                        { 
                            CountryList = (List<SelectListItem>)Session["CountryList"]
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

                        throw ex;
                    }

                }
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetRatesList()
        {

            return PartialView("_RatesFormPartial");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRatesList(string rateType)
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

                    //PrintSubRates model = new PrintSubRates
                    //    {
                    //        Rates = nratesList,
                    //        RateType = rateType
                    //    };
                    model.Rates = nratesList;
                    model.RateType = rateType;

                    return PartialView("_RatesPartial", model);
                }
                catch (Exception e)
                {
                    //handle exception
                    throw e;
                }
            }

            return PartialView("_RatesPartial", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SubscriptionInfo(SubscriptionDetails data, string prevBtn, string nextBtn)
        {
            ViewData["preloadSub"] = GetPreloadSub();

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
                        CountryList = (List<SelectListItem>)Session["CountryList"]
                    };

                    return View("AddressDetails", ad);
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
                        DeliveryAddress objDelv = GetSubscriberDeliveryAddress();
                        Subscriber_Epaper objEp = GetEpaperDetails();
                        Subscriber_Print objPr = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        AuthSubcriber authUser = GetAuthSubscriber();
                        List<SubscriptionDetails> subscriptionDetails = new List<SubscriptionDetails>();
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
                                RateType = selectedPlan.Type
                            };

                            subscriptionDetails.Add(printSubscription);
                            authUser.SubscriptionDetails = subscriptionDetails;

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
                                StartDate = data.StartDate,
                                EndDate = endDate,
                                RateID = data.RateID,
                                SubType = data.SubType,
                                NotificationEmail = data.NotificationEmail,
                                RateType = selectedPlan.Type
                            };

                            subscriptionDetails.Add(epaperSubscription);
                            authUser.SubscriptionDetails = subscriptionDetails;

                        }
                        if (selectedPlan.Type == "Bundle")
                        {
                            var pEndDate = data.EndDate = data.StartDate.AddDays(30);
                            SubscriptionDetails printSubscription = new SubscriptionDetails
                            {
                                StartDate = data.StartDate,
                                EndDate = pEndDate,
                                RateID = data.RateID,
                                DeliveryInstructions = data.DeliveryInstructions,
                            };
                            //print subscription
                            subscriptionDetails.Add(printSubscription);

                            var eEndDate = data.EndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
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

                            authUser.SubscriptionDetails = subscriptionDetails;


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


                        //Country =Session["CountryList"] = 
                        AddressDetails billingAddress = new AddressDetails {
                            //Test data
                            AddressLine1 = "Lot 876 Scheme Steet",
                            CityTown = "Bay Town",
                            StateParish = "Portland",
                            CountryCode = "JAM",
                            ZipCode = "JAMWI",
                            //end 
                            CountryList = (List<SelectListItem>)Session["CountryList"]
                        };

                        
                        PaymentDetails pd = new PaymentDetails
                        {
                            RateID = data.RateID,
                            RateDescription = selectedPlan.RateDescr,
                            Currency = selectedPlan.Curr,
                            CardAmount = (decimal)selectedPlan.Rate,
                            SubType = selectedPlan.Type,
                            BillingAddress = billingAddress,
                            //test data
                            CardOwner = "Dwayne Mendez",
                        };

                        return View("PaymentDetails", pd);
                    }
                    catch (Exception ex)
                    {

                        LogError(ex);
                    }

                }
            }

            return View();
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
                deliveryAddress.CityTown = form["DeliveryAddress.CityTown"];
                deliveryAddress.StateParish = form["DeliveryAddress.StateParish"];
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

            data.BillingAddress.CountryList = (List<SelectListItem>)Session["CountryList"];

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

                    throw ex;
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
                    }

                    dynamic summary = await ChargeCard(data);

                }
            }

            return View(data);
        }

        public async Task<bool> SaveSubscriptionAsync(PaymentDetails paymentDetails)
        {
           
            //get all session variables
            Subscriber objSub = GetSubscriber();
            Subscriber_Address objAdd = GetSubscriberAddress();
            Subscriber_Epaper objE = GetEpaperDetails();
            Subscriber_Print objP = GetPrintDetails();
            Subscriber_Tranx objTran = GetTransaction();
            ApplicationUser user = GetAppUser();

            AuthSubcriber authUser = GetAuthSubscriber();
            authUser.FirstName = objSub.FirstName;
            authUser.LastName = objSub.LastName;
            authUser.EmailAddress = objSub.EmailAddress;

            string SubscriberID = "";
            int addressID = 0;
            var rateID = objTran.RateID;

            //save subscribers
            objSub.IsActive = true;

            var newAccount = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                Subscriber = objSub
            };
            //create application user
            var createAccount = await UserManager.CreateAsync(newAccount, user.PasswordHash);
            if (createAccount.Succeeded)
            {
                //get Subscriber ID
                SubscriberID = newAccount.Id;
                //assign User Role
                createAccount = await UserManager.AddToRoleAsync(SubscriberID, "Subscriber");
                //save to DB
                using (var context = new ApplicationDbContext())
                {
                    //save address
                    objAdd.SubscriberID = SubscriberID;
                    context.subscriber_address.Add(objAdd);
                    await context.SaveChangesAsync();

                    //get Address ID
                    addressID = objAdd.AddressID;

                    //update subscribers table w/ address ID
                    var result = context.subscribers.SingleOrDefault(b => b.SubscriberID == SubscriberID);
                    if (result != null)
                    {
                        result.AddressID = addressID;
                        await context.SaveChangesAsync();
                    }

                    var selectedPlan = context.printandsubrates.SingleOrDefault(b => b.Rateid == rateID);

                    //save transaction
                    objTran.EmailAddress = objSub.EmailAddress;
                    objTran.TranxDate = DateTime.Now;
                    objTran.SubscriberID = SubscriberID;
                    objTran.RateID = rateID;
                    objTran.IpAddress = Request.UserHostAddress;
                    //from gateway
                    objTran.CardOwner = paymentDetails.CardOwner;
                    objTran.CardType = paymentDetails.CardType;
                    objTran.CardExp = paymentDetails.CardExp;
                    objTran.CardLastFour = paymentDetails.CardNumberLastFour;
                    objTran.TranxAmount = (double)paymentDetails.CardAmount;
                    objTran.OrderID = paymentDetails.OrderNumber;
                    objTran.PromoCode = paymentDetails.CardOwner;
                    objTran.EnrolledIn3DSecure = paymentDetails.EnrolledIn3DSecure;
                    context.subscriber_tranx.Add(objTran);
                    await context.SaveChangesAsync();

                    //disable subscription unti 3ds process is complete
                    if (paymentDetails.EnrolledIn3DSecure)
                    {
                        objP.IsActive = false;
                        objE.IsActive = false;
                    }

                    //save based on subscription
                    if (selectedPlan != null)
                    {
                        switch (selectedPlan.Type)
                        {
                            case "Print":
                                //save print subscription
                                objP.AddressID = addressID;
                                objP.SubscriberID = SubscriberID;
                                context.subscriber_print.Add(objP);
                                await context.SaveChangesAsync();
                                break;

                            case "Epaper":
                                //save epaper subscription
                                objE.SubType = SubscriptionType.Paid.ToString();
                                objE.SubscriberID = SubscriberID;
                                context.subscriber_epaper.Add(objE);
                                await context.SaveChangesAsync();
                                break;

                            case "Bundle":
                                //save print subscription
                                objP.AddressID = addressID;
                                objP.SubscriberID = SubscriberID;
                                context.subscriber_print.Add(objP);

                                //save epaper subscription
                                objE.SubscriberID = SubscriberID;
                                context.subscriber_epaper.Add(objE);

                                await context.SaveChangesAsync();
                                break;

                            default:
                                break;
                        }
                        }
                    }

                    if (paymentDetails.EnrolledIn3DSecure)
                    {
                        return true;

                    }
                    else
                    {
                        return false;

                    }
                    //RemoveSubscriber();
            }

            AddErrors(createAccount);
            return false;
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
                AddressDetails mailingAddress = authUser.AddressDetails.FirstOrDefault(x => x.AddressType == "M");
                Subscriber_Address objAdd = new Subscriber_Address
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

                AddressDetails deliveryAddress = authUser.AddressDetails.FirstOrDefault(x => x.AddressType == "D");
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

                SubscriptionDetails epaperSub = authUser.SubscriptionDetails.FirstOrDefault(x => x.RateType == "Epaper" || x.RateType == "Bundle");
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
                        NotificationEmail = epaperSub.NotificationEmail
                    };
                }

                SubscriptionDetails printSub = authUser.SubscriptionDetails.FirstOrDefault(x => x.RateType == "Print" || x.RateType == "Bundle");
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
                    };
                }

                PaymentDetails trxDetails = authUser.PaymentDetails.FirstOrDefault();
                Subscriber_Tranx objTran = new Subscriber_Tranx
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
                    EnrolledIn3DSecure = true
                };


                string SubscriberID = "";
                int addressID = 0;
                var rateID = objTran.RateID;

                //save subscribers

                var newAccount = new ApplicationUser
                {
                    UserName = emailAddress,
                    Email = emailAddress,
                    Subscriber = objSub
                };
                //create application user
                var createAccount = await UserManager.CreateAsync(newAccount, authUser.PasswordKey);
                if (createAccount.Succeeded)
                {
                    //get Subscriber ID
                    SubscriberID = newAccount.Id;
                    //assign User Role
                    createAccount = await UserManager.AddToRoleAsync(SubscriberID, "Subscriber");
                    //save to DB
                    using (var context = new ApplicationDbContext())
                    {
                        //save address
                        objAdd.SubscriberID = SubscriberID;
                        context.subscriber_address.Add(objAdd);
                        await context.SaveChangesAsync();

                        //save delivery address
                        if (deliveryAddress != null) {
                            objDelAdd.SubscriberID = SubscriberID;
                            context.subscriber_address.Add(objDelAdd);
                            await context.SaveChangesAsync();
                        }

                        //get Address ID
                        addressID = objAdd.AddressID;

                        //update subscribers table w/ address ID
                        var result = context.subscribers.SingleOrDefault(b => b.SubscriberID == SubscriberID);
                        if (result != null)
                        {
                            result.AddressID = addressID;
                            await context.SaveChangesAsync();
                        }

                        var selectedPlan = context.printandsubrates.SingleOrDefault(b => b.Rateid == rateID);

                        objTran.SubscriberID = SubscriberID;
                        context.subscriber_tranx.Add(objTran);
                        await context.SaveChangesAsync();


                        //save based on subscription
                        if (selectedPlan != null)
                        {
                            switch (selectedPlan.Type)
                            {
                                case "Print":
                                    //save print subscription
                                    objP.AddressID = addressID;
                                    objP.SubscriberID = SubscriberID;
                                    context.subscriber_print.Add(objP);
                                    await context.SaveChangesAsync();
                                    break;

                                case "Epaper":
                                    //save epaper subscription
                                    objE.SubType = SubscriptionType.Paid.ToString();
                                    objE.SubscriberID = SubscriberID;
                                    context.subscriber_epaper.Add(objE);
                                    await context.SaveChangesAsync();
                                    break;

                                case "Bundle":
                                    //save print subscription
                                    objP.AddressID = addressID;
                                    objP.SubscriberID = SubscriberID;
                                    context.subscriber_print.Add(objP);

                                    //save epaper subscription
                                    objE.SubType = SubscriptionType.Paid.ToString();
                                    objE.SubscriberID = SubscriberID;
                                    context.subscriber_epaper.Add(objE);

                                    await context.SaveChangesAsync();
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    await CompleteTransactionProcess(int.Parse(rateID.ToString()), objTran.OrderID, emailAddress);


                    return true;
                    //RemoveSubscriber();
                }

                AddErrors(createAccount);
            }
            catch (Exception ex)
            {

                LogError(ex);
            }

            
            return false;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PaymentSuccess()
        {
            RemoveSubscriber();
            
            return View();
        }

        public async Task<ActionResult> ChargeCard(PaymentDetails paymentDetails)
        {
            TransactionSummary summary = new TransactionSummary();
            TransactionSummary encrypted = new TransactionSummary();

            try
            {
                AuthSubcriber clientData = GetAuthSubscriber(); //pull exisiting data
                var processedClientData = new AuthSubcriber();
                List<PaymentDetails> paymentDetailsList = new List<PaymentDetails>();
               
                // Setup card processor.
                var cardProcessor = new CardProcessor();
                var transactionDetails = new TransactionDetails();
                var cardDetails = new CardDetails();
                var billingDetails = new BillingDetails();


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

                //For Testing Purposes Only
                string xxx = (DateTime.Now.Millisecond).ToString();
                xxx = xxx.Substring(xxx.Length - 2, 2);

                transactionDetails.OrderNumber = $"{DateTime.Now.Year}{xxx}{paymentDetails.Currency}{"-"}{paymentDetails.SubType.ToUpper()}{"-"}{CardUtils.ZeroPadAmount(paymentDetails.RateID)}";

                paymentDetails.OrderNumber = transactionDetails.OrderNumber;
                paymentDetailsList.Add(paymentDetails);
                processedClientData.PaymentDetails = paymentDetailsList;
                clientData.PaymentDetails = paymentDetailsList;

                // Update Billing Details
                billingDetails.BillToAddress = paymentDetails.BillingAddress.AddressLine1;
                billingDetails.BillToAddress2 = paymentDetails.BillingAddress.AddressLine2;
                billingDetails.BillToCity = paymentDetails.BillingAddress.CityTown;

                if (!await CardUtils.IsCardCharged(transactionDetails.OrderNumber))
                {
                    // Clear sensitive data and save for later retrieval.
                    paymentDetails.CardCVV = "";
                    paymentDetails.CardNumber = "";

                    JOL_UserSession session;
                    var sessionRepository = new SessionRepository();
                    session = sessionRepository.CreateObject(clientData);
                    var isSaved = await sessionRepository.AddOrUpdate(transactionDetails.OrderNumber, session, paymentDetails.RateID, clientData);

                    summary = await cardProcessor.ChargeCard(cardDetails, transactionDetails, billingDetails, null);

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
                        return View("PaymentDetails", paymentDetails);
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

                                await SaveSubscriptionAsync(paymentDetails);
                                RemoveSubscriber();
                                return View("PaymentSuccess");

                            case PaymentStatus.Failed:
                                // TODO: Add Friendly Message property to Card Processor to display to user.
                                return View("PaymentDetails", paymentDetails);
                            case PaymentStatus.GatewayError:
                            case PaymentStatus.InternalError:
                                break;
                        }
                        //TODO: Remove only cookies?
                        RemoveSubscriber();

                        paymentDetails.TransactionSummary = summary;
                        await SaveSubscriptionAsync(paymentDetails);
                        return View("PaymentDetails", paymentDetails);

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
                    await SaveSubscriptionAsync(paymentDetails);
                    return View("PaymentDetails", paymentDetails);
                }


                //encrypted = summary;
                //encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(responseData), "E");
                //return encrypted;

            }
            catch (Exception ex)
            {
                LogError(ex);
                return View("PaymentDetails");
            }
            // Something went wrong to get here.
            // return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> CompleteTransactionProcess(int rateID, string orderNumber, string emailAddress)
        {
            var appDbOther = new ApplicationDbContext();
            var cardProcessor = new CardProcessor();

            try
            {
                var savedTransaction = await appDbOther.subscriber_tranx.FirstOrDefaultAsync(t => t.OrderID == orderNumber);

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

                        var transactionResponse = await cardProcessor.GetGatewayTransactionStatus(orderNumber);
                        var transSummary = cardProcessor.GetTransactionSummary(transactionResponse);

                        currentTransaction.ConfirmationNumber = savedTransaction.ConfirmationNo;
                        //currentTransaction.OrderID = orderID;
                        currentTransaction.AuthorizationCode = savedTransaction.AuthCode;
                        using (var context = new ApplicationDbContext())
                        {
                            //load data and join via foriegn keys
                            var clientData = context.subscribers
                                .Include(x => x.Subscriber_Epaper)
                                .Include(x => x.Subscriber_Print)
                                .Include(x => x.Subscriber_Tranx)
                                .FirstOrDefault(u => u.EmailAddress == emailAddress);

                            if (clientData != null)
                            {
                                //update transaction table with authcode and confirmation no.
                                clientData.Subscriber_Tranx.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID).AuthCode = transSummary.AuthCode;
                                clientData.Subscriber_Tranx.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID).ConfirmationNo = transSummary.ReferenceNo;
                                clientData.Subscriber_Tranx.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID).IsMadeLiveSuccessful = true;

                                //make subscription active
                                if (currentTransaction.SubType == "Epaper" || currentTransaction.SubType == "Bundle")
                                    clientData.Subscriber_Epaper.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID).IsActive = true;

                                if (currentTransaction.SubType == "Print" || currentTransaction.SubType == "Bundle")
                                    clientData.Subscriber_Print.FirstOrDefault(x => x.EmailAddress == emailAddress && x.RateID == rateID).IsActive = true;
                            }

                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogError(ex);
            }


            RemoveSubscriber();
            return View("PaymentSuccess");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CompleteTransaction(FormCollection form)
        {
            try
            {

                var websiteHost = ConfigurationManager.AppSettings["ecomm_Prod"];
                //var host = Utilities.SetAppEnvironment(websiteHost);

                var sessionRepository = new SessionRepository();
                JOL_UserSession existing = new JOL_UserSession();
                //AuthSubcriber customerData = new AuthSubcriber();
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


                var originalAmount = CardUtils.GetAmountFromString(keys[2]);
                threedsparams.Amount = originalAmount;


                // Free up cache
                tempData.Cache.Remove(threedsparams.OrderID);
                var summary = CardProcessor.GetGateway3DSecureResponse(threedsparams);

                // Send Payment Gateway notification
                //var notificationDetails = new EmailNotificationDetails()
                //{
                //    FullName = fullName,
                //    AddressLine1 = transaction.CardAddress1,
                //    AddressLine2 = transaction.CardAddress2,
                //    AddressLine3 = transaction.CardParish,
                //    Email = email,
                //    Contact = customerData?.Contacts?.FirstOrDefault(c => c.ItemName.ToLower() == "number")?.Item ?? ""
                //};

                //_logger.CreateLog("Attempting to send payment notification", logModel, LogType.Information, additionalFields: logDetails);


                // Redirect
                // Setup messages for failure and passes.
                switch (summary.TransactionStatus.Status)
                {
                    case PaymentStatus.Successful:
                        //save subscription
                        var existingSession = await sessionRepository.Get(email);
                        var customerData = JsonConvert.DeserializeObject<AuthSubcriber>(existingSession.RootObject);
                        //await SaveSubscriptionAsync();
                        await SaveSubscriptionInfoAsync(customerData);

                        // Set to 15 minutes by default if not found
                        int cacheExpiryDuration = int.Parse(ConfigurationManager.AppSettings["cacheExpiryDuration"] ?? "15");
                        // Repopulate cache for new flow.
                        tempData.Cache.Add(summary.OrderId, $"{email}|{rateID}", new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheExpiryDuration) });

                        //await CompleteTransactionProcess(int.Parse(rateID), threedsparams.OrderID, email);
                        
                        RemoveSubscriber();

                        return View("PaymentSuccess");
                    case PaymentStatus.Failed:
                    //_logger.CreateLog("Authorization failed", logModel, LogType.Warning, additionalFields: logDetails);
                    //return Redirect($"{host}/payments?status=failed");
                    case PaymentStatus.InternalError:
                    case PaymentStatus.GatewayError:
                        //_logger.CreateLog("Gateway/Internal failure", logModel, LogType.Warning, additionalFields: logDetails);
                        //return Redirect($"{host}/payments?status=error");
                        break;
                    default:
                        //_logger.CreateLog("Generic failure", logModel, LogType.Warning, additionalFields: logDetails);
                        //return Redirect($"{host}/payments?status=failed");
                        break;
                }
                // return Ok();
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                ////_logger.CreateLog("Something went wrong on the server with this request", logModel, LogType.Error, ex, additionalFields: logDetails);
                //return InternalServerError(ex);
            }

           // return new RedirectResult("/Account/PaymentDetails", true);
            return View("PaymentDetails");

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

        private DeliveryAddress GetSubscriberDeliveryAddress()
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
                Session["subscriber_location"] = new UserLocation();
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

        private void RemoveSubscriber()
        {
            Session.Remove("subscriber");
            Session.Remove("subscriber_address");
            Session.Remove("subscriber_epaper");
            Session.Remove("subscriber_print");
            Session.Remove("subscriber_tranx");
            Session.Clear();
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }

            Dispose(true);
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