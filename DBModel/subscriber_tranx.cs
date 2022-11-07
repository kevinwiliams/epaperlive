namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class subscriber_tranx
    {
        public int TranxID { get; set; }
        public int SubscriberID { get; set; }
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
        public virtual subscriber Subscriber { get; set; }
    }
}
