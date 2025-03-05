using System.Collections.Generic;
using Scribe.Models;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using Scribe.Infrastructure;
using Scribe.Data;

public interface IActiveDirectoryService
{
    SelectList GetGroupMembersSelectList(string groupName);
    //List<SystemUser> GetUsersInGroup();
    void StoreUsersInGroup();
}
public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly string _domain;
    private readonly string _container;
    private readonly ApplicationDbContext _context;

    public ActiveDirectoryService(string domain, string container, ApplicationDbContext context)
    {
        _domain = domain;
        _container = container;
        _context = context;
    }


    public SelectList GetGroupMembersSelectList(string groupName)
    {
        //StoreUsersInGroup();
        var users = _context.SystemUsers
            .Select(u => new SelectListItem
            {
                Value = u.SamAccountName,
                Text = $"{u.SamAccountName}"
            })
            .ToList();

        return new SelectList(users, "Value", "Text");
    }

    //public List<SystemUser> GetUsersInGroup()
    //{
    //    string groupName = "zim-web-it";
    //    string domain = "zlt.co.zw";
    //    var users = new List<SystemUser>();

    //    using (var context = new PrincipalContext(ContextType.Domain, domain))
    //    using (var group = GroupPrincipal.FindByIdentity(context, groupName))
    //    {
    //        if (group != null)
    //        {
    //            foreach (var principal in group.GetMembers())
    //            {
    //                DirectoryEntry de = principal.GetUnderlyingObject() as DirectoryEntry;
    //                if (de != null)
    //                {
    //                    users.Add(new SystemUser
    //                    {
    //                        FirstName = de.Properties["givenName"].Value?.ToString(),
    //                        LastName = de.Properties["sn"].Value?.ToString(),
    //                        SamAccountName = de.Properties["samAccountName"].Value?.ToString(),
    //                        UserPrincipalName = de.Properties["userPrincipalName"].Value?.ToString()
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    return users;
    //}

    //public List<SystemUser> GetUsersInGroup()
    //{
    //    string groupName = "zim-web-it";
    //    string domain = "zlt.co.zw";
    //    var users = new List<SystemUser>();

    //    using (var context = new PrincipalContext(ContextType.Domain, domain))
    //    using (var group = GroupPrincipal.FindByIdentity(context, groupName))
    //    {
    //        if (group != null)
    //        {
    //            foreach (var principal in group.GetMembers())
    //            {
    //                DirectoryEntry de = principal.GetUnderlyingObject() as DirectoryEntry;
    //                if (de != null)
    //                {
    //                    string samAccountName = de.Properties["samAccountName"].Value?.ToString();

    //                    if (!UserExistsInDatabase(samAccountName))
    //                    {
    //                        users.Add(new SystemUser
    //                        {
    //                            FirstName = de.Properties["givenName"].Value?.ToString(),
    //                            LastName = de.Properties["sn"].Value?.ToString(),
    //                            SamAccountName = samAccountName,
    //                            UserPrincipalName = de.Properties["userPrincipalName"].Value?.ToString()
    //                        });
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    return users;
    //}

    ////Methos checks the existence of a user in SystemUsers Table
    private bool UserExistsInDatabase(string samAccountName)
    {
        
            return _context.SystemUsers.Any(u => u.SamAccountName == samAccountName);
        
    }

    public void StoreUsersInGroup()
    {
        string groupName = "zim-web-it";
        string domain = "zlt.co.zw";
        using (var context = new PrincipalContext(ContextType.Domain, domain))
        using (var group = GroupPrincipal.FindByIdentity(context, groupName))
        {
            if (group != null)
            {
                foreach (var principal in group.GetMembers())
                {
                    DirectoryEntry de = principal.GetUnderlyingObject() as DirectoryEntry;
                    if (de != null)
                    {
                        string samAccountName = de.Properties["samAccountName"].Value?.ToString();

                        if (!UserExistsInDatabase(samAccountName))
                        {
                            var systemUser = new SystemUser
                            {
                                FirstName = de.Properties["givenName"].Value?.ToString(),
                                LastName = de.Properties["sn"].Value?.ToString(),
                                SamAccountName = de.Properties["samAccountName"].Value?.ToString(),
                                UserPrincipalName = de.Properties["userPrincipalName"].Value?.ToString(),
                                DisplayName = de.Properties["DisplayName"].Value?.ToString()
                            };

                            if (systemUser != null)
                            {
                                _context.SystemUsers.Add(systemUser);
                            }
                        }
                    }
                }

                _context.SaveChanges();
            }
        }
    }

}