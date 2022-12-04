namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class JOL_UserSession
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionID { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string RootObject { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
        public string LastPageVisited { get; set; }
    }
}
