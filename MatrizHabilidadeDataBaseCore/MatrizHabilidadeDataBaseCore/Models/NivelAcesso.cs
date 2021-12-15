using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrizHabilidadeDatabase.Models
{
    public enum NivelAcesso
    {
        None = -1,
        Master = 0,
        Administrador = 1,
        Coordenador = 2,
        Funcionario = 3,
    }
}
