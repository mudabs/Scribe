using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Scribe.Models
{
    public class AllocationHistory
    {
        public int Id { get; set; }
        [DisplayName("Serial Number")]
        public int SerialNumberId { get; set; }
        public SerialNumber? SerialNumber { get; set; } // Navigation property
        [DisplayName("Em[ployee Name")]
        public int ADUsersId { get; set; }
        public ADUsers? ADUsers { get; set; } // Navigation property
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Allocation Date")]
        public DateTime AllocationDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Deallocation Date")]
        public DateTime? DeallocationDate { get; set; } // Nullable
        [DisplayName("Allocated By")]
        public string? AllocatedBy { get; set; }
        [DisplayName("Deallocated By")]
        public string? DeallocatedBy { get; set; }
        [DisplayName("Group Name")]
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
