using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Pergunta")]
    public class Pergunta
    {
        public int Id { get; set; }

        public string Descricao { get; set; }

        public int Peso { get; set; }

        public virtual List<PerguntaAuditoria> Perguntas { get; set; }
    }
}