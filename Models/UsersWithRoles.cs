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

    public class LocationReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PlanDesc { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<DateTime> TransactionDate { get; set; }
        public string IpAddress { get; set; }
        public string OrderNumber { get; set; }
    }

    public class CorporateParent
    {
        public string SubscriberID { get; set; }
        public string ParentName { get; set; }
        public string EmailAddress { get; set; }
        public List<string> ChildAddress { get; set; }
    }
}