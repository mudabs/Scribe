namespace Scribe.Models
{
    public class DashboardViewModel
    {
        public int TotalDevices { get; set; }
        public int DeadDevices { get; set; }
        public int NewDevices { get; set; }
        public int InUseDevices { get; set; }

        public List<string> Labels { get; set; } = new();
        public List<int> MonthlyNewDevices { get; set; } = new();
        public List<int> MonthlyAllocations { get; set; } = new();
        public List<int> MonthlyDeadDevices { get; set; } = new();
        public Dictionary<string, int> NewDevicesMonthly { get; set; } = new();
        public Dictionary<string, int> AllocationsMonthly { get; set; } = new();
        public Dictionary<string, int> DeviceExpiryMonthly { get; set; } = new();
        public Dictionary<string, int> MostServicedModels { get; set; } = new();
        public Dictionary<string, double> ModelLifetimes { get; set; } = new();
        public Dictionary<string, double> ModelLifetimeForecasts { get; set; } = new();

    }
}
