using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class OrderByTests : TestBase
  {
    /// <summary>
    /// This sample uses orderby to sort Employees by hire date.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy01()
    {
      var q =
          from e in DB.Employees
          orderby e.HireDate
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses where and orderby to sort Orders shipped to London by freight.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy02()
    {
      var q =
          from o in DB.Orders
          where o.ShipCity == "London"
          orderby o.Freight
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Products by unit price from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy03()
    {
      var q =
          from p in DB.Products
          orderby p.UnitPrice descending
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a compound orderby to sort Customers by city and then contact name.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy04()
    {
      var q =
          from c in DB.Customers
          orderby c.City , c.ContactName
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Orders from EmployeeID 1 by ship-to country, and then by freight from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy05()
    {
      var q =
          from o in DB.Orders
          where o.EmployeeID == 1
          orderby o.ShipCountry , o.Freight descending
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby, Max and Group By to find the Products that have the highest 
    /// unit price in each category, and sorts the group by category id.
    /// </summary>
    [Test]
    [Ignore ("RM-3207: When a NewExpression contains a subquery whose original type is IEnumerable<T>, an ArgumentException (wrapped into a "
        + "TargetInvocationException) is thrown")]
    public void LinqToSqlOrderBy06()
    {
      var categories =
          from p in DB.Products
          group p by p.CategoryID
          into g
          orderby g.Key
          select new
                   {
                       g.Key,
                       MostExpensiveProducts =
              from p2 in g
              where p2.UnitPrice == g.Max (p3 => p3.UnitPrice)
              select p2
                   };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod());
    }
  }
}