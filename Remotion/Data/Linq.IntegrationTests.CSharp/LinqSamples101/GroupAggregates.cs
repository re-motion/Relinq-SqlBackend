using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  /// <summary>
  ///  für die COUNT/SUM/MIN/MAX/AVG
  /// </summary>
  class GroupAggregates:Executor
  {
    //[Title ("Count - Simple")]
    //[Description ("This sample uses Count to find the number of Customers in the database.")]
    public void LinqToSqlCount01 ()
    {
      var q = db.Customers.Count ();
      serializer.Serialize (q);
    }


    //[Title ("Count - Conditional")]
    //[Description ("This sample uses Count to find the number of Products in the database " +
    //             "that are not discontinued.")]
    public void LinqToSqlCount02 ()
    {
      var q = db.Products.Count (p => !p.Discontinued);
      serializer.Serialize (q);
    }

    
    //[Title ("Sum - Simple")]
    //[Description ("This sample uses Sum to find the total freight over all Orders.")]
    public void LinqToSqlCount03 ()
    {
      var q = db.Orders.Select (o => o.Freight).Sum ();
      serializer.Serialize (q);
    }

    
    //[Title ("Sum - Mapped")]
    //[Description ("This sample uses Sum to find the total number of units on order over all Products.")]
    public void LinqToSqlCount04 ()
    {
      var q = db.Products.Sum (p => p.UnitsOnOrder);
      serializer.Serialize (q);
    }

    
    //[Title ("Min - Simple")]
    //[Description ("This sample uses Min to find the lowest unit price of any Product.")]
    public void LinqToSqlCount05 ()
    {
      var q = db.Products.Select (p => p.UnitPrice).Min ();
      serializer.Serialize (q);
    }

    
    //[Title ("Min - Mapped")]
    //[Description ("This sample uses Min to find the lowest freight of any Order.")]
    public void LinqToSqlCount06 ()
    {
      var q = db.Orders.Min (o => o.Freight);
      serializer.Serialize (q);
    }

    
    //[Title ("Min - Elements")]
    //[Description ("This sample uses Min to find the Products that have the lowest unit price " +
    //             "in each category.")]
    public void LinqToSqlCount07 ()
    {
      var categories =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            CategoryID = g.Key,
            CheapestProducts =
                from p2 in g
                where p2.UnitPrice == g.Min (p3 => p3.UnitPrice)
                select p2
          };

      serializer.Serialize (categories);
    }


    //[Title ("Max - Simple")]
    //[Description ("This sample uses Max to find the latest hire date of any Employee.")]
    public void LinqToSqlCount08 ()
    {
      var q = db.Employees.Select (e => e.HireDate).Max ();
      serializer.Serialize (q);
    }

    
    //[Title ("Max - Mapped")]
    //[Description ("This sample uses Max to find the most units in stock of any Product.")]
    public void LinqToSqlCount09 ()
    {
      var q = db.Products.Max (p => p.UnitsInStock);
      serializer.Serialize (q);
    }

    
    //[Title ("Max - Elements")]
    //[Description ("This sample uses Max to find the Products that have the highest unit price " +
    //             "in each category.")]
    public void LinqToSqlCount10 ()
    {
      var categories =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            MostExpensiveProducts =
                from p2 in g
                where p2.UnitPrice == g.Max (p3 => p3.UnitPrice)
                select p2
          };

      serializer.Serialize (categories);
    }

    
    //[Title ("Average - Simple")]
    //[Description ("This sample uses Average to find the average freight of all Orders.")]
    public void LinqToSqlCount11 ()
    {
      var q = db.Orders.Select (o => o.Freight).Average ();
      serializer.Serialize (q);
    }

    
    //[Title ("Average - Mapped")]
    //[Description ("This sample uses Average to find the average unit price of all Products.")]
    public void LinqToSqlCount12 ()
    {
      var q = db.Products.Average (p => p.UnitPrice);
      serializer.Serialize (q);
    }

    
    //[Title ("Average - Elements")]
    //[Description ("This sample uses Average to find the Products that have unit price higher than " +
    //             "the average unit price of the category for each category.")]
    public void LinqToSqlCount13 ()
    {
      var categories =
          from p in db.Products
          group p by p.CategoryID into g
          select new
          {
            g.Key,
            ExpensiveProducts =
                from p2 in g
                where p2.UnitPrice > g.Average (p3 => p3.UnitPrice)
                select p2
          };

      serializer.Serialize (categories);
    }
  }
}
