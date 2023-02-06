using ePaperLive.DBModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class PrintSubRates
    {
        [Key]
        public int RateID { get; set; }
        public string RateType { get; set; }
        public IEnumerable<printandsubrate> Rates { get; set; }
        public bool IsRenewal { get; set; }
    }
}