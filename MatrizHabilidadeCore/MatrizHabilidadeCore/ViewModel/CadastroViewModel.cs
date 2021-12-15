namespace MatrizHabilidade.ViewModel
{
    public class CadastroIntegracaoViewModel
    {
        public ProppertyViewModel FormIntegracao { get; set; }

        public TableViewModel TabelaIntegracoes { get; set; }

        public InputViewModel Integracao { get; set; }

        public InputViewModel MetaIntegracao { get; set; }

        public SelectViewModel TreinamentoIntegracao { get; set; }

        public StyleViewModel DesativarIntegracaoContainer { get; set; }

        public CheckboxViewModel DesativarIntegracao { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosIntegracao { get; set; }

        public AlertaViewModel Alerta { get; set; }
    }

    public class CadastroUnidadeViewModel
    {
        public AlertaViewModel Alerta { get; set; }

        public TableViewModel TabelaUnidades { get; set; }

        public InputViewModel DescricaoUnidade { get; set; }

        public SelectViewModel PlantaBdados { get; set; }

        public InputViewModel Unidade { get; set; }

        public ProppertyViewModel FormUnidade { get; set; }

        public StyleViewModel DesativarUnidadeContainer { get; set; }

        public CheckboxViewModel DesativarUnidade { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosUnidade { get; set; }
    }

    public class CadastroMaquinaViewModel
    {
        public InputViewModel DescricaoMaquina { get; set; }

        public InputViewModel Maquina { get; set; }

        public TableViewModel TabelaMaquinas { get; set; }

        public ProppertyViewModel FormMaquina { get; set; }

        public SelectViewModel UniorgMaquina { get; set; }

        public SelectViewModel FiltroPlantaMaquina { get; set; }

        public SelectViewModel FiltroAreaMaquina { get; set; }

        public SelectViewModel FiltroCoordenadorMaquina { get; set; }

        public CustomViewModel UniorgContainer { get; set; }

        public SelectViewModel CategoriaMaquina { get; set; }

        public StyleViewModel DesativarMaquinaContainer { get; set; }

        public CheckboxViewModel DesativarMaquina { get; set; }

        public AlertaViewModel Alerta { get; set; }

        public SelectViewModel FiltroUnidadeTabelaMaquina { get; set; }

        public SelectViewModel FiltroAreaTabelaMaquina { get; set; }

        public SelectViewModel FiltroCoordenadorTabelaMaquina { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosMaquina { get; set; }
    }

    public class CadastroTipoTreinamentoViewModel
    {
        public ProppertyViewModel FormTipoTreinamento { get; set; }

        public TableViewModel TabelaTipoTreinamentos { get; set; }

        public InputViewModel TipoTreinamento { get; set; }

        public InputViewModel CorTituloTipoTreinamento { get; set; }

        public InputViewModel CorTreinamentoTipoTreinamento { get; set; }

        public InputViewModel CorFonteTipoTreinamento { get; set; }

        public InputViewModel DescricaoTipoTreinamento { get; set; }

        public SelectViewModel MaquinaTipoTreinamento { get; set; }

        public SelectViewModel CategoriaTipoTreinamento { get; set; }

        public SelectViewModel FiltroPlantaTipoTreinamento { get; set; }

        public SelectViewModel FiltroAreaTipoTreinamento { get; set; }

        public CustomViewModel MaquinaTipoTreinamentoContainer { get; set; }

        public CheckboxViewModel DesativarTipoTreinamento { get; set; }

        public StyleViewModel DesativarTipoTreinamentoContainer { get; set; }

        public CustomViewModel CategoriaTipoTreinamentoContainer { get; set; }

        public AlertaViewModel Alerta { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosTipoTreinamento { get; set; }
    }

    public class CadastroTreinamentoViewModel
    {
        public ProppertyViewModel FormTreinamento { get; set; }

        public TableViewModel TabelaTreinamentos { get; set; }

        public InputViewModel Treinamento { get; set; }

        public InputViewModel DescricaoTreinamento { get; set; }

        public SelectViewModel TipoTreinamento { get; set; }

        public SelectViewModel FiltroTipoTabelaTreinamento { get; set; }

        public StyleViewModel DesativarTreinamentoContainer { get; set; }

        public CheckboxViewModel DesativarTreinamento { get; set; }

        public AlertaViewModel Alerta { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosTreinamento { get; set; }
    }

    public class CadastroTreinamentoEspecificoViewModel
    {
        public CustomViewModel MaquinaTreinamentoEspecificoContainer { get; set; }

        public InputViewModel DescricaoTreinamentoEspecifico { get; set; }

        public SelectViewModel MaquinaTreinamentoEspecifico { get; set; }

        public SelectViewModel FiltroUnidadeTreinamentoEspecifico { get; set; }

        public SelectViewModel CategoriaTreinamentoEspecifico { get; set; }

        public InputViewModel TreinamentoEspecifico { get; set; }

        public ProppertyViewModel FormTreinamentoEspecifico { get; set; }

        public TableViewModel TabelaTreinamentoEspecificos { get; set; }

        public StyleViewModel DesativarTreinamentoEspecificoContainer { get; set; }

        public CheckboxViewModel DesativarTreinamentoEspecífico { get; set; }

        public AlertaViewModel Alerta { get; set; }

        public SelectViewModel FiltroUnidadeTabelaTreinamentoEspecifico { get; set; }

        public SelectViewModel FiltroAreaTabelaTreinamentoEspecifico { get; set; }

        public SelectViewModel FiltroMaquinaTabelaTreinamentoEspecifico { get; set; }

        public SelectViewModel FiltroAreaTreinamentoEspecifico { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosTreinamentoEspecifico { get; set; }
    }

    public class CadastroCategoriaViewModel
    {
        public AlertaViewModel Alerta { get; set; }

        public TableViewModel TabelaCategorias { get; set; }

        public InputViewModel DescricaoCategoria { get; set; }

        public InputViewModel Categoria { get; set; }

        public ProppertyViewModel FormCategoria { get; set; }

        public StyleViewModel DesativarCategoriaContainer { get; set; }

        public CheckboxViewModel DesativarCategoria { get; set; }

        public CheckboxViewModel FiltroMostrarDesativadosCategoria { get; set; }
    }

    public class FacilitadorViewModel
    {
        public bool IsEdited { get; set; }

        public bool IsFacilitador { get; set; }
    }

    public class CadastroFacilitadorViewModel
    {
        public AlertaViewModel Alerta { get; set; }

        public TableViewModel TabelaFacilitadores { get; set; }

        public SelectViewModel FiltroAreaFacilitador { get; set; }

        public SelectViewModel FiltroMaquinaFacilitador { get; set; }

        public SelectViewModel FiltroCoordenadorFacilitador { get; set; }

        public InputViewModel FiltroNomeFacilitador { get; set; }

        public SelectViewModel FiltroUnidadeFacilitador { get; set; }
    }

    public class CadastroCoordenadorViewModel
    {
        public InputViewModel Coordenador { get; set; }

        public AlertaViewModel Alerta { get; set; }

        public ProppertyViewModel FormCoordenador { get; set; }

        public SelectViewModel CoordenadorCoordenador { get; set; }

        public SelectViewModel MaquinaCoordenador { get; set; }

        public TableViewModel TabelaCoordenador { get; set; }

        public SelectViewModel FiltroPlantaCoordenador { get; set; }

        public SelectViewModel FiltroUnidadeTabelaCoordenador { get; set; }

        public SelectViewModel FiltroAreaCoordenador { get; set; }

        public StyleViewModel DesativarCoordenadorContainer { get; set; }

        public CheckboxViewModel DesativarCoordenador { get; set; }
    }
}