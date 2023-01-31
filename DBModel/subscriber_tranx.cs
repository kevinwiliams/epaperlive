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
        [StringLength(128)]
        public string SubscriberID { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        public string CardOwner { get; set; }
        [StringLength(20)]
        public string CardType { get; set; }
        [StringLength(5)]
        public string CardExp { get; set; }
        [StringLength(5)]
        public string CardLastFour { get; set; }
        [StringLength(25)]
        public string PromoCode { get; set; }
        public Nullable<double> TranxAmount { get; set; }
        public Nullable<System.DateTime> TranxDate { get; set; }
        public Nullable<int> RateID { get; set; }
        [StringLength(50)]
        public string TranxType { get; set; }
        [StringLength(50)]
        public string OrderID { get; set; }
        [StringLength(100)]
        public string TranxNotes { get; set; }
        [StringLength(50)]
        public string IpAddress { get; set; }
        public bool IsMadeLiveSuccessful { get; set; } = false;
        public bool EnrolledIn3DSecure { get; set; } = false;
        [StringLength(50)]
        public string AuthCode { get; set; }
        [StringLength(50)]
        public string ConfirmationNo { get; set; }

        [ForeignKey("SubscriberID")]
        public virtual Subscriber Subscriber { get; set; }
        public string PlanDesc { get; set; }
    }
}
