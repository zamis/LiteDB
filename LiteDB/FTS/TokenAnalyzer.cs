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
    /// </summary>
    public class TokenAnalyzer
    {
        private Regex _whiteSpace = new Regex(@"\s*");

        /// <summary>
        /// Regex pattern for each king of token
        /// </summary>
        private List<Regex> _tokenPatterns = new List<Regex>() 
        {
            //TODO: email, url, phone
            new Regex(@"[a-z]+('[a-z]+)?"), // word
            new Regex(@"[0-9]+") // int
        };

        private Locale _locale;

        public TokenAnalyzer(Locale locale)
        {
            _locale = locale;
        }

        /// <summary>
        /// Read next valid token using tokens patterns
        /// </summary>
        private string GetToken(StringScanner s)
        {
            while (!s.HasTerminated)
            {
                // remove whitespaces
                s.Scan(_whiteSpace);

                foreach (var regex in _tokenPatterns)
                {
                    var token = s.Scan(regex);

                    if (token.Length > 0) return token;
                }

                s.Seek(1);
            }

            return null;
        }

        /// <summary>
        /// Parse content returns a dictionary with all token found on content. Returns a dict with key = stem word
        /// </summary>
        public virtual Dictionary<string, List<TokenPosition>> ParseContent(string content)
        {
            var s = new StringScanner(content);
            var result = new Dictionary<string, List<TokenPosition>>();
            var index = 0;

            while (!s.HasTerminated)
            {
                var token = this.GetToken(s);

                if (token == null) break;

                var stem = _locale.Normalize(token);

                List<TokenPosition> positions;

                if (!result.TryGetValue(stem, out positions))
                {
                    positions = new List<TokenPosition>();
                }

                positions.Add(new TokenPosition { Position = ++index, Offset = s.Index - token.Length, Token = token });

                result[stem] = positions;
            }

            return result;
        }

        public Query ParseQuery(string query)
        {
            throw new NotImplementedException();
        }
    }
}
