using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [LoginRequired("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api)
        {
            _api = api;
        }

        // ---------- Dashboard ----------
        public IActionResult Dashboard()
        {
            var model = new DashboardModel();
            model.Rows = new List<DashboardRow>();

            foreach (var ev in LoadMyEvents())
            {
                var row = new DashboardRow();
                row.Event = ev;

                var enrolResult = _api.Get("api/events/" + ev.EventId + "/enrolments");
                if (enrolResult.Success)
                {
                    row.TotalEnrolments = enrolResult.Read<List<EnrolmentModel>>().Count;
                }

                model.Rows.Add(row);
            }

            return View(model);
        }

        // ---------- Events ----------
        public IActionResult Events()
        {
            return View(LoadMyEvents());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            SetFormInfo("CreateEvent", "Create Event");
            return View("EventForm", new EventForm { EventDate = DateTime.Today.AddDays(14), EventType = "run" });
        }

        [HttpPost]
        public IActionResult CreateEvent(EventForm form)
        {
            SetFormInfo("CreateEvent", "Create Event");
            if (!ModelState.IsValid) return View("EventForm", form);

            var result = _api.Post("api/events", ToBody(form));
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return View("EventForm", form);
            }

            int newId = result.Read<IdModel>().EventId;

            if (form.Banner != null)
            {
                var upload = _api.PostFile("api/events/" + newId + "/banner", form.Banner);
                if (!upload.Success)
                {
                    TempData["Error"] = "Event saved, but the banner failed: " + upload.Error;
                    return RedirectToAction("Events");
                }
            }

            TempData["Success"] = "Event created.";
            return RedirectToAction("Events");
        }

        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            var ev = LoadEvent(id);
            if (ev == null) return RedirectToAction("Events");

            var form = new EventForm();
            form.EventId = ev.EventId;
            form.EventName = ev.EventName;
            form.Description = ev.Description;
            form.EventDate = ev.EventDate;
            form.Location = ev.Location;
            form.Distance = ev.Distance;
            form.EventType = ev.EventType;
            form.ExistingBannerUrl = ev.BannerUrl;

            SetFormInfo("EditEvent", "Edit Event");
            return View("EventForm", form);
        }

        [HttpPost]
        public IActionResult EditEvent(EventForm form)
        {
            SetFormInfo("EditEvent", "Edit Event");
            if (!ModelState.IsValid) return View("EventForm", form);

            var result = _api.Put("api/events/" + form.EventId, ToBody(form));
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return View("EventForm", form);
            }

            if (form.Banner != null)
            {
                var upload = _api.PostFile("api/events/" + form.EventId + "/banner", form.Banner);
                if (!upload.Success)
                {
                    TempData["Error"] = "Event saved, but the banner failed: " + upload.Error;
                    return RedirectToAction("Events");
                }
            }

            TempData["Success"] = "Event updated.";
            return RedirectToAction("Events");
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var result = _api.Delete("api/events/" + id);

            if (result.Success) TempData["Success"] = "Event deleted.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("Events");
        }

        // ---------- Categories ----------
        public IActionResult Categories(int id)
        {
            var ev = LoadEvent(id);
            if (ev == null) return RedirectToAction("Events");

            var model = new CategoriesPageModel();
            model.Event = ev;
            model.Categories = new List<CategoryModel>();

            var result = _api.Get("api/events/" + id + "/categories");
            if (result.Success) model.Categories = result.Read<List<CategoryModel>>();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddCategory(CategoryForm form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in the category name, a distance and max participants.";
                return RedirectToAction("Categories", new { id = form.EventId });
            }

            var result = _api.Post("api/events/" + form.EventId + "/categories", new
            {
                categoryName = form.CategoryName,
                distanceKm = form.DistanceKm,
                maxParticipants = form.MaxParticipants
            });

            if (result.Success) TempData["Success"] = "Category added.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("Categories", new { id = form.EventId });
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id, int eventId)
        {
            var result = _api.Delete("api/categories/" + id);

            if (result.Success) TempData["Success"] = "Category deleted.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("Categories", new { id = eventId });
        }

        // ---------- Enrolments ----------
        public IActionResult Enrolments(int id)
        {
            var ev = LoadEvent(id);
            if (ev == null) return RedirectToAction("Events");

            var model = new EnrolmentsPageModel();
            model.Event = ev;
            model.Enrolments = new List<EnrolmentModel>();

            var result = _api.Get("api/events/" + id + "/enrolments");
            if (result.Success) model.Enrolments = result.Read<List<EnrolmentModel>>();
            else TempData["Error"] = result.Error;

            return View(model);
        }

        // ---------- Results ----------
        public IActionResult Results(int id)
        {
            var ev = LoadEvent(id);
            if (ev == null) return RedirectToAction("Events");

            var model = new ResultsPageModel();
            model.Event = ev;
            model.Enrolments = new List<EnrolmentModel>();
            model.Results = new List<ResultModel>();

            var enrolResult = _api.Get("api/events/" + id + "/enrolments");
            if (enrolResult.Success) model.Enrolments = enrolResult.Read<List<EnrolmentModel>>();

            var resultResult = _api.Get("api/events/" + id + "/results");
            if (resultResult.Success) model.Results = resultResult.Read<List<ResultModel>>();

            return View(model);
        }

        [HttpPost]
        public IActionResult CaptureResult(ResultForm form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Enter a finish time like 01:45:30 and a position of 1 or more.";
                return RedirectToAction("Results", new { id = form.EventId });
            }

            var result = _api.Post("api/enrolments/" + form.EnrolmentId + "/results", new
            {
                finishTime = form.FinishTime,
                position = form.Position
            });

            if (result.Success) TempData["Success"] = "Result saved and published.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("Results", new { id = form.EventId });
        }

        // ---------- Helpers ----------
        private List<EventModel> LoadMyEvents()
        {
            var events = new List<EventModel>();
            var result = _api.Get("api/events/mine");

            if (result.Success) events = result.Read<List<EventModel>>();
            else TempData["Error"] = result.Error;

            return events;
        }

        private EventModel LoadEvent(int id)
        {
            var result = _api.Get("api/events/" + id);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return null;
            }
            return result.Read<EventModel>();
        }

        private object ToBody(EventForm form)
        {
            return new
            {
                eventName = form.EventName,
                eventDate = form.EventDate,
                location = form.Location,
                description = form.Description,
                distance = form.Distance,
                eventType = form.EventType
            };
        }

        private void SetFormInfo(string action, string heading)
        {
            ViewBag.FormAction = action;
            ViewBag.Heading = heading;
        }
    }
}
