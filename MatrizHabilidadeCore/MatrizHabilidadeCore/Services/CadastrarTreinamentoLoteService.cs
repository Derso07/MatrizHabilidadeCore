using MatrizHabilidadeDatabase.Models;
using NPOI.SS.UserModel;
using System;
using System.Linq;
using System.IO;
using NPOI.XSSF.UserModel;
using MatrizHabilidadeCore.Utility;
using MatrizHabilidadeDataBaseCore;

namespace MatrizHabilidade.Services
{
    public class CadastrarTreinamentoLoteService
    {
        private readonly DataBaseContext _db;
        public CadastrarTreinamentoLoteService(DataBaseContext db)
        {
            _db = db;
        }
        private enum TipoTreinamento
        {
            None = -1,
            WCM = 0,
            Qualidade = 1,
            Especifico = 2,
        }

        public enum Result
        {
            Success = 0,
            IncorrectExcelFormat = 1,
            InternalError = 2,
            NoDataFound = 3,
        }

        private class Coluna
        {
            public const int Vazio = -2;

            public const int Nenhum = -1;

            public const int IdAlternativo = 0;

            public const int TituloTreinamento = 4;

            public const int DataInicial = 5;

            public const int Pontuacao = 6;

            public const int NumeroLocalizador = 7;

            public static int GetColumnType(IRow row, int columnIndex)
            {
                var cell = row.Sheet.GetRow(0).GetCell(columnIndex);

                if (cell == null)
                {
                    return Vazio;
                }

                var value = cell.GetStringValue();

                if (value.Trim().Contains("Alternativo_ID"))
                {
                    return IdAlternativo;
                }
                else if (value.Trim().Contains("Título do Treinamento"))
                {
                    return TituloTreinamento;
                }
                else if (value.Trim().Contains("Data Inicial do Treinamento"))
                {
                    return DataInicial;
                }
                else if (value.Trim().Contains("Pontuação"))
                {
                    return Pontuacao;
                }
                else if (value.Trim().Contains("Número do Localizador do Treinamento"))
                {
                    return NumeroLocalizador;
                }

                return Nenhum;
            }
        }

        public Result ReadFile(Stream stream)
        {
            try
            {
                IWorkbook book = new XSSFWorkbook(stream);
                ISheet sheet = book.GetSheetAt(0);
                IRow row;

                for (int rowIndex = 1; true; rowIndex++)
                {
                    row = sheet.GetRow(rowIndex);

                    if (row == null)
                    {
                        if (rowIndex == 1)
                        {
                            return Result.NoDataFound;
                        }

                        break;
                    }
                    else if (row.LastCellNum == -1)
                    {
                        if (rowIndex == 1)
                        {
                            return Result.NoDataFound;
                        }

                        break;
                    }

                    string value;
                    int columnIndex = 0;
                    int emptyCounter = 0;

                    int columnsFound = 0;

                    TipoTreinamento tipoTreinamento = TipoTreinamento.None;
                    string tituloTreinamento = "";
                    var colaborador = new Colaborador();
                    DateTime data = default;
                    int pontuacao = 1;
                    string numeroLocalizador = "";

                    while (true)
                    {
                        var cell = row.GetCell(columnIndex);

                        value = cell.GetStringValue();

                        var type = Coluna.GetColumnType(row, columnIndex);

                        // Se for encontrado mais de 5 células vazias para-se a leitura da linha
                        if (string.IsNullOrEmpty(value))
                        {
                            if (emptyCounter >= 5)
                            {
                                break;
                            }
                            else
                            {
                                emptyCounter++;
                                columnIndex++;
                                continue;
                            }
                        }
                        else
                        {
                            emptyCounter = 0;
                        }

                        if (type == Coluna.IdAlternativo)
                        {
                            colaborador.Chapa = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.TituloTreinamento)
                        {
                            if (value.Contains("NSA MH WCM"))
                            {
                                tipoTreinamento = TipoTreinamento.WCM;
                                tituloTreinamento = value.Replace("NSA MH WCM -", "").Replace(" - Modulo PCS", "").Trim();
                            }
                            else
                            {
                                if (value.Contains("NSA MH Qualidade"))
                                {
                                    tipoTreinamento = TipoTreinamento.Qualidade;
                                    tituloTreinamento = value.Replace("NSA MH Qualidade -", "").Trim();
                                }
                                else if (value.Contains("NSA MH"))
                                {
                                    tipoTreinamento = TipoTreinamento.Especifico;
                                    tituloTreinamento = value.Replace("NSA MH", "").Replace(":", "").Trim();
                                }
                                else
                                {
                                    tipoTreinamento = TipoTreinamento.None;
                                }
                            }
                            columnsFound++;
                        }
                        else if (type == Coluna.DataInicial)
                        {
                            if(Convert.ToDateTime(value) != null)
                            {
                                data = Convert.ToDateTime(value);
                            }
                            else
                            {
                                data = DateTime.MinValue;
                            }

                            //data = Convert.ToDateTime(value) ?? DateTime.MinValue;
                            columnsFound++;
                        }
                        else if (type == Coluna.Pontuacao)
                        {
                            pontuacao = Convert.ToInt32(value);
                            columnsFound++;
                        }
                        else if (type == Coluna.NumeroLocalizador)
                        {
                            numeroLocalizador = value;
                            columnsFound++;
                        }

                        columnIndex++;
                    };

                    if (columnsFound < 5)
                    {
                        return Result.IncorrectExcelFormat;
                    }

                        if (tipoTreinamento != TipoTreinamento.None)
                        {
                            var colaboradorQuery = _db.Colaboradores.Where(c => c.Chapa == colaborador.Chapa);

                            if (colaboradorQuery.Any())
                            {
                                colaborador = colaboradorQuery.FirstOrDefault();

                                if (tipoTreinamento == TipoTreinamento.Especifico)
                                {
                                    TurmaTreinamentoEspecifico turma = null;

                                    var turmaQuery = _db.TurmasTreinamentosEspecificos.Where(t => t.NumeroLocalizador == numeroLocalizador);

                                    if (turmaQuery.Any())
                                    {
                                        turma = turmaQuery.FirstOrDefault();
                                    }
                                    else
                                    {
                                        var treinamentoQuery = _db.TreinamentosEspecificos.Where(t => t.Descricao == tituloTreinamento);

                                        if (treinamentoQuery.Any())
                                        {
                                            var treinamento = treinamentoQuery.First();

                                            turma = new TurmaTreinamentoEspecifico()
                                            {
                                                NumeroLocalizador = numeroLocalizador,
                                                DataRealizacao = data,
                                                DataLancamento = DateTime.Now,
                                                TreinamentoEspecificoId = treinamento.Id,
                                            };

                                            _db.TurmasTreinamentosEspecificos.Add(turma);
                                            _db.SaveChanges();
                                        }
                                    }

                                    if (turma != null)
                                    {
                                        _db.TurmasTreinamentosEspecificosColaboradores.Add(new TurmaTreinamentoEspecificoColaborador()
                                        {
                                            IsAtivo = true,
                                            ColaboradorId = colaborador.Id,
                                            TurmaTreinamentoEspecificoId = turma.Id,
                                        });
                                        _db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    TurmaTreinamento turma = null;

                                    var turmaQuery = _db.TurmasTreinamentos.Where(t => t.NumeroLocalizador == numeroLocalizador);

                                    if (turmaQuery.Any())
                                    {
                                        turma = turmaQuery.FirstOrDefault();
                                    }
                                    else
                                    {
                                        var treinamentoQuery = _db.Treinamentos.Where(t => t.Descricao == tituloTreinamento);

                                        if (treinamentoQuery.Any())
                                        {
                                            var treinamento = treinamentoQuery.First();

                                            turma = new TurmaTreinamento()
                                            {
                                                NumeroLocalizador = numeroLocalizador,
                                                DataRealizacao = data,
                                                DataLancamento = DateTime.Now,
                                                TreinamentoId = treinamento.Id,
                                            };

                                            _db.TurmasTreinamentos.Add(turma);
                                            _db.SaveChanges();
                                        }
                                    }

                                    if (turma != null)
                                    {
                                        _db.NotasTreinamentos.Add(new NotaTreinamento()
                                        {
                                            Nota = pontuacao,
                                            TurmaTreinamentoId = turma.Id,
                                            ColaboradorId = colaborador.Id,
                                        });
                                        _db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }

                return Result.Success;
            }
            catch (Exception e)
            {
                    _db.Erros.Add(new Error(e, "CadastrarTreinamentoLoteService"));
                    _db.SaveChanges();
            }

            return Result.InternalError;
        }
    }
}