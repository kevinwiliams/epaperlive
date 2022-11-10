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
        public string CardOwner { get; set; }

        [Display(Name = "Type of Card")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a card type")]
        public string CardType { get; set; }

        [Display(Name = "Card Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter card number")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Please enter a valid card number")]
        public string CardNumber { get; set; }

        [Display(Name = "CVV")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter card CVV number")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "Please enter 3 digit CVV.")]
        public string CardCVV { get; set; }

        [Display(Name = "Expiration Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter card expiration date")]
        public string CardExp { get; set; }
        public float CardAmount { get; set; }

        [Required]
        public int RateID;

        public string RateDescription { get; set; }

        public DateTime? TranxDate { get; set; }

    }

    public enum PaymentMethod
    {
        Visa,
        Mastercard,
        KeyCard
    }

    public enum SubscriptionType
    {
        Paid,
        Complimentary,
        Promotion
    }
}