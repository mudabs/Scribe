using Microsoft.AspNetCore.Identity;
namespace Scribe.Models
{
    public class SystemUser 
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SamAccountName { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? DisplayName { get; set; }
    }

}

