using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatrizHabilidade.ViewModel
{
    public class CoordenadorViewModel : ViewModelBase
    {
        public Grafico Auditorias { get; set; }

        public Grafico Treinamentos { get; set; }

        public Grafico Conhecimento { get; set; }

        public Grafico Reducao { get; set; }

        public Grafico Instrutores { get; set; }

        public Grafico AuditoriasExportacao { get; set; }

        public Grafico TreinamentosExportacao { get; set; }

        public Grafico ConhecimentoExportacao { get; set; }

        public Grafico ReducaoExportacao { get; set; }

        public Grafico InstrutoresExportacao { get; set; }

        public string PathAndQuery { get; set; }
    }
}