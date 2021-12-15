using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Treinamento")]
    public class Treinamento
    {
        public Treinamento()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime DataCriacao { get; set; }

        public int TipoTreinamentoId { get; set; }

        public virtual TipoTreinamento TipoTreinamento { get; set; }

        public virtual List<TurmaTreinamento> TurmasTreinamento { get; set; }

        public virtual List<ConfiguracaoIntegracaoTreinamento> Configuracoes { get; set; }
    }
}