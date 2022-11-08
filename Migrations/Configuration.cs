namespace ePaperLive.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;

    using System;
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
            string admin = "Admin",
             staff = "Staff",
             subscriber = "Subscriber";

            if (!context.Roles.Any(r => r.Name == admin))
            {
                context.Roles.Add(new IdentityRole() { Name = admin});
            }

            if (!context.Roles.Any(r => r.Name == staff))
            {
                context.Roles.Add(new IdentityRole() { Name = staff });
            }

            if (!context.Roles.Any(r => r.Name == subscriber))
            {
                context.Roles.Add(new IdentityRole() { Name = subscriber });
            }

        }
    }
}
