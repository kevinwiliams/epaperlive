using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePaperLive.DBModel
{
    public class ClientSnapShot
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int ClientKey { get; set; }

        public string Email { get; set; }

        [Required]
        public string SnapshotData { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }
    }
}