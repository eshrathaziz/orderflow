using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.ViewModels;

namespace OrderFlow.MVC.Services
{
    public class DashboardService
    {
        private readonly OrderFlowDbContext _db;
        public DashboardService(OrderFlowDbContext db) { _db = db; }
        public async Task<DashboardViewModel> BuildAsync()
        {
            var now = DateTime.UtcNow; var monthStart = new DateTime(now.Year, now.Month, 1); var previousStart = monthStart.AddMonths(-1); var recent = await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.CreatedAtUtc).Take(7).ToListAsync(); var low = await _db.Inventory.Include(i => i.Product).Where(i => i.AvailableQuantity <= i.Product.ReorderLevel).OrderBy(i => i.AvailableQuantity).Take(5).ToListAsync(); var delivered = _db.Orders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed);
            return new DashboardViewModel { TotalCustomers = await _db.Customers.CountAsync(c => c.Status == CustomerStatus.Active), TotalProducts = await _db.Products.CountAsync(p => p.Status == ProductStatus.Active), TotalOrders = await _db.Orders.CountAsync(), PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Created || o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing), CompletedOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Completed), LowStockProducts = await _db.Inventory.CountAsync(i => i.AvailableQuantity <= i.Product.ReorderLevel), MonthRevenue = await delivered.Where(o => o.CompletedAtUtc >= monthStart).Select(o => (decimal?)o.TotalAmount).SumAsync() ?? 0m, PreviousMonthRevenue = await delivered.Where(o => o.CompletedAtUtc >= previousStart && o.CompletedAtUtc < monthStart).Select(o => (decimal?)o.TotalAmount).SumAsync() ?? 0m, RecentOrders = recent.Select(o => new RecentOrderViewModel { OrderId = o.OrderId, OrderNumber = o.OrderNumber, CustomerName = o.Customer.CompanyName, TotalAmount = o.TotalAmount, Status = o.Status, CreatedAtUtc = o.CreatedAtUtc }).ToList(), LowStockItems = low.Select(i => new LowStockViewModel { ProductId = i.ProductId, Sku = i.Product.Sku, ProductName = i.Product.Name, AvailableQuantity = i.AvailableQuantity, ReorderLevel = i.Product.ReorderLevel }).ToList(), RevenueTrend = Enumerable.Range(0, 6).Select(offset => { var start = monthStart.AddMonths(offset - 5); var end = start.AddMonths(1); return new RevenuePointViewModel { Label = start.ToString("MMM"), Revenue = _db.Orders.Where(o => (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed) && o.CompletedAtUtc >= start && o.CompletedAtUtc < end).Select(o => (decimal?)o.TotalAmount).Sum() ?? 0m }; }).ToList() };
        }
    }
}
