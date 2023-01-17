namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Promocodes
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromoCodeID { get; set; }
        [StringLength(25)]
        public string PromoCode { get; set; }
        [StringLength(50)]
        public string PromoDescription { get; set; }
        public double Discount { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<bool> Active { get; set; }
        public string PromoType { get; set; }
        public string PromoMarket { get; set; }
    }
}
