using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Auditoria")]
    public class Auditoria
    {
        public Auditoria()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public DateTime DataLancamento { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public virtual TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        public int ColaboradorId { get; set; }

        public virtual Colaborador Colaborador { get; set; }

        public virtual List<PerguntaAuditoria> Perguntas { get; set; }

        public float Nota { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime? DataInicio { get; set; }
    }
}