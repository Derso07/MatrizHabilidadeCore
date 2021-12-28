using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    public enum TipoHistorico
    {
        Auditoria = 0,
        Treinamento = 1,
        Conhecimento = 2,
        Reducao = 3,
        Instrutores = 4,
    }

    [Table("Historico")]
    public class Historico
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TipoHistorico Tipo { get; set; }
    }

    [Table("HistoricoMaquina")]
    public class HistoricoMaquina
    {
        public HistoricoMaquina()
        {
            DataCongelamento = DateTime.Now;
        }

        [Key, Column(Order = 0)]
        public TipoHistorico Tipo { get; set; }

        [Key, Column(Order = 1)]
        public DateTime DataCorrespondente { get; set; }

        public double Valor { get; set; }

        [Key, Column(Order = 2)]
        public int MaquinaId { get; set; }

        public Maquina Maquina { get; set; }

        public DateTime? DataCongelamento { get; set; }
    }

    [Table("HistoricoCoordenador")]
    public class HistoricoCoordenador
    {
        public HistoricoCoordenador()
        {
            DataCongelamento = DateTime.Now;
        }

        [Key, Column(Order = 0)]
        public TipoHistorico Tipo { get; set; }

        [Key, Column(Order = 1)]
        public DateTime DataCorrespondente { get; set; }

        public double Valor { get; set; }

        [Key, Column(Order = 2)]
        public int CoordenadorId { get; set; }

        [Key, Column(Order = 3)]
        public int AreaId { get; set; }

        public Area Area { get; set; }

        public Coordenador Coordenador { get; set; }

        public DateTime? DataCongelamento { get; set; }
    }
}