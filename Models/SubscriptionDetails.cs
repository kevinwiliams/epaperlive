using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ePaperLive.DBModel;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class SubscriptionDetails
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a plan to proceed")]
        [Key]
        public int RateID { get; set; }
        public string RateDescription { get; set; }
        public string SubType { get; set; }

        [Display(Name = "Subscription Start Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Display(Name = "Email address for promotions and updates")]
        public string NotificationEmail { get; set; }

        [Display(Name = "Special delivery instructions")]
        public string DeliveryInstructions { get; set; }

        [Display(Name = "Newsletter")]
        public bool newsletterSignUp { get; set; }


        [Display(Name = "Terms & Conditions")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please select terms and conditions")]
        [MustBeTrue(ErrorMessage = "Please select terms and conditions!")]
        [Required(ErrorMessage = "Please select terms and conditions")]
        public bool TermsAndCon { get; set; }

        public IEnumerable<printandsubrate> RatesList { get; set; }

        [Display(Name = "Same as mailing address?")]
        public bool SameAsMailing { get; set; }

        public AddressDetails deliveryAddress { get; set; }

    }

    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }
}