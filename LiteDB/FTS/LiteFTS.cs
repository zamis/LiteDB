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
    /// Represent a class that implement full text search
    /// </summary>
    public class LiteFTS
    {
        public LiteCollection<Token> Tokens { get; private set; }
        public LiteDatabase Database { get; private set; }

        public IAnalyzer Analyzer { get; private set; }

        public LiteFTS(LiteDatabase db)
        {
            this.Analyzer = new StandardAnalyzer();
            this.Database = db;
            this.Tokens = this.Database.GetCollection<Token>("_fts_token");
        }

        /// <summary>
        /// Break content in valid tokens and insert in _fts_token collection
        /// </summary>
        public void Insert(BsonValue id, string content, int weight = 1)
        {
            if (id == null || id.IsNull) throw new ArgumentException("id");
            if (string.IsNullOrEmpty(content)) throw new ArgumentException("content");

            this.Database.BeginTrans();

            var tokens = this.Analyzer.ParseContent(content);

            foreach (var key in tokens.Keys)
            {
                var token = new Token { Id = key, DocId = id, Weight = weight, Positions = tokens[key] };

                this.Tokens.Insert(token);
            }

            this.Database.Commit();
        }

        /// <summary>
        /// Remove all tokens about this document id
        /// </summary>
        public bool Delete(BsonValue id)
        {
            if (id == null || id.IsNull) throw new ArgumentException("id");

            return this.Tokens.Delete(x => x.DocId == id) > 0;
        }

        /// <summary>
        /// Find all document id that match with query. Result are ordered by score (hi score first)
        /// </summary>
        public IEnumerable<FindResult> Find(string query)
        {
            var tokens = this.Analyzer.ParseContent(query);




            return null;
        }
    }
}
