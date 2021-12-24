using MatrizHabilidadeDatabase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("Coordenador")]
    public class Coordenador 
    {
        public int UsuarioId { get; set; }

        public int AreaId { get; set; }

        public virtual Area Area { get; set; }

        public List<HistoricoCoordenador> HistoricoCoordenadores { get; set; }

        public virtual List<Uniorg> Uniorgs { get; set; }

        public virtual List<CadastroCoordenador> CadastroCoordenadores { get; set; }

        public virtual List<PlanoAcao> ResponsavelPlanosAcao { get; set; }

        public virtual List<PlanoAcao> CriadorPlanosAcao { get; set; }

        public virtual List<Retreinamento> Retreinamentos { get; set; }

        public virtual List<PlanoAcao> PlanoAcoes { get; set; }


    }
}