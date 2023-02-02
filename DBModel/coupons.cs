namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Coupons
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CouponID { get; set; }
        [StringLength(25)]
        public string CouponCode { get; set; }
        public int SubDays { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.DateTime> UsedDate { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
