//Microsoft Public License (Ms-PL)

//This license governs use of the accompanying software. If you use the software, you
//accept this license. If you do not accept the license, do not use the software.

//1. Definitions
//The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
//same meaning here as under U.S. copyright law.
//A "contribution" is the original software, or any additions or changes to the software.
//A "contributor" is any person that distributes its contribution under this license.
//"Licensed patents" are a contributor's patent claims that read directly on its contribution.

//2. Grant of Rights
//(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
//prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
//sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//3. Conditions and Limitations
//(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
//such contributor to the software ends automatically.
//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
//in the software.
//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
//this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
//license that complies with this license.
//(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
//You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
//the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class JoinTests:TestBase
  {
    /// <summary>
    ///  This sample uses foreign key navigation in the from clause to select all orders for customers in London.
    /// </summary>
    [Test]
    public void LinqToSqlJoin01 ()
    {
      var q =
          from c in DB.Customers
          from o in c.Orders
          where c.City == "London"
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample uses foreign key navigation in the where clause to filter for Products whose Supplier is in the USA that are out of stock.
    /// </summary>
    [Test]
    public void LinqToSqlJoin02 ()
    {
      var q =
          from p in DB.Products
          where p.Supplier.Country == "USA" && p.UnitsInStock == 0
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample uses foreign key navigation in the from clause to filter for employees in Seattle, and also list their territories.
    /// </summary>
    [Test]
    public void LinqToSqlJoin03 ()
    {
      var q =
          from e in DB.Employees
          from et in e.EmployeeTerritories
          where e.City == "Seattle"
          select new { e.FirstName, e.LastName, et.Territory.TerritoryDescription };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample uses foreign key navigation in the select clause to filter for pairs of employees where one employee reports to the other and where both employees are from the same City.
    /// </summary>
    [Test]
    public void LinqToSqlJoin04 ()
    {
      var q =
          from e1 in DB.Employees
          from e2 in e1.Employees
          where e1.City == e2.City
          select new
          {
            FirstName1 = e1.FirstName,
            LastName1 = e1.LastName,
            FirstName2 = e2.FirstName,
            LastName2 = e2.LastName,
            e1.City
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample explicitly joins two tables and projects results from both tables.
    /// </summary>
    [Test]
    public void LinqToSqlJoin05 ()
    {
      var q =
          from c in DB.Customers
          join o in DB.Orders on c.CustomerID equals o.CustomerID into orders
          select new { c.ContactName, OrderCount = orders.Count () };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample explicitly joins three tables and projects results from each of them.
    /// </summary>
    [Test]
    public void LinqToSqlJoin06 ()
    {
      var q =
          from c in DB.Customers
          join o in DB.Orders on c.CustomerID equals o.CustomerID into ords
          join e in DB.Employees on c.City equals e.City into emps
          select new { c.ContactName, ords = ords.Count (), emps = emps.Count () };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). The DefaultIfEmpty() method returns null when there is no Order for the Employee.
    /// </summary>
    [Test]
    public void LinqToSqlJoin07 ()
    {
      var q =
          from e in DB.Employees
          join o in DB.Orders on e equals o.Employee into ords
          from o in ords.DefaultIfEmpty ()
          select new { e.FirstName, e.LastName, Order = o };

      // Added to make query result stable.
      var stableResult = q.AsEnumerable().OrderBy (t => t.FirstName).ThenBy (t => t.LastName).ThenBy (t => t.Order.OrderID);
      TestExecutor.Execute (stableResult, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample projects a 'let' expression resulting from a join.
    /// </summary>
    [Test]
    public void LinqToSqlJoin08 ()
    {
      var q =
          from c in DB.Customers
          join o in DB.Orders on c.CustomerID equals o.CustomerID into ords
          let z = c.City + c.Country
          from o in ords
          select new { c.ContactName, o.OrderID, z };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample shows a join with a composite key.
    /// </summary>
    [Test]
    public void LinqToSqlJoin09 ()
    {
      var q =
          from o in DB.Orders
          from p in DB.Products
          join d in DB.OrderDetails
              on new { o.OrderID, p.ProductID } equals new { d.OrderID, d.ProductID }
              into details
          from d in details
          select new { o.OrderID, p.ProductID, d.UnitPrice };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  This sample shows how to construct a join where one side is nullable and the other is not.
    /// </summary>
    [Test]
    public void LinqToSqlJoin10 ()
    {
      var q =
          from o in DB.Orders
          join e in DB.Employees
              on o.EmployeeID equals (int?) e.EmployeeID into emps
          from e in emps
          select new { o.OrderID, e.FirstName };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

  }
}
