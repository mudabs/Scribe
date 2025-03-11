using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Scribe.Models
{
    public class IndividualAssignment
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Employee Name")]
        public int ADUsersId { get; set; }
        public ADUsers? ADUsers { get; set; }
        //public List<SerialNumberGroup>? SerialNumberGroups { get; set; }
        public int SerialNumberId { get; set; }
        public SerialNumber? SerialNumber { get; set; }
    }
}
