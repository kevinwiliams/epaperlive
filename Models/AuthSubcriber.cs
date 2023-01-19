using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class AuthSubcriber
    {
        public string SubscriberID { get; set; }
        [Display(Name = "First Name")]
        [MinLength(2)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        public string LastPageVisited { get; set; }
        public string PasswordKey { get; set; }
        public List<SubscriptionDetails> SubscriptionDetails { get; set; }
        public List<AddressDetails> AddressDetails { get; set; }
        public List<PaymentDetails> PaymentDetails { get; set; }
        public UserLoginInfo Login { get; set; }
    }
}