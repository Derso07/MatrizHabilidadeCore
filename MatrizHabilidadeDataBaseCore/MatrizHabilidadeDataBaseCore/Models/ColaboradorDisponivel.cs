using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("ColaboradorDisponivel")]
    public class ColaboradorDisponivel
    {
        public int Id { get; set; }

        public DateTime Data { get; set; }

        public int ColaboradorId { get; set; }

        public virtual Colaborador Colaborador { get; set; }
    }
}