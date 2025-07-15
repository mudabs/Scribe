using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scribe.Models
{
    public class SerialNumber
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Serial Number")]
        public string? Name { get; set; }
        [DisplayName("Model")]
        public int? ModelId { get; set; }
        public Model? Model { get; set; }
        [DisplayName("Employee Name")]
        public int? ADUsersId { get; set; }
        public ADUsers? ADUsers { get; set; }
        [DisplayName("Condition")]
        public int? ConditionId { get; set; }
        public Condition? Condition { get; set; }
        [DisplayName("Department")]
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]

        public Department? Department { get; set; }
        [DisplayName("Location")]
        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]

        public Location? Location { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Creation { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Allocation Date")]
        public DateTime? Allocation { get; set; }
        [DisplayName("Allocated By")]
        public string? AllocatedBy { get; set; }
        [DisplayName("Deallocated By")]
        public string? DeallocatedBy { get; set; }
        public List<SerialNumber>? SerialNumbers { get; set; }
        [DisplayName("Group")]
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group? Group { get; set; }
        public bool CurrentlyAllocated { get; set; } = false;
        public string? Description { get; set; }
        public string? MacAddress { get; set; }
    }
}
