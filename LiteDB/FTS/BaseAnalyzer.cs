using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LiteDB
{
    /// <summary>
    /// Represent base analyzer class used in full text search
    /// http://stackoverflow.com/questions/20690499/concrete-javascript-regex-for-accented-characters-diacritics
    /// 
    /// </summary>
    public abstract class BaseAnalyzer
    {
        protected Regex WhiteSpace = new Regex(@"\s*");

        protected Regex[] TokenPatterns = new Regex[] 
        {
            new Regex(@"[a-z]+([-'][a-z]+){0,2}"), // word
            new Regex(@"[0-9]+") // int
        };

        /// <summary>
        /// Returns true if word is a stop-word (like: "the", "a", "of")
        /// </summary>
        protected abstract bool IsStopWord(string word);

        /// <summary>
        /// Stemmer a word getting base word (thinking => think), remove plural (does => do)
        /// https://stemmersnet.codeplex.com/
        /// </summary>
        protected abstract string Stemmer(string word);

        /// <summary>
        /// Read next valid token
        /// </summary>
        public string GetNextToken(StringScanner s)
        {
            while (!s.HasTerminated)
            {
                // remove whitespaces
                s.Scan(WhiteSpace);

                foreach (var regex in this.TokenPatterns)
                {
                    var token = s.Scan(regex);

                    if (token.Length >= 2 && !this.IsStopWord(token)) return token;
                }

                s.Seek(1);
            }

            return null;
        }

        /// <summary>
        /// Parse content returns a dictionary with all token found on content
        /// </summary>
        public Dictionary<string, List<TokenOccurence>> ParseContent(string content)
        {
            var s = new StringScanner(content.ToLower());
            var result = new Dictionary<string, List<TokenOccurence>>();
            var position = 0;

            while (!s.HasTerminated)
            {
                var token = this.GetNextToken(s);

                if (token == null) break;

                var word = this.Stemmer(token);
                List<TokenOccurence> occurs;

                if (!result.TryGetValue(word, out occurs))
                {
                    occurs = new List<TokenOccurence>();
                }

                occurs.Add(new TokenOccurence { Position = ++position, Offset = s.Index - token.Length, Length = token.Length });

                result[word] = occurs;
            }

            return result;
        }
    }
}
