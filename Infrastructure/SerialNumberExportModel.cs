using System.ComponentModel.DataAnnotations;
namespace Scribe.Infrastructure
{
    public class SerialNumberExportModel
    {
        
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? User { get; set; }
        public string? Condition { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Creation { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Allocation { get; set; }
    }
}
