using LiteDB;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();

            Console.ReadKey();
        }

        static BsonDocument[] list = new []
        {
            new BsonDocument { ["name"] = "john" },
            new BsonDocument { ["name"] = "doe" },
        };

        static async Task MainAsync()
        {
            var e = BsonExpression.Create(@"$");

            var values = e.Execute(list);

            foreach(var value in values)
            {
                Console.WriteLine(value.ToString());
            }
        }

        static async IAsyncEnumerable<BsonDocument> SourceAsync()
        {
            foreach(var doc in list)
            {
                yield return doc;
                await Task.Delay(200);
            }
        }
    }
}
