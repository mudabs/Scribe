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
    bool UserExists(string samAccountName);
    int? GetSystemUserId(string samAccountName);
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

    public int? GetSystemUserId(string samAccountName)
    {
        return _context.SystemUsers.FirstOrDefault(u => u.SamAccountName == samAccountName)?.Id;
    }

    public bool UserExists(string samAccountName)
    {
        return _context.SystemUsers.Any(u => u.SamAccountName == samAccountName);
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

    ////Methos checks the existence of a user in SystemUsers Table
    private bool UserExistsInDatabase(string samAccountName)
    {
        
            return _context.SystemUsers.Any(u => u.SamAccountName == samAccountName);
        
    }

    public void StoreUsersInGroup()
    {
        string groupName = "Scribe Admins";
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