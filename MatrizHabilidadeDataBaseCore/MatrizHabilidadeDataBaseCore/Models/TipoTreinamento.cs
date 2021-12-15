using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TipoTreinamento")]
    public class TipoTreinamento
    {
        public TipoTreinamento()
        {
            IsAtivo = true;
            Maquinas = new List<Maquina>();
            Categorias = new List<Categoria>();
            Treinamentos = new List<Treinamento>();
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public string CorTitulo { get; set; }

        public string CorTreinamento { get; set; }

        public string CorFonte { get; set; }

        public virtual List<Categoria> Categorias { get; set; }

        public virtual List<Treinamento> Treinamentos { get; set; }

        public virtual List<Maquina> Maquinas { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}