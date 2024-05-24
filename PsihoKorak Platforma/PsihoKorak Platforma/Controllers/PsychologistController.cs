using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsihoKorak_Platforma.Models;
using PsihoKorak_Platforma.Utils;

namespace PsihoKorak_Platforma.Controllers
{
    public class PsychologistController : Controller
    {
        private readonly PsihoKorakPlatformaContext ctx;

        public PsychologistController(PsihoKorakPlatformaContext ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Id") == null)
            {
                return RedirectToAction("Login");
            }

            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            if (int.TryParse(HttpContext.Session.GetString("Id"), out int psychologistId))
            {
                return View();
            }
            else
            {
                return BadRequest();
            }
        }

        public IActionResult Login()
        {
            if(HttpContext.Session.GetString("Id") != null)
            {
                return RedirectToAction("Index");
            }

            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Append("SessionName", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            });
            HttpContext.Response.Cookies.Delete("SessionName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPsychologist(String Email, String Password)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Invalid data.";
                return View("Login");
            }

            var psychologist = await ctx.Psychologists.AsNoTracking()
                                                      .FirstOrDefaultAsync(p => p.Email == Email);

            if (psychologist != null)
            {
                string hashedPassword = CryptoUtils.HashPassword(Password, Convert.FromBase64String(psychologist.PasswordSalt));
                if (psychologist.HashedPassword.Equals(hashedPassword, StringComparison.Ordinal))
                {
                    HttpContext.Session.SetString("FirstName", psychologist.FirstName);
                    HttpContext.Session.SetString("LastName", psychologist.LastName);
                    HttpContext.Session.SetString("Id", psychologist.PsychologistId.ToString());

                    return RedirectToAction("Index");
                }
            }

            ViewData["Error"] = "Wrong email or password";
            return View("Login");
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Id") != null)
            {
                return RedirectToAction("Index");
            }

            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Append("SessionName", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            });
            HttpContext.Response.Cookies.Delete("SessionName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPsychologist(String FirstName, String LastName, String Email, String Password)
        {
            var psythologistToCheck = await ctx.Psychologists.AsNoTracking()
                                                             .FirstOrDefaultAsync(p => p.Email == Email);

            if (psythologistToCheck != null)
            {
                ViewData["Error"] = "Email is already in use";
                return View("Register");
            }

            var salt = CryptoUtils.GetSalt();
            var hashedPassword = CryptoUtils.HashPassword(Password, salt);

            var newPsychologist = new Psychologist
            {
                Email = Email,
                HashedPassword = hashedPassword,
                PasswordSalt = Convert.ToBase64String(salt),
                FirstName = FirstName,
                LastName = LastName
            };

            try
            {
                await ctx.Psychologists.AddAsync(newPsychologist);
                await ctx.SaveChangesAsync();

                HttpContext.Session.SetString("FirstName", newPsychologist.FirstName);
                HttpContext.Session.SetString("LastName", newPsychologist.LastName);
                HttpContext.Session.SetString("Id", newPsychologist.PsychologistId.ToString());

                return RedirectToAction("Index");
            }
            catch
            {
                ViewData["Error"] = "Error with registration";
                return View("Register");
            }
        }
    }
}
