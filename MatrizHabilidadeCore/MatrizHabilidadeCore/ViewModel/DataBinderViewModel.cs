using System;
using System.Collections.Generic;

namespace MatrizHabilidade.ViewModel
{
    public enum DataBinderType
    {
        None = -1,
        Checkbox = 0,
        Radio = 1,
        Table = 2,
        Propperty = 3,
        Style = 4,
        Select = 5,
        Chart = 6,
        Custom = 7,
        Alert = 8,
        Reload = 9,
        Input = 10,
    }

    public class DataBinderViewModel
    {
        public const DataBinderType Type = DataBinderType.None;
    }

    public class CheckboxViewModel : DataBinderViewModel
    {
        public CheckboxViewModel()
        {
            Value = false;
        }

        public CheckboxViewModel(bool value)
        {
            Value = value;
        }

        public new DataBinderType Type = DataBinderType.Checkbox;

        public bool Value { get; set; }
    }

    public class InputViewModel : DataBinderViewModel
    {
        public InputViewModel()
        {
            Value = "";
        }

        public InputViewModel(string value)
        {
            Value = value;
        }

        public new DataBinderType Type = DataBinderType.Input;

        public string Value { get; set; }
    }

    public class StyleViewModel : DataBinderViewModel
    {
        public StyleViewModel(string propperty, string value)
        {
            Value = new Style(propperty, value);
        }

        public new DataBinderType Type = DataBinderType.Style;

        public Style Value { get; set; }

        public class Style
        {
            public Style(string propperty, string value)
            {
                Propperty = propperty;
                Value = value;
            }

            public string Propperty { get; set; }

            public string Value { get; set; }
        }
    }

    public class ProppertyViewModel : DataBinderViewModel
    {
        public ProppertyViewModel(string propperty, string value)
        {
            Value = new Prop(propperty, value);
        }

        public new DataBinderType Type = DataBinderType.Propperty;

        public Prop Value { get; set; }

        public class Prop
        {
            public Prop(string propperty, string value)
            {
                Propperty = propperty;
                Value = value;
            }

            public string Propperty { get; set; }

            public string Value { get; set; }
        }
    }

    public class AlertaViewModel : DataBinderViewModel
    {
        public AlertaViewModel(string value)
        {
            Value = value;
        }

        public new DataBinderType Type = DataBinderType.Alert;

        public string Value { get; set; }
    }

    public class TableViewModel : DataBinderViewModel
    {
        public TableViewModel()
        {
            Value = new Table();
        }

        public new DataBinderType Type = DataBinderType.Table;

        public Table Value { get; set; }

        [Serializable]
        public class Row
        {
            public Row() { }

            public Row(string[] Values, int Index)
            {
                this.Index = Index;
                this.Values = Values;
            }

            public string Key { get; set; }

            public int Index { get; set; }

            public string Class { get; set; }

            public string[] Values { get; set; }
        }

        [Serializable]
        public class Table
        {
            public Table()
            {
                Rows = new List<Row>();
            }

            public string OnClick { get; set; }

            public List<Row> Rows { get; set; }
        }
    }

    public class SelectViewModel : DataBinderViewModel
    {
        public SelectViewModel()
        {
            Value = new Select();
        }

        public SelectViewModel(string defaultText, string allText)
        {
            Value = new Select
            {
                DefaultText = defaultText,
                AllText = allText,
            };
        }

        public SelectViewModel(string allText, bool showFirstOption)
        {
            Value = new Select
            {
                ShowFirstOption = showFirstOption,
                AllText = allText,
            };
        }

        public SelectViewModel(string defaultText, string allText, bool showFirstOption)
        {
            Value = new Select
            {
                DefaultText = defaultText,
                AllText = allText,
                ShowFirstOption = showFirstOption,
            };
        }

        public new DataBinderType Type = DataBinderType.Select;

        public Select Value { get; set; }

        public class Select
        {
            public Select()
            {
                Options = new Dictionary<string, string>();
                SelectedIndex = -1;
                DefaultText = "Selecione";
                AllText = "Todos";
            }

            public string SelectedKey { get; set; }

            public int SelectedIndex { get; set; }

            public Dictionary<string, string> Options { get; set; }

            public bool IsRequired { get; set; }

            public string OnChange { get; set; }

            public bool ShowFirstOption { get; set; }

            public string DefaultText { get; set; }

            public string AllText { get; set; }
        }
    }

    public class CustomViewModel : DataBinderViewModel
    {
        public CustomViewModel()
        {
            Value = "";
        }

        public new DataBinderType Type = DataBinderType.Custom;

        public string Value { get; set; }
    }
}