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

using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class AggregatesTests : TestBase
  {
    ///<summary>
    ///This sample uses Count to find the number of Customers in the database.
    ///</summary>
    [Test]
    public void LinqToSqlCount01 ()
    {
      var q = DB.Customers.Count();
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
    
    ///<summary>
    ///This sample uses Count to find the number of Products in the database that are not discontinued.
    ///</summary>
    [Test]
    public void LinqToSqlCount02 ()
    {
      var q = DB.Products.Count (p => !p.Discontinued);
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
    
    /// <summary>
    /// This sample uses Sum to find the total freight over all Orders.
    /// </summary>
    [Test]
    public void LinqToSqlCount03 ()
    {
      var q = DB.Orders.Select (o => o.Freight).Sum();
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///This sample uses Sum to find the total number of units on order over all Products.
    /// </summary>
    [Test]
    public void LinqToSqlCount04 ()
    {
      var q = DB.Products.Sum (p => p.UnitsOnOrder);
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
    
    /// <summary>
    /// This sample uses Min to find the lowest unit price of any Product.
    /// </summary>
    [Test]
    public void LinqToSqlCount05 ()
    {
      var q = DB.Products.Select (p => p.UnitPrice).Min();
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
    
    /// <summary>
    /// This sample uses Min to find the lowest freight of any Order.
    /// </summary>
    [Test]
    public void LinqToSqlCount06 ()
    {
      var q = DB.Orders.Min (o => o.Freight);
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Min to find the Products that have the lowest unit price in each category.
    /// </summary>
    [Test]
    [Ignore ("RM-3265: Support collections to be selected at the top level of a query")]
    public void LinqToSqlCount07 ()
    {
      var categories =
          from p in DB.Products
          group p by p.CategoryID
          into g
          select new
                 {
                     CategoryID = g.Key,
                     CheapestProducts =
              from p2 in g
              where p2.UnitPrice == g.Min (p3 => p3.UnitPrice)
              select p2
                 };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Max to find the latest hire date of any Employee.
    /// </summary>
    [Test]
    public void LinqToSqlCount08 ()
    {
      var q = DB.Employees.Select (e => e.HireDate).Max();
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Max to find the most units in stock of any Product.
    /// </summary>
    [Test]
    public void LinqToSqlCount09 ()
    {
      var q = DB.Products.Max (p => p.UnitsInStock);
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
    
    /// <summary>
    /// This sample uses Max to find the Products that have the highest unit price in each category.
    /// </summary>
    [Test]
    [Ignore ("RM-3265: Support collections to be selected at the top level of a query")]
    public void LinqToSqlCount10 ()
    {
      var categories =
          from p in DB.Products
          group p by p.CategoryID
          into g
          select new
                 {
                     g.Key,
                     MostExpensiveProducts =
              from p2 in g
              where p2.UnitPrice == g.Max (p3 => p3.UnitPrice)
              select p2
                 };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod ());
    }


    /// <summary>
    /// This sample uses Average to find the average freight of all Orders.
    /// </summary>
    [Test]
    public void LinqToSqlCount11 ()
    {
      var q = DB.Orders.Select (o => o.Freight).Average();
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }


    /// <summary>
    /// This sample uses Average to find the average unit price of all Products.
    /// </summary>
    [Test]
    public void LinqToSqlCount12 ()
    {
      var q = DB.Products.Average (p => p.UnitPrice);
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }


    /// <summary>
    /// This sample uses Average to find the Products that have unit price higher than the average unit price of the category for each category.
    /// </summary>
    [Test]
    [Ignore ("RM-3265: Support collections to be selected at the top level of a query")]
    public void LinqToSqlCount13 ()
    {
      var categories =
          from p in DB.Products
          group p by p.CategoryID
          into g
          select new
                 {
                     g.Key,
                     ExpensiveProducts =
              from p2 in g
              where p2.UnitPrice > g.Average (p3 => p3.UnitPrice)
              select p2
                 };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod ());
    }
  }
}