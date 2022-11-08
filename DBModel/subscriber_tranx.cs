namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Subscriber_Tranx
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Subscriber_TranxID { get; set; }
        public string SubscriberID { get; set; }
        public string EmailAddress { get; set; }
        public string CardOwner { get; set; }
        public string CardType { get; set; }
        public string CardExp { get; set; }
        public string CardLastFour { get; set; }
        public Nullable<double> TranxAmount { get; set; }
        public Nullable<System.DateTime> TranxDate { get; set; }
        public Nullable<int> RateID { get; set; }
        public string TranxType { get; set; }
        public string OrderID { get; set; }
        public string TranxNotes { get; set; }
        public string IpAddress { get; set; }

        [ForeignKey("SubscriberID")]
        [Required]
        public virtual Subscriber Subscriber { get; set; }
    }
}
