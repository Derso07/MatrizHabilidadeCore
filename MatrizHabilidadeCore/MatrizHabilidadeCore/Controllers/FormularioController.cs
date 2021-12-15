using MatrizHabilidade.Services;
using MatrizHabilidade.ViewModel;
using MatrizHabilidade.ViewModel.Formulario;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class FormularioController : BaseController
    {

        public FormularioController(DataBaseContext _db, UserManager<Usuario> userManager, CookieService cookieService) : base(_db, userManager, cookieService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }

        #region Auditoria
        public ActionResult CadastroAuditoria()
        {

            var model = new AuditoriaViewModel()
            {
                UserName = CurrentUser.Nome,
                Email = CurrentUser.Email,
                FiltroPlantaPlanoAcao = new Dictionary<string, string>(),
                FiltroAreaPlanoAcao = new Dictionary<string, string>(),
                ResponsavelPlanoAcao = new Dictionary<string, string>(),
            };

            if (string.IsNullOrEmpty(CurrentUser.Email))
            {
                SetMessage("O usuário não tem login cadastrado. Contate o setor de RH");

                return RedirectToAction("Index", "Home");
            }

            foreach (var planta in _db.Plantas.OrderBy(p => p.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.FiltroPlantaPlanoAcao.Add(key, planta.Descricao);
            }

            foreach (var area in _db.Areas.OrderBy(a => a.Alias).ToList())
            {
                var key = Encrypting.Encrypt(area.Id.ToString());

                model.FiltroAreaPlanoAcao.Add(key, area.Alias);
            }

            foreach (var responsavel in _db.Coordenadores.OrderBy(c => c.Usuario.Nome).ToList())
            {
                var key = Encrypting.Encrypt(responsavel.Usuario.Id.ToString());

                model.ResponsavelPlanoAcao.Add("coordenador_" + key, responsavel.Usuario.Nome);
            }

            foreach (var _colaborador in _db.Colaboradores.Where(c => c.IsFacilitador).ToList())
            {
                var key = "facilitador_" + Encrypting.Encrypt(_colaborador.Usuario.Id.ToString());

                model.ResponsavelPlanoAcao.Add(key, _colaborador.Usuario.Nome);
            }

            model.ResponsavelPlanoAcao = model.ResponsavelPlanoAcao.OrderBy(r => r.Value).ToDictionary(r => r.Key, r => r.Value);

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(a => a.Alias)
                .ToList();

            if (CurrentUser.UsuarioAcesso <= (int)NivelAcesso.Administrador)
            {
                foreach (var area in areas)
                {
                    var key = Encrypting.Encrypt(area.Id.ToString());

                    model.Areas.Add(key, area.Alias);
                }

                var maquinas = _db.Maquinas
                    .Where(m => m.IsAtivo)
                    .Where(m => m.Uniorgs.Any())
                    .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                    .OrderBy(m => m.Descricao)
                    .ToList();

                foreach (var maquina in maquinas)
                {
                    var key = Encrypting.Encrypt(maquina.Id.ToString());

                    model.Maquinas.Add(key, maquina.Descricao);
                }

                foreach (var padrao in _db.TreinamentosEspecificos.Where(t => t.IsAtivo).OrderBy(t => t.Descricao).ToList())
                {
                    var key = Encrypting.Encrypt(padrao.Id.ToString());

                    model.Padroes.Add(key, padrao.Descricao);
                }
            }
            else
            {
                var coordenador = _db.Coordenadores.Where(u => u.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();

                if (coordenador == null)
                {
                    SetMessage("O usuário não tem login cadastrado. Contate o setor de RH");

                    return RedirectToAction("Index", "Home");
                }

                var maquinas = coordenador.Uniorgs
                    .SelectMany(u => u.Maquinas)
                    .Where(m => m.IsAtivo)
                    .Where(m => m.Uniorgs.Any())
                    .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                    .OrderBy(m => m.Descricao)
                    .ToList();

                foreach (var maquina in maquinas)
                {
                    var key = Encrypting.Encrypt(maquina.Id.ToString());

                    if (!model.Maquinas.ContainsKey(key))
                    {
                        model.Maquinas.Add(key, maquina.Descricao);
                    }
                }

                var padroes = _db.TreinamentosEspecificos
                    .Where(t => t.IsAtivo)
                    .Where(t => t.Maquinas.Any(m => m.Uniorgs.Any(u => u.CoordenadorId == coordenador.Usuario.Id)))
                    .OrderBy(t => t.Descricao)
                    .ToList();

                foreach (var padrao in padroes)
                {
                    var key = Encrypting.Encrypt(padrao.Id.ToString());

                    model.Padroes.Add(key, padrao.Descricao);
                }
            }

            foreach (var pergunta in _db.Perguntas.ToList())
            {
                var key = Encrypting.Encrypt(pergunta.Id.ToString());

                model.Perguntas.Add(key, pergunta.Descricao);
            }

            return View(model);
        }

        public ActionResult ChecarMetaAuditoria(CadastroAuditoriaViewModel model)
        {
            if (int.TryParse(Encrypting.Decrypt(model.Operador), out int operadorId))
            {
                if (int.TryParse(Encrypting.Decrypt(model.Padrao), out int padraoId))
                {
                    float nota = 0;
                    float total = 0;

                    foreach (var pergunta in model.Perguntas)
                    {
                        if (int.TryParse(Encrypting.Decrypt(pergunta.Key), out int perguntaId))
                        {
                            var _pergunta = _db.Perguntas.Find(perguntaId);

                            if (pergunta.Value)
                            {
                                nota += _pergunta.Peso;
                            }

                            total += _pergunta.Peso;
                        }
                    }

                    nota = nota / total * 100;

                    if (nota >= 90)
                    {
                        nota = 4;
                    }
                    else if (nota >= 80)
                    {
                        nota = 3;
                    }
                    else if (nota >= 80)
                    {
                        nota = 2;
                    }
                    else
                    {
                        nota = 1;
                    }

                    int? meta = null;

                    var metaQuery = _db.MetasTreinamentosEspecificos.Where(m => m.ColaboradorId == operadorId && m.TreinamentoEspecificoId == padraoId);

                    if (metaQuery.Any())
                    {
                        meta = metaQuery.OrderByDescending(m => m.DataLancamento).First().Meta;
                    }

                    var response = new AuditoriaResponse()
                    {
                        Nota = Convert.ToInt32(nota),
                        Meta = meta ?? 0,
                        IsOK = !meta.HasValue || nota >= meta,
                    };

                    return Json(response);
                }
            }

            _db.Erros.Add(new Error($"Erro ao checar a auditoria. Operador: {model.Operador}; Padrao: {model.Padrao}"));
            _db.SaveChanges();

            return StatusCode(400);
        }

        public async Task<IActionResult> CadastrarAuditoria(CadastroAuditoriaViewModel model)
        {
            if (int.TryParse(Encrypting.Decrypt(model.Operador), out int operadorId))
            {
                if (int.TryParse(Encrypting.Decrypt(model.Padrao), out int padraoId))
                {
                    var auditoria = new Auditoria()
                    {
                        ColaboradorId = operadorId,
                        DataLancamento = DateTime.Now,
                        TreinamentoEspecificoId = padraoId,
                        DataInicio = model.DataInicio.Date,
                    };

                    _db.Auditorias.Add(auditoria);

                    _db.SaveChanges();

                    float nota = 0;
                    float total = 0;

                    foreach (var pergunta in model.Perguntas)
                    {
                        if (int.TryParse(Encrypting.Decrypt(pergunta.Key), out int perguntaId))
                        {
                            var _pergunta = _db.Perguntas.Find(perguntaId);

                            if (pergunta.Value)
                            {
                                nota += _pergunta.Peso;
                            }

                            total += _pergunta.Peso;

                            _db.PerguntasAuditorias.Add(new PerguntaAuditoria()
                            {
                                AuditoriaId = auditoria.Id,
                                IsOK = pergunta.Value,
                                PerguntaId = _pergunta.Id,
                            });

                            _db.SaveChanges();
                        }
                    }

                    nota = nota / total * 100.0f;

                    if (nota >= 90)
                    {
                        nota = 4;
                    }
                    else if (nota >= 80)
                    {
                        nota = 3;
                    }
                    else if (nota >= 80)
                    {
                        nota = 2;
                    }
                    else
                    {
                        nota = 1;
                    }

                    auditoria.Nota = nota;

                    _db.Entry(auditoria).State = EntityState.Modified;
                    _db.SaveChanges();

                    if (model.Acoes != null)
                    {
                        foreach (var acao in model.Acoes)
                        {
                            acao.IsCoordenador = acao.ResponsavelPlanoAcao.Contains("coordenador");

                            acao.ResponsavelPlanoAcao = acao.ResponsavelPlanoAcao.Split('_')[1];

                            if (int.TryParse(Encrypting.Decrypt(acao.ResponsavelPlanoAcao), out int responsavelId))
                            {
                                var criador = _db.Coordenadores.Where(c => c.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();
                                var operador = _db.Colaboradores.Find(operadorId);
                                var requisitante = _db.Coordenadores.Where(c => c.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();

                                var planoAcao = new PlanoAcao()
                                {
                                    Acao = acao.Descricao,
                                    ColaboradorId = operadorId,
                                    CriadorId = criador.Usuario.Id,
                                    DataCriacao = DateTime.Now,
                                    Prazo = acao.Prazo,
                                    TreinamentoEspecificoId = padraoId,
                                };

                                var email = new EmailPlanoAcao()
                                {
                                    Acao = acao.Descricao,
                                    Prazo = acao.Prazo.ToString("dd/MM/yyyy"),
                                    Url = "https://novelis.gestao.annimar.com.br/MatrizHabilidade/PlanoAcao",
                                };

                                if (acao.IsCoordenador)
                                {
                                    planoAcao.CoordenadorResponsavelId = responsavelId;
                                    var responsavel = _db.Coordenadores.Find(responsavelId);
                                    email.NomeColaborador = responsavel.Usuario.Nome;
                                    email.EmailColaborador = responsavel.Usuario.Email;
                                }
                                else
                                {
                                    planoAcao.ColaboradorResponsavelId = responsavelId;
                                    var responsavel = _db.Colaboradores.Find(responsavelId);
                                    email.NomeColaborador = responsavel.Usuario.Nome;
                                    email.EmailColaborador = responsavel.Usuario.Email;
                                }

                                _db.PlanosAcao.Add(planoAcao);
                                _db.SaveChanges();

                                email.AreaRequisitante = requisitante.Area.Descricao;
                                email.NomeRequisitante = requisitante.Usuario.Nome;
                                email.MaquinaRequisitante = requisitante.Uniorgs.SelectMany(u => u.Maquinas).FirstOrDefault().Descricao;

                                var mailBody = await this.RenderViewAsync("~/Views/Shared/EmailPlanoAcao.cshtml", email);

                                EmailSender.Send("NSA Treinamento", operador.Usuario.Email, operador.Usuario.Nome, "Pilar E&T - Ação corretiva", mailBody);
                                EmailSender.Send("NSA Treinamento", "miyagawa.emanuel@gmail.com", "Emanuel", "Pilar E&T - Ação corretiva", mailBody);
                            }
                        }
                    }

                    return StatusCode(200);
                }
            }

            return StatusCode(400);
        }

        public ActionResult RetreinarColaborador(string treinamento, string colaborador)
        {
            if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoEspecificoId))
            {
                if (int.TryParse(Encrypting.Decrypt(colaborador), out int colaboradorId))
                {
                    var auditorias = _db.Auditorias.Where(a => a.TreinamentoEspecificoId == treinamentoEspecificoId && a.ColaboradorId == colaboradorId);
                    var coordenador = _db.Coordenadores.Where(u => u.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();

                    _db.Retreinamentos.Add(new Retreinamento()
                    {
                        ColaboradorId = colaboradorId,
                        CoordenadorId = coordenador.Usuario.Id,
                        TreinamentoEspecificoId = treinamentoEspecificoId,
                        Date = DateTime.Now,
                    });

                    _db.SaveChanges();

                    foreach (var auditoria in auditorias.ToList())
                    {
                        auditoria.IsAtivo = false;

                        _db.Entry(auditoria).State = EntityState.Modified;

                        _db.SaveChanges();
                    }

                    var turmas = _db.TurmasTreinamentosEspecificosColaboradores.Where(t => t.TurmaTreinamentoEspecifico.TreinamentoEspecificoId == treinamentoEspecificoId && t.ColaboradorId == colaboradorId);

                    foreach (var turma in turmas.ToList())
                    {
                        turma.IsAtivo = false;

                        _db.Entry(turma).State = EntityState.Modified;

                        _db.SaveChanges();
                    }
                }
            }

            return StatusCode(200);
        }

        public ActionResult FiltrarAreaAuditoria(string area)
        {
            var model = new AuditoriaViewModel();

            if (int.TryParse(Encrypting.Decrypt(area), out int areaId))
            {
                var _area = _db.Areas.Find(areaId);

                var maquinas = _area.Coordenadores
                    .SelectMany(c => c.Uniorgs)
                    .SelectMany(u => u.Maquinas)
                    .Where(m => m.IsAtivo)
                    .OrderBy(m => m.Descricao)
                    .ToList();

                foreach (var maquina in maquinas)
                {
                    var key = Encrypting.Encrypt(maquina.Id.ToString());

                    if (!model.Maquinas.ContainsKey(key))
                    {
                        model.Maquinas.Add(key, maquina.Descricao);
                    }
                }

                foreach (var padrao in _db.TreinamentosEspecificos.Where(t => t.IsAtivo).Where(t => t.Maquinas.Any(m => m.AreaId == _area.Id)).ToList())
                {
                    var key = Encrypting.Encrypt(padrao.Id.ToString());

                    model.Padroes.Add(key, padrao.Descricao);
                }
            }

            return Json(model);
        }

        public ActionResult FiltrarMaquinaAuditoria(string maquina)
        {
            var model = new AuditoriaViewModel();

            if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
            {
                var _maquina = _db.Maquinas.Find(maquinaId);

                foreach (var padrao in _db.TreinamentosEspecificos.Where(t => t.IsAtivo).Where(t => t.Maquinas.Any(m => m.Id == _maquina.Id)).ToList())
                {
                    var key = Encrypting.Encrypt(padrao.Id.ToString());

                    model.Padroes.Add(key, padrao.Descricao);
                }
            }

            return Json(model);
        }

        public ActionResult BuscarOperadores(string area, string maquina, string padrao, string operador)
        {
            int? areaId = null;
            int? maquinaId = null;
            int aux = 0;

            if (int.TryParse(Encrypting.Decrypt(padrao), out int treinamentoEspecificoId))
            {
                var coordenador = _db.Coordenadores.Where(u => u.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();

                if (int.TryParse(Encrypting.Decrypt(area), out aux))
                {
                    areaId = aux;
                }

                if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
                {
                    maquinaId = aux;
                }

                var query = _db.Colaboradores
                    .Where(c => c.Usuario.IsAtivo)
                    .AsQueryable();

                if (areaId.HasValue)
                {
                    query = query.Where(c => c.Uniorg.Coordenador.AreaId == areaId.Value);
                }

                if (maquinaId.HasValue)
                {
                    query = query.Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId.Value));
                }

                if (!string.IsNullOrEmpty(operador))
                {
                    query = query.Where(c => c.Usuario.Nome.ToLower().Contains(operador.ToLower()) || c.Usuario.Chapa.ToLower().Contains(operador.ToLower()));
                }

                if (CurrentUser.UsuarioAcesso > (int)NivelAcesso.Administrador)
                {
                    query = query.Where(c => c.Uniorg.CoordenadorId == coordenador.Usuario.Id);
                }

                var model = new List<OperadorViewModel>();

                foreach (var _operador in query.OrderBy(c => c.Usuario.Nome).ToList())
                {
                    var key = Encrypting.Encrypt(_operador.Usuario.Id.ToString());

                    OperadorViewModel.Farol status;

                    var colaboradorHasTreinamento = _db.TurmasTreinamentosEspecificos
                        .Where(t => t.TreinamentoEspecificoId == treinamentoEspecificoId)
                        .Where(t => t.TurmaColaboradores.Any(c => c.ColaboradorId == _operador.Usuario.Id))
                        .Any();

                    var metaQuery = _operador.MetasAuditoria
                        .Where(m => m.TreinamentoEspecificoId == treinamentoEspecificoId)
                        .OrderByDescending(m => m.DataLancamento);

                    if (!metaQuery.Any())
                    {
                        continue;
                    }

                    var meta = metaQuery.First().Meta;

                    if (!meta.HasValue)
                    {
                        continue;
                    }
                    else if (meta == 1)
                    {
                        continue;
                    }

                    if (!colaboradorHasTreinamento)
                    {
                        status = OperadorViewModel.Farol.Disabled;
                    }
                    else if (!_operador.Auditorias.Any(a => a.TreinamentoEspecificoId == treinamentoEspecificoId && a.IsAtivo))
                    {
                        status = OperadorViewModel.Farol.Vermelho;
                    }
                    else
                    {
                        var nota = _operador.Auditorias
                            .Where(a => a.TreinamentoEspecificoId == treinamentoEspecificoId && a.IsAtivo)
                            .OrderByDescending(m => m.DataLancamento)
                            .First()
                            .Nota;

                        if (nota >= meta)
                        {
                            status = OperadorViewModel.Farol.Verde;
                        }
                        else
                        {
                            status = OperadorViewModel.Farol.Amarelo;
                        }
                    }

                    model.Add(new OperadorViewModel()
                    {
                        Key = key,
                        Nome = $"{_operador.Usuario.Chapa} - {_operador.Usuario.Nome}",
                        Status = status,
                    });
                }

                return Json(model);
            }

            return StatusCode(400);
        }

        public ActionResult FiltrarAuditoria(string area, string maquina, string padrao)
        {
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(area), out int aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var model = new AuditoriaViewModel();

            var treinamentos = _db.TreinamentosEspecificos.Where(t => t.IsAtivo).Where(t => t.Descricao.Contains(padrao));

            if (areaId.HasValue)
            {
                treinamentos = treinamentos.Where(t => t.Maquinas.Any(m => m.AreaId == areaId.Value));
            }

            if (maquinaId.HasValue)
            {
                treinamentos = treinamentos.Where(t => t.Maquinas.Any(m => m.Id == maquinaId.Value));
            }

            if (CurrentUser.UsuarioAcesso > (int)NivelAcesso.Administrador)
            {
                var coordenador = _db.Coordenadores.Where(u => u.Usuario.Login.ToLower() == CurrentUser.Email.ToLower()).FirstOrDefault();

                treinamentos = treinamentos.Where(t => t.Maquinas.Any(m => m.Uniorgs.Any(u => u.CoordenadorId == coordenador.Usuario.Id)));
            }

            foreach (var treinamentoEspecifico in treinamentos.ToList())
            {
                var key = Encrypting.Encrypt(treinamentoEspecifico.Id.ToString());

                model.Padroes.Add(key, treinamentoEspecifico.Descricao);
            }

            return Json(model);
        }

        public ActionResult FiltrarResponsavelPlanoAcao(string planta, string area)
        {
            var model = new CadastroPlanoAcaoViewModel()
            {
                Responsaveis = new Dictionary<string, CadastroPlanoAcaoViewModel.Option>(),
            };

            int? plantaId = null;
            int? areaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            model.Areas = new Dictionary<string, CadastroPlanoAcaoViewModel.Option>
            {
                { "", new CadastroPlanoAcaoViewModel.Option("Todas") }
            };

            var areaQuery = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any());

            if (plantaId.HasValue)
            {
                areaQuery = areaQuery.Where(a => a.PlantaId == plantaId.Value);
            }

            foreach (var _area in areaQuery.OrderBy(c => c.Alias).ToList())
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                if (!model.Areas.ContainsKey(key))
                {
                    model.Areas.Add(key, new CadastroPlanoAcaoViewModel.Option(_area.Alias)
                    {
                        IsSelected = area == key,
                        Text = _area.Alias,
                    });
                }
            }

            var coordenadorQuery = _db.Coordenadores.Where(c => c.Usuario.IsAtivo);

            if (plantaId.HasValue)
            {
                coordenadorQuery = coordenadorQuery.Where(c => c.Area.PlantaId == plantaId.Value);
            }

            if (areaId.HasValue)
            {
                coordenadorQuery = coordenadorQuery.Where(c => c.AreaId == areaId.Value);
            }

            foreach (var _coordenador in coordenadorQuery.ToList())
            {
                var key = "coordenador_" + Encrypting.Encrypt(_coordenador.Usuario.Id.ToString());

                if (!model.Responsaveis.ContainsKey(key))
                {
                    model.Responsaveis.Add(key, new CadastroPlanoAcaoViewModel.Option()
                    {
                        Text = _coordenador.Usuario.Nome,
                    });
                }
            }

            var colaboradorQuery = _db.Colaboradores.Where(c => c.IsFacilitador).Where(c => c.Usuario.IsAtivo);

            if (plantaId.HasValue)
            {
                colaboradorQuery = colaboradorQuery.Where(c => c.Uniorg.Coordenador.Area.PlantaId == plantaId.Value);
            }

            if (areaId.HasValue)
            {
                colaboradorQuery = colaboradorQuery.Where(c => c.Uniorg.Coordenador.AreaId == areaId.Value);
            }

            foreach (var _colaborador in colaboradorQuery.ToList())
            {
                var key = "facilitador_" + Encrypting.Encrypt(_colaborador.Usuario.Id.ToString());

                if (!model.Responsaveis.ContainsKey(key))
                {
                    model.Responsaveis.Add(key, new CadastroPlanoAcaoViewModel.Option()
                    {
                        Text = _colaborador.Usuario.Nome
                    });
                }
            }

            model.Responsaveis = model.Responsaveis.OrderBy(r => r.Value.Text).ToDictionary(o => o.Key, o => o.Value);

            model.Responsaveis = model.Responsaveis.Prepend(new KeyValuePair<string, CadastroPlanoAcaoViewModel.Option>("", new CadastroPlanoAcaoViewModel.Option()
            {
                Text = "Todos",
            })).ToDictionary(o => o.Key, o => o.Value);

            return Json(model);
        }
        #endregion

        #region Meta
        [HttpGet]
        public ActionResult Meta()
        {
            var model = new MetaViewModel();

            foreach (var _planta in _db.Plantas.ToList())
            {
                var key = Encrypting.Encrypt(_planta.Id.ToString());

                model.Plantas.Add(new Option()
                {
                    Key = key,
                    Text = _planta.Descricao,
                });
            }

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(a => a.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Add(new Option()
                {
                    Key = key,
                    Text = _area.Alias,
                });
            }

            var maquinas = _db.Maquinas
                .Where(m => m.IsAtivo)
                .Where(m => m.Uniorgs.Any())
                .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                .OrderBy(m => m.Descricao)
                .ToList();

            foreach (var _maquina in maquinas)
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Add(new Option()
                {
                    Key = key,
                    Text = _maquina.Descricao,
                });
            }

            foreach (var _padrao in _db.TreinamentosEspecificos.OrderBy(t => t.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_padrao.Id.ToString());

                model.Padroes.Add(new Option()
                {
                    Key = key,
                    Text = _padrao.Descricao,
                });
            }

            foreach (var _profissional in _db.Colaboradores.Where(c => c.Usuario.IsAtivo).Where(c => c.Usuario.Situacao == Situacao.Normal).OrderBy(c => c.Usuario.Nome).ToList())
            {
                var key = Encrypting.Encrypt(_profissional.Usuario.Id.ToString());

                model.Profissionais.Add(new Option()
                {
                    Key = key,
                    Text = $"{_profissional.Usuario.Chapa} - {_profissional.Usuario.Nome}",
                });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Meta(CadastroMetaViewModel model)
        {
            if (int.TryParse(Encrypting.Decrypt(model.Colaborador), out int colaboradorId))
            {
                if (int.TryParse(Encrypting.Decrypt(model.Treinamento), out int treinamentoEspecificoId))
                {
                    int? meta = null;

                    if (int.TryParse(model.Meta, out int aux))
                    {
                        meta = aux;
                    }

                    _db.MetasTreinamentosEspecificos.Add(new MetaTreinamentoEspecifico()
                    {
                        ColaboradorId = colaboradorId,
                        TreinamentoEspecificoId = treinamentoEspecificoId,
                        Meta = meta,
                        DataLancamento = DateTime.Now,
                    });

                    _db.SaveChanges();
                }
            }

            return StatusCode(200);
        }

        public ActionResult FiltrarMetas(string planta, string area, string maquina)
        {

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var model = new MetaViewModel();

            foreach (var _planta in _db.Plantas.ToList())
            {
                var key = Encrypting.Encrypt(_planta.Id.ToString());

                model.Plantas.Add(new Option()
                {
                    Key = key,
                    Text = _planta.Descricao,
                    IsSelected = key == planta,
                });
            }

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any());

            if (plantaId.HasValue)
            {
                areas = areas.Where(a => a.PlantaId == plantaId.Value);
            }

            foreach (var _area in areas.OrderBy(a => a.Alias).ToList())
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Add(new Option()
                {
                    Key = key,
                    Text = _area.Alias,
                    IsSelected = key == area,
                });
            }

            var maquinas = _db.Maquinas
                .Where(m => m.IsAtivo)
                .Where(m => m.Uniorgs.Any())
                .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any());

            if (areaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value));
            }

            foreach (var _maquina in maquinas.OrderBy(m => m.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Add(new Option()
                {
                    Key = key,
                    Text = _maquina.Descricao,
                    IsSelected = key == maquina,
                });
            }

            var _padroes = _db.TreinamentosEspecificos.AsQueryable();

            if (maquinaId.HasValue)
            {
                _padroes = _padroes.Where(p => p.Maquinas.Any(m => m.Id == maquinaId.Value));
            }
            else if (areaId.HasValue)
            {
                _padroes = _padroes.Where(p => p.Maquinas.Any(m => m.AreaId == areaId.Value));
            }
            else if (plantaId.HasValue)
            {
                _padroes = _padroes.Where(p => p.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
            }

            foreach (var _padrao in _padroes.OrderBy(p => p.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_padrao.Id.ToString());

                model.Padroes.Add(new Option()
                {
                    Key = key,
                    Text = _padrao.Descricao,
                });
            }

            var _profissionais = _db.Colaboradores.Where(c => c.Usuario.IsAtivo).Where(c => c.Usuario.Situacao == Situacao.Normal);

            if (maquinaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Maquinas.Any(m => m.Id == maquinaId.Value));
            }
            else if (areaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Coordenador.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Coordenador.Area.PlantaId == plantaId.Value);
            }

            foreach (var _profissional in _profissionais.OrderBy(p => p.Usuario.Nome).ToList())
            {
                var key = Encrypting.Encrypt(_profissional.Usuario.Id.ToString());

                model.Profissionais.Add(new Option()
                {
                    Key = key,
                    Text = $"{_profissional.Usuario.Chapa} - {_profissional.Usuario.Nome}",
                });
            }

            return Json(model);
        }

        public ActionResult BuscarTabelaMetas(string planta, string area, string maquina, string[] padroes, string[] profissionais)
        {

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var model = new List<MetaViewModel.Row>();

            var colaboradores = _db.Colaboradores.Where(c => c.Usuario.IsAtivo && c.Usuario.Situacao == Situacao.Normal);

            if (profissionais?.Length > 0)
            {
                var colaboradoresSelecionados = new List<int>();

                foreach (var profissional in profissionais)
                {
                    if (int.TryParse(Encrypting.Decrypt(profissional), out int colaboradorId))
                    {
                        colaboradoresSelecionados.Add(colaboradorId);
                    }
                }

                colaboradores = colaboradores.Where(t => colaboradoresSelecionados.Contains(t.Usuario.Id));
            }
            else if (maquinaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId.Value));
            }
            else if (areaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.Area.PlantaId == plantaId.Value);
            }

            foreach (var colaborador in colaboradores.OrderBy(c => c.Usuario.Nome).ToList())
            {
                var treinamentos = colaborador.Uniorg.Maquinas.SelectMany(m => m.TreinamentoEspecificos).AsQueryable();

                if (padroes?.Length > 0)
                {
                    var treinamentosEspecificos = new List<int>();

                    foreach (var padrao in padroes)
                    {
                        if (int.TryParse(Encrypting.Decrypt(padrao), out int treinamentoEspecificoId))
                        {
                            treinamentosEspecificos.Add(treinamentoEspecificoId);
                        }
                    }

                    treinamentos = treinamentos.Where(t => treinamentosEspecificos.Contains(t.Id));
                }
                else if (maquinaId.HasValue)
                {
                    treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.Id == maquinaId.Value));
                }
                else if (areaId.HasValue)
                {
                    treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.AreaId == areaId.Value));
                }
                else if (plantaId.HasValue)
                {
                    treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                }

                foreach (var treinamentoEspecifico in treinamentos.ToList())
                {
                    var metaTreinamentoEspecifico = _db.MetasTreinamentosEspecificos
                        .Where(m => m.ColaboradorId == colaborador.Usuario.Id && m.TreinamentoEspecificoId == treinamentoEspecifico.Id)
                        .OrderByDescending(m => m.DataLancamento)
                        .FirstOrDefault();

                    string meta = "";

                    if (metaTreinamentoEspecifico == null)
                    {
                        meta = "";
                    }
                    else if (metaTreinamentoEspecifico.Meta.HasValue)
                    {
                        meta = metaTreinamentoEspecifico.Meta.Value.ToString();
                    }
                    else
                    {
                        meta = "NA";
                    }

                    var row = new MetaViewModel.Row()
                    {
                        Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                        Treinamento = Encrypting.Encrypt(treinamentoEspecifico.Id.ToString()),
                        Planta = colaborador.Uniorg.Coordenador.Area.Planta.Descricao,
                        Area = colaborador.Uniorg.Coordenador.Area.Descricao,
                        Maquina = string.Join(", ", colaborador.Uniorg.Maquinas.Select(m => m.Descricao).Distinct()),
                        Chapa = colaborador.Usuario.Chapa,
                        Nome = colaborador.Usuario.Nome,
                        Padrao = treinamentoEspecifico.Descricao,
                        Meta = meta,
                    };

                    model.Add(row);
                }
            }

            return Json(model);
        }

        public async Task<IActionResult> CadastrarMetaLote(IFormFile file)
        {
            if (file != null)
            {
                CadastrarMetaLoteService service = new CadastrarMetaLoteService(_db);
                var result = service.ReadFile(file.OpenReadStream());

                if (result == CadastrarMetaLoteService.Result.Success)
                {
                    return Json(new { Result = "Cadastrado com sucesso" });
                }
                else if (result == CadastrarMetaLoteService.Result.InternalError)
                {
                    return Json(new { Result = "Algo deu errado" });
                }
                else if (result == CadastrarMetaLoteService.Result.IncorrectExcelFormat)
                {
                    return Json(new { Result = "Excel enviado no formato incorreto" });
                }
                else if (result == CadastrarMetaLoteService.Result.NoDataFound)
                {
                    return Json(new { Result = "Nenhum dado encontrado" });
                }

                return Json(new { Result = "Algo deu errado" });
            }

            return Json(new { Result = "Nenhum arquivo enviado" });
        }
        #endregion

        #region Treinamento
        
        public ActionResult Treinamento()
        {

            var model = new TreinamentoViewModel();

            foreach (var _planta in _db.Plantas.ToList())
            {
                var key = Encrypting.Encrypt(_planta.Id.ToString());

                model.Plantas.Add(new Option()
                {
                    Key = key,
                    Text = _planta.Descricao,
                });
            }

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(a => a.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Add(new Option()
                {
                    Key = key,
                    Text = _area.Alias,
                });
            }

            var maquinas = _db.Maquinas
                .Where(m => m.IsAtivo)
                .Where(m => m.Uniorgs.Any())
                .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                .OrderBy(m => m.Descricao)
                .ToList();

            foreach (var _maquina in maquinas)
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Add(new Option()
                {
                    Key = key,
                    Text = _maquina.Descricao,
                });
            }

            foreach (var _padrao in _db.TreinamentosEspecificos.Where(t => t.IsAtivo).OrderBy(t => t.Descricao).ToList())
            {
                var key = "especifico_" + Encrypting.Encrypt(_padrao.Id.ToString());

                model.Padroes.Add(new Option()
                {
                    Key = key,
                    Text = _padrao.Descricao,
                });
            }

            foreach (var _padrao in _db.Treinamentos.Where(t => t.IsAtivo).OrderBy(t => t.Descricao).ToList())
            {
                var key = "treinamento_" + Encrypting.Encrypt(_padrao.Id.ToString());

                model.Padroes.Add(new Option()
                {
                    Key = key,
                    Text = _padrao.Descricao,
                });
            }

            model.Padroes = model.Padroes.OrderBy(p => p.Text).ToList();

            foreach (var _profissional in _db.Colaboradores.Where(c => c.Usuario.IsAtivo).Where(c => c.Usuario.Situacao == Situacao.Normal).OrderBy(c => c.Usuario.Nome).ToList())
            {
                var key = Encrypting.Encrypt(_profissional.Usuario.Id.ToString());

                model.Profissionais.Add(new Option()
                {
                    Key = key,
                    Text = $"{_profissional.Usuario.Chapa} - {_profissional.Usuario.Nome}",
                });
            }

            return View(model);
        }

        public ActionResult FiltrarTreinamentos(TreinamentoViewModel.TipoTreinamento? tipoTreinamento, string planta, string area, string maquina)
        {

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var model = new TreinamentoViewModel();

            foreach (var _planta in _db.Plantas.ToList())
            {
                var key = Encrypting.Encrypt(_planta.Id.ToString());

                model.Plantas.Add(new Option()
                {
                    Key = key,
                    Text = _planta.Descricao,
                    IsSelected = key == planta,
                });
            }

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any());

            if (plantaId.HasValue)
            {
                areas = areas.Where(a => a.PlantaId == plantaId.Value);
            }

            foreach (var _area in areas.OrderBy(a => a.Alias).ToList())
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.Areas.Add(new Option()
                {
                    Key = key,
                    Text = _area.Alias,
                    IsSelected = key == area,
                });
            }

            var maquinas = _db.Maquinas
                .Where(m => m.IsAtivo)
                .Where(m => m.Uniorgs.Any())
                .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any());

            if (areaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value));
            }

            foreach (var _maquina in maquinas.OrderBy(m => m.Descricao).ToList())
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.Maquinas.Add(new Option()
                {
                    Key = key,
                    Text = _maquina.Descricao,
                    IsSelected = key == maquina,
                });
            }

            if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Específico || tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos)
            {
                var treinamentosEspecificos = _db.TreinamentosEspecificos.Where(t => t.IsAtivo);

                if (maquinaId.HasValue)
                {
                    treinamentosEspecificos = treinamentosEspecificos.Where(p => p.Maquinas.Any(m => m.Id == maquinaId.Value));
                }
                else if (areaId.HasValue)
                {
                    treinamentosEspecificos = treinamentosEspecificos.Where(p => p.Maquinas.Any(m => m.AreaId == areaId.Value));
                }
                else if (plantaId.HasValue)
                {
                    treinamentosEspecificos = treinamentosEspecificos.Where(p => p.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                }

                foreach (var _padrao in treinamentosEspecificos.OrderBy(p => p.Descricao).ToList())
                {
                    var key = "especifico_" + Encrypting.Encrypt(_padrao.Id.ToString());

                    model.Padroes.Add(new Option()
                    {
                        Key = key,
                        Text = _padrao.Descricao,
                    });
                }
            }

            if (tipoTreinamento != TreinamentoViewModel.TipoTreinamento.Específico)
            {
                var treinamentos = _db.Treinamentos.Where(t => t.IsAtivo);

                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.WCM)
                {
                    treinamentos = treinamentos.Where(t => t.TipoTreinamentoId == 1);
                }
                else if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Qualidade)
                {
                    treinamentos = treinamentos.Where(t => t.TipoTreinamentoId == 2);
                }

                if (maquinaId.HasValue)
                {
                    treinamentos = treinamentos.Where(t => t.TipoTreinamento.Maquinas.Any(m => m.Id == maquinaId.Value));
                }
                else if (areaId.HasValue)
                {
                    treinamentos = treinamentos.Where(t => t.TipoTreinamento.Maquinas.Any(m => m.AreaId == areaId.Value));
                }
                else if (plantaId.HasValue)
                {
                    treinamentos = treinamentos.Where(t => t.TipoTreinamento.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                }

                foreach (var _padrao in treinamentos.OrderBy(p => p.Descricao).ToList())
                {
                    var key = "treinamento_" + Encrypting.Encrypt(_padrao.Id.ToString());

                    model.Padroes.Add(new Option()
                    {
                        Key = key,
                        Text = _padrao.Descricao,
                    });
                }
            }

            model.Padroes = model.Padroes.OrderBy(p => p.Text).ToList();

            var _profissionais = _db.Colaboradores.Where(c => c.Usuario.IsAtivo).Where(c => c.Usuario.Situacao == Situacao.Normal);

            if (maquinaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Maquinas.Any(m => m.Id == maquinaId.Value));
            }
            else if (areaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Coordenador.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                _profissionais = _profissionais.Where(p => p.Uniorg.Coordenador.Area.PlantaId == plantaId.Value);
            }

            foreach (var _profissional in _profissionais.OrderBy(p => p.Usuario.Nome).ToList())
            {
                var key = Encrypting.Encrypt(_profissional.Usuario.Id.ToString());

                model.Profissionais.Add(new Option()
                {
                    Key = key,
                    Text = $"{_profissional.Usuario.Chapa} - {_profissional.Usuario.Nome}",
                });
            }

            return Json(model);
        }

        public ActionResult BuscarTabelaTreinamento(TreinamentoViewModel.TipoTreinamento? tipoTreinamento, string planta, string area, string maquina, string[] padroes, string[] profissionais)
        {

            if (!tipoTreinamento.HasValue)
            {
                tipoTreinamento = TreinamentoViewModel.TipoTreinamento.Todos;
            }

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var model = new List<TreinamentoViewModel.Row>();

            var colaboradores = _db.Colaboradores.Where(c => c.Usuario.IsAtivo && c.Usuario.Situacao == Situacao.Normal);

            if (profissionais?.Length > 0)
            {
                var colaboradoresSelecionados = new List<int>();

                foreach (var profissional in profissionais)
                {
                    if (int.TryParse(Encrypting.Decrypt(profissional), out int colaboradorId))
                    {
                        colaboradoresSelecionados.Add(colaboradorId);
                    }
                }

                colaboradores = colaboradores.Where(t => colaboradoresSelecionados.Contains(t.Usuario.Id));
            }
            else if (maquinaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId.Value));
            }
            else if (areaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.AreaId == areaId.Value);
            }
            else if (plantaId.HasValue)
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.Area.PlantaId == plantaId.Value);
            }

            foreach (var colaborador in colaboradores.OrderBy(c => c.Usuario.Nome).ToList())
            {
                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Específico || tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos)
                {
                    var treinamentos = colaborador.Uniorg.Maquinas.SelectMany(m => m.TreinamentoEspecificos).AsQueryable();

                    if (padroes?.Length > 0)
                    {
                        var treinamentosEspecificos = new List<int>();

                        foreach (var padrao in padroes)
                        {
                            if (padrao.Contains("especifico"))
                            {
                                var treinamento = padrao.Split('_')[1];

                                if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoEspecificoId))
                                {
                                    treinamentosEspecificos.Add(treinamentoEspecificoId);
                                }
                            }
                        }

                        treinamentos = treinamentos.Where(t => treinamentosEspecificos.Contains(t.Id));
                    }
                    else if (maquinaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.Id == maquinaId.Value));
                    }
                    else if (areaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.AreaId == areaId.Value));
                    }
                    else if (plantaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                    }

                    foreach (var treinamentoEspecifico in treinamentos.ToList())
                    {
                        var turmaTreinamentoEspecifico = _db.TurmasTreinamentosEspecificos
                            .Where(m => m.TreinamentoEspecificoId == treinamentoEspecifico.Id)
                            .Where(m => m.TurmaColaboradores.Any(c => c.ColaboradorId == colaborador.Usuario.Id))
                            .OrderByDescending(m => m.DataRealizacao);

                        if (turmaTreinamentoEspecifico.Any())
                        {
                            foreach (var turma in turmaTreinamentoEspecifico.ToList())
                            {
                                model.Add(new TreinamentoViewModel.Row()
                                {
                                    Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                                    Treinamento = Encrypting.Encrypt(treinamentoEspecifico.Id.ToString()),

                                    Chapa = colaborador.Usuario.Chapa,
                                    Nome = colaborador.Usuario.Nome,
                                    Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                                    Padrao = treinamentoEspecifico.Descricao,
                                    DataConclusao = turma.DataRealizacao.ToString("dd/MM/yyyy"),
                                    Pontuacao = "",
                                    Localizador = string.IsNullOrEmpty(turma.NumeroLocalizador) ? "" : turma.NumeroLocalizador,
                                });
                            }
                        }
                        else
                        {
                            model.Add(new TreinamentoViewModel.Row()
                            {
                                Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                                Treinamento = Encrypting.Encrypt(treinamentoEspecifico.Id.ToString()),

                                Chapa = colaborador.Usuario.Chapa,
                                Nome = colaborador.Usuario.Nome,
                                Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                                Padrao = treinamentoEspecifico.Descricao,
                                DataConclusao = "",
                                Pontuacao = "",
                                Localizador = "",
                            });
                        }
                    }
                }

                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Qualidade || tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos)
                {
                    var tipoTreinamentoId = 2;

                    var treinamentos = colaborador.Uniorg.Maquinas
                        .SelectMany(m => m.TiposTreinamento)
                        .Where(t => t.Id == tipoTreinamentoId)
                        .SelectMany(t => t.Treinamentos)
                        .AsQueryable();

                    if (padroes?.Length > 0)
                    {
                        var treinamentoIds = new List<int>();

                        foreach (var padrao in padroes)
                        {
                            if (padrao.Contains("treinamento"))
                            {
                                var treinamento = padrao.Split('_')[1];

                                if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
                                {
                                    treinamentoIds.Add(treinamentoId);
                                }
                            }
                        }

                        treinamentos = treinamentos.Where(t => treinamentoIds.Contains(t.Id));
                    }
                    else if (maquinaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.Id == maquinaId.Value));
                    }
                    else if (areaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.AreaId == areaId.Value));
                    }
                    else if (plantaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                    }

                    foreach (var treinamento in treinamentos.ToList())
                    {
                        var turmas = _db.TurmasTreinamentos
                            .Where(m => m.TreinamentoId == treinamento.Id)
                            .Where(m => m.Notas.Any(n => n.ColaboradorId == colaborador.Usuario.Id))
                            .OrderByDescending(m => m.DataRealizacao);

                        if (turmas.Any())
                        {
                            foreach (var turma in turmas.ToList())
                            {
                                string nota = "";

                                var notaQuery = turma.Notas.Where(n => n.ColaboradorId == colaborador.Usuario.Id);

                                if (notaQuery.Any())
                                {
                                    nota = notaQuery.FirstOrDefault().Nota.ToString();
                                }

                                model.Add(new TreinamentoViewModel.Row()
                                {
                                    Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                                    Treinamento = Encrypting.Encrypt(treinamento.Id.ToString()),

                                    Chapa = colaborador.Usuario.Chapa,
                                    Nome = colaborador.Usuario.Nome,
                                    Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                                    Padrao = treinamento.Descricao,
                                    DataConclusao = turma.DataRealizacao.ToString("dd/MM/yyyy"),
                                    Pontuacao = nota,
                                    Localizador = string.IsNullOrEmpty(turma.NumeroLocalizador) ? "" : turma.NumeroLocalizador,
                                });
                            }
                        }
                        else
                        {
                            model.Add(new TreinamentoViewModel.Row()
                            {
                                Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                                Treinamento = Encrypting.Encrypt(treinamento.Id.ToString()),

                                Chapa = colaborador.Usuario.Chapa,
                                Nome = colaborador.Usuario.Nome,
                                Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                                Padrao = treinamento.Descricao,
                                DataConclusao = "",
                                Pontuacao = "",
                                Localizador = "",
                            });
                        }
                    }
                }

                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.WCM || tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos)
                {
                    var tipoTreinamentoId = 1;

                    var treinamentos = colaborador.Uniorg.Maquinas
                        .SelectMany(m => m.TiposTreinamento)
                        .Where(t => t.Id == tipoTreinamentoId)
                        .SelectMany(t => t.Treinamentos)
                        .AsQueryable();

                    if (padroes?.Length > 0)
                    {
                        var treinamentoIds = new List<int>();

                        foreach (var padrao in padroes)
                        {
                            if (padrao.Contains("treinamento"))
                            {
                                var treinamento = padrao.Split('_')[1];

                                if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
                                {
                                    treinamentoIds.Add(treinamentoId);
                                }
                            }
                        }

                        treinamentos = treinamentos.Where(t => treinamentoIds.Contains(t.Id));
                    }
                    else if (maquinaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.Id == maquinaId.Value));
                    }
                    else if (areaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.AreaId == areaId.Value));
                    }
                    else if (plantaId.HasValue)
                    {
                        treinamentos = treinamentos.Where(p => p.TipoTreinamento.Maquinas.Any(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)));
                    }

                    foreach (var treinamento in treinamentos.ToList())
                    {
                        var turmaTreinamentoEspecifico = _db.TurmasTreinamentos
                            .Where(m => m.Notas.Any(n => n.Colaborador.Usuario.Id == colaborador.Usuario.Id) && m.TreinamentoId == treinamento.Id)
                            .OrderByDescending(m => m.DataRealizacao);

                        foreach (var turma in turmaTreinamentoEspecifico.ToList())
                        {
                            string nota = "";

                            var notaQuery = turma.Notas.Where(n => n.ColaboradorId == colaborador.Usuario.Id);

                            if (notaQuery.Any())
                            {
                                nota = notaQuery.FirstOrDefault().Nota.ToString();
                            }

                            var row = new TreinamentoViewModel.Row()
                            {
                                Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                                Treinamento = Encrypting.Encrypt(treinamento.Id.ToString()),

                                Chapa = colaborador.Usuario.Chapa,
                                Nome = colaborador.Usuario.Nome,
                                Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                                Padrao = treinamento.Descricao,
                                DataConclusao = turma.DataRealizacao.ToString("dd/MM/yyyy"),
                                Pontuacao = nota,
                                Localizador = string.IsNullOrEmpty(turma.NumeroLocalizador) ? "" : turma.NumeroLocalizador,
                            };

                            model.Add(row);
                        }
                    }
                }
            }

            return Json(model);
        }

        public ActionResult BuscarTabelaCadastroTreinamento(TreinamentoViewModel.TipoTreinamento? tipoTreinamento, string planta, string area, string maquina, string[] padroes, string[] profissionais)
        {
            var model = new List<TreinamentoViewModel.Row>();

            if (!tipoTreinamento.HasValue)
            {
                tipoTreinamento = TreinamentoViewModel.TipoTreinamento.Todos;
            }

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            List<Colaborador> colaboradores = new List<Colaborador>();

            if (profissionais?.Length > 0)
            {
                foreach (var profissional in profissionais)
                {
                    if (int.TryParse(Encrypting.Decrypt(profissional), out int colaboradorId))
                    {
                        colaboradores.Add(_db.Colaboradores.Find(colaboradorId));
                    }
                }
            }
            else
            {
                var colaboradoresQuery = _db.Colaboradores.AsQueryable();

                if (maquinaId.HasValue)
                {
                    colaboradoresQuery = colaboradoresQuery.Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId));
                }
                else if (areaId.HasValue)
                {
                    colaboradoresQuery = colaboradoresQuery.Where(c => c.Uniorg.Coordenador.AreaId == areaId);
                }
                else if (plantaId.HasValue)
                {
                    colaboradoresQuery = colaboradoresQuery.Where(c => c.Uniorg.Coordenador.Area.PlantaId == plantaId);
                }

                foreach (var colaborador in colaboradoresQuery.ToList())
                {
                    colaboradores.Add(colaborador);
                }
            }

            Dictionary<int, TreinamentoViewModel.TipoTreinamento> treinamentos = new Dictionary<int, TreinamentoViewModel.TipoTreinamento>();

            if (padroes?.Length > 0)
            {
                foreach (var padrao in padroes)
                {
                    string treinamento = "";

                    TreinamentoViewModel.TipoTreinamento tipo = TreinamentoViewModel.TipoTreinamento.Todos;

                    if (padrao.Contains("treinamento"))
                    {
                        treinamento = padrao.Replace("treinamento_", "");
                        tipo = TreinamentoViewModel.TipoTreinamento.WCM;
                    }
                    else if (padrao.Contains("especifico"))
                    {
                        treinamento = padrao.Replace("especifico_", "");
                        tipo = TreinamentoViewModel.TipoTreinamento.Específico;
                    }

                    if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
                    {
                        treinamentos.Add(treinamentoId, tipo);
                    }
                }
            }
            else
            {
                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos || tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Específico)
                {
                    var treinamentoQuery = _db.TreinamentosEspecificos.AsQueryable();

                    if (maquinaId.HasValue)
                    {
                        treinamentoQuery = treinamentoQuery.Where(t => t.Maquinas.Any(m => m.Id == maquinaId));
                    }
                    else if (areaId.HasValue)
                    {
                        treinamentoQuery = treinamentoQuery.Where(t => t.Maquinas.Any(m => m.AreaId == areaId));
                    }
                    else if (plantaId.HasValue)
                    {
                        treinamentoQuery = treinamentoQuery.Where(t => t.Maquinas.Any(m => m.Uniorgs.Select(u => u.Coordenador.Area.PlantaId).Any(p => p == plantaId)));
                    }

                    foreach (var treinamento in treinamentoQuery.ToList())
                    {
                        treinamentos.Add(treinamento.Id, TreinamentoViewModel.TipoTreinamento.Específico);
                    }
                }

                if (tipoTreinamento == TreinamentoViewModel.TipoTreinamento.Todos || tipoTreinamento != TreinamentoViewModel.TipoTreinamento.Específico)
                {
                    foreach (var treinamento in _db.Treinamentos.ToList())
                    {
                        TreinamentoViewModel.TipoTreinamento tipo;

                        if (treinamento.TipoTreinamentoId == 1)
                        {
                            tipo = TreinamentoViewModel.TipoTreinamento.WCM;
                        }
                        else
                        {
                            tipo = TreinamentoViewModel.TipoTreinamento.Qualidade;
                        }

                        treinamentos.Add(treinamento.Id, tipo);
                    }
                }
            }

            foreach (var colaborador in colaboradores.OrderBy(c => c.Usuario.Nome))
            {
                foreach (var treinamento in treinamentos)
                {
                    bool isEspecifico = false;
                    string descricaoTreinamento = "";
                    string treinamentoId = "";

                    if (treinamento.Value == TreinamentoViewModel.TipoTreinamento.Específico)
                    {
                        isEspecifico = true;
                        descricaoTreinamento = _db.TreinamentosEspecificos.Find(treinamento.Key).Descricao;
                        treinamentoId = "especifico_" + Encrypting.Encrypt(treinamento.Key.ToString());
                    }
                    else
                    {
                        descricaoTreinamento = _db.Treinamentos.Find(treinamento.Key).Descricao;
                        treinamentoId = "treinamento_" + Encrypting.Encrypt(treinamento.Key.ToString());
                    }

                    var row = new TreinamentoViewModel.Row()
                    {
                        Colaborador = Encrypting.Encrypt(colaborador.Usuario.Id.ToString()),
                        Treinamento = treinamentoId,

                        Chapa = colaborador.Usuario.Chapa,
                        Nome = colaborador.Usuario.Nome,
                        Gestor = colaborador.Uniorg.Coordenador.Usuario.Nome,
                        Padrao = descricaoTreinamento,

                        IsEspecifico = isEspecifico,
                    };

                    model.Add(row);
                }
            }

            return Json(model);
        }

        public ActionResult CadastrarTreinamento(int? pontuacao, string localizador, DateTime dataTreinamento, string colaborador, string treinamento)
        {
            if (int.TryParse(Encrypting.Decrypt(colaborador), out int colaboradorId))
            {
                if (treinamento.Contains("especifico_"))
                {
                    treinamento = treinamento.Replace("especifico_", "");

                    if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
                    {
                        TurmaTreinamentoEspecifico turma;
                        var query = _db.TurmasTreinamentosEspecificos.Where(t => t.NumeroLocalizador == localizador);

                        if (query.Any())
                        {
                            turma = query.FirstOrDefault();
                        }
                        else
                        {
                            turma = new TurmaTreinamentoEspecifico()
                            {
                                DataLancamento = DateTime.Now,
                                DataRealizacao = dataTreinamento,
                                NumeroLocalizador = localizador,
                                TreinamentoEspecificoId = treinamentoId,
                            };

                            _db.TurmasTreinamentosEspecificos.Add(turma);
                            _db.SaveChanges();
                        }

                        _db.TurmasTreinamentosEspecificosColaboradores.Add(new TurmaTreinamentoEspecificoColaborador()
                        {
                            IsAtivo = true,
                            ColaboradorId = colaboradorId,
                            TurmaTreinamentoEspecificoId = turma.Id,
                        });
                        _db.SaveChanges();
                    }
                    else
                    {
                        return StatusCode(500);
                    }
                }
                else if (pontuacao.HasValue)
                {
                    treinamento = treinamento.Replace("treinamento_", "");

                    if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
                    {
                        TurmaTreinamento turma;
                        var query = _db.TurmasTreinamentos.Where(t => t.NumeroLocalizador == localizador);

                        if (query.Any())
                        {
                            turma = query.FirstOrDefault();
                        }
                        else
                        {
                            turma = new TurmaTreinamento()
                            {
                                DataLancamento = DateTime.Now,
                                DataRealizacao = dataTreinamento,
                                NumeroLocalizador = localizador,
                                TreinamentoId = treinamentoId,
                            };

                            _db.TurmasTreinamentos.Add(turma);
                            _db.SaveChanges();
                        }

                        _db.NotasTreinamentos.Add(new NotaTreinamento()
                        {
                            Nota = pontuacao.Value,
                            ColaboradorId = colaboradorId,
                            TurmaTreinamentoId = turma.Id,
                        });
                        _db.SaveChanges();
                    }
                    else
                    {
                        return StatusCode(500);
                    }
                }
                else
                {
                    return StatusCode(400);
                }
            }
            else
            {
                return StatusCode(400);
            }

            return StatusCode(200);
        }

        public ActionResult CadastrarTreinamentoLote(IFormFile file)
        {
            if (file != null)
            {
                CadastrarTreinamentoLoteService service = new CadastrarTreinamentoLoteService(_db);
                var result = service.ReadFile(file.OpenReadStream());

                if (result == CadastrarTreinamentoLoteService.Result.Success)
                {
                    return Json(new { Result = "Cadastrado com sucesso" });
                }
                else if (result == CadastrarTreinamentoLoteService.Result.InternalError)
                {
                    return Json(new { Result = "Algo deu errado" });
                }
                else if (result == CadastrarTreinamentoLoteService.Result.IncorrectExcelFormat)
                {
                    return Json(new { Result = "Excel enviado no formato incorreto" });
                }
                else if (result == CadastrarTreinamentoLoteService.Result.NoDataFound)
                {
                    return Json(new { Result = "Nenhum dado encontrado" });
                }

                return Json(new { Result = "Algo deu errado" });
            }

            return Json(new { Result = "Nenhum arquivo enviado" });
        }
        #endregion
    }
}