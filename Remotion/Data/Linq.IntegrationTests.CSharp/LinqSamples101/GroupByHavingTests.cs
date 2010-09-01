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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class GroupByHavingTests:TestBase
  {
    /// <summary>
    /// This sample uses group by to partition Products by CategoryID.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - NotSupportedException - This Sql generator does not support queries returning groupings that result from a GroupBy operator")]
    public void LinqToSqlGroupBy01 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select g;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Max to find the maximum unit price for each CategoryID.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy02 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            MaxPrice = g.Max (p => p.UnitPrice)
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Min to find the minimum unit price for each CategoryID.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy03 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            MinPrice = g.Min (p => p.UnitPrice)
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Average to find the average UnitPrice for each CategoryID.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy04 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            AveragePrice = g.Average (p => p.UnitPrice)
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Sum to find the total UnitPrice for each CategoryID.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy05 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            TotalPrice = g.Sum (p => p.UnitPrice)
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Count to find the number of Products in each CategoryID.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy06 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            NumProducts = g.Count ()
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses group by and Count to find the number of Products in each CategoryID that are discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy07 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            NumProducts = g.Count (p => p.Discontinued)
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a where clause after a group by clause to find all categories that have at least 10 products.
    /// </summary>
    [Test]
    public void LinqToSqlGroupBy08 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          where g.Count () >= 10
          select new
          {
            g.Key,
            ProductCount = g.Count ()
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Group By to group products by CategoryID and SupplierID.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - NotSupportedException - This Sql generator does not support queries returning groupings that result from a GroupBy operator")]
    public void LinqToSqlGroupBy09 ()
    {
      var categories =
          from p in DB.Products
          group p by new { p.CategoryID, p.SupplierID } into g
          select new { g.Key, g };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses Group By to return two sequences of products. The first sequence contains products with unit price greater than 10.
    /// The second sequence contains products with unit price less than or equal to 10.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - NotSupportedException - ArgumentException : The inner expression must be an expression of type Int32.Parameter name: expression")]
    public void LinqToSqlGroupBy10 ()
    {
      var categories =
          from p in DB.Products
          group p by new { Criterion = p.UnitPrice > 10 } into g
          select g;

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod ());
    }
  }
}
