using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TurmaTreinamentoEspecificoColaborador")]
    public class TurmaTreinamentoEspecificoColaborador
    {
        public TurmaTreinamentoEspecificoColaborador()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public int TurmaTreinamentoEspecificoId { get; set; }

        public TurmaTreinamentoEspecifico TurmaTreinamentoEspecifico { get; set; }

        public int ColaboradorId { get; set; }

        public virtual Colaborador Colaborador { get; set; }

        public bool IsAtivo { get; set; }
    }
}