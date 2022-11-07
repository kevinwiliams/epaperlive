using ePaperLive.DBModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ePaperLive.Models
{
    public class Reader
    {
        public string call { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int userid { get; set; }
        public string token { get; set; }
    }

    public class member
    {
        public int userID { get; set; }
        public string loginName { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string homeareacode { get; set; }
        public string homephone { get; set; }
        public string workareacode { get; set; }
        public string workphone { get; set; }
        public string address { get; set; }
        public string apartment { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string postalcode { get; set; }
        public string country { get; set; }
        public string gender { get; set; }
        public string nickname { get; set; }
        public string subscription { get; set; }
        public string expiration { get; set; }
    }

    public class error
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    
}