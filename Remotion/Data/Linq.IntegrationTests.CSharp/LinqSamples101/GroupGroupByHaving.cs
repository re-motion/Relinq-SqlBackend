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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupGroupByHaving:TestBase
  {
    //This sample uses group by to partition Products by CategoryID.
    public void LinqToSqlGroupBy01 ()
    {
      var q =
          from p in DB.Products
          group p by p.CategoryID into g
          select g;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses group by and Max to find the maximum unit price for each CategoryID.
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

    //This sample uses group by and Min to find the minimum unit price for each CategoryID.")]
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

    //This sample uses group by and Average to find the average UnitPrice for each CategoryID.
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

    //This sample uses group by and Sum to find the total UnitPrice for each CategoryID.
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

    //This sample uses group by and Count to find the number of Products in each CategoryID.")]
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

    //This sample uses group by and Count to find the number of Products in each CategoryID that are discontinued.")]
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

    //This sample uses a where clause after a group by clause to find all categories that have at least 10 products.
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

    //This sample uses Group By to group products by CategoryID and SupplierID.")]
    public void LinqToSqlGroupBy09 ()
    {
      var categories =
          from p in DB.Products
          group p by new { p.CategoryID, p.SupplierID } into g
          select new { g.Key, g };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod ());
    }

    //This sample uses Group By to return two sequences of products. The first sequence contains products with unit price 
    //greater than 10. The second sequence contains products with unit price less than or equal to 10.
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
