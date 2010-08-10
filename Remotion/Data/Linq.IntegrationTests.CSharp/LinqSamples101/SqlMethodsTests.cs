using System;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class SqlMethodsTests:TestBase
  {
    /// <summary>
    /// This sample uses SqlMethods to filter for Customers with CustomerID that starts with 'C'.
    /// </summary>
    [Test]
    public void LinqToSqlSqlMethods01 ()
    {

      var q = from c in DB.Customers
              where SqlMethods.Like (c.CustomerID, "C%")
              select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());

    }

    /// <summary>
    /// This sample uses SqlMethods to find all orders which shipped within 10 days the order created")]
    /// </summary>
    [Test]
    public void LinqToSqlSqlMethods02 ()
    {

      var q = from o in DB.Orders
              where SqlMethods.DateDiffDay (o.OrderDate, o.ShippedDate) < 10
              select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());

    }

  }
}
