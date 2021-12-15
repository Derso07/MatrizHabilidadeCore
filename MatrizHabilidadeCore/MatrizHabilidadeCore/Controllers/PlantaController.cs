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

        public PlantaController(DataBaseContext _db, UserManager<Usuario> userManager, CookieService cookieService) : base(_db, userManager, cookieService)
        {

        }
        public ActionResult Index(string planta)
        {
            if (int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();

                var model = new PlantaViewModel(_planta.Id)
                {
                    PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                    PlantaDescricao = _planta.Descricao,
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
