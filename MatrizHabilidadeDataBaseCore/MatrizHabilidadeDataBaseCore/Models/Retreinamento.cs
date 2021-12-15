using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Retreinamento")]
    public class Retreinamento
    {
        public int Id { get; set; }

        public int CoordenadorId { get; set; }

        public Coordenador Coordenador { get; set; }

        public int ColaboradorId { get; set; }

        public Colaborador Colaborador { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        public DateTime Date { get; set; }
    }
}
