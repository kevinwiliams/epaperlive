using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class PaymentDetails
    {
        [Display(Name = "Name on Card")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter cardholder name")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Must have a minimum length of 5.")]
        public string cardOwner { get; set; }

        [Display(Name = "Type of Card")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a card type")]
        public string cardType { get; set; }

        [Display(Name = "Card Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter cardholder name")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Please enter a valid card number")]
        public string cardNumber { get; set; }

        [Display(Name = "CVV")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter card CVV number")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "Please enter 3 digit CVV.")]
        public string cardCVV { get; set; }

        [Display(Name = "Expiration Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter card expiration date")]
        public string cardExp { get; set; }

        [Required]
        public int rateID;

    }

    public enum PaymentMethod
    {
        Visa,
        Mastercard,
        KeyCard
    }
}