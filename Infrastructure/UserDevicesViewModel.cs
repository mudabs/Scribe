using Scribe.Models;

namespace Scribe.Infrastructure
{
    public class UserDevicesViewModel
    {
        public ADUsers User { get; set; }
        public List<SerialNumber> Devices { get; set; } = new List<SerialNumber>(); // Initialize to avoid null reference
    }
}
