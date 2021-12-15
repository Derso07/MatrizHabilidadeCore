using MatrizHabilidadeDatabase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrizHabilidadeDataBaseCore
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
        {

        }

        public DbSet<AnoFiscal> AnosFiscais { get; set; }

        public DbSet<Area> Areas { get; set; }

        public DbSet<Auditoria> Auditorias { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<CadastroCoordenador> CadastroCoordenadores { get; set; }

        public DbSet<ConfiguracaoIntegracaoTreinamento> Integracoes { get; set; }

        public DbSet<Colaborador> Colaboradores { get; set; }

        public DbSet<ColaboradorDisponivel> ColaboradoresDisponiveis { get; set; }

        public DbSet<Coordenador> Coordenadores { get; set; }

        public DbSet<Error> Erros { get; set; }

        public DbSet<EvidenciaPlanoAcao> Evidencias { get; set; }

        public DbSet<Historico> Historicos { get; set; }

        public DbSet<HistoricoMaquina> HistoricoMaquinas { get; set; }

        public DbSet<HistoricoCoordenador> HistoricoCoordenadores { get; set; }

        public DbSet<Maquina> Maquinas { get; set; }

        public DbSet<MetaTreinamentoEspecifico> MetasTreinamentosEspecificos { get; set; }

        public DbSet<NotaTreinamento> NotasTreinamentos { get; set; }

        public DbSet<Pergunta> Perguntas { get; set; }

        public DbSet<PerguntaAuditoria> PerguntasAuditorias { get; set; }

        public DbSet<ConsoleApplicationLog> ConsoleApplicationLogs { get; set; }

        public DbSet<PlanoAcao> PlanosAcao { get; set; }

        public DbSet<Planta> Plantas { get; set; }

        public DbSet<PlantaBDados> PlantasBDaddos { get; set; }

        public DbSet<TipoTreinamento> TiposTreinamentos { get; set; }

        public DbSet<Treinamento> Treinamentos { get; set; }

        public DbSet<TreinamentoEspecifico> TreinamentosEspecificos { get; set; }

        public DbSet<TurmaTreinamento> TurmasTreinamentos { get; set; }

        public DbSet<TurmaTreinamentoEspecifico> TurmasTreinamentosEspecificos { get; set; }

        public DbSet<TurmaTreinamentoEspecificoColaborador> TurmasTreinamentosEspecificosColaboradores { get; set; }

        public DbSet<Uniorg> Uniorgs { get; set; }

        public DbSet<UniorgMaquina> UniorgMaquinas { get; set; }

        public DbSet<ViewTreinamento> ViewTreinamentos { get; set; }

        public DbSet<ViewTreinamentoEspecifico> ViewTreinamentosEspecificos { get; set; }

        public DbSet<Retreinamento> Retreinamentos { get; set; }

        public DbSet<IntegrationLog> IntegrationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataBaseContext).Assembly);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasOne(a => a.Coordenador)
                .WithOne(b => b.Usuario)
                .HasForeignKey<Coordenador>(b => b.UsuarioId);

            modelBuilder.Entity<Usuario>()
                .HasOne(a => a.Colaborador)
                .WithOne(b => b.Usuario)
                .HasForeignKey<Colaborador>(b => b.UsuarioId);
        }
    }
}
