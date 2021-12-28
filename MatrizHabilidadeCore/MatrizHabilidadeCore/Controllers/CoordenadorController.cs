using MatrizHabilidade.Services;
using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MatrizHabilidadeCore.Controllers
{
    public class CoordenadorController : BaseController
    {
        private readonly HistoricoCalculatorService _historicoCalculatorService;

        public CoordenadorController(DataBaseContext _db, CookieService cookieService) : base(_db, cookieService)
        {
        }
        public ActionResult Index(string planta, string area)
        {
            var path = HttpContext.Request.Path;
            var query = HttpContext.Request.QueryString;

            if (!int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!int.TryParse(Encrypting.Decrypt(area), out int areaId))
            {
                return RedirectToAction("Index", "Home");
            }

            Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();
            Area _area = _db.Areas.Where(p => p.Id == areaId).FirstOrDefault();

            var tiposTreinamento = _db.TiposTreinamentos.Select(t => t.Id).ToList();
            var gapCalculator = new GAPCalculatorService();
            var chartBuilder = new ChartBuilderService(_db, _historicoCalculatorService);
            var historicoCalculator = new HistoricoCalculatorService(_db);

            int ano = GetCurrentYear();

            if (ano == 0)
            {
                ano = _db.AnosFiscais.OrderBy(a => a.Ano).Last().Ano;
            }

            var model = new CoordenadorViewModel()
            {
                PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                PlantaDescricao = _planta.Descricao,
                AreaId = Encrypting.Encrypt(_area.Id.ToString()),
                AreaDescricao = _area.Alias,
                PathAndQuery = $"{path}{query}"
            };

            var coordenadores = _db.HistoricoCoordenadores
                .Where(c => c != null)
                .Where(h => h.Tipo == TipoHistorico.Auditoria || h.Tipo == TipoHistorico.Treinamento)
                .Where(h => h.AreaId == _area.Id)
                .Select(c => c.Coordenador)
                .Distinct()
                .OrderBy(c => c.Nome)
                .ToList();

            coordenadores.AddRange(_db.Coordenadores
                .Where(c => c.Uniorgs.Any())
                .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any(m => m.IsAtivo))
                .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any(m => m.AreaId == areaId)));

            coordenadores = coordenadores.Distinct().ToList();

            #region Auditorias Realizadas
            var aditional = new List<Grafico.Serie>()
            {
                new Grafico.Serie()
                {
                    Tipo = Grafico.Serie.TipoSerie.Spline,
                    Name = "Meta",
                    Color = "#5FBC23",
                    Data = new List<double>() { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 },
                }
            };

            model.Auditorias = new Grafico("GraficoAuditoria")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Series = chartBuilder.BuildCoordenadoresChart(coordenadores, ano, areaId, TipoHistorico.Auditoria, aditional),
            };

            model.AuditoriasExportacao = new Grafico("GraficoAuditoriaExportacao")
            {
                Title = "Auditorias Realizadas",
                Alinhamento = Grafico.AlinhamentoGrafico.Bottom,
                Series = model.Auditorias.Series,
            };
            #endregion

            #region GAP de Profissionais a Serem Treinados
            model.Treinamentos = new Grafico("GraficoTreinamento")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Unit = "%",
                Rotate = true,
                HasLine = true,
                Series = chartBuilder.BuildCoordenadoresChart(coordenadores, ano, areaId, TipoHistorico.Treinamento, null),
            };

            model.TreinamentosExportacao = new Grafico("GraficoTreinamentoExportacao")
            {
                Title = "GAP de Profissionais a Serem Treinados",
                Alinhamento = Grafico.AlinhamentoGrafico.Bottom,
                Unit = "%",
                Rotate = true,
                HasLine = true,
                Series = model.Treinamentos.Series,
            };
            #endregion

            #region GAP de Conhecimento
            model.Conhecimento = new Grafico("GraficoConhecimento")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Unit = "%",
                HasLine = true,
                Series = chartBuilder.BuildCoordenadoresChart(coordenadores, ano, areaId, TipoHistorico.Conhecimento, null),
            };

            model.ConhecimentoExportacao = new Grafico("GraficoConhecimentoExportacao")
            {
                Title = "GAP de Conhecimento",
                Alinhamento = Grafico.AlinhamentoGrafico.Bottom,
                Unit = "%",
                HasLine = true,
                Series = model.Conhecimento.Series,
            };
            #endregion

            #region Redução do GAP de Conhecimento
            model.Reducao = new Grafico("GraficoReducao")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Unit = "%",
                HasLine = true,
                Series = chartBuilder.BuildCoordenadoresChart(coordenadores, ano, areaId, TipoHistorico.Reducao, null),
            };

            model.ReducaoExportacao = new Grafico("GraficoReducaoExportacao")
            {
                Title = "Redução do GAP de Conhecimento",
                Alinhamento = Grafico.AlinhamentoGrafico.Bottom,
                Unit = "%",
                HasLine = true,
                Series = model.Reducao.Series,
            };
            #endregion

            #region Número de Instrutores
            model.Instrutores = new Grafico("GraficoInstrutores")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Series = chartBuilder.BuildCoordenadoresChart(coordenadores, ano, areaId, TipoHistorico.Instrutores, null),
            };

            model.InstrutoresExportacao = new Grafico("GraficoInstrutoresExportacao")
            {
                Title = "Número de Instrutores",
                Alinhamento = Grafico.AlinhamentoGrafico.Bottom,
                Series = model.Instrutores.Series,
            };
            #endregion

            return View(model);
        }
    }
}