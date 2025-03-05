using System.ComponentModel;

namespace Scribe.Models
{
    public class SerialNumberGroup
    {
        public int Id { get; set; }
        [DisplayName("Serial Number")]
        public int SerialNumberId { get; set; }
        public SerialNumber? SerialNumber { get; set; }
        [DisplayName("Group")]
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        [DisplayName("Employee")]
        public int? ADUsersId { get; set; }
        public ADUsers? ADUsers { get; set; }
    }
}
