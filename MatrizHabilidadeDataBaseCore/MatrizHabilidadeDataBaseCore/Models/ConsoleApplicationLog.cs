using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("consoleapplicationlog")]
    public class ConsoleApplicationLog
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public bool IsFinished { get; set; }

        public int? ErrorId { get; set; }

        public string Version { get; set; }

        public virtual Error Error { get; set; }
    }
}