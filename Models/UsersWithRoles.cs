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
        public string SubscriberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string RoleID { get; set; }
        public string Role { get; set; }
        public Nullable<int> AddressID { get; set; }
        public Nullable<bool> IsActive { get; set; }

    }

    public class SignUpsList
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")] 
        public DateTime SignUpDate { get; set; }
        public int Total { get; set; }
    }
}