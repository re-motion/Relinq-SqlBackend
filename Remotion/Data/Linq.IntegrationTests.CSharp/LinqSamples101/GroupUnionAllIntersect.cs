using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupUnionAllIntersect:Executor
  {
    //This sample uses Concat to return a sequence of all Customer and Employee phone/fax numbers.")]
    public void LinqToSqlUnion01 ()
    {
      var q = (
               from c in db.Customers
               select c.Phone
              ).Concat (
               from c in db.Customers
               select c.Fax
              ).Concat (
               from e in db.Employees
               select e.HomePhone
              );

      serializer.Serialize (q);
    }

    //This sample uses Concat to return a sequence of all Customer and Employee name and phone number mappings.")]
    public void LinqToSqlUnion02 ()
    {
      var q = (
               from c in db.Customers
               select new { Name = c.CompanyName, c.Phone }
              ).Concat (
               from e in db.Employees
               select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone }
              );

      serializer.Serialize (q);
    }

    //This sample uses Union to return a sequence of all countries that either Customers or Employees are in.")]
    public void LinqToSqlUnion03 ()
    {
      var q = (
               from c in db.Customers
               select c.Country
              ).Union (
               from e in db.Employees
               select e.Country
              );

      serializer.Serialize (q);
    }

    //This sample uses Intersect to return a sequence of all countries that both Customers and Employees live in.")]
    public void LinqToSqlUnion04 ()
    {
      var q = (
               from c in db.Customers
               select c.Country
              ).Intersect (
               from e in db.Employees
               select e.Country
              );

      serializer.Serialize (q);
    }

    //This sample uses Except to return a sequence of all countries that Customers live in but no Employees live in.")]
    public void LinqToSqlUnion05 ()
    {
      var q = (
               from c in db.Customers
               select c.Country
              ).Except (
               from e in db.Employees
               select e.Country
              );

      serializer.Serialize (q);
    }
  }
}
