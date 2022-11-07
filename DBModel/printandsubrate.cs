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
        public string Market { get; set; }
        public string Type { get; set; }
        public string RateDescr { get; set; }
        public string PrintDayPattern { get; set; }
        public Nullable<int> PrintTerm { get; set; }
        public string PrintTermUnit { get; set; }
        public string EDayPattern { get; set; }
        public Nullable<int> ETerm { get; set; }
        public string ETermUnit { get; set; }
        public string Curr { get; set; }
        public Nullable<double> Rate { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<int> Active { get; set; }
    }
}
