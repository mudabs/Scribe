using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Scribe.Models
{
    public class Warranty
    {
        public int Id { get; set; }

        [Required]
        public int ModelId { get; set; }

        [ForeignKey("ModelId")]
        public Model? Model { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, 10)]
        public int WarrantyDurationYears { get; set; } // e.g. 1, 2, 5
    }
}
