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
        [StringLength(128)]
        public string SubscriberID { get; set; }
        [StringLength(5)]
        public string AddressType { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(100)]
        public string AddressLine1 { get; set; }
        [StringLength(100)]
        public string AddressLine2 { get; set; }
        [StringLength(50)]
        public string CityTown { get; set; }
        [StringLength(50)]
        public string StateParish { get; set; }
        [StringLength(10)]
        public string ZipCode { get; set; }
        [StringLength(50)]
        public string CountryCode { get; set; }
        public System.DateTime CreatedAt { get; set; }
        [ForeignKey("SubscriberID")]
        public virtual Subscriber Subscriber { get; set; }
    }
}
