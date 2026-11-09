using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index(string type, string search)
        {
            string role = HttpContext.Session.GetString("Role");

            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");

            string url = "api/events?type=" + Uri.EscapeDataString(type ?? "") +
                         "&search=" + Uri.EscapeDataString(search ?? "");

            var events = new List<EventModel>();
            var result = _api.Get(url);
            if (result.Success)
            {
                events = result.Read<List<EventModel>>();
            }
            else
            {
                TempData["Error"] = result.Error;
            }

            ViewBag.Type = type;
            ViewBag.Search = search;
            return View(events);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
