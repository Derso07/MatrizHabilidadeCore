using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("NotaTreinamento")]
    public class NotaTreinamento
    {
        public int Id { get; set; }

        public int Nota { get; set; }

        public int TurmaTreinamentoId { get; set; }

        public virtual TurmaTreinamento TurmaTreinamento { get; set; }

        public int ColaboradorId { get; set; }

        public virtual Colaborador Colaborador { get; set; }
    }
}