using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;

namespace MatrizHabilidade.ViewModel.Formulario
{
    public class AuditoriaViewModel
    {
        public AuditoriaViewModel()
        {
            Areas = new Dictionary<string, string>();
            Maquinas = new Dictionary<string, string>();
            Padroes = new Dictionary<string, string>();
            Perguntas = new Dictionary<string, string>();
        }

        public string UserName { get; set; }

        public string Email { get; set; }

        public Dictionary<string, string> Areas { get; set; }

        public Dictionary<string, string> Maquinas { get; set; }

        public Dictionary<string, string> Padroes { get; set; }

        public Dictionary<string, string> Perguntas { get; set; }

        public Dictionary<string, string> FiltroPlantaPlanoAcao { get; set; }

        public Dictionary<string, string> FiltroAreaPlanoAcao { get; set; }

        public Dictionary<string, string> ResponsavelPlanoAcao { get; set; }
    }

    public class CadastroAuditoriaViewModel
    {
        public CadastroAuditoriaViewModel()
        {
            Perguntas = new Dictionary<string, bool>();
        }

        public string Operador { get; set; }

        public string Padrao { get; set; }

        public bool HasRetreino { get; set; }

        public DateTime DataInicio { get; set; }

        public Dictionary<string, bool> Perguntas { get; set; }

        public List<Acao> Acoes { get; set; }

        public class Acao
        {
            public string Descricao { get; set; }

            public string ResponsavelPlanoAcao { get; set; }

            public DateTime Prazo { get; set; }

            public bool IsCoordenador { get; set; }
        }
    }

    public class AuditoriaResponse
    {
        public bool IsOK { get; set; }

        public int Meta { get; set; }

        public int Nota { get; set; }
    }

    public class MetaViewModel
    {
        public MetaViewModel()
        {
            Plantas = new List<Option>();
            Areas = new List<Option>();
            Maquinas = new List<Option>();
            TabelaMeta = new List<Row>();
            Padroes = new List<Option>();
            Profissionais = new List<Option>();

            Plantas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });

            Areas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });

            Maquinas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });

            Padroes.Add(new Option()
            {
                Key = "",
                Text = "Todos",
            });

            Profissionais.Add(new Option()
            {
                Key = "",
                Text = "Todos",
            });
        }

        public List<Option> Plantas { get; set; }

        public List<Option> Areas { get; set; }

        public List<Option> Maquinas { get; set; }

        public List<Option> Padroes { get; set; }

        public List<Option> Profissionais { get; set; }

        public List<Row> TabelaMeta { get; set; }

        public class Row
        {
            public string Area { get; set; }

            public string Maquina { get; set; }

            public string Chapa { get; set; }

            public string Nome { get; set; }

            public string Email { get; set; }

            public string Meta { get; set; }

            public string Padrao { get; set; }

            public string Planta { get; set; }

            public string Colaborador { get; set; }

            public string Treinamento { get; set; }
        }
    }

    public class CadastroMetaViewModel
    {
        public string Colaborador { get; set; }

        public string Treinamento { get; set; }

        public string Meta { get; set; }
    }

    public class CadastroTurmaTreinamentoViewModel
    {
        public enum TipoTreinamento
        {
            Especifico = 0,
            Geral = 1,
        }

        public TipoTreinamento Tipo { get; set; }

        public string Colaborador { get; set; }

        public string Treinamento { get; set; }

        public string Nota { get; set; }
    }

    public class TreinamentoViewModel
    {
        public enum TipoTreinamento
        {
            Todos = 0,
            WCM = 1,
            Qualidade = 2,
            Específico = 3,
        }

        public TreinamentoViewModel()
        {
            Plantas = new List<Option>();
            Areas = new List<Option>();
            Maquinas = new List<Option>();
            TabelaMeta = new List<Row>();
            Padroes = new List<Option>();
            Profissionais = new List<Option>();

            Plantas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });

            Areas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });

            Maquinas.Add(new Option()
            {
                Key = "",
                Text = "Todas",
            });
        }

        public List<Option> Plantas { get; set; }

        public List<Option> Areas { get; set; }

        public List<Option> Maquinas { get; set; }

        public List<Option> Padroes { get; set; }

        public List<Option> Profissionais { get; set; }

        public List<Row> TabelaMeta { get; set; }

        public class Row
        {
            public string Chapa { get; set; }

            public string Nome { get; set; }

            public string Padrao { get; set; }

            public string Colaborador { get; set; }

            public string Treinamento { get; set; }

            public string Gestor { get; set; }

            public string DataConclusao { get; set; }

            public string Pontuacao { get; set; }

            public bool IsEspecifico { get; set; }

            public string Localizador { get; set; }
        }
    }

    public class CadastroPlanoAcaoViewModel
    {
        public Dictionary<string, Option> Responsaveis { get; set; }

        public Dictionary<string, Option> Areas { get; set; }

        public class Option
        {
            public Option() { }

            public Option(string text)
            {
                Text = text;
            }

            public string Text { get; set; }

            public bool IsSelected { get; set; }

            public string Class { get; set; }
        }
    }

    public class OperadorViewModel
    {
        public enum Farol
        {
            Vermelho = 0, // Não auditado
            Amarelo = 1,  // Abaixo da meta
            Verde = 2,    // Regular
            Disabled = 3, // Quando o usuário não realizou o treinamento
        }

        public string Key { get; set; }

        public string Nome { get; set; }

        public Farol Status { get; set; }
    }

    public class Option
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public bool IsSelected { get; set; }
    }
}