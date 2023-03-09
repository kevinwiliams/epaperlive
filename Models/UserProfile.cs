using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class UserProfile
    {
        public string SubscriberID { get; set; }
        [Display(Name = "First Name")]
        [MinLength(2)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        public bool isActive { get; set; }
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<DateTime> DateOfBirth { get; set; }
        public Nullable<int> AddressID { get; set; }
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }
        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }
        [Display(Name = "City/Town")]
        public string CityTown { get; set; }
        [Display(Name = "State/Province")]
        public string StateParish { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "Country")]
        public string CountryCode { get; set; }
        [Display(Name = "Contact Number")]
        public string PhoneNumber { get; set; }
    }
}