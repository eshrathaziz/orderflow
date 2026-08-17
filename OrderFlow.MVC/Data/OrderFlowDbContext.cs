using System.Data.Entity;
using OrderFlow.MVC.Models;
namespace OrderFlow.MVC.Data
{
    public class OrderFlowDbContext : DbContext
    {
        public OrderFlowDbContext() : base("OrderFlowConnection") { }
        public DbSet<Role> Roles { get; set; } public DbSet<ApplicationUser> Users { get; set; } public DbSet<Customer> Customers { get; set; } public DbSet<Category> Categories { get; set; } public DbSet<Product> Products { get; set; } public DbSet<Inventory> Inventory { get; set; } public DbSet<InventoryTransaction> InventoryTransactions { get; set; } public DbSet<Order> Orders { get; set; } public DbSet<OrderItem> OrderItems { get; set; } public DbSet<CustomerRequest> CustomerRequests { get; set; } public DbSet<RequestHistory> RequestHistory { get; set; } public DbSet<AuditEvent> AuditEvents { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
            modelBuilder.Entity<ApplicationUser>().HasOptional(u => u.Customer).WithMany().HasForeignKey(u => u.CustomerId).WillCascadeOnDelete(false);
            modelBuilder.Entity<InventoryTransaction>().HasOptional(t => t.Order).WithMany().HasForeignKey(t => t.OrderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<InventoryTransaction>().HasOptional(t => t.PerformedBy).WithMany().HasForeignKey(t => t.PerformedByUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Order>().HasRequired(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Order>().HasOptional(o => o.SalesExecutive).WithMany().HasForeignKey(o => o.SalesExecutiveUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CustomerRequest>().HasRequired(r => r.Customer).WithMany(c => c.Requests).HasForeignKey(r => r.CustomerId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CustomerRequest>().HasOptional(r => r.Order).WithMany(o => o.CustomerRequests).HasForeignKey(r => r.OrderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CustomerRequest>().HasOptional(r => r.AssignedEmployee).WithMany().HasForeignKey(r => r.AssignedEmployeeUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<RequestHistory>().HasOptional(h => h.Author).WithMany().HasForeignKey(h => h.AuthorUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<AuditEvent>().HasOptional(a => a.User).WithMany().HasForeignKey(a => a.UserId).WillCascadeOnDelete(false);
            base.OnModelCreating(modelBuilder);
        }
    }
}
