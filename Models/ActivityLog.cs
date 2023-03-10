using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class ActivityLog
    {
        public string SubscriberID { get; set; }
        public string Role { get; set; }
        public string EmailAddress { get; set; }
        public string IPAddress { get; set; }
        public string LogInformation { get; set; }
        public string SystemInformation { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}