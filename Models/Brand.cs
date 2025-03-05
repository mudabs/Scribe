using Scribe.Infrastructure;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scribe.Models
{
    public class Brand
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Brand Name")]
        public string? Name { get; set; }
        [DisplayName("Logo")]
        public string? ImageName { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
