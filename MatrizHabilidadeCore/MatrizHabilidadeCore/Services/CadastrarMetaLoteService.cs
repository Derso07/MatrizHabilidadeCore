using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Linq;

namespace MatrizHabilidade.Services
{
    public class CadastrarMetaLoteService
    {
        private readonly DataBaseContext _db;

        public CadastrarMetaLoteService(DataBaseContext db)
        {
            _db = db;
        }
        public enum Result
        {
            Success = 0,
            IncorrectExcelFormat = 1,
            InternalError = 2,
            NoDataFound = 3,
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
                    else if (row.ZeroHeight)
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

                    Planta planta = new Planta();
                    Area area = new Area();
                    Maquina maquina = new Maquina();
                    Colaborador colaborador = new Colaborador();
                    TreinamentoEspecifico treinamentoEspecifico = new TreinamentoEspecifico();
                    string meta = "";

                    while (true)
                    {
                        var cell = row.GetCell(columnIndex);

                        value = cell.StringCellValue;

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

                        if (type == Coluna.Planta)
                        {
                            planta.Descricao = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Area)
                        {
                            area.Alias = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Maquina)
                        {
                            maquina.Descricao = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Chapa)
                        {
                            colaborador.Chapa = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Nome)
                        {
                            colaborador.Nome = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Padrao)
                        {
                            treinamentoEspecifico.Descricao = value;
                            columnsFound++;
                        }
                        else if (type == Coluna.Meta)
                        {
                            meta = value;
                            columnsFound++;
                        }

                        columnIndex++;
                    };

                    if (columnsFound < 7)
                    {
                        return Result.IncorrectExcelFormat;
                    }

                        #region Planta
                        var plantaQuery = _db.Plantas.Where(u => u.Descricao == planta.Descricao);

                        if (plantaQuery.Any())
                        {
                            planta = plantaQuery.First();
                        }
                        else
                        {
                            planta = null;
                        }
                        #endregion

                        if (planta != null)
                        {
                            #region Area
                            var areaQuery = _db.Areas.Where(u => u.Alias == area.Alias);

                            if (areaQuery.Any())
                            {
                                area = areaQuery.First();
                            }
                            else
                            {
                                area = null;
                            }
                            #endregion

                            if (area != null)
                            {
                                #region Maquina
                                var maquinaQuery = _db.Maquinas.Where(m => m.Descricao == maquina.Descricao);

                                if (maquinaQuery.Any())
                                {
                                    maquina = maquinaQuery.First();
                                }
                                else
                                {
                                    maquina = null;
                                }
                                #endregion

                                if (maquina != null)
                                {
                                    #region Usuario
                                    var usuarioQuery = _db.Colaboradores.Where(c => c.Chapa == colaborador.Chapa);

                                    if (usuarioQuery.Any())
                                    {
                                        colaborador = usuarioQuery.First();
                                    }
                                    else
                                    {
                                        colaborador = null;
                                    }
                                    #endregion

                                    if (colaborador != null)
                                    {
                                        #region Treinamento Específico
                                        var treinamentoEspecificoQuery = _db.TreinamentosEspecificos
                                            .Where(t => t.Maquinas.Any(m => m.Id == maquina.Id))
                                            .Where(t => t.Descricao == treinamentoEspecifico.Descricao);

                                        if (treinamentoEspecificoQuery.Any())
                                        {
                                            treinamentoEspecifico = treinamentoEspecificoQuery.First();
                                        }
                                        else
                                        {
                                            treinamentoEspecifico = null;
                                        }
                                        #endregion

                                        if (treinamentoEspecifico != null)
                                        {
                                            int? _meta = null;

                                            if (int.TryParse(meta, out int aux))
                                            {
                                                _meta = aux;
                                            }

                                            _db.MetasTreinamentosEspecificos.Add(new MetaTreinamentoEspecifico()
                                            {
                                                ColaboradorId = colaborador.Id,
                                                TreinamentoEspecificoId = treinamentoEspecifico.Id,
                                                DataLancamento = DateTime.Now,
                                                Meta = _meta,
                                            });

                                            _db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                
                return Result.Success;
            }
            catch (Exception e)
            {
                    _db.Erros.Add(new Error(e, "CadastrarMetaLoteService"));
                    _db.SaveChanges();
            }

            return Result.InternalError;
        }

        private class Coluna
        {
            public const int Vazio = -2;

            public const int Nenhum = -1;

            public const int Planta = 0;

            public const int Area = 2;

            public const int Maquina = 3;

            public const int Chapa = 4;

            public const int Nome = 5;

            public const int Padrao = 6;

            public const int Meta = 7;

            public static int GetColumnType(IRow row, int columnIndex)
            {
                var cell = row.Sheet.GetRow(0).GetCell(columnIndex);

                if (cell == null)
                {
                    return Vazio;
                }

                var value = cell.StringCellValue;

                if (value.Trim() == "Planta")
                {
                    return Planta;
                }
                else if (value.Trim() == "Área")
                {
                    return Area;
                }
                else if (value.Trim() == "Máquina")
                {
                    return Maquina;
                }
                else if (value.Trim() == "Chapa")
                {
                    return Chapa;
                }
                else if (value.Trim() == "Nome")
                {
                    return Nome;
                }
                else if (value.Trim() == "Padrão")
                {
                    return Padrao;
                }
                else if (value.Trim() == "Meta")
                {
                    return Meta;
                }

                return Nenhum;
            }
        }
    }
}