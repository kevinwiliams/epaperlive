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

namespace ePaperLive.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationDbContext _db;

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



        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
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
                                    paymentDetails.CardAmount = (float)payments.TranxAmount;
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

                        Subscriber obj = GetSubscriber();
                        ApplicationUser user = GetAppUser();
                        obj.FirstName = data.FirstName;
                        obj.LastName = data.LastName;
                        obj.EmailAddress = data.EmailAddress;
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
                            CountryList = CountryList
                            //AddressLine1 = "Lot 876 Scheme Steet",
                            //CityTown = "Bay Town",
                            //StateParish = "Portland",
                            //country = "Jamaica",
                            //ZipCode = "JAMWI",
                            //phone = "876-875-8651"
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


                        var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                        //use for 2-step auth
                        user.PhoneNumber = data.Phone;
                        //add email from subscriber
                        objAdd.EmailAddress = objSub.EmailAddress;
                        //address type M - Mailing --- B - Billing
                        objAdd.AddressType = "M";
                        objAdd.AddressLine1 = data.AddressLine1;
                        objAdd.AddressLine2 = data.AddressLine2;
                        objAdd.CityTown = data.CityTown;
                        objAdd.StateParish = data.StateParish;
                        objAdd.ZipCode = data.ZipCode;
                        objAdd.CountryCode = data.CountryCode;
                        objAdd.CreatedAt = DateTime.Now;

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
                        Subscriber_Epaper objEp = GetEpaperDetails();
                        Subscriber_Print objPr = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();

                        objSub.Newsletter = data.NewsletterSignUp;
                        objTran.RateID = data.RateID;

                        var selectedPlan = db.printandsubrates.FirstOrDefault(x => x.Rateid == data.RateID);

                        if (selectedPlan.Type == "Print")
                        {
                            objPr.StartDate = data.StartDate;
                            objPr.EndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            objPr.RateID = data.RateID;
                            objPr.IsActive = true;
                            objPr.EmailAddress = objSub.EmailAddress;
                            objPr.DeliveryInstructions = data.DeliveryInstructions;
                            objPr.CreatedAt = DateTime.Now;

                        }
                        if (selectedPlan.Type == "Epaper")
                        {
                            objEp.StartDate = data.StartDate;
                            objEp.EndDate = data.StartDate.AddDays((double)selectedPlan.ETerm);
                            objEp.RateID = data.RateID;
                            objEp.SubType = data.SubType;
                            objEp.IsActive = true;
                            objEp.EmailAddress = objSub.EmailAddress;
                            objEp.NotificationEmail = data.NotificationEmail;
                            objEp.CreatedAt = DateTime.Now;
                        }
                        if (selectedPlan.Type == "Bundle")
                        {
                            //print subscription
                            objPr.StartDate = data.StartDate;
                            objPr.EndDate = data.StartDate.AddDays(30);
                            objPr.RateID = data.RateID;
                            objPr.IsActive = true;
                            objPr.EmailAddress = objSub.EmailAddress;
                            objPr.DeliveryInstructions = data.DeliveryInstructions;
                            objPr.CreatedAt = DateTime.Now;

                            //Epaper subscription
                            objEp.StartDate = data.StartDate;
                            objEp.EndDate = data.StartDate.AddDays((double)selectedPlan.PrintTerm * 7);
                            objEp.RateID = data.RateID;
                            objEp.SubType = data.SubType;
                            objEp.IsActive = true;
                            objEp.EmailAddress = objSub.EmailAddress;
                            objEp.NotificationEmail = data.NotificationEmail;
                            objEp.CreatedAt = DateTime.Now;
                        }

                    
                        PaymentDetails pd = new PaymentDetails
                        {
                            RateDescription = selectedPlan.RateDescr,
                            Currency = selectedPlan.Curr,
                            CardAmount = (float)selectedPlan.Rate,
                            SubType = selectedPlan.Type
                            // cardOwner = "Dwayne Mendez",
                        };

                        return View("PaymentDetails", pd);
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
        public async Task<ActionResult> PaymentDetails(PaymentDetails data, string prevBtn, string nextBtn)
        {

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
                        Subscriber objSub = GetSubscriber();
                        Subscriber_Address objAdd = GetSubscriberAddress();
                        Subscriber_Epaper objE = GetEpaperDetails();
                        Subscriber_Print objP = GetPrintDetails();
                        Subscriber_Tranx objTran = GetTransaction();
                        ApplicationUser user = GetAppUser();

                        string SubscriberID = "";
                        int addressID = 0;
                        var rateID = objTran.RateID;

                        //save subscribers
                        objSub.IsActive = true;
                        //objSub.RoleID = 9002; //SET AspNetUsersRole

                        var newAccount = new ApplicationUser
                        {
                            UserName = user.Email,
                            Email = user.Email,
                            Subscriber = objSub
                        };

                        var createAccount = await UserManager.CreateAsync(newAccount, user.PasswordHash);
                        if (createAccount.Succeeded)
                        {
                            //get Subscriber ID
                            SubscriberID = newAccount.Id;

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
                                //save based on subscription
                                var selectedPlan = context.printandsubrates.SingleOrDefault(b => b.Rateid == rateID);
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

                                    //save transaction
                                    objTran.EmailAddress = objSub.EmailAddress;
                                    objTran.CardType = data.CardType;
                                    objTran.CardOwner = data.CardOwner;
                                    objTran.TranxAmount = selectedPlan.Rate;
                                    objTran.TranxDate = DateTime.Now;
                                    objTran.SubscriberID = SubscriberID;
                                    objTran.IpAddress = Request.UserHostAddress;
                                    context.subscriber_tranx.Add(objTran);
                                    await context.SaveChangesAsync();
                                }
                            }

                            RemoveSubscriber();
                            return View("PaymentSuccess");
                        }
                        AddErrors(createAccount);


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
        public ActionResult PaymentSuccess()
        {
            return View();
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

        private Subscriber_Address GetSubscriberDeliveryAddress()
        {
            if (Session["subscriber_del_address"] == null)
            {
                Session["subscriber_del_address"] = new Subscriber_Address();
            }
            return (Subscriber_Address)Session["subscriber_del_address"];
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

        private void RemoveSubscriber()
        {
            Session.Remove("subscriber");
            Session.Remove("subscriber_address");
            Session.Remove("subscriber_epaper");
            Session.Remove("subscriber_print");
            Session.Remove("subscriber_tranx");
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