using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using ePaperLive.DBModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ePaperLive.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        //add subsciber to Idenity Class
        public subscriber Subscriber { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DBEntities", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<subscriber>()
                .HasRequired(u => u.ApplicationUser).WithRequiredDependent(c => c.Subscriber);
        }

        public virtual DbSet<printandsubrate> printandsubrates { get; set; }
        public virtual DbSet<subscriber> subscribers { get; set; }
        public virtual DbSet<subscriber_address> subscriber_address { get; set; }
        public virtual DbSet<subscriber_epaper> subscriber_epaper { get; set; }
        public virtual DbSet<subscriber_print> subscriber_print { get; set; }
        public virtual DbSet<subscriber_roles> subscriber_roles { get; set; }
        public virtual DbSet<subscriber_tranx> subscriber_tranx { get; set; }

    }
}