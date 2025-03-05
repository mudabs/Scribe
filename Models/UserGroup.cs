using System.ComponentModel;

namespace Scribe.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        [DisplayName("Employee")]
        public int UserId { get; set; }
        public ADUsers? User { get; set; }
        [DisplayName("Group")]
        public int GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
