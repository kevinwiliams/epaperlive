namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber_Epaper
    {
        // This was changed  to follow convention and prevent errors
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Subscriber_EpaperID { get; set; }
        public int SubscriberID { get; set; }
        public string EmailAddress { get; set; }
        public int RateID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string SubType { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string NotificationEmail { get; set; }

        [ForeignKey("SubscriberID")]
        [Required]
        public Subscriber Subscriber { get; set; }
    }
}
