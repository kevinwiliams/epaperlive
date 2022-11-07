using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ePaperLive.DBModel;

namespace ePaperLive.Models
{
    public class AddressDetails
    {
      
        [Display(Name = "Address Line 1")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Provide an address")]
        public string addressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Email address is required")]
        public string addressLine2 { get; set; }

        [Display(Name = "City/Town")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a city or closest town")]
        public string cityTown { get; set; }

        [Display(Name = "State/Parish")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a state or parish")]
        public string stateParish { get; set; }
        
        [Display(Name = "Zip")]
        public string zipCode { get; set; }

        [Display(Name = "Country")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a country")]
        public string country { get; set; }

        [Display(Name = "Phone Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid phone number")]
        public string phone { get; set; }

        
    }

}