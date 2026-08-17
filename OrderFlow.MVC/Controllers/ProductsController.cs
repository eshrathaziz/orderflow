using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Filters;
using OrderFlow.MVC.Models;

namespace OrderFlow.MVC.Controllers
{
    [AuthorizeRole("Admin", "InventoryManager")]
    public class ProductsController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index(string search = "", int? categoryId = null, ProductStatus? status = null, int page = 1)
        {
            const int pageSize = 20; var query = _db.Products.Include(p => p.Category).Include(p => p.Inventory).AsQueryable();
            if (!String.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Sku.Contains(search) || p.Name.Contains(search)); if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId); if (status.HasValue) query = query.Where(p => p.Status == status);
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(); ViewBag.Total = await query.CountAsync(); ViewBag.Page = Math.Max(1, page); ViewBag.PageSize = pageSize; return View(await query.OrderBy(p => p.Name).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync());
        }
        public async Task<ActionResult> Details(int id) { var product = await _db.Products.Include(p => p.Category).Include(p => p.Inventory).SingleOrDefaultAsync(p => p.ProductId == id); return product == null ? (ActionResult)HttpNotFound() : View(product); }
        public async Task<ActionResult> Create() { ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync(); return View(new Product { Status = ProductStatus.Active }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Create([Bind(Include = "CategoryId,Sku,Name,Description,UnitPrice,ReorderLevel,Status")] Product product, int initialQuantity = 0) { if (!ModelState.IsValid) { ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync(); return View(product); } _db.Products.Add(product); await _db.SaveChangesAsync(); _db.Inventory.Add(new Inventory { ProductId = product.ProductId, QuantityOnHand = Math.Max(0, initialQuantity), ReservedQuantity = 0 }); await _db.SaveChangesAsync(); TempData["Success"] = "Product and inventory record created."; return RedirectToAction("Details", new { id = product.ProductId }); }
        public async Task<ActionResult> Edit(int id) { var product = await _db.Products.FindAsync(id); if (product == null) return HttpNotFound(); ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync(); return View(product); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Edit([Bind(Include = "ProductId,CategoryId,Sku,Name,Description,UnitPrice,ReorderLevel,Status")] Product input) { if (!ModelState.IsValid) { ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync(); return View(input); } var entity = await _db.Products.FindAsync(input.ProductId); if (entity == null) return HttpNotFound(); _db.Entry(entity).CurrentValues.SetValues(input); entity.UpdatedAtUtc = DateTime.UtcNow; await _db.SaveChangesAsync(); TempData["Success"] = "Product updated."; return RedirectToAction("Details", new { id = entity.ProductId }); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
