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
using Remotion.Linq.IntegrationTests.Common;

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class SelectTests : TestBase
  {
    private class ProductData
    {
      public int ProductID { get; private set; }

      public string Name { get; set; }

      public bool Discontinued { get; set; }

      public bool HasUnitsInStock { get; set; }

      public ProductData (int productID)
      {
        ProductID = productID;
      }
    }

    [Test]
    [Ignore ("RM-3306: Support for MemberInitExpressions")]
    public void WithMemberInitExpression_InOuterMostLevel ()
    {
      var query = DB.Products.Select (
          p => new ProductData (p.ProductID) { Name = p.ProductName, Discontinued = p.Discontinued, HasUnitsInStock = p.UnitsInStock > 0 });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SimpleQuery_WithRelatedEntity ()
    {
      var query =
          from od in DB.OrderDetails
          select od.Order;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void MethodCallOnCoalesceExpression ()
    {
      var query =
          from o in DB.Orders
          where (o.ShipRegion ?? o.Customer.Region).ToUpper() == "ISLE OF WIGHT"
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void MethodCallOnConditionalExpression ()
    {
      var query =
          from o in DB.Orders
          where (o.ShipRegion == "Isle of Wight" ? o.ShipRegion : o.Customer.Region).ToUpper() == "ISLE OF WIGHT"
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void LogicalMemberAccessOnCoalesceExpression ()
    {
      var query =
          from o in DB.Orders
          where (o.ShipRegion ?? o.Customer.Region).Length == 2
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void LogicalMemberAccessOnConditionalExpression ()
    {
      var query =
          from o in DB.Orders
          where (o.ShipRegion == "Isle of Wight" ? o.ShipRegion : o.Customer.Region).Length == 13
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-93: When the Coalesce operator is used with relations"
             + "(.i.e. their ID and ForeignKey-columns), nullable<valueType> gets compared with valueType, resulting in an ArgumentException")]
    public void CoalesceExpression_ColumnMember ()
    {
      var query =
          from e in DB.Employees
          where (e.ReportsToEmployee ?? e).EmployeeID == 1
          select e;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithConstant ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select 1).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithObjectID ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select o.OrderID).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    public override TestMode Mode
    {
      get { return TestMode.SaveReferenceResults; }
    }

    [Test]
    public void Query_WithNullableValueInProjectionForNotNullableColumn ()
    {
      var query =
          from o in DB.Orders
          select new
                 {
                     Order = o,
                     Employee = (from e in DB.Employees where e.EmployeeID == o.EmployeeID && e.FirstName.StartsWith ("A") select e).FirstOrDefault()
                 }
          into _
          where _.Order.OrderID == 10248
          select new { _.Order.OrderID, EmployeeID = (int?) _.Employee.EmployeeID };

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}