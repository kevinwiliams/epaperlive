namespace ePaperLive.DBModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Delivery_Zones
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeliveryAreaID { get; set; }
        [StringLength(100)]
        public string Town { get; set; }
        [StringLength(100)]
        public string Neighbourhood { get; set; }
        [StringLength(50)]
        public string Parish { get; set; }
        [StringLength(12)]
        public string PostalCode { get; set; }
        public bool IsActive { get; set; } = true;

        
    }
}
