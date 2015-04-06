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
    /// Represent each occurence of a token in a text search
    /// </summary>
    public class TokenPosition
    {
        [BsonField("p")]
        public int Position { get; set; }

        [BsonField("o")]
        public int Offset { get; set; }

        [BsonField("t")]
        public string Token { get; set; }
    }
}
