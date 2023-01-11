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
                new IdentityRole() { Name = "Subscriber" }
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
            

            context.printandsubrates.AddOrUpdate(x => x.Rateid,
                new printandsubrate() { Rateid = 1, Market = "Local", Type = "Epaper", RateDescr = "1 Month (30 Days)", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "JMD", Rate = 1199, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 2, Market = "Local", Type = "Epaper", RateDescr = "6 Months (180 Days)", EDayPattern = "1111111", ETerm = 180, ETermUnit = "Days", Curr = "JMD", Rate = 7499, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 3, Market = "Local", Type = "Epaper", RateDescr = "1 Year (360 Days)", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 11999, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 4, Market = "Local", Type = "Epaper", RateDescr = "1 Month (30 Days) - Free", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "JMD", Rate = 0, SortOrder = 4, Active = false },
                new printandsubrate() { Rateid = 5, Market = "International", Type = "Epaper", RateDescr = "1 Month (30 Days)", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "USD", Rate = 7.99, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 6, Market = "International", Type = "Epaper", RateDescr = "6 Months (180 Days)", EDayPattern = "1111111", ETerm = 180, ETermUnit = "Days", Curr = "USD", Rate = 44.99, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 7, Market = "International", Type = "Epaper", RateDescr = "12 Months (360 Days)", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "USD", Rate = 19.99, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 8, Market = "International", Type = "Epaper", RateDescr = "1 Month (30 Days) - Free", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "USD", Rate = 0, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 9, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 740, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 10, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 13, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 2405, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 11, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 26, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 4810, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 12, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 52, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 9620, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 13, Market = "Local", Type = "Print", RateDescr = "Saturday & Sunday Only", PrintDayPattern = "1000001", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 1180, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 14, Market = "Local", Type = "Print", RateDescr = "Saturday & Sunday Only", PrintDayPattern = "1000001", PrintTerm = 13, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 3644, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 15, Market = "Local", Type = "Print", RateDescr = "Saturday & Sunday Only", PrintDayPattern = "1000001", PrintTerm = 26, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 7095, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 16, Market = "Local", Type = "Print", RateDescr = "Saturday & Sunday Only", PrintDayPattern = "1000001", PrintTerm = 52, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 13806, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 17, Market = "Local", Type = "Print", RateDescr = "Monday to Friday", PrintDayPattern = "0111110", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 2200, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 18, Market = "Local", Type = "Print", RateDescr = "Monday to Friday", PrintDayPattern = "0111110", PrintTerm = 13, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 6897, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 19, Market = "Local", Type = "Print", RateDescr = "Monday to Friday", PrintDayPattern = "0111110", PrintTerm = 26, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 13228, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 20, Market = "Local", Type = "Print", RateDescr = "Monday to Friday", PrintDayPattern = "0111110", PrintTerm = 52, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 25641, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 21, Market = "Local", Type = "Print", RateDescr = "Monday to Saturday", PrintDayPattern = "0111111", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 2640, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 22, Market = "Local", Type = "Print", RateDescr = "Monday to Saturday", PrintDayPattern = "0111111", PrintTerm = 13, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 8256, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 23, Market = "Local", Type = "Print", RateDescr = "Monday to Saturday", PrintDayPattern = "0111111", PrintTerm = 26, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 15873, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 24, Market = "Local", Type = "Print", RateDescr = "Monday to Saturday", PrintDayPattern = "0111111", PrintTerm = 52, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 30789, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 25, Market = "Local", Type = "Print", RateDescr = "Monday to Sunday", PrintDayPattern = "1111111", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 3380, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 26, Market = "Local", Type = "Print", RateDescr = "Monday to Sunday", PrintDayPattern = "1111111", PrintTerm = 13, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 10541, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 27, Market = "Local", Type = "Print", RateDescr = "Monday to Sunday", PrintDayPattern = "1111111", PrintTerm = 26, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 20323, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 28, Market = "Local", Type = "Print", RateDescr = "Monday to Sunday", PrintDayPattern = "1111111", PrintTerm = 52, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 39447, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 29, Market = "Local", Type = "Bundle", RateDescr = "1 Year - Print (Sunday to Saturday)  / 1 Year - ePaper", PrintDayPattern = "1111111", PrintTerm = 52, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 27835.2, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 30, Market = "Local", Type = "Bundle", RateDescr = "26 weeks - Print (Sunday to Saturday)  / 1 Year - ePaper", PrintDayPattern = "1111111", PrintTerm = 26, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 20156.5, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 31, Market = "Local", Type = "Bundle", RateDescr = "13 weeks - Print (Sunday to Saturday)  / 1 Year - ePaper", PrintDayPattern = "1111111", PrintTerm = 13, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 16135.3, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 32, Market = "International", Type = "Print", RateDescr = "Saturday Only", PrintDayPattern = "0000001", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "USD", Rate = 25, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 33, Market = "International", Type = "Print", RateDescr = "Monday - Friday", PrintDayPattern = "0111110", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "USD", Rate = 80, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 34, Market = "International", Type = "Print", RateDescr = "Monday - Saturday", PrintDayPattern = "0111111", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "USD", Rate = 120, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 35, Market = "International", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "USD", Rate = 50, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 36, Market = "International", Type = "Print", RateDescr = "Sunday & Saturday Only", PrintDayPattern = "1000001", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "USD", Rate = 75, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 37, Market = "International", Type = "Print", RateDescr = "Sunday - Saturday", PrintDayPattern = "1111111", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "USD", Rate = 200, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 38, Market = "International", Type = "Print", RateDescr = "Business", PrintDayPattern = "1001010", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "USD", Rate = 180, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 39, Market = "International", Type = "Bundle", RateDescr = "13 weeks - Print (Sunday to Saturday)  / 1 Year - ePaper", PrintDayPattern = "1111111", PrintTerm = 13, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "USD", Rate = 175, SortOrder = 1, Active = true }
            );
            context.SaveChanges();

        }
        
    }
}
