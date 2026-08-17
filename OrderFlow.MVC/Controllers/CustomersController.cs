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
    [AuthorizeRole("Admin", "SalesExecutive")]
    public class CustomersController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index(string search = "", CustomerStatus? status = null, string sort = "company", int page = 1)
        {
            const int pageSize = 20; var query = _db.Customers.AsQueryable();
            if (!String.IsNullOrWhiteSpace(search)) query = query.Where(c => c.CompanyName.Contains(search) || c.ContactName.Contains(search) || c.Email.Contains(search));
            if (status.HasValue) query = query.Where(c => c.Status == status.Value);
            query = sort == "created" ? query.OrderByDescending(c => c.CreatedAtUtc) : query.OrderBy(c => c.CompanyName);
            ViewBag.Total = await query.CountAsync(); ViewBag.Page = Math.Max(1, page); ViewBag.PageSize = pageSize; ViewBag.Search = search; ViewBag.Status = status; return View(await query.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync());
        }
        public async Task<ActionResult> Details(int id) { var customer = await _db.Customers.Include(c => c.Orders).Include(c => c.Requests).SingleOrDefaultAsync(c => c.CustomerId == id); return customer == null ? (ActionResult)HttpNotFound() : View(customer); }
        public ActionResult Create() { return View(new Customer()); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Create([Bind(Include = "CompanyName,ContactName,Email,Phone,AddressLine1,City,StateOrRegion,PostalCode,Country,Status")] Customer customer) { if (!ModelState.IsValid) return View(customer); _db.Customers.Add(customer); await _db.SaveChangesAsync(); TempData["Success"] = "Customer profile created."; return RedirectToAction("Details", new { id = customer.CustomerId }); }
        public async Task<ActionResult> Edit(int id) { var customer = await _db.Customers.FindAsync(id); return customer == null ? (ActionResult)HttpNotFound() : View(customer); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Edit([Bind(Include = "CustomerId,CompanyName,ContactName,Email,Phone,AddressLine1,City,StateOrRegion,PostalCode,Country,Status")] Customer input) { if (!ModelState.IsValid) return View(input); var entity = await _db.Customers.FindAsync(input.CustomerId); if (entity == null) return HttpNotFound(); _db.Entry(entity).CurrentValues.SetValues(input); await _db.SaveChangesAsync(); TempData["Success"] = "Customer profile updated."; return RedirectToAction("Details", new { id = entity.CustomerId }); }
        [HttpPost, ValidateAntiForgeryToken, AuthorizeRole("Admin")] public async Task<ActionResult> Deactivate(int id) { var customer = await _db.Customers.FindAsync(id); if (customer == null) return HttpNotFound(); customer.Status = CustomerStatus.Inactive; await _db.SaveChangesAsync(); TempData["Success"] = "Customer deactivated."; return RedirectToAction("Index"); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
