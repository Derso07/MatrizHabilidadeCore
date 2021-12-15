using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("UniorgMaquina")]
    public class UniorgMaquina
    {
        public int Id { get; set; }

        public int UniorgId { get; set; }

        public int MaquinaId { get; set; }

        public virtual Uniorg Uniorg { get; set; }

        public virtual Maquina Maquina { get; set; }

        [NotMapped]
        public bool ShoudRemain { get; set; }
    }
}