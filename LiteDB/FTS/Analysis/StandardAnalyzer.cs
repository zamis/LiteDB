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
    public class StandardAnalyzer : IAnalyzer
    {
        protected Regex WhiteSpace = new Regex(@"\s*");

        /// <summary>
        /// Regex pattern for each king of token
        /// </summary>
        protected List<Regex> TokenPatterns = new List<Regex>() 
        {
            //TODO: email, url, phone
            new Regex(@"[a-z]+('[a-z]+)?"), // word
            new Regex(@"[0-9]+") // int
        };

        /// <summary>
        /// Snowball stemmers on C# - Get base of word https://stemmersnet.codeplex.com
        /// </summary>
        internal Iveonik.Stemmers.IStemmer Stemmer = new Iveonik.Stemmers.EnglishStemmer();

        #region StopWords List (english)

        // based on https://raw.githubusercontent.com/mongodb/mongo/master/src/mongo/db/fts/stop_words_english.txt
        protected HashSet<string> StopWords = new HashSet<string>
        { 
            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at", 
            "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", 
            "can't", "cannot", "could", "couldn't",
            "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during",
            "each",
            "few", "for", "from", "further",
            "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how", "how's",
            "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself",
            "let's",
            "me", "more", "most", "mustn't", "my", "myself",
            "no", "nor", "not",
            "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own",
            "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so", "some", "such",
            "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too",
            "under", "until", "up", 
            "very", 
            "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't",
            "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
        };

        #endregion

        /// <summary>
        /// Read next valid token using tokens patterns
        /// </summary>
        protected virtual string GetToken(StringScanner s)
        {
            while (!s.HasTerminated)
            {
                // remove whitespaces
                s.Scan(this.WhiteSpace);

                foreach (var regex in this.TokenPatterns)
                {
                    var token = s.Scan(regex);

                    if (token.Length > 0) return token;
                }

                s.Seek(1);
            }

            return null;
        }

        /// <summary>
        /// Return if a word is a stop word
        /// </summary>
        protected virtual bool IsStopWord(string word)
        {
            return this.StopWords.Contains(word);
        }

        /// <summary>
        /// Prepare string content before start break in tokens - used to lowercase, remove accents, ...
        /// </summary>
        protected virtual string Prepare(string content)
        {
            return content.ToLowerInvariant();
        }

        /// <summary>
        /// Parse content returns a dictionary with all token found on content. Returns a dict with key = stem word
        /// </summary>
        public virtual Dictionary<string, List<TokenPosition>> ParseContent(string content)
        {
            var s = new StringScanner(this.Prepare(content));
            var result = new Dictionary<string, List<TokenPosition>>();
            var index = 0;

            while (!s.HasTerminated)
            {
                var token = this.GetToken(s);

                if (token == null || this.IsStopWord(token)) break;

                var stem = this.Stemmer.Stem(token);

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
