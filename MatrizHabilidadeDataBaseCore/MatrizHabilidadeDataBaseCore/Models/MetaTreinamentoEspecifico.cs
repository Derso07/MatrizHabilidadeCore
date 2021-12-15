using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("MetaTreinamentoEspecifico")]
    public class MetaTreinamentoEspecifico
    {
        public int Id { get; set; }

        public int? Meta { get; set; }

        public DateTime DataLancamento { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public virtual TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        public int ColaboradorId { get; set; }

        public virtual Colaborador Colaborador { get; set; }
    }
}