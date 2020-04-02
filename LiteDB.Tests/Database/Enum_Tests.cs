using System.Linq;
using FluentAssertions;
using Xunit;

namespace LiteDB.Tests.Database
{
    public class Enum_Tests
    {
        public enum PersonSex
        {
            Male,
            Female
        }

        public class Person
        {
            public int Id { get; set; }
            public string Fullname { get; set; }
            public PersonSex Sex { get; set; }
        }


        [Fact]
        public void FindFemale()
        {
            using (var f = new TempFile())
            {
                using (var db = new LiteDatabase(f.Filename))
                {
                    var col = db.GetCollection<Person>("Person");

                    col.Insert(new Person { Fullname = "John", Sex = PersonSex.Male });
                    col.Insert(new Person { Fullname = "Doe", Sex = PersonSex.Male });
                    col.Insert(new Person { Fullname = "Joana", Sex = PersonSex.Female });
                    col.Insert(new Person { Fullname = "Marcus", Sex = PersonSex.Male });
                }
                // close datafile

                using (var db = new LiteDatabase(f.Filename))
                {
                    var PersonSexFemale = PersonSex.Female;
                    var p = db.GetCollection<Person>("Person").Find(x => x.Sex == PersonSexFemale);

                    p.Count().Should().Be(1);
                }
            }
        }
    }
}