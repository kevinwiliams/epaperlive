using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ePaperLive.Models
{
    public class LoginDetails
    {

        [Display(Name = "First Name")]
        [MinLength(2)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email address is required")]
        public string EmailAddress { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Confirm Password is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Must have a minimum length of 5.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public UserLocation location { get; set; }
    }
}