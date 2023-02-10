using ePaperLive.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ePaperLive.Controllers.Admin.Subscribers
{
    [Authorize]
    [RoutePrefix("Admin/Subscribers")]
    [Route("action = index")]
    public class SubscribersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Subscribers
        [Route]
        public async Task<ActionResult> Index()
        {
            using (var context = new ApplicationDbContext())
            {
                var sql = @"
                    SELECT Subscribers.FirstName + ' ' + Subscribers.LastName as FullName, AspNetUsers.UserName, AspNetRoles.Name As Role, Subscriber_Address.AddressID, Subscribers.IsActive, Subscribers.SubscriberID
                    FROM AspNetUsers 
                    LEFT JOIN AspNetUserRoles ON  AspNetUserRoles.UserId = AspNetUsers.Id 
                    LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId
                    LEFT JOIN Subscribers ON Subscribers.SubscriberID = AspNetUsers.Id			
                    LEFT JOIN Subscriber_Address ON Subscriber_Address.AddressID = Subscribers.AddressID";
                //WHERE AspNetUsers.Id = @Id";
                //var idParam = new SqlParameter("Id", theUserId);

                var result = await context.Database.SqlQuery<UsersWithRoles>(sql).ToListAsync();
                return View(result);
            }
           
        }

        // GET: Subscribers/Edit/5
        [Route("edit/{id}")]
        public async Task<ActionResult> Edit(string id)
        {
            var rolesList = new List<SelectListItem>();
            var userRoles = new SelectList(db.Roles.Where(u => !u.Name.Contains("Admins"))
                                    .ToList(), "Name", "Name");

            foreach (var role in userRoles)
            {
                rolesList.Add(new SelectListItem { Text = role.Text, Value = role.Value });
            }

            ViewBag.Roles = rolesList;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sql = @"
                    SELECT Subscribers.FirstName + ' ' + Subscribers.LastName as FullName, AspNetUsers.UserName, AspNetRoles.Name As Role, Subscriber_Address.AddressID, Subscribers.IsActive,  Subscribers.SubscriberID
                    FROM AspNetUsers 
                    LEFT JOIN AspNetUserRoles ON  AspNetUserRoles.UserId = AspNetUsers.Id 
                    LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId
                    LEFT JOIN Subscribers ON Subscribers.SubscriberID = AspNetUsers.Id			
                    LEFT JOIN Subscriber_Address ON Subscriber_Address.AddressID = Subscribers.AddressID
                    WHERE AspNetUsers.Id = @Id";
            var idParam = new SqlParameter("Id", id);

            UsersWithRoles result = await db.Database.SqlQuery<UsersWithRoles>(sql, idParam).FirstOrDefaultAsync();
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }
    }
}