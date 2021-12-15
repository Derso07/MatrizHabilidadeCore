using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    public enum StatusAcao
    {
        Nenhum = 0,
        Concluido = 1,
        EmAndamento = 2,
        Atrasado = 3,
        ConcluidoAtraso = 4,
    }


    [Table("planoacao")]
    public class PlanoAcao
    {
        public int Id { get; set; }

        public string Acao { get; set; }

        public int? ColaboradorResponsavelId { get; set; }

        public int? CoordenadorResponsavelId { get; set; }

        public DateTime Prazo { get; set; }

        public int ColaboradorId { get; set; }

        public int? CriadorId { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataConclusao { get; set; }

        public int TreinamentoEspecificoId { get; set; }

        public virtual Coordenador CoordenadorResponsavel { get; set; }

        public virtual Colaborador ColaboradorResponsavel { get; set; }

        public virtual Colaborador Colaborador { get; set; }

        public virtual Coordenador Criador { get; set; }

        public virtual TreinamentoEspecifico TreinamentoEspecifico { get; set; }

        public StatusAcao Status
        {
            get
            {
                var data = DateTime.Now;

                if (DataConclusao.HasValue)
                {
                    if (DataConclusao.Value <= Prazo)
                    {
                        return StatusAcao.Concluido;
                    }

                    return StatusAcao.ConcluidoAtraso;
                }
                else if (data > Prazo)
                {
                    return StatusAcao.Atrasado;
                }
                else
                {
                    return StatusAcao.EmAndamento;
                }
            }
        }
    }
}