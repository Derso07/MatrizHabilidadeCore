using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    public class Situacao
    {
        public const int Normal = 0;

        public const int Afastado = 1;

        public static int GetSituacao(string value)
        {
            if (value.Trim().ToLower() == "normal")
            {
                return Normal;
            }
            else
            {
                return Afastado;
            }
        }
    }

    public class Usuario : IdentityUser
    {
        public Usuario() 
        {
            IsAtivo = true;
        }

        public Usuario(Coordenador coordenador)
        {
            Nome = coordenador.Nome;
            Chapa = coordenador.Chapa;
            Email = coordenador.Email;
            Situacao = coordenador.Situacao;
            Funcao = coordenador.Funcao;
            IsAtivo = coordenador.IsAtivo;

            IsHorista = coordenador.IsHorista;
            IsCoordenador = coordenador.IsCoordenador;
            IsCLT = coordenador.IsCLT;
            IsManutencao = coordenador.IsManutencao;
            DataAdmissao = coordenador.DataAdmissao;
        }

        public Usuario (Colaborador colaborador)
        {
            Nome = colaborador.Nome;
            Chapa = colaborador.Chapa;
            Email = colaborador.Email;
            Situacao = colaborador.Situacao;
            Funcao = colaborador.Funcao;
            IsAtivo = colaborador.IsAtivo;

            IsHorista = colaborador.IsHorista;
            IsCoordenador = colaborador.IsCoordenador;
            IsCLT = colaborador.IsCLT;
            IsManutencao = colaborador.IsManutencao;
            DataAdmissao = colaborador.DataAdmissao;
        }
        public int? ColaboradorId { get; set; }

        public int? CoordenadorId { get; set; }

        public bool? IsCoordenador { get; set; }

        public string Login { get; set; }

        public string Nome { get; set; }

        public string Chapa { get; set; }

        public string Funcao { get; set; }

        public int Situacao { get; set; }

        [NotMapped]
        public bool IsHorista { get; set; }

        [NotMapped]
        public bool IsCLT { get; set; }

        [NotMapped]
        public bool IsManutencao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime? DataAdmissao { get; set; }

        public virtual Colaborador Colaborador { get; set; }

        public virtual Coordenador Coordenador { get; set; }
    }
}
