using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [LoginRequired("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Details(int id)
        {
            var eventResult = _api.Get("api/events/" + id);
            if (!eventResult.Success)
            {
                TempData["Error"] = eventResult.Error;
                return RedirectToAction("Index", "Home");
            }

            var model = new EventDetailsModel();
            model.Event = eventResult.Read<EventModel>();
            model.Categories = new List<CategoryModel>();

            var categoryResult = _api.Get("api/events/" + id + "/categories");
            if (categoryResult.Success)
            {
                model.Categories = categoryResult.Read<List<CategoryModel>>();
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Enrol(int categoryId, int eventId)
        {
            var result = _api.Post("api/categories/" + categoryId + "/enrolments", new { });

            if (result.Success)
            {
                TempData["Success"] = "You are enrolled!";
                return RedirectToAction("MyEnrolments");
            }

            TempData["Error"] = result.Error;
            return RedirectToAction("Details", new { id = eventId });
        }

        public IActionResult MyEnrolments()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var enrolments = new List<EnrolmentModel>();

            var result = _api.Get("api/users/" + userId + "/enrolments");
            if (result.Success) enrolments = result.Read<List<EnrolmentModel>>();
            else TempData["Error"] = result.Error;

            return View(enrolments.OrderBy(e => e.EventDate).ToList());
        }

        [HttpPost]
        public IActionResult CancelEnrolment(int id)
        {
            var result = _api.Delete("api/enrolments/" + id);

            if (result.Success) TempData["Success"] = "Enrolment cancelled.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("MyEnrolments");
        }

        public IActionResult MyResults()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var results = new List<ResultModel>();

            var result = _api.Get("api/users/" + userId + "/results");
            if (result.Success) results = result.Read<List<ResultModel>>();
            else TempData["Error"] = result.Error;

            return View(results.OrderByDescending(r => r.EventDate).ToList());
        }
    }
}
