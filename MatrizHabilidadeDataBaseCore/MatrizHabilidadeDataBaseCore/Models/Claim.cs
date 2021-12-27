using MatrizHabilidadeDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrizHabilidadeDataBaseCore.Models
{
    public class Claim
    {
        public int Id { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }

    }
}
