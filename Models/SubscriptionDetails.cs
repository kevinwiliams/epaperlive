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
        public int rateID { get; set; }
        public string subType { get; set; }

        [Display(Name = "Subscription Start Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        [Display(Name = "Email address for promotions and updates")]
        public string notificationEmail { get; set; }

        [Display(Name = "Special delivery instructions")]
        public string deliveryInstructions { get; set; }

        [Display(Name = "Newsletter")]
        public bool newsletterSignUp { get; set; }


        [Display(Name = "Terms & Conditions")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please select terms and conditions")]
        [MustBeTrue(ErrorMessage = "Please select terms and conditions!")]
        [Required(ErrorMessage = "Please select terms and conditions")]
        public bool termsAndCon { get; set; }

        public IEnumerable<printandsubrate> RatesList { get; set; }

        [Display(Name = "Same as mailing address?")]
        public bool sameAsMailing { get; set; }

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