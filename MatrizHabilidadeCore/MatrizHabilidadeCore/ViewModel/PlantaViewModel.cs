using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;

namespace MatrizHabilidade.ViewModel
{
    public class PlantaViewModel : ViewModelBase
    {
        private readonly DataBaseContext _db;

        public PlantaViewModel(int plantaId, DataBaseContext db, int ano_selecionado)
        {
            db = _db;
            Areas = new List<LinhaGraficoFarol>();
            Conhecimento = new ProgressBar();
            Treinamento = new ProgressBar();

                var date = new DateTime(ano_selecionado, 3, DateTime.DaysInMonth(ano_selecionado, 3));
                var gapCalculator = new GAPCalculatorService();

                var tiposTreinamento = db.TiposTreinamentos
                    .Where(t => t.Treinamentos.Any())
                    .ToList();

                var areas = db.Areas
                    .Where(a => a.PlantaId == plantaId)
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                    .Where(a => a.Id != 1)
                    .ToList();

                if (areas.Count.Equals(1))
                {
                    RequireRedirect = true;
                    Parameter = Encrypting.Encrypt(areas[0].Id.ToString());
                }
                else
                {
                    List<int> areaIds = areas
                        .Select(a => a.Id)
                        .ToList();

                    var treinamentos = db.ViewTreinamentos
                        .Where(t => t.PlantaId == plantaId)
                        .Where(t => areaIds.Contains(t.AreaId))
                        .ToList();

                    var treinamentosEspecificos = db.ViewTreinamentosEspecificos
                        .Where(t => t.PlantaId == plantaId)
                        .Where(t => areaIds.Contains(t.AreaId))
                        .Where(t => !t.IsNA)
                        .ToList();

                    var data = gapCalculator.Compute(date, tiposTreinamento.Select(t => t.Id).ToList(), treinamentos, treinamentosEspecificos);

                    Conhecimento.Media = data.Geral.Conhecimento;
                    Conhecimento.Treinamentos = new Dictionary<string, double>()
                    {
                        { "Específico", data.Especifico.Conhecimento },
                    };

                    foreach (var tipoTreinamento in data.TipoTreinamentos)
                    {
                        var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                        Conhecimento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Conhecimento);
                    }

                    Treinamento.Media = data.Geral.Treinamento;
                    Treinamento.Treinamentos = new Dictionary<string, double>()
                    {
                        { "Específico", data.Especifico.Treinamento },
                    };

                    foreach (var tipoTreinamento in data.TipoTreinamentos)
                    {
                        var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                        Treinamento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Treinamento);
                    }

                    for (var x = 0; x < areas.Count; x++)
                    {
                        var treinamentosArea = treinamentos
                            .Where(t => t.AreaId == areas[x].Id)
                            .ToList();

                        var treinamentosEspecificosArea = treinamentosEspecificos
                            .Where(t => t.AreaId == areas[x].Id)
                            .ToList();

                        data = gapCalculator.Compute(date, tiposTreinamento.Select(t => t.Id).ToList(), treinamentosArea, treinamentosEspecificosArea);

                        var linha = new LinhaGraficoFarol()
                        {
                            Id = Encrypting.Encrypt(areas[x].Id.ToString()),
                            Descricao = areas[x].Alias,
                        };

                        linha.Auditoria = new LinhaGraficoFarol.Farol()
                        {
                            Media = data.Geral.Conhecimento,
                            Treinamentos = new Dictionary<string, double>()
                        {
                            { "Específico", data.Especifico.Conhecimento },
                        },
                        };

                        foreach (var tipoTreinamento in data.TipoTreinamentos)
                        {
                            var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                            linha.Auditoria.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Conhecimento);
                        }

                        linha.Treinamento = new LinhaGraficoFarol.Farol()
                        {
                            Media = data.Geral.Treinamento,
                            Treinamentos = new Dictionary<string, double>()
                        {
                            { "Específico", data.Especifico.Treinamento },
                        },
                        };

                        foreach (var tipoTreinamento in data.TipoTreinamentos)
                        {
                            var descricaoTipoTreinamento = tiposTreinamento.Where(t => t.Id == tipoTreinamento.Key).FirstOrDefault().Descricao;

                            linha.Treinamento.Treinamentos.Add(descricaoTipoTreinamento, tipoTreinamento.Value.Treinamento);
                        }

                        Areas.Add(linha);
                    }
                }
            }

        public bool RequireRedirect { get; set; }

        public string Parameter { get; set; }

        public string PathAndQuery { get; set; }

        public List<LinhaGraficoFarol> Areas { get; set; }

        public ProgressBar Conhecimento { get; set; }

        public ProgressBar Treinamento { get; set; }
    }
}