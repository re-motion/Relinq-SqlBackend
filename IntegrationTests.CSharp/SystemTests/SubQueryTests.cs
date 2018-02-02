// Microsoft Public License (Ms-PL)
// 
// This license governs use of the accompanying software. If you use the software, you
// accept this license. If you do not accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
// same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
// prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
// sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
// such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
// in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
// this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
// license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
// the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.IntegrationTests.Common;
using Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind;

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class SubQueryTests : TestBase
  {
    [Test]
    public void QueryWithSubQuery_InWhere ()
    {
      var query =
          from o in DB.Orders
          where (from c in DB.Customers select c).Contains (o.Customer)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQueryInWhere_AccessingOuterVariable_InMainFromClause ()
    {
      var orders = DB.Orders.Single (x => x.OrderID == 10248);

      var query =
          from c in DB.Customers
          where (from o in c.Orders select o).Contains (orders)
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQueryAndJoinInWhere ()
    {
      var query =
          from o in DB.Orders
          where (from od in DB.OrderDetails select od.Order).Contains (o)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQueryAndJoinInWhere_WithOuterVariable ()
    {
      var outerCustomer = DB.Orders.Single (x => x.OrderID == 10248);

      var query =
          from customer in DB.Customers
          where (from order in DB.Orders where order.Customer == customer select order).Contains (outerCustomer)
          select customer;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQuery_InMainFrom ()
    {
      var query =
          from c in (
              from cu in DB.Customers
              select cu
              )
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQuery_WithResultOperator_InMainFrom ()
    {
      var query =
          from c in (from or in DB.Orders
            orderby or.OrderID
            select or).Take (1)
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQuery_InAdditionalFrom ()
    {
      var query =
          from o in DB.Orders
          from od in
              (from od in DB.OrderDetails where od.Order == o select od)
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQuery_InThirdFrom ()
    {
      var query =
          (from o1 in DB.Orders
            from o2 in DB.Orders
            from od in
                (from od in DB.OrderDetails where od.Order == o1 || od.Order == o2 select od)
            select od).Distinct();

      //Make query stable because of ordering
      var stableResult = query.AsEnumerable().OrderBy (o => o.OrderID).ThenBy (o => o.ProductID);

      TestExecutor.Execute (stableResult, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubQuery_InSelectClause ()
    {
      var query =
          from o in DB.Orders
          select
              (from p in DB.Shippers select p);

      //Get currentMethod because calling it inside a lambda returns a false result
      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      if (IsLinqToSqlActive)
      {
        TestExecutor.Execute (query, currentMethod);
      }
      else if (IsRelinqSqlBackendActive)
      {
        Assert.That (
            () => TestExecutor.Execute (query, currentMethod),
            Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo (
                    "Queries selecting collections are not supported because SQL is not well-suited to returning collections. You can use "
                    + "SelectMany or an additional 'from' clause to return the elements of the collection, grouping them in-memory."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Ie., instead of 'from c in Cooks select c.Assistants', write the following query: "
                    + Environment.NewLine
                    + "'(from c in Cooks from a in Assistants select new { GroupID = c.ID, Element = a }).AsEnumerable().GroupBy (t => t.GroupID, t => t.Element)'"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Note that above query will group the query result in-memory, which might be inefficient, depending on the number of results returned "
                    + "by the query."));
      }
    }

    [Test]
    public void SubQueryWithNonConstantFromExpression ()
    {
      var query =
          from o in DB.Orders
          from od in (from od1 in o.OrderDetails select od1)
          where o.OrderID == 10248
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void FirstOrDefault_WithEntity_InSelectAndWhere ()
    {
      var query =
          from c in DB.Customers
          where c.Orders.FirstOrDefault() != null
          select c.Orders.OrderBy (od => od.OrderID).FirstOrDefault();

      //Make query stable because of ordering
      var stableResult = query.AsEnumerable().OrderBy (t => t.OrderDate).ThenBy (t => t.Freight).ThenBy (t => t.OrderID);

      TestExecutor.Execute (stableResult, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void OrderingsInSubQuery_WithDistinct ()
    {
      var query =
          from o in (
              from od in DB.OrderDetails
              where od.Order != null
              orderby od.Order.OrderID
              select od.Order).Distinct()
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void OrderingsInSubQuery_WithTake ()
    {
      var query =
          from o in (from o in DB.Orders
            orderby o.OrderID
            select o).Take (2)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void OrderingsInSubQuery_WithoutTakeOrDistinct ()
    {
      var query =
          from c in DB.Customers
          where c.CustomerID == "ALFKI"
          from o in (from o in c.Orders orderby o.OrderID ascending select o)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void OrderingsInSubQuery_WithoutTakeOrDistinct_WithAccessToMemberOfSubQuery ()
    {
      var query =
          from c in DB.Customers
          where c.CustomerID == "ALFKI"
          from o in (from o in c.Orders orderby o.OrderID ascending select o)
          where o.OrderID < 10249
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void MemberAccess_OnSubQuery_WithEntities ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select (from od in o.OrderDetails
              orderby od.OrderID
              select od).First().Product).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void MemberAccess_OnSubQuery_WithColumns ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select (from od in o.OrderDetails
              orderby od.OrderID
              select od.Order.CustomerID).First().Length).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-104: Support conversion operator for Query Source")]
    public void SecondquerySource_WithCastToGenericIEnumerable ()
    {
      //This Pattern occurs in VBNet when using orderby in Subquery
      var queryResult = DB.Orders.SelectMany (
          o => (IEnumerable<Customer>) DB.Customers.OrderBy (c => c.City),
          (o, i) => i);

      queryResult.ToArray ();
    }

    [Test]
    [Ignore ("RMLNQSQL-104: Support conversion operator for Query Source")]
    public void SecondQuerySource_WithExpressionCastToGeneric ()
    {
      var query =
          DB.Products.Where (
              (p =>
                  0
                  != DB.Suppliers.Select ((s => s.SupplierID))
                      .Concat (DB.Suppliers.Select ((Expression<Func<Supplier, int>>) (s => s.SupplierID)))
                      .Count()));
      
      query.ToArray();
    }

    [Test]
    [Ignore ("RMLNQSQL-104: Support conversion operator for Query Source")]
    public void SecondQuerySource_WithExpressionCastToIEnumerable ()
    {
      var query =
          DB.Products.Where (
              (p =>
                  0
                  != DB.Suppliers.Select ((s => s.SupplierID))
                      .Concat ((IEnumerable<int>) DB.Suppliers.Select ((s => s.SupplierID)))
                      .Count ()));

      query.ToArray ();
    }
  }
}