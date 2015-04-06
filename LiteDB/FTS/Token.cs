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
    /// Represent a token document that stores each token content for full text seach
    /// </summary>
    public class Token
    {
        public string Id { get; set; }

        [BsonField("$id")]
        public BsonValue DocId { get; set; }

        [BsonField("w")]
        public int Weight { get; set; }

        [BsonField("pos")]
        public List<TokenPosition> Positions { get; set; }
    }
}
