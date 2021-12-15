using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MatrizHabilidadeDatabase.Services
{
    public class AuxiliaryTableService
    {
        private readonly DataBaseContext _db;

        public AuxiliaryTableService(DataBaseContext db)
        {
            _db = db;
        }

        public async Task<bool> UpdateAuxiliaryTables()
        {
            try
            {
                    _db.TreinamentosEspecificos.FromSqlRaw("DROP TABLE IF EXISTS treinamento_especifico_complete;");
                    _db.TreinamentosEspecificos.FromSqlRaw("CREATE TABLE treinamento_especifico_complete AS SELECT * FROM vw_treinamento_especifico_complete;");

                    _db.Treinamentos.FromSqlRaw("DROP TABLE IF EXISTS treinamento_complete;");
                    _db.Treinamentos.FromSqlRaw("CREATE TABLE treinamento_complete AS SELECT * FROM vw_treinamento_complete;");

                    await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                    _db.Erros.Add(new Error(e, "AuxiliaryTableService - UpdateAuxiliaryTables"));
                    await _db.SaveChangesAsync();
            }

            return false;
        }
    }
}