using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("configuracaointegracaotreinamento")]
    public class ConfiguracaoIntegracaoTreinamento
    {
        public ConfiguracaoIntegracaoTreinamento()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public int Target { get; set; }

        public int TreinamentoId { get; set; }

        public virtual Treinamento Treinamento { get; set; }

        public bool IsAtivo { get; set; }
    }
}