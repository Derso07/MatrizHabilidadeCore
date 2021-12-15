using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TurmaTreinamentoEspecifico")]
    public class TurmaTreinamentoEspecifico
    {
        public int Id { get; set; }

        public DateTime DataLancamento { get; set; }

        public DateTime DataRealizacao { get; set; }

        public string NumeroLocalizador { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public virtual TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        public virtual List<TurmaTreinamentoEspecificoColaborador> TurmaColaboradores { get; set; }
    }
}