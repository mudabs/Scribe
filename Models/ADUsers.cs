using Microsoft.AspNetCore.Mvc.Rendering;

namespace Scribe.Models
{
    public class ADUsers
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<SerialNumber>? SerialNumbers { get; set; }
    }
}
