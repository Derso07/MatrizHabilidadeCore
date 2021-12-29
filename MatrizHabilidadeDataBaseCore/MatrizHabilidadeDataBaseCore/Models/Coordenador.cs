using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Coordenador")]
    public class Coordenador
    {
        public Coordenador() { }

        public Coordenador(Usuario usuario)
        {
            Login = usuario.Login;
            Nome = usuario.Nome;
            Chapa = usuario.Chapa;
            Email = usuario.Email;
            Situacao = usuario.Situacao;
            Funcao = usuario.Funcao;
            IsAtivo = usuario.IsAtivo;

            IsHorista = usuario.IsHorista;
            IsCoordenador = usuario.IsCoordenador;
            IsCLT = usuario.IsCLT;
            IsManutencao = usuario.IsManutencao;
            DataAdmissao = usuario.DataAdmissao;
        }
        public int Id { get; set; }

        public string Login { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Chapa { get; set; }

        public string Funcao { get; set; }

        public int Situacao { get; set; }

        [NotMapped]
        public bool IsHorista { get; set; }

        [NotMapped]
        public bool? IsCoordenador { get; set; }

        [NotMapped]
        public bool IsCLT { get; set; }

        [NotMapped]
        public bool IsManutencao { get; set; }

        public bool IsAtivo { get; set; }

        public DateTime? DataAdmissao { get; set; }

        public int AreaId { get; set; }

        public virtual Area Area { get; set; }

        public List<HistoricoCoordenador> HistoricoCoordenadores { get; set; }

        public virtual List<Uniorg> Uniorgs { get; set; }

        public virtual List<CadastroCoordenador> CadastroCoordenadores { get; set; }

        public virtual List<PlanoAcao> ResponsavelPlanosAcao { get; set; }

        public virtual List<PlanoAcao> CriadorPlanosAcao { get; set; }

        public virtual List<Retreinamento> Retreinamentos { get; set; }

        public NivelAcesso NivelAcesso { get; set; }
        public Usuario Usuarios { get; set; }
    }
}