using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("MaquinaTreinamentoEspecifico")]
    public class MaquinaTreinamentoEspecifico
    {
        public int Id { get; set; }

        public int MaquinaId { get; set; }

        public virtual Maquina Maquina { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public virtual TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}