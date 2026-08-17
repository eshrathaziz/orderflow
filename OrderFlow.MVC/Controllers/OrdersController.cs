using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Filters;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.Security;
using OrderFlow.MVC.Services;
using OrderFlow.MVC.ViewModels;

namespace OrderFlow.MVC.Controllers
{
    [AuthorizeRole("Admin", "SalesExecutive", "InventoryManager")]
    public class OrdersController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index(string search = "", OrderStatus? status = null, int page = 1) { const int pageSize = 20; var query = _db.Orders.Include(o => o.Customer).AsQueryable(); if (!String.IsNullOrWhiteSpace(search)) query = query.Where(o => o.OrderNumber.Contains(search) || o.Customer.CompanyName.Contains(search)); if (status.HasValue) query = query.Where(o => o.Status == status); ViewBag.Total = await query.CountAsync(); ViewBag.Page = Math.Max(1, page); ViewBag.PageSize = pageSize; return View(await query.OrderByDescending(o => o.CreatedAtUtc).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync()); }
        public async Task<ActionResult> Details(int id) { var order = await _db.Orders.Include(o => o.Customer).Include(o => o.Items.Select(i => i.Product)).Include(o => o.SalesExecutive).SingleOrDefaultAsync(o => o.OrderId == id); return order == null ? (ActionResult)HttpNotFound() : View(order); }
        [AuthorizeRole("Admin", "SalesExecutive")] public async Task<ActionResult> Create() { ViewBag.Customers = await _db.Customers.Where(c => c.Status == CustomerStatus.Active).OrderBy(c => c.CompanyName).ToListAsync(); return View(new OrderCreateViewModel { Items = new List<OrderItemInputViewModel> { new OrderItemInputViewModel() } }); }
        [HttpPost, AuthorizeRole("Admin", "SalesExecutive"), ValidateAntiForgeryToken] public async Task<ActionResult> Create(OrderCreateViewModel model) { if (!ModelState.IsValid) { ViewBag.Customers = await _db.Customers.Where(c => c.Status == CustomerStatus.Active).OrderBy(c => c.CompanyName).ToListAsync(); return View(model); } try { var order = await new OrderService(_db).CreateAsync(model, ((CustomPrincipal)User).UserId); TempData["Success"] = "Order " + order.OrderNumber + " created and inventory reserved."; return RedirectToAction("Details", new { id = order.OrderId }); } catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); ViewBag.Customers = await _db.Customers.Where(c => c.Status == CustomerStatus.Active).OrderBy(c => c.CompanyName).ToListAsync(); return View(model); } }
        [AuthorizeRole("Admin", "SalesExecutive")] public async Task<ActionResult> Edit(int id) { var order = await _db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.OrderId == id); if (order == null) return HttpNotFound(); if (order.Status != OrderStatus.Created) { TempData["Error"] = "Only orders in Created status can be edited."; return RedirectToAction("Details", new { id }); } await LoadOrderEditLists(); return View(new OrderEditViewModel { OrderId = order.OrderId, CustomerId = order.CustomerId, TaxRate = order.TaxRate, ShippingAddress = order.ShippingAddress, Notes = order.Notes, Items = order.Items.Select(i => new OrderItemInputViewModel { ProductId = i.ProductId, Quantity = i.Quantity }).ToList() }); }
        [HttpPost, AuthorizeRole("Admin", "SalesExecutive"), ValidateAntiForgeryToken] public async Task<ActionResult> Edit(OrderEditViewModel model) { if (!ModelState.IsValid) { await LoadOrderEditLists(); return View(model); } try { await new OrderService(_db).UpdateAsync(model, ((CustomPrincipal)User).UserId); TempData["Success"] = "Order updated and inventory allocations reconciled."; return RedirectToAction("Details", new { id = model.OrderId }); } catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); await LoadOrderEditLists(); return View(model); } }
        [HttpGet] public async Task<JsonResult> ProductSearch(string query) { query = query ?? ""; var products = await _db.Products.Include(p => p.Inventory).Where(p => p.Status == ProductStatus.Active && (p.Name.Contains(query) || p.Sku.Contains(query))).OrderBy(p => p.Name).Take(12).Select(p => new { p.ProductId, p.Sku, p.Name, p.UnitPrice, Available = p.Inventory.QuantityOnHand - p.Inventory.ReservedQuantity }).ToListAsync(); return Json(products, JsonRequestBehavior.AllowGet); }
        [HttpGet] public async Task<JsonResult> CheckAvailability(int productId, int quantity) { var item = await new InventoryService(_db).GetAvailabilityAsync(productId); return Json(new { ok = item != null && quantity > 0 && item.AvailableQuantity >= quantity, available = item?.AvailableQuantity ?? 0, reorderLevel = item?.ReorderLevel ?? 0 }, JsonRequestBehavior.AllowGet); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<JsonResult> Calculate(OrderCreateViewModel model) { var result = await new OrderService(_db).CalculateAsync(model.Items ?? new List<OrderItemInputViewModel>(), model.TaxRate); return Json(result); }
        [HttpPost, ValidateAntiForgeryToken, AuthorizeRole("Admin", "SalesExecutive", "InventoryManager")] public async Task<ActionResult> UpdateStatus(OrderStatusUpdateViewModel model) { if (!ModelState.IsValid) return new HttpStatusCodeResult(400, "Invalid status update."); try { await new OrderService(_db).UpdateStatusAsync(model.OrderId, model.Status, ((CustomPrincipal)User).UserId); return Json(new { ok = true, message = "Order status updated to " + model.Status + "." }); } catch (InvalidOperationException ex) { return new HttpStatusCodeResult(409, ex.Message); } }
        private async Task LoadOrderEditLists() { ViewBag.Customers = await _db.Customers.Where(c => c.Status == CustomerStatus.Active).OrderBy(c => c.CompanyName).ToListAsync(); ViewBag.Products = await _db.Products.Where(p => p.Status == ProductStatus.Active).OrderBy(p => p.Name).ToListAsync(); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
