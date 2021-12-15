using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("CadastroCoordenador")]
    public class CadastroCoordenador
    {
        public int Id { get; set; }

        public int CoordenadorId { get; set; }

        public int MaquinaId { get; set; }

        public bool IsAtivo { get; set; }

        public virtual Coordenador Coordenador { get; set; }

        public virtual Maquina Maquina { get; set; }
    }
}