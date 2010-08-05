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
using System.Data.Common;
using System.Data.Linq;
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  internal class LinqToSqlNorthwindDataProvider : INorthwindDataProvider
  {
    //TODO edit connection
    private readonly Northwind _dataContext = new Northwind ("...");

    // TODO: ctor
    // TODO: implement additional properties - see INorthwindDataProvider
    public IQueryable<Product> Products
    {
      get { return _dataContext.Products; }
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

    IQueryable<Order> INorthwindDataProvider.Orders
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<OrderDetail> OrderDetails
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<Contact> Contacts
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<Invoices> Invoices
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<QuarterlyOrder> QuarterlyOrders
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<Shipper> Shippers
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IQueryable<Supplier> Suppliers
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
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