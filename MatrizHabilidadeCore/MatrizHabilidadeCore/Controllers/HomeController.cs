using MatrizHabilidade.ViewModel;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MatrizHabilidadeCore.Utility;
using Microsoft.AspNetCore.Identity;
using MatrizHabilidadeCore.Services;
using MatrizHabilidade.Services;
using MatrizHabilidadeCore.ViewModel;
using System.Net;

namespace MatrizHabilidadeCore.Controllers
{
    public class HomeController : BaseController
    {

        public HomeController(DataBaseContext _db, CookieService _cookieService, UserManager<Usuario> _userManager, SignInManager<Usuario> _signInManager) : base(_db, _cookieService, _userManager, _signInManager)
        {
        }

        public IActionResult Teste()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var model = new Dictionary<string, string>();
            
            foreach (var planta in _db.Plantas.Where(p => p.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.Add(key, planta.Descricao);
            }

            foreach (var tipo in _db.TiposTreinamentos.ToList())
            {
                foreach (var maquina in _db.Maquinas.ToList())
                {
                    tipo.Maquinas.Add(maquina);

                    _db.SaveChanges();
                }
            }

            if (Request.IsMobileDevice())
            {
                return RedirectToAction("Index", "Formulario");
            }

            return View(model);
        }

        public IActionResult Sair()
        {

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ChangeYear(int ano, string returnUrl)
        {
            string redirectUri = "/MatrizHabilidade/Home";

            if (Url.IsLocalUrl(returnUrl))
            {
                redirectUri = returnUrl;
            }

            await SetCurrentYear(ano);

            return Redirect(redirectUri);
        }

        public IActionResult GetColaborador(string text, string maquina)
        {
            var model = new List<AutoCompleteViewModel>();

            int maquinaId = Convert.ToInt32(Encrypting.Decrypt(maquina));

            var query = _db.Colaboradores
                 .Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId))
                 .Select(c => new AutoCompleteViewModel() {
                     key = c.Id.ToString(),
                     value = $"{c.Chapa} - {c.Nome}"
                 })
                 .ToList()
                 .Where(c => c.value.Contains(text))
                 .OrderBy(c => c.value)
                 .Take(20);

            foreach (var result in query)
            {
                result.Init(text);
                model.Add(result);
            }

            return Json(model);
        }

        public IActionResult GetTreinamentos(int tipo, string maquina)
        {
            if (maquina != null)
            {
                int maquinaId = Convert.ToInt32(Encrypting.Decrypt(maquina));

                var result = new Dictionary<string, string>();

                var _maquina = _db.Maquinas.Find(maquinaId);

                if (tipo == 2)
                {
                    foreach (var treinamentoEspecifico in _maquina.TreinamentoEspecificos)
                    {
                        var key = Encrypting.Encrypt(treinamentoEspecifico.Id.ToString());

                        if (!result.ContainsKey(key))
                        {
                            result.Add(key, treinamentoEspecifico.Descricao);
                        }
                    }
                }
                else
                {
                    int tipoTreinamentoId;

                    if (tipo == 0)
                    {
                        tipoTreinamentoId = 2;
                    }
                    else
                    {
                        tipoTreinamentoId = 1;
                    }

                    foreach (var treinamento in _maquina.TiposTreinamento.Where(t => t.Id == tipoTreinamentoId).SelectMany(t => t.Treinamentos).ToList())
                    {
                        var key = Encrypting.Encrypt(treinamento.Id.ToString());

                        if (!result.ContainsKey(key))
                        {
                            result.Add(key, treinamento.Descricao);
                        }
                    }
                }
                return Json(result);
            }

            return Content("");
        }

        public async Task<IActionResult> AtualizarHistorico()
        {
            var historyService = new HistoryService(_db);
            await historyService.UpdateHistory();

            SetMessage("Atualizado com Sucesso");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AtualizarGraficos()
        {
            var auxiliaryTableService = new AuxiliaryTableService(_db);
            await auxiliaryTableService.UpdateAuxiliaryTables();

            SetMessage("Atualizado com Sucesso");

            return RedirectToAction("Index");
        }

        public string GetUserInformation(string login, string email)
        {
            login = Encrypting.Decrypt(login);
            email = Encrypting.Decrypt(email);

            if (string.IsNullOrEmpty(login) && string.IsNullOrEmpty(email))
                return "";

            UserInformationViewModel model = null;

            var colaborador = _db.Colaboradores
                .Where(c => c.Login.ToLower() == login.ToLower() || c.Email.ToLower().StartsWith(email.ToLower()))
                .FirstOrDefault();

            if (colaborador != null)
            {
                string maquina = "";
                string responsavel = "";
                string area = "";
                string unidade = "";

                if (colaborador.Uniorg != null)
                {
                    responsavel = colaborador.Uniorg.Coordenador.Login;
                    area = colaborador.Uniorg.Coordenador.Area.Descricao;
                    unidade = colaborador.Uniorg.Coordenador.Area.Planta.Descricao;

                    if (colaborador.Uniorg.Maquinas.Any())
                    {
                        maquina = colaborador.Uniorg.Maquinas.FirstOrDefault().Descricao;
                    }
                }

                model = new UserInformationViewModel
                {
                    Login = colaborador.Login,
                    Email = colaborador.Email,
                    Chapa = colaborador.Chapa,
                    Cargo = colaborador.Funcao,
                    Maquina = maquina,
                    LoginResponsavel = responsavel,
                    Area = area,
                    Unidade = unidade,
                };
            }
            else
            {
                var coordenador = _db.Coordenadores
                .Where(c => c.Login.ToLower() == login.ToLower() || c.Email.ToLower().StartsWith(email.ToLower()))
                .FirstOrDefault();

                if (coordenador != null)
                {
                    model = new UserInformationViewModel
                    {
                        Login = coordenador.Login,
                        Email = coordenador.Email,
                        Chapa = coordenador.Chapa,
                        Cargo = coordenador.Funcao,
                        Area = coordenador.Area.Descricao,
                        Unidade = coordenador.Area.Planta.Descricao
                    };
                }
            }

            if (model == null)
            {
                return "";
            }

            return Encrypting.Encrypt(JsonConvert.SerializeObject(model));
        }

        public IActionResult TreinamentoVirtual(string json)
        {
            json = Encrypting.Decrypt(json);

            _db.IntegrationLogs.Add(new IntegrationLog()
            {
                JSON = json,
                Date = DateTime.Now,
            });
            _db.SaveChanges();

            if (string.IsNullOrEmpty(json))
            {
                return StatusCode(400);
            }

            var response = JsonConvert.DeserializeObject<APIResponseViewModel>(json);

            var colaboradorQuery = _db.Colaboradores.Where(c => c.Nome == response.NomeUsuario);

            if (!colaboradorQuery.Any())
            {
                colaboradorQuery = _db.Colaboradores.Where(c => c.Email == response.EmailUsuario);

                if (!colaboradorQuery.Any())
                {
                    return StatusCode(404);
                }
            }

            var colaborador = colaboradorQuery.First();

            var treinamentoEspecificoQuery = _db.TreinamentosEspecificos.Where(t => t.Descricao.ToLower() == response.NomeTreinamento.ToLower());

            if (!treinamentoEspecificoQuery.Any())
            {
                var matches = Regex.Matches(response.NomeTreinamento, @"WI-\d+ (.+)");

                if (matches.Count > 0 && matches[0].Groups.Count > 1)
                {
                    var descricao = matches[0].Groups[1].Value;
                    treinamentoEspecificoQuery = _db.TreinamentosEspecificos.Where(t => t.Descricao.ToLower() == descricao.ToLower());
                }
            }

            if (treinamentoEspecificoQuery.Any())
            {
                if (response.Perguntas != null)
                {
                    return StatusCode(100);
                }

                var treinamentoEspecifico = treinamentoEspecificoQuery.First();

                var turma = new TurmaTreinamentoEspecifico()
                {
                    DataLancamento = Util.DateTimeNow(),
                    DataRealizacao = Util.DateTimeNow(),
                    TreinamentoEspecificoId = treinamentoEspecifico.Id,
                };

                _db.TurmasTreinamentosEspecificos.Add(turma);
                _db.SaveChanges();

                var turmaColaborador = new TurmaTreinamentoEspecificoColaborador()
                {
                    IsAtivo = true,
                    ColaboradorId = colaborador.Id,
                    TurmaTreinamentoEspecificoId = turma.Id,
                };

                _db.TurmasTreinamentosEspecificosColaboradores.Add(turmaColaborador);
                _db.SaveChanges();
            }
            else
            {
                var treinamentoQuery = _db.Treinamentos.Where(t => t.Descricao == response.NomeTreinamento);

                if (!treinamentoQuery.Any())
                {
                    return StatusCode(404);
                }

                var treinamento = treinamentoQuery.First();

                if (response.Perguntas == null)
                {
                    return StatusCode(100);
                }

                if (!treinamento.Configuracoes.Any())
                {
                    return StatusCode(100);
                }

                var configuracao = treinamento.Configuracoes.First();

                var acerto = response.QuantidadePerguntasAcertadas / response.QuantidadePerguntas * 100f;

                var nota = acerto >= configuracao.Target ? 2 : 1;

                var turma = new TurmaTreinamento()
                {
                    DataLancamento = Util.DateTimeNow(),
                    DataRealizacao = Util.DateTimeNow(),
                    TreinamentoId = treinamento.Id,
                };

                _db.TurmasTreinamentos.Add(turma);
                _db.SaveChanges();

                var notaTreinamento = new NotaTreinamento()
                {
                    ColaboradorId = colaborador.Id,
                    Nota = nota,
                    TurmaTreinamentoId = turma.Id,
                };

                _db.NotasTreinamentos.Add(notaTreinamento);
                _db.SaveChanges();
            }

            return StatusCode(200);
        }
    }
}
