using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Filters;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.Security;
using OrderFlow.MVC.ViewModels;

namespace OrderFlow.MVC.Controllers
{
    [Authorize]
    public class CustomerRequestsController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index(RequestStatus? status = null, RequestPriority? priority = null) { var principal = User as CustomPrincipal; var query = _db.CustomerRequests.Include(r => r.Customer).Include(r => r.Order).Include(r => r.AssignedEmployee).AsQueryable(); if (principal != null && principal.IsInRole("Customer")) query = query.Where(r => r.CustomerId == principal.CustomerId); if (status.HasValue) query = query.Where(r => r.Status == status); if (priority.HasValue) query = query.Where(r => r.Priority == priority); return View(await query.OrderByDescending(r => r.Priority).ThenByDescending(r => r.SubmittedAtUtc).ToListAsync()); }
        public async Task<ActionResult> Details(int id) { var principal = User as CustomPrincipal; var request = await _db.CustomerRequests.Include(r => r.Customer).Include(r => r.Order).Include(r => r.AssignedEmployee).Include(r => r.History.Select(h => h.Author)).SingleOrDefaultAsync(r => r.CustomerRequestId == id); if (request == null || (principal != null && principal.IsInRole("Customer") && request.CustomerId != principal.CustomerId)) return HttpNotFound(); return View(request); }
        public ActionResult Create(int? orderId) { var principal = User as CustomPrincipal; return View(new CustomerRequestCreateViewModel { CustomerId = principal?.CustomerId ?? 0, OrderId = orderId, Priority = RequestPriority.Medium }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<ActionResult> Create(CustomerRequestCreateViewModel model) { var principal = User as CustomPrincipal; if (principal != null && principal.IsInRole("Customer")) model.CustomerId = principal.CustomerId ?? 0; if (!ModelState.IsValid || model.CustomerId == 0) return View(model); var entity = new CustomerRequest { RequestNumber = "CR-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant(), CustomerId = model.CustomerId, OrderId = model.OrderId, Type = model.Type, Priority = model.Priority, Description = model.Description, Status = RequestStatus.Open }; _db.CustomerRequests.Add(entity); _db.RequestHistory.Add(new RequestHistory { CustomerRequest = entity, AuthorUserId = principal?.UserId, NewStatus = RequestStatus.Open, Message = "Request submitted.", IsVisibleToCustomer = true }); await _db.SaveChangesAsync(); TempData["Success"] = "Request " + entity.RequestNumber + " submitted."; return RedirectToAction("Details", new { id = entity.CustomerRequestId }); }
        [HttpPost, ValidateAntiForgeryToken, AuthorizeRole("Admin", "SalesExecutive", "InventoryManager")] public async Task<ActionResult> Update(CustomerRequestUpdateViewModel model) { if (!ModelState.IsValid) return new HttpStatusCodeResult(400, "Invalid request update."); var entity = await _db.CustomerRequests.FindAsync(model.CustomerRequestId); if (entity == null) return HttpNotFound(); var previous = entity.Status; entity.Status = model.Status; entity.AssignedEmployeeUserId = model.AssignedEmployeeUserId; entity.Resolution = model.Resolution; if (model.Status == RequestStatus.Resolved || model.Status == RequestStatus.Closed) entity.ResolvedAtUtc = DateTime.UtcNow; _db.RequestHistory.Add(new RequestHistory { CustomerRequestId = entity.CustomerRequestId, AuthorUserId = ((CustomPrincipal)User).UserId, PreviousStatus = previous, NewStatus = model.Status, Message = model.Message, IsVisibleToCustomer = model.IsVisibleToCustomer }); await _db.SaveChangesAsync(); return Json(new { ok = true, message = "Request updated." }); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
