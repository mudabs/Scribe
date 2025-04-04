using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scribe.Models
{
    public class Group
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public List<UserGroup>? UserGroups { get; set; }
        public List<SerialNumberGroup>? SerialNumberGroups { get; set; }

        public static implicit operator Group(Task<Group?> v)
        {
            throw new NotImplementedException();
        }
    }
}
