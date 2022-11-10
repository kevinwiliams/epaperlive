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
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Email address is required")]
        public string AddressLine2 { get; set; }

        [Display(Name = "City/Town")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a city or closest town")]
        public string CityTown { get; set; }

        [Display(Name = "State/Parish")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a state or parish")]
        public string StateParish { get; set; }
        
        [Display(Name = "Zip")]
        public string ZipCode { get; set; }

        [Display(Name = "Country")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select a country")]
        public string CountryCode { get; set; }

        [Display(Name = "Phone Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; }
        public string AddressType { get; set; }


        
    }

}