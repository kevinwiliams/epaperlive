using FACGatewayService;
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
        [StringLength(19, MinimumLength = 19, ErrorMessage = "Please enter a valid card number")] // Just to accomodate mask
        public string CardNumber { get; set; }

        [Display(Name = "CVV")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter card CVV number")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "Please enter 3 digit CVV.")]
        public string CardCVV { get; set; }

        [Display(Name = "Expiration Date")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter card expiration date")]
        public string CardExp { get; set; }
        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal CardAmount { get; set; }
        public string Currency { get; set; }
        public string AuthorizationCode { get; set; }

        [Required]
        public int RateID { get; set; }
        public int OrderID { get; set; }

        [Display(Name = "Type")]
        public string SubType { get; set; }
        [Display(Name = "Subcription")]
        public string RateDescription { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime? TranxDate { get; set; }
        [Display(Name = "Promo Code")]
        public string PromoCode { get; set; }
        public string CardNumberLastFour { get; set; }
        public AddressDetails BillingAddress { get; set; }
        public TransactionSummary TransactionSummary { get; set; }
        public bool EnrolledIn3DSecure { get; set; }
        public bool IsMadeLiveSuccessful { get; set; }
        public string EmailAddress { get; set; }
        public string ConfirmationNumber { get; set; }
        public string OrderNumber { get; set; }
        public bool IsExtension { get; set; }
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