using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("PlantaBDados")]
    public class PlantaBDados
    {
        public int Id { get; set; }

        public string Descricao { get; set; }

        public virtual List<Planta> Plantas { get; set; }
    }
}