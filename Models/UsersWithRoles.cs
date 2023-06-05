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

    public class PlansList
    {
        public string PlanTitle { get; set; }
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

    public class EPaperSubscriberResult
    {
        public string EmailAddress { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EndDate { get; set; }
        public string SubType { get; set; }
        public string PlanDesc { get; set; }
        public string PhoneNumber { get; set; }
        public string OrderNumber { get; set; }
        public string InitialSubType { get; set; }
        public string InitialOrderId { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime InitialCreated { get; set; }
        public string InitialPlan { get; set; }
        public string IpAddress { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime OldestTranxDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityTown { get; set; }
        public string StateParish { get; set; }
        public string CountryCode { get; set; }
    }
}