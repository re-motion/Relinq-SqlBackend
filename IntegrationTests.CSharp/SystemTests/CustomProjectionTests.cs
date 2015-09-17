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
  public class CustomProjectionTests : TestBase
  {
    [Test]
    public void SequenceOfEntityProperties ()
    {
      var query =
          from od in DB.OrderDetails
          where od.Quantity <= 55
          select od.Quantity;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SequenceOfPrimaryKeys ()
    {
      var query =
          (from od in DB.OrderDetails
            where od.Quantity <= 55
            select od.OrderID);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SequenceOfForeignKeyIDs ()
    {
      var query =
          (from od in DB.OrderDetails
            where od.Quantity == 10
            select od.Order.OrderID);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-99: Support .Count on Collections")]
    public void ComplexProjection ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select new { o.OrderID, o.OrderDate, Property = new { o.Customer.ContactName, o.OrderDetails.Count } });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-99: Support .Count on Collections")]
    public void ComplexProjection_WithSingleQuery ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select new { o.OrderID, o.OrderDate, Property = new { o.Customer.ContactName, o.OrderDetails.Count } }).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ComplexProjection_ContainingEntity ()
    {
      var query =
          (from o in DB.Orders
            select new { o.OrderID, o });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SingleBoolean ()
    {
      var query =
          (from p in DB.Products
            where p.ProductID == 2
            select p.Discontinued);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SingleNullableBoolean ()
    {
      var query =
          (from e in DB.Employees
            where e.EmployeeID == 2
            select e.HasCar);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ComplexProjection_WithBooleans ()
    {
      var query =
          (from p in DB.Products
            where p.ProductID == 2
            select new { p.Discontinued });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ComplexProjection_WithNullableBooleans ()
    {
      var query =
          (from p in DB.Employees
           where p.EmployeeID == 2
           select new { p.HasCar });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void SingleProperty ()
    {
      var query =
          (from p in DB.Products
            where p.ProductID == 2
            select p.ProductName);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SingleValueType ()
    {
      var query =
          from e in DB.Employees
          where e.EmployeeID == 1
          select e.EmployeeID;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void SingleValueType_Nullable ()
    {
      var query =
          from e in DB.Employees
          where e.ReportsTo != null
          select e.ReportsTo;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ComplexProjection_WithValueTypes ()
    {
      var query =
          (from p in DB.Products
            where p.ProductID == 2
            select new { p.ProductID });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ComplexProjection_WithNullableValueTypes ()
    {
      var query =
          (from e in DB.Employees
           where e.ReportsTo != null
           select new { e.ReportsTo });

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }
  }
}