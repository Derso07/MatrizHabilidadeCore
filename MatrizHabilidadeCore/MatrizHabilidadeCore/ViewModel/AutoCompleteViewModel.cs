using MatrizHabilidadeDataBaseCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MatrizHabilidade.ViewModel
{
    public class AutoCompleteViewModel
    {
        public string key { get; set; }

        public string value { get; set; }

        public string text { get; set; }

        public AutoCompleteViewModel Init(string targetText)
        {
            key = Encrypting.Encrypt(key);

            var regex = new Regex($@"(.*)({targetText})(.*)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.IgnorePatternWhitespace);

            var match = regex.Match(value);

            text = $@"{ match.Groups[1] }<strong>{ match.Groups[2] }</strong>{ match.Groups[3] }";

            return this;
        }
    }
}