using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeCore.ViewModel;
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

        public Colaborador CurrentColaborador { get; set; }
        public Coordenador CurrentCoordenador { get; set; }

        private int? _currentYear;
        public async Task<int> SetCurrentYear(int year)
        {
            dynamic claim;
            _currentYear = year;
            TempData[Claims.CurrentYear.Value] = year;
            if (_currentColaborador != null)
            {
                claim = _db.Claims.Where(c => c.Id == _currentColaborador.Id && c.ClaimType == Claims.CurrentYear.Value).FirstOrDefault();
            }
            claim = _db.Claims.Where(c => c.Id == _currentCoordenador.Id && c.ClaimType == Claims.CurrentYear.Value).FirstOrDefault();

            if (claim == null)
            {
                _db.Claims.Add(new MatrizHabilidadeDataBaseCore.Models.Claim()
                {
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
            dynamic year;
            if (_currentcolaborador != null)
            {
                year = _db.claims.where(y => y.id == _currentcolaborador.id && y.claimtype == claims.currentyear.value).select(y => y.claimvalue).firstordefault();
            }
            else
            {
                year = _db.claims.where(y => y.id == _currentcoordenador.id && y.claimtype == claims.currentyear.value).select(y => y.claimvalue).firstordefault();
            }
            _currentyear = convert.toint32(year);
            return 0;
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
