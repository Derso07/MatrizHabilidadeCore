using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatrizHabilidadeDatabase.Models
{
    [Table("error")]
    public class Error
    {
        public Error() { }

        public Error(string text)
        {
            InnerExceptionType = text;
            Data = DateTime.Now;
        }

        public Error(Exception exception, string source)
        {
            if (exception.InnerException != null)
            {
                InnerExceptionType = exception.InnerException.GetType().ToString();
                InnerException = exception.InnerException.Message;
                InnerSource = exception.InnerException.Source;
                if (exception.InnerException.StackTrace != null)
                    InnerStackTrace = exception.InnerException.StackTrace;
            }

            ExceptionType = exception.GetType().ToString();

            Exception = exception.Message;

            Source = source;

            if (exception.StackTrace != null)
                StackTrace = exception.StackTrace;

            Data = DateTime.Now;
        }

        public int Id { get; set; }

        public string InnerExceptionType { get; set; }

        public string InnerException { get; set; }

        public string InnerSource { get; set; }

        public string InnerStackTrace { get; set; }

        public string ExceptionType { get; set; }

        public string Exception { get; set; }

        public string Source { get; set; }

        public string StackTrace { get; set; }

        public DateTime Data { get; set; }

        public string Url { get; set; }

        public virtual List<ConsoleApplicationLog> Logs { get; set; }
    }
}