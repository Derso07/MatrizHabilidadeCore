using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Area")]
    public class Area
    {
        public int Id { get; set; }

        public string Descricao { get; set; }

        public string Alias { get; set; }

        public int PlantaId { get; set; }

        public virtual Planta Planta { get; set; }

        public virtual List<Coordenador> Coordenadores { get; set; }

        public virtual List<Maquina> Maquinas { get; set; }
    }
}