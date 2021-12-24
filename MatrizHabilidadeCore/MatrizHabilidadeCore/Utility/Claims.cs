using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Utility
{
    public class Claims
    {
        private Claims(string value) { Value = value; }

        public string Value { get; set; }

        public static Claims CurrentYear { get { return new Claims("CurrentYear"); } }

        public static Claims UserId { get { return new Claims("UserId"); } }


    }
}
