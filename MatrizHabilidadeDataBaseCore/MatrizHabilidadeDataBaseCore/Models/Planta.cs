using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Planta")]
    public class Planta
    {
        public Planta()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public int? PlantaBDadosId { get; set; }

        public virtual PlantaBDados PlantaBDados { get; set; }

        public virtual List<Area> Areas { get; set; }
    }
}