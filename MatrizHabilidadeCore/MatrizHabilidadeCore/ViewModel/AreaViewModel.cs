using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MatrizHabilidade.ViewModel
{
    public class AreaViewModel : ViewModelBase
    {
        private readonly DataBaseContext _db;
        private readonly CookieService _cookieService;
        public AreaViewModel(int areaId, DataBaseContext db, CookieService cookieService)
        {
            _db = db;
            _cookieService = cookieService;

            Maquinas = new List<LinhaGraficoFarol>();
            Conhecimento = new ProgressBar();
            Treinamento = new ProgressBar();

                var date = new DateTime(cookieService.AnoSelecionado, 3, DateTime.DaysInMonth(cookieService.AnoSelecionado, 3));
                var gapCalculator = new GAPCalculatorService();

                var tiposTreinamento = db.TiposTreinamentos
                    .Where(t => t.Treinamentos.Any())
                    .ToList();

                var maquinas = db.Maquinas
                    .Where(m => m.AreaId == areaId)
                    .Where(m => m.Uniorgs.Any())
                    .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                    .OrderByDescending(m => m.Descricao)
                    .ToList();

                List<int> maquinaIds = maquinas
                    .Select(m => m.Id)
                    .ToList();

                var treinamentos = db.ViewTreinamentos
                    .Where(t => t.AreaId == areaId)
                    .Where(t => maquinaIds.Contains(t.MaquinaId))
                    .ToList();

                var treinamentosEspecificos = db.ViewTreinamentosEspecificos
                    .Where(t => t.AreaId == areaId)
                    .Where(t => maquinaIds.Contains(t.MaquinaId))
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

                for (var x = 0; x < maquinas.Count; x++)
                {
                    var treinamentosArea = treinamentos
                        .Where(t => t.MaquinaId == maquinas[x].Id)
                        .ToList();

                    var treinamentosEspecificosArea = treinamentosEspecificos
                        .Where(t => t.MaquinaId == maquinas[x].Id)
                        .ToList();

                    data = gapCalculator.Compute(date, tiposTreinamento.Select(t => t.Id).ToList(), treinamentosArea, treinamentosEspecificosArea);

                    var linha = new LinhaGraficoFarol()
                    {
                        Id = Encrypting.Encrypt(maquinas[x].Id.ToString()),
                        Descricao = maquinas[x].Descricao,
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

                    Maquinas.Add(linha);
                }
            }

        public List<LinhaGraficoFarol> Maquinas { get; set; }

        public ProgressBar Conhecimento { get; set; }

        public ProgressBar Treinamento { get; set; }

        public bool IsRedirected { get; set; }
    }
}