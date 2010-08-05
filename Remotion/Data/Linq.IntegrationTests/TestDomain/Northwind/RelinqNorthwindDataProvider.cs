using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  class RelinqNorthwindDataProvider : INorthwindDataProvider // TODO: implement
  {
    public MetaModel NorthwindMetaModel
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Product> Products
    {
      get { throw new NotImplementedException(); }
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

    public IQueryable<Order> Orders
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<OrderDetail> OrderDetails
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Contact> Contacts
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Invoices> Invoices
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<QuarterlyOrder> QuarterlyOrders
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Shipper> Shippers
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryable<Supplier> Suppliers
    {
      get { throw new NotImplementedException(); }
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

    public int CustomersCountByRegion (string wa)
    {
      throw new NotImplementedException();
    }

    public ISingleResult<CustomersByCityResult> CustomersByCity (string london)
    {
      throw new NotImplementedException();
    }

    public IMultipleResults WholeOrPartialCustomersSet (int p0)
    {
      throw new NotImplementedException();
    }

    public IMultipleResults GetCustomerAndOrders (string seves)
    {
      throw new NotImplementedException();
    }

    public void CustomerTotalSales (string customerID, ref decimal? totalSales)
    {
      throw new NotImplementedException();
    }
  }
}
