﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PsihoKorak_Platforma.Models;
using PsihoKorak_Platforma.Utils;
using PsihoKorak_Platforma.ViewModels;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PsihoKorak_Platforma.Controllers
{
    public class PsychologistController : Controller
    {
        private readonly PsihoKorakPlatformaContext? ctx;

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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Append("SessionName", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            });
            HttpContext.Response.Cookies.Delete("SessionName");

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
            if(String.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewData["Error"] = "Invalid email format";
                return View("Register");
            }

            var psythologistToCheck = await ctx.Psychologists.AsNoTracking()
                                                             .FirstOrDefaultAsync(p => p.Email == Email);

            if (psythologistToCheck != null)
            {
                ViewData["Error"] = "Email is already in use";
                return View("Register");
            }

            if (String.IsNullOrWhiteSpace(Password) || Password.Length < 8 || !Regex.IsMatch(Password, @"[A-Z]") ||
                                                                              !Regex.IsMatch(Password, @"[a-z]") ||
                                                                              !Regex.IsMatch(Password, @"[0-9]") ||
                                                                              !Regex.IsMatch(Password, @"[\W_]"))
            {
                ViewData["Error"] = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character";
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

            var petients = await ctx.Patients
                                    .OrderBy(p => p.PatientId)
                                    .Select(p => new SelectListItem
                                    {
                                        Value = p.PatientId.ToString(),
                                        Text = p.FirstName + " " + p.LastName
                                    })
                                    .ToListAsync();

            ViewBag.Patients = petients;
        }

        [HttpPost]
        public IActionResult Detail(int SessionId)
        {
            var s = ctx.Sessions.Include(a => a.Helps).Include(b => b.SessionType).FirstOrDefault(v => v.SessionId == SessionId);

            var helps = ctx.Helps.AsNoTracking()
                                 .Include(v => v.Patient)
                                 .Where(h => h.SessionId == SessionId)
                                 .Select(m => new HelpsViewModelHelper
                                 {
                                     HelpsId = m.HelpsId,
                                     Note = m.Note,
                                     PatientId = m.PatientId,
                                     SessionId = m.SessionId,
                                     PsychologistId = m.PsychologistId,
                                     PatientName = m.Patient.FirstName + m.Patient.LastName
                                 })
                                 .ToList();

            return View(new DetailMasterDetailViewModel
            {
                Sessions = new MDViewModel
                {
                    SessionId = SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    SessionType = s.SessionType.SessionTypeName,
                    Helps = s.Helps
                },
                Helps = helps,
                Patients = ctx.Patients.AsNoTracking().ToList(),
                SessionTypes = ctx.SessionTypes.AsNoTracking().ToList()
            });
        }

        [HttpPost]
        public IActionResult FilterByPatient(String PatientName, int SessionId)
        {
            var s = ctx.Sessions.Include(a => a.Helps).Include(b => b.SessionType).FirstOrDefault(v => v.SessionId == SessionId);

            var helps = ctx.Helps.AsNoTracking()
                                 .Include(h => h.Patient)
                                 .Where(h => (h.Patient.FirstName + " " + h.Patient.LastName).Contains(PatientName))
                                 .Select(h => new HelpsViewModelHelper
                                 {
                                     HelpsId = h.HelpsId,
                                     Note = h.Note,
                                     PatientId = h.PatientId,
                                     SessionId = h.SessionId,
                                     PsychologistId = h.PsychologistId,
                                     PatientName = h.Patient.FirstName + " " + h.Patient.LastName
                                 })
                                 .ToList();

            return View("Detail", new DetailMasterDetailViewModel
            {
                Sessions = new MDViewModel
                {
                    SessionId = SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    SessionType = s.SessionType.SessionTypeName,
                    Helps = s.Helps
                },
                Helps = helps,
                Patients = ctx.Patients.AsNoTracking().ToList(),
                SessionTypes = ctx.SessionTypes.AsNoTracking().ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHelp(String Note, int PatientId, int SessionId)
        {
            var session = await ctx.Sessions.Include(s => s.SessionType)
                                            .Include(s => s.Helps)
                                            .FirstOrDefaultAsync(s => s.SessionId == SessionId);

            int patientCount = session.Helps.Select(h => h.PatientId).Distinct().Count();

            if(session.SessionType.SessionTypeName != "Grupna sesija" && patientCount > 0)
            {
                ViewData["Error"] = "Only one patient can be added to a non-group session";
                return View("Detail", await PrepareDetailViewModel(SessionId));
            }

            var help = new Help
            {
                Note = Note,
                PatientId = PatientId,
                SessionId = SessionId,
                PsychologistId = int.Parse(HttpContext.Session.GetString("Id"))
            };

            ctx.Add(help);
            await ctx.SaveChangesAsync();

            return View("Detail", await PrepareDetailViewModel(SessionId));
        }

        private async Task<DetailMasterDetailViewModel> PrepareDetailViewModel(int sessionId)
        {
            var s = await ctx.Sessions.Include(a => a.Helps)
                                      .Include(b => b.SessionType)
                                      .FirstOrDefaultAsync(v => v.SessionId == sessionId);

            var helps = await ctx.Helps.AsNoTracking()
                                       .Include(v => v.Patient)
                                       .Where(h => h.SessionId == sessionId)
                                       .Select(m => new HelpsViewModelHelper
                                       {
                                           HelpsId = m.HelpsId,
                                           Note = m.Note,
                                           PatientId = m.PatientId,
                                           SessionId = m.SessionId,
                                           PsychologistId = m.PsychologistId,
                                           PatientName = m.Patient.FirstName + m.Patient.LastName
                                       })
                                       .ToListAsync();

            return new DetailMasterDetailViewModel
            {
                Sessions = new MDViewModel
                {
                    SessionId = s.SessionId,
                    DateTime = s.DateTime.ToString(),
                    Duration = s.Duration.ToString(),
                    SessionType = s.SessionType.SessionTypeName,
                    Helps = s.Helps
                },
                Helps = helps,
                Patients = await ctx.Patients.AsNoTracking().ToListAsync(),
                SessionTypes = await ctx.SessionTypes.AsNoTracking().ToListAsync()
            };
        }

        [HttpPost]
        public async Task<IActionResult> DetailUpdate(int HelpsId)
        {
            var help = ctx.Helps.Include(h => h.Patient)
                                .FirstOrDefault(h => h.HelpsId == HelpsId);

            var model = new HelpsViewModelHelper
            {
                HelpsId = HelpsId,
                Note = help.Note,
                PatientId = help.PatientId,
                SessionId = help.SessionId,
                PsychologistId = help.PsychologistId,
                PatientName = help.Patient.FirstName + " " + help.Patient.LastName
            };

            await PrepareDropDownList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHelp(int HelpsId, String Note, int PatientId)
        {
            var help = ctx.Helps.Find(HelpsId);

            if (help != null)
            {
                var sessionId = help.SessionId;
                help.Note = Note;
                help.PatientId = PatientId;
                await ctx.SaveChangesAsync();

                var s = ctx.Sessions.Include(a => a.Helps).Include(b => b.SessionType).FirstOrDefault(v => v.SessionId == sessionId);

                var helps = ctx.Helps.AsNoTracking()
                                     .Include(v => v.Patient)
                                     .Where(h => h.SessionId == sessionId)
                                     .Select(m => new HelpsViewModelHelper
                                     {
                                         HelpsId = m.HelpsId,
                                         Note = m.Note,
                                         PatientId = m.PatientId,
                                         SessionId = m.SessionId,
                                         PsychologistId = m.PsychologistId,
                                         PatientName = m.Patient.FirstName + m.Patient.LastName
                                     })
                                     .ToList();

                return View("Detail", new DetailMasterDetailViewModel
                {
                    Sessions = new MDViewModel
                    {
                        SessionId = sessionId,
                        DateTime = s.DateTime.ToString(),
                        Duration = s.Duration.ToString(),
                        SessionType = s.SessionType.SessionTypeName,
                        Helps = s.Helps
                    },
                    Helps = helps,
                    Patients = ctx.Patients.AsNoTracking().ToList(),
                    SessionTypes = ctx.SessionTypes.AsNoTracking().ToList()
                });
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult DeleteHelp(int HelpsId)
        {
            var help = ctx.Helps.Find(HelpsId);

            if(help != null)
            {
                var sessionId = help.SessionId;
                ctx.Remove(help);
                ctx.SaveChanges();

                var s = ctx.Sessions.Include(a => a.Helps).Include(b => b.SessionType).FirstOrDefault(v => v.SessionId == sessionId);

                var helps = ctx.Helps.AsNoTracking()
                                     .Include(v => v.Patient)
                                     .Where(h => h.SessionId == sessionId)
                                     .Select(m => new HelpsViewModelHelper
                                     {
                                         HelpsId = m.HelpsId,
                                         Note = m.Note,
                                         PatientId = m.PatientId,
                                         SessionId = m.SessionId,
                                         PsychologistId = m.PsychologistId,
                                         PatientName = m.Patient.FirstName + m.Patient.LastName
                                     })
                                     .ToList();

                return View("Detail", new DetailMasterDetailViewModel
                {
                    Sessions = new MDViewModel
                    {
                        SessionId = sessionId,
                        DateTime = s.DateTime.ToString(),
                        Duration = s.Duration.ToString(),
                        SessionType = s.SessionType.SessionTypeName,
                        Helps = s.Helps
                    },
                    Helps = helps,
                    Patients = ctx.Patients.AsNoTracking().ToList(),
                    SessionTypes = ctx.SessionTypes.AsNoTracking().ToList()
                });
            }

            return NotFound();
        }

        public IActionResult SessionTypes()
        {
            var query = ctx.SessionTypes.AsNoTracking()
                                        .ToList();

            var st = query.Select(s => new SessionTypeViewModel
            {
                SessionTypeId = s.SessionTypeId,
                SessionTypeName = s.SessionTypeName
            }).ToList();

            var model = new ListSessionTypeViewModel
            {
                sessionTypes = st
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSessionType(String SessionTypeName)
        {
            var sessionType = new SessionType
            {
                SessionTypeName = SessionTypeName
            };

            ctx.SessionTypes.Add(sessionType);
            await ctx.SaveChangesAsync();

            return RedirectToAction("sessionTypes");
        }

        [HttpPost]
        public IActionResult DeleteSessionType(int SessionTypeId)
        {
            var sessionType = ctx.SessionTypes.Find(SessionTypeId);
            var isForeignKey = ctx.Sessions.Any(s => s.SessionTypeId == SessionTypeId);

            if(sessionType != null && !isForeignKey)
            {
                ctx.Remove(sessionType);
                ctx.SaveChanges();

                return RedirectToAction("sessionTypes");
            }

            ViewData["Error"] = "Cannot delete because it is foreign key";
            return RedirectToAction("sessionTypes");
        }

        [HttpPost]
        public IActionResult FilterBySessionType(String SessionTypeName)
        {
            if(!String.IsNullOrEmpty(SessionTypeName))
            {
                var query = ctx.SessionTypes.Where(st => st.SessionTypeName.Contains(SessionTypeName))
                                            .ToList();

                var st = query.Select(s => new SessionTypeViewModel
                {
                    SessionTypeId = s.SessionTypeId,
                    SessionTypeName = s.SessionTypeName
                }).ToList();

                var model = new ListSessionTypeViewModel
                {
                    sessionTypes = st
                };

                return View("SessionTypes", model);
            }
            else
            {
                var query = ctx.SessionTypes.ToList();

                var st = query.Select(s => new SessionTypeViewModel
                {
                    SessionTypeId = s.SessionTypeId,
                    SessionTypeName = s.SessionTypeName
                }).ToList();

                var model = new ListSessionTypeViewModel
                {
                    sessionTypes = st
                };

                return View("SessionTypes", model);
            }
        }

        [HttpPost]
        public IActionResult SessionTypeUpdate(int SessionTypeId)
        {
            var sessionType = ctx.SessionTypes.FirstOrDefault(st => st.SessionTypeId == SessionTypeId);

            var model = new SessionTypeViewModel
            {
                SessionTypeId = SessionTypeId,
                SessionTypeName = sessionType.SessionTypeName
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateSessionType(int SessionTypeId, String SessionTypeName)
        {
            var sessionType = ctx.SessionTypes.Find(SessionTypeId);

            if(sessionType != null)
            {
                sessionType.SessionTypeName = SessionTypeName;
                ctx.SaveChanges();

                return RedirectToAction("SessionTypes");
            }

            return BadRequest();
        }
    }
}
