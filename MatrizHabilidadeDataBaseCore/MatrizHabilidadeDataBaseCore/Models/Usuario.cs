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

    public class Usuario : IdentityUser<int>
    {
        public Usuario() 
        {
            IsAtivo = true;
        }

        public string Login { get; set; }

        public string Nome { get; set; }

        public string Chapa { get; set; }

        public string Funcao { get; set; }

        public int Situacao { get; set; }

        [NotMapped]
        public bool IsHorista { get; set; }

        [NotMapped]
        public bool IsCoordenador { get; set; }

        [NotMapped]
        public bool IsCLT { get; set; }

        [NotMapped]
        public bool IsManutencao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime? DataAdmissao { get; set; }

        [Column("usu_acesso")]
        public int UsuarioAcesso { get; set; }

        public virtual Coordenador Coordenador { get; set; }

        public virtual Colaborador Colaborador { get; set; }
    }


}
