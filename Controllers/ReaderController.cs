using ePaperLive.DBModel;
using ePaperLive.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Data.Entity;
using static ePaperLive.Util;
using System.Globalization;
using ePaperLive.Filters;
using System.Web.Configuration;
using Microsoft.AspNet.Identity.Owin;


namespace ePaperLive.Controllers
{
    [BasicAuthentication]
    [System.Web.Mvc.RequireHttps]
    public class ReaderController : ApiController
    {
        [HttpPost]
        [ResponseType(typeof(Reader))]
        public async System.Threading.Tasks.Task<HttpResponseMessage> Post([FromBody] Reader reader)
        {
            if (ModelState.IsValid && reader != null)
            {
                member mb = new member();
                error error = new error();
                var xml = "";
                var errCode = "";
                var errMsg = "";
                Subscriber result = new Subscriber();

                try
                {
                    // Convert any HTML markup in the status text.
                    reader.call = HttpUtility.HtmlEncode(reader.call);

                    using (var context = new ApplicationDbContext())
                    {

                        /* var tableData = (from s in context.subscribers
                                       join a in context.subscriber_address on s.AddressID equals a.AddressID
                                       join e in context.subscriber_epaper on s.SubscriberID equals e.SubscriberID
                                       select new { subscriber = s, address = a, subscription = e});*/

                        //load data and join via foriegn keys
                        var tableData = context.subscribers
                            .Include(x => x.Subscriber_Address)
                            .Include(x => x.Subscriber_Epaper);

                        if (tableData != null)
                        {
                            //check call coming from request
                            switch (reader.call)
                            {
                                case "authenticate":
                                    //encrypt password
                                    var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                                    var user = userManager.FindAsync(reader.username, reader.password).Result;

                                    if(user != null)                                   
                                        result = await tableData.FirstOrDefaultAsync(b => b.EmailAddress == reader.username && b.IsActive == true);
                                    //pass error values if query fails
                                    errCode = "03";
                                    errMsg = "Invalid credentials";
                                    break;
                                case "get_user_by_userid":
                                    result = await tableData.FirstOrDefaultAsync(b => b.SubscriberID == reader.userid && b.IsActive == true);
                                    //pass error values if query fails
                                    errCode = "04";
                                    errMsg = "User not found";
                                    break;
                                case "get_user_by_token":
                                    var getUserByToken = context.Users.FirstOrDefaultAsync(x => x.SecurityStamp == reader.token).Result;

                                    if(getUserByToken != null)
                                        result = await tableData.FirstOrDefaultAsync(b => b.SubscriberID == getUserByToken.Id && b.IsActive == true);
                                    //pass error values if query fails
                                    errCode = "05";
                                    errMsg = "Invalid token";
                                    break;
                                default:
                                    break;
                            }
                        }

                        //load object with database results
                        if (result != null)
                        {
                            //get active subscription
                            if (result.Subscriber_Epaper != null)
                            {
                                if (result.Subscriber_Epaper.SingleOrDefault(x => x.IsActive == true) != null)
                                    xml = MemberXML(mb, result);
                                else
                                    xml = ErrorXML(errCode, errMsg);
                            }
                        }

                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
                        };

                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [NonAction]
        public string MemberXML(member mb, Subscriber result)
        {
            var xml = "";
            try
            {
           
                DateTime today = DateTime.Now;
                DateTime endDate = result.Subscriber_Epaper.FirstOrDefault(x => x.IsActive == true).EndDate;

                TimeSpan t = endDate - today;
                double daysLeft = t.TotalDays;
                var subscriptionCode = WebConfigurationManager.AppSettings["SubcriptionCode"];

                //set subscription code is epaper valid
                if (daysLeft > 1)
                {
                    mb.userID = result.SubscriberID;
                    mb.email = result.EmailAddress;
                    mb.loginName = result.EmailAddress;
                    mb.firstname = result.FirstName;
                    mb.lastname = result.LastName;
                    mb.homeareacode = String.Empty;
                    mb.homephone = String.Empty;
                    mb.workareacode = String.Empty;
                    mb.workphone = String.Empty;
                    if (result.Subscriber_Address.Count() > 0)
                    {
                        mb.address = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").AddressLine1;
                        mb.apartment = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").AddressLine2;
                        mb.city = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").CityTown;
                        mb.province = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").StateParish;
                        mb.postalcode = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").ZipCode;
                        mb.country = result.Subscriber_Address.FirstOrDefault(x => x.AddressType == "M").CountryCode;
                    }
                    else
                    {
                        mb.address = String.Empty;
                        mb.apartment = String.Empty;
                        mb.city = String.Empty;
                        mb.province = String.Empty;
                        mb.postalcode = String.Empty;
                        mb.country = String.Empty;
                    }
                    
                    mb.gender = String.Empty;
                    mb.nickname = result.FirstName + " " + result.LastName;
                    mb.subscription = subscriptionCode;
                    //change date format to YYYY-MM-DD
                    var dateTime = result.Subscriber_Epaper.FirstOrDefault(x => x.IsActive == true).EndDate.ToString("yyyy-MM-dd");
                    mb.expiration = dateTime;
                }
                        
                //serialize class to xml string using helper
                xml = ObjectSerializer<member>.Serialize(mb);
                
            }
            catch (Exception ex)
            {

                LogError(ex);
            }
            return xml;
        }

        public string ErrorXML(string code, string msg) 
        {
            error error = new error
            {
                code = code,
                message = msg
            };
            var xml = ObjectSerializer<error>.Serialize(error);

            return xml;
        }

    }
}
