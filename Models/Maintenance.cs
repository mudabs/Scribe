using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Scribe.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        [DisplayName("Serial Number")]
        public int SerialNumberId { get; set; }
        public SerialNumber? SerialNumber { get; set; } // Navigation property
        
        [DisplayName("Description")]
        public string? ServiceDescription { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Service Date")]
        public DateTime ServiceDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Next Service")]
        public DateTime NextServiceDate { get; set; }
        [DisplayName("Condition")]
        public int ConditionId { get; set; }
        public Condition? Condition { get; set; }
        [DisplayName("Serviced By")]
        public string? SystemUserId { get; set; }
    }
}
