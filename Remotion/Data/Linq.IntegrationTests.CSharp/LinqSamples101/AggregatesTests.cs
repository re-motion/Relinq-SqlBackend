// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
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
    [Ignore ("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")]
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
    [Ignore ("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")]
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
    [Ignore ("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")]
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