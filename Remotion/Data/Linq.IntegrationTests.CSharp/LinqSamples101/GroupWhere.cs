using System;
using System.IO;
using System.Linq;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupWhere:Executor
  {
    //This sample uses WHERE to filter for Customers in London.

    public void LinqToSqlWhere01 ()
    {
      var q =
          from c in db.Customers
          where c.City == "London"
          select c;
      serializer.Serialize (q);
    }

    //This sample uses WHERE to filter for Employees hired during or after 1994.
    public void LinqToSqlWhere02 ()
    {
      var q =
          from e in db.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      serializer.Serialize (q);
    }

    //This sample uses WHERE to filter for Products that have stock below their reorder level and are not discontinued.
    public void LinqToSqlWhere03 ()
    {
      var q =
          from p in db.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;

      serializer.Serialize (q);
    }

    //This sample uses WHERE to filter out Products that are either UnitPrice is greater than 10 or is discontinued.
    public void LinqToSqlWhere04 ()
    {
      var q =
          from p in db.Products
          where p.UnitPrice > 10m || p.Discontinued
          select p;

      serializer.Serialize (q);
    }

    //This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10 and is discontinued.
    public void LinqToSqlWhere05 ()
    {
      var q =
          db.Products.Where (p => p.UnitPrice > 10m).Where (p => p.Discontinued);

      serializer.Serialize (q);
    }

    //This sample uses First to select the first Shipper in the table.
    public void LinqToSqlWhere06 ()
    {
      Shipper shipper = db.Shippers.First ();
      serializer.Serialize (shipper);
    }

    //This sample uses First to select the single Customer with CustomerID 'BONAP'.
    public void LinqToSqlWhere07 ()
    {
      Customer cust = db.Customers.First (c => c.CustomerID == "BONAP");
      serializer.Serialize (cust);
    }

    // This sample uses First to select an Order with freight greater than 10.00.
    public void LinqToSqlWhere08 ()
    {
      Order ord = db.Orders.First (o => o.Freight > 10.00M);
      serializer.Serialize (ord);
    }
  }
}
