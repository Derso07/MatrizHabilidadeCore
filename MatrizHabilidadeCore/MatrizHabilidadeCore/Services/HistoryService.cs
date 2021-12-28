using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeDatabase.Services
{
    public class HistoryService
    {
        private readonly DataBaseContext _db;
        public HistoryService(DataBaseContext db)
        {
            _db = db;
        }
        public async Task<bool> UpdateHistory()
        {
            try
            {
                var gapCalculator = new GAPCalculatorService();
                var auxiliaryTableService = new AuxiliaryTableService(_db);
                await auxiliaryTableService.UpdateAuxiliaryTables();

                var date = DateTime.Now;
                date = date.AddMonths(-1);
                date = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                var tiposTreinamento = _db.TiposTreinamentos.Select(t => t.Id).ToList();

                var coordenadores = _db.Coordenadores
                    .Where(c => c.Uniorgs.Any())
                    .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas.Where(m => m.IsAtivo)).Any())
                    .Where(c => c.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                    .ToList();

                var maquinas = _db.Maquinas.Where(m => m.IsAtivo).ToList();

                var historicoCalculator = new HistoricoCalculatorService(_db);

                await PopularHistorico(coordenadores, maquinas, date, TipoHistorico.Auditoria, historicoCalculator.AuditoriaMaquina, historicoCalculator.AuditoriaCoordenador);
                await PopularHistorico(coordenadores, maquinas, date, TipoHistorico.Treinamento, historicoCalculator.TreinamentoMaquina, historicoCalculator.TreinamentoCoordenador);
                await PopularHistorico(coordenadores, maquinas, date, TipoHistorico.Conhecimento, historicoCalculator.ConhecimentoMaquina, historicoCalculator.ConhecimentoCoordenador);
                await PopularHistorico(coordenadores, maquinas, date, TipoHistorico.Reducao, historicoCalculator.ReducaoMaquina, historicoCalculator.ReducaoCoordenador);
                await PopularHistorico(coordenadores, maquinas, date, TipoHistorico.Instrutores, historicoCalculator.InstrutoresMaquina, historicoCalculator.InstrutoresCoordenador);

                return true;
            }
            catch (Exception e)
            {
                _db.Erros.Add(new Error(e, "HistoryService - UpdateHistory"));
                await _db.SaveChangesAsync();
            }

            return false;
        }

        private async Task<bool> PopularHistorico(
            List<Coordenador> coordenadores,
            List<Maquina> maquinas,
            DateTime date,
            TipoHistorico tipo,
            Func<Maquina, int, int, double> funcaoMaquina,
            Func<Coordenador, int, int, double> funcaoCoordenador)
        {
            var hasHistorico = _db.Historicos
                .Where(h => h.Date == date)
                .Where(h => h.Tipo == tipo);

            if (hasHistorico.Any())
            {
                return false;
            }

            #region Maquina
            foreach (var maquina in maquinas)
            {
                var hasHistoricoMaquina = _db.HistoricoMaquinas
                    .Where(h => h.DataCorrespondente.Year == date.Year)
                    .Where(h => h.DataCorrespondente.Month == date.Month)
                    .Where(h => h.Tipo == tipo)
                    .Where(h => h.MaquinaId == maquina.Id);

                if (hasHistoricoMaquina.Any())
                {
                    continue;
                }

                _db.HistoricoMaquinas.Add(new HistoricoMaquina()
                {
                    DataCorrespondente = date,
                    MaquinaId = maquina.Id,
                    Valor = funcaoMaquina(maquina, date.Year, date.Month),
                    Tipo = tipo,
                });

                await _db.SaveChangesAsync();
            }
            #endregion

            #region Coordenador
            foreach (var coordenador in coordenadores)
            {
                var hasHistoricoCoordenador = _db.HistoricoCoordenadores
                    .Where(h => h.DataCorrespondente.Year == date.Year)
                    .Where(h => h.DataCorrespondente.Month == date.Month)
                    .Where(h => h.Tipo == tipo)
                    .Where(h => h.CoordenadorId == coordenador.Id);

                if (hasHistoricoCoordenador.Any())
                {
                    continue;
                }

                _db.HistoricoCoordenadores.Add(new HistoricoCoordenador()
                {
                    CoordenadorId = coordenador.Id,
                    DataCorrespondente = date,
                    Tipo = tipo,
                    Valor = funcaoCoordenador(coordenador, date.Year, date.Month),
                    AreaId = coordenador.AreaId,
                });
                await _db.SaveChangesAsync();
            }
            #endregion

            _db.Historicos.Add(new Historico()
            {
                Date = date,
                Tipo = tipo,
            });

            await _db.SaveChangesAsync();

            return true;
        }
    }
}