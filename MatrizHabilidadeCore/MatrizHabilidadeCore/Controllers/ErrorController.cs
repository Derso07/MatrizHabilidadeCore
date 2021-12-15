using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(DataBaseContext db, UserManager<Usuario> userManager, CookieService cookieService) : base(db, userManager, cookieService) { }

        public IActionResult Index()
        {
            IActionResult result = RedirectToAction("Index", "Home");
            int? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userId = CurrentUser.Id;
            }

            string origin = "";
            string message = "Algo deu errado";

            // Essa função é utilizada para obter o erro que ocorreu para chamar essa função
            var handler = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = handler.Error;

            // Para se obter a URL da página que originou esse erro é
            // necessário converter o handler para "ExceptionHandlerFeature" (pouco consistente)
            // ou obte-lo da header (menos consistente ainda)
            if (handler is ExceptionHandlerFeature context)
            {
                origin = context.Path;
            }
            else if (Request.Headers.TryGetValue("Origin", out StringValues originValues))
            {
                origin = originValues;
            }

            // Se a página que originou o erro for a "/" não podemos
            // redirecionar o usuário a ela para que não se exiba uma 
            // página de erro não esperada.
            if (origin == "/")
            {
                result = RedirectToAction("Login", "User");
            }

            // O salvamento dos erros no banco de dados não é necessário 
            // dentro do ambiente de desenvolvimento
            try
            {
                _db.Erros.Add(new Error()
                {
                    Id = (int)userId,
                    Data = DateTime.Now,
                });
                _db.SaveChanges();
            }
            catch
            {
                // Algo deu errado ao realizar a conexão com o banco de dados
                message = "Algo deu errado. Código: 226";
            }

            SetMessage(message);

            return result;
        }

        public IActionResult Status(string origin, int errorCode)
        {
            int? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userId = CurrentUser.Id;
            }

            string message = "Algo deu errado.";

            if (errorCode == (int)HttpStatusCode.NotFound)
            {
                message = "Página não encontrada";
            }

            try
            {
                _db.Erros.Add(new Error()
                {
                    InnerExceptionType = $"{{ ErrorCode: {errorCode}, Origin: {origin} }}",
                    Id = (int)userId,
                    Data = DateTime.Now,
                });
                _db.SaveChanges();
            }
            catch
            {
                // Algo deu errado ao realizar a conexão com o banco de dados
                message = "Algo deu errado. Código: 226";
            }

            SetMessage(message);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult DividirTreinamentoEspecifico()
        {
            /*
            Atualização das auditorias
            UPDATE `auditoria` SET TreinamentoEspecificoId = 157
            WHERE TreinamentoEspecificoId = 31 AND ColaboradorId IN 
            (
                SELECT colaborador.Id FROM colaborador 
                INNER JOIN uniorgmaquina ON uniorgmaquina.UniorgId = colaborador.UniorgId
                WHERE uniorgmaquina.MaquinaId = 10
            )
             */

            /*
            Atualização das metas
            UPDATE `metatreinamentoespecifico` SET TreinamentoEspecificoId = 157
            WHERE TreinamentoEspecificoId = 31 AND ColaboradorId IN 
            (
	            SELECT colaborador.Id FROM colaborador 
                INNER JOIN uniorgmaquina ON uniorgmaquina.UniorgId = colaborador.UniorgId
                WHERE uniorgmaquina.MaquinaId = 10
            )
             */

            int maquinaId = 10;

                var transfer = new Dictionary<int, int>()
                {
                    { 31, 157 },
                };

                foreach (var turma in _db.TurmasTreinamentosEspecificos.Where(t => t.TreinamentoEspecificoId == 31).ToList())
                {
                    var newTreinamentoId = 157;

                    TurmaTreinamentoEspecifico newTurma = _db.TurmasTreinamentosEspecificos
                        .Where(t => t.TreinamentoEspecificoId == newTreinamentoId)
                        .Where(t => t.DataLancamento == turma.DataLancamento)
                        .Where(t => t.DataRealizacao == turma.DataRealizacao)
                        .Where(t => t.NumeroLocalizador == turma.NumeroLocalizador)
                        .FirstOrDefault();

                    if (newTurma == null)
                    {
                        newTurma = new TurmaTreinamentoEspecifico()
                        {
                            TreinamentoEspecificoId = newTreinamentoId,
                            DataLancamento = turma.DataLancamento,
                            DataRealizacao = turma.DataRealizacao,
                            NumeroLocalizador = turma.NumeroLocalizador,
                        };

                        _db.TurmasTreinamentosEspecificos.Add(newTurma);
                        _db.SaveChanges();
                    }

                    var turmaColaboradores = turma.TurmaColaboradores
                        .Where(c => c.Colaborador != null)
                        .Where(c => c.Colaborador.Uniorg != null)
                        .Where(c => c.Colaborador.Uniorg.Maquinas.Any(m => m.Id == maquinaId))
                        .ToList();

                    foreach (var colaborador in turmaColaboradores)
                    {
                        //db.TurmasTreinamentosEspecificosColaboradores.Remove(colaborador);
                        //db.SaveChanges();

                        var query = _db.TurmasTreinamentosEspecificosColaboradores
                            .Where(t => t.IsAtivo)
                            .Where(t => t.ColaboradorId == colaborador.ColaboradorId)
                            .Where(t => t.TurmaTreinamentoEspecificoId == newTurma.Id);

                        if (!query.Any())
                        {
                            _db.TurmasTreinamentosEspecificosColaboradores.Add(new TurmaTreinamentoEspecificoColaborador()
                            {
                                IsAtivo = true,
                                ColaboradorId = colaborador.ColaboradorId,
                                TurmaTreinamentoEspecificoId = newTurma.Id,
                            });
                            _db.SaveChanges();
                        }
                    }
                }
            return Content("Ok");
        }

        public ActionResult ChangeUser(string cookieValue)
        {
            return RedirectToAction("Index", "Home");
        }
        
        public ActionResult UsuarioHasTreinamento(string login, int nota)
        {

                int treinamentoId = 3;
                var colaborador = _db.Colaboradores.Where(c => c.Usuario.Login == login).FirstOrDefault();

                if (colaborador != null)
                {
                    var hasTreinamento = _db.TurmasTreinamentos
                        .Where(t => t.TreinamentoId == treinamentoId)
                        .SelectMany(t => t.Notas)
                        .Where(n => n.ColaboradorId == colaborador.Usuario.Id)
                        .Where(n => n.Nota >= nota)
                        .Any();

                    return Content(hasTreinamento.ToString());
                }
            return Content("");
        }

        public ActionResult NotFound()
        {
            SetMessage("Algo deu errado");

            return RedirectToAction("Index", "Home");
        }
    }
}
