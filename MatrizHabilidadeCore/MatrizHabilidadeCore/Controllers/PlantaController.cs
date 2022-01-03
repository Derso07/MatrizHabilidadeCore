using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrizHabilidadeCore.Controllers
{
    public class PlantaController : BaseController
    {

        public PlantaController(DataBaseContext _db, CookieService cookieService, UserManager<Usuario> _userManager, SignInManager<Usuario> _signInManager) : base(_db, cookieService, _userManager, _signInManager)
        {

        }
        public ActionResult Index(string planta)
        {
            var path = HttpContext.Request.Path;
            var query = HttpContext.Request.QueryString;

            if (int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();

                var model = new PlantaViewModel()
                {
                    PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                    PlantaDescricao = _planta.Descricao,
                    PathAndQuery = $"{path}{query}"
                };

                model.Areas = new List<LinhaGraficoFarol>();
                model.Conhecimento = new ProgressBar();
                model.Treinamento = new ProgressBar();

                var date = new DateTime(CurrentYear, 3, DateTime.DaysInMonth(CurrentYear, 3));
                var gapCalculator = new GAPCalculatorService();

                var tiposTreinamento = _db.TiposTreinamentos
                    .Where(t => t.Treinamentos.Any())
                    .ToList();

                var areas = _db.Areas
                    .Where(a => a.PlantaId == _planta.Id)
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                    .Where(a => a.Id != 1)
                    .ToList();

                if (areas.Count.Equals(1))
                {
                    model.RequireRedirect = true;
                    model.Parameter = Encrypting.Encrypt(areas[0].Id.ToString());
                }
                else
                {
                    List<int> areaIds = areas
                        .Select(a => a.Id)
                        .ToList();

                    var treinamentos = _db.ViewTreinamentos
                        .Where(t => t.PlantaId == _planta.Id)
                        .Where(t => areaIds.Contains(t.AreaId))
                        .ToList();

                    var treinamentosEspecificos = _db.ViewTreinamentosEspecificos
                        .Where(t => t.PlantaId == _planta.Id)
                        .Where(t => areaIds.Contains(t.AreaId))
                        .Where(t => !t.IsNA)
                        .ToList();

                    var data = gapCalculator.Compute(date, tiposTreinamento.Select(t => t.Id).ToList(), treinamentos, treinamentosEspecificos);

                    model.Conhecimento.Media = data.Geral.Conhecimento;
                    model.Conhecimento.Treinamentos = new Dictionary<string, double>()
                    {
                        { "Específico", data.Especifico.Conhecimento },
                    };

                    foreach (var tipoTreinamento in data.TipoTreinamentos)
                    {
                        var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                        model.Conhecimento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Conhecimento);
                    }

                    model.Treinamento.Media = data.Geral.Treinamento;
                    model.Treinamento.Treinamentos = new Dictionary<string, double>()
                    {
                        { "Específico", data.Especifico.Treinamento },
                    };

                    foreach (var tipoTreinamento in data.TipoTreinamentos)
                    {
                        var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                        model.Treinamento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Treinamento);
                    }

                    for (var x = 0; x < areas.Count; x++)
                    {
                        var treinamentosArea = treinamentos
                            .Where(t => t.AreaId == areas[x].Id)
                            .ToList();

                        var treinamentosEspecificosArea = treinamentosEspecificos
                            .Where(t => t.AreaId == areas[x].Id)
                            .ToList();

                        data = gapCalculator.Compute(date, tiposTreinamento.Select(t => t.Id).ToList(), treinamentosArea, treinamentosEspecificosArea);

                        var linha = new LinhaGraficoFarol()
                        {
                            Id = Encrypting.Encrypt(areas[x].Id.ToString()),
                            Descricao = areas[x].Alias,
                        };

                        linha.Auditoria = new LinhaGraficoFarol.Farol()
                        {
                            Media = data.Geral.Conhecimento,
                            Treinamentos = new Dictionary<string, double>()
                        {
                            { "Específico", data.Especifico.Conhecimento },
                        },
                        };

                        foreach (var tipoTreinamento in data.TipoTreinamentos)
                        {
                            var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                            linha.Auditoria.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Conhecimento);
                        }

                        linha.Treinamento = new LinhaGraficoFarol.Farol()
                        {
                            Media = data.Geral.Treinamento,
                            Treinamentos = new Dictionary<string, double>()
                        {
                            { "Específico", data.Especifico.Treinamento },
                        },
                        };

                        foreach (var tipoTreinamento in data.TipoTreinamentos)
                        {
                            var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                            linha.Treinamento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Treinamento);
                        }

                        model.Areas.Add(linha);
                    }
                }

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
