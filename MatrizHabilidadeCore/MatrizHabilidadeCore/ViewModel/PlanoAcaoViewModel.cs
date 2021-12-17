using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;

namespace MatrizHabilidade.ViewModel
{
    public class PlanoAcaoViewModel
    {
        public PlanoAcaoViewModel()
        {
            Plantas = new SelectViewModel("Todas", "Todas", true);
            Areas = new SelectViewModel("Todas", "Todas", true);
            Maquinas = new SelectViewModel("Todas", "Todas", true);
            SiteMap = new List<string>();
            Status = new List<int>();
        }

        public string ReturnUrl { get; set; }

        public string PlantasKey { get; set; }

        public string AreasKey { get; set; }

        public string[] MaquinasKey { get; set; }

        public double GAPAcaoCorretiva { get; set; }

        public string PathAndQuery { get; set; }

        public List<string> SiteMap { get; set; }

        public GraficoPie GraficoAcompanhamento { get; set; }

        public StatusAtividades TabelaStatusAtividade { get; set; }

        public List<Linha> TabelaPlanoAcao { get; set; }

        public class StatusAtividades
        {
            public int Total { get; set; }

            public int Concluida { get; set; }

            public int PorcentagemConcluida
            {
                get
                {
                    var result = (float)Concluida / (float)Total * 100;

                    if (double.IsNaN(result))
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            public int Atrasada { get; set; }

            public int PorcentagemAtrasada
            {
                get
                {
                    var result = (float)Atrasada / (float)Total * 100;

                    if (double.IsNaN(result))
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            public int Andamento { get; set; }

            public int PorcentagemAndamento
            {
                get
                {
                    var result = (float)Andamento / (float)Total * 100;

                    if (double.IsNaN(result))
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            public int ConcluidaAtraso { get; set; }

            public int PorcentagemConcluidaAtraso
            {
                get
                {
                    var result = (float)ConcluidaAtraso / (float)Total * 100;

                    if (double.IsNaN(result))
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
        }

        public class Linha
        {
            public string Key { get; set; }

            public string Area { get; set; }

            public string Maquina { get; set; }

            public string Coordenador { get; set; }

            public int Index { get; set; }

            public string Acao { get; set; }

            public string Responsavel { get; set; }

            public string Prazo { get; set; }

            public string Status { get; set; }

            public bool HasEvidencia { get; set; }

            public bool IsResponsavel { get; set; }

            public string StatusClass { get; set; }

            public string DataCriacao { get; set; }
        }

        public SelectViewModel Plantas { get; set; }

        public SelectViewModel Areas { get; set; }

        public SelectViewModel Maquinas { get; set; }

        public List<int> Status { get; set; }
    }

    public class FiltroPlanoAcaoViewModel
    {
        public FiltroPlanoAcaoViewModel()
        {
            Areas = new SelectViewModel();
            Maquinas = new SelectViewModel();
        }

        public SelectViewModel Areas { get; set; }

        public SelectViewModel Maquinas { get; set; }
    }

    public enum FileStatus
    {
        Enviando = 1,
        Concluido = 2,
        Error = 3,
    };
}