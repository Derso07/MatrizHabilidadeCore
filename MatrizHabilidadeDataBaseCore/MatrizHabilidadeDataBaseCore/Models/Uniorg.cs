using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Uniorg")]
    public class Uniorg
    {
        public int Id { get; set; }

        public string UniorgNumero { get; set; }

        public string DescricaoUniorg { get; set; }

        public int CoordenadorId { get; set; }

        public int CoordenadorOriginalId { get; set; }

        public virtual Coordenador Coordenador { get; set; }

        public virtual List<Maquina> Maquinas { get; set; }

        public virtual List<Colaborador> Colaboradores { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}