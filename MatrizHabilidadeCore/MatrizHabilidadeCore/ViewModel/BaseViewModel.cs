using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MatrizHabilidade.ViewModel
{
    public class Grafico
    {
        public Grafico(string containerId)
        {
            ContainerId = containerId;
            Series = new List<Serie>();
        }

        public readonly string ContainerId;

        public string Title { get; set; }

        public string Unit { get; set; }

        public bool Rotate { get; set; }

        public bool HasLine { get; set; }

        public enum AlinhamentoGrafico
        {
            Bottom = 0,
            Right = 1,
        }

        public AlinhamentoGrafico Alinhamento { get; set; }

        public List<Serie> Series { get; set; }

        public class Serie
        {
            public Serie()
            {
                Data = new List<double>();
            }

            public enum TipoSerie
            {
                Column = 0,
                Spline = 1,
                Line = 2,
            }

            public TipoSerie Tipo { get; set; }

            public string Name { get; set; }

            public string Color { get; set; }

            public List<double> Data { get; set; }

            public override string ToString()
            {
                return $@"
                    {{
                        type: '{Tipo.ToString("g").ToLower()}',
                        name: '{Name}',
                        color: '{Color}',
                        data: [{string.Join(", ", Data.Select(d => d.ToString("f2").Replace(",", ".")))}]
                    }}
                ";
            }
        }

        private string GetLegendParameters()
        {
            if (Alinhamento == AlinhamentoGrafico.Right)
            {
                return $@"                    
                    legend: {{
                        align: 'right',
                        verticalAlign: 'middle',
                        layout: 'vertical',
                        itemStyle:{{
                            ""fontSize"": ""10px"",
                        }}
                    }},";
            }
            else
            {
                return $@"                    
                    legend: {{
     		            align: 'center',
                        verticalAlign: 'bottom',
                        x: 0,
                        y: 0,
                        itemStyle: {{
                            ""fontSize"": ""10px"",
                        }}
                    }},";
            }
        }

        public override string ToString()
        {
            return $@"
                Highcharts.chart('{ContainerId}', {{
                    title: {{
                        text: '{Title}'
                    }},
                    {GetLegendParameters()}
                    xAxis: {{
                        categories: ['Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez', 'Jan', 'Fev', 'Mar'],
                        gridLineWidth: 1,
                        labels:{{
                            style:{{
                                fontSize: 12,
                            }}
                        }}
                    }},
                    yAxis: {{
                        title:{{
                            enabled: false,
                        }},
                        opposite: true,
                        plotLines: [{{
                            value: 0,
                            color: '#5FBC23',
                            width: {(HasLine ? 4 : 0)},
                            zIndex: 99,
                        }}],
                    }},
                    plotOptions: {{
                        column: {{
                            groupPadding: 0.1,
                            pointPadding: 0.2,
                            dataLabels: {{
                                enabled: true,
                                align: 'center',
                                allowOverlap: true,
                                crop: false,
                                overflow: 'none',
                                rotation: -45,
                                y: -8,

                                color: 'black',
                                inside: true,
                                verticalAlign: 'top',

                                style:{{
                                    fontSize: 10,
                                }},
                                formatter: function() {{
                                    if (this.y != 0) {{
                                        return Highcharts.numberFormat(this.y, 0) + '{Unit}';
                                    }}
                                }}
                            }}
                        }},
                        spline: {{
                            dataLabels: {{
                                enabled: true,
                                align: 'center',
                                allowOverlap: true,
                                formatter: function() {{
                                    return Highcharts.numberFormat(this.y, 0) + '{Unit}';
                                }},
                            }}
                        }},
                        line: {{
                            dataLabels: {{
                                enabled: true,
                                align: 'center',
                                allowOverlap: true,
                                formatter: function() {{
                                    return Highcharts.numberFormat(this.y, 0) + '{Unit}';
                                }},
                            }}
                        }}
                    }},
                    responsive: {{
                        rules: [{{
                            condition: {{
                                maxWidth: 1200,
                            }},
                            legend: {{
                                itemStyle:{{
                                    ""fontSize"": ""12px"",
                                }}
                            }},
                            xAxis: {{
                                labels:{{
                                    style:{{
                                        fontSize: 14,
                                    }}
                                }}
                            }},
                        }}]
                    }},
                    series: [{string.Join(", ", Series)}]
                }})";
        }
    }

    public class LinhaGraficoFarol
    {
        public LinhaGraficoFarol()
        {
            Auditoria = new Farol();
            Treinamento = new Farol();
        }

        public string Id { get; set; }

        public string Descricao { get; set; }

        public Farol Auditoria { get; set; }

        public Farol Treinamento { get; set; }

        public class Farol
        {
            public Farol()
            {
                Treinamentos = new Dictionary<string, double>();
            }

            public double Media { get; set; }

            public Dictionary<string, double> Treinamentos { get; set; }
        }
    }

    public class ProgressBar
    {
        public ProgressBar()
        {
            Treinamentos = new Dictionary<string, double>();
        }

        public double Media { get; set; }

        public Dictionary<string, double> Treinamentos { get; set; }
    }

    public class GraficoLinhaViewModel
    {
        public GraficoLinhaViewModel()
        {
            Series = new List<Data>();
        }

        public string Container { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Unit { get; set; }

        public List<Data> Series { get; set; }

        public class Data
        {
            public Data()
            {
                Values = new string[12];
            }

            public string Name { get; set; }

            public string[] Values { get; set; }

            public string Color { get; set; }

            public override string ToString()
            {
                return $@"
                    {{
                        name: '{Name}',
                        data: [{string.Join(", ", Values.Select(v => v))}],
                        color: '{Color}',
                    }}";
            }
        }

        public override string ToString()
        {
            return $@"        
                Highcharts.chart('{Container}', {{
                    title: {{
                        text: '{Title}'
                    }},
                    plotOptions: {{
                        line: {{
                            dataLabels: {{
                                enabled: true
                            }},
                            enableMouseTracking: false
                        }}
                    }},
                    subtitle: {{
                        text: '{Subtitle}'
                    }},
                    xAxis: {{
                        categories: ['Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez', 'Jan', 'Fev', 'Mar']
                    }},
                    yAxis: {{
                        title: {{
                            text: '{Unit}'
                        }}
                    }},
                    series: [{string.Join(", ", Series)}]
                }});";
        }
    }

    public class GraficoPie
    {
        public GraficoPie()
        {
            Series = new List<Data>();
        }

        public string Container { get; set; }

        public string Title { get; set; }

        public string SeriesName { get; set; }

        public List<Data> Series { get; set; }

        public class Data
        {
            public string Name { get; set; }

            public float Value { get; set; }

            public string Color { get; set; }

            public override string ToString()
            {
                return $@"
                    {{
                        name: '{Name}',
                        y: {Value.ToString("F", CultureInfo.CreateSpecificCulture("en-CA"))},
                        color: '{Color}',
                    }}";
            }
        }

        public override string ToString()
        {
            return $@"
                Highcharts.chart('{Container}', {{
                    chart: {{
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false,
                        type: 'pie',
                        margin: [0, 0, 0, 0],
                        spacing: [0, 0, 0, 0],
                        backgroundColor: 'transparent',
                    }},
                    title: {{
                        text: '{Title}'
                    }},
                    tooltip: {{
                        pointFormat: '{{series.name}}: <b>{{point.percentage:.1f}}%</b>'
                    }},
                    accessibility: {{
                        point: {{
                            valueSuffix: '%'
                        }}
                    }},
                    plotOptions: {{
                        pie: {{
                            allowPointSelect: true,
                            cursor: 'pointer',
                             dataLabels: {{
                                enabled: true,
                                format: '{{point.percentage:.1f}}%',
                                distance: -15,
                                filter: {{
                                    property: 'percentage',
                                    operator: '>',
                                    value: 4
                                }}
                            }}
                        }}
                    }},
                    series: [{{
                        name: '{SeriesName}',
                        minSize: 100,
                        data: [{string.Join(", ", Series)}],
                        size: '110%',
                        innerSize: '60%',
                        showInLegend: false,
                    }}],
                    credits: {{
                        enabled: false
                    }},
                    navigation: {{
                        buttonOptions: {{
                            enabled: false
                        }}
                    }},
                    responsive: {{
                        rules: [{{
                            condition: {{
                                callback: function() {{
                                    if (window.innerWidth < 576) {{
                                        return true;
                                    }}
                                }}
                            }},
                            chartOptions: {{
                                plotOptions: {{
                                    pie: {{
                                        dataLabels: {{
                                            distance: -25,
                                        }},
                                    }}
                                }},
                            }},
                        }}]
                    }},
                }});";
        }
    }

    public class ViewModelBase
    {
        public string PlantaId { get; set; }

        public string PlantaDescricao { get; set; }

        public string AreaId { get; set; }

        public string AreaDescricao { get; set; }

        public string MaquinaId { get; set; }

        public string MaquinaDescricao { get; set; }
    }
}