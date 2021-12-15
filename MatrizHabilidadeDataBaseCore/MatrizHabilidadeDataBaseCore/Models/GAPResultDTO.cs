using System.Collections.Generic;

namespace MatrizHabilidadeDatabase.Models
{
    public class GAPResultDTO
    {
        public TipoGAP Geral { get; set; }

        public Dictionary<int, TipoGAP> TipoTreinamentos { get; set; }

        public TipoGAP Especifico { get; set; }

        public class TipoGAP
        {
            public float Conhecimento { get; set; }

            public float Treinamento { get; set; }
        }
    }
}