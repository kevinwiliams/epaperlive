﻿using ePaperLive.Models;
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
using System.Configuration;
using System.Data;
using System.Linq.Expressions;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin/Subscribers")]
    [Route("action = index")]
    public class SubscribersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ActivityLog _actLog = new ActivityLog();

        // GET: Subscribers
        [Route]
        public async Task<ActionResult> Index()
        {
            using (var context = new ApplicationDbContext())
            {
                //var sql = @"
                //    SELECT s.SubscriberID, s.FirstName, s.LastName, u.UserName,r.Id as RoleID, r.Name As Role, sa.AddressID, s.IsActive
                //    FROM AspNetUsers u
                //    LEFT JOIN AspNetUserRoles ur ON  ur.UserId = u.Id 
                //    LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId
                //    LEFT JOIN Subscribers s ON s.SubscriberID = u.Id			
                //    LEFT JOIN Subscriber_Address sa ON sa.AddressID = s.AddressID";
                ////WHERE AspNetUsers.Id = @Id";
                ////var idParam = new SqlParameter("Id", theUserId);

                //var result = await context.Database.SqlQuery<UsersWithRoles>(sql).ToListAsync();
                await Task.FromResult(0);
                return View();
            }
           
        }

        [HttpPost]
        [Route]
        public async Task<ActionResult> Index(DataTableParameters dataTableParameters)
        {
            var usersRoles = (from u in db.Users
                                join r in db.Roles on u.Id equals r.Id into rj
                                from r in rj.DefaultIfEmpty()
                                join s in db.subscribers on u.Id equals s.SubscriberID into sj
                                from s in sj.DefaultIfEmpty()
                                join sa in db.subscriber_address on s.AddressID equals sa.AddressID into saj
                                from sa in saj.DefaultIfEmpty()
                                select new UsersWithRoles
                                {
                                    SubscriberID = s.SubscriberID,
                                    FirstName = s.FirstName,
                                    LastName = s.LastName,
                                    UserName = u.UserName,
                                    RoleID = r.Id,
                                    Role = r.Name,
                                    AddressID = sa.AddressID,
                                    IsActive = s.IsActive
                                }).AsEnumerable();


            var searchTerm = dataTableParameters.search?.value;
            var filteredData = usersRoles.AsQueryable();
            try
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredData = filteredData.Where(x => x.UserName.Contains(searchTerm) || x.FirstName.Contains(searchTerm) || x.LastName.Contains(searchTerm));
                }

                // Order the data by the specified column and direction
                if (!string.IsNullOrEmpty(dataTableParameters.order?.FirstOrDefault()?.column.ToString()))
                {
                    var sortColumnIndex = int.Parse(dataTableParameters.order.FirstOrDefault().column.ToString());
                    var sortColumn = dataTableParameters.columns[sortColumnIndex].data;
                    var sortDirection = dataTableParameters.order.FirstOrDefault().dir == "desc" ? "OrderByDescending" : "OrderBy";
                    var property = typeof(UsersWithRoles).GetProperty(sortColumn);
                    var parameter = Expression.Parameter(typeof(UsersWithRoles), "p");
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    var resultExp = Expression.Call(typeof(Queryable), sortDirection, new Type[] { typeof(UsersWithRoles), property.PropertyType }, filteredData.Expression, Expression.Quote(orderByExp));
                    filteredData = filteredData.Provider.CreateQuery<UsersWithRoles>(resultExp);
                }

                var filteredCount = await filteredData.CountAsync(); // Get the filtered count
                var pageData = filteredData.Skip(dataTableParameters.start).Take(dataTableParameters.length);

                var pageDataList = await pageData.ToListAsync();


                var filteredDataList = await filteredData.ToListAsync();

                return Json(new
                {
                    draw = dataTableParameters.draw,
                    recordsTotal = usersRoles.Count(),
                    recordsFiltered = filteredCount,
                    data = pageDataList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
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
            _actLog.SubscriberID = User.Identity.GetUserId();
            _actLog.EmailAddress = usersWithRoles.UserName;
            _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Circulation");

            if (ModelState.IsValid)
            {
                var store = new UserStore<ApplicationUser>(db);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = new ApplicationUser();

                try
                {
                    //AspnetUser
                    user = manager.FindById(usersWithRoles.SubscriberID);
                    //log
                    if (user.Email != usersWithRoles.UserName)
                    {
                        _actLog.LogInformation = "Updated email address from (" + user.Email + ")";
                        Util.LogUserActivity(_actLog);
                    }

                    user.Email = usersWithRoles.UserName;
                    user.UserName = usersWithRoles.UserName;
                    await manager.UpdateAsync(user);

                   

                    //Subscribers
                    Subscriber subscriber = await db.subscribers.FindAsync(usersWithRoles.SubscriberID);
                    //log
                    if (subscriber.IsActive != (bool)usersWithRoles.IsActive)
                    {
                        _actLog.LogInformation = "Update user active status from " + subscriber.IsActive + " to " + (bool)usersWithRoles.IsActive;
                        Util.LogUserActivity(_actLog);
                    }

                    if (subscriber != null)
                    {
                        db.Entry(subscriber).State = EntityState.Modified;
                        
                        subscriber.FirstName = usersWithRoles.FirstName;
                        subscriber.LastName = usersWithRoles.LastName;
                        subscriber.EmailAddress = usersWithRoles.UserName;
                        subscriber.IsActive = (bool)usersWithRoles.IsActive;
                        await db.SaveChangesAsync();
                    }

                    //Epaper
                    List<Subscriber_Epaper> subscriber_Epaper = await db.subscriber_epaper.Where(x => x.SubscriberID == usersWithRoles.SubscriberID).ToListAsync();

                    if (subscriber_Epaper != null)
                    {              
                        foreach (var item in subscriber_Epaper)
                        {
                            db.Entry(item).State = EntityState.Modified;
                            item.EmailAddress = usersWithRoles.UserName;
                        }
                        await db.SaveChangesAsync();
                    }

                    //Print
                    List<Subscriber_Print> subscriber_Print = await db.subscriber_print.Where(x => x.SubscriberID == usersWithRoles.SubscriberID).ToListAsync();
                    if (subscriber_Print != null)
                    {
                        
                        foreach (var item in subscriber_Print)
                        {
                            db.Entry(item).State = EntityState.Modified;
                            item.EmailAddress = usersWithRoles.UserName;
                        }
                        await db.SaveChangesAsync();
                    }

                    //Transactions
                    List<Subscriber_Tranx> subscriber_Tranx = await db.subscriber_tranx.Where(x => x.SubscriberID == usersWithRoles.SubscriberID).ToListAsync();
                    if (subscriber_Tranx != null)
                    {
                        

                        foreach (var item in subscriber_Tranx)
                        {
                            db.Entry(item).State = EntityState.Modified;
                            item.EmailAddress = usersWithRoles.UserName;
                            item.CardOwner = usersWithRoles.FirstName + " " + usersWithRoles.LastName;
                        }
                        await db.SaveChangesAsync();
                    }


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
                catch (Exception ex)
                {
                    Util.LogError(ex);
                    return View();
                }
            }

            return View();
        }
        // GET: Subscribers/Delete/5
        [Route("delete/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
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

        // POST: Subscribers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            bool result = false;

            try
            {
                AccountController account = new AccountController();
                account.InitializeController(this.Request.RequestContext);

                var applicationUser = await account.UserManager.FindByIdAsync(id);
                string emailAddress = applicationUser.UserName;

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBEntities"].ConnectionString))
                {
                    using (var command = new SqlCommand("dbo.ResetSubscriber", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@EmailAddress", SqlDbType.NVarChar).Value = emailAddress;
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                        connection.Close();
                        result = true;

                        _actLog.SubscriberID = User.Identity.GetUserId();
                        _actLog.EmailAddress = User.Identity.GetUserName();
                        _actLog.Role = (User.IsInRole("Admin") ? "Admin" : "Circulation");
                        _actLog.LogInformation = "Removed user " + emailAddress + " from the database";
                        Util.LogUserActivity(_actLog);
                    }
                }

            }
            catch (Exception ex)
            {
                result = false;
                Util.LogError(ex);
            }

            return RedirectToAction("Index");
        }
    }
}