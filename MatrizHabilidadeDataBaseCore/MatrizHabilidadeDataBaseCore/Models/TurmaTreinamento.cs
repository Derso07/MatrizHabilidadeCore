using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TurmaTreinamento")]
    public class TurmaTreinamento
    {
        public int Id { get; set; }

        public DateTime DataRealizacao { get; set; }

        public DateTime DataLancamento { get; set; }

        public string NumeroLocalizador { get; set; }

        public int TreinamentoId { get; set; }

        public virtual Treinamento Treinamento { get; set; }

        public virtual List<NotaTreinamento> Notas { get; set; }
    }
}