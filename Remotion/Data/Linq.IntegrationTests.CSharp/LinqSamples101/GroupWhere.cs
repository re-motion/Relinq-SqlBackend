using System;
using System.IO;
using System.Linq;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupWhere:Executor
  {
    //This sample uses WHERE to filter for Customers in London.

    public void LinqToSqlWhere01 ()
    {
      var q =
          from c in db.Customers
          where c.City == "London"
          select c;
      ObjectDumper.Write (q);
    }
  }
}
