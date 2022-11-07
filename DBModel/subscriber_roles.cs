namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class subscriber_roles
    {
        public subscriber_roles()
        {
            this.subscribers = new HashSet<subscriber>();
        }
    
        public int RoleID { get; set; }
        public string RoleDescription { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual ICollection<subscriber> subscribers { get; set; }
    }
}
