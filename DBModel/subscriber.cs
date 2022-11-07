namespace ePaperLive.DBModel
{
    using ePaperLive.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber
    {
        public Subscriber()
        {
            this.Subscriber_Epaper = new List<Subscriber_Epaper>();
            this.Subscriber_Print = new List<Subscriber_Print>();
            this.Subscriber_Tranx = new List<Subscriber_Tranx>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriberID { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Secretquestion { get; set; }
        public string Secretans { get; set; }
        public string IpAddress { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> AddressID { get; set; }
        
        public Nullable<bool> Newsletter { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<int> RoleID { get; set; }
        public string Token { get; set; }
        public Nullable<int> CCHashID { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }

        //Navigation properties
        
        public ApplicationUser ApplicationUser { get; set; }
        //Navigation properties
        
        public virtual ICollection<Subscriber_Address> Subscriber_Address { get; set; }
        public virtual ICollection<Subscriber_Epaper> Subscriber_Epaper { get; set; }
        public virtual ICollection<Subscriber_Print> Subscriber_Print { get; set; }
        public virtual Subscriber_Roles Subscriber_Roles { get; set; }
        public virtual ICollection<Subscriber_Tranx> Subscriber_Tranx { get; set; }
    }
}
