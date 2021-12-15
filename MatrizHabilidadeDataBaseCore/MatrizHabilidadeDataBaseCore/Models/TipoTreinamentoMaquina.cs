using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TipoTreinamentoMaquina")]
    public class TipoTreinamentoMaquina
    {
        public int Id { get; set; }

        public int TipoTreinamentoId { get; set; }

        public int MaquinaId { get; set; }

        public virtual TipoTreinamento TipoTreinamento { get; set; }

        public virtual Maquina Maquina { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}