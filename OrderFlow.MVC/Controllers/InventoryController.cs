using System;
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
    [AuthorizeRole("Admin", "InventoryManager")]
    public class InventoryController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index(string search = "", bool lowStock = false, int page = 1) { const int pageSize = 20; var query = _db.Inventory.Include(i => i.Product).Include(i => i.Product.Category).AsQueryable(); if (!String.IsNullOrWhiteSpace(search)) query = query.Where(i => i.Product.Sku.Contains(search) || i.Product.Name.Contains(search)); if (lowStock) query = query.Where(i => i.AvailableQuantity <= i.Product.ReorderLevel); ViewBag.Total = await query.CountAsync(); ViewBag.Page = Math.Max(1, page); ViewBag.PageSize = pageSize; ViewBag.LowStock = lowStock; return View(await query.OrderBy(i => i.AvailableQuantity).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync()); }
        [HttpGet] public async Task<JsonResult> Availability(int productId) { var result = await new InventoryService(_db).GetAvailabilityAsync(productId); return Json(result, JsonRequestBehavior.AllowGet); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Adjust(InventoryAdjustmentViewModel model) { if (!ModelState.IsValid) return new HttpStatusCodeResult(400, "Please correct the inventory adjustment."); try { await new InventoryService(_db).AdjustAsync(model.ProductId, model.QuantityDelta, model.Type, model.Notes, ((CustomPrincipal)User).UserId); return Json(new { ok = true, message = "Inventory transaction posted." }); } catch (InvalidOperationException ex) { return new HttpStatusCodeResult(409, ex.Message); } }
        public async Task<ActionResult> History(int productId) { var product = await _db.Products.FindAsync(productId); if (product == null) return HttpNotFound(); ViewBag.Product = product; return View(await _db.InventoryTransactions.Include(t => t.PerformedBy).Where(t => t.ProductId == productId).OrderByDescending(t => t.CreatedAtUtc).ToListAsync()); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
