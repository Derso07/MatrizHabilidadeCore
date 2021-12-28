using MatrizHabilidade.ViewModel;
using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDatabase.Services;
using MatrizHabilidadeDataBaseCore;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Controllers
{
    public class CadastroController : BaseController
    {
        public CadastroController(DataBaseContext _db, CookieService cookieService) : base(_db, cookieService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }

        #region Unidade
        public ActionResult LimparUnidade()
        {
            var model = GetUnidades(false);

            return Json(model);
        }

        public ActionResult CadastrarUnidade(string DescricaoUnidade, string PlantaBDados, bool? FiltroMostrarDesativadosUnidade)
        {
            string alerta;

            if (!FiltroMostrarDesativadosUnidade.HasValue)
            {
                FiltroMostrarDesativadosUnidade = false;
            }

            if (int.TryParse(Encrypting.Decrypt(PlantaBDados), out int plantaBDadosId))
            {
                var planta = new Planta()
                {
                    Descricao = DescricaoUnidade,
                    PlantaBDadosId = plantaBDadosId,
                };

                _db.Plantas.Add(planta);

                _db.SaveChanges();

                alerta = "Cadastrado com Sucesso";
            }
            else
            {
                alerta = @"Deve-se selecionar ao menos uma Unidade do BDados";
            }

            var result = GetUnidades(FiltroMostrarDesativadosUnidade.Value);

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult AtualizarUnidade(string Unidade, string DescricaoUnidade, string PlantaBDados, bool? DesativarUnidade, bool? FiltroMostrarDesativadosUnidade)
        {
            if (!FiltroMostrarDesativadosUnidade.HasValue)
            {
                FiltroMostrarDesativadosUnidade = false;
            }

            if (!DesativarUnidade.HasValue)
            {
                DesativarUnidade = false;
            }

            if (int.TryParse(Encrypting.Decrypt(Unidade), out int plantaId))
            {
                if (int.TryParse(Encrypting.Decrypt(PlantaBDados), out int plantaBDadosId))
                {
                    var planta = _db.Plantas.Find(plantaId);

                    planta.Descricao = DescricaoUnidade;
                    planta.PlantaBDadosId = plantaBDadosId;
                    planta.IsAtivo = !DesativarUnidade.Value;

                    _db.SaveChanges();
                }
            }

            var result = GetUnidades(FiltroMostrarDesativadosUnidade.Value);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult PopularUnidade(string unidade, bool isDesativado)
        {
            var model = GetUnidades(isDesativado);

            if (int.TryParse(Encrypting.Decrypt(unidade), out int unidadeId))
            {
                var _unidade = _db.Plantas.Find(unidadeId);

                model.FormUnidade = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarUnidade");
                model.DescricaoUnidade = new InputViewModel(_unidade.Descricao);
                model.Unidade = new InputViewModel(unidade);
                model.DesativarUnidadeContainer = new StyleViewModel("display", "block");
                model.DesativarUnidade = new CheckboxViewModel(!_unidade.IsAtivo);

                var key = Encrypting.Encrypt(_unidade.PlantaBDadosId.ToString());

                int selectedPlanta = model.PlantaBdados.Value.Options.Keys.ToList().FindIndex(p => p == key);

                model.PlantaBdados.Value.SelectedIndex = selectedPlanta;
            }

            return Json(model);
        }

        public ActionResult FiltrarUnidade(bool? isDesativado)
        {
            if (!isDesativado.HasValue)
            {
                isDesativado = false;
            }

            var model = GetUnidades(isDesativado.Value);

            return Json(model);
        }

        private CadastroUnidadeViewModel GetUnidades(bool isDesativado)
        {
            var model = new CadastroUnidadeViewModel
            {
                FormUnidade = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarUnidade"),
                TabelaUnidades = GetTabelaUnidade(isDesativado),
                Unidade = new InputViewModel(""),
                DescricaoUnidade = new InputViewModel(""),
                PlantaBdados = new SelectViewModel(),
                DesativarUnidadeContainer = new StyleViewModel("display", "none"),
                DesativarUnidade = new CheckboxViewModel(),
                FiltroMostrarDesativadosUnidade = new CheckboxViewModel(isDesativado),
            };

            foreach (var planta in _db.PlantasBDaddos)
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.PlantaBdados.Value.Options.Add(key, planta.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaUnidade(bool isDesativado)
        {
            var model = new TableViewModel();
            List<Planta> plantas = new List<Planta>();

            model.Value.OnClick = "Binder.Bind('/MatrizHabilidade/Cadastro/PopularUnidade', { unidade: this.getAttribute('data-key'), isDesativado: document.querySelector('[name=FiltroMostrarDesativadosUnidade]').value })";

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição da Unidade", "Descrição no BDados", "Desativado" } });
                plantas = _db.Plantas.OrderBy(p => p.Descricao).ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição da Unidade", "Descrição no BDados" } });
                plantas = _db.Plantas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList();
            }

            foreach (var unidade in plantas)
            {
                string[] values;

                var descricaoPlantaBDados = _db.PlantasBDaddos.Find(unidade.PlantaBDadosId).Descricao;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        unidade.Descricao,
                        descricaoPlantaBDados,
                        unidade.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        unidade.Descricao,
                        descricaoPlantaBDados,
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(unidade.Id.ToString()),
                    Values = values,
                });
            }

            return model;
        }
        #endregion

        #region Máquina
        public ActionResult LimparMaquina()
        {
            var model = GetMaquinas(null, null, null, false);

            return Json(model);
        }

        public ActionResult CadastrarMaquina(string DescricaoMaquina, string[] Uniorgs, string CategoriaMaquina, string FiltroUnidadeTabelaMaquina, string FiltroAreaTabelaMaquina, string FiltroCoordenadorTabelaMaquina, bool? FiltroMostrarDesativadosMaquina)
        {
            string alerta;

            if (!FiltroMostrarDesativadosMaquina.HasValue)
            {
                FiltroMostrarDesativadosMaquina = false;
            }

            if (string.IsNullOrEmpty(DescricaoMaquina))
            {
                alerta = @"Deve-se preencher o campo ""Descrição Máquina""";
            }
            else if (Uniorgs == null)
            {
                alerta = @"Deve-se selecionar ao menos uma Uniorg";
            }
            else
            {
                var uniorgs = new List<Uniorg>();

                for (int x = 0; x < Uniorgs.Length; x++)
                {
                    if (int.TryParse(Encrypting.Decrypt(Uniorgs[x]), out int uniorgMaquinaId))
                    {
                        var uniorg = _db.Uniorgs.Find(uniorgMaquinaId);

                        uniorgs.Add(uniorg);
                    }
                }

                if (uniorgs.Count == 0)
                {
                    alerta = @"Deve-se selecionar ao menos uma Uniorg";
                }
                else
                {
                    int coordenadorId = uniorgs[0].CoordenadorId;

                    bool hasError = false;

                    if (uniorgs.Count > 1)
                    {
                        for (int i = 1; i < uniorgs.Count; i++)
                        {
                            if (coordenadorId != uniorgs[i].CoordenadorId)
                            {
                                hasError = true;
                                break;
                            }
                        }
                    }

                    if (hasError)
                    {
                        alerta = "Deve-se selecionar Uniorgs de somente um Coordenador";
                    }
                    else
                    {
                        int? categoriaId = null;

                        if (int.TryParse(Encrypting.Decrypt(CategoriaMaquina), out int aux))
                        {
                            categoriaId = aux;
                        }

                        var maquina = new Maquina()
                        {
                            Descricao = DescricaoMaquina,
                            CategoriaId = categoriaId,
                        };

                        _db.Maquinas.Add(maquina);

                        _db.SaveChanges();

                        foreach (var uniorg in uniorgs)
                        {
                            _db.UniorgMaquinas.Add(new UniorgMaquina()
                            {
                                MaquinaId = maquina.Id,
                                UniorgId = uniorg.Id,
                            });

                            _db.SaveChanges();
                        }

                        alerta = "Cadastrado com Sucesso";
                    }
                }
            }

            var result = GetMaquinas(FiltroUnidadeTabelaMaquina, FiltroAreaTabelaMaquina, FiltroCoordenadorTabelaMaquina, FiltroMostrarDesativadosMaquina.Value);

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult AtualizarMaquina(string Maquina, string DescricaoMaquina, string[] Uniorgs, string CategoriaMaquina, bool? DesativarMaquina, string FiltroUnidadeTabelaMaquina, string FiltroAreaTabelaMaquina, string FiltroCoordenadorTabelaMaquina, bool? FiltroMostrarDesativadosMaquina)
        {
            string alerta = "Algo deu errado";

            if (!DesativarMaquina.HasValue)
            {
                DesativarMaquina = false;
            }

            if (!FiltroMostrarDesativadosMaquina.HasValue)
            {
                FiltroMostrarDesativadosMaquina = false;
            }

            if (int.TryParse(Encrypting.Decrypt(Maquina), out int maquinaId))
            {
                var _maquina = _db.Maquinas.Find(maquinaId);

                var uniorgs = new List<Uniorg>();

                for (int x = 0; x < Uniorgs.Length; x++)
                {
                    if (int.TryParse(Encrypting.Decrypt(Uniorgs[x]), out int uniorgMaquinaId))
                    {
                        var uniorg = _db.Uniorgs.Find(uniorgMaquinaId);

                        uniorgs.Add(uniorg);
                    }
                }

                if (uniorgs.Count == 0)
                {
                    alerta = @"Deve-se selecionar ao menos uma Uniorg";
                }
                else
                {
                    bool hasError = false;

                    int coordenadorId = uniorgs[0].CoordenadorId;

                    if (uniorgs.Count > 1)
                    {
                        for (int i = 1; i < uniorgs.Count; i++)
                        {
                            if (coordenadorId != uniorgs[i].CoordenadorId)
                            {
                                hasError = true;
                                break;
                            }
                        }
                    }

                    if (hasError)
                    {
                        alerta = "Deve-se selecionar Uniorgs de somente um Coordenador";
                    }
                    else
                    {
                        int? categoriaId = null;

                        if (int.TryParse(Encrypting.Decrypt(CategoriaMaquina), out int aux))
                        {
                            categoriaId = aux;
                        }

                        _maquina.Descricao = DescricaoMaquina;
                        _maquina.CategoriaId = categoriaId;
                        _maquina.IsAtivo = !DesativarMaquina.Value;
                        _maquina.HasError = false;

                        _db.SaveChanges();

                        if (!DesativarMaquina.Value)
                        {
                            var _uniorgs = _maquina.Uniorgs.ToList();

                            foreach (var uniorg in uniorgs)
                            {
                                var index = _uniorgs.FindIndex(u => u.Id == uniorg.Id);

                                if (index >= 0)
                                {
                                    _uniorgs[index].ShouldRemain = true;
                                }
                                else
                                {
                                    _db.UniorgMaquinas.Add(new UniorgMaquina()
                                    {
                                        MaquinaId = _maquina.Id,
                                        UniorgId = uniorg.Id,
                                    });

                                    _db.SaveChanges();
                                }
                            }

                            foreach (var uniorg in _uniorgs)
                            {
                                if (!uniorg.ShouldRemain)
                                {
                                    _maquina.Uniorgs.Remove(uniorg);

                                    _db.SaveChanges();
                                }
                            }
                        }

                        alerta = "Atualizado com Sucesso";
                    }
                }
            }

            var result = GetMaquinas(FiltroUnidadeTabelaMaquina, FiltroAreaTabelaMaquina, FiltroCoordenadorTabelaMaquina, FiltroMostrarDesativadosMaquina.Value);

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult PopularMaquina(string Maquina, string planta, string area, string coordenador, bool isDesativado)
        {
            var model = GetMaquinas(planta, area, coordenador, isDesativado);

            if (int.TryParse(Encrypting.Decrypt(Maquina), out int maquinaId))
            {
                var _maquina = _db.Maquinas.Find(maquinaId);

                model.FormMaquina = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarMaquina");
                model.DescricaoMaquina = new InputViewModel(_maquina.Descricao);
                model.Maquina = new InputViewModel(Maquina);
                model.DesativarMaquina = new CheckboxViewModel(!_maquina.IsAtivo);

                model.DesativarMaquinaContainer = new StyleViewModel("display", "block");

                if (_maquina.CategoriaId.HasValue)
                {
                    var categoria = Encrypting.Encrypt(_maquina.CategoriaId.ToString());
                    int categoriaSelecionada = model.CategoriaMaquina.Value.Options.Keys.ToList().FindIndex(p => p == categoria);
                    model.CategoriaMaquina.Value.SelectedIndex = categoriaSelecionada;
                }

                foreach (var uniorg in _maquina.Uniorgs)
                {
                    var key = Encrypting.Encrypt(uniorg.Id.ToString());

                    model.UniorgContainer.Value += $@"
                        <li title=""{uniorg.DescricaoUniorg}"" onclick=""onUniorgRemoved(this)"">
                            <input type=""hidden"" name=""Uniorgs"" value=""{key}"" />
                            <span>{uniorg.DescricaoUniorg}</span>
                        </li>";
                }

                if (_maquina.HasError)
                {
                    var coordenadores = string.Join(", ", _maquina.Uniorgs.Select(u => u.Coordenador.Nome));

                    model.Alerta = new AlertaViewModel($"A Máquina selecionada contém mais de um Coordenador ({coordenadores})");
                }
            }

            return Json(model);
        }

        public ActionResult FiltrarMaquina(string planta, string area, string coordenador)
        {
            var model = new CadastroMaquinaViewModel();

            int? plantaId = null;
            int? areaId = null;
            int? coordenadorId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(coordenador), out int coordenadorAux))
            {
                coordenadorId = coordenadorAux;
            }

            if (plantaId.HasValue)
            {
                var _planta = _db.Plantas.Find(plantaId);

                model.UniorgMaquina = new SelectViewModel();
                model.FiltroAreaMaquina = new SelectViewModel("Todas as Áreas", true);
                model.FiltroCoordenadorMaquina = new SelectViewModel("Todos os Coordenadores", true);

                foreach (var _area in _planta.Areas.Where(a => a.Coordenadores.Any()).OrderBy(a => a.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaMaquina.Value.Options.Add(key, _area.Alias);

                    foreach (var _coordenador in _area.Coordenadores.OrderBy(c => c.Nome))
                    {
                        var coordenadorKey = Encrypting.Encrypt(_coordenador.Id.ToString());

                        model.FiltroCoordenadorMaquina.Value.Options.Add(coordenadorKey, _coordenador.Nome);

                        foreach (var uniorg in _coordenador.Uniorgs.Where(u => u.Maquinas.Count == 0).OrderBy(u => u.DescricaoUniorg).ToList())
                        {
                            var uniorgKey = Encrypting.Encrypt(uniorg.Id.ToString());

                            model.UniorgMaquina.Value.Options.Add(uniorgKey, uniorg.DescricaoUniorg);
                        }
                    }
                }

                if (areaId.HasValue)
                {
                    var index = model.FiltroAreaMaquina.Value.Options.Keys.ToList().FindIndex(p => p == area);

                    if (index >= 0)
                    {
                        model.FiltroAreaMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        areaId = null;
                    }
                }

                if (coordenadorId.HasValue)
                {
                    var index = model.FiltroCoordenadorMaquina.Value.Options.Keys.ToList().FindIndex(p => p == coordenador);

                    if (index >= 0)
                    {
                        model.FiltroCoordenadorMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        coordenadorId = null;
                    }
                }
            }

            if (areaId.HasValue)
            {
                var _area = _db.Areas.Find(areaId);

                model.UniorgMaquina = new SelectViewModel();
                model.FiltroCoordenadorMaquina = new SelectViewModel("Todos os Coordenadores", true);

                foreach (var _coordenador in _area.Coordenadores.OrderBy(c => c.Nome).ToList())
                {
                    var coordenadorKey = Encrypting.Encrypt(_coordenador.Id.ToString());

                    model.FiltroCoordenadorMaquina.Value.Options.Add(coordenadorKey, _coordenador.Nome);

                    foreach (var uniorg in _coordenador.Uniorgs.Where(u => u.Maquinas.Count == 0).OrderBy(u => u.DescricaoUniorg).ToList())
                    {
                        var uniorgKey = Encrypting.Encrypt(uniorg.Id.ToString());

                        if (!model.UniorgMaquina.Value.Options.ContainsKey(uniorgKey))
                        {
                            model.UniorgMaquina.Value.Options.Add(uniorgKey, uniorg.DescricaoUniorg);
                        }
                    }
                }

                if (coordenadorId.HasValue)
                {
                    var index = model.FiltroCoordenadorMaquina.Value.Options.Keys.ToList().FindIndex(p => p == coordenador);

                    if (index >= 0)
                    {
                        model.FiltroCoordenadorMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        coordenadorId = null;
                    }
                }
            }

            if (coordenadorId.HasValue)
            {
                model.UniorgMaquina = new SelectViewModel();
                var _coordenador = _db.Coordenadores.Find(coordenadorId);

                foreach (var uniorg in _coordenador.Uniorgs.Where(u => u.Maquinas.Count == 0).ToList())
                {
                    var uniorgKey = Encrypting.Encrypt(uniorg.Id.ToString());

                    if (!model.UniorgMaquina.Value.Options.ContainsKey(uniorgKey))
                    {
                        model.UniorgMaquina.Value.Options.Add(uniorgKey, uniorg.DescricaoUniorg);
                    }
                }
            }

            if (!plantaId.HasValue && !areaId.HasValue && !coordenadorId.HasValue)
            {
                model.UniorgMaquina = new SelectViewModel();
                model.FiltroAreaMaquina = new SelectViewModel("Todas as Áreas", true);
                model.FiltroCoordenadorMaquina = new SelectViewModel("Todos os Coordenadores", true);

                foreach (var uniorg in _db.Uniorgs.Where(u => u.Maquinas.Count == 0).OrderBy(c => c.DescricaoUniorg).ToList())
                {
                    var key = Encrypting.Encrypt(uniorg.Id.ToString());

                    model.UniorgMaquina.Value.Options.Add(key, uniorg.DescricaoUniorg);
                }

                foreach (var _area in _db.Areas.OrderBy(c => c.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaMaquina.Value.Options.Add(key, _area.Alias);
                }

                foreach (var _coordenador in _db.Coordenadores.OrderBy(c => c.Nome).ToList())
                {
                    var key = Encrypting.Encrypt(_coordenador.Id.ToString());

                    model.FiltroCoordenadorMaquina.Value.Options.Add(key, _coordenador.Nome) ;
                }
            }

            return Json(model);
        }

        public ActionResult FiltrarTabelaMaquina(string unidade, string area, string coordenador, bool isDesativado)
        {
            var model = new CadastroMaquinaViewModel();

            int? plantaId = null;
            int? areaId = null;
            int? coordenadorId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(coordenador), out int coordenadorAux))
            {
                coordenadorId = coordenadorAux;
            }

            if (plantaId.HasValue)
            {
                model.TabelaMaquinas = new TableViewModel();
                model.TabelaMaquinas.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Coordenador", "Descrição da Máquina", "Uniorgs", "Descrição das Uniorgs", "Categoria" } });

                var _planta = _db.Plantas.Find(plantaId);

                model.FiltroAreaTabelaMaquina = new SelectViewModel("Área", "Todas as Áreas", true);
                model.FiltroCoordenadorTabelaMaquina = new SelectViewModel("Coordenador", "Todos os Coordenadores", true);

                var areas = _planta.Areas
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .OrderBy(a => a.Alias)
                    .ToList();

                foreach (var _area in areas)
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTabelaMaquina.Value.Options.Add(key, _area.Alias);

                    var coordenadores = _area.Coordenadores
                        .Where(c => c.Uniorgs.Any())
                        .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                        .OrderBy(c => c.Nome);

                    foreach (var _coordenador in coordenadores)
                    {
                        var coordenadorKey = Encrypting.Encrypt(_coordenador.Id.ToString());

                        model.FiltroCoordenadorTabelaMaquina.Value.Options.Add(coordenadorKey, _coordenador.Nome);
                    }
                }

                if (areaId.HasValue)
                {
                    var index = model.FiltroAreaTabelaMaquina.Value.Options.Keys.ToList().FindIndex(p => p == area);

                    if (index >= 0)
                    {
                        model.FiltroAreaTabelaMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        areaId = null;
                        area = null;
                    }
                }

                if (coordenadorId.HasValue)
                {
                    var index = model.FiltroCoordenadorTabelaMaquina.Value.Options.Keys.ToList().FindIndex(p => p == coordenador);

                    if (index >= 0)
                    {
                        model.FiltroCoordenadorTabelaMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        coordenadorId = null;
                        coordenador = null;
                    }
                }
            }

            if (areaId.HasValue)
            {
                model.TabelaMaquinas = new TableViewModel();
                model.TabelaMaquinas.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Coordenador", "Descrição da Máquina", "Uniorgs", "Descrição das Uniorgs", "Categoria" } });

                var _area = _db.Areas.Find(areaId);

                model.FiltroCoordenadorTabelaMaquina = new SelectViewModel("Coordenador", "Todos os Coordenadores", true);

                var coordenadores = _area.Coordenadores
                    .Where(c => c.Uniorgs.Any())
                    .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                    .OrderBy(c => c.Nome);

                foreach (var _coordenador in coordenadores)
                {
                    var coordenadorKey = Encrypting.Encrypt(_coordenador.Id.ToString());

                    model.FiltroCoordenadorTabelaMaquina.Value.Options.Add(coordenadorKey, _coordenador.Nome);
                }

                if (coordenadorId.HasValue)
                {
                    var index = model.FiltroCoordenadorTabelaMaquina.Value.Options.Keys.ToList().FindIndex(p => p == coordenador);

                    if (index >= 0)
                    {
                        model.FiltroCoordenadorTabelaMaquina.Value.SelectedIndex = index;
                    }
                    else
                    {
                        coordenadorId = null;
                        coordenador = null;
                    }
                }
            }

            if (!plantaId.HasValue && !areaId.HasValue && !coordenadorId.HasValue)
            {
                model.TabelaMaquinas = new TableViewModel();
                model.TabelaMaquinas.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Coordenador", "Descrição da Máquina", "Uniorgs", "Descrição das Uniorgs", "Categoria" } });

                model.FiltroAreaTabelaMaquina = new SelectViewModel("Área", "Todas as Áreas", true);
                model.FiltroCoordenadorTabelaMaquina = new SelectViewModel("Coordenador", "Todos os Coordenadores", true);

                var areas = _db.Areas
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .OrderBy(a => a.Alias)
                    .ToList();

                foreach (var _area in _db.Areas.OrderBy(c => c.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTabelaMaquina.Value.Options.Add(key, _area.Alias);
                }

                var coordenadores = _db.Coordenadores
                    .Where(c => c.Uniorgs.Any())
                    .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                    .OrderBy(c => c.Nome);

                foreach (var _coordenador in coordenadores)
                {
                    var key = Encrypting.Encrypt(_coordenador.Id.ToString());

                    model.FiltroCoordenadorTabelaMaquina.Value.Options.Add(key, _coordenador.Nome);
                }
            }

            if (model.FiltroAreaTabelaMaquina != null)
            {
                model.FiltroAreaTabelaMaquina.Value.OnChange = "filtrarTabelaMaquina(this)";
            }

            if (model.FiltroCoordenadorTabelaMaquina != null)
            {
                model.FiltroCoordenadorTabelaMaquina.Value.OnChange = "filtrarTabelaMaquina(this)";
            }

            model.TabelaMaquinas = GetTabelaMaquina(unidade, area, coordenador, isDesativado);

            return Json(model);
        }

        private CadastroMaquinaViewModel GetMaquinas(string unidade, string area, string coordenador, bool isDesativado)
        {
            var model = new CadastroMaquinaViewModel
            {
                FormMaquina = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarMaquina"),
                TabelaMaquinas = GetTabelaMaquina(unidade, area, coordenador, isDesativado),

                Maquina = new InputViewModel(),
                DescricaoMaquina = new InputViewModel(),
                CategoriaMaquina = new SelectViewModel(),

                UniorgContainer = new CustomViewModel(),
                UniorgMaquina = new SelectViewModel(),

                FiltroPlantaMaquina = new SelectViewModel("Todas as Unidades", true),
                FiltroAreaMaquina = new SelectViewModel("Todas as Áreas", true),
                FiltroCoordenadorMaquina = new SelectViewModel("Todos os Coordenadores", true),

                FiltroUnidadeTabelaMaquina = new SelectViewModel("Unidade", "Todas as Unidades", true),
                FiltroAreaTabelaMaquina = new SelectViewModel("Área", "Todos as Áreas", true),
                FiltroCoordenadorTabelaMaquina = new SelectViewModel("Coordenador", "Todos os Coordenadores", true),

                DesativarMaquinaContainer = new StyleViewModel("display", "none"),
                DesativarMaquina = new CheckboxViewModel(),
                FiltroMostrarDesativadosMaquina = new CheckboxViewModel(isDesativado),
            };

            model.FiltroUnidadeTabelaMaquina.Value.OnChange = "filtrarTabelaMaquina(this)";
            model.FiltroAreaTabelaMaquina.Value.OnChange = "filtrarTabelaMaquina(this)";
            model.FiltroCoordenadorTabelaMaquina.Value.OnChange = "filtrarTabelaMaquina(this)";

            foreach (var categoria in _db.Categorias.OrderBy(c => c.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(categoria.Id.ToString());

                model.CategoriaMaquina.Value.Options.Add(key, categoria.Descricao);
            }

            foreach (var planta in _db.Plantas.OrderBy(c => c.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.FiltroPlantaMaquina.Value.Options.Add(key, planta.Descricao);
                model.FiltroUnidadeTabelaMaquina.Value.Options.Add(key, planta.Descricao);
            }

            var areas = _db.Areas
                .OrderBy(c => c.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.FiltroAreaMaquina.Value.Options.Add(key, _area.Alias);
            }

            areas = areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(c => c.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.FiltroAreaTabelaMaquina.Value.Options.Add(key, _area.Alias);
            }

            var coordenadores = _db.Coordenadores
                .OrderBy(c => c.Nome)
                .ToList();

            foreach (var _coordenador in coordenadores)
            {
                var key = Encrypting.Encrypt(_coordenador.Id.ToString());

                model.FiltroCoordenadorMaquina.Value.Options.Add(key, _coordenador.Nome);
            }

            coordenadores = coordenadores
                .Where(c => c.Uniorgs.Any())
                .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                .Where(c => c.Uniorgs.SelectMany(m => m.Colaboradores).Any())
                .OrderBy(c => c.Nome)
                .ToList();

            foreach (var _coordenador in coordenadores)
            {
                var key = Encrypting.Encrypt(_coordenador.Id.ToString());

                model.FiltroCoordenadorTabelaMaquina.Value.Options.Add(key, _coordenador.Nome);
            }

            foreach (var uniorg in _db.Uniorgs.Where(u => u.Maquinas.Count == 0).OrderBy(c => c.DescricaoUniorg).ToList())
            {
                var key = Encrypting.Encrypt(uniorg.Id.ToString());

                model.UniorgMaquina.Value.Options.Add(key, uniorg.DescricaoUniorg);
            }

            return model;
        }

        private TableViewModel GetTabelaMaquina(string unidade, string area, string coordenador, bool isDesativado)
        {
            int? plantaId = null;
            int? areaId = null;
            int? coordenadorId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(coordenador), out int coordenadorAux))
            {
                coordenadorId = coordenadorAux;
            }

            TableViewModel model = new TableViewModel();

            model.Value.OnClick = @"
                Binder.Bind('/MatrizHabilidade/Cadastro/PopularMaquina', 
                { 
                    Maquina: this.getAttribute('data-key'),
                    unidade: document.querySelector('select[name=FiltroUnidadeTabelaMaquina]').value,
                    area: document.querySelector('select[name=FiltroAreaTabelaMaquina]').value,
                    coordenador: document.querySelector('select[name=FiltroCoordenadorTabelaMaquina]').value,
                    isDesativado: document.querySelector('input[type=checkbox][name=FiltroMostrarDesativadosMaquina]').checked,
                })";


            List<Maquina> maquinas = new List<Maquina>();

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Responsável", "Descrição da Máquina", "Uniorgs", "Descrição das Uniorgs", "Categoria", "Desativado" } });
                maquinas = _db.Maquinas.OrderBy(m => m.Descricao).Distinct().ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Responsável", "Descrição da Máquina", "Uniorgs", "Descrição das Uniorgs", "Categoria" } });
                maquinas = _db.Maquinas.OrderBy(m => m.Descricao).Distinct().Where(i => i.IsAtivo).ToList();
            }

            if (coordenadorId.HasValue)
            {
                maquinas = maquinas.Where(m => m.Uniorgs.Any(u => u.Coordenador.Id == coordenadorId.Value)).ToList();
            }
            else if (areaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.AreaId == areaId.Value).ToList();
            }
            else if (plantaId.HasValue)
            {
                maquinas = maquinas.Where(m => m.Uniorgs.Any(u => u.Coordenador.Area.PlantaId == plantaId.Value)).ToList();
            }

            foreach (var maquina in maquinas.ToList())
            {
                string unidades = "";
                string areas = "";
                string coordenadores = "";
                string uniorgNumeros = "";
                string uniorgDescricoes = "";

                if (maquina.Uniorgs.Any())
                {
                    unidades = maquina.Area.Planta.Descricao;
                    areas = maquina.Area.Descricao;
                    coordenadores = string.Join(", ", maquina.Uniorgs.Select(u => u.Coordenador.Nome).Distinct());
                    uniorgNumeros = string.Join(", ", maquina.Uniorgs.Select(u => u.UniorgNumero));
                    uniorgDescricoes = string.Join(", ", maquina.Uniorgs.Select(u => u.DescricaoUniorg));
                }

                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        unidades,
                        areas,
                        coordenadores,
                        maquina.Descricao,
                        uniorgNumeros,
                        uniorgDescricoes,
                        maquina.CategoriaId.HasValue ? maquina.Categoria.Descricao : "",
                        maquina.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        unidades,
                        areas,
                        coordenadores,
                        maquina.Descricao,
                        uniorgNumeros,
                        uniorgDescricoes,
                        maquina.CategoriaId.HasValue ? maquina.Categoria.Descricao : "",
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(maquina.Id.ToString()),
                    Class = maquina.HasError ? "error" : "",
                    Values = values,
                });
            }

            return model;
        }
        #endregion

        #region Tipo de Treinamento
        public ActionResult LimparTipoTreinamento()
        {
            var model = GetTipoTreinamentos(false);

            return Json(model);
        }

        public ActionResult CadastrarTipoTreinamento(List<string> CategoriasTipoTreinamento, string DescricaoTipoTreinamento, string[] Maquinas, string CorTituloTipoTreinamento, string CorTreinamentoTipoTreinamento, string CorFonteTipoTreinamento, bool FiltroMostrarDesativadosTipoTreinamento)
        {
            if (CategoriasTipoTreinamento == null)
            {
                var errorResult = GetTipoTreinamentos(FiltroMostrarDesativadosTipoTreinamento);

                errorResult.Alerta = new AlertaViewModel(@"Deve-se selecionar ao menos uma ""Categoria""");

                return Json(errorResult);
            }

            var tipoTreinamento = new TipoTreinamento()
            {
                Descricao = DescricaoTipoTreinamento,
                CorTitulo = CorTituloTipoTreinamento,
                CorTreinamento = CorTreinamentoTipoTreinamento,
                CorFonte = CorFonteTipoTreinamento,
            };

            _db.TiposTreinamentos.Add(tipoTreinamento);

            _db.SaveChanges();

            if (CategoriasTipoTreinamento != null)
            {
                foreach (var categoria in CategoriasTipoTreinamento)
                {
                    if (int.TryParse(Encrypting.Decrypt(categoria), out int categoriaId))
                    {
                        var _categoria = _db.Categorias.Find(categoriaId);

                        tipoTreinamento.Categorias.Add(_categoria);

                        _db.SaveChanges();
                    }
                }
            }

            if (Maquinas == null)
            {
                foreach (var maquina in _db.Maquinas.Where(i => i.IsAtivo).ToList())
                {
                    tipoTreinamento.Maquinas.Add(maquina);

                    _db.SaveChanges();
                }
            }
            else if (Maquinas.Contains(""))
            {
                foreach (var maquina in _db.Maquinas.Where(i => i.IsAtivo).ToList())
                {
                    tipoTreinamento.Maquinas.Add(maquina);

                    _db.SaveChanges();
                }
            }
            else
            {
                foreach (var maquina in Maquinas)
                {
                    if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
                    {
                        var _maquina = _db.Maquinas.Find(maquinaId);

                        tipoTreinamento.Maquinas.Add(_maquina);

                        _db.SaveChanges();
                    }
                }
            }

            var result = GetTipoTreinamentos(FiltroMostrarDesativadosTipoTreinamento);

            result.Alerta = new AlertaViewModel("Cadastrado com Sucesso");

            return Json(result);
        }

        public ActionResult AtualizarTipoTreinamento(string TipoTreinamento, List<string> CategoriasTipoTreinamento, string DescricaoTipoTreinamento, string[] Maquinas, bool? DesativarTipoTreinamento, string CorTituloTipoTreinamento, string CorTreinamentoTipoTreinamento, string CorFonteTipoTreinamento, bool FiltroMostrarDesativadosTipoTreinamento)
        {
            if (CategoriasTipoTreinamento == null)
            {
                var errorResult = GetTipoTreinamentos(FiltroMostrarDesativadosTipoTreinamento);

                errorResult.Alerta = new AlertaViewModel(@"Deve-se selecionar ao menos uma ""Categoria""");

                return Json(errorResult);
            }

            if (int.TryParse(Encrypting.Decrypt(TipoTreinamento), out int tipoTreinamentoId))
            {
                bool isAtivo = true;

                if (DesativarTipoTreinamento.HasValue)
                {
                    if (DesativarTipoTreinamento.Value)
                    {
                        isAtivo = false;
                    }
                }

                var _tipoTreinamento = _db.TiposTreinamentos.Find(tipoTreinamentoId);

                _tipoTreinamento.Descricao = DescricaoTipoTreinamento;
                _tipoTreinamento.IsAtivo = isAtivo;

                _tipoTreinamento.CorTitulo = CorTituloTipoTreinamento;
                _tipoTreinamento.CorTreinamento = CorTreinamentoTipoTreinamento;
                _tipoTreinamento.CorFonte = CorFonteTipoTreinamento;

                _db.SaveChanges();

                if (isAtivo)
                {
                    if (CategoriasTipoTreinamento != null)
                    {
                        var _categorias = _tipoTreinamento.Categorias.ToList();

                        foreach (var categoria in CategoriasTipoTreinamento)
                        {
                            if (int.TryParse(Encrypting.Decrypt(categoria), out int categoriaId))
                            {
                                var index = _categorias.FindIndex(m => m.Id == categoriaId);

                                if (index >= 0)
                                {
                                    _categorias[index].ShouldRemain = true;
                                }
                                else
                                {
                                    _tipoTreinamento.Categorias.Add(_db.Categorias.Find(categoriaId));

                                    _db.SaveChanges();
                                }
                            }
                        }

                        foreach (var categoria in _categorias)
                        {
                            if (!categoria.ShouldRemain)
                            {
                                _tipoTreinamento.Categorias.Remove(categoria);

                                _db.SaveChanges();
                            }
                        }
                    }

                    if (Maquinas == null)
                    {
                        foreach (var maquina in _db.Maquinas.Where(i => i.IsAtivo).ToList())
                        {
                            if (!_tipoTreinamento.Maquinas.Contains(maquina))
                            {
                                _tipoTreinamento.Maquinas.Add(maquina);
                            }

                            _db.SaveChanges();
                        }
                    }
                    else if (Maquinas.Contains(""))
                    {
                        foreach (var maquina in _db.Maquinas.Where(i => i.IsAtivo).ToList())
                        {
                            if (!_tipoTreinamento.Maquinas.Contains(maquina))
                            {
                                _tipoTreinamento.Maquinas.Add(maquina);
                            }

                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        var _maquinas = _tipoTreinamento.Maquinas.ToList();

                        foreach (var maquina in Maquinas)
                        {
                            if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
                            {
                                var index = _maquinas.FindIndex(m => m.Id == maquinaId);

                                if (index >= 0)
                                {
                                    _maquinas[index].ShouldRemain = true;
                                }
                                else
                                {
                                    if (!_tipoTreinamento.Maquinas.Contains(_maquinas[index]))
                                    {
                                        _tipoTreinamento.Maquinas.Add(_maquinas[index]);
                                    }

                                    _db.SaveChanges();
                                }
                            }
                        }

                        foreach (var maquina in _maquinas)
                        {
                            if (!maquina.ShouldRemain)
                            {
                                _tipoTreinamento.Maquinas.Remove(maquina);

                                _db.SaveChanges();
                            }
                        }
                    }
                }
            }

            _db.SaveChanges();

            var result = GetTipoTreinamentos(FiltroMostrarDesativadosTipoTreinamento);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult FiltrarTipoTreinamento(string planta, string area)
        {
            var model = new CadastroTipoTreinamentoViewModel();

            int? plantaId = null;
            int? areaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (plantaId.HasValue)
            {
                model.MaquinaTipoTreinamento = new SelectViewModel("Todas as Máquinas", true);
                model.FiltroAreaTipoTreinamento = new SelectViewModel("Todas as Áreas", true);

                var _planta = _db.Plantas.Find(plantaId);

                foreach (var _area in _planta.Areas.Where(a => a.Coordenadores.Any()).OrderBy(a => a.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTipoTreinamento.Value.Options.Add(key, _area.Alias);

                    var maquinas = _area.Coordenadores
                        .Where(c => c.Uniorgs.Any())
                        .SelectMany(c => c.Uniorgs)
                        .SelectMany(c => c.Maquinas)
                        .OrderBy(m => m.Descricao)
                        .ToList();

                    foreach (var maquina in maquinas)
                    {
                        var maquinaKey = Encrypting.Encrypt(maquina.Id.ToString());

                        if (!model.MaquinaTipoTreinamento.Value.Options.ContainsKey(maquinaKey))
                        {
                            model.MaquinaTipoTreinamento.Value.Options.Add(maquinaKey, maquina.Descricao);
                        }
                    }
                }

                if (areaId.HasValue)
                {
                    var index = model.FiltroAreaTipoTreinamento.Value.Options.Keys.ToList().FindIndex(p => p == area);

                    if (index >= 0)
                    {
                        model.FiltroAreaTipoTreinamento.Value.SelectedIndex = index;
                    }
                    else
                    {
                        areaId = null;
                    }
                }
            }

            if (areaId.HasValue)
            {
                model.MaquinaTipoTreinamento = new SelectViewModel("Todas as Máquinas", true);

                var _area = _db.Areas.Find(areaId);

                var maquinas = _area.Maquinas
                    .OrderBy(m => m.Descricao)
                    .ToList();

                foreach (var maquina in maquinas)
                {
                    var maquinaKey = Encrypting.Encrypt(maquina.Id.ToString());

                    if (!model.MaquinaTipoTreinamento.Value.Options.ContainsKey(maquinaKey))
                    {
                        model.MaquinaTipoTreinamento.Value.Options.Add(maquinaKey, maquina.Descricao);
                    }
                }
            }

            if (!plantaId.HasValue && !areaId.HasValue)
            {
                model.MaquinaTipoTreinamento = new SelectViewModel("Todas as Máquinas", true);
                model.FiltroAreaTipoTreinamento = new SelectViewModel("Todas as Áreas", true);

                foreach (var maquina in _db.Maquinas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
                {
                    var key = Encrypting.Encrypt(maquina.Id.ToString());

                    model.MaquinaTipoTreinamento.Value.Options.Add(key, maquina.Descricao);
                }

                foreach (var _area in _db.Areas.OrderBy(p => p.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTipoTreinamento.Value.Options.Add(key, _area.Alias);
                }
            }

            return Json(model);
        }

        public ActionResult PopularTipoTreinamento(string tipoTreinamento, bool isDesativado)
        {
            var model = GetTipoTreinamentos(isDesativado);

            if (int.TryParse(Encrypting.Decrypt(tipoTreinamento), out int tipoTreinamentoId))
            {
                var _tipoTreinamento = _db.TiposTreinamentos.Find(tipoTreinamentoId);

                var quantidadeMaquinas = _db.Maquinas.Count();

                if (_tipoTreinamento.Maquinas.Count == quantidadeMaquinas)
                {
                    model.MaquinaTipoTreinamentoContainer.Value += $@"
                        <li title=""Todas as Máquinas"" onclick=""onMaquinaTipoTreinamentoRemoved(this)"">
                            <input type=""hidden"" name=""Maquinas"" value="""" />
                            <span>Todas as Máquinas</span>
                        </li>";
                }
                else
                {
                    foreach (var maquina in _tipoTreinamento.Maquinas)
                    {
                        var key = Encrypting.Encrypt(maquina.Id.ToString());

                        model.MaquinaTipoTreinamentoContainer.Value += $@"
                            <li title=""{maquina.Descricao}"" onclick=""onMaquinaTipoTreinamentoRemoved(this)"">
                                <input type=""hidden"" name=""Maquinas"" value=""{key}"" />
                                <span>{maquina.Descricao}</span>
                            </li>";

                        model.MaquinaTipoTreinamento.Value.Options.Remove(key);
                    }
                }

                foreach (var categoria in _tipoTreinamento.Categorias)
                {
                    var key = Encrypting.Encrypt(categoria.Id.ToString());

                    model.CategoriaTipoTreinamentoContainer.Value += $@"
                        <li title=""{categoria.Descricao}"" onclick=""onCategoriaTipoTreinamentoRemoved(this)"">
                            <input type=""hidden"" name=""CategoriasTipoTreinamento"" value=""{key}"" />
                            <span>{categoria.Descricao}</span>
                        </li>";
                }

                model.FormTipoTreinamento = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarTipoTreinamento");
                model.DescricaoTipoTreinamento = new InputViewModel(_tipoTreinamento.Descricao);
                model.TipoTreinamento = new InputViewModel(tipoTreinamento);

                model.CorTituloTipoTreinamento = new InputViewModel(_tipoTreinamento.CorTitulo);
                model.CorTreinamentoTipoTreinamento = new InputViewModel(_tipoTreinamento.CorTreinamento);
                model.CorFonteTipoTreinamento = new InputViewModel(_tipoTreinamento.CorFonte);

                model.DesativarTipoTreinamento = new CheckboxViewModel(!_tipoTreinamento.IsAtivo);
                model.DesativarTipoTreinamentoContainer = new StyleViewModel("display", "block");
            }

            return Json(model);
        }

        public ActionResult FiltrarTabelaTipoTreinamento(bool isDesativado)
        {
            var model = GetTipoTreinamentos(isDesativado);

            return Json(model);
        }

        private CadastroTipoTreinamentoViewModel GetTipoTreinamentos(bool isDesativado)
        {
            var model = new CadastroTipoTreinamentoViewModel
            {
                FormTipoTreinamento = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarTipoTreinamento"),
                TabelaTipoTreinamentos = GetTabelaTipoTreinamento(isDesativado),

                TipoTreinamento = new InputViewModel(),
                DescricaoTipoTreinamento = new InputViewModel(),

                CorFonteTipoTreinamento = new InputViewModel(),
                CorTituloTipoTreinamento = new InputViewModel(),
                CorTreinamentoTipoTreinamento = new InputViewModel(),

                FiltroPlantaTipoTreinamento = new SelectViewModel("Todas as Unidades", true),
                FiltroAreaTipoTreinamento = new SelectViewModel("Todoas as Áreas", true),

                CategoriaTipoTreinamento = new SelectViewModel(),
                CategoriaTipoTreinamentoContainer = new CustomViewModel(),

                MaquinaTipoTreinamentoContainer = new CustomViewModel(),
                MaquinaTipoTreinamento = new SelectViewModel("Todas as Máquinas", true),

                DesativarTipoTreinamento = new CheckboxViewModel(),
                DesativarTipoTreinamentoContainer = new StyleViewModel("display", "none"),

                FiltroMostrarDesativadosTipoTreinamento = new CheckboxViewModel(isDesativado),
            };

            foreach (var planta in _db.Plantas.OrderBy(t => t.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.FiltroPlantaTipoTreinamento.Value.Options.Add(key, planta.Descricao);
            }

            foreach (var area in _db.Areas.OrderBy(p => p.Alias).ToList())
            {
                var key = Encrypting.Encrypt(area.Id.ToString());

                model.FiltroAreaTipoTreinamento.Value.Options.Add(key, area.Alias);
            }

            foreach (var maquina in _db.Maquinas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(maquina.Id.ToString());

                model.MaquinaTipoTreinamento.Value.Options.Add(key, maquina.Descricao);
            }

            foreach (var categoria in _db.Categorias.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(categoria.Id.ToString());

                model.CategoriaTipoTreinamento.Value.Options.Add(key, categoria.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaTipoTreinamento(bool isDesativado)
        {
            var model = new TableViewModel();
            List<TipoTreinamento> tipoTreinamentos;

            model.Value.OnClick = "Binder.Bind('/MatrizHabilidade/Cadastro/PopularTipoTreinamento', { tipoTreinamento: this.getAttribute('data-key'), isDesativado: document.querySelector('input[type=checkbox][name=FiltroMostrarDesativadosTipoTreinamento]').checked })";

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição do Tipo de Treinamento", "Categorias", "Máquinas", "Desativado" } });
                tipoTreinamentos = _db.TiposTreinamentos.OrderBy(t => t.Descricao).ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição do Tipo de Treinamento", "Categorias", "Máquinas" } });
                tipoTreinamentos = _db.TiposTreinamentos.OrderBy(t => t.Descricao).Where(i => i.IsAtivo).ToList();
            }

            foreach (var tipo in tipoTreinamentos)
            {
                List<Maquina> maquinas = new List<Maquina>();
                List<Categoria> categorias = new List<Categoria>();

                if (tipo.Maquinas != null)
                {
                    maquinas = tipo.Maquinas.ToList();
                }

                if (tipo.Categorias != null)
                {
                    categorias = tipo.Categorias.ToList();
                }

                var totalMaquinas = _db.Maquinas.Count();
                var totalCategorias = _db.Categorias.Count();

                string colunaMaquinas = "";
                string colunaCategorias = "";

                if (maquinas.Any())
                {
                    if (totalMaquinas == maquinas.Count())
                    {
                        colunaMaquinas = "Todas";
                    }
                    else
                    {
                        colunaMaquinas = string.Join(", ", maquinas.Select(m => m.Descricao));
                    }
                }
                else
                {
                    colunaMaquinas = "";
                }

                if (categorias.Any())
                {
                    if (totalCategorias == categorias.Count())
                    {
                        colunaCategorias = "Todas";
                    }
                    else
                    {
                        colunaCategorias = string.Join(", ", categorias.Select(m => m.Descricao));
                    }
                }
                else
                {
                    colunaCategorias = "";
                }

                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        tipo.Descricao,
                        colunaCategorias,
                        colunaMaquinas,
                        tipo.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        tipo.Descricao,
                        colunaCategorias,
                        colunaMaquinas,
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(tipo.Id.ToString()),
                    Values = values
                });
            }

            return model;
        }
        #endregion

        #region Treinamento
        public ActionResult LimparTreinamento()
        {
            var model = GetTreinamentos(false);

            return Json(model);
        }

        public ActionResult CadastrarTreinamento(string DescricaoTreinamento, string TipoTreinamento, bool FiltroMostrarDesativadosTreinamento)
        {
            if (int.TryParse(Encrypting.Decrypt(TipoTreinamento), out int tipoTreinamentoId))
            {
                var treinamento = new Treinamento()
                {
                    Descricao = DescricaoTreinamento,
                    DataCriacao = DateTime.Now,
                    TipoTreinamentoId = tipoTreinamentoId,
                };

                _db.Treinamentos.Add(treinamento);

                _db.SaveChanges();
            }

            var result = GetTreinamentos(FiltroMostrarDesativadosTreinamento);

            result.Alerta = new AlertaViewModel("Cadastrado com Sucesso");

            return Json(result);
        }

        public ActionResult AtualizarTreinamento(string Treinamento, string DescricaoTreinamento, string TipoTreinamento, bool? DesativarTreinamento, bool FiltroMostrarDesativadosTreinamento)
        {
            if (int.TryParse(Encrypting.Decrypt(Treinamento), out int treinamentoId))
            {
                if (int.TryParse(Encrypting.Decrypt(TipoTreinamento), out int tipoTreinamentoId))
                {
                    bool isAtivo = true;

                    if (DesativarTreinamento.HasValue)
                    {
                        if (DesativarTreinamento.Value)
                        {
                            isAtivo = false;
                        }
                    }

                    var _treinamento = _db.Treinamentos.Find(treinamentoId);

                    _treinamento.Descricao = DescricaoTreinamento;
                    _treinamento.IsAtivo = isAtivo;
                    _treinamento.TipoTreinamentoId = tipoTreinamentoId;

                    _db.SaveChanges();
                }
            }

            var result = GetTreinamentos(FiltroMostrarDesativadosTreinamento);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult FiltrarTabelaTreinamento(string tipo, bool isDesativado)
        {
            var model = new CadastroTreinamentoViewModel
            {
                TabelaTreinamentos = GetTabelaTreinamento(tipo, isDesativado)
            };

            return Json(model);
        }

        public ActionResult PopularTreinamento(string treinamento, bool isDesativado)
        {
            var model = GetTreinamentos(isDesativado);

            if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
            {
                var _treinamento = _db.Treinamentos.Find(treinamentoId);

                var tipoTreinamento = Encrypting.Encrypt(_treinamento.TipoTreinamentoId.ToString());

                int tipoTreinamentoSelecionado = model.TipoTreinamento.Value.Options.Keys.ToList().FindIndex(p => p == tipoTreinamento);

                model.FormTreinamento = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarTreinamento");
                model.DescricaoTreinamento = new InputViewModel(_treinamento.Descricao);
                model.TipoTreinamento.Value.SelectedIndex = tipoTreinamentoSelecionado;
                model.Treinamento = new InputViewModel(treinamento);

                model.DesativarTreinamentoContainer = new StyleViewModel("display", "block");
                model.DesativarTreinamento = new CheckboxViewModel(!_treinamento.IsAtivo);
            }

            return Json(model);
        }

        private CadastroTreinamentoViewModel GetTreinamentos(bool isDesativado)
        {
            var model = new CadastroTreinamentoViewModel
            {
                FormTreinamento = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarTreinamento"),
                TabelaTreinamentos = GetTabelaTreinamento(null, isDesativado),
                Treinamento = new InputViewModel(),
                DescricaoTreinamento = new InputViewModel(),
                TipoTreinamento = new SelectViewModel(),
                FiltroTipoTabelaTreinamento = new SelectViewModel("Tipo Treinamento", "Todos os Tipos de Treinamento", true),

                DesativarTreinamentoContainer = new StyleViewModel("display", "none"),
                DesativarTreinamento = new CheckboxViewModel(),
                FiltroMostrarDesativadosTreinamento = new CheckboxViewModel(isDesativado),
            };

            model.FiltroTipoTabelaTreinamento.Value.OnChange = "filtrarTreinamento(this)";

            foreach (var tipo in _db.TiposTreinamentos.OrderBy(t => t.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(tipo.Id.ToString());

                model.TipoTreinamento.Value.Options.Add(key, tipo.Descricao);
                model.FiltroTipoTabelaTreinamento.Value.Options.Add(key, tipo.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaTreinamento(string tipo, bool isDesativado)
        {
            var model = new TableViewModel();
            List<Treinamento> treinamentos;

            model.Value.OnClick = "Binder.Bind('/MatrizHabilidade/Cadastro/PopularTreinamento', { treinamento: this.getAttribute('data-key'), isDesativado: document.querySelector('input[type=hidden][name=FiltroMostrarDesativadosTreinamento]').value })";

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Tipo", "Descrição do Treinamento", "Desativado" } });
                treinamentos = _db.Treinamentos.OrderBy(t => t.Descricao).ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Tipo", "Descrição do Treinamento" } });
                treinamentos = _db.Treinamentos.OrderBy(t => t.Descricao).Where(i => i.IsAtivo).ToList();
            }

            if (int.TryParse(Encrypting.Decrypt(tipo), out int tipoId))
            {
                treinamentos = treinamentos.Where(t => t.Id == tipoId).ToList();
            }

            foreach (var treinamento in treinamentos)
            {
                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        treinamento.TipoTreinamento.Descricao,
                        treinamento.Descricao,
                        treinamento.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        treinamento.TipoTreinamento.Descricao,
                        treinamento.Descricao
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(treinamento.Id.ToString()),
                    Values = values,
                });
            }

            return model;
        }
        #endregion

        #region Treinamento Específico
        public ActionResult LimparTreinamentoEspecifico()
        {
            var model = GetTreinamentoEspecificos(null, null, null, null, false);

            return Json(model);
        }

        public ActionResult CadastrarTreinamentoEspecifico(string DescricaoTreinamentoEspecifico, string[] Maquinas, string CategoriaTreinamentoEspecifico, string FiltroUnidadeTabelaTreinamentoEspecifico, string FiltroAreaTabelaTreinamentoEspecifico, string FiltroMaquinaTabelaTreinamentoEspecifico, string FiltroDescricaoTreinamentoEspecifico, bool FiltroMostrarDesativadosTreinamentoEspecifico)
        {
            if (int.TryParse(Encrypting.Decrypt(CategoriaTreinamentoEspecifico), out int categoriaId))
            {
                var treinamento = new TreinamentoEspecifico()
                {
                    Descricao = DescricaoTreinamentoEspecifico,
                    DataCriacao = DateTime.Now,
                    CategoriaId = categoriaId,
                };

                _db.TreinamentosEspecificos.Add(treinamento);

                _db.SaveChanges();

                treinamento.Maquinas = new List<Maquina>();

                foreach (var maquina in Maquinas)
                {
                    if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
                    {
                        var _maquina = _db.Maquinas.Find(maquinaId);

                        treinamento.Maquinas.Add(_maquina);

                        _db.SaveChanges();
                    }
                }
            }

            var result = GetTreinamentoEspecificos(FiltroUnidadeTabelaTreinamentoEspecifico, FiltroAreaTabelaTreinamentoEspecifico, FiltroMaquinaTabelaTreinamentoEspecifico, FiltroDescricaoTreinamentoEspecifico, FiltroMostrarDesativadosTreinamentoEspecifico);

            result.Alerta = new AlertaViewModel("Cadastrado com Sucesso");

            return Json(result);
        }

        public ActionResult AtualizarTreinamentoEspecifico(string TreinamentoEspecifico, string DescricaoTreinamentoEspecifico, string[] Maquinas, bool? DesativarTreinamentoEspecífico, string CategoriaTreinamentoEspecifico, string FiltroUnidadeTabelaTreinamentoEspecifico, string FiltroAreaTabelaTreinamentoEspecifico, string FiltroMaquinaTabelaTreinamentoEspecifico, string FiltroDescricaoTreinamentoEspecifico, bool FiltroMostrarDesativadosTreinamentoEspecifico)
        {
            if (int.TryParse(Encrypting.Decrypt(TreinamentoEspecifico), out int treinamentoId))
            {
                if (int.TryParse(Encrypting.Decrypt(CategoriaTreinamentoEspecifico), out int categoriaId))
                {
                    bool isAtivo = true;

                    if (DesativarTreinamentoEspecífico.HasValue)
                    {
                        if (DesativarTreinamentoEspecífico.Value)
                        {
                            isAtivo = false;
                        }
                    }

                    var _treinamento = _db.TreinamentosEspecificos.Find(treinamentoId);

                    _treinamento.Descricao = DescricaoTreinamentoEspecifico;
                    _treinamento.IsAtivo = isAtivo;
                    _treinamento.CategoriaId = categoriaId;

                    var _maquinas = _treinamento.Maquinas.ToList();

                    if (Maquinas != null)
                    {
                        for (int x = 0; x < Maquinas.Length; x++)
                        {
                            if (int.TryParse(Encrypting.Decrypt(Maquinas[x]), out int maquinaId))
                            {
                                var index = _maquinas.FindIndex(u => u.Id == maquinaId);

                                if (index >= 0)
                                {
                                    _maquinas[index].ShouldRemain = true;
                                }
                                else
                                {
                                    var _maquina = _db.Maquinas.Find(maquinaId);

                                    _treinamento.Maquinas.Add(_maquina);

                                    _db.SaveChanges();
                                }
                            }
                        }
                    }

                    foreach (var maquina in _maquinas)
                    {
                        if (!maquina.ShouldRemain)
                        {
                            _treinamento.Maquinas.Remove(maquina);

                            _db.SaveChanges();
                        }
                    }

                    _db.SaveChanges();
                }
            }

            var result = GetTreinamentoEspecificos(FiltroUnidadeTabelaTreinamentoEspecifico, FiltroAreaTabelaTreinamentoEspecifico, FiltroMaquinaTabelaTreinamentoEspecifico, FiltroDescricaoTreinamentoEspecifico, FiltroMostrarDesativadosTreinamentoEspecifico);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult PopularTreinamentoEspecifico(string treinamento, string unidade, string area, string maquina, string descricao, bool isDesativado)
        {
            var model = GetTreinamentoEspecificos(unidade, area, maquina, descricao, isDesativado);

            if (int.TryParse(Encrypting.Decrypt(treinamento), out int treinamentoId))
            {
                var _treinamento = _db.TreinamentosEspecificos.Find(treinamentoId);

                model.FormTreinamentoEspecifico = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarTreinamentoEspecifico");
                model.DescricaoTreinamentoEspecifico = new InputViewModel(_treinamento.Descricao);
                model.TreinamentoEspecifico = new InputViewModel(treinamento);

                model.DesativarTreinamentoEspecificoContainer = new StyleViewModel("display", "block");
                model.DesativarTreinamentoEspecífico = new CheckboxViewModel(!_treinamento.IsAtivo);

                var categoria = Encrypting.Encrypt(_treinamento.CategoriaId.ToString());
                int categoriaSelecionada = model.CategoriaTreinamentoEspecifico.Value.Options.Keys.ToList().FindIndex(p => p == categoria);
                model.CategoriaTreinamentoEspecifico.Value.SelectedIndex = categoriaSelecionada;

                foreach (var _maquina in _treinamento.Maquinas)
                {
                    var key = Encrypting.Encrypt(_maquina.Id.ToString());

                    var query = model.MaquinaTreinamentoEspecifico.Value.Options.Keys.Where(m => m == key);

                    if (query.Any())
                    {
                        model.MaquinaTreinamentoEspecifico.Value.Options.Remove(query.First());
                    }

                    model.MaquinaTreinamentoEspecificoContainer.Value += $@"
                        <li title=""{_maquina.Descricao}"" onclick=""onMaquinaTreinamentoEspecificoRemoved(this)"">
                            <input type=""hidden"" name=""Maquinas"" value=""{key}"" />
                            <span>{_maquina.Descricao}</span>
                        </li>";
                }
            }

            return Json(model);
        }

        public ActionResult FiltrarMaquinaTreinamentoEspecifico(string unidade, string area)
        {
            var model = new CadastroTreinamentoEspecificoViewModel();

            int? plantaId = null;
            int? areaId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (plantaId.HasValue)
            {
                var _planta = _db.Plantas.Find(plantaId);

                model.MaquinaTreinamentoEspecifico = new SelectViewModel();
                model.FiltroAreaTreinamentoEspecifico = new SelectViewModel("Todos os Áreas", true);

                foreach (var _area in _planta.Areas.Where(a => a.Coordenadores.Any()).OrderBy(a => a.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTreinamentoEspecifico.Value.Options.Add(key, _area.Alias);

                    foreach (var maquina in _area.Maquinas)
                    {
                        var uniorgKey = Encrypting.Encrypt(maquina.Id.ToString());

                        if (!model.MaquinaTreinamentoEspecifico.Value.Options.ContainsKey(uniorgKey))
                        {
                            model.MaquinaTreinamentoEspecifico.Value.Options.Add(uniorgKey, maquina.Descricao);
                        }
                    }
                }

                if (areaId.HasValue)
                {
                    var index = model.FiltroAreaTreinamentoEspecifico.Value.Options.Keys.ToList().FindIndex(p => p == area);

                    if (index >= 0)
                    {
                        model.FiltroAreaTreinamentoEspecifico.Value.SelectedIndex = index;
                    }
                    else
                    {
                        areaId = null;
                    }
                }
            }

            if (areaId.HasValue)
            {
                var _area = _db.Areas.Find(areaId);

                model.MaquinaTreinamentoEspecifico = new SelectViewModel();

                foreach (var maquina in _area.Maquinas.ToList())
                {
                    var key = Encrypting.Encrypt(maquina.Id.ToString());

                    if (!model.MaquinaTreinamentoEspecifico.Value.Options.ContainsKey(key))
                    {
                        model.MaquinaTreinamentoEspecifico.Value.Options.Add(key, maquina.Descricao);
                    }
                }
            }

            if (!plantaId.HasValue && !areaId.HasValue)
            {
                foreach (var planta in _db.Plantas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
                {
                    var key = Encrypting.Encrypt(planta.Id.ToString());

                    model.FiltroUnidadeTreinamentoEspecifico.Value.Options.Add(key, planta.Descricao);
                }

                foreach (var areas in _db.Areas.OrderBy(m => m.Alias).ToList())
                {
                    var key = Encrypting.Encrypt(areas.Id.ToString());

                    model.FiltroAreaTreinamentoEspecifico.Value.Options.Add(key, areas.Alias);
                }

                foreach (var _maquina in _db.Maquinas.OrderBy(m => m.Descricao).Where(i => i.IsAtivo).ToList())
                {
                    var key = Encrypting.Encrypt(_maquina.Id.ToString());

                    model.MaquinaTreinamentoEspecifico.Value.Options.Add(key, _maquina.Descricao);
                }
            }

            return Json(model);
        }

        public ActionResult FiltrarTabelaTreinamentoEspecifico(string unidade, string area, string maquina, string descricao, bool isDesativado)
        {
            var model = new CadastroTreinamentoEspecificoViewModel();

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaAux))
            {
                maquinaId = maquinaAux;
            }

            if (plantaId.HasValue)
            {
                var _planta = _db.Plantas.Find(plantaId);

                model.FiltroAreaTabelaTreinamentoEspecifico = new SelectViewModel("Todas as Áreas", true);

                model.FiltroMaquinaTabelaTreinamentoEspecifico = new SelectViewModel("Todas as Máquinas", true);

                var areas = _planta.Areas
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                    .OrderBy(a => a.Alias).ToList();

                foreach (var _area in areas)
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTabelaTreinamentoEspecifico.Value.Options.Add(key, _area.Alias);

                    var maquinas = _area.Coordenadores
                        .Where(c => c.Uniorgs.Any())
                        .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                        .Where(c => c.Uniorgs.SelectMany(m => m.Colaboradores).Any())
                        .SelectMany(c => c.Uniorgs)
                        .SelectMany(c => c.Maquinas)
                        .OrderBy(c => c.Descricao)
                        .ToList();

                    foreach (var _maquina in maquinas)
                    {
                        var coordenadorKey = Encrypting.Encrypt(_maquina.Id.ToString());

                        if (!model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.ContainsKey(coordenadorKey))
                        {
                            model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Add(coordenadorKey, _maquina.Descricao);
                        }
                    }
                }

                if (areaId.HasValue)
                {
                    var index = model.FiltroAreaTabelaTreinamentoEspecifico.Value.Options.Keys.ToList().FindIndex(p => p == area);

                    if (index >= 0)
                    {
                        model.FiltroAreaTabelaTreinamentoEspecifico.Value.SelectedIndex = index;
                    }
                    else
                    {
                        areaId = null;
                        area = null;
                    }
                }

                if (maquinaId.HasValue)
                {
                    var index = model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Keys.ToList().FindIndex(p => p == maquina);

                    if (index >= 0)
                    {
                        model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.SelectedIndex = index;
                    }
                    else
                    {
                        maquinaId = null;
                        maquina = null;
                    }
                }
            }

            if (areaId.HasValue)
            {
                model.TabelaTreinamentoEspecificos = new TableViewModel();
                model.TabelaTreinamentoEspecificos.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidades", "Áreas", "Máquinas", "Descrição do Treinamento Específico" } });

                var _area = _db.Areas.Find(areaId);

                model.FiltroMaquinaTabelaTreinamentoEspecifico = new SelectViewModel("Todas as Máquinas", true);

                var maquinas = _area.Coordenadores
                    .Where(c => c.Uniorgs.Any())
                    .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                    .Where(c => c.Uniorgs.SelectMany(m => m.Colaboradores).Any())
                    .SelectMany(c => c.Uniorgs)
                    .SelectMany(c => c.Maquinas)
                    .OrderBy(c => c.Descricao)
                    .ToList();

                foreach (var _maquina in maquinas)
                {
                    var coordenadorKey = Encrypting.Encrypt(_maquina.Id.ToString());

                    if (!model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.ContainsKey(coordenadorKey))
                    {
                        model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Add(coordenadorKey, _maquina.Descricao);
                    }
                }

                if (maquinaId.HasValue)
                {
                    var index = model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Keys.ToList().FindIndex(p => p == maquina);

                    if (index >= 0)
                    {
                        model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.SelectedIndex = index;
                    }
                    else
                    {
                        maquinaId = null;
                        maquina = null;
                    }
                }
            }

            if (!plantaId.HasValue && !areaId.HasValue && !maquinaId.HasValue)
            {
                model.FiltroAreaTabelaTreinamentoEspecifico = new SelectViewModel("Todas as Áreas", true);

                model.FiltroMaquinaTabelaTreinamentoEspecifico = new SelectViewModel("Todas as Máquinas", true);

                var areas = _db.Areas
                    .Where(a => a.Coordenadores.Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                    .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                    .OrderBy(m => m.Alias)
                    .ToList();

                foreach (var _area in areas)
                {
                    var key = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaTabelaTreinamentoEspecifico.Value.Options.Add(key, _area.Alias);
                }

                var maquinas = _db.Maquinas
                    .Where(m => m.Uniorgs.Any())
                    .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                    .OrderBy(m => m.Descricao)
                    .Where(i => i.IsAtivo).ToList();

                foreach (var _maquina in maquinas)
                {
                    var key = Encrypting.Encrypt(_maquina.Id.ToString());

                    model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Add(key, _maquina.Descricao);
                }
            }

            var onChange = @"onFiltroTipoTreinamentoChanged(this)";

            if (model.FiltroAreaTabelaTreinamentoEspecifico != null)
            {
                model.FiltroAreaTabelaTreinamentoEspecifico.Value.OnChange = onChange;
            }

            if (model.FiltroMaquinaTabelaTreinamentoEspecifico != null)
            {
                model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.OnChange = onChange;
            }

            model.TabelaTreinamentoEspecificos = GetTabelaTreinamentoEspecifico(unidade, area, maquina, descricao, isDesativado);

            return Json(model);
        }

        private CadastroTreinamentoEspecificoViewModel GetTreinamentoEspecificos(string unidade, string area, string maquina, string descricao, bool isDesativado)
        {
            var model = new CadastroTreinamentoEspecificoViewModel
            {
                FormTreinamentoEspecifico = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarTreinamentoEspecifico"),
                TabelaTreinamentoEspecificos = GetTabelaTreinamentoEspecifico(unidade, area, maquina, descricao, isDesativado),

                TreinamentoEspecifico = new InputViewModel(),
                DescricaoTreinamentoEspecifico = new InputViewModel(),

                CategoriaTreinamentoEspecifico = new SelectViewModel(),

                FiltroUnidadeTreinamentoEspecifico = new SelectViewModel("Todas as Unidades", true),
                FiltroAreaTreinamentoEspecifico = new SelectViewModel("Todas as Áreas", true),

                FiltroUnidadeTabelaTreinamentoEspecifico = new SelectViewModel("Unidade", "Todas as Unidades", true),
                FiltroAreaTabelaTreinamentoEspecifico = new SelectViewModel("Área", "Todas as Áreas", true),
                FiltroMaquinaTabelaTreinamentoEspecifico = new SelectViewModel("Máquina", "Todas as Máquinas", true),

                MaquinaTreinamentoEspecifico = new SelectViewModel(),
                MaquinaTreinamentoEspecificoContainer = new CustomViewModel(),

                DesativarTreinamentoEspecificoContainer = new StyleViewModel("display", "none"),
                DesativarTreinamentoEspecífico = new CheckboxViewModel(),
                FiltroMostrarDesativadosTreinamentoEspecifico = new CheckboxViewModel(isDesativado),
            };

            var onChange = @"onFiltroTipoTreinamentoChanged(this)";

            model.FiltroUnidadeTabelaTreinamentoEspecifico.Value.OnChange = onChange;
            model.FiltroAreaTabelaTreinamentoEspecifico.Value.OnChange = onChange;
            model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.OnChange = onChange;

            foreach (var planta in _db.Plantas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());

                model.FiltroUnidadeTreinamentoEspecifico.Value.Options.Add(key, planta.Descricao);
                model.FiltroUnidadeTabelaTreinamentoEspecifico.Value.Options.Add(key, planta.Descricao);
            }

            var areas = _db.Areas
                .OrderBy(m => m.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.FiltroAreaTreinamentoEspecifico.Value.Options.Add(key, _area.Alias);
            }

            areas = areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(m => m.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.FiltroAreaTabelaTreinamentoEspecifico.Value.Options.Add(key, _area.Alias);
            }

            var maquinas = _db.Maquinas
                .OrderBy(m => m.Descricao)
                .Where(i => i.IsAtivo).ToList();

            foreach (var _maquina in maquinas)
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.MaquinaTreinamentoEspecifico.Value.Options.Add(key, _maquina.Descricao);
            }

            maquinas = maquinas
                .Where(m => m.Uniorgs.Any())
                .Where(m => m.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                .OrderBy(m => m.Descricao)
                .ToList();

            foreach (var _maquina in maquinas)
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.FiltroMaquinaTabelaTreinamentoEspecifico.Value.Options.Add(key, _maquina.Descricao);
            }

            foreach (var categoria in _db.Categorias.OrderBy(m => m.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(categoria.Id.ToString());

                model.CategoriaTreinamentoEspecifico.Value.Options.Add(key, categoria.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaTreinamentoEspecifico(string unidade, string area, string maquina, string descricao, bool isDesativado)
        {
            var model = new TableViewModel();
            model.Value.OnClick = @"
                Binder.Bind('/MatrizHabilidade/Cadastro/PopularTreinamentoEspecifico', 
                { 
                    treinamento: this.getAttribute('data-key'),
                    unidade: document.querySelector('[name=FiltroUnidadeTabelaTreinamentoEspecifico]').value,
                    area: document.querySelector('[name=FiltroAreaTabelaTreinamentoEspecifico]').value,
                    maquina: document.querySelector('[name=FiltroMaquinaTabelaTreinamentoEspecifico]').value,
                    descricao: document.querySelector('[name=FiltroDescricaoTreinamentoEspecifico]').value,
                    isDesativado: document.querySelector('input[type=checkbox][name=FiltroMostrarDesativadosTreinamentoEspecifico]').checked,
                })";

            int? plantaId = null;
            int? areaId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaAux))
            {
                maquinaId = maquinaAux;
            }

            List<TreinamentoEspecifico> treinamentos;

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidades", "Áreas", "Máquinas", "Descrição do Treinamento Específico", "Desativado" } });
                treinamentos = _db.TreinamentosEspecificos.ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidades", "Áreas", "Máquinas", "Descrição do Treinamento Específico" } });
                treinamentos = _db.TreinamentosEspecificos.Where(i => i.IsAtivo).ToList();
            }

            if (maquinaId.HasValue)
            {
                treinamentos = treinamentos.Where(t => t.Maquinas.Any(m => m.Id == maquinaId)).ToList();
            }
            else if (areaId.HasValue)
            {
                treinamentos = treinamentos.Where(t => t.Maquinas.SelectMany(m => m.Uniorgs).Select(u => u.Coordenador.Area).Any(a => a.Id == areaId)).ToList();
            }
            else if (plantaId.HasValue)
            {
                treinamentos = treinamentos.Where(t => t.Maquinas.SelectMany(m => m.Uniorgs).Select(u => u.Coordenador.Area).Any(a => a.PlantaId == plantaId)).ToList();
            }

            if (!string.IsNullOrEmpty(descricao))
            {
                treinamentos = treinamentos.Where(t => t.Descricao.Contains(descricao)).ToList();
            }

            foreach (var treinamento in treinamentos.Distinct().OrderBy(t => t.Descricao).ToList())
            {
                var unidades = treinamento.Maquinas
                    .Where(m => m.Uniorgs.Any())
                    .Select(m => m.Uniorgs.First().Coordenador.Area.Planta)
                    .Select(u => u.Descricao)
                    .Distinct()
                    .ToList();

                var areas = treinamento.Maquinas
                    .Where(m => m.Uniorgs.Any())
                    .Select(m => m.Uniorgs.First().Coordenador.Area)
                    .Select(u => u.Alias)
                    .Distinct()
                    .ToList();

                var maquinas = treinamento.Maquinas
                    .Select(u => u.Descricao)
                    .Distinct()
                    .ToList();

                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        string.Join(", ", unidades),
                        string.Join(", ", areas),
                        string.Join(", ", maquinas),
                        treinamento.Descricao,
                        treinamento.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        string.Join(", ", unidades),
                        string.Join(", ", areas),
                        string.Join(", ", maquinas),
                        treinamento.Descricao,
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(treinamento.Id.ToString()),
                    Values = values,
                });
            }

            return model;
        }
        #endregion

        #region Categoria
        public ActionResult LimparCategoria()
        {
            var model = GetCategorias(false);

            return Json(model);
        }

        public ActionResult CadastrarCategoria(string DescricaoCategoria, bool FiltroMostrarDesativadosCategoria)
        {
            string alerta;

            var categoria = new Categoria()
            {
                Descricao = DescricaoCategoria,
            };

            _db.Categorias.Add(categoria);

            _db.SaveChanges();

            alerta = "Cadastrado com Sucesso";

            var result = GetCategorias(FiltroMostrarDesativadosCategoria);

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult AtualizarCategoria(string Categoria, string DescricaoCategoria, bool? DesativarCategoria, bool FiltroMostrarDesativadosCategoria)
        {
            if (int.TryParse(Encrypting.Decrypt(Categoria), out int categoriaId))
            {
                bool isAtivo = true;

                if (DesativarCategoria.HasValue)
                {
                    if (DesativarCategoria.Value)
                    {
                        isAtivo = false;
                    }
                }

                var categoria = _db.Categorias.Find(categoriaId);

                categoria.Descricao = DescricaoCategoria;
                categoria.IsAtivo = isAtivo;

                _db.SaveChanges();
            }

            var result = GetCategorias(FiltroMostrarDesativadosCategoria);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult PopularCategoria(string categoria, bool isDesativado)
        {
            var model = GetCategorias(isDesativado);

            if (int.TryParse(Encrypting.Decrypt(categoria), out int categoriaId))
            {
                var _categoria = _db.Categorias.Find(categoriaId);

                model.FormCategoria = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarCategoria");
                model.DescricaoCategoria = new InputViewModel(_categoria.Descricao);
                model.Categoria = new InputViewModel(categoria);
                model.DesativarCategoriaContainer = new StyleViewModel("display", "block");
                model.DesativarCategoria = new CheckboxViewModel(!_categoria.IsAtivo);
            }

            return Json(model);
        }

        public ActionResult FiltrarCategoria(bool isDesativado)
        {
            var model = new CadastroCategoriaViewModel
            {
                TabelaCategorias = GetTabelaCategoria(isDesativado)
            };

            return Json(model);
        }

        private CadastroCategoriaViewModel GetCategorias(bool isDesativado)
        {
            var model = new CadastroCategoriaViewModel
            {
                FormCategoria = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarCategoria"),
                TabelaCategorias = GetTabelaCategoria(isDesativado),
                Categoria = new InputViewModel(""),
                DescricaoCategoria = new InputViewModel(""),

                DesativarCategoriaContainer = new StyleViewModel("display", "none"),
                DesativarCategoria = new CheckboxViewModel(),
                FiltroMostrarDesativadosCategoria = new CheckboxViewModel(isDesativado),
            };

            return model;
        }

        private TableViewModel GetTabelaCategoria(bool isDesativado)
        {
            var model = new TableViewModel();
            List<Categoria> categorias;

            model.Value.OnClick = "Binder.Bind('/MatrizHabilidade/Cadastro/PopularCategoria', { categoria: this.getAttribute('data-key'), isDesativado: document.querySelector('input[type=hidden][name=FiltroMostrarDesativadosCategoria]').value })";

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição da Categoria", "Desativado", } });
                categorias = _db.Categorias.OrderBy(p => p.Descricao).ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição da Categoria", } });
                categorias = _db.Categorias.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList();
            }

            foreach (var categoria in categorias)
            {
                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        categoria.Descricao,
                        categoria.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        categoria.Descricao,
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(categoria.Id.ToString()),
                    Values = values,
                });
            }

            return model;
        }
        #endregion

        #region Facilitador
        public ActionResult LimparFacilitador()
        {
            var model = GetFacilitadores(null, null, null, null, null);

            return Json(model);
        }

        public ActionResult CadastrarFacilitador(Dictionary<string, FacilitadorViewModel> facilitadores)
        {
            if (facilitadores != null)
            {
                foreach (var facilitador in facilitadores)
                {
                    if (facilitador.Value.IsEdited)
                    {
                        if (int.TryParse(Encrypting.Decrypt(facilitador.Key), out int colaboradorId))
                        {
                            var colaborador = _db.Colaboradores.Find(colaboradorId);

                            colaborador.IsFacilitador = facilitador.Value.IsFacilitador;

                            _db.SaveChanges();
                        }
                    }
                }
            }

            var result = new CadastroCategoriaViewModel
            {
                Alerta = new AlertaViewModel("Cadastrado com Sucesso")
            };

            return Json(result);
        }

        public ActionResult FiltrarFacilitador(string unidade, string area, string coordenador, string maquina, string nome)
        {
            var model = new CadastroFacilitadorViewModel
            {
                TabelaFacilitadores = GetTabelaFacilitador(unidade, area, coordenador, maquina, nome),
                FiltroAreaFacilitador = new SelectViewModel(),
                FiltroCoordenadorFacilitador = new SelectViewModel(),
                FiltroMaquinaFacilitador = new SelectViewModel(),
            };

            int? plantaId = null;
            int? areaId = null;
            int? coordenadorId = null;
            int? maquinaId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int aux))
            {
                plantaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out aux))
            {
                areaId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(coordenador), out aux))
            {
                coordenadorId = aux;
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out aux))
            {
                maquinaId = aux;
            }

            var areasQuery = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Colaboradores).Any())
                .Where(a => a.Maquinas.Any());

            var coordenadoresQuery = _db.Coordenadores
                .Where(c => c.Uniorgs.Any())
                .Where(c => c.Uniorgs.SelectMany(u => u.Colaboradores).Any())
                .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any());

            var maquinasQuery = _db.Maquinas.AsQueryable();

            if (areaId.HasValue)
            {
                maquinasQuery = maquinasQuery.Where(m => m.AreaId == areaId);
                coordenadoresQuery = coordenadoresQuery.Where(m => m.AreaId == areaId);
            }
            else if (plantaId.HasValue)
            {
                areasQuery = areasQuery.Where(a => a.PlantaId == plantaId);
                maquinasQuery = maquinasQuery.Where(m => m.Area.PlantaId == plantaId);
                coordenadoresQuery = coordenadoresQuery.Where(m => m.Area.PlantaId == plantaId);
            }

            areasQuery = areasQuery.OrderBy(a => a.Alias);
            coordenadoresQuery = coordenadoresQuery.OrderBy(c => c.Nome);
            maquinasQuery = maquinasQuery.OrderBy(c => c.Descricao);

            var areas = areasQuery.ToList();
            var coordenadores = coordenadoresQuery.ToList();
            var maquinas = maquinasQuery.Where(i => i.IsAtivo).ToList();

            for (var i = 0; i < areas.Count; i++)
            {
                var key = Encrypting.Encrypt(areas[i].Id.ToString());

                if (areas[i].Id == areaId)
                {
                    model.FiltroAreaFacilitador.Value.SelectedIndex = i;
                }

                model.FiltroAreaFacilitador.Value.Options.Add(key, areas[i].Alias);
            }

            for (var i = 0; i < coordenadores.Count; i++)
            {
                var key = Encrypting.Encrypt(coordenadores[i].Id.ToString());

                if (coordenadores[i].Id == coordenadorId)
                {
                    model.FiltroCoordenadorFacilitador.Value.SelectedIndex = i;
                }

                model.FiltroCoordenadorFacilitador.Value.Options.Add(key, coordenadores[i].Nome);
            }

            for (var i = 0; i < maquinas.Count; i++)
            {
                var key = Encrypting.Encrypt(maquinas[i].Id.ToString());

                if (maquinas[i].Id == maquinaId)
                {
                    model.FiltroMaquinaFacilitador.Value.SelectedIndex = i;
                }

                model.FiltroMaquinaFacilitador.Value.Options.Add(key, maquinas[i].Descricao);
            }

            var onchange = "filtrarFacilitador()";
            model.FiltroAreaFacilitador.Value.OnChange = onchange;
            model.FiltroCoordenadorFacilitador.Value.OnChange = onchange;
            model.FiltroMaquinaFacilitador.Value.OnChange = onchange;

            return Json(model);
        }

        private CadastroFacilitadorViewModel GetFacilitadores(string unidade, string area, string coordenador, string maquina, string nome)
        {
            var model = new CadastroFacilitadorViewModel
            {
                FiltroUnidadeFacilitador = new SelectViewModel(),
                FiltroAreaFacilitador = new SelectViewModel(),
                FiltroCoordenadorFacilitador = new SelectViewModel(),
                FiltroMaquinaFacilitador = new SelectViewModel(),
                FiltroNomeFacilitador = new InputViewModel(),
                TabelaFacilitadores = GetTabelaFacilitador(unidade, area, coordenador, maquina, nome),
            };

            var onchange = "filtrarFacilitador()";
            model.FiltroUnidadeFacilitador.Value.OnChange = onchange;
            model.FiltroAreaFacilitador.Value.OnChange = onchange;
            model.FiltroCoordenadorFacilitador.Value.OnChange = onchange;
            model.FiltroMaquinaFacilitador.Value.OnChange = onchange;

            foreach (var _unidade in _db.Plantas.OrderBy(p => p.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(_unidade.Id.ToString());

                model.FiltroUnidadeFacilitador.Value.Options.Add(key, _unidade.Descricao);
            }

            var areas = _db.Areas
                .Where(a => a.Coordenadores.Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(u => u.Maquinas).Any())
                .Where(a => a.Coordenadores.SelectMany(c => c.Uniorgs).SelectMany(m => m.Colaboradores).Any())
                .OrderBy(a => a.Alias)
                .ToList();

            foreach (var _area in areas)
            {
                var key = Encrypting.Encrypt(_area.Id.ToString());

                model.FiltroAreaFacilitador.Value.Options.Add(key, _area.Alias);
            }

            var coordenadores = _db.Coordenadores
                .Where(c => c.Uniorgs.Any())
                .Where(c => c.Uniorgs.SelectMany(u => u.Maquinas).Any())
                .Where(c => c.Uniorgs.SelectMany(m => m.Colaboradores).Any())
                .OrderBy(c => c.Nome)
                .ToList();

            foreach (var _coordenador in coordenadores)
            {
                var key = Encrypting.Encrypt(_coordenador.Id.ToString());

                model.FiltroCoordenadorFacilitador.Value.Options.Add(key, _coordenador.Nome);
            }

            foreach (var _maquina in _db.Maquinas.OrderBy(m => m.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(_maquina.Id.ToString());

                model.FiltroMaquinaFacilitador.Value.Options.Add(key, _maquina.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaFacilitador(string unidade, string area, string coordenador, string maquina, string nome)
        {
            var model = new TableViewModel();

            var colaboradores = _db.Colaboradores
                .Where(c => c.Uniorg.Maquinas.Any())
                .Where(c => c.Uniorg.Maquinas.Where(m => m.IsAtivo).Any())
                .Where(c => c.IsAtivo);

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaId))
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.Area.PlantaId == plantaId);
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaId))
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Coordenador.AreaId == areaId);
            }

            if (int.TryParse(Encrypting.Decrypt(coordenador), out int coordenadorId))
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.CoordenadorId == coordenadorId);
            }

            if (int.TryParse(Encrypting.Decrypt(maquina), out int maquinaId))
            {
                colaboradores = colaboradores.Where(c => c.Uniorg.Maquinas.Any(m => m.Id == maquinaId));
            }

            if (!string.IsNullOrEmpty(nome))
            {
                colaboradores = colaboradores.Where(c => c.Nome.Contains(nome) || c.Chapa.Contains(nome));
            }

            model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Unidade", "Área", "Responsável", "Máquina", "Chapa", "Nome", "Facilitador" } });

            foreach (var colaborador in colaboradores.ToList())
            {
                var unidades = colaborador.Uniorg.Coordenador.Area.Planta.Descricao;
                var areas = colaborador.Uniorg.Coordenador.Area.Alias;
                var coordenadores = colaborador.Uniorg.Coordenador.Nome;

                var key = Encrypting.Encrypt(colaborador.Id.ToString());

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = key,
                    Values = new string[]
                    {
                        unidades,
                        areas,
                        coordenadores,
                        colaborador.Uniorg.Maquinas.FirstOrDefault().Descricao,
                        colaborador.Chapa,
                        colaborador.Nome,
                        $@"
                            <span style=""display: none"">{(colaborador.IsFacilitador ? "Sim" : "Não")}</span>
                            <div>
                                <input type=""checkbox"" onchange=""onFacilitadorChanged(this)"" {(colaborador.IsFacilitador ? "checked" : "")} />
                                <input type=""hidden"" name=""facilitadores[{key}].IsFacilitador"" value=""{colaborador.IsFacilitador.ToString().ToLower()}"" />
                                <input type=""hidden"" name=""facilitadores[{key}].IsEdited"" value=""{colaborador.IsFacilitador.ToString().ToLower()}"" />
                            </div>",
                    },
                });
            }

            return model;
        }
        #endregion

        #region Coordenador
        public ActionResult LimparCoordenador()
        {
            var model = GetCoordenadores(null);

            return Json(model);
        }

        public async Task<ActionResult> CadastrarCoordenador(string CoordenadorCoordenador, string MaquinaCoordenador, string FiltroUnidadeTabelaCoordenador)
        {
            string alerta;

            CadastroCoordenadorViewModel result = new CadastroCoordenadorViewModel();

            if (int.TryParse(Encrypting.Decrypt(CoordenadorCoordenador), out int coordenadorId))
            {
                if (int.TryParse(Encrypting.Decrypt(MaquinaCoordenador), out int maquinaId))
                {
                    if (_db.CadastroCoordenadores.Where(c => c.IsAtivo && c.MaquinaId == maquinaId).Any())
                    {
                        alerta = "Máquina já Selecionada";
                    }
                    else
                    {
                        _db.CadastroCoordenadores.Add(new CadastroCoordenador()
                        {
                            CoordenadorId = coordenadorId,
                            MaquinaId = maquinaId,
                            IsAtivo = true,
                        });

                        _db.SaveChanges();

                        var maquina = _db.Maquinas.Find(maquinaId);

                        foreach (var uniorg in maquina.Uniorgs)
                        {
                            uniorg.CoordenadorId = coordenadorId;

                            _db.SaveChanges();
                        }

                        var auxiliaryTableService = new AuxiliaryTableService(_db);
                        await auxiliaryTableService.UpdateAuxiliaryTables();

                        result = GetCoordenadores(FiltroUnidadeTabelaCoordenador);

                        alerta = "Cadastrado com Sucesso";
                    }
                }
                else
                {
                    alerta = "Selecione uma Máquina";
                }
            }
            else
            {
                alerta = "Selecione um Coordenador";
            }

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public async Task<ActionResult> AtualizarCoordenador(string Coordenador, string CoordenadorCoordenador, string MaquinaCoordenador, string FiltroUnidadeTabelaCoordenador, bool? DesativarCoordenador)
        {
            string alerta;

            CadastroCoordenadorViewModel result = new CadastroCoordenadorViewModel();

            if (int.TryParse(Encrypting.Decrypt(Coordenador), out int cadastroCoordenadorId))
            {
                if (int.TryParse(Encrypting.Decrypt(CoordenadorCoordenador), out int coordenadorId))
                {
                    if (int.TryParse(Encrypting.Decrypt(MaquinaCoordenador), out int maquinaId))
                    {
                        if (!DesativarCoordenador.HasValue || DesativarCoordenador.Value)
                        {
                            var cadastroCoordenador = _db.CadastroCoordenadores.Find(cadastroCoordenadorId);

                            cadastroCoordenador.IsAtivo = false;

                            _db.SaveChanges();

                            var maquina = _db.Maquinas.Find(maquinaId);

                            foreach (var uniorg in maquina.Uniorgs)
                            {
                                uniorg.CoordenadorId = uniorg.CoordenadorOriginalId;

                                _db.SaveChanges();
                            }

                            var auxiliaryTableService = new AuxiliaryTableService(_db);
                            await auxiliaryTableService.UpdateAuxiliaryTables();

                            var coordenador = _db.Coordenadores.Find(coordenadorId);
                            var date = DateTime.Now;

                            var dataCorrespondente = DateTime.Now.AddMonths(-1);
                            dataCorrespondente = new DateTime(dataCorrespondente.Year, dataCorrespondente.Month, DateTime.DaysInMonth(dataCorrespondente.Year, dataCorrespondente.Month));

                            _db.HistoricoCoordenadores.Add(new HistoricoCoordenador()
                            {
                                AreaId = coordenador.AreaId,
                                CoordenadorId = coordenadorId,
                                DataCongelamento = date,
                                DataCorrespondente = dataCorrespondente,
                                Tipo = TipoHistorico.Treinamento,
                                Valor = 0,
                            });

                            _db.SaveChanges();

                            _db.HistoricoCoordenadores.Add(new HistoricoCoordenador()
                            {
                                AreaId = coordenador.AreaId,
                                CoordenadorId = coordenadorId,
                                DataCongelamento = date,
                                DataCorrespondente = dataCorrespondente,
                                Tipo = TipoHistorico.Conhecimento,
                                Valor = 0,
                            });

                            _db.SaveChanges();

                            result = GetCoordenadores(FiltroUnidadeTabelaCoordenador);

                            alerta = "Desativado com Sucesso";
                        }
                        else if (_db.CadastroCoordenadores.Where(c => c.IsAtivo).Where(c => c.MaquinaId == maquinaId && c.Id != cadastroCoordenadorId).Any())
                        {
                            alerta = "Máquina já Selecionada";
                        }
                        else
                        {
                            var cadastroCoordenador = _db.CadastroCoordenadores.Find(cadastroCoordenadorId);

                            cadastroCoordenador.CoordenadorId = coordenadorId;
                            cadastroCoordenador.MaquinaId = maquinaId;

                            _db.SaveChanges();

                            var maquina = _db.Maquinas.Find(maquinaId);

                            foreach (var uniorg in maquina.Uniorgs)
                            {
                                uniorg.CoordenadorId = coordenadorId;

                                _db.SaveChanges();
                            }

                            var auxiliaryTableService = new AuxiliaryTableService(_db);
                            await auxiliaryTableService.UpdateAuxiliaryTables();

                            result = GetCoordenadores(FiltroUnidadeTabelaCoordenador);

                            alerta = "Atualizado com Sucesso";
                        }
                    }
                    else
                    {
                        alerta = "Selecione uma Máquina";
                    }
                }
                else
                {
                    alerta = "Selecione um Coordenador";
                }
            }
            else
            {
                alerta = "Algo deu Errado";
            }

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult PopularCoordenador(string Coordenador, string planta)
        {
            var model = GetCoordenadores(planta);

            if (int.TryParse(Encrypting.Decrypt(Coordenador), out int coordenadorId))
            {
                model.Coordenador = new InputViewModel(Coordenador);

                var cadastroCoordenador = _db.CadastroCoordenadores.Find(coordenadorId);

                model.FormCoordenador = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarCoordenador");

                var key = Encrypting.Encrypt(cadastroCoordenador.CoordenadorId.ToString());

                int selectedCoordenador = model.CoordenadorCoordenador.Value.Options.Keys.ToList().FindIndex(p => p == key);

                model.CoordenadorCoordenador.Value.SelectedIndex = selectedCoordenador;

                key = Encrypting.Encrypt(cadastroCoordenador.MaquinaId.ToString());

                model.MaquinaCoordenador.Value.Options.Add(key, cadastroCoordenador.Maquina.Descricao);

                model.MaquinaCoordenador.Value.Options = model.MaquinaCoordenador.Value.Options.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                int selectedMaquina = model.MaquinaCoordenador.Value.Options.Keys.ToList().FindIndex(p => p == key);

                model.MaquinaCoordenador.Value.SelectedIndex = selectedMaquina;

                model.DesativarCoordenadorContainer = new StyleViewModel("display", "block");
                model.DesativarCoordenador = new CheckboxViewModel();
            }

            return Json(model);
        }

        public ActionResult FiltrarCoordenador(string planta, string area)
        {
            var model = new CadastroCoordenadorViewModel();

            int? plantaId = null;
            int? areaId = null;

            if (int.TryParse(Encrypting.Decrypt(planta), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            if (int.TryParse(Encrypting.Decrypt(area), out int areaAux))
            {
                areaId = areaAux;
            }

            if (plantaId.HasValue && areaId.HasValue)
            {
                var _planta = _db.Plantas.Where(p => p.Id == plantaId).FirstOrDefault();

                if (!_planta.Areas.Any(a => a.Id == areaId))
                {
                    areaId = null;
                }
            }

            model.CoordenadorCoordenador = new SelectViewModel();
            model.MaquinaCoordenador = new SelectViewModel();

            var maquinasQuery = _db.Maquinas
                .OrderBy(c => c.Descricao)
                .Where(m => !m.CadastroCoordenadores.Where(c => c.IsAtivo).Any());

            var coordenadoresQuery = _db.Coordenadores.AsQueryable();

            if (areaId.HasValue)
            {
                maquinasQuery = maquinasQuery.Where(m => m.AreaId == areaId);
                coordenadoresQuery = coordenadoresQuery.Where(c => c.AreaId == areaId);
            }
            else if (plantaId.HasValue)
            {
                maquinasQuery = maquinasQuery.Where(m => m.Area.PlantaId == plantaId);
                coordenadoresQuery = coordenadoresQuery.Where(c => c.Area.PlantaId == plantaId);

                model.FiltroAreaCoordenador = new SelectViewModel("Todas as Áreas", true);

                var areas = _db.Areas
                    .Where(a => a.PlantaId == plantaId)
                    .Where(a => a.Coordenadores.Any())
                    .OrderBy(a => a.Alias)
                    .ToList();

                foreach (var _area in areas)
                {
                    var areaKey = Encrypting.Encrypt(_area.Id.ToString());

                    model.FiltroAreaCoordenador.Value.Options.Add(areaKey, _area.Alias);
                }
            }

            maquinasQuery = maquinasQuery.OrderBy(m => m.Descricao);
            coordenadoresQuery = coordenadoresQuery.OrderBy(m => m.Nome);

            foreach (var maquina in maquinasQuery.Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(maquina.Id.ToString());

                model.MaquinaCoordenador.Value.Options.Add(key, maquina.Descricao);
            }

            foreach (var coordenador in coordenadoresQuery.ToList())
            {
                var key = Encrypting.Encrypt(coordenador.Id.ToString());

                model.CoordenadorCoordenador.Value.Options.Add(key, coordenador.Nome);
            }

            model.CoordenadorCoordenador.Value.Options = model.CoordenadorCoordenador.Value.Options.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return Json(model);
        }

        public ActionResult FiltrarTabelaCoordenador(string unidade)
        {
            var model = new CadastroMaquinaViewModel
            {
                TabelaMaquinas = GetTabelaCoordenador(unidade)
            };

            return Json(model);
        }

        private CadastroCoordenadorViewModel GetCoordenadores(string unidade)
        {
            var model = new CadastroCoordenadorViewModel
            {
                FormCoordenador = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarCoordenador"),
                TabelaCoordenador = GetTabelaCoordenador(unidade),

                Coordenador = new InputViewModel(),
                CoordenadorCoordenador = new SelectViewModel(),
                MaquinaCoordenador = new SelectViewModel(),

                FiltroPlantaCoordenador = new SelectViewModel("Todas as Unidades", true),
                FiltroAreaCoordenador = new SelectViewModel("Todas as Áreas", true),
                FiltroUnidadeTabelaCoordenador = new SelectViewModel("Unidade", "Todas as Unidades", true),

                DesativarCoordenadorContainer = new StyleViewModel("display", "none"),
                DesativarCoordenador = new CheckboxViewModel(),
            };

            model.FiltroPlantaCoordenador.Value.OnChange = "filtrarCoordenador()";
            model.FiltroAreaCoordenador.Value.OnChange = "filtrarCoordenador()";

            model.FiltroUnidadeTabelaCoordenador.Value.OnChange = "filtrarMaquina(this)";

            foreach (var planta in _db.Plantas.OrderBy(c => c.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(planta.Id.ToString());
                model.FiltroPlantaCoordenador.Value.Options.Add(key, planta.Descricao);
                model.FiltroUnidadeTabelaCoordenador.Value.Options.Add(key, planta.Descricao);
            }

            foreach (var area in _db.Areas.OrderBy(c => c.Alias).ToList())
            {
                var key = Encrypting.Encrypt(area.Id.ToString());

                model.FiltroAreaCoordenador.Value.Options.Add(key, area.Alias);
            }

            foreach (var maquina in _db.Maquinas.Where(m => !m.CadastroCoordenadores.Where(c => c.IsAtivo).Any()).Where(m => m.IsAtivo).OrderBy(c => c.Descricao).Where(i => i.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(maquina.Id.ToString());

                model.MaquinaCoordenador.Value.Options.Add(key, maquina.Descricao);
            }

            foreach (var coordenador in _db.Coordenadores.Where(c => c.IsAtivo).OrderBy(c => c.Nome).ToList())
            {
                var key = Encrypting.Encrypt(coordenador.Id.ToString());

                model.CoordenadorCoordenador.Value.Options.Add(key, coordenador.Nome);
            }

            return model;
        }

        private TableViewModel GetTabelaCoordenador(string unidade)
        {
            int? plantaId = null;

            if (int.TryParse(Encrypting.Decrypt(unidade), out int plantaAux))
            {
                plantaId = plantaAux;
            }

            TableViewModel model = new TableViewModel();

            model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Máquina", "Responsável" } });

            model.Value.OnClick = @"
                Binder.Bind('/MatrizHabilidade/Cadastro/PopularCoordenador', 
                { 
                    Coordenador: this.getAttribute('data-key'),
                })";

            List<CadastroCoordenador> CadastroCoordenadores;

            if (plantaId.HasValue)
            {
                var planta = _db.Plantas.Find(plantaId.Value);

                CadastroCoordenadores = _db.CadastroCoordenadores
                    .Include("Maquina")
                    .Include("Coordenador")
                    .Where(c => c.IsAtivo)
                    .Where(c => c.Coordenador.Area.PlantaId == planta.Id)
                    .ToList();
            }
            else
            {
                CadastroCoordenadores = _db.CadastroCoordenadores
                    .Include("Maquina")
                    .Include("Coordenador")
                    .Where(c => c.IsAtivo)
                    .ToList();
            }

            foreach (var cadastroCoordenador in CadastroCoordenadores.ToList())
            {
                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(cadastroCoordenador.Id.ToString()),
                    Values = new string[]
                    {
                        cadastroCoordenador.Maquina.Descricao,
                        cadastroCoordenador.Coordenador.Nome,
                    },
                });
            }

            return model;
        }
        #endregion

        #region Integração
        public ActionResult LimparIntegracao()
        {
            var model = GetIntegracao(false);

            return Json(model);
        }

        public ActionResult CadastrarIntegracao(string TreinamentoIntegracao, int MetaIntegracao, bool? FiltroMostrarDesativadosIntegracao)
        {
            string alerta;

            if (!FiltroMostrarDesativadosIntegracao.HasValue)
            {
                FiltroMostrarDesativadosIntegracao = false;
            }

            if (int.TryParse(Encrypting.Decrypt(TreinamentoIntegracao), out int treinamentoId))
            {
                var integracao = new ConfiguracaoIntegracaoTreinamento()
                {
                    TreinamentoId = treinamentoId,
                    Target = MetaIntegracao,
                };

                _db.Integracoes.Add(integracao);

                _db.SaveChanges();

                alerta = "Cadastrado com Sucesso";
            }
            else
            {
                alerta = @"Deve-se selecionar ao menos uma Unidade do BDados";
            }

            var result = GetIntegracao(FiltroMostrarDesativadosIntegracao.Value);

            result.Alerta = new AlertaViewModel(alerta);

            return Json(result);
        }

        public ActionResult AtualizarIntegracao(string Integracao, string TreinamentoIntegracao, int MetaIntegracao, bool? DesativarIntegracao, bool? FiltroMostrarDesativadosIntegracao)
        {
            if (!FiltroMostrarDesativadosIntegracao.HasValue)
            {
                FiltroMostrarDesativadosIntegracao = false;
            }

            if (!DesativarIntegracao.HasValue)
            {
                DesativarIntegracao = false;
            }

            if (int.TryParse(Encrypting.Decrypt(Integracao), out int integracaoId))
            {
                if (int.TryParse(Encrypting.Decrypt(TreinamentoIntegracao), out int treinamentoId))
                {
                    var integracao = _db.Integracoes.Find(integracaoId);

                    integracao.TreinamentoId = treinamentoId;
                    integracao.Target = MetaIntegracao;
                    integracao.IsAtivo = !DesativarIntegracao.Value;

                    _db.SaveChanges();
                }
            }

            var result = GetIntegracao(FiltroMostrarDesativadosIntegracao.Value);

            result.Alerta = new AlertaViewModel("Atualizado com Sucesso");

            return Json(result);
        }

        public ActionResult PopularIntegracao(string integracao, bool isDesativado)
        {
            var model = GetIntegracao(isDesativado);

            if (int.TryParse(Encrypting.Decrypt(integracao), out int integracaoId))
            {
                var _integracao = _db.Integracoes.Find(integracaoId);

                model.FormIntegracao = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/AtualizarIntegracao");
                model.MetaIntegracao = new InputViewModel(_integracao.Target.ToString());
                model.Integracao = new InputViewModel(integracao);
                model.DesativarIntegracaoContainer = new StyleViewModel("display", "block");
                model.DesativarIntegracao = new CheckboxViewModel(!_integracao.IsAtivo);

                var key = Encrypting.Encrypt(_integracao.TreinamentoId.ToString());

                int selectedTreinamento = model.TreinamentoIntegracao.Value.Options.Keys.ToList().FindIndex(p => p == key);

                model.TreinamentoIntegracao.Value.SelectedIndex = selectedTreinamento;
            }

            return Json(model);
        }

        public ActionResult FiltrarIntegracao(bool? isDesativado)
        {
            if (!isDesativado.HasValue)
            {
                isDesativado = false;
            }

            var model = GetIntegracao(isDesativado.Value);

            return Json(model);
        }

        private CadastroIntegracaoViewModel GetIntegracao(bool isDesativado)
        {
            var model = new CadastroIntegracaoViewModel
            {
                FormIntegracao = new ProppertyViewModel("action", "/MatrizHabilidade/Cadastro/CadastrarIntegracao"),
                TabelaIntegracoes = GetTabelaIntegracao(isDesativado),
                Integracao = new InputViewModel(""),
                MetaIntegracao = new InputViewModel(),
                TreinamentoIntegracao = new SelectViewModel(),
                DesativarIntegracaoContainer = new StyleViewModel("display", "none"),
                DesativarIntegracao = new CheckboxViewModel(),
                FiltroMostrarDesativadosIntegracao = new CheckboxViewModel(isDesativado),
            };

            foreach (var treinamento in _db.Treinamentos.Where(t => t.IsAtivo).ToList())
            {
                var key = Encrypting.Encrypt(treinamento.Id.ToString());

                model.TreinamentoIntegracao.Value.Options.Add(key, treinamento.Descricao);
            }

            return model;
        }

        private TableViewModel GetTabelaIntegracao(bool isDesativado)
        {
            var model = new TableViewModel();

            List<ConfiguracaoIntegracaoTreinamento> integracoes;
            model.Value.OnClick = @"
                Binder.Bind('/MatrizHabilidade/Cadastro/PopularIntegracao', 
                { 
                    integracao: this.getAttribute('data-key'), 
                    isDesativado: document.querySelector('[name=FiltroMostrarDesativadosIntegracao]').value 
                })";

            if (isDesativado)
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição do Treinamento", "Meta", "Desativado" } });
                integracoes = _db.Integracoes.Include("Treinamento").OrderBy(i => i.Treinamento.Descricao).ToList();
            }
            else
            {
                model.Value.Rows.Add(new TableViewModel.Row() { Values = new string[] { "Descrição do Treinamento", "Meta" } });
                integracoes = _db.Integracoes.Include("Treinamento").Where(i => i.IsAtivo).OrderBy(i => i.Treinamento.Descricao).ToList();
            }

            foreach (var integracao in integracoes)
            {
                string[] values;

                if (isDesativado)
                {
                    values = new string[]
                    {
                        integracao.Treinamento.Descricao,
                        integracao.Target.ToString(),
                        integracao.IsAtivo ? "Não" : "Sim",
                    };
                }
                else
                {
                    values = new string[]
                    {
                        integracao.Treinamento.Descricao,
                        integracao.Target.ToString(),
                    };
                }

                model.Value.Rows.Add(new TableViewModel.Row()
                {
                    Key = Encrypting.Encrypt(integracao.Id.ToString()),
                    Values = values,
                });
            }

            return model;
        }
        #endregion
    }
}

