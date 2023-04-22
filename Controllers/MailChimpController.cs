using System.Threading.Tasks;
using MailChimp.Net;
using MailChimp.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MailChimp.Net.Models;
using ePaperLive.Models;

namespace ePaperLive.Controllers
{
    public class MailChimpController : Controller
    {
        // GET: MailChimp
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetAllList()
        {
            try
            {
                var mailChimpApiKey = ConfigurationManager.AppSettings["MailChimpApiKey"];
                var mailChimpListID = ConfigurationManager.AppSettings["MailChimpListID"];

                IMailChimpManager manager = new MailChimpManager(mailChimpApiKey);

                //var mailChimpListCollection = await manager.Lists.GetAllAsync().ConfigureAwait(false);
                //var allMembers = await manager.Members.GetAllAsync(listId).ConfigureAwait(false);

                using (var context = new ApplicationDbContext())
                {
                    var sql = @"
                    SELECT DISTINCT s.FirstName, s.LastName, t.EmailAddress FROM [dbo].[Subscriber_Tranx] t
                    JOIN [dbo].[Subscribers] s ON s.SubscriberID = t.SubscriberID
                    WHERE t.OrderID LIKE 'FreeTrial%' AND t.EmailAddress NOT LIKE '%jamaicaobserver%'";

                    var result = await context.Database.SqlQuery<MailChimpFields>(sql).ToListAsync();

                    foreach (var item in result)
                    {
                        var member = new Member { EmailAddress = item.EmailAddress, StatusIfNew = Status.Subscribed};
                        member.MergeFields.Add("FNAME", item.FirstName);
                        member.MergeFields.Add("LNAME", item.LastName);
                        await manager.Members.AddOrUpdateAsync(mailChimpListID, member);
                        
                        Tags tags = new Tags();
                        tags.MemberTags.Add(new Tag() { Name = "Free Trial Subscriber", Status = "active" });
                        await manager.Members.AddTagsAsync(mailChimpListID, item.EmailAddress, tags);
                    }
                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
            }
           
            return View();
        }
    }
}