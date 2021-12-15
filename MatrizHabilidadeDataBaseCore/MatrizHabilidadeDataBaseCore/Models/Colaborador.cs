using MatrizHabilidadeDatabase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Colaborador")]
    public class Colaborador
    {

        public int UsuarioId { get; set; }

        public int? UniorgId { get; set; }

        public virtual Uniorg Uniorg { get; set; }

        public virtual List<NotaTreinamento> NotasTreinamento { get; set; }

        public virtual List<MetaTreinamentoEspecifico> MetasAuditoria { get; set; }

        public virtual List<TurmaTreinamentoEspecificoColaborador> TurmasTreinamentoEspecifico { get; set; }

        public virtual List<Auditoria> Auditorias { get; set; }

        public virtual List<ColaboradorDisponivel> ColaboradoresDisponiveis { get; set; }

        public virtual List<PlanoAcao> PlanosAcaoResponsavel { get; set; }

        public virtual List<PlanoAcao> PlanosAcao { get; set; }

        public virtual List<Retreinamento> Retreinamentos { get; set; }

        public virtual Usuario Usuario { get; set; }

        public bool IsFacilitador { get; set; }

    }
}