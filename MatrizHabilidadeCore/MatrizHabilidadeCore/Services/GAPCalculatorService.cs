using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrizHabilidadeDatabase.Services
{
    public class GAPCalculatorService
    {
        public GAPResultDTO Compute(DateTime? date, List<int> tipos, List<ViewTreinamento> treinamentos, List<ViewTreinamentoEspecifico> treinamentosEspecificos)
        {
            var result = new GAPResultDTO();

            if (!date.HasValue)
            {
                date = DateTime.MaxValue;
            }

            float totalTreinamento = 0;
            float totalTreinamentoEspecifico = 0;

            float totalTreinamentoOk = 0;
            float totalTreinamentoEspecificoOk = 0;

            #region Geral
            result.Geral = new GAPResultDTO.TipoGAP();

            #region Conhecimento
            totalTreinamento = treinamentos
                .Where(t => t.Nota.HasValue)
                .Where(t => t.Data <= date)
                .Sum(t => t.Nota.Value);

            totalTreinamentoEspecifico = treinamentosEspecificos
                .Where(t => t.Nota.HasValue)
                .Where(t => t.DataAuditoria <= date)
                .Sum(t => t.Nota).Value;

            totalTreinamentoOk = treinamentos
                .Sum(t => t.Meta);

            totalTreinamentoEspecificoOk = treinamentosEspecificos
                .Where(t => t.Meta.HasValue)
                .ToList()
                .Sum(t => t.Meta.Value);

            result.Geral.Conhecimento = (1 - (totalTreinamento + totalTreinamentoEspecifico) / (totalTreinamentoOk + totalTreinamentoEspecificoOk)) * 100;

            if (float.IsNaN(result.Geral.Conhecimento))
            {
                result.Geral.Conhecimento = 100;
            }
            #endregion

            #region Treinamento
            totalTreinamento = treinamentos
                .Where(t => !t.IsOK || t.Data > date)
                .Count();

            totalTreinamentoEspecifico = treinamentosEspecificos
                .Where(t => !t.IsOK || t.DataTreinamento > date)
                .Count();

            totalTreinamentoOk = treinamentos.Count();
            totalTreinamentoEspecificoOk = treinamentosEspecificos.Count();

            result.Geral.Treinamento = (totalTreinamento + totalTreinamentoEspecifico) / (totalTreinamentoOk + totalTreinamentoEspecificoOk) * 100;

            if (float.IsNaN(result.Geral.Treinamento))
            {
                result.Geral.Treinamento = 100;
            }
            #endregion
            #endregion

            #region Específico
            result.Especifico = new GAPResultDTO.TipoGAP();

            #region Conhecimento
            totalTreinamento = treinamentos
                .Where(t => t.Nota.HasValue)
                .Where(t => t.Data <= date)
                .Sum(t => t.Nota.Value);

            totalTreinamentoEspecifico = treinamentosEspecificos
                .Where(t => t.Nota.HasValue)
                .Where(t => t.DataAuditoria <= date)
                .Sum(t => t.Nota).Value;

            totalTreinamentoOk = treinamentos
                .Sum(t => t.Meta);

            totalTreinamentoEspecificoOk = treinamentosEspecificos
                .Where(t => t.Meta.HasValue)
                .ToList()
                .Sum(t => t.Meta.Value);

            result.Especifico.Conhecimento = (1 - totalTreinamentoEspecifico / totalTreinamentoEspecificoOk) * 100;

            if (float.IsNaN(result.Especifico.Conhecimento))
            {
                result.Especifico.Conhecimento = 100;
            }
            #endregion

            #region Treinamento
            totalTreinamentoEspecifico = treinamentosEspecificos
                .Where(t => !t.IsOK || t.DataTreinamento > date)
                .Count();

            totalTreinamentoEspecificoOk = treinamentosEspecificos.Count();

            result.Especifico.Treinamento = totalTreinamentoEspecifico / totalTreinamentoEspecificoOk * 100;

            if (float.IsNaN(result.Especifico.Treinamento))
            {
                result.Especifico.Treinamento = 100;
            }
            #endregion
            #endregion

            if (tipos != null)
            {
                result.TipoTreinamentos = new Dictionary<int, GAPResultDTO.TipoGAP>();

                foreach (var tipo in tipos)
                {
                    var tipoGAP = new GAPResultDTO.TipoGAP();

                    #region Conhecimento

                    totalTreinamento = treinamentos
                        .Where(t => t.TipoTreinamentoId == tipo)
                        .Where(t => t.Nota.HasValue)
                        .Where(t => t.Data <= date)
                        .Sum(t => t.Nota.Value);

                    totalTreinamentoOk = treinamentos
                        .Where(t => t.TipoTreinamentoId == tipo)
                        .Sum(t => t.Meta);

                    tipoGAP.Conhecimento = (1 - totalTreinamento / totalTreinamentoOk) * 100;

                    if (float.IsNaN(tipoGAP.Conhecimento))
                    {
                        tipoGAP.Conhecimento = 100;
                    }

                    #endregion

                    #region Treinamento

                    totalTreinamento = treinamentos
                        .Where(t => t.TipoTreinamentoId == tipo)
                        .Where(t => !t.IsOK || t.Data > date)
                        .Count();

                    totalTreinamentoOk = treinamentos
                        .Where(t => t.TipoTreinamentoId == tipo)
                        .Count();

                    tipoGAP.Treinamento = totalTreinamento / totalTreinamentoOk * 100;

                    if (float.IsNaN(tipoGAP.Treinamento))
                    {
                        tipoGAP.Treinamento = 100;
                    }

                    #endregion

                    if (!result.TipoTreinamentos.ContainsKey(tipo))
                    {
                        result.TipoTreinamentos.Add(tipo, tipoGAP);
                    }
                }
            }

            return result;
        }

        public GAPResultDTO Compute(Dictionary<int, GAPMetadataDTO> treinamentos,  GAPMetadataDTO treinamentoEspecifico)
        {
            var result = new GAPResultDTO()
            {
                Especifico = new GAPResultDTO.TipoGAP(),
                Geral = new GAPResultDTO.TipoGAP(),
                TipoTreinamentos = new Dictionary<int, GAPResultDTO.TipoGAP>(),
            };

            float somatoriaNota = 0;
            float somatoriaMeta = 0;
            float somatoriaPresenca = 0;
            float somatoriaTreinamentos = 0;

            float gapConhecimento = 0;
            float gapTreinamento = 0;

            foreach (var tipoTreinamentoId in treinamentos.Keys)
            {
                if (!result.TipoTreinamentos.ContainsKey(tipoTreinamentoId))
                {
                    result.TipoTreinamentos.Add(tipoTreinamentoId, new GAPResultDTO.TipoGAP());
                }

                somatoriaNota = treinamentos[tipoTreinamentoId].SomatoriaNota;
                somatoriaMeta = treinamentos[tipoTreinamentoId].SomatoriaMeta;
                somatoriaPresenca = treinamentos[tipoTreinamentoId].SomatoriaPresenca;
                somatoriaTreinamentos = treinamentos[tipoTreinamentoId].SomatoriaTreinamentos;

                gapConhecimento = (1 - somatoriaNota / somatoriaMeta) * 100;
                gapTreinamento = (1 - somatoriaPresenca / somatoriaTreinamentos) * 100;

                result.TipoTreinamentos[tipoTreinamentoId].Conhecimento = gapConhecimento;
                result.TipoTreinamentos[tipoTreinamentoId].Treinamento = gapTreinamento;
            }

            somatoriaNota = treinamentoEspecifico.SomatoriaNota;
            somatoriaMeta = treinamentoEspecifico.SomatoriaMeta;
            somatoriaPresenca = treinamentoEspecifico.SomatoriaPresenca;
            somatoriaTreinamentos = treinamentoEspecifico.SomatoriaTreinamentos;

            gapConhecimento = (1 - somatoriaNota / somatoriaMeta) * 100;
            gapTreinamento = (1 - somatoriaPresenca / somatoriaTreinamentos) * 100;

            result.Especifico.Conhecimento = gapConhecimento;
            result.Especifico.Treinamento = gapTreinamento;

            somatoriaNota = treinamentos.Sum(g => g.Value.SomatoriaNota) + treinamentoEspecifico.SomatoriaNota;
            somatoriaMeta = treinamentos.Sum(g => g.Value.SomatoriaMeta) + treinamentoEspecifico.SomatoriaMeta;
            somatoriaPresenca = treinamentos.Sum(g => g.Value.SomatoriaPresenca) + treinamentoEspecifico.SomatoriaPresenca;
            somatoriaTreinamentos = treinamentos.Sum(g => g.Value.SomatoriaTreinamentos) + treinamentoEspecifico.SomatoriaTreinamentos;

            result.Geral.Conhecimento = (1 - somatoriaNota / somatoriaMeta) * 100;
            result.Geral.Treinamento = (1 - somatoriaPresenca / somatoriaTreinamentos) * 100;

            return result;
        }
    }
}