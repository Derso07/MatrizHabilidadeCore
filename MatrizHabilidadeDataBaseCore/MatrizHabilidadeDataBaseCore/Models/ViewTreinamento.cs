using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("treinamento_complete")]
    public class ViewTreinamento
    {
        [Key, Column(Order = 0)]
        public int PlantaId { get; set; }

        [Key, Column(Order = 1)]
        public int AreaId { get; set; }

        [Key, Column(Order = 2)]
        public int CoordenadorId { get; set; }

        [Key, Column(Order = 3)]
        public int MaquinaId { get; set; }

        [Key, Column(Order = 4)]
        public int ColaboradorId { get; set; }

        [Key, Column(Order = 5)]
        public int TreinamentoId { get; set; }

        [Key, Column(Order = 6)]
        public int TipoTreinamentoId { get; set; }

        public int Meta { get; set; }

        public int? Nota { get; set; }

        public DateTime? Data { get; set; }

        public bool IsOK { get; set; }
    }
}