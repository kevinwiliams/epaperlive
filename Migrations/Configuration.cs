namespace ePaperLive.Migrations
{
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
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ePaperLive.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            // Define role names
            string adminRole = "Admin",
             staffRole = "Staff",
             subscriberRole = "Subscriber",
             adminEmail = "admin@jamaicaobserver.com";
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);

            // Create roles if the don't exist
            if (!context.Roles.Any(r => r.Name == adminRole))
            {
                context.Roles.Add(new IdentityRole() { Name = adminRole});
            }

            if (!context.Roles.Any(r => r.Name == staffRole))
            {
                context.Roles.Add(new IdentityRole() { Name = staffRole });
            }

            if (!context.Roles.Any(r => r.Name == subscriberRole))
            {
                context.Roles.Add(new IdentityRole() { Name = subscriberRole });
            }


            // Create an admin and add to role
            if (!context.Users.Any(u => u.Email == adminEmail))
            {
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
                context.Users.Add(adminUser);
                context.SaveChanges();

                userManager.AddToRole(adminUser.Id, adminRole);
            }

        }
    }
}
