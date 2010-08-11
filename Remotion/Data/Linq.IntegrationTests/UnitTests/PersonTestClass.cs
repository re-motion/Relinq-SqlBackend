using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests
{
  [Table (Name = "Person")]
  class PersonTestClass
  {
    public PersonTestClass ()
    {
    }

    public PersonTestClass (string first, int age)
    {
      First = first;
      Age = age;
    }

    [Column (Name = "FirstName", IsPrimaryKey = true)]
    public string First { get; set; }

    [Column (Name = "Age")]
    public int Age { get; set; }

    public override bool Equals (object obj)
    {
      if (obj == null || GetType () != obj.GetType ())
      {
        return false;
      }

      if (!((PersonTestClass) obj).First.Equals (First))
        return false;
      if (!((PersonTestClass) obj).Age.Equals (Age))
        return false;
      return true;
    }

    public override int GetHashCode ()
    {
      throw new NotImplementedException ();
    }
  }
}
