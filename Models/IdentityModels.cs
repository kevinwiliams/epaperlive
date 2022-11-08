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
        public Subscriber Subscriber { get; set; }
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
            modelBuilder.Entity<Subscriber>()
                .HasRequired(u => u.ApplicationUser).WithRequiredDependent(c => c.Subscriber);
        }

        public virtual DbSet<printandsubrate> printandsubrates { get; set; }
        public virtual DbSet<Subscriber> subscribers { get; set; }
        public virtual DbSet<Subscriber_Address> subscriber_address { get; set; }
        public virtual DbSet<Subscriber_Epaper> subscriber_epaper { get; set; }
        public virtual DbSet<Subscriber_Print> subscriber_print { get; set; }
        public virtual DbSet<Subscriber_Roles> subscriber_roles { get; set; }
        public virtual DbSet<Subscriber_Tranx> subscriber_tranx { get; set; }

    }
}