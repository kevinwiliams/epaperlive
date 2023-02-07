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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Key, ForeignKey("ApplicationUser")]
        [StringLength(128)]
        public string SubscriberID { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        [StringLength(250)]
        public string Secretquestion { get; set; }
        [StringLength(250)]
        public string Secretans { get; set; }
        [StringLength(50)]
        public string IpAddress { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> AddressID { get; set; }
        public Nullable<bool> Newsletter { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string Token { get; set; }
        public Nullable<int> CCHashID { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }

        //Navigation properties
        [ForeignKey("SubscriberID")]
        public ApplicationUser ApplicationUser { get; set; }
        //Navigation properties
        [ForeignKey("AddressID")]
        public virtual ICollection<Subscriber_Address> Subscriber_Address { get; set; }
        public virtual ICollection<Subscriber_Epaper> Subscriber_Epaper { get; set; }
        public virtual ICollection<Subscriber_Print> Subscriber_Print { get; set; }
        public virtual ICollection<Subscriber_Tranx> Subscriber_Tranx { get; set; }
    }
}
