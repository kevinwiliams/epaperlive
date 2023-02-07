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
    public class Eusers
    {
        public int Recid { get; set; }
        public string Email { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Province { get; set; }
        public int? Zip { get; set; }
        public string Country { get; set; }
        public DateTime SubscriptionStart { get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public string Active { get; set; }
        public string Newsletter { get; set; }
        public decimal CardAmount { get; set; }
        public string CardType { get; set; }
        public string CardOwnerName { get; set; }
        public string OrderId { get; set; }
        public string SecretQuestion { get; set; }
        public string SecretAnswer { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Ip { get; set; }
    }

}