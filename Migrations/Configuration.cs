namespace ePaperLive.Migrations
{
    using ePaperLive.DBModel;
    using ePaperLive.Models;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ePaperLive.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ePaperLive.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            // Define role names
            string adminRole = "Admin",
             adminEmail = "admin@jamaicaobserver.com";
            
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);

            // Create roles if the don't exist
            context.Roles.AddOrUpdate( r => r.Name,
                new IdentityRole() { Name = adminRole,  },
                new IdentityRole() { Name = "Staff" },
                new IdentityRole() { Name = "Subscriber" },
                new IdentityRole() { Name = "Circulation" }
                );

            context.SaveChanges();

            // Create an admin and add to role
            var adminPwd = ConfigurationManager.AppSettings["adminPassword"] ?? "Password37!";
            var hashedPwd = userManager.PasswordHasher.HashPassword(adminPwd);

            var adminUser = new ApplicationUser()
            {
                Id = Guid.NewGuid().ToString(),
                Email = adminEmail,
                PasswordHash = hashedPwd,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                LockoutEnabled = true
            };

            context.Users.AddOrUpdate(u => u.Email, adminUser);
            context.SaveChanges();

            if (!userManager.IsInRole(adminUser.Id, adminRole))
                userManager.AddToRole(adminUser.Id, adminRole);

        }
        
    }
}
