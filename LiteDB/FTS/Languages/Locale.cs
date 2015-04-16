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
    /// </summary>
    public class Locale
    {
        public virtual string Normalize(string token)
        {
            return token;
        }

        #region Static access

        public static readonly Locale None = new EnglishLocale();

        public static readonly Locale English = new EnglishLocale();

        private static Dictionary<string, Locale> _locales = new Dictionary<string, Locale>();

        static Locale()
        {
            _locales["none"] = None;
            _locales["english"] = English;
        }

        public static void Register<T>(string name)
            where T : Locale, new()
        {
            _locales[name] = new T();
        }

        public static Locale Get(string name)
        {
            return _locales[name];
        }

        #endregion
    }
}
