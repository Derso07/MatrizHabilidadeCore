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
    public class MatrizController : BaseController
    {
        public MatrizController(DataBaseContext _db, UserManager<Usuario> userManager, CookieService cookieService) : base(_db, userManager, cookieService)
        {

        }
        public ActionResult Index(FiltroTabelaViewModel model)
        {
            if (int.TryParse(Encrypting.Decrypt(model.Planta), out int plantaId))
            {
                Planta planta = _db.Plantas.Find(plantaId);

                if (int.TryParse(Encrypting.Decrypt(model.Area), out int areaId))
                {
                    Area area = _db.Areas.Find(areaId);

                    if (int.TryParse(Encrypting.Decrypt(model.Maquina), out int maquinaId))
                    {
                        Maquina maquina = _db.Maquinas.Find(maquinaId);

                        var gapCalculator = new GAPCalculatorService();

                        var tiposTreinamento = _db.TiposTreinamentos.Select(t => t.Id).ToList();

                        var gapMetadata = new Dictionary<int, GAPMetadataDTO>();
                        var gapEspecificoMetadata = new GAPMetadataDTO();

                        var result = new MatrizViewModel()
                        {
                            PlantaId = Encrypting.Encrypt(planta.Id.ToString()),
                            PlantaDescricao = planta.Descricao,
                            AreaId = Encrypting.Encrypt(area.Id.ToString()),
                            AreaDescricao = area.Alias,
                            MaquinaId = Encrypting.Encrypt(maquina.Id.ToString()),
                            MaquinaDescricao = maquina.Descricao,

                            FiltroTipo = model.Tipo ?? -1,
                            FiltroNota = model.FiltroNota ?? -1,
                            FiltroStatus = model.FiltroStatus ?? -1,
                            FiltroMeta = model.FiltroMeta ?? -1,
                        };

                        int? filtroTreinamentoId = null;

                        if (!string.IsNullOrEmpty(model.Treinamento))
                        {
                            if (model.Treinamento != "-1")
                            {
                                if (int.TryParse(Encrypting.Decrypt(model.Treinamento), out int aux))
                                {
                                    filtroTreinamentoId = aux;

                                    if (model.Tipo == 2)
                                    {
                                        var treinamento = _db.TreinamentosEspecificos.Find(filtroTreinamentoId);

                                        if (treinamento != null)
                                        {
                                            model.TreinamentoId = treinamento.Id;
                                            result.FiltroTreinamento = new Tuple<string, string>(model.Treinamento, treinamento.Descricao);
                                        }
                                    }
                                    else
                                    {
                                        var treinamento = _db.Treinamentos.Find(filtroTreinamentoId);

                                        if (treinamento != null)
                                        {
                                            model.TreinamentoId = treinamento.Id;
                                            result.FiltroTreinamento = new Tuple<string, string>(model.Treinamento, treinamento.Descricao);
                                        }
                                    }
                                }
                            }
                        }

                        var treinamentos = new List<Treinamento>();
                        var treinamentosEspecificos = new List<TreinamentoEspecifico>();

                        foreach (var tipoTreinamento in maquina.TiposTreinamento.Where(t => t.Treinamentos.Any()).ToList())
                        {
                            var tipo = new MatrizViewModel.TipoTreinamento
                            {
                                Id = tipoTreinamento.Id,
                                Descricao = tipoTreinamento.Descricao,
                                CorTitulo = tipoTreinamento.CorTitulo,
                                CorTreinamento = tipoTreinamento.CorTreinamento,
                                CorFonte = tipoTreinamento.CorFonte,
                            };

                            foreach (var treinamento in tipoTreinamento.Treinamentos.Where(t => t.IsAtivo).ToList())
                            {
                                treinamentos.Add(treinamento);

                                tipo.Treinamentos.Add(new MatrizViewModel.Treinamento()
                                {
                                    Descricao = treinamento.Descricao,
                                });

                                if (filtroTreinamentoId.HasValue)
                                {
                                    if (model.Tipo == 0)
                                    {
                                        if (treinamento.Id != filtroTreinamentoId.Value)
                                        {
                                            var key = Encrypting.Encrypt(treinamento.Id.ToString());

                                            if (!result.TreinamentosFiltrados.ContainsKey(key))
                                            {
                                                result.TreinamentosFiltrados.Add(key, treinamento.Descricao);
                                            }
                                        }
                                    }
                                }
                            }

                            result.TiposTreinamento.Add(tipo);
                        }

                        foreach (var treinamento in maquina.TreinamentoEspecificos.Where(t => t.IsAtivo))
                        {
                            treinamentosEspecificos.Add(treinamento);

                            result.TreinamentosEspecificos.Add(new MatrizViewModel.Treinamento()
                            {
                                Descricao = treinamento.Descricao,
                            });

                            if (filtroTreinamentoId.HasValue)
                            {
                                if (model.Tipo == 2)
                                {
                                    if (treinamento.Id != filtroTreinamentoId.Value)
                                    {
                                        var key = Encrypting.Encrypt(treinamento.Id.ToString());

                                        if (!result.TreinamentosFiltrados.ContainsKey(key))
                                        {
                                            result.TreinamentosFiltrados.Add(key, treinamento.Descricao);
                                        }
                                    }
                                }
                            }
                        }

                        List<Colaborador> colaboradores = new List<Colaborador>();

                        if (model.Colaboradores == null)
                        {
                            colaboradores = maquina.Uniorgs
                                .SelectMany(u => u.Colaboradores)
                                .Where(c => c.Usuario.IsAtivo)
                                .ToList();
                        }
                        else
                        {
                            foreach (var colaborador in model.Colaboradores)
                            {
                                if (int.TryParse(Encrypting.Decrypt(colaborador), out int colaboradorId))
                                {
                                    var _colaborador = _db.Colaboradores.Find(colaboradorId);

                                    if (_colaborador != null)
                                    {
                                        if (_colaborador.Usuario.IsAtivo)
                                        {
                                            result.FiltroColaboradores.Add(colaborador, $"{_colaborador.Usuario.Nome} - {_colaborador.Usuario.Email}");

                                            colaboradores.Add(_colaborador);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var colaborador in colaboradores.OrderBy(c => c.Usuario.Nome))
                        {
                            var operador = new MatrizViewModel.Colaborador()
                            {
                                Chapa = colaborador.Usuario.Chapa,
                                Nome = colaborador.Usuario.Nome,
                                Funcao = colaborador.Usuario.Funcao,
                                Maquina = maquina.Descricao,
                                IsFacilitador = colaborador.IsFacilitador,
                            };

                            operador.Treinamentos = new TreinamentoColaboradorViewModel[treinamentos.Count + treinamentosEspecificos.Count];

                            int index = 0;
                            bool ignoreRow = false;

                            for (var treinamentoIndex = 0; index < treinamentos.Count; treinamentoIndex++, index++)
                            {
                                if (!gapMetadata.ContainsKey(treinamentos[treinamentoIndex].TipoTreinamentoId))
                                {
                                    gapMetadata.Add(treinamentos[treinamentoIndex].TipoTreinamentoId, new GAPMetadataDTO());
                                }

                                int? nota = null;

                                operador.Treinamentos[index] = new TreinamentoColaboradorViewModel();

                                int treinamentoId = treinamentos[treinamentoIndex].Id;

                                var notaQuery = _db.NotasTreinamentos
                                    .Where(n => n.ColaboradorId == colaborador.Usuario.Id)
                                    .Where(n => n.TurmaTreinamento.TreinamentoId == treinamentoId)
                                    .OrderByDescending(n => n.TurmaTreinamento.DataRealizacao)
                                    .OrderByDescending(n => n.TurmaTreinamento.DataLancamento)
                                    .ThenByDescending(n => n.Id);

                                if (notaQuery.Any())
                                {
                                    nota = notaQuery.First().Nota;
                                }

                                if (nota.HasValue)
                                {
                                    gapMetadata[treinamentos[treinamentoIndex].TipoTreinamentoId].SomatoriaNota += nota.Value;
                                }

                                operador.Treinamentos[index].Nota = new TreinamentoColaboradorViewModel.Cell()
                                {
                                    Value = nota.ToString(),
                                    CellType = TreinamentoColaboradorViewModel.CellType.Number,
                                };

                                gapMetadata[treinamentos[treinamentoIndex].TipoTreinamentoId].SomatoriaMeta += 2;

                                operador.Treinamentos[index].Meta = new TreinamentoColaboradorViewModel.Cell()
                                {
                                    Value = "2",
                                    CellType = TreinamentoColaboradorViewModel.CellType.Number,
                                };

                                bool status = nota.HasValue;

                                gapMetadata[treinamentos[treinamentoIndex].TipoTreinamentoId].SomatoriaTreinamentos += 1;

                                if (status)
                                {
                                    gapMetadata[treinamentos[treinamentoIndex].TipoTreinamentoId].SomatoriaPresenca += 1;

                                    operador.Treinamentos[index].IsOK = new TreinamentoColaboradorViewModel.Cell()
                                    {
                                        Value = "OK"
                                    };
                                }
                                else
                                {
                                    operador.Treinamentos[index].IsOK = new TreinamentoColaboradorViewModel.Cell()
                                    {
                                        Value = "NOK",
                                        CellStyle = TreinamentoColaboradorViewModel.Style.Yellow,
                                    };
                                }

                                if (model.Tipo.HasValue)
                                {
                                    // Se o tipo for igual a 3 só temos que filtrar
                                    // a partir dos treinamentos específicos
                                    if (model.Tipo != 3)
                                    {
                                        ignoreRow = IsRowIgnored(model, treinamentoId, nota, null, status);
                                    }

                                    if (ignoreRow)
                                    {
                                        break;
                                    }
                                }
                            }

                            // Se a linha já foi ignorada não precisamos buscar os dados
                            // de treinamento específico
                            if (!ignoreRow)
                            {
                                for (var treinamentoIndex = 0; index < operador.Treinamentos.Length; treinamentoIndex++, index++)
                                {
                                    float? nota = null;
                                    float? meta = 0;

                                    operador.Treinamentos[index] = new TreinamentoColaboradorViewModel();

                                    int treinamentoId = treinamentosEspecificos[treinamentoIndex].Id;

                                    var auditoriaQuery = _db.Auditorias
                                        .Where(a => a.ColaboradorId == colaborador.Usuario.Id)
                                        .Where(a => a.TreinamentoEspecificoId == treinamentoId)
                                        .Where(a => a.IsAtivo)
                                        .OrderByDescending(a => a.DataLancamento);

                                    var metaQuery = _db.MetasTreinamentosEspecificos
                                        .Where(m => m.ColaboradorId == colaborador.Usuario.Id)
                                        .Where(m => m.TreinamentoEspecificoId == treinamentoId);

                                    if (metaQuery.Any())
                                    {
                                        meta = metaQuery
                                            .OrderByDescending(m => m.DataLancamento)
                                            .First()
                                            .Meta;
                                    }

                                    if (auditoriaQuery.Any())
                                    {
                                        nota = auditoriaQuery.First().Nota;
                                    }

                                    if (!meta.HasValue || meta.Value <= 1)
                                    {
                                        operador.Treinamentos[index].Nota = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "NA",
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Gray,
                                        };
                                    }
                                    else if (nota.HasValue)
                                    {
                                        nota = (nota > meta ? meta : nota);

                                        gapEspecificoMetadata.SomatoriaNota += nota.Value;

                                        operador.Treinamentos[index].Nota = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = nota.Value.ToString("f0"),
                                            CellType = TreinamentoColaboradorViewModel.CellType.Number,
                                        };
                                    }
                                    else
                                    {
                                        operador.Treinamentos[index].Nota = new TreinamentoColaboradorViewModel.Cell();
                                    }

                                    if (!meta.HasValue)
                                    {
                                        operador.Treinamentos[index].Meta = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "NA",
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Gray,
                                        };
                                    }
                                    else if (meta == 0)
                                    {
                                        operador.Treinamentos[index].Meta = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Red,
                                        };
                                    }
                                    else if (meta == 1)
                                    {
                                        operador.Treinamentos[index].Meta = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "1",
                                            CellType = TreinamentoColaboradorViewModel.CellType.Number,
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Gray,
                                        };
                                    }
                                    else
                                    {
                                        gapEspecificoMetadata.SomatoriaMeta += meta.Value;

                                        operador.Treinamentos[index].Meta = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = meta.Value.ToString("f0"),
                                            CellType = TreinamentoColaboradorViewModel.CellType.Number,
                                        };
                                    }

                                    var status = _db.TurmasTreinamentosEspecificos
                                        .Where(t => t.TreinamentoEspecificoId == treinamentoId)
                                        .Where(t => t.TurmaColaboradores.Any(c => c.ColaboradorId == colaborador.Usuario.Id && c.IsAtivo))
                                        .Any();

                                    if (!meta.HasValue || meta.Value <= 1)
                                    {
                                        operador.Treinamentos[index].IsOK = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "NA",
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Gray,
                                        };
                                    }
                                    else if (status && (!nota.HasValue || nota.Value > 0))
                                    {
                                        gapEspecificoMetadata.SomatoriaPresenca += 1;
                                        gapEspecificoMetadata.SomatoriaTreinamentos += 1;

                                        operador.Treinamentos[index].IsOK = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "OK"
                                        };
                                    }
                                    else
                                    {
                                        gapEspecificoMetadata.SomatoriaTreinamentos += 1;

                                        operador.Treinamentos[index].IsOK = new TreinamentoColaboradorViewModel.Cell()
                                        {
                                            Value = "NOK",
                                            CellStyle = TreinamentoColaboradorViewModel.Style.Yellow,
                                        };
                                    }

                                    if (model.Tipo.HasValue)
                                    {
                                        // Se o tipo for igual a 3 só temos que filtrar
                                        // os treinamentos específicos
                                        if (model.Tipo == 2)
                                        {
                                            ignoreRow = IsRowIgnored(model, treinamentoId, nota, meta, status);
                                        }

                                        if (ignoreRow)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!ignoreRow)
                            {
                                result.Colaboradores.Add(operador);
                            }
                        }

                        var treinamentosArea = _db.ViewTreinamentos
                            .Where(t => t.MaquinaId == maquinaId)
                            .ToList();

                        var treinamentosEspecificosArea = _db.ViewTreinamentosEspecificos
                            .Where(t => t.MaquinaId == maquinaId)
                            .Where(t => !t.IsNA)
                            .ToList();

                        var defaultGapResult = gapCalculator.Compute(null, tiposTreinamento, treinamentosArea, treinamentosEspecificosArea);

                        if (model.HasFilter())
                        {
                            result.TiposTreinamento = result.TiposTreinamento.Select(t => new MatrizViewModel.TipoTreinamento()
                            {
                                Id = t.Id,
                                CorFonte = t.CorFonte,
                                CorTitulo = t.CorTitulo,
                                CorTreinamento = t.CorTreinamento,
                                Descricao = t.Descricao,
                                Treinamentos = t.Treinamentos,
                                GAP = new MatrizViewModel.GAPDTO()
                                {
                                    Conhecimento = Math.Round(defaultGapResult.TipoTreinamentos[t.Id].Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0"),
                                    Treinamento = Math.Round(defaultGapResult.TipoTreinamentos[t.Id].Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0"),
                                }
                            }).ToList();

                            result.GAPTipoEspecifico.Conhecimento = Math.Round(defaultGapResult.Especifico.Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0");
                            result.GAPTipoEspecifico.Treinamento = Math.Round(defaultGapResult.Especifico.Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0");

                            result.GAPGeral.Conhecimento = Math.Round(defaultGapResult.Geral.Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0");
                            result.GAPGeral.Treinamento = Math.Round(defaultGapResult.Geral.Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0");
                        }
                        else
                        {
                            var customGapResult = gapCalculator.Compute(gapMetadata, gapEspecificoMetadata);

                            result.TiposTreinamento = result.TiposTreinamento.Select(t => new MatrizViewModel.TipoTreinamento()
                            {
                                Id = t.Id,
                                CorFonte = t.CorFonte,
                                CorTitulo = t.CorTitulo,
                                CorTreinamento = t.CorTreinamento,
                                Descricao = t.Descricao,
                                Treinamentos = t.Treinamentos,
                                GAP = new MatrizViewModel.GAPDTO()
                                {
                                    Conhecimento = Math.Round(customGapResult.TipoTreinamentos[t.Id].Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0"),
                                    Treinamento = Math.Round(customGapResult.TipoTreinamentos[t.Id].Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0"),
                                }
                            }).ToList();

                            result.GAPTipoEspecifico.Conhecimento = Math.Round(customGapResult.Especifico.Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0");
                            result.GAPTipoEspecifico.Treinamento = Math.Round(customGapResult.Especifico.Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0");

                            result.GAPGeral.Conhecimento = Math.Round(customGapResult.Geral.Conhecimento, 0, MidpointRounding.AwayFromZero).ToString("f0");
                            result.GAPGeral.Treinamento = Math.Round(customGapResult.Geral.Treinamento, 0, MidpointRounding.AwayFromZero).ToString("f0");

                            if (Math.Abs(customGapResult.Geral.Conhecimento - defaultGapResult.Geral.Conhecimento) > 0.5)
                            {
#if (DEBUG)
                                var oi = 0;
#else
                                Task.Run(()=> 
                                {
                                    MailService mail = new MailService();
                                    mail.SendEmailAsync("emanuel.hiroshi@annimar.com.br", "Email", $"Ocorreu um erro no GAP da Máquina {maquinaId}", "Erro na Matriz de Habilidade");
                                });
                                _db.Erros.Add(new Error($"Erro na máquina {maquinaId}"));
                                _db.SaveChanges();
#endif
                            }
                            else if (Math.Abs(customGapResult.Geral.Treinamento - defaultGapResult.Geral.Treinamento) > 0.5)
                            {
#if (DEBUG)
                                var oi = 0;
#else
                                Task.Run(() =>
                                {
                                    MailService mail = new MailService();
                                    mail.SendEmailAsync("emanuel.hiroshi@annimar.com.br", "Email", $"Ocorreu um erro no GAP da Máquina {maquinaId}", "Erro na Matriz de Habilidade");
                                });

                                _db.Erros.Add(new Error($"Erro na máquina {maquinaId}"));
                                _db.SaveChanges();
#endif
                            }
                        }

                        return View(result);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private bool IsRowIgnored(FiltroTabelaViewModel model, int treinamentoId, float? nota, float? meta, bool status)
        {
            bool ignoreRow = false;

            // Se houver algum treinamento selecionado
            if (model.TreinamentoId.HasValue)
            {
                // Se o treinamento do filtro for igual ao treinamento sendo lido
                if (treinamentoId == model.TreinamentoId.Value)
                {
                    if (model.FiltroNota.HasValue)
                    {
                        // Se o filtro da nota for difentente de "Todos" (-1)
                        if (model.FiltroNota > -1)
                        {
                            // Se o filtro da nota for "Vazio" (0)
                            if (model.FiltroNota == 0)
                            {
                                // Se a nota tiver algum valor ignoramos a linha
                                if (nota.HasValue)
                                {
                                    ignoreRow = true;
                                }
                            }
                            else if (model.FiltroNota != nota)
                            {
                                // Se o filtro de nota for maior que 1 ele é igual a nota desejada
                                // e se eles forem diferentes ignoramos a linha
                                ignoreRow = true;
                            }
                        }
                        else if (model.FiltroNota == -2)
                        {
                            // Se o filtro da nota for igual a "NA" (-2)
                            if (meta.HasValue)
                            {
                                // Se a meta tiver algum valor então ignoramos a linha
                                ignoreRow = true;
                            }
                        }
                    }

                    if (model.FiltroStatus.HasValue)
                    {
                        // Se o filtro de status for diferente de "Todos" (-1)
                        if (model.FiltroStatus > -1)
                        {
                            if (model.FiltroStatus == 0 && status == false)
                            {
                                // Se o filtro de status for igual a "OK" (0)
                                // e o status for igual a "NOK" (false) ignoramos a linha

                                ignoreRow = true;
                            }
                            else if (model.FiltroStatus == 1 && status == true)
                            {
                                // Se o filtro de status for igual a "NOK" (1)
                                // e o status for igual a "OK" (true) ignoramos a linha

                                ignoreRow = true;
                            }
                        }
                        else if (model.FiltroStatus == -2)
                        {
                            // Se o filtro de status for igual a "NA" (-2)
                            if (meta.HasValue)
                            {
                                // Se a meta tiver algum valor então ignoramos a linha
                                ignoreRow = true;
                            }
                        }
                    }

                    if (model.FiltroMeta.HasValue)
                    {
                        // Se o filtro da meta for difentente de "Todos" (-1)
                        if (model.FiltroMeta > -1)
                        {
                            // Se o filtro da meta for "Vazio" (0)
                            if (model.FiltroMeta == 0)
                            {
                                // Se a meta tiver algum valor ignoramos a linha
                                if (meta.HasValue)
                                {
                                    ignoreRow = true;
                                }
                            }
                            else if (model.FiltroMeta != meta)
                            {
                                // Se o filtro de meta for maior que 1 ele é igual a meta desejada
                                // e se eles forem diferentes ignoramos a linha
                                ignoreRow = true;
                            }
                        }
                        else if (model.FiltroMeta == -2)
                        {
                            // Se o filtro da meta for igual a "NA" (-2)
                            if (meta.HasValue)
                            {
                                // Se a meta tiver algum valor então ignoramos a linha
                                ignoreRow = true;
                            }
                        }
                    }
                }
            }

            return ignoreRow;
        }
    }
}
