namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber_Epaper
    {
        // This was changed  to follow convention and prevent errors
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Subscriber_EpaperID { get; set; }
        [StringLength(128)]
        public string SubscriberID { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        public int RateID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        [StringLength(50)]
        public string SubType { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public bool NotificationEmail { get; set; }
        [ForeignKey("SubscriberID")]
        public Subscriber Subscriber { get; set; }
        public Subscriber_Epaper()
        {
            NotificationEmail = false;

        }
    }
}
