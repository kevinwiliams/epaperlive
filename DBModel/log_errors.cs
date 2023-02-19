namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class log_errors
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int errID { get; set; }
        [StringLength(255)]
        public string err_msg { get; set; }
        [StringLength(25)]
        public string err_date { get; set; }
        [StringLength(10)]
        public string err_time { get; set; }
        [StringLength(255)]
        public string err_name { get; set; }
        public string stacktrace { get; set; }

        
    }
}
