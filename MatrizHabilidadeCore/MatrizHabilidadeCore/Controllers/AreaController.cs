using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class AreaController : BaseController
    {
        public AreaController(DataBaseContext _db, CookieService _cookieService):base(_db, _cookieService)
        {
        }
        public ActionResult Index(string planta, string area, bool? isRedirected)
        {
            var path = HttpContext.Request.Path;
            var query = HttpContext.Request.QueryString;
            var pathAndQuery = path + query;

            if (!isRedirected.HasValue)
            {
                isRedirected = false;
            }

            if (int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();

                if (int.TryParse(Encrypting.Decrypt(area), out int area_id))
                {
                    Area _area = _db.Areas.Where(p => p.Id == area_id).FirstOrDefault();

                    var model = new AreaViewModel(_area.Id, _db, _cookieService, base.GetCurrentYear())
                    {
                        PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                        PlantaDescricao = _planta.Descricao,
                        AreaId = Encrypting.Encrypt(_area.Id.ToString()),
                        AreaDescricao = _area.Alias,
                        IsRedirected = isRedirected.Value,
                        PathAndQuery = pathAndQuery
                    };

                    return View(model);
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

