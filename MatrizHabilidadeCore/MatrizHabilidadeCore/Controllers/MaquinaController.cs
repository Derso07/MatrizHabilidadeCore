using MatrizHabilidade.Services;
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
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class MaquinaController : BaseController
    {
        private readonly HistoricoCalculatorService _historicoCalculatorService;

        public MaquinaController(DataBaseContext _db, UserManager<Usuario> userManager, CookieService cookieService) : base(_db, userManager, cookieService)
        {
        }
        public ActionResult Index(string planta, string area, string maquina)
        {
            if (!int.TryParse(Encrypting.Decrypt(planta), out int planta_id))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!int.TryParse(Encrypting.Decrypt(area), out int area_id))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!int.TryParse(Encrypting.Decrypt(maquina), out int maquina_id))
            {
                return RedirectToAction("Index", "Home");
            }

            Planta _planta = _db.Plantas.Where(p => p.Id == planta_id).FirstOrDefault();
            Area _area = _db.Areas.Where(p => p.Id == area_id).FirstOrDefault();

            var tiposTreinamento = _db.TiposTreinamentos.Select(t => t.Id).ToList();
            var gapCalculator = new GAPCalculatorService();
            var chartBuilder = new ChartBuilderService(_db, _historicoCalculatorService);
            var historicoCalculator = new HistoricoCalculatorService(_db);

            Maquina _maquina = _db.Maquinas.Where(m => m.Id == maquina_id).FirstOrDefault();

            int ano = CurrentYear.Ano;
            var currentDate = DateTime.Now;

            var model = new MaquinaViewModel()
            {
                PlantaId = Encrypting.Encrypt(_planta.Id.ToString()),
                PlantaDescricao = _planta.Descricao,
                AreaId = Encrypting.Encrypt(_area.Id.ToString()),
                AreaDescricao = _area.Alias,
                MaquinaId = Encrypting.Encrypt(_maquina.Id.ToString()),
                MaquinaDescricao = _maquina.Descricao,
            };

            #region Auditorias Realizadas
            model.Auditorias = new Grafico("GraficoAuditoria")
            {
                Alinhamento = Grafico.AlinhamentoGrafico.Right,
                Series = chartBuilder.BuildMaquinasChart(_maquina, ano, TipoHistorico.Auditoria, null),
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
                Series = chartBuilder.BuildMaquinasChart(_maquina, ano, TipoHistorico.Treinamento, null),
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
                Series = chartBuilder.BuildMaquinasChart(_maquina, ano, TipoHistorico.Conhecimento, null),
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
                Series = chartBuilder.BuildMaquinasChart(_maquina, ano, TipoHistorico.Reducao, null),
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
                Series = chartBuilder.BuildMaquinasChart(_maquina, ano, TipoHistorico.Instrutores,null),
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
