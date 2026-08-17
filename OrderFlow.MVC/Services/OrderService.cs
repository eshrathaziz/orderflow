using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.ViewModels;

namespace OrderFlow.MVC.Services
{
    public class OrderService
    {
        private readonly OrderFlowDbContext _db;
        private readonly InventoryService _inventory;
        public OrderService(OrderFlowDbContext db) { _db = db; _inventory = new InventoryService(db); }
        public async Task<Order> CreateAsync(OrderCreateViewModel input, int userId)
        {
            if (!input.Items.Any()) throw new InvalidOperationException("Add at least one product to the order.");
            var customer = await _db.Customers.FindAsync(input.CustomerId);
            if (customer == null || customer.Status != CustomerStatus.Active) throw new InvalidOperationException("Select an active customer.");
            var productIds = input.Items.Select(i => i.ProductId).Distinct().ToList();
            if (productIds.Count != input.Items.Count) throw new InvalidOperationException("A product can appear only once per order.");
            var products = await _db.Products.Include(p => p.Inventory).Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            if (products.Count != productIds.Count || products.Any(p => p.Status != ProductStatus.Active)) throw new InvalidOperationException("One or more products are not available for sale.");
            foreach (var item in input.Items) { var product = products.Single(p => p.ProductId == item.ProductId); if (product.Inventory == null || product.Inventory.AvailableQuantity < item.Quantity) throw new InvalidOperationException(product.Name + " does not have sufficient available inventory."); }
            var order = new Order { OrderNumber = "OF-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant(), CustomerId = input.CustomerId, SalesExecutiveUserId = userId, TaxRate = input.TaxRate, ShippingAddress = input.ShippingAddress, Notes = input.Notes };
            foreach (var inputItem in input.Items) { var product = products.Single(p => p.ProductId == inputItem.ProductId); var lineSubtotal = product.UnitPrice * inputItem.Quantity; var lineTax = decimal.Round(lineSubtotal * input.TaxRate, 2, MidpointRounding.AwayFromZero); order.Items.Add(new OrderItem { ProductId = product.ProductId, Quantity = inputItem.Quantity, UnitPrice = product.UnitPrice, LineSubtotal = lineSubtotal, TaxAmount = lineTax, LineTotal = lineSubtotal + lineTax }); }
            order.Subtotal = order.Items.Sum(i => i.LineSubtotal); order.TaxAmount = order.Items.Sum(i => i.TaxAmount); order.TotalAmount = order.Items.Sum(i => i.LineTotal); _db.Orders.Add(order); await _db.SaveChangesAsync();
            foreach (var item in order.Items) await _inventory.ReserveAsync(item.ProductId, item.Quantity, userId, order.OrderId);
            _db.AuditEvents.Add(new AuditEvent { UserId = userId, EntityType = "Order", EntityId = order.OrderId.ToString(), Action = AuditAction.Created, Detail = "Order created and stock reserved." }); await _db.SaveChangesAsync(); return order;
        }
        public async Task<OrderCalculationViewModel> CalculateAsync(IList<OrderItemInputViewModel> items, decimal taxRate)
        {
            var result = new OrderCalculationViewModel(); var ids = items.Select(i => i.ProductId).Distinct().ToList(); var products = await _db.Products.Include(p => p.Inventory).Where(p => ids.Contains(p.ProductId)).ToListAsync();
            foreach (var item in items) { var p = products.SingleOrDefault(x => x.ProductId == item.ProductId); if (p == null) { result.Errors.Add("Unknown product."); continue; } if (p.Inventory == null || p.Inventory.AvailableQuantity < item.Quantity) result.Errors.Add(p.Name + " has insufficient available stock."); result.Subtotal += p.UnitPrice * item.Quantity; }
            result.TaxAmount = decimal.Round(result.Subtotal * taxRate, 2, MidpointRounding.AwayFromZero); result.Total = result.Subtotal + result.TaxAmount; return result;
        }
        public async Task UpdateStatusAsync(int orderId, OrderStatus nextStatus, int userId)
        {
            var order = await _db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.OrderId == orderId); if (order == null) throw new InvalidOperationException("Order was not found."); if (!IsValidTransition(order.Status, nextStatus)) throw new InvalidOperationException("The requested workflow transition is not allowed.");
            var previous = order.Status; order.Status = nextStatus; if (nextStatus == OrderStatus.Confirmed) order.ConfirmedAtUtc = DateTime.UtcNow; if (nextStatus == OrderStatus.Shipped) { foreach (var item in order.Items) await _inventory.CommitStockOutAsync(item.ProductId, item.Quantity, userId, order.OrderId); order.ShippedAtUtc = DateTime.UtcNow; } if (nextStatus == OrderStatus.Completed) order.CompletedAtUtc = DateTime.UtcNow;
            _db.AuditEvents.Add(new AuditEvent { UserId = userId, EntityType = "Order", EntityId = order.OrderId.ToString(), Action = AuditAction.StatusChanged, Detail = previous + " to " + nextStatus }); await _db.SaveChangesAsync();
        }
        private static bool IsValidTransition(OrderStatus current, OrderStatus next) { return (current == OrderStatus.Created && next == OrderStatus.Confirmed) || (current == OrderStatus.Confirmed && next == OrderStatus.Processing) || (current == OrderStatus.Processing && next == OrderStatus.Shipped) || (current == OrderStatus.Shipped && next == OrderStatus.Delivered) || (current == OrderStatus.Delivered && next == OrderStatus.Completed); }
    }
}
