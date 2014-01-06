using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class NullTests : TestBase
  {
    /// <summary>
    /// This sample uses the null value to find Employees that do not report to another Employee.
    /// </summary>
    [Test]
    public void LinqToSqlNull01()
    {
      var q =
          from e in DB.Employees
          where e.ReportsToEmployee == null
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Nullable{T}.HasValue to find Employees that do not report to another Employee.
    /// </summary>
    [Test]
    public void LinqToSqlNull02()
    {
      var q =
          from e in DB.Employees
          where !e.ReportsTo.HasValue
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Nullable{T}.Value for Employees that report to another Employee to 
    /// return the EmployeeID number of that employee. Note that the .Value is optional.
    /// </summary>
    [Test]
    public void LinqToSqlNull03 ()
    {
      var q =
          from e in DB.Employees
          where e.ReportsTo.HasValue
          select new {e.FirstName, e.LastName, ReportsTo = e.ReportsTo.Value};

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}