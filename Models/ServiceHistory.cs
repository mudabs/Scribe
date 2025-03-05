using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Scribe.Models
{
    public class ServiceHistory
    {
        public int Id { get; set; }
        [DisplayName("Serial Number")]
        public int SerialNumberId { get; set; }
        public SerialNumber? SerialNumber { get; set; } // Navigation property
        [DisplayName("Description")]
        public string? ServiceDescription { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Service Date")]
        public DateTime ServiceDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Next Service")]
        public DateTime NextServiceDate { get; set; }
        [DisplayName("Condition")]
        public int ConditionId { get; set; }
        public Condition? Condition { get; set; }
        [DisplayName("User")]
        public string? SystemUserId { get; set; }
    }
}
