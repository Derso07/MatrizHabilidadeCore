using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;

namespace MatrizHabilidade.ViewModel
{
    public class PlantaViewModel : ViewModelBase
    {
        public bool RequireRedirect { get; set; }

        public string Parameter { get; set; }

        public string PathAndQuery { get; set; }

        public List<LinhaGraficoFarol> Areas { get; set; }

        public ProgressBar Conhecimento { get; set; }

        public ProgressBar Treinamento { get; set; }
    }
}