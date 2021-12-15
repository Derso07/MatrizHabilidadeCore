using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("PerguntaAuditoria")]
    public class PerguntaAuditoria
    {
        public int Id { get; set; }

        public bool IsOK { get; set; }

        public int AuditoriaId { get; set; }

        public virtual Auditoria Auditoria { get; set; }

        public int PerguntaId { get; set; }

        public virtual Pergunta Pergunta { get; set; }
    }
}