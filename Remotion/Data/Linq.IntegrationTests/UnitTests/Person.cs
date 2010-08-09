using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests
{
  [Table (Name = "Person")]
  class Person
  {
    public Person ()
    {
    }

    public Person (string first, int age)
    {
      First = first;
      Age = age;
      //this.p_3 = p_3;
      //this.p_4 = p_4;
    }

    [Column (Name = "First")]
    public string First { get; set; }

    [Column (Name = "Age")]
    public int Age { get; set; }

    public override bool Equals (object obj)
    {
      //       
      // See the full list of guidelines at
      //   http://go.microsoft.com/fwlink/?LinkID=85237  
      // and also the guidance for operator== at
      //   http://go.microsoft.com/fwlink/?LinkId=85238
      //

      if (obj == null || GetType() != obj.GetType())
      {
        return false;
      }

      if (!((Person) obj).First.Equals (First))
        return false;
      if (!((Person) obj).Age.Equals (Age))
        return false;
      return true;
    }

    // override object.GetHashCode
    public override int GetHashCode ()
    {
      // TODO: write your implementation of GetHashCode() here
      throw new NotImplementedException();
      return base.GetHashCode();
    }
  }
}
