namespace ePaperLive.DBModel
{
    using ePaperLive.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class subscriber
    {
        public subscriber()
        {
            this.subscriber_epaper = new HashSet<subscriber_epaper>();
            this.subscriber_print = new HashSet<subscriber_print>();
            this.subscriber_tranx = new HashSet<subscriber_tranx>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Key, ForeignKey("ApplicationUser")]
        public int SubscriberID { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Secretquestion { get; set; }
        public string Secretans { get; set; }
        public string IpAddress { get; set; }
        public bool isActive { get; set; }
        public Nullable<int> AddressID { get; set; }
        
        public Nullable<bool> Newsletter { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<int> RoleID { get; set; }
        public string Token { get; set; }
        public Nullable<int> CCHashID { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }

        //Navigation properties
        [ForeignKey("SubscriberID")]
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        //Navigation properties
        [ForeignKey("AddressID")]
        public virtual ICollection<subscriber_address> Subscriber_Address { get; set; }
        public virtual ICollection<subscriber_epaper> Subscriber_Epaper { get; set; }
        public virtual ICollection<subscriber_print> Subscriber_Print { get; set; }
        public virtual subscriber_roles Subscriber_Roles { get; set; }
        public virtual ICollection<subscriber_tranx> Subscriber_Tranx { get; set; }
    }
}
