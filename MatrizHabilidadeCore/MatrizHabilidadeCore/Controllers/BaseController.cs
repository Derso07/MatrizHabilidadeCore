using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class BaseController : Controller
    {
        protected readonly DataBaseContext _db;
        protected readonly UserManager<Usuario> _userManager;
        protected readonly CookieService _cookieService;
        protected readonly ClaimService _claimService;

        public BaseController(DataBaseContext db, UserManager<Usuario> userManager, CookieService cookieService, ClaimService claimService)
        {
            _db = db;
            _userManager = userManager;
            _cookieService = cookieService;
            _claimService = claimService;
        }

        private Usuario _currentUser;

        public Usuario CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    var userName = User.FindFirstValue(ClaimTypes.Name);

                    CurrentUser = _userManager.Users
                        .Where(u => u.UserName == userName)
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
            await _claimService.AddUpdateClaim(Claims.CurrentYear, year.ToString());

            return 0;
        }

        public int GetCurrentYear()
        {
           
            return _currentYear.Value;
        }
        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var year = await _claimService.GetClaimValue<int>(Claims.CurrentYear);
            _currentYear = year;
            TempData[Claims.CurrentYear.Value] = year;

            return;
        }
       
        protected void SetMessage(string message)
        {
            TempData["Message"] = message;
        }
    }
}
