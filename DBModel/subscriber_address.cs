namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class subscriber_address
    {
          
        public int addressID { get; set; }
        public int SubscriberID { get; set; }
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
        public virtual subscriber Subscriber { get; set; }
    }
}
