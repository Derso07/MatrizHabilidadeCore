using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("AnoFiscal")]
    public class AnoFiscal
    {
        public int Id { get; set; }

        public int Ano { get; set; }
    }
}