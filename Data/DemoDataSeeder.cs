using Microsoft.EntityFrameworkCore;
using Scribe.Models;

namespace Scribe.Data;

public static class DemoDataSeeder
{
    private const string LaptopIcon = "ca2de226-17a3-4c19-9d66-4ffd5710932a_laptop.svg";
    private const string PrinterIcon = "9a819f22-86c4-4689-9615-f8b89758ad25_printer-svgrepo-com (1).svg";

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var brands = new Dictionary<string, Brand>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in new[]
        {
            (Name: "Dell", Image: "dell.svg"),
            (Name: "HP", Image: "hp.svg"),
            (Name: "Lenovo", Image: "lenovo.svg"),
            (Name: "Apple", Image: "apple.svg"),
            (Name: "Cisco", Image: "cisco.svg"),
            (Name: "Epson", Image: "epson.svg"),
            (Name: "Samsung", Image: "samsung.svg"),
            (Name: "ASUS", Image: "asus.svg"),
            (Name: "Acer", Image: "acer.svg"),
            (Name: "Intel", Image: "intel.svg")
        })
        {
            var brand = await db.Brands!.FirstOrDefaultAsync(x => x.Name == seed.Name);
            if (brand == null)
            {
                brand = new Brand { Name = seed.Name, ImageName = seed.Image };
                db.Brands.Add(brand);
            }
            else if (string.IsNullOrWhiteSpace(brand.ImageName))
            {
                brand.ImageName = seed.Image;
            }

            brands[seed.Name] = brand;
        }

        var categories = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in new[]
        {
            (Name: "Laptops", Icon: LaptopIcon),
            (Name: "Printers", Icon: PrinterIcon),
            (Name: "Monitors", Icon: LaptopIcon),
            (Name: "Smartphones", Icon: LaptopIcon),
            (Name: "Networking", Icon: LaptopIcon),
            (Name: "Accessories", Icon: PrinterIcon)
        })
        {
            var category = await db.Categories!.FirstOrDefaultAsync(x => x.Name == seed.Name);
            if (category == null)
            {
                category = new Category { Name = seed.Name, Icon = seed.Icon };
                db.Categories.Add(category);
            }
            else if (string.IsNullOrWhiteSpace(category.Icon))
            {
                category.Icon = seed.Icon;
            }

            categories[seed.Name] = category;
        }

        var locations = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in new[]
        {
            (Name: "New Storage", Description: "IT Storage room"),
            (Name: "Old Storage", Description: "IT Storage room"),
            (Name: "User Station", Description: "User Office or Station"),
            (Name: "Head Office", Description: "Main office floor"),
            (Name: "Remote Office", Description: "Remote office location"),
            (Name: "Server Room", Description: "Restricted infrastructure room")
        })
        {
            var location = await db.Locations!.FirstOrDefaultAsync(x => x.Name == seed.Name);
            if (location == null)
            {
                location = new Location { Name = seed.Name, Description = seed.Description };
                db.Locations.Add(location);
            }

            locations[seed.Name] = location;
        }

        var departments = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);
        foreach (var seedName in new[] { "IT", "No Department", "Finance", "HR", "Operations", "Sales" })
        {
            var department = await db.Department!.FirstOrDefaultAsync(x => x.Name == seedName);
            if (department == null)
            {
                department = new Department { Name = seedName };
                db.Department.Add(department);
            }

            departments[seedName] = department;
        }

        var conditions = await db.Condition!.ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var users = new Dictionary<string, ADUsers>(StringComparer.OrdinalIgnoreCase);
        foreach (var userName in new[]
        {
            "No User",
            "Alice Johnson",
            "Brian Moyo",
            "Carol Smith",
            "David Ncube",
            "Evelyn Dube"
        })
        {
            var user = await db.ADUsers.FirstOrDefaultAsync(x => x.Name == userName);
            if (user == null)
            {
                user = new ADUsers { Name = userName };
                db.ADUsers.Add(user);
            }

            users[userName] = user;
        }

        var groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupName in new[] { "IT Support", "Finance Team", "Operations Team" })
        {
            var group = await db.Group.FirstOrDefaultAsync(x => x.Name == groupName);
            if (group == null)
            {
                group = new Group { Name = groupName };
                db.Group.Add(group);
            }

            groups[groupName] = group;
        }

        var demoSystemUser = await db.SystemUsers.FirstOrDefaultAsync(x => x.SamAccountName == "demo");
        if (demoSystemUser == null)
        {
            db.SystemUsers.Add(new SystemUser
            {
                FirstName = "Demo",
                LastName = "Administrator",
                SamAccountName = "demo",
                UserPrincipalName = "demo@demo.local",
                DisplayName = "Demo Administrator"
            });
        }

        await db.SaveChangesAsync();

        var models = new Dictionary<string, Model>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in new[]
        {
            (Name: "Dell Latitude 5440", Brand: "Dell", Category: "Laptops", Image: "demo-laptop.jpg"),
            (Name: "HP EliteBook 840 G10", Brand: "HP", Category: "Laptops", Image: "demo-laptop.jpg"),
            (Name: "Lenovo ThinkPad T14 Gen 4", Brand: "Lenovo", Category: "Laptops", Image: "demo-laptop.jpg"),
            (Name: "Apple MacBook Air M2", Brand: "Apple", Category: "Laptops", Image: "demo-laptop.jpg"),
            (Name: "Acer Aspire 5", Brand: "Acer", Category: "Laptops", Image: "demo-laptop.jpg"),
            (Name: "Dell UltraSharp U2422H", Brand: "Dell", Category: "Monitors", Image: "demo-monitor.jpg"),
            (Name: "HP LaserJet Pro M404dn", Brand: "HP", Category: "Printers", Image: "demo-printer.jpg"),
            (Name: "Epson WorkForce Pro WF-4830", Brand: "Epson", Category: "Printers", Image: "demo-printer.jpg"),
            (Name: "Samsung Galaxy S21", Brand: "Samsung", Category: "Smartphones", Image: "demo-smartphone.jpg"),
            (Name: "Cisco RV340", Brand: "Cisco", Category: "Networking", Image: "demo-router.jpg"),
            (Name: "ASUS RT-AX58U", Brand: "ASUS", Category: "Networking", Image: "demo-router.jpg")
        })
        {
            var model = await db.Models!.FirstOrDefaultAsync(x => x.Name == seed.Name);
            if (model == null)
            {
                model = new Model
                {
                    Name = seed.Name,
                    BrandId = brands[seed.Brand].Id,
                    CategoryId = categories[seed.Category].Id,
                    Image = seed.Image
                };
                db.Models.Add(model);
            }
            else
            {
                model.BrandId ??= brands[seed.Brand].Id;
                model.CategoryId ??= categories[seed.Category].Id;
                model.Image ??= seed.Image;
            }

            models[seed.Name] = model;
        }

        await db.SaveChangesAsync();

        var assets = new Dictionary<string, SerialNumber>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in new[]
        {
            (Serial: "DEMO-DL5440-001", Model: "Dell Latitude 5440", Condition: "In Use", Department: "IT", Location: "User Station", User: "Alice Johnson", Group: (string?)null, Allocated: true, Description: "Primary laptop for the IT service desk", Mac: "02:00:00:54:40:01"),
            (Serial: "DEMO-DL5440-002", Model: "Dell Latitude 5440", Condition: "New", Department: "IT", Location: "New Storage", User: (string?)null, Group: (string?)null, Allocated: false, Description: "New replacement laptop", Mac: "02:00:00:54:40:02"),
            (Serial: "DEMO-HP840-001", Model: "HP EliteBook 840 G10", Condition: "In Use", Department: "Finance", Location: "Head Office", User: "Carol Smith", Group: (string?)null, Allocated: true, Description: "Finance analyst laptop", Mac: "02:00:00:84:00:01"),
            (Serial: "DEMO-T14-001", Model: "Lenovo ThinkPad T14 Gen 4", Condition: "In Use", Department: "Operations", Location: "Remote Office", User: "Brian Moyo", Group: (string?)null, Allocated: true, Description: "Operations field laptop", Mac: "02:00:00:14:00:01"),
            (Serial: "DEMO-MBA-001", Model: "Apple MacBook Air M2", Condition: "Awaiting User", Department: "Sales", Location: "New Storage", User: (string?)null, Group: (string?)null, Allocated: false, Description: "Sales onboarding laptop", Mac: "02:00:00:BA:00:01"),
            (Serial: "DEMO-ACER-001", Model: "Acer Aspire 5", Condition: "Needs Repairs", Department: "HR", Location: "Old Storage", User: (string?)null, Group: (string?)null, Allocated: false, Description: "Laptop awaiting keyboard replacement", Mac: "02:00:00:AC:00:01"),
            (Serial: "DEMO-DMON-001", Model: "Dell UltraSharp U2422H", Condition: "In Use", Department: "IT", Location: "User Station", User: "David Ncube", Group: (string?)null, Allocated: true, Description: "External monitor", Mac: (string?)null),
            (Serial: "DEMO-HPPRN-001", Model: "HP LaserJet Pro M404dn", Condition: "Needs Repairs", Department: "Finance", Location: "Head Office", User: (string?)null, Group: "Finance Team", Allocated: true, Description: "Shared finance printer", Mac: (string?)null),
            (Serial: "DEMO-EPPRN-001", Model: "Epson WorkForce Pro WF-4830", Condition: "New", Department: "Operations", Location: "New Storage", User: (string?)null, Group: (string?)null, Allocated: false, Description: "Spare multifunction printer", Mac: (string?)null),
            (Serial: "DEMO-SAMS21-001", Model: "Samsung Galaxy S21", Condition: "In Use", Department: "Sales", Location: "User Station", User: "Evelyn Dube", Group: (string?)null, Allocated: true, Description: "Sales mobile device", Mac: (string?)null),
            (Serial: "DEMO-CISCO-001", Model: "Cisco RV340", Condition: "In Use", Department: "IT", Location: "Server Room", User: (string?)null, Group: "IT Support", Allocated: true, Description: "Branch network router", Mac: (string?)null),
            (Serial: "DEMO-ASUS-001", Model: "ASUS RT-AX58U", Condition: "Out Of Order", Department: "IT", Location: "Old Storage", User: (string?)null, Group: (string?)null, Allocated: false, Description: "Router with intermittent power failure", Mac: (string?)null)
        })
        {
            var asset = await db.SerialNumbers!.FirstOrDefaultAsync(x => x.Name == seed.Serial);
            if (asset == null)
            {
                asset = new SerialNumber
                {
                    Name = seed.Serial,
                    ModelId = models[seed.Model].Id,
                    ConditionId = conditions[seed.Condition].Id,
                    DepartmentId = departments[seed.Department].Id,
                    LocationId = locations[seed.Location].Id,
                    ADUsersId = seed.User == null ? null : users[seed.User].Id,
                    GroupId = seed.Group == null ? null : groups[seed.Group].Id,
                    Creation = DateTime.UtcNow.Date.AddMonths(-6),
                    Allocation = seed.Allocated ? DateTime.UtcNow.Date.AddMonths(-2) : null,
                    AllocatedBy = seed.Allocated ? "demo" : null,
                    CurrentlyAllocated = seed.Allocated,
                    Description = seed.Description,
                    MacAddress = seed.Mac
                };
                db.SerialNumbers.Add(asset);
            }

            assets[seed.Serial] = asset;
        }

        await db.SaveChangesAsync();

        foreach (var model in models.Values)
        {
            if (!await db.Warranties.AnyAsync(x => x.ModelId == model.Id))
            {
                db.Warranties.Add(new Warranty
                {
                    ModelId = model.Id,
                    PurchaseDate = DateTime.UtcNow.Date.AddYears(-1),
                    WarrantyDurationYears = 3
                });
            }
        }

        foreach (var seed in new[]
        {
            (Serial: "DEMO-ACER-001", Description: "Keyboard replacement and inspection", Condition: "Needs Repairs", MonthsAgo: 1, NextMonths: 1),
            (Serial: "DEMO-HPPRN-001", Description: "Toner and paper-feed inspection", Condition: "Needs Repairs", MonthsAgo: 2, NextMonths: 1),
            (Serial: "DEMO-ASUS-001", Description: "Power adapter diagnostics", Condition: "Out Of Order", MonthsAgo: 3, NextMonths: 0)
        })
        {
            if (!await db.Maintenances.AnyAsync(x => x.SerialNumberId == assets[seed.Serial].Id && x.ServiceDescription == seed.Description))
            {
                db.Maintenances.Add(new Maintenance
                {
                    SerialNumberId = assets[seed.Serial].Id,
                    ServiceDescription = seed.Description,
                    ServiceDate = DateTime.UtcNow.Date.AddMonths(-seed.MonthsAgo),
                    NextServiceDate = DateTime.UtcNow.Date.AddMonths(seed.NextMonths),
                    ConditionId = conditions[seed.Condition].Id,
                    SystemUserId = "demo"
                });
            }
        }

        foreach (var seed in new[]
        {
            (Serial: "DEMO-DL5440-001", User: "Alice Johnson", Group: (string?)null),
            (Serial: "DEMO-HP840-001", User: "Carol Smith", Group: (string?)null),
            (Serial: "DEMO-T14-001", User: "Brian Moyo", Group: (string?)null),
            (Serial: "DEMO-DMON-001", User: "David Ncube", Group: (string?)null),
            (Serial: "DEMO-SAMS21-001", User: "Evelyn Dube", Group: (string?)null),
            (Serial: "DEMO-HPPRN-001", User: (string?)null, Group: "Finance Team"),
            (Serial: "DEMO-CISCO-001", User: (string?)null, Group: "IT Support")
        })
        {
            var asset = assets[seed.Serial];
            var userId = seed.User == null ? (int?)null : users[seed.User].Id;
            var groupId = seed.Group == null ? (int?)null : groups[seed.Group].Id;

            if (!await db.AllocationHistory.AnyAsync(x => x.SerialNumberId == asset.Id && x.ADUsersId == userId && x.GroupId == groupId))
            {
                db.AllocationHistory.Add(new AllocationHistory
                {
                    SerialNumberId = asset.Id,
                    ADUsersId = userId,
                    GroupId = groupId,
                    AllocationDate = DateTime.UtcNow.Date.AddMonths(-2),
                    AllocatedBy = "demo"
                });
            }

            if (userId.HasValue && !await db.IndividualAssignment.AnyAsync(x => x.SerialNumberId == asset.Id && x.ADUsersId == userId.Value))
            {
                db.IndividualAssignment.Add(new IndividualAssignment
                {
                    SerialNumberId = asset.Id,
                    ADUsersId = userId.Value
                });
            }

            if (groupId.HasValue && !await db.SerialNumberGroup.AnyAsync(x => x.SerialNumberId == asset.Id && x.GroupId == groupId.Value))
            {
                db.SerialNumberGroup.Add(new SerialNumberGroup
                {
                    SerialNumberId = asset.Id,
                    GroupId = groupId
                });
            }
        }

        foreach (var seed in new[]
        {
            (User: "Alice Johnson", Group: "IT Support"),
            (User: "David Ncube", Group: "IT Support"),
            (User: "Carol Smith", Group: "Finance Team"),
            (User: "Brian Moyo", Group: "Operations Team")
        })
        {
            if (!await db.UserGroup.AnyAsync(x => x.UserId == users[seed.User].Id && x.GroupId == groups[seed.Group].Id))
            {
                db.UserGroup.Add(new UserGroup
                {
                    UserId = users[seed.User].Id,
                    GroupId = groups[seed.Group].Id
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
