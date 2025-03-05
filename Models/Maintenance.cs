using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Scribe.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Maintenance Date")]
        public DateTime? MaintenanceDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Next Maintenance")]
        public DateTime? NextMaintenance { get; set; }
        public string Details { get; set; }
        [DisplayName("Serial Number")]
        public int SerialId { get; set; }
        [ForeignKey("SerialId")]
        public SerialNumber? SerialNumber { get; set; }
        [DisplayName("Condition")]
        public int ConditionId { get; set; }
        public Condition? Condition { get; set; }
    }
}
