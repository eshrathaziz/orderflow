using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using OrderFlow.MVC.Data;
using OrderFlow.MVC.Models;
using OrderFlow.MVC.Security;
using OrderFlow.MVC.Services;
using OrderFlow.MVC.ViewModels;

namespace OrderFlow.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly OrderFlowDbContext _db = new OrderFlowDbContext();
        [AllowAnonymous] public ActionResult Login(string returnUrl) { return View(new LoginViewModel { ReturnUrl = returnUrl }); }
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = _db.Users.Include(u => u.Role).SingleOrDefault(u => u.Email == model.Email && u.IsActive);
            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash, user.PasswordSalt)) { ModelState.AddModelError("", "The email or password is incorrect."); return View(model); }
            user.LastLoginUtc = DateTime.UtcNow; _db.AuditLogs.Add(new AuditLog { UserId = user.UserId, EntityType = "User", EntityId = user.UserId.ToString(), Action = AuditAction.Login, Detail = "Successful login." }); _db.SaveChanges();
            var userData = new JavaScriptSerializer().Serialize(new CustomPrincipalSerializeModel { UserId = user.UserId, CustomerId = user.CustomerId, DisplayName = user.DisplayName, Role = user.Role.Name });
            var ticket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(model.RememberMe ? 720 : 60), model.RememberMe, userData);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) { HttpOnly = true, Secure = Request.IsSecureConnection });
            return RedirectToLocal(model.ReturnUrl);
        }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult Logout() { FormsAuthentication.SignOut(); return RedirectToAction("Login"); }
        private ActionResult RedirectToLocal(string returnUrl) { return Url.IsLocalUrl(returnUrl) ? (ActionResult)Redirect(returnUrl) : RedirectToAction("Index", "Dashboard"); }
        protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
    }
}
