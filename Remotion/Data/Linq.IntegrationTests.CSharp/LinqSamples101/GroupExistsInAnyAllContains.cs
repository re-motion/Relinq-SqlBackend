using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupExistsInAnyAllContains:Executor
  {

    //This sample uses Any to return only Customers that have no Orders.")]
    public void LinqToSqlExists01 ()
    {
      var q =
          from c in db.Customers
          where !c.Orders.Any ()
          select c;

      serializer.Serialize (q);
    }

    //This sample uses Any to return only Categories that have at least one Discontinued product.")]
    public void LinqToSqlExists02 ()
    {
      var q =
          from c in db.Categories
          where c.Products.Any (p => p.Discontinued)
          select c;

      serializer.Serialize (q);
    }

    //This sample uses All to return Customers whom all of their orders have been shipped to their own city or whom have no orders.")]
    public void LinqToSqlExists03 ()
    {
      var q =
          from c in db.Customers
          where c.Orders.All (o => o.ShipCity == c.City)
          select c;

      serializer.Serialize (q);
    }

    //This sample uses Contain to find which Customer contains an order with OrderID 10248.")]
    public void LinqToSqlExists04 ()
    {
      var order = (from o in db.Orders
                   where o.OrderID == 10248
                   select o).First ();

      var q = db.Customers.Where (p => p.Orders.Contains (order)).ToList ();

      foreach (var cust in q)
      {
        foreach (var ord in cust.Orders)
        {
          serializer.Serialize (String.Format ("Customer {0} has OrderID {1}.", cust.CustomerID, ord.OrderID));
        }
      }
    }

    //This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.")]
    public void LinqToSqlExists05 ()
    {
      string[] cities = new string[] { "Seattle", "London", "Vancouver", "Paris" };
      var q = db.Customers.Where (p => cities.Contains (p.City)).ToList ();

      serializer.Serialize (q);
    }
  }
}
