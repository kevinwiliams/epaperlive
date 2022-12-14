using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ePaperLive.DBModel;
using System.Web.Mvc;

namespace ePaperLive.Models
{
    public class DeliveryAddress
    {
      
        [Display(Name = "Address Line 1")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Provide an address")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Email address is required")]
        public string AddressLine2 { get; set; }

        [Display(Name = "City/Town")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Enter a city or closest town")]
        public string CityTown { get; set; }

        [Display(Name = "State/Parish")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Enter a state or parish")]
        public string StateParish { get; set; }
        
        [Display(Name = "Zip")]
        public string ZipCode { get; set; }

        [Display(Name = "Country")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please select a country")]
        public string CountryCode { get; set; }

        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
        public string AddressType { get; set; }

        public List<SelectListItem> CountryList { get; set; }

    }

    public class District
    {
        public string ParishName { get; set; }
        public string TownName { get; set; }
        public static IQueryable<District> GetDistrict()
        {
            using (var context = new ApplicationDbContext())
            {
                var result = context.delivery_zones.Where(x => x.IsActive == true);

                if (result != null)
                {
                    List<District> TownParish = new List<District>();
                    foreach (var district in result)
                    {
                        TownParish.Add( new District { 
                                    ParishName = district.Parish, 
                                    TownName = district.Town 
                            });
                    }
                   
                    return TownParish.AsQueryable();
                }
            }

            return new List<District> { }.AsQueryable();
        }
    }

}