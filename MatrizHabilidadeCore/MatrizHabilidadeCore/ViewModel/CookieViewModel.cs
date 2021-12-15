using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.ViewModel
{
    public class CookieViewModel
    {
        public int Usu_id { get; set; }

        public string Usu_nome { get; set; }

        public string Equipes { get; set; }

        public bool IsEscola { get; set; }

        public int Fab_id { get; set; }

        public string Fab_descricao { get; set; }

        public bool Usu_mudar_senha { get; set; }

        public int Usu_acesso { get; set; }

        public int Usu_eqp_id { get; set; }

        public bool IsAnnaliseHabilitado { get; set; }

        public int TempoLogin { get; set; }

        public bool IsDemonstracao { get; set; }

        public string Usu_login { get; set; }

        public int Tre_id { get; set; }

        public int Teu_id { get; set; }

        public int? AnnaliseId { get; set; }

        public string AnnaliseNome { get; set; }

        public TipoMediaRealizada TipoMediaRealizada { get; set; }

        public bool ExibirTreinamentosDesabilitados { get; set; }

        public string Usu_email { get; set; }
    }
    public enum TipoMediaRealizada
    {
        Ultimo = 0,
        Last2Months = 1,
        Last3Months = 2,
        Media = 3,
    }
}
