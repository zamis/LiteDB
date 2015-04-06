using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LiteDB;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnitTest
{
    [TestClass]
    public class FtsTest
    {
        [TestMethod]
        public void FtsAnalyzer_Test()
        {
            var a = new StandardAnalyzer();

            var d1 = a.ParseContent("Getting Started");


        }
    }
}
