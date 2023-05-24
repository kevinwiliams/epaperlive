namespace ePaperLive.DBModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    public class school_govt_rates
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SchGovtID { get; set; }
        public Nullable<int> ParentRateID { get; set; }
        [StringLength(50)]
        public string Domains { get; set; }
        [StringLength(5)]
        public string Category { get; set; }
        [StringLength(5)]
        public string RateDescr { get; set; }
        [StringLength(10)]
        public string Curr { get; set; }
        public double Rate { get; set; }
        public int Term { get; set; }
        [StringLength(10)]
        public string Units { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public bool Active { get; set; }
       
    }
}