using ePaperLive.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ePaperLive.DBModel;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

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
                    SELECT s.SubscriberID, s.FirstName, s.LastName, u.UserName,r.Id as RoleID, r.Name As Role, sa.AddressID, s.IsActive
                    FROM AspNetUsers u
                    LEFT JOIN AspNetUserRoles ur ON  ur.UserId = u.Id 
                    LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId
                    LEFT JOIN Subscribers s ON s.SubscriberID = u.Id			
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = s.AddressID";
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
                    SELECT s.SubscriberID, s.FirstName, s.LastName, u.UserName,r.Id as RoleID, r.Name As Role, sa.AddressID, s.IsActive
                    FROM AspNetUsers u
                    LEFT JOIN AspNetUserRoles ur ON  ur.UserId = u.Id 
                    LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId
                    LEFT JOIN Subscribers s ON s.SubscriberID = u.Id			
                    LEFT JOIN Subscriber_Address sa ON sa.AddressID = s.AddressID
                    WHERE u.Id = @Id";
            var idParam = new SqlParameter("Id", id);

            UsersWithRoles result = await db.Database.SqlQuery<UsersWithRoles>(sql, idParam).FirstOrDefaultAsync();
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }
        [HttpPost]
        [Route("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SubscriberID,FirstName,LastName,UserName,RoleID,Role,AddressID,IsActive")]UsersWithRoles usersWithRoles) 
        {
            if (ModelState.IsValid)
            {
                var store = new UserStore<ApplicationUser>(db);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = new ApplicationUser();
                
                //AspnetUser
                user = manager.FindById(usersWithRoles.SubscriberID);
                user.Email = usersWithRoles.UserName;
                user.UserName = usersWithRoles.UserName;
                await manager.UpdateAsync(user);

                //Subscribers
                Subscriber subscriber = await db.subscribers.FindAsync(usersWithRoles.SubscriberID);
                if (subscriber != null)
                {
                    subscriber.FirstName = usersWithRoles.FirstName;
                    subscriber.LastName = usersWithRoles.LastName;
                    subscriber.EmailAddress = usersWithRoles.UserName;
                    subscriber.IsActive = (bool)usersWithRoles.IsActive;

                }
                db.Entry(subscriber).State = EntityState.Modified;
                
                //AspNetUserRoles
                var role = db.Roles.SingleOrDefault(r => r.Id == usersWithRoles.RoleID).Name;
                if (role != usersWithRoles.Role)
                {
                    await manager.RemoveFromRoleAsync(user.Id, role);
                    await manager.AddToRoleAsync(user.Id, usersWithRoles.Role);
                }

                db.Entry(user).State = EntityState.Modified;


                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}