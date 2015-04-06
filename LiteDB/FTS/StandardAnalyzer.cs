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
    /// Represent a standard implementation of Base Analyzer for text search - use englishs dictionary
    /// </summary>
    public class StandardAnalyzer : BaseAnalyzer
    {
        #region English Stop-Words

        // based on https://raw.githubusercontent.com/mongodb/mongo/master/src/mongo/db/fts/stop_words_english.txt
        private HashSet<string> _stopWords = new HashSet<string> 
        { 
            "a",
            "about",
            "above",
            "after",
            "again",
            "against",
            "all",
            "am",
            "an",
            "and",
            "any",
            "are",
            "aren't",
            "as",
            "at",
            "be",
            "because",
            "been",
            "before",
            "being",
            "below",
            "between",
            "both",
            "but",
            "by",
            "can't",
            "cannot",
            "could",
            "couldn't",
            "did",
            "didn't",
            "do",
            "does",
            "doesn't",
            "doing",
            "don't",
            "down",
            "during",
            "each",
            "few",
            "for",
            "from",
            "further",
            "had",
            "hadn't",
            "has",
            "hasn't",
            "have",
            "haven't",
            "having",
            "he",
            "he'd",
            "he'll",
            "he's",
            "her",
            "here",
            "here's",
            "hers",
            "herself",
            "him",
            "himself",
            "his",
            "how",
            "how's",
            "i",
            "i'd",
            "i'll",
            "i'm",
            "i've",
            "if",
            "in",
            "into",
            "is",
            "isn't",
            "it",
            "it's",
            "its",
            "itself",
            "let's",
            "me",
            "more",
            "most",
            "mustn't",
            "my",
            "myself",
            "no",
            "nor",
            "not",
            "of",
            "off",
            "on",
            "once",
            "only",
            "or",
            "other",
            "ought",
            "our",
            "ours",
            "ourselves",
            "out",
            "over",
            "own",
            "same",
            "shan't",
            "she",
            "she'd",
            "she'll",
            "she's",
            "should",
            "shouldn't",
            "so",
            "some",
            "such",
            "than",
            "that",
            "that's",
            "the",
            "their",
            "theirs",
            "them",
            "themselves",
            "then",
            "there",
            "there's",
            "these",
            "they",
            "they'd",
            "they'll",
            "they're",
            "they've",
            "this",
            "those",
            "through",
            "to",
            "too",
            "under",
            "until",
            "up",
            "very",
            "was",
            "wasn't",
            "we",
            "we'd",
            "we'll",
            "we're",
            "we've",
            "were",
            "weren't",
            "what",
            "what's",
            "when",
            "when's",
            "where",
            "where's",
            "which",
            "while",
            "who",
            "who's",
            "whom",
            "why",
            "why's",
            "with",
            "won't",
            "would",
            "wouldn't",
            "you",
            "you'd",
            "you'll",
            "you're",
            "you've",
            "your",
            "yours",
            "yourself",
            "yourselves"
        };

        #endregion

        #region Irregular verbs

        // based on http://www.englishpage.com/irregularverbs/irregularverbs.html
        private Dictionary<string, string> _irrgularVerbs = new Dictionary<string, string>
        {
            { "arose", "arise" },
            { "arisen", "arise" },
            { "awakened", "awake" },
            { "awoke", "awake" },
            { "awoken", "awake" },
        };

        #endregion

        /// <summary>
        /// Returns true if word is a stop-word (like: "the", "a", "of")
        /// </summary>
        protected override bool IsStopWord(string word)
        {
            return _stopWords.Contains(word);
        }

        /// <summary>
        /// Normalize a word getting base word (thinking => think), remove plural (does => do)
        /// </summary>
        protected override string Stemmer(string word)
        {
            // check for irregular verbs
            string verb;

            if (_irrgularVerbs.TryGetValue(word, out verb))
            {
                return verb;
            }

            // check for regular verbs
            if (word.EndsWith("ed"))
            {
                return word.Substring(0, word.Length - 2);
            }
            else if (word.EndsWith("ing"))
            {
                return word.Substring(0, word.Length - 3);
            }

            // remove plural
            if (word.EndsWith("es"))
            {
                return word.Substring(0, word.Length - 2);
            }
            else if (word.EndsWith("s"))
            {
                return word.Substring(0, word.Length - 1);
            }

            // nothing to change
            return word;
        }
    }
}
