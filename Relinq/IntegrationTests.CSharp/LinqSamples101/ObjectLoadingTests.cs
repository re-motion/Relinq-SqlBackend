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

using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class ObjectLoadingTests : TestBase
  {
    /// <summary>
    /// This sample demonstrates how navigating through relationships in 
    /// retrieved objects can end up triggering new queries to the database
    /// if the data was not requested by the original query.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Lazy loading")]
    public void LinqToSqlObject01 ()
    {
      //var custs =
      //    from c in DB.Customers
      //    where c.City == "Sao Paulo"
      //    select c;

      //TestExecutor.Execute (custs, MethodBase.GetCurrentMethod ());

      //foreach (var cust in custs)
      //{
      //  foreach (var ord in cust.Orders)
      //    serializer.Serialize (String.Format ("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID));
      //}
    }

    /// <summary>
    /// This sample demonstrates how to use LoadWith to request related 
    /// data during the original query so that additional roundtrips to the 
    /// database are not required later when navigating through 
    /// the retrieved objects.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Load options")]
    public void LinqToSqlObject02 ()
    {
      //Northwind db2 = new Northwind (connString);

      //DataLoadOptions ds = new DataLoadOptions ();
      //ds.LoadWith<Customer> (p => p.Orders);

      //db2.LoadOptions = ds;

      //var custs = (
      //                from c in db2.Customers
      //                where c.City == "Sao Paulo"
      //                select c);

      //TestExecutor.Execute (custs, MethodBase.GetCurrentMethod ());

      //foreach (var cust in custs)
      //{
      //  foreach (var ord in cust.Orders)
      //    serializer.Serialize (String.Format ("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID));
      //}
    }

    /// <summary>
    /// This sample demonstrates how navigating through relationships in 
    /// retrieved objects can end up triggering new queries to the database 
    /// if the data was not requested by the original query. Also this sample shows relationship 
    /// objects can be filtered using Assoicate With when they are deferred loaded.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Load options")]
    public void LinqToSqlObject03 ()
    {
      //Northwind db2 = new Northwind (connString);

      //DataLoadOptions ds = new DataLoadOptions ();
      //ds.AssociateWith<Customer> (p => p.Orders.Where (o => o.ShipVia > 1));

      //db2.LoadOptions = ds;
      //var custs =
      //    from c in db2.Customers
      //    where c.City == "London"
      //    select c;

      //foreach (var cust in custs)
      //{
      //  foreach (var ord in cust.Orders)
      //  {
      //    foreach (var orderDetail in ord.OrderDetails)
      //    {
      //      serializer.Serialize (
      //          String.Format (
      //              "CustomerID {0} has an OrderID {1} that ShipVia is {2} with ProductID {3} that has name {4}.",
      //              cust.CustomerID,
      //              ord.OrderID,
      //              ord.ShipVia,
      //              orderDetail.ProductID,
      //              orderDetail.Product.ProductName));
      //    }
      //  }
      //}
    }


    /// <summary>
    /// This sample demonstrates how to use LoadWith to request related 
    /// data during the original query so that additional roundtrips to the 
    /// database are not required later when navigating through 
    /// the retrieved objects. Also this sample shows relationship 
    /// objects can be ordered by using Assoicate With when they are eager loaded.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Load options")]
    public void LinqToSqlObject04 ()
    {
      //Northwind db2 = new Northwind (connString);

      //DataLoadOptions ds = new DataLoadOptions ();
      //ds.LoadWith<Customer> (p => p.Orders);
      //ds.LoadWith<Order> (p => p.OrderDetails);
      //ds.AssociateWith<Order> (p => p.OrderDetails.OrderBy (o => o.Quantity));

      //db2.LoadOptions = ds;

      //var custs = (
      //                from c in db2.Customers
      //                where c.City == "London"
      //                select c);

      //foreach (var cust in custs)
      //{
      //  foreach (var ord in cust.Orders)
      //  {
      //    foreach (var orderDetail in ord.OrderDetails)
      //    {
      //      serializer.Serialize (
      //          string.Format (
      //              "CustomerID {0} has an OrderID {1} with ProductID {2} that has Quantity {3}.",
      //              cust.CustomerID,
      //              ord.OrderID,
      //              orderDetail.ProductID,
      //              orderDetail.Quantity));
      //    }
      //  }
      //}
    }

    /// <summary>
    /// This sample demonstrates how navigating through relationships in 
    /// retrieved objects can result in triggering new queries to the database
    /// if the data was not requested by the original query.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Lazy loading")]
    public void LinqToSqlObject05 ()
    {
      //var emps = from e in db.Employees
      //           select e;

      //foreach (var emp in emps)
      //{
      //  foreach (var man in emp.Employees)
      //  {
      //    Console.WriteLine ("Employee {0} reported to Manager {1}.", emp.FirstName, man.FirstName);
      //  }
      //}
    }

    /// <summary>
    /// this sample demonstrates how navigating through link in 
    /// retrieved objects can end up triggering new queries to the database
    /// if the data type is link.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Lazy loading")]
    public void LinqToSqlObject06 ()
    {
      //var emps = from c in db.Employees
      //           select c;

      //foreach (Employee emp in emps)
      //{
      //  Console.WriteLine ("{0}", emp.Notes);
      //}
    }

    /// <summary>
    /// This samples overrides the partial method LoadProducts in Category class. When products of a category are being loaded,
    /// LoadProducts is being called to load products that are not discontinued in this category.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Load methods")]
    public void LinqToSqlObject07 ()
    {
      //Northwind db2 = new Northwind (connString);

      //DataLoadOptions ds = new DataLoadOptions ();

      //ds.LoadWith<Category> (p => p.Products);
      //db2.LoadOptions = ds;

      //var q = (
      //            from c in db2.Categories
      //            where c.CategoryID < 3
      //            select c);

      //TestExecutor.Execute (q, MethodBase.GetCurrentMethod ());
    }
  }
}