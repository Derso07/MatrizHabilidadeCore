using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MatrizHabilidadeCore.Controllers
{
    public class PlantaController : BaseController
    {

        public PlantaController(DataBaseContext _db, CookieService cookieService) : base(_db, cookieService)
        {

        }
        public ActionResult Index(string planta)
        {
            var path = HttpContext.Request.Path;
            var query = HttpContext.Request.QueryString;

            if (int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();

                var model = new PlantaViewModel(_planta.Id,_db, base.GetCurrentYear())
                {
                    PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                    PlantaDescricao = _planta.Descricao,
                    PathAndQuery = $"{path}{query}"
                };

                if (model.RequireRedirect)
                {
                    return RedirectToAction("Index", "Area", new { planta, area = model.Parameter, isRedirected = true });
                }

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
