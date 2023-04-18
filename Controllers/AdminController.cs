using ePaperLive.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ePaperLive.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("Admin")]
    [Route("action = index")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        [Route("addrole")]
        public ActionResult AddRole()
        {
               List<RoleViewModel> roleViewModels = new List<RoleViewModel>();
            
            try
            {
                var userRoles = new SelectList(db.Roles.Where(u => !u.Name.Contains("Admins"))
                                   .ToList(), "Name", "Name");

                var uRoles = db.Roles.ToList();

                foreach (var role in uRoles)
                {
                    var rvm = new RoleViewModel
                    {
                        Id = role.Id,
                        Name = role.Name
                    };
                    roleViewModels.Add(rvm);
                }

                return View(roleViewModels);


            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return View(roleViewModels);

            }
        }

        [HttpPost]
        [Route("addrole")]
        public async Task<ActionResult> AddRole(string roleName)
        {
            List<RoleViewModel> roleViewModels = new List<RoleViewModel>();

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new ApplicationUserManager(userStore);
                    // Create roles if the don't exist
                    context.Roles.AddOrUpdate(r => r.Name, new IdentityRole() { Name = roleName });

                    await context.SaveChangesAsync();

                    foreach (var role in context.Roles.ToList())
                    {
                        var rvm = new RoleViewModel
                        {
                            Id = role.Id,
                            Name = role.Name
                        };
                        roleViewModels.Add(rvm);
                    }


                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
            }

            return View(roleViewModels);

        }
    }
}