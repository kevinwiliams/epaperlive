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
                new printandsubrate() { Rateid = 1, Market = "Local", Type = "Epaper", RateDescr = "1 Month (30 Days)", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "JMD", Rate = 1248, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 2, Market = "Local", Type = "Epaper", RateDescr = "3 Months (90 Days)", EDayPattern = "1111111", ETerm = 90, ETermUnit = "Days", Curr = "JMD", Rate = 3743, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 3, Market = "Local", Type = "Epaper", RateDescr = "6 Months (180 Days)", EDayPattern = "1111111", ETerm = 180, ETermUnit = "Days", Curr = "JMD", Rate = 7241, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 4, Market = "Local", Type = "Epaper", RateDescr = "12 Months (360 Days)", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 13919, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 5, Market = "Local", Type = "Epaper", RateDescr = "1 Month (30 Days) - Free", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "JMD", Rate = 0, SortOrder = 0, Active = true },
                new printandsubrate() { Rateid = 6, Market = "International", Type = "Epaper", RateDescr = "1 Month (30 Days)", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "USD", Rate = 15, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 7, Market = "International", Type = "Epaper", RateDescr = "3 Months (90 Days)", EDayPattern = "1111111", ETerm = 90, ETermUnit = "Days", Curr = "USD", Rate = 40, SortOrder = 2, Active = true },
                new printandsubrate() { Rateid = 8, Market = "International", Type = "Epaper", RateDescr = "6 Months (180 Days)", EDayPattern = "1111111", ETerm = 180, ETermUnit = "Days", Curr = "USD", Rate = 80, SortOrder = 3, Active = true },
                new printandsubrate() { Rateid = 9, Market = "International", Type = "Epaper", RateDescr = "12 Months (360 Days)", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "USD", Rate = 140, SortOrder = 4, Active = true },
                new printandsubrate() { Rateid = 10, Market = "International", Type = "Epaper", RateDescr = "1 Month (30 Days) - Free", EDayPattern = "1111111", ETerm = 30, ETermUnit = "Days", Curr = "USD", Rate = 0, SortOrder = 0, Active = true },
                new printandsubrate() { Rateid = 11, Market = "Local", Type = "Print", RateDescr = "Saturday Only", PrintDayPattern = "0000001", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 620.04, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 12, Market = "Local", Type = "Print", RateDescr = "Saturday Only", PrintDayPattern = "0000001", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 1199.33, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 13, Market = "Local", Type = "Print", RateDescr = "Saturday Only", PrintDayPattern = "0000001", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 2305.2, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 14, Market = "Local", Type = "Print", RateDescr = "Monday - Friday", PrintDayPattern = "0111110", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 3100.21, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 15, Market = "Local", Type = "Print", RateDescr = "Monday - Friday", PrintDayPattern = "0111110", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 5996.64, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 16, Market = "Local", Type = "Print", RateDescr = "Monday - Friday", PrintDayPattern = "0111110", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 11527.5, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 17, Market = "Local", Type = "Print", RateDescr = "Monday - Saturday", PrintDayPattern = "0111111", PrintTerm = 4, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 3720.24, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 18, Market = "Local", Type = "Print", RateDescr = "Monday - Saturday", PrintDayPattern = "0111111", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 7195.97, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 19, Market = "Local", Type = "Print", RateDescr = "Monday - Saturday", PrintDayPattern = "0111111", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 13833, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 20, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 1343.43, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 21, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 2595.55, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 22, Market = "Local", Type = "Print", RateDescr = "Sunday Only", PrintDayPattern = "1000000", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 4595.26, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 23, Market = "Local", Type = "Print", RateDescr = "Sunday & Saturday Only", PrintDayPattern = "1000001", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 1963.43, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 24, Market = "Local", Type = "Print", RateDescr = "Sunday & Saturday Only", PrintDayPattern = "1000001", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 3797.87, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 25, Market = "Local", Type = "Print", RateDescr = "Sunday & Saturday Only", PrintDayPattern = "1000001", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 7300.77, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 26, Market = "Local", Type = "Print", RateDescr = "Sunday - Saturday", PrintDayPattern = "1111111", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 5063.67, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 27, Market = "Local", Type = "Print", RateDescr = "Sunday - Saturday", PrintDayPattern = "1111111", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 9794.51, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 28, Market = "Local", Type = "Print", RateDescr = "Sunday - Saturday", PrintDayPattern = "1111111", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 18828.3, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 29, Market = "Local", Type = "Print", RateDescr = "Business", PrintDayPattern = "1001010", PrintTerm = 8, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 4875, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 30, Market = "Local", Type = "Print", RateDescr = "Business", PrintDayPattern = "1001010", PrintTerm = 16, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 8478.26, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 31, Market = "Local", Type = "Print", RateDescr = "Business", PrintDayPattern = "1001010", PrintTerm = 32, PrintTermUnit = "Weeks", Curr = "JMD", Rate = 16956.5, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 32, Market = "Local", Type = "Bundle", RateDescr = "Print - 1 Year Sunday to Saturday   / 1 Year ePaper", PrintDayPattern = "1111111", PrintTerm = 52, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 27835.2, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 33, Market = "Local", Type = "Bundle", RateDescr = "Print - 26 weeks Sunday to Saturday  / 1 Year ePaper", PrintDayPattern = "1111111", PrintTerm = 26, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 20156.5, SortOrder = 1, Active = true },
                new printandsubrate() { Rateid = 34, Market = "Local", Type = "Bundle", RateDescr = "Print - 13 weeks Sunday to Saturday  / 1 Year ePaper", PrintDayPattern = "1111111", PrintTerm = 13, PrintTermUnit = "Weeks", EDayPattern = "1111111", ETerm = 360, ETermUnit = "Days", Curr = "JMD", Rate = 16135.3, SortOrder = 1, Active = true }
            );
            context.SaveChanges();

        }
        
    }
}
