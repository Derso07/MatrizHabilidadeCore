using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("evidenciaplanoacao")]
    public class EvidenciaPlanoAcao
    {
        public int Id { get; set; }

        public string Caminho { get; set; }

        public int PlanoAcaoId { get; set; }

        public virtual PlanoAcao PlanoAcao { get; set; }
    }
}