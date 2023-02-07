using ePaperLive.DBModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class UsersWithRoles
    {
     
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public Nullable<int> AddressID { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string SubscriberID { get; set; }

    }
}