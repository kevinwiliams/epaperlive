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

namespace ePaperLive.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
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

                    return RedirectToAction("Index", "Home");
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
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
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
        public ActionResult ReadPaper()
        {
            string token = Session["tokenHash"].ToString();
            string pressReaderURL = "https://jamaicaobserver.pressreader.com/?token=";

            return Redirect(pressReaderURL + token);
        }

        [AllowAnonymous]
        public ActionResult LoginModal()
        {
            return PartialView("_LoginModal");
        }

        
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Subscribe()
        {
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

                        subscriber obj = GetSubscriber();
                        obj.FirstName = data.FirstName;
                        obj.LastName = data.LastName;
                        obj.EmailAddress = data.EmailAddress;
                        //obj.passwordHash = PasswordHash(data.Password);
                        obj.CreatedAt = DateTime.Now;
                        obj.IpAddress = Request.UserHostAddress;

                        //Test Data
                        AddressDetails ad = new AddressDetails
                        {
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
                    subscriber obj = GetSubscriber();
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
                        subscriber objSub = GetSubscriber();
                        subscriber_address objAdd = GetSubscriberAddress();
                        UserLocation objLoc = GetSubscriberLocation();

                        var market = (objLoc.Country_Code == "JM") ? "Local" : "International";

                        //objSub.phoneNumber = data.phone;
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
                        SubscriptionDetails subscriptionDetails = new SubscriptionDetails
                        {
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(30),
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
                                        .Where(x => x.Active == 1).ToList();


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
                    subscriber_address objAdd = GetSubscriberAddress();
                    AddressDetails ad = new AddressDetails
                    {
                        AddressLine1 = objAdd.AddressLine1,
                        AddressLine2 = objAdd.AddressLine2,
                        CityTown = objAdd.CityTown,
                        StateParish = objAdd.StateParish,
                        ZipCode = objAdd.ZipCode,
                        CountryCode = objAdd.CountryCode
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
                        subscriber objSub = GetSubscriber();
                        subscriber_epaper objEp = GetEpaperDetails();
                        subscriber_print objPr = GetPrintDetails();
                        subscriber_tranx objTran = GetTransaction();

                        objSub.Newsletter = data.newsletterSignUp;
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

                        //Test Data
                        PaymentDetails pd = new PaymentDetails
                        {
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
                    subscriber objSub = GetSubscriber();
                    subscriber_epaper objEp = GetEpaperDetails();
                    subscriber_print objPr = GetPrintDetails();
                    UserLocation objLoc = GetSubscriberLocation();
                    var market = (objLoc.Country_Code == "JM") ? "Local" : "International";


                    SubscriptionDetails sd = new SubscriptionDetails
                    {
                        StartDate = objEp.StartDate,
                        RateID = objEp.RateID,
                        DeliveryInstructions = objPr.DeliveryInstructions,
                        newsletterSignUp = objSub.Newsletter ?? false,
                        NotificationEmail = objEp.NotificationEmail,
                        SubType = objEp.SubType,
                        RatesList = db.printandsubrates.Where(x => x.Market == market).Where(x => x.Active == 1).ToList()
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
                        subscriber objSub = GetSubscriber();
                        subscriber_address objAdd = GetSubscriberAddress();
                        subscriber_epaper objE = GetEpaperDetails();
                        subscriber_print objP = GetPrintDetails();
                        subscriber_tranx objTran = GetTransaction();

                        int SubscriberID = 0;
                        int addressID = 0;
                        var rateID = objTran.RateID;

                        //save to DB
                        using (var context = new ApplicationDbContext())
                        {
                            //save subscribers
                            objSub.IsActive = true;
                            objSub.RoleID = 9002; //User
                            context.subscribers.Add(objSub);
                            await context.SaveChangesAsync();

                            //get Subscriber ID
                            SubscriberID = objSub.SubscriberID;

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

        private subscriber GetSubscriber()
        {
            if (Session["subscriber"] == null)
            {
                Session["subscriber"] = new subscriber();
            }
            return (subscriber)Session["subscriber"];
        }

        private subscriber_address GetSubscriberAddress()
        {
            if (Session["subscriber_address"] == null)
            {
                Session["subscriber_address"] = new subscriber_address();
            }
            return (subscriber_address)Session["subscriber_address"];
        }

        private subscriber_address GetSubscriberDeliveryAddress()
        {
            if (Session["subscriber_del_address"] == null)
            {
                Session["subscriber_del_address"] = new subscriber_address();
            }
            return (subscriber_address)Session["subscriber_del_address"];
        }

        private subscriber_epaper GetEpaperDetails()
        {
            if (Session["subscriber_epaper"] == null)
            {
                Session["subscriber_epaper"] = new subscriber_epaper();
            }
            return (subscriber_epaper)Session["subscriber_epaper"];
        }

        private subscriber_print GetPrintDetails()
        {
            if (Session["subscriber_print"] == null)
            {
                Session["subscriber_print"] = new subscriber_print();
            }
            return (subscriber_print)Session["subscriber_print"];
        }

        private subscriber_tranx GetTransaction()
        {
            if (Session["subscriber_tranx"] == null)
            {
                Session["subscriber_tranx"] = new subscriber_tranx();
            }
            return (subscriber_tranx)Session["subscriber_tranx"];
        }

        private UserLocation GetSubscriberLocation()
        {
            if (Session["subscriber_location"] == null)
            {
                Session["subscriber_location"] = new UserLocation();
            }
            return (UserLocation)Session["subscriber_location"];
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