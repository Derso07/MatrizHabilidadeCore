using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class BaseController : Controller
    {
        protected readonly DataBaseContext _db;
        protected readonly CookieService _cookieService;

        public BaseController(DataBaseContext db, CookieService cookieService)
        {
            _db = db;
            _cookieService = cookieService;
        }

        private Usuario _currentUser;

        public Usuario CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    var userName = User.FindFirstValue(Claims.UserId.Value);

                    CurrentUser = _db.Usuarios
                        .Where(u => u.Nome == userName)
                        .FirstOrDefault();
                }
                return _currentUser;
            }
            set { _currentUser = value; }
        }

        private int? _currentYear;
        public async Task<int> SetCurrentYear(int year)
        {
            _currentYear = year;
            TempData[Claims.CurrentYear.Value] = year;
            var claim = _db.Claims.Where(c => c.UsuarioId == _currentUser.Id && c.ClaimType == Claims.CurrentYear.Value).FirstOrDefault();
            if (claim == null )
            {
                _db.Claims.Add(new MatrizHabilidadeDataBaseCore.Models.Claim() {
                    ClaimType = Claims.CurrentYear.Value,
                    ClaimValue = year.ToString()
                });
            }
            else
            {
                claim.ClaimValue = year.ToString();
                _db.Update(claim);
                _db.SaveChanges();
            }

            return 0;
        }

        public int GetCurrentYear()
        {
            var year = _db.Claims.Where(y => y.UsuarioId == CurrentUser.Id && y.ClaimType == Claims.CurrentYear.Value).Select(y => y.ClaimValue).FirstOrDefault();
            _currentYear = Convert.ToInt32(year);
            return _currentYear.Value;
        }

        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            TempData[Claims.CurrentYear.Value] = GetCurrentYear();
            return;
        }

        protected void SetMessage(string message)
        {
            TempData["Message"] = message;
        }
    }
}
