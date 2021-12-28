using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using System;
using System.Linq;

namespace MatrizHabilidadeDatabase.Services
{
    public class HistoricoCalculatorService
    {
        private readonly DataBaseContext _db;

        public HistoricoCalculatorService(DataBaseContext db)
        {
            _db = db;
        }

        public double GetHistorico(TipoHistorico tipo, Maquina maquina, int year, int month)
        {

            if (tipo == TipoHistorico.Auditoria)
            {
                return AuditoriaMaquina(maquina, year, month);
            }
            else if (tipo == TipoHistorico.Conhecimento)
            {
                return ConhecimentoMaquina(maquina, year, month);
            }
            else if (tipo == TipoHistorico.Instrutores)
            {
                return InstrutoresMaquina(maquina, year, month);
            }
            else if (tipo == TipoHistorico.Reducao)
            {
                return ReducaoMaquina(maquina, year, month);
            }
            else
            {
                return TreinamentoMaquina(maquina, year, month);
            }
        }

        public double GetHistorico(TipoHistorico tipo, Coordenador coordenador, int year, int month)
        {

            if (tipo == TipoHistorico.Auditoria)
            {
                return AuditoriaCoordenador(coordenador, year, month);
            }
            else if (tipo == TipoHistorico.Conhecimento)
            {
                return ConhecimentoCoordenador(coordenador, year, month);
            }
            else if (tipo == TipoHistorico.Instrutores)
            {
                return InstrutoresCoordenador(coordenador, year, month);
            }
            else if (tipo == TipoHistorico.Reducao)
            {
                return ReducaoCoordenador(coordenador, year, month);
            }
            else
            {
                return TreinamentoCoordenador(coordenador, year, month);
            }
        }

        #region Coordenador
        public double AuditoriaCoordenador(Coordenador coordenador, int year, int month)
        {
            return coordenador.Uniorgs
                .SelectMany(m => m.Colaboradores)
                .SelectMany(c => c.Auditorias)
                .Where(a => a.DataLancamento.Year == year)
                .Where(a => a.DataLancamento.Month == month)
                .GroupBy(a => new { a.ColaboradorId, a.TreinamentoEspecificoId })
                .Count();
        }

        public double TreinamentoCoordenador(Coordenador coordenador, int year, int month)
        {
            GAPCalculatorService gapCalculator = new GAPCalculatorService();

            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var maquinas = coordenador.Uniorgs.SelectMany(u => u.Maquinas).Select(m => m.Id).ToList();

            var treinamentosArea = _db.ViewTreinamentos
                .Where(t => maquinas.Contains(t.MaquinaId))
                .Where(t => !t.Data.HasValue || t.Data.Value <= date)
                .ToList();

            var treinamentosEspecificosArea = _db.ViewTreinamentosEspecificos
                .Where(t => maquinas.Contains(t.MaquinaId))
                .Where(t => !t.DataTreinamento.HasValue || t.DataTreinamento.Value <= date)
                .Where(t => !t.IsNA)
                .ToList();

            var gap = gapCalculator.Compute(null, null, treinamentosArea, treinamentosEspecificosArea);

            if (gap.Geral.Treinamento < 100)
            {
                return Convert.ToInt32(gap.Geral.Treinamento);
            }
            else
            {
                return 0;
            }
        }

        public double ConhecimentoCoordenador(Coordenador coordenador, int year, int month)
        {
            GAPCalculatorService gapCalculator = new GAPCalculatorService();

            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var maquinas = coordenador.Uniorgs.SelectMany(u => u.Maquinas).Select(m => m.Id).ToList();

            var treinamentosArea = _db.ViewTreinamentos
                .Where(t => maquinas.Contains(t.MaquinaId))
                .Where(t => !t.Data.HasValue || t.Data.Value <= date)
                .ToList();

            var treinamentosEspecificosArea = _db.ViewTreinamentosEspecificos
                .Where(t => maquinas.Contains(t.MaquinaId))
                .Where(t => !t.DataTreinamento.HasValue || t.DataTreinamento.Value <= date)
                .Where(t => !t.IsNA)
                .ToList();

            var gap = gapCalculator.Compute(null, null, treinamentosArea, treinamentosEspecificosArea);

            if (gap.Geral.Treinamento < 100)
            {
                return Convert.ToInt32(gap.Geral.Conhecimento);
            }
            else
            {
                return 0;
            }
        }

        public double ReducaoCoordenador(Coordenador coordenador, int year, int month)
        {
            var dataAtual = new DateTime(year, month, 1);
            dataAtual = new DateTime(dataAtual.Year, dataAtual.Month, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));

            var dataAnterior = dataAtual.AddMonths(-1);
            dataAnterior = new DateTime(dataAnterior.Year, dataAnterior.Month, DateTime.DaysInMonth(dataAnterior.Year, dataAnterior.Month));

            var maquinas = coordenador.Uniorgs.SelectMany(u => u.Maquinas).Select(m => m.Id).ToList();

            var atual = _db.HistoricoMaquinas
                .Where(h => maquinas.Contains(h.MaquinaId))
                .Where(h => h.DataCorrespondente == dataAtual)
                .Where(h => h.Tipo == TipoHistorico.Conhecimento);

            var anterior = _db.HistoricoMaquinas
                .Where(h => maquinas.Contains(h.MaquinaId))
                .Where(h => h.DataCorrespondente == dataAnterior)
                .Where(h => h.Tipo == TipoHistorico.Conhecimento);

            if (atual.Any() && anterior.Any())
            {
                return anterior.First().Valor - atual.First().Valor;
            }
            else
            {
                return 0;
            }
        }

        public double InstrutoresCoordenador(Coordenador coordenador, int year, int month)
        {
            return _db.Colaboradores
                .Where(c => c.IsFacilitador)
                .Where(c => c.Uniorg.CoordenadorId == coordenador.Id)
                .Count();
        }
        #endregion

        #region Máquina
        public double AuditoriaMaquina(Maquina maquina, int year, int month)
        {
            return maquina.Uniorgs
                .SelectMany(m => m.Colaboradores)
                .SelectMany(c => c.Auditorias)
                .Where(a => a.DataLancamento.Year == year)
                .Where(a => a.DataLancamento.Month == month)
                .GroupBy(a => new { a.ColaboradorId, a.TreinamentoEspecificoId })
                .Count();
        }

        public double TreinamentoMaquina(Maquina maquina, int year, int month)
        {
            GAPCalculatorService gapCalculator = new GAPCalculatorService();

            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var treinamentosArea = _db.ViewTreinamentos
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(t => !t.Data.HasValue || t.Data.Value <= date)
                .ToList();

            var treinamentosEspecificosArea = _db.ViewTreinamentosEspecificos
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(t => !t.DataTreinamento.HasValue || t.DataTreinamento.Value <= date)
                .Where(t => !t.IsNA)
                .ToList();

            var gap = gapCalculator.Compute(null, null, treinamentosArea, treinamentosEspecificosArea);

            if (gap.Geral.Treinamento < 100)
            {
                return Convert.ToInt32(gap.Geral.Treinamento);
            }
            else
            {
                return 0;
            }
        }

        public double ConhecimentoMaquina(Maquina maquina, int year, int month)
        {
            GAPCalculatorService gapCalculator = new GAPCalculatorService();

            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var treinamentosArea = _db.ViewTreinamentos
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(t => !t.Data.HasValue || t.Data.Value <= date)
                .ToList();

            var treinamentosEspecificosArea = _db.ViewTreinamentosEspecificos
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(t => !t.DataTreinamento.HasValue || t.DataTreinamento.Value <= date)
                .Where(t => !t.IsNA)
                .ToList();

            var gap = gapCalculator.Compute(null, null, treinamentosArea, treinamentosEspecificosArea);

            if (gap.Geral.Treinamento < 100)
            {
                return Convert.ToInt32(gap.Geral.Conhecimento);
            }
            else
            {
                return 0;
            }
        }

        public double ReducaoMaquina(Maquina maquina, int year, int month)
        {
            var dataAtual = new DateTime(year, month, 1);
            dataAtual = new DateTime(dataAtual.Year, dataAtual.Month, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));

            var dataAnterior = dataAtual.AddMonths(-1);
            dataAnterior = new DateTime(dataAnterior.Year, dataAnterior.Month, DateTime.DaysInMonth(dataAnterior.Year, dataAnterior.Month));

            var atual = _db.HistoricoMaquinas
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(h => h.DataCorrespondente == dataAtual)
                .Where(h => h.Tipo == TipoHistorico.Conhecimento);

            var anterior = _db.HistoricoMaquinas
                .Where(t => t.MaquinaId == maquina.Id)
                .Where(h => h.DataCorrespondente == dataAnterior)
                .Where(h => h.Tipo == TipoHistorico.Conhecimento);

            if (atual.Any() && anterior.Any())
            {
                return anterior.First().Valor - atual.First().Valor;
            }
            else
            {
                return 0;
            }
        }

        public double InstrutoresMaquina(Maquina maquina, int year, int month)
        {
            return _db.Colaboradores
                .Where(c => c.IsFacilitador)
                .Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquina.Id))
                .Count();
        }
        #endregion
    }
}