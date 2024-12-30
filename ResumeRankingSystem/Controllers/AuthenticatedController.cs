using Microsoft.AspNetCore.Mvc;

namespace ResumeRankingSystem.Controllers
{
    public class AuthenticatedController : Controller
    {
        protected bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        protected IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Users");
        }
    }
}
