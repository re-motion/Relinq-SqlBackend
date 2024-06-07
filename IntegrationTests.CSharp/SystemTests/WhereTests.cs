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
using Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind;

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class WhereTests : TestBase
  {
    [Test]
    public void QueryWithStringLengthProperty ()
    {
      var query =
          from c in DB.Customers
          where c.City.Length == "London".Length
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithStringIsNullOrEmpty ()
    {
      IQueryable query = null;

      if (IsLinqToSqlActive)
      {
        //LinqToSQL has string.IsNullOrEmpty not implemented
        query =
            from c in DB.Customers
            where !(c.Region == null || c.Region == string.Empty)
            select c;
      }
      else if (IsRelinqSqlBackendActive)
      {
        query =
            from c in DB.Customers
            where !string.IsNullOrEmpty (c.Region)
            select c;
      }

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereConditions ()
    {
      var query =
          from c in DB.Customers
          where c.City == "Berlin" || c.City == "London"
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereConditionsAndNull ()
    {
      var query =
          from c in DB.Customers
          where c.Region != null
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereConditionAndStartsWith ()
    {
      var query =
          from c in DB.Customers
          where c.PostalCode.StartsWith ("H1J")
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereConditionAndStartsWith_NonConstantValue ()
    {
      var query =
          from c in DB.Customers
          where c.PostalCode.StartsWith (DB.Customers.Select (x => x.Fax.Substring (0, 3)).First())
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereConditionAndEndsWith ()
    {
      var query =
          from c in DB.Customers
          where c.PostalCode.EndsWith ("876")
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereAndEndsWith_NonConstantValue ()
    {
      var query =
          from c in DB.Customers
          where c.City.EndsWith (DB.Customers.Select (x => x.City.Substring (x.PostalCode.Length - 2)).First())
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContains_Like ()
    {
      var query =
          from c in DB.Customers
          where c.CompanyName.Contains ("restauration")
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContains_Like_NonConstantValue ()
    {
      var query =
          from c in DB.Customers
          where c.City.Contains (DB.Customers.OrderBy (x => x.City).Select (y => y.City.Substring (1, 2)).First())
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContains_Collection_ConstantValue ()
    {
      var cities = new[] { "Berlin", "München", "Graz" };
      var query =
          from c in DB.Customers
          where cities.Contains (c.City)
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContains_Collection_NonConstantValue ()
    {
      var cities = from customer in DB.Customers where customer.Country == "Germany" select customer.City;
      var query =
          from c in DB.Customers
          where cities.Contains (c.City)
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_OuterObject ()
    {
      Customer customer = DB.Customers.First();

      var query =
          from c in DB.Customers
          where c == customer
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyOnly ()
    {
      var query =
          from p in DB.Products
          where p.Discontinued
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanProperty_ExplicitComparison ()
    {
      var query =
          from p in DB.Products
          // ReSharper disable RedundantBoolCompare
          where p.Discontinued == true
          // ReSharper restore RedundantBoolCompare
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyOnly_Negate ()
    {
      var query =
          from p in DB.Products
          where !p.Discontinued
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyAndAnother ()
    {
      int discontinuedProductID = 5;

      var query =
          from p in DB.Products
          where p.ProductID == discontinuedProductID && p.Discontinued
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyAndAnother_Negate ()
    {
      int notDiscontinuedProductID = 4;

      var query =
          from p in DB.Products
          where p.ProductID == notDiscontinuedProductID && !p.Discontinued
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyAndAnother_ExplicitComparison_True ()
    {
      int discontinuedProductID = 5;

      var query =
          from p in DB.Products
          // ReSharper disable RedundantBoolCompare
          where p.ProductID == discontinuedProductID && p.Discontinued == true
          // ReSharper restore RedundantBoolCompare
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhere_BooleanPropertyAndAnother_ExplicitComparison_False ()
    {
      int notDiscontinuedProductID = 4;

      var query =
          from p in DB.Products
          where p.ProductID == notDiscontinuedProductID && p.Discontinued == false
          select p;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithVirtualKeySide_EqualsNull ()
    {
      var query =
          from e in DB.Employees
          where e.ReportsToEmployee == null
          select e;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithVirtualKeySide_NotEqualsNull ()
    {
      var query =
          from e in DB.Employees
          where e.ReportsToEmployee != null
          select e;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithVirtualKeySide_EqualsOuterObject ()
    {
      Product product = DB.Products.First();

      var query =
          from od in DB.OrderDetails
          where od.Product == product
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithVirtualKeySide_NotEqualsOuterObject ()
    {
      Product product = DB.Products.First();

      var query =
          from od in DB.OrderDetails
          where od.Product != product
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithIDInCondition ()
    {
      Product product = DB.Products.First();

      var query =
          from od in DB.OrderDetails
          where od.ProductID == product.ProductID
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithWhereOnForeignKey_RealSide ()
    {
      int orderID = DB.Orders.Select (x => x.OrderID).First();

      var query =
          from od in DB.OrderDetails
          where od.Order.OrderID == orderID
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithConditionTrueInWherePart ()
    {
      var firstOrder = DB.Orders.First();

      var query =
          from o in DB.Orders
          where o.OrderID == (true ? firstOrder.OrderID : o.OrderID)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithConditionFalseInWherePart ()
    {
      var query =
          from o1 in DB.Orders
          where o1.OrderID == (false ? 1 : o1.OrderID)
          select o1;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithEqualConditionInWherePart ()
    {
      var query =
          from o2 in DB.Orders
          where o2.OrderID == (o2.OrderID == 1 ? 2 : 3)
          select o2;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_Is ()
    {
      var query = DB.Contacts.Where (c => c is CustomerContact);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}