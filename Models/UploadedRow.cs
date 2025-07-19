namespace Scribe.Models
{
    public class UploadedRow
    {
        public string SerialNumber { get; set; }
        public string AllocateToType { get; set; }  // "User" or "Group"
        public string AllocateToName { get; set; }

        public int? ResolvedUserId { get; set; }   // FK if AllocateToType == User
        public int? ResolvedGroupId { get; set; }  // FK if AllocateToType == Group

        public bool IsValid { get; set; } = true;
        public string Error { get; set; }
    }
}
