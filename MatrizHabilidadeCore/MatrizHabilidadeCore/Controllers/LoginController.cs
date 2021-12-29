using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.ViewModel;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MatrizHabilidadeCore.Controllers
{
    public class LoginController : BaseController
    {
        public LoginController(DataBaseContext _db, CookieService _cookieService, UserManager<Usuario> _userManager, SignInManager<Usuario> _signInManager) : base(_db, _cookieService, _userManager, _signInManager)
        {
        }

        [System.Web.Mvc.HttpPost()]
        public async Task<IActionResult> Login(string returnUrl)
        {
            return Redirect(returnUrl);
            var cookie = _cookieService.GetCookie<CookieViewModel>("GestaoConhecimentoNovelis");
            var user = _db.Usuarios.Where(x => x.Login == cookie.Usu_login).FirstOrDefault();
#if DEBUG
            if (user == null)
            {
                user = new Usuario()
                {
                    Login = "SILVAALES",
                    Nome = "ALESSANDRA APARECIDA SILVA",
                    Chapa = "2002782",
                    Email = "ALESSANDRA.SILVA1@NOVELIS.ADITYABIRLA.COM",
                    IsAtivo = true,
                };
            }
#endif
            if (user != null)
            {
                //Salvando informações do usuario em um cookie para que seja reconhecido o login
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Login),
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.PrimarySid, user.Id.ToString())
                };

                //Guardando as informações da Claim no Cookie
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                await HttpContext.SignInAsync(claimsPrincipal);
                //Redireciona para a página inicial
                return Redirect("/");
            }

            TempData["Erro"] = "Ops! Usuario ou senha inválidos!";
            return Redirect("/login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}
