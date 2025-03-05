using Scribe.Models;

namespace Scribe.Infrastructure
{
    public class RoleDetailsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<SystemUser>? UsersNotInRole { get; set; }
        public List<SystemUser>? UsersInRole { get; set; }
    }

    public class RoleViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
