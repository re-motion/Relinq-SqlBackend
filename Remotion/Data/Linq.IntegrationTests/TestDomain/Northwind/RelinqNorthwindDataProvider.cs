using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  class RelinqNorthwindDataProvider : INorthwindDataProvider // TODO: implement
  {
    public IQueryable<Product> Products
    {
      get { throw new NotImplementedException (); }
    }

    public IQueryable<Customer> Customers
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Employee> Employees
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Category> Categories
    {
      get { throw new NotImplementedException(); }
    }

    public EntitySet<Order> Orders
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public DbCommand GetCommand (IQueryable<string> query)
    {
      throw new NotImplementedException();
    }

    public decimal? TotalProductUnitPriceByCategory (int categoryID)
    {
      throw new NotImplementedException();
    }

    public decimal? MinUnitPriceByCategory (int? nullable)
    {
      throw new NotImplementedException();
    }

    public IQueryable<ProductsUnderThisUnitPriceResult> ProductsUnderThisUnitPrice (decimal @decimal)
    {
      throw new NotImplementedException();
    }
  }
}
