using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("integrationlog")]
    public class IntegrationLog
    {
        public int Id { get; set; }

        public string JSON { get; set; }

        public DateTime Date { get; set; }
    }
}