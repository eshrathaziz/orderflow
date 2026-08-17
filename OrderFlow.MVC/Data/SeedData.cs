using System;
using System.Data.Entity;
using System.Linq;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.Security;
namespace OrderFlow.MVC.Data
{
    public class SeedData : CreateDatabaseIfNotExists<OrderFlowDbContext>
    {
        protected override void Seed(OrderFlowDbContext db)
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole))) db.Roles.Add(new Role { RoleId = (int)role, Name = role.ToString(), Description = role + " role" });
            db.Categories.AddRange(new[] { new Category { Name = "Network Equipment", Description = "Managed infrastructure components" }, new Category { Name = "Office Systems", Description = "Enterprise office equipment" }, new Category { Name = "Warehouse Supplies", Description = "Fulfilment and warehouse consumables" } }); db.SaveChanges();
            var customer = new Customer { CompanyName = "Northstar Logistics", ContactName = "Ava Martinez", Email = "ava@northstar.example", Phone = "+1 312 555 0148", City = "Chicago", StateOrRegion = "IL", Country = "United States", Status = CustomerStatus.Active }; db.Customers.Add(customer); db.SaveChanges();
            AddUser(db, "admin@orderflow.local", "System Administrator", UserRole.Admin, null); AddUser(db, "sales@orderflow.local", "Jordan Lee", UserRole.SalesExecutive, null); AddUser(db, "inventory@orderflow.local", "Morgan Chen", UserRole.InventoryManager, null); AddUser(db, "ava@northstar.example", "Ava Martinez", UserRole.Customer, customer.CustomerId);
            var network = db.Categories.Single(c => c.Name == "Network Equipment"); var office = db.Categories.Single(c => c.Name == "Office Systems"); var products = new[] { new Product { CategoryId = network.CategoryId, Sku = "NET-SW-24P", Name = "24-Port Managed Switch", UnitPrice = 1249m, ReorderLevel = 12, Status = ProductStatus.Active }, new Product { CategoryId = network.CategoryId, Sku = "NET-AP-6E", Name = "Wi-Fi 6E Access Point", UnitPrice = 399m, ReorderLevel = 20, Status = ProductStatus.Active }, new Product { CategoryId = office.CategoryId, Sku = "OFF-DK-USB", Name = "USB-C Docking Station", UnitPrice = 219m, ReorderLevel = 30, Status = ProductStatus.Active } }; db.Products.AddRange(products); db.SaveChanges(); db.Inventory.AddRange(products.Select((p, i) => new Inventory { ProductId = p.ProductId, QuantityOnHand = new[] { 8, 58, 146 }[i], ReservedQuantity = 0 })); db.SaveChanges();
        }
        private static void AddUser(OrderFlowDbContext db, string email, string name, UserRole role, int? customerId) { string salt; string hash = PasswordHasher.Hash("ChangeMe!123", out salt); db.Users.Add(new ApplicationUser { Email = email, DisplayName = name, RoleId = (int)role, CustomerId = customerId, PasswordHash = hash, PasswordSalt = salt }); }
    }
}
