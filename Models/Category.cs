using Scribe.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scribe.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Icon { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? IconUpload { get; set; }

    }
}
