using Scribe.Infrastructure;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scribe.Models
{
    public class Model
    { 
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [DisplayName("Brand")]
        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand? Brand { get; set; }

        [DisplayName("Category")]
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        //File Properties
        public string? Image { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
        [NotMapped]
        public string? AllocationSummary { get; set; }
        public virtual ICollection<SerialNumber> SerialNumbers { get; set; }


    }
}
