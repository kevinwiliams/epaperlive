namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber_Address
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }
        public string SubscriberID { get; set; }
        public string AddressType { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityTown { get; set; }
        public string StateParish { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public System.DateTime CreatedAt { get; set; }
        [ForeignKey("SubscriberID")]
        [Required]
        public virtual Subscriber Subscriber { get; set; }
    }
}
