using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrizHabilidadeCore.Controllers
{
    public class PlanoAcaoController : BaseController
    {
        public PlanoAcaoController(DataBaseContext _db, CookieService cookieService, UserManager<Usuario> _userManager, SignInManager<Usuario> _signInManager) : base(_db, cookieService, _userManager, _signInManager)
        {
        }
        public enum ReturnUrl
        {
            Planta = 0,
            Area = 1,
            Maquina = 2,
        }

        public ActionResult Index(string returnUrl, string planta, string area, string[] maquinas, string status)
        {
            var path = HttpContext.Request.Path;
            var queryString = HttpContext.Request.QueryString;


            var model = new PlanoAcaoViewModel()
            {
                PlantasKey = planta,
                AreasKey = area,
                MaquinasKey = maquinas,
                PathAndQuery = $"{path}{queryString}"
            };

            List<int> statuses = new List<int>();

            if (!string.IsNullOrEmpty(status))
            {
                statuses = status.Split(',').Select(s => Convert.ToInt32(s)).ToList();
                model.Status = statuses.Select(s => (int)s).ToList();
            }

            int plantaId = 0;
            int areaId = 0;

            Usuario usuario = new Usuario();

            var coordenadorQuery = _db.Coordenadores.Where(u => u.Login.ToLower() == CurrentUser.Email.ToLower());
            var colaboradorQuery = _db.Colaboradores.Where(u => u.Login.ToLower() == CurrentUser.Email.ToLower());

            if (coordenadorQuery.Any())
            {
                usuario = coordenadorQuery.First().Usuarios;
            }
            else if (colaboradorQuery.Any())
            {
                usuario = colaboradorQuery.First().Usuarios;
            }
            else
            {
                SetMessage("O usuário não tem login cadastrado. Contate o setor de RH");

                return RedirectToAction("Index", "Home");
            }

            var dataAtual = DateTime.Now;
            var dataReferencia = new DateTime(dataAtual.Year, dataAtual.Month, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));

            if (!string.IsNullOrEmpty(returnUrl))
            {
                model.ReturnUrl = returnUrl;
            }
            else
            {
                var referer = Request.Headers["Referer"];

                if (!string.IsNullOrEmpty(referer))
                {
                    var uri = new Uri(referer);

                    if (!uri.PathAndQuery.Contains("PlanoAcao"))
                    {
                        model.ReturnUrl = uri.PathAndQuery;
                    }
                }
            }

            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                if (Request.IsMobileDevice())
                {
                    model.ReturnUrl = "/MatrizHabilidade/Formulario";
                }
                else
                {
                    model.ReturnUrl = "/MatrizHabilidade";
                }
            }

            var query = _db.PlanosAcao.AsQueryable();

            if (User.IsInRole(NivelAcesso.Funcionario.ToString("g")))
            {
                var colaborador = Colaborador;

                query = query.Where(p => p.Colaborador.Uniorg.Maquinas.Select(m => m.Id).Intersect(colaborador.Uniorg.Maquinas.Select(m => m.Id)).Any());

                model.SiteMap.Add(colaborador.Uniorg.Coordenador.Area.Planta.Descricao);
                model.SiteMap.Add(colaborador.Uniorg.Coordenador.Area.Alias);
                model.SiteMap.Add(colaborador.Uniorg.Maquinas.FirstOrDefault().Descricao);
            }
            else
            {
                if (int.TryParse(Encrypting.Decrypt(planta), out plantaId))
                {
                    query = query.Where(p => p.Colaborador.Uniorg.Coordenador.Area.PlantaId == plantaId);

                    model.SiteMap.Add(_db.Plantas.Find(plantaId).Descricao);
                }

                if (int.TryParse(Encrypting.Decrypt(area), out areaId))
                {
                    query = query.Where(p => p.Colaborador.Uniorg.Coordenador.AreaId == areaId);

                    model.SiteMap.Add(_db.Areas.Find(areaId).Alias);
                }

                if (maquinas != null)
                {
                    List<int> filtroIdMaquinas = new List<int>();
                    List<string> filtroDescricaoMaquinas = new List<string>();

                    foreach (var maquina in maquinas)
                    {
                        if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
                        {
                            filtroIdMaquinas.Add(maquinaId);

                            var _maquina = _db.Maquinas.Find(maquinaId);

                            if (!filtroDescricaoMaquinas.Any(f => f == _maquina.Descricao))
                            {
                                filtroDescricaoMaquinas.Add(_maquina.Descricao);
                            }
                        }
                    }

                    query = query.Where(p => p.Colaborador.Uniorg.Maquinas.Any(m => filtroIdMaquinas.Contains(m.Id)));

                    model.SiteMap.Add(string.Join(", ", filtroDescricaoMaquinas));
                }
            }

            var dataInicial = new DateTime(CurrentYear - 1, 04, 01);
            var dataFinal = new DateTime(CurrentYear, 03, DateTime.DaysInMonth(CurrentYear, 03));

            query = query.Where(q => q.DataCriacao >= dataInicial && q.DataCriacao <= dataFinal);

            var acoes = query.ToList();

            #region Tabela Status Atividade
            model.TabelaStatusAtividade = new PlanoAcaoViewModel.StatusAtividades
            {
                Total = acoes.Count(),
                Concluida = acoes.Where(a => a.DataConclusao.HasValue && a.DataConclusao.Value <= a.Prazo).Count(),
                ConcluidaAtraso = acoes.Where(a => a.DataConclusao.HasValue && a.DataConclusao.Value > a.Prazo).Count(),
                Andamento = acoes.Where(a => !a.DataConclusao.HasValue && a.Prazo > dataAtual).Count(),
                Atrasada = acoes.Where(a => !a.DataConclusao.HasValue && a.Prazo < dataAtual).Count(),
            };
            #endregion

            #region Grafico Acompanhamento
            model.GraficoAcompanhamento = new GraficoPie()
            {
                Container = "GraficoAcompanhamento",
                SeriesName = "Status Atividades",
            };

            model.GraficoAcompanhamento.Series.Add(new GraficoPie.Data()
            {
                Name = "Concluídas",
                Value = model.TabelaStatusAtividade.Concluida,
                Color = "#A1DD00",
            });

            model.GraficoAcompanhamento.Series.Add(new GraficoPie.Data()
            {
                Name = "Concluído com Atraso",
                Value = model.TabelaStatusAtividade.ConcluidaAtraso,
                Color = "#00ADFB",
            });

            model.GraficoAcompanhamento.Series.Add(new GraficoPie.Data()
            {
                Name = "Atrasadas",
                Value = model.TabelaStatusAtividade.Atrasada,
                Color = "#CA2024",
            });

            model.GraficoAcompanhamento.Series.Add(new GraficoPie.Data()
            {
                Name = "Em Andamento",
                Value = model.TabelaStatusAtividade.Andamento,
                Color = "#DF7E26",
            });
            #endregion

            #region GAP de Ação Corretiva
            var gap = (1f - (float)model.TabelaStatusAtividade.Concluida / (float)model.TabelaStatusAtividade.Total) * 100f;

            if (double.IsNaN(gap))
            {
                model.GAPAcaoCorretiva = 0;
            }
            else
            {
                model.GAPAcaoCorretiva = Convert.ToInt32(gap);
            }
            #endregion

            if (statuses.Count > 0)
            {
                acoes = acoes.Where(p => statuses.Contains((int)p.Status)).ToList();
            }

            #region Tabela Plano Acão
            model.TabelaPlanoAcao = new List<PlanoAcaoViewModel.Linha>();

            for (var x = 0; x < acoes.Count; x++)
            {
                var isResponsavel = false;
                var nomeResponsavel = "";

                if (acoes[x].ColaboradorResponsavel != null)
                {
                    isResponsavel = acoes[x].ColaboradorResponsavelId == usuario.Id;
                    nomeResponsavel = acoes[x].ColaboradorResponsavel.Nome;
                }
                else if (acoes[x].CoordenadorResponsavel != null)
                {
                    isResponsavel = acoes[x].CoordenadorResponsavelId == usuario.Id;
                    nomeResponsavel = acoes[x].CoordenadorResponsavel.Nome;
                }

                var statusText = "";
                var statusClass = "";

                var statusAcao = acoes[x].Status;

                if (statusAcao == StatusAcao.Concluido)
                {
                    statusText = "Concluído";
                    statusClass = "concluido";
                }
                else if (statusAcao == StatusAcao.EmAndamento)
                {
                    statusText = "Em Andamento";
                    statusClass = "em-andamento";
                }
                else if (statusAcao == StatusAcao.Atrasado)
                {
                    statusText = "Atrasado";
                    statusClass = "atrasado";
                }
                else if (statusAcao == StatusAcao.ConcluidoAtraso)
                {
                    statusText = "Concluído com Atraso";
                    statusClass = "concluido-atraso";
                }

                var linha = new PlanoAcaoViewModel.Linha()
                {
                    Key = Encrypting.Encrypt(acoes[x].Id.ToString()),
                    Area = acoes[x].Colaborador.Uniorg.Coordenador.Area.Alias,
                    Maquina = acoes[x].Colaborador.Uniorg.Maquinas.FirstOrDefault().Descricao,
                    Coordenador = acoes[x].Colaborador.Uniorg.Coordenador.Nome,
                    Index = x + 1,
                    Acao = acoes[x].Acao,
                    Responsavel = nomeResponsavel,
                    Prazo = acoes[x].Prazo.ToString("dd/MM/yyyy"),
                    DataCriacao = acoes[x].DataCriacao.ToString("dd/MM/yyyy"),
                    Status = statusText,
                    StatusClass = statusClass,
                    HasEvidencia = acoes[x].DataConclusao.HasValue,
                    IsResponsavel = isResponsavel,
                };

                model.TabelaPlanoAcao.Add(linha);
            }
            #endregion

            #region Filtros
            foreach (var _planta in _db.Plantas.OrderBy(p => p.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_planta.Id.ToString());

                model.Plantas.Value.Options.Add(key, _planta.Descricao);
            }

            model.Plantas.Value.SelectedKey = planta;

            IQueryable<Area> areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any());

            IQueryable<Maquina> _maquinas = _db.Maquinas.Where(m => m.IsAtivo);

            if (plantaId > 0)
            {
                areas = areas.Where(a => a.PlantaId == plantaId);
                _maquinas = _maquinas.Where(m => m.Uniorgs.FirstOrDefault().Coordenador.Area.PlantaId == plantaId);
            }

            if (areaId > 0)
            {
                _maquinas = _maquinas.Where(m => m.AreaId == areaId);
            }

            foreach (var _area in areas.OrderBy(a => a.Alias).ToList())
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Value.Options.Add(key, _area.Alias);
            }

            model.Areas.Value.SelectedKey = area;

            foreach (var _maquina in _maquinas.OrderBy(m => m.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Value.Options.Add(key, _maquina.Descricao);
            }
            #endregion

            return View(model);
        }

        public ActionResult FiltrarPlanoAcao(string planta, string area, string maquina)
        {
            var model = new FiltroPlanoAcaoViewModel();

            int.TryParse(Encrypting.Decrypt(planta), out int plantaId);
            int.TryParse(Encrypting.Decrypt(area), out int areaId);

            IQueryable<Area> areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any());

            IQueryable<Maquina> maquinas = _db.Maquinas.Where(m => m.IsAtivo);

            if (plantaId > 0)
            {
                areas = areas.Where(a => a.PlantaId == plantaId);
                maquinas = maquinas.Where(m => m.Uniorgs.FirstOrDefault().Coordenador.Area.PlantaId == plantaId);
            }

            if (areaId > 0)
            {
                maquinas = maquinas.Where(m => m.AreaId == areaId);
            }

            foreach (var _area in areas.OrderBy(a => a.Alias).ToList())
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Value.Options.Add(key, _area.Alias);
            }

            model.Areas.Value.SelectedKey = area;

            foreach (var _maquina in maquinas.OrderBy(m => m.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Value.Options.Add(key, _maquina.Descricao);
            }

            model.Maquinas.Value.SelectedKey = maquina;

            return Json(model);
        }

        public ActionResult FinalizarAcao(string plano)
        {
            if (int.TryParse(Encrypting.Decrypt(plano), out int planoId))
            {
                var _plano = _db.PlanosAcao.Find(planoId);

                _plano.DataConclusao = DateTime.Now;

                _db.SaveChanges();

                return StatusCode(200);
            }

            return StatusCode(400); ;
        }
    }
}
