using System;
using System.Data.Linq.SqlClient;
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupSqlMethods:Executor
  {
    //This sample uses SqlMethods to filter for Customers with CustomerID that starts with 'C'.")]
    public void LinqToSqlSqlMethods01 ()
    {

      var q = from c in db.Customers
              where SqlMethods.Like (c.CustomerID, "C%")
              select c;

      serializer.Serialize (q);

    }

    //This sample uses SqlMethods to find all orders which shipped within 10 days the order created")]
    public void LinqToSqlSqlMethods02 ()
    {

      var q = from o in db.Orders
              where SqlMethods.DateDiffDay (o.OrderDate, o.ShippedDate) < 10
              select o;

      serializer.Serialize (q);

    }

  }
}
