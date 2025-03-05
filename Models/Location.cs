using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Scribe.Models
{
    public class Location
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Location")]
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}


