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
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class GroupByTests : TestBase
  {
    [Test]
    public void GroupBy_WithAggregateFunction ()
    {
      var query =
          DB.OrderDetails.GroupBy (o => o.OrderID).Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_GroupingWithSeveralAggregateFunction ()
    {
      var query =
          from od in DB.OrderDetails
          group od by od.OrderID
          into orderDetailsByOrderID
          select
              new
              {
                  OrderID = orderDetailsByOrderID.Key,
                  Count = orderDetailsByOrderID.Count(),
                  Sum = orderDetailsByOrderID.Sum (o => o.Quantity),
                  Min = orderDetailsByOrderID.Min (o => o.Quantity)
              };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_TopLevel ()
    {
      var query =
          DB.Orders.GroupBy (o => o.OrderID);

      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      if (IsLinqToSqlActive)
      {
        TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
      }
      else if (IsRelinqSqlBackendActive)
      {
        Assert.That
            (
                () => { TestExecutor.Execute (query, currentMethod); },
                Throws.TypeOf<NotSupportedException>()
                    .With.Message.EqualTo (
                        "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to "
                        + "efficiently return "
                        + "LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional from clause, or perform "
                        + "an aggregation on the groupings. "
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Eg., instead of: "
                        + Environment.NewLine + "'from c in Cooks group c.ID by c.Name', "
                        + Environment.NewLine + "write: "
                        + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
                        + Environment.NewLine + " from c in groupedCooks select new { Key = groupedCooks.Key, Item = c }', "
                        + Environment.NewLine + "or: "
                        + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
                        + Environment.NewLine + " select new { Key = groupedCooks.Key, Count = groupedCooks.Count() }'."));
      }
    }

    [Test]
    public void GroupBy_WithinSubqueryInFromClause ()
    {
      var query =
          from ordersByCustomer in DB.Orders.GroupBy (o => o.Customer)
          where ordersByCustomer.Key.ContactTitle.StartsWith ("Sales")
          select new { ordersByCustomer.Key.ContactTitle, Count = ordersByCustomer.Count() };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression ()
    {
      var query =
          from o in DB.Orders
          group o.OrderID by o.CustomerID
          into orderByCustomerID
          from id in orderByCustomerID
          select new { orderByCustomerID.Key, OrderID = id };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression_WithObject ()
    {
      var query =
          from o in DB.Orders
          group o by o.OrderID
          into orderByOrderId
          from o in orderByOrderId
          where o != null
          select new { orderByOrderId.Key, Order = o };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression_WithSubQuery ()
    {
      var query =
          from o in DB.Orders
          group o.OrderID by o.OrderID
          into orderByOrderID
          from o in
              (
                  from so in orderByOrderID
                  select so).Distinct()
          select new { orderByOrderID.Key, Order = o };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_ResultSelector ()
    {
      var query =
          DB.Orders
              .GroupBy (o => o.CustomerID, (key, group) => key);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_WithSubqueryKey ()
    {
      var query =
          (from o in DB.Orders
            group o by DB.OrderDetails.Where (od => od.Order == o).Select (od => od.Product).Count()).Select (g => g.Key);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_WithConstantKey ()
    {
      var query = DB.Orders.GroupBy (o => 0).Select (c => c.Key);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_NonEntityKey ()
    {
      var query =
          from o in DB.Orders
          group o by o.Customer.CustomerID
          into ordersByCustomer
          from c in DB.Customers
          where c.CustomerID == ordersByCustomer.Key
          select c;

      //Make query stable because of ordering
      var stableResult = query.AsEnumerable().OrderBy (t => t.CustomerID).ThenBy (t => t.PostalCode).ThenBy (t => t.ContactTitle);

      TestExecutor.Execute (stableResult, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_EntityKey ()
    {
      var query =
          from o in DB.Orders
          group o by o.Customer
            into ordersByCustomer
            where ordersByCustomer.Key != null
            select ordersByCustomer.Key.ContactName;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void GroupBy_EntityKey_WithEmptySet ()
    {
      var query =
          from o in DB.Orders
          where o.OrderID != -1
          group o by o.Customer
          into ordersyByCustomer
          select ordersyByCustomer.Key;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void GroupBy_EntityKey_CustomProjection ()
    {
      var query =
          from o in DB.Orders
          from od in o.OrderDetails
          group od.OrderID by od
            into orderDetailsByOrder
            from order in orderDetailsByOrder
            select new { orderDetailsByOrder.Key.Quantity, OrderID = order };

      var stableResult = query.AsEnumerable ().OrderBy (t => t.OrderID).ThenBy (t => t.Quantity);

      TestExecutor.Execute (stableResult, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void GroupBy_AccessKey_Nesting ()
    {
      var query =
          from o in DB.Orders
          from x in
              (
                  from c in o.OrderDetails
                  group c by c.Quantity
                  into ordersByCustomer
                  select new { OrderID = o.OrderID, OrderDetail = ordersByCustomer }
                  )
          let customerName = x.OrderDetail.Key
          where customerName == 25
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}