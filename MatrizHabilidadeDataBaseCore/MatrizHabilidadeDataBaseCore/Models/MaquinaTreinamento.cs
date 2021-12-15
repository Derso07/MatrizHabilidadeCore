using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("MaquinaTreinamento")]
    public class MaquinaTreinamento
    {
        public int Id { get; set; }

        public int MaquinaId { get; set; }

        public virtual Maquina Maquina { get; set; }

        public int TreinamentoId { get; set; }

        public virtual Treinamento Treinamento { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}