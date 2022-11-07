namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    
    public class Subscriber_Roles
    {
        public Subscriber_Roles()
        {
            this.Subscribers = new List<Subscriber>();
        }
    
        public int Subscriber_RolesID { get; set; }
        public string RoleDescription { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual ICollection<Subscriber> Subscribers { get; set; }
    }
}
