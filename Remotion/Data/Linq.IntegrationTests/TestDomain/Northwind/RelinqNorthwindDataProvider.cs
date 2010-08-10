using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  public class RelinqNorthwindDataProvider : INorthwindDataProvider
  {
    private readonly IConnectionManager manager;
    private readonly NorthwindMappingResolver resolver;
    private readonly IQueryResultRetriever retriever;
    private readonly IQueryExecutor executor;

    public RelinqNorthwindDataProvider ()
    {
      manager = new NorthwindConnectionManager ();
      resolver = new NorthwindMappingResolver (new AttributeMappingSource().GetModel (typeof (Northwind)));
      retriever = new QueryResultRetriever (manager, resolver);
      executor = new RelinqQueryExecutor (retriever, resolver);
    }

    public IQueryable<Product> Products
    {
      get { return CreateQueryable<Product>(); }
    }

    public IQueryable<Customer> Customers
    {
      get { return CreateQueryable<Customer> (); }
    }

    public IQueryable<Employee> Employees
    {
      get { return CreateQueryable<Employee> (); }
    }

    public IQueryable<Category> Categories
    {
      get { return CreateQueryable<Category> (); }
    }

    public IQueryable<Order> Orders
    {
      get { return CreateQueryable<Order> (); }
    }

    public IQueryable<OrderDetail> OrderDetails
    {
      get { return CreateQueryable<OrderDetail> (); }
    }

    public IQueryable<Contact> Contacts
    {
      get { return CreateQueryable<Contact> (); }
    }

    public IQueryable<Invoices> Invoices
    {
      get { return CreateQueryable<Invoices> (); }
    }

    public IQueryable<QuarterlyOrder> QuarterlyOrders
    {
      get { return CreateQueryable<QuarterlyOrder> (); }
    }

    public IQueryable<Shipper> Shippers
    {
      get { return CreateQueryable<Shipper> (); }
    }

    public IQueryable<Supplier> Suppliers
    {
      get { return CreateQueryable<Supplier> (); }
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

    #region private methods

    private IQueryable<T> CreateQueryable<T> ()
    {
      return new QueryableAdapter<T> (executor);
    }

    #endregion
  }
}
