namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber_Print
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Subscriber_PrintID { get; set; }
        public int SubscriberID { get; set; }
        public string EmailAddress { get; set; }
        public int RateID { get; set; }
        public int AddressID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string DeliveryInstructions { get; set; }
        public string Circprosubid { get; set; }
        public System.DateTime CreatedAt { get; set; }

        [ForeignKey("SubscriberID")]
        [Required]
        public Subscriber Subscriber { get; set; }
        public Subscriber_Address Address { get; set; }
    }
}
