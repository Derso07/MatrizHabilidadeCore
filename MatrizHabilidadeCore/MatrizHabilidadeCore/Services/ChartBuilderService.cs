using System;
using System.Collections.Generic;
using System.Linq;
using MatrizHabilidade.ViewModel;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;

namespace MatrizHabilidade.Services
{
    public class ChartBuilderService
    {
        private readonly string[] Colors = new string[] { "#00ADFB", "#A1DD00", "#DF7E26", "#0E2961", "#787F81", "#CA2024", "#00532A", "#1D428A", "#8F1626", "#00351F", };
        private readonly DataBaseContext _db;
        private readonly HistoricoCalculatorService _historicoCalculatorService;

        public ChartBuilderService(DataBaseContext db, HistoricoCalculatorService historicoCalculatorService)
        {
            _db = db;
            _historicoCalculatorService = historicoCalculatorService;
        }

        public List<Grafico.Serie> BuildCoordenadoresChart(
            List<Coordenador> coordenadores,
            int ano,
            int areaId,
            TipoHistorico tipo,
            List<Grafico.Serie> aditionalSeries)
        {
            var currentDate = DateTime.Now;
            var result = new List<Grafico.Serie>();

            for (var x = 0; x < coordenadores.Count; x++)
            {
                int coordenadorId = coordenadores[x].Id;

                var serie = new Grafico.Serie
                {
                    Name = coordenadores[x].Nome,
                };

                int month = 4;
                int year = ano - 1;

                for (int counter = 0; counter < 12; counter++)
                {
                    if (currentDate.Year < year)
                    {
                        serie.Data.Add(0);
                    }
                    else if (currentDate.Year == year && currentDate.Month < month)
                    {
                        serie.Data.Add(0);
                    }
                    else if (currentDate.Year == year && currentDate.Month - 1 == month)
                    {
                        var query = _db.HistoricoCoordenadores
                            .Where(h => h.Tipo == tipo)
                            .Where(h => h.CoordenadorId == coordenadorId)
                            .Where(h => h.AreaId == areaId)
                            .Where(h => h.DataCorrespondente.Year == year)
                            .Where(h => h.DataCorrespondente.Month == month);

                        if (query.Any())
                        {
                            var historico = query.First();

                            if (historico.Valor < 100)
                            {
                                serie.Data.Add(historico.Valor);
                            }
                            else
                            {
                                serie.Data.Add(0);
                            }
                        }
                        else if (coordenadores[x].Uniorgs.SelectMany(u => u.Maquinas).Any(m => m.AreaId == areaId))
                        {
                            serie.Data.Add(_historicoCalculatorService.GetHistorico(tipo, coordenadores[x], year, month));
                        }
                    }
                    else if (currentDate.Year == year && currentDate.Month == month)
                    {
                        if (coordenadores[x].Uniorgs.SelectMany(u => u.Maquinas).Any(m => m.AreaId == areaId))
                        {
                            serie.Data.Add(_historicoCalculatorService.GetHistorico(tipo, coordenadores[x], year, month));
                        }
                    }
                    else
                    {
                        var historico = _db.HistoricoCoordenadores
                            .Where(h => h.Tipo == tipo)
                            .Where(h => h.CoordenadorId == coordenadorId)
                            .Where(h => h.AreaId == areaId)
                            .Where(h => h.DataCorrespondente.Year == year)
                            .Where(h => h.DataCorrespondente.Month == month);

                        if (historico.Any())
                        {
                            serie.Data.Add(historico.First().Valor);
                        }
                        else
                        {
                            serie.Data.Add(0);
                        }
                    }

                    month += 1;

                    if (month > 12)
                    {
                        month = 1;
                        year += 1;
                    }
                }

                result.Add(serie);
            }

            if (aditionalSeries != null)
            {
                result.AddRange(aditionalSeries);
            }

            return result;
        }

        public List<Grafico.Serie> BuildMaquinasChart(
            Maquina maquina,
            int ano,
            TipoHistorico tipo,
            List<Grafico.Serie> aditionalSeries)
        {
            var currentDate = DateTime.Now;
            var result = new List<Grafico.Serie>();

            int maquinaId = maquina.Id;

            var serie = new Grafico.Serie
            {
                Name = maquina.Descricao,
                Color = Colors[0],
                Tipo = Grafico.Serie.TipoSerie.Column,
            };

            int month = 4;
            int year = ano - 1;

            for (int counter = 0; counter < 12; counter++)
            {
                if (currentDate.Year < year)
                {
                    serie.Data.Add(0);
                }
                else if (currentDate.Year == year && currentDate.Month < month)
                {
                    serie.Data.Add(0);
                }
                else if (currentDate.Year == year && currentDate.Month - 1 == month )
                {
                    var query = _db.HistoricoMaquinas
                        .Where(h => h.Tipo == tipo)
                        .Where(h => h.MaquinaId == maquinaId)
                        .Where(h => h.DataCorrespondente.Year == year)
                        .Where(h => h.DataCorrespondente.Month == month);

                    if (query.Any())
                    {
                        var historico = query.First();

                        if (historico.Valor < 100)
                        {
                            serie.Data.Add(historico.Valor);
                        }
                        else
                        {
                            serie.Data.Add(0);
                        }
                    }
                    else
                    {
                        serie.Data.Add(_historicoCalculatorService.GetHistorico(tipo, maquina, year, month));
                    }
                }
                else if (currentDate.Year == year && currentDate.Month == month)
                {
                    serie.Data.Add(_historicoCalculatorService.GetHistorico(tipo, maquina, year, month));
                }
                else
                {
                    var historico = _db.HistoricoMaquinas
                        .Where(h => h.Tipo == tipo)
                        .Where(h => h.MaquinaId == maquinaId)
                        .Where(h => h.DataCorrespondente.Year == year)
                        .Where(h => h.DataCorrespondente.Month == month);

                    if (historico.Any())
                    {
                        serie.Data.Add(historico.First().Valor);
                    }
                    else
                    {
                        serie.Data.Add(0);
                    }
                }

                month += 1;

                if (month > 12)
                {
                    month = 1;
                    year += 1;
                }
            }

            result.Add(serie);
            if (aditionalSeries != null)
            {
                result.AddRange(aditionalSeries);
            }

            return result;
        }
    }
}