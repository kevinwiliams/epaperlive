using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class CorporateAccount
    {
        public string ParentSubscriberID { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Provide a parent email address")]
        public string EmailAddress { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Provide a list of delimited accounts")]
        public string ManagedAccts { get; set; }
        List<SubscriptionDetails> ManagedAccountsList { get; set; }
    }
}