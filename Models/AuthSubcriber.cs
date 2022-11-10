using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class AuthSubcriber
    {
        public string SubscriberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<SubscriptionDetails> SubscriptionDetails { get; set; }
        public List<AddressDetails> AddressDetails { get; set; }
        public List<PaymentDetails> PaymentDetails { get; set; }
    }
}