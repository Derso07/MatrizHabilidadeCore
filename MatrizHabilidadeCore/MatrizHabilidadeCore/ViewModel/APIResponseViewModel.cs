using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatrizHabilidade.ViewModel
{
    public class APIResponseViewModel
    {
        public string KeyUsuario { get; set; }

        public string LoginUsuario { get; set; }

        public string EmailUsuario { get; set; }

        public string NomeUsuario { get; set; }

        public string ChapaUsuario { get; set; }

        public string KeyTreinamento { get; set; }

        public string NomeTreinamento { get; set; }

        public float QuantidadePerguntas { get; set; }

        public float QuantidadePerguntasAcertadas { get; set; }

        public List<Pergunta> Perguntas { get; set; }

        public class Pergunta
        {
            public string Key { get; set; }

            public string Text { get; set; }

            public bool IsAcertado { get; set; }

            public List<Resposta> Respostas { get; set; }

            public class Resposta
            {
                public string Key { get; set; }

                public string Text { get; set; }

                public bool IsCorreta { get; set; }
            }
        }
    }
}