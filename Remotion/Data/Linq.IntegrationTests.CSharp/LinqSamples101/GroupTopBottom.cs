using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupTopBottom:Executor
  {
    //This sample uses Take to select the first 5 Employees hired.")]
    public void LinqToSqlTop01 ()
    {
      var q = (
          from e in db.Employees
          orderby e.HireDate
          select e)
          .Take (5);

      serializer.Serialize (q);
    }

    //This sample uses Skip to select all but the 10 most expensive Products.")]
    public void LinqToSqlTop02 ()
    {
      var q = (
          from p in db.Products
          orderby p.UnitPrice descending
          select p)
          .Skip (10);

      serializer.Serialize (q);
    }
  }
}
