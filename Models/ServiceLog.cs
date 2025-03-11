using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace Scribe.Models
{
    public class ServiceLog
    {
        public int Id { get; set; }
        [DisplayName("SerialNumber")]
        public int SerialNumberId { get; set; }
        [ForeignKey("SerialNumberId")]
        public SerialNumber? SerialNumber { get; set; }
        public string Description { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
    }
}
