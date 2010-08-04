using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupGroupByHaving:Executor
  {
    //This sample uses group by to partition Products by CategoryID.
    public void LinqToSqlGroupBy01 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select g;

      serializer.Serialize (q);
    }

    //This sample uses group by and Max to find the maximum unit price for each CategoryID.
    public void LinqToSqlGroupBy02 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            MaxPrice = g.Max (p => p.UnitPrice)
          };

      serializer.Serialize (q);
    }

    //This sample uses group by and Min to find the minimum unit price for each CategoryID.")]
    public void LinqToSqlGroupBy03 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            MinPrice = g.Min (p => p.UnitPrice)
          };

      serializer.Serialize (q);
    }

    //This sample uses group by and Average to find the average UnitPrice for each CategoryID.
    public void LinqToSqlGroupBy04 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            AveragePrice = g.Average (p => p.UnitPrice)
          };

      serializer.Serialize (q);
    }

    //This sample uses group by and Sum to find the total UnitPrice for each CategoryID.
    public void LinqToSqlGroupBy05 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            TotalPrice = g.Sum (p => p.UnitPrice)
          };

      serializer.Serialize (q);
    }

    //This sample uses group by and Count to find the number of Products in each CategoryID.")]
    public void LinqToSqlGroupBy06 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            NumProducts = g.Count ()
          };

      serializer.Serialize (q);
    }

    //This sample uses group by and Count to find the number of Products in each CategoryID that are discontinued.")]
    public void LinqToSqlGroupBy07 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            NumProducts = g.Count (p => p.Discontinued)
          };

      serializer.Serialize (q);
    }

    //This sample uses a where clause after a group by clause to find all categories that have at least 10 products.
    public void LinqToSqlGroupBy08 ()
    {
      var q =
          from p in db.Products
          group p by p.CategoryID into g
          where g.Count () >= 10
          select new
          {
            g.Key,
            ProductCount = g.Count ()
          };

      serializer.Serialize (q);
    }

    //This sample uses Group By to group products by CategoryID and SupplierID.")]
    public void LinqToSqlGroupBy09 ()
    {
      var categories =
          from p in db.Products
          group p by new { p.CategoryID, p.SupplierID } into g
          select new { g.Key, g };

      serializer.Serialize (categories);
    }

    //This sample uses Group By to return two sequences of products. The first sequence contains products with unit price 
    //greater than 10. The second sequence contains products with unit price less than or equal to 10.
    public void LinqToSqlGroupBy10 ()
    {
      var categories =
          from p in db.Products
          group p by new { Criterion = p.UnitPrice > 10 } into g
          select g;

      serializer.Serialize (categories);
    }
  }
}
