using System.ComponentModel;

namespace Scribe.Models
{
    public class Condition
    {
        public int Id { get; set; }
        [DisplayName("Condition")]
        public string Name { get; set; }
        [DisplayName("Color Code")]
        public string ColorCode { get; set; }
    }
}
