namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class user_activity_log
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }
        [StringLength(25)]
        public string SubscriberID { get; set; }
        [StringLength(25)]
        public string Role { get; set; }
        [StringLength(150)]
        public string EmailAddress { get; set; }
        public string IPAddress { get; set; }
        public string LogInformation { get; set; }
        public string SystemInformation { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
