using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginForm());
        }

        [HttpPost]
        public IActionResult Login(LoginForm form)
        {
            if (!ModelState.IsValid) return View(form);

            var result = _api.Post("api/auth/login", new { email = form.Email, password = form.Password });
            if (!result.Success)
            {
                ViewBag.Error = result.StatusCode == 401 ? "Invalid email or password." : result.Error;
                return View(form);
            }

            // Save the login details in the session
            var login = result.Read<LoginModel>();
            HttpContext.Session.SetString("Token", login.Token);
            HttpContext.Session.SetString("Role", login.Role);
            HttpContext.Session.SetInt32("UserId", login.UserId);

            var userResult = _api.Get("api/users/" + login.UserId);
            if (userResult.Success)
            {
                var user = userResult.Read<UserModel>();
                HttpContext.Session.SetString("FullName", user.FullName);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterForm { Role = "Participant" });
        }

        [HttpPost]
        public IActionResult Register(RegisterForm form)
        {
            if (!ModelState.IsValid) return View(form);

            var result = _api.Post("api/auth/register", new
            {
                fullName = form.FullName,
                email = form.Email,
                password = form.Password,
                role = form.Role
            });

            if (!result.Success)
            {
                ViewBag.Error = result.Error;
                return View(form);
            }

            TempData["Success"] = "Account created. Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [LoginRequired]
        [HttpGet]
        public IActionResult Profile()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var result = _api.Get("api/users/" + userId);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            var user = result.Read<UserModel>();
            var form = new ProfileForm();
            form.FullName = user.FullName;
            form.Email = user.Email;
            form.ProfilePictureUrl = user.ProfilePictureUrl;
            return View(form);
        }

        [LoginRequired]
        [HttpPost]
        public IActionResult Profile(ProfileForm form)
        {
            if (!ModelState.IsValid) return View(form);

            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var result = _api.Put("api/users/" + userId, new { fullName = form.FullName, email = form.Email });

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return View(form);
            }

            HttpContext.Session.SetString("FullName", form.FullName);
            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Profile");
        }

        [LoginRequired]
        [HttpPost]
        public IActionResult UploadPicture(IFormFile picture)
        {
            if (picture == null)
            {
                TempData["Error"] = "Please choose an image first.";
                return RedirectToAction("Profile");
            }

            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var result = _api.PostFile("api/users/" + userId + "/picture", picture);

            if (result.Success) TempData["Success"] = "Profile picture updated.";
            else TempData["Error"] = result.Error;

            return RedirectToAction("Profile");
        }
    }
}
