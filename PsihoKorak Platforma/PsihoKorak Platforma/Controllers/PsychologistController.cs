using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PsihoKorak_Platforma.Models;
using PsihoKorak_Platforma.Utils;
using PsihoKorak_Platforma.ViewModels;

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
                var query = ctx.Sessions.AsNoTracking()
                                        .Where(s => s.Helps.Any(h => h.PsychologistId == psychologistId))
                                        .Include(s => s.SessionType)
                                        .ToList();

                var sessions = query.Select(s => new MDViewModel
                {
                    SessionId = s.SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    Helps = s.Helps,
                    SessionType = s.SessionType.SessionTypeName
                }).ToList();

                var model = new ListMDViewModel
                {
                    Md = sessions,
                };

                return View(model);
            }
            else
            {
                return BadRequest();
            }
        }

        public IActionResult Login()
        {
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

        [HttpPost]
        public IActionResult FilterSessionsByType(String SessionType)
        {
            if (int.TryParse(HttpContext.Session.GetString("Id"), out int psychologistId))
            {
                var query = ctx.Sessions.AsNoTracking()
                                        .Where(s => s.Helps.Any(h => h.PsychologistId == psychologistId))
                                        .Where(s => s.SessionType.SessionTypeName.Contains(SessionType))
                                        .Include(s => s.SessionType)
                                        .ToList();

                var sessions = query.Select(s => new MDViewModel
                {
                    SessionId = s.SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    Helps = s.Helps,
                    SessionType = s.SessionType.SessionTypeName
                }).ToList();

                var model = new ListMDViewModel
                {
                    Md = sessions,
                };

                return View("Index", model);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Id") == null)
            {
                return RedirectToAction("Login");
            }

            await PrepareDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession(DateTime Date, int Duration, String SessionTypeId)
        {
            try
            {
                Session newSession = new Session
                {
                    DateTime = Date,
                    Duration = TimeSpan.FromMinutes(Duration),
                    SessionTypeId = int.Parse(SessionTypeId)
                };

                ctx.Sessions.Add(newSession);
                await ctx.SaveChangesAsync();

                var lastSession = await ctx.Sessions
                                           .OrderByDescending(s => s.SessionId) 
                                           .FirstOrDefaultAsync();

                var newHelp = new Help
                {
                    PatientId = null,
                    SessionId = lastSession.SessionId,
                    PsychologistId = int.Parse(HttpContext.Session.GetString("Id"))
                };

                ctx.Helps.Add(newHelp);
                await ctx.SaveChangesAsync();

                var query = ctx.Sessions.AsNoTracking()
                                        .Where(s => s.Helps.Any(h => h.PsychologistId == int.Parse(HttpContext.Session.GetString("Id"))))
                                        .Include(s => s.SessionType)
                                        .ToList();

                var sessions = query.Select(s => new MDViewModel
                {
                    SessionId = s.SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    Helps = s.Helps,
                    SessionType = s.SessionType.SessionTypeName
                }).ToList();

                var model = new ListMDViewModel
                {
                    Md = sessions,
                };

                return View(nameof(Index), model);
            }
            catch
            {
                await PrepareDropDownList();
                return View("Create");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int SessionId)
        {
            if (HttpContext.Session.GetString("Id") == null)
            {
                return RedirectToAction("Login");
            }

            var session = ctx.Sessions.Include(s => s.SessionType)
                                      .FirstOrDefault(s => s.SessionId == SessionId);

            var model = new MDViewModel
            {
                SessionId = SessionId,
                DateTime = session.DateTime.ToString(),
                Duration = session.Duration.ToString(),
                Helps = session.Helps,
                SessionType = session.SessionType.SessionTypeName
            };

            await PrepareDropDownList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSession(int SessionId, DateTime Date, int Duration, String SessionTypeId)
        {
            var session = ctx.Sessions.Find(SessionId);

            if (session != null)
            {
                session.DateTime = Date;
                session.Duration = TimeSpan.FromMinutes(Duration);
                session.SessionTypeId = int.Parse(SessionTypeId);
                await ctx.SaveChangesAsync();
            }

            var query = ctx.Sessions.AsNoTracking()
                                        .Where(s => s.Helps.Any(h => h.PsychologistId == int.Parse(HttpContext.Session.GetString("Id"))))
                                        .Include(s => s.SessionType)
                                        .ToList();

            var sessions = query.Select(s => new MDViewModel
            {
                SessionId = s.SessionId,
                DateTime = s.DateTime.ToString(),
                Duration = s.Duration.ToString(),
                Helps = s.Helps,
                SessionType = s.SessionType.SessionTypeName
            }).ToList();

            var model = new ListMDViewModel
            {
                Md = sessions,
            };

            return View(nameof(Index), model);
        }

        [HttpPost]
        public IActionResult DeleteSession(int SessionId)
        {
            var session = ctx.Sessions.Find(SessionId);

            var helps = ctx.Helps.Where(h => h.SessionId == SessionId)
                                 .ToList();

            foreach(var help in helps)
            {
                var helpId = help.HelpsId;
                var helpToDelete = ctx.Helps.Find(helpId);
                if(helpToDelete != null)
                {
                    ctx.Remove(helpToDelete);
                    ctx.SaveChanges();
                }
            }

            if(session != null)
            {
                ctx.Remove(session);
                ctx.SaveChanges();
            }

            var query = ctx.Sessions.AsNoTracking()
                                        .Where(s => s.Helps.Any(h => h.PsychologistId == int.Parse(HttpContext.Session.GetString("Id"))))
                                        .Include(s => s.SessionType)
                                        .ToList();

            var sessions = query.Select(s => new MDViewModel
            {
                SessionId = s.SessionId,
                DateTime = s.DateTime.ToString(),
                Duration = s.Duration.ToString(),
                Helps = s.Helps,
                SessionType = s.SessionType.SessionTypeName
            }).ToList();

            var model = new ListMDViewModel
            {
                Md = sessions,
            };

            return View(nameof(Index), model);
        }

        private async Task PrepareDropDownList()
        {
            var sessionTypes = await ctx.SessionTypes
                                       .OrderBy(st => st.SessionTypeId)
                                       .Select(st => new SelectListItem
                                       {
                                           Value = st.SessionTypeId.ToString(),
                                           Text = st.SessionTypeName
                                       })
                                       .ToListAsync();

            ViewBag.SessionItems = sessionTypes;
        }
    }
}
