using System.Threading.Tasks;
using System.Web.Mvc;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Filters;
using OrderFlow.MVC.Services;

namespace OrderFlow.MVC.Controllers
{
    [AuthorizeRole("Admin", "SalesExecutive", "InventoryManager")]
    public class DashboardController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        public async Task<ActionResult> Index() { return View(await new DashboardService(_db).BuildAsync()); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
