using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsihoKorak_Platforma.Models;
using PsihoKorak_Platforma.Utils;
using System.Diagnostics;
using System.Security.Cryptography;

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
            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Append("SessionName", "", new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(-1)
            });
            HttpContext.Response.Cookies.Delete("SessionName");
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPsychologist(String email, string password)
        {
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) 
            {
                ViewData["Error"] = "Wrong email or password";
                return View("Login");
            }

            var psychologist = await ctx.Psychologists.AsNoTracking()
                                                       .FirstOrDefaultAsync(p => p.Email == email);

            if(psychologist != null)
            {
                String hashedPassword = CryptoUtils.HashPassword(password, Convert.FromBase64String(psychologist.PasswordSalt));
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
    }
}