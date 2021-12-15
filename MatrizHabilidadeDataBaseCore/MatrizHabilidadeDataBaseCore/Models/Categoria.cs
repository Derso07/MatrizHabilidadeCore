using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Categoria")]
    public class Categoria
    {
        public Categoria()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public virtual List<Maquina> Maquinas { get; set; }

        public virtual List<TipoTreinamento> TiposTreinamento { get; set; }

        public virtual List<TreinamentoEspecifico> TreinamentoEspecificos { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}