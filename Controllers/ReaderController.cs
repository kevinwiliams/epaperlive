using ePaperLive.DBModel;
using ePaperLive.Models;
using System;
using System.Collections.Generic;
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

namespace ePaperLive.Controllers
{
    [BasicAuthentication]
    [System.Web.Mvc.RequireHttps]
    public class ReaderController : ApiController
    {
        [HttpPost]
        [ResponseType(typeof(Reader))]
        public HttpResponseMessage Post([FromBody] Reader reader)
        {
            if (ModelState.IsValid && reader != null)
            {
                member mb = new member();
                error error = new error();
                var xml = "";
                var errCode = "";
                var errMsg = "";
                subscriber result = new subscriber();

                try
                {
                    // Convert any HTML markup in the status text.
                    reader.call = HttpUtility.HtmlEncode(reader.call);

                    using (var context = new Entities())
                    {

                        /* var tableData = (from s in context.subscribers
                                       join a in context.subscriber_address on s.addressID equals a.addressID
                                       join e in context.subscriber_epaper on s.subscriberID equals e.subscriberID
                                       select new { subscriber = s, address = a, subscription = e});*/

                        //load data and join via foriegn keys
                        var tableData = context.subscribers
                            .Include(x => x.subscriber_address)
                            .Include(x => x.subscriber_epaper);

                        if (tableData != null)
                        {
                            //check call coming from request
                            switch (reader.call)
                            {
                                case "authenticate":
                                    //encrypt password
                                    var password = PasswordHash(reader.password);
                                    result = tableData.SingleOrDefault(b => b.emailAddress == reader.username && b.passwordHash == password && b.isActive == true);
                                    //pass error values if query fails
                                    errCode = "03";
                                    errMsg = "Invalid credentials";
                                    break;
                                case "get_user_by_userid":
                                    result = tableData.SingleOrDefault(b => b.subscriberID == reader.userid && b.isActive == true);
                                    //pass error values if query fails
                                    errCode = "04";
                                    errMsg = "User not found";
                                    break;
                                case "get_user_by_token":
                                    result = tableData.SingleOrDefault(b => b.token == reader.token && b.isActive == true);
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
                            if (result.subscriber_epaper != null)
                            {
                                if (result.subscriber_epaper.FirstOrDefault(x => x.isActive == true) != null)
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
                    throw ex;
                }
               
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [NonAction]
        public string MemberXML(member mb, subscriber result)
        {
            var xml = "";
            try
            {
           
                DateTime today = DateTime.Now;
                DateTime endDate = result.subscriber_epaper.FirstOrDefault(x => x.isActive == true).endDate;

                TimeSpan t = endDate - today;
                double daysLeft = t.TotalDays;
                var subscriptionCode = WebConfigurationManager.AppSettings["SubcriptionCode"];

                //set subscription code is epaper valid
                if (daysLeft > 1)
                {
                    mb.userID = result.subscriberID;
                    mb.email = result.emailAddress;
                    mb.loginName = result.emailAddress;
                    mb.firstname = result.firstName;
                    mb.lastname = result.lastName;
                    mb.homeareacode = result.phoneNumber;
                    mb.homephone = result.phoneNumber;
                    mb.workareacode = String.Empty;
                    mb.workphone = String.Empty;
                    mb.address = result.subscriber_address.addressLine1;
                    mb.apartment = result.subscriber_address.addressLine2;
                    mb.city = result.subscriber_address.cityTown;
                    mb.province = result.subscriber_address.stateParish;
                    mb.postalcode = result.subscriber_address.zipCode;
                    mb.country = result.subscriber_address.country;
                    mb.gender = String.Empty;
                    mb.nickname = String.Empty;
                    mb.subscription = subscriptionCode;
                    //change date format to YYYY-MM-DD
                    var dateTime = result.subscriber_epaper.FirstOrDefault(x => x.isActive == true).endDate.ToString();
                    DateTime dt = DateTime.ParseExact(dateTime, "d/M/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    mb.expiration = dt.ToString("yyyy-MM-dd");
                }
                        
                //serialize class to xml string using helper
                xml = ObjectSerializer<member>.Serialize(mb);
                
            }
            catch (Exception ex)
            {

                throw ex;
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
