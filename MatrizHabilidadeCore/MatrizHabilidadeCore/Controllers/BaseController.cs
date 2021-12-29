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
        protected readonly UserManager<Usuario> _userManager;
        protected readonly SignInManager<Usuario> _signInManager;

        public BaseController(DataBaseContext db, CookieService cookieService, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _db = db;
            _cookieService = cookieService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Usuario CurrentUser { get; set; }

        public int CurrentYear { get; private set; }

        public async Task SetCurrentYear(int year)
        {
            var claims = await _userManager.GetClaimsAsync(CurrentUser);

            if (claims.Any(c => c.Type == Claims.CurrentYear.Value))
            {
                var claim = claims.Where(c => c.Type == Claims.CurrentYear.Value).FirstOrDefault();

                await _userManager.RemoveClaimAsync(CurrentUser, claim);
            }

            await _userManager.AddClaimAsync(CurrentUser, new Claim(Claims.CurrentYear.Value, year.ToString()));
            CurrentYear = year;
        }

        protected void SetMessage(string message)
        {
            TempData["Message"] = message;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var cookie = _cookieService.GetCookie<CookieViewModel>("GestaoConhecimentoNovelis");
                var user = _db.Usuarios.Where(u => u.Login == cookie.Usu_login).FirstOrDefault();

                if (user == null)
                {
                    user = new Usuario()
                    {
                        Id = new Guid().ToString("N"),
                        Login = cookie.Usu_login,
                        UserName = cookie.Usu_login,
                        Nome = cookie.Usu_nome,
                        Email = cookie.Usu_email,
                        IsAtivo = true,
                    };
                    await _userManager.CreateAsync(user, "12345678");
                }

                foreach (var acesso in Enum.GetValues<NivelAcesso>())
                {
                    if (cookie.Usu_acesso < acesso)
                    {
                        await _userManager.RemoveFromRoleAsync(user, acesso.ToString("g"));
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, acesso.ToString("g"));
                    }
                }
                await _signInManager.SignInAsync(user, true);
            }
            CurrentUser = await _userManager.GetUserAsync(User);

            var claims = await _userManager.GetClaimsAsync(CurrentUser);

            if (claims.Any(c => c.Type == Claims.CurrentYear.Value))
            {
                CurrentYear = Convert.ToInt32(claims.Where(c => c.Type == Claims.CurrentYear.Value).FirstOrDefault().Value);
            }
            else
            {
                await SetCurrentYear(DateTime.Now.Year);
            }

            var coordenador = _db.Coordenadores.Where(c => c.Login == CurrentUser.Login).FirstOrDefault();

            if (coordenador != null)
            {
                CurrentUser.IsCoordenador = true;
                CurrentUser.CoordenadorId = coordenador.Id;
                CurrentUser.ColaboradorId = null;

                _db.Update(CurrentUser);
                _db.SaveChanges();
            }
            else
            {
                var colaborador = _db.Colaboradores.Where(c => c.Login == CurrentUser.Login).FirstOrDefault();

                if (colaborador != null)
                {
                    CurrentUser.IsCoordenador = false;
                    CurrentUser.ColaboradorId = colaborador.Id;
                    CurrentUser.CoordenadorId = null;

                    _db.Update(CurrentUser);
                    _db.SaveChanges();
                }
                else
                {
                    CurrentUser.IsCoordenador = null;
                    CurrentUser.CoordenadorId = null;
                    CurrentUser.ColaboradorId = null;

                    _db.Update(CurrentUser);
                    _db.SaveChanges();
                }
            }
        }
    }
}
