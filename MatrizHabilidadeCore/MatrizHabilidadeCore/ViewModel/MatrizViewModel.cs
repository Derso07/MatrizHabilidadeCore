using System;
using System.Collections.Generic;
using System.Globalization;

namespace MatrizHabilidade.ViewModel
{
    public class MatrizViewModel : ViewModelBase
    {
        public MatrizViewModel()
        {
            TiposTreinamento = new List<TipoTreinamento>();
            TreinamentosEspecificos = new List<Treinamento>();
            Colaboradores = new List<Colaborador>();

            TreinamentosFiltrados = new Dictionary<string, string>();

            FiltroColaboradores = new Dictionary<string, string>();
            FiltroTipo = -1;
            FiltroTreinamento = null;
            FiltroNota = -1;
            FiltroStatus = -1;

            GAPGeral = new GAPDTO();
            GAPTipoEspecifico = new GAPDTO();
        }

        public GAPDTO GAPGeral { get; set; }

        public GAPDTO GAPTipoEspecifico { get; set; }

        public Dictionary<string, string> FiltroColaboradores { get; set; }

        public int FiltroTipo { get; set; }

        public Tuple<string, string> FiltroTreinamento { get; set; }

        public Dictionary<string, string> TreinamentosFiltrados { get; set; }

        public bool IgnoreManutencao { get; set; }

        public int FiltroNota { get; set; }

        public int FiltroMeta { get; set; }

        public int FiltroStatus { get; set; }

        public List<TipoTreinamento> TiposTreinamento { get; set; }

        public List<Treinamento> TreinamentosEspecificos { get; set; }

        public List<Colaborador> Colaboradores { get; set; }

        public class TipoTreinamento
        {
            public TipoTreinamento()
            {
                Treinamentos = new List<Treinamento>();
                GAP = new GAPDTO();
            }

            public int Id { get; set; }

            public string Descricao { get; set; }

            public string CorTitulo { get; set; }

            public string CorTituloClara
            {
                get
                {
                    var r = int.Parse(CorTitulo.Substring(1, 2), NumberStyles.HexNumber);
                    var g = int.Parse(CorTitulo.Substring(3, 2), NumberStyles.HexNumber);
                    var b = int.Parse(CorTitulo.Substring(5, 2), NumberStyles.HexNumber);

                    r = (int)Math.Round((r + (0.5 * (255 - r))));
                    g = (int)Math.Round((g + (0.5 * (255 - g))));
                    b = (int)Math.Round((b + (0.5 * (255 - b))));

                    return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
                }
            }

            public string CorTreinamento { get; set; }

            public string CorFonte { get; set; }

            public List<Treinamento> Treinamentos { get; set; }

            public GAPDTO GAP { get; set; }
        }


        public class Treinamento
        {
            public string Descricao { get; set; }
        }

        public class Colaborador
        {
            public string Chapa { get; set; }

            public string Nome { get; set; }

            public string Funcao { get; set; }

            public bool IsFacilitador { get; set; }

            public string Maquina { get; set; }

            public TreinamentoColaboradorViewModel[] Treinamentos { get; set; }
        }

        public class GAPDTO
        {
            public string Conhecimento { get; set; }

            public string Treinamento { get; set; }
        }

        public string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }
    }

    public class TreinamentoColaboradorViewModel
    {
        public Cell Nota { get; set; }

        public Cell Meta { get; set; }

        public Cell IsOK { get; set; }

        public class Cell
        {
            public Cell()
            {
                CellStyle = Style.Default;
                CellType = CellType.String;
            }

            public string Value { get; set; }

            public CellType CellType { get; set; }

            public Style CellStyle { get; set; }
        }

        public class CellType
        {
            private CellType(string value) { Value = value; }

            public string Value { get; private set; }

            public static CellType String { get { return new CellType("s"); } }
            public static CellType Number { get { return new CellType("n"); } }
            public static CellType Formula { get { return new CellType("f"); } }
        }

        public class Style
        {
            private Style(string @class, string backgroundColor, string fontColor)
            {
                Class = @class;
                BackgroundColor = backgroundColor;
                FontColor = fontColor;
            }

            public string Class { get; private set; }

            public string BackgroundColor { get; private set; }

            public string FontColor { get; private set; }

            public static Style Default
            {
                get
                {
                    return new Style("", "ebebeb", "1D428A");
                }
            }

            public static Style Red
            {
                get
                {
                    return new Style("red", "8F1626", "ffffff");
                }
            }

            public static Style Yellow
            {
                get
                {
                    return new Style("yellow", "F8D44B", "1D428A");
                }
            }

            public static Style Gray
            {
                get
                {
                    return new Style("gray", "a6a6a6", "ffffff");
                }
            }
        }
    }

    public class FiltroTabelaViewModel
    {
        public string Planta { get; set; }

        public string Area { get; set; }

        public string Maquina { get; set; }

        public int? Tipo { get; set; }

        public string Treinamento { get; set; }

        public int? TreinamentoId { get; set; }

        public int? FiltroNota { get; set; }

        public int? FiltroMeta { get; set; }

        public int? FiltroStatus { get; set; }

        public string[] Colaboradores { get; set; }

        public bool HasFilter()
        {
            if (Tipo.HasValue)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Treinamento))
            {
                return true;
            }

            if (TreinamentoId.HasValue)
            {
                return true;
            }

            if (FiltroNota.HasValue)
            {
                return true;
            }

            if (FiltroMeta.HasValue)
            {
                return true;
            }

            if (FiltroStatus.HasValue)
            {
                return true;
            }

            if (Colaboradores != null)
            {
                return true;
            }

            return false;
        }
    }
}