using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Maquina")]
    public class Maquina
    {
        public Maquina()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public int? CategoriaId { get; set; }
        
        public bool HasError { get; set; }

        public int? AreaId { get; set; }

        public virtual Area Area { get; set; }

        public virtual Categoria Categoria { get; set; }

        public virtual List<TreinamentoEspecifico> TreinamentoEspecificos { get; set; }

        public virtual List<Uniorg> Uniorgs { get; set; }

        public virtual List<HistoricoMaquina> HistoricoMaquinas { get; set; }

        public virtual List<TipoTreinamento> TiposTreinamento { get; set; }

        public virtual List<CadastroCoordenador> CadastroCoordenadores { get; set; }

        [NotMapped]
        public bool ShouldRemain { get; set; }
    }
}