using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Colaborador")]
    public class Colaborador
    {
        public Colaborador() { }

        public int Id { get; set; }

        public string Login { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Chapa { get; set; }

        public string Funcao { get; set; }

        public int Situacao { get; set; }

        [NotMapped]
        public bool IsHorista { get; set; }

        [NotMapped]
        public bool IsCoordenador { get; set; }

        [NotMapped]
        public bool IsCLT { get; set; }

        [NotMapped]
        public bool IsManutencao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime? DataAdmissao { get; set; }

        public int? UniorgId { get; set; }

        public virtual Uniorg Uniorg { get; set; }

        public virtual List<NotaTreinamento> NotasTreinamento { get; set; }

        public virtual List<MetaTreinamentoEspecifico> MetasAuditoria { get; set; }

        public virtual List<TurmaTreinamentoEspecificoColaborador> TurmasTreinamentoEspecifico { get; set; }

        public virtual List<Auditoria> Auditorias { get; set; }

        public virtual List<ColaboradorDisponivel> ColaboradoresDisponiveis { get; set; }

        public virtual List<PlanoAcao> PlanosAcaoResponsavel { get; set; }

        public virtual List<PlanoAcao> PlanosAcoes { get; set; }

        public virtual List<Retreinamento> Retreinamentos { get; set; }

        public bool IsFacilitador { get; set; }

        public NivelAcesso NivelAcesso { get; set; }
    }
}