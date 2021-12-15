using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("TreinamentoEspecifico")]
    public class TreinamentoEspecifico
    {
        public TreinamentoEspecifico()
        {
            IsAtivo = true;
        }

        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime DataCriacao { get; set; }

        public int CategoriaId { get; set; }

        public virtual Categoria Categoria { get; set; }

        public virtual List<MetaTreinamentoEspecifico> MetasAuditoria { get; set; }

        public virtual List<TurmaTreinamentoEspecifico> TurmasTreinamentos { get; set; }

        public virtual List<Maquina> Maquinas { get; set; }

        public virtual List<Auditoria> Auditorias { get; set; }

        public virtual List<PlanoAcao> PlanosAcao { get; set; }

        public virtual List<Retreinamento> Retreinamentos { get; set; }
    }
}