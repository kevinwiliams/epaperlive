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
        [Key]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a plan to proceed")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please choose your preferred subscription")]
        public int RateID { get; set; }
        [Display(Name = "Subscription")]
        public string RateDescription { get; set; }
        [Display(Name = "Type")]
        public string SubType { get; set; }
        [MustBeSelected(ErrorMessage = "Please select rates")]
        public string RateType { get; set; }
        public string Market { get; set; }

        [Display(Name = "Start Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Email address for promotions and updates")]
        public string NotificationEmail { get; set; }

        [Display(Name = "Special delivery instructions")]
        public string DeliveryInstructions { get; set; }

        [Display(Name = "Newsletter")]
        public bool NewsletterSignUp { get; set; }


        [Display(Name = "Terms & Conditions")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please select terms and conditions")]
        [MustBeTrue(ErrorMessage = "Please select terms and conditions!")]
        [Required(ErrorMessage = "Please select terms and conditions")]
        public bool TermsAndCon { get; set; }

        public IEnumerable<printandsubrate> RatesList { get; set; }

        [Display(Name = "Same as mailing address?")]
        public bool SameAsMailing { get; set; }

        public AddressDetails DeliveryAddress { get; set; }

    }

    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }

    public class MustBeSelectedAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;
            else
                return true;
        }
    }

}