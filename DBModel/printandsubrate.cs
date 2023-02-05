namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class printandsubrate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Rateid { get; set; }
        [StringLength(25)]
        public string Market { get; set; }
        [StringLength(25)]
        public string Type { get; set; }
        public string RateDescr { get; set; }
        [StringLength(10)]
        public string PrintDayPattern { get; set; }
        public Nullable<int> PrintTerm { get; set; }
        [StringLength(25)]
        public string PrintTermUnit { get; set; }
        [StringLength(10)]
        public string EDayPattern { get; set; }
        public Nullable<int> ETerm { get; set; }
        [StringLength(25)]
        public string ETermUnit { get; set; }
        [StringLength(10)]
        public string Curr { get; set; }
        public Nullable<double> Rate { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<bool> Active { get; set; }
        public double IntroRate { get; set; } = 0;
        public bool OfferIntroRate { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public bool IsCTA { get; set; } = false;
        public string FeatureList { get; set; }
        public bool BestDealFlag { get; set; } = false;
        public bool IsRenewalRate { get; set; } = false;


    }
}
