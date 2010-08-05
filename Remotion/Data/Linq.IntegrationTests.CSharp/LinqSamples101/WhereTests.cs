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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  public class WhereTests:TestBase
  {
    /// <summary>
    /// This sample uses WHERE to filter for Customers in London.
    /// </summary>
    [Test]
    public void LinqToSqlWhere01 ()
    {
      var q =
          from c in DB.Customers
          where c.City == "London"
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter for Employees hired during or after 1994.
    /// </summary>
    [Test]
    public void LinqToSqlWhere02 ()
    {
      var q =
          from e in DB.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter for Products that have stock below their reorder level and are not discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere03 ()
    {
      var q =
          from p in DB.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter out Products that are either UnitPrice is greater than 10 or is discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere04 ()
    {
      var q =
          from p in DB.Products
          where p.UnitPrice > 10m || p.Discontinued
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10 and is discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere05 ()
    {
      var q =
          DB.Products.Where (p => p.UnitPrice > 10m).Where (p => p.Discontinued);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses First to select the first Shipper in the table.
    /// </summary>
    [Test]
    public void LinqToSqlWhere06 ()
    {
      Shipper shipper = DB.Shippers.First ();
      TestExecutor.Execute (shipper, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses First to select the single Customer with CustomerID 'BONAP'.
    /// </summary>
    [Test]
    public void LinqToSqlWhere07 ()
    {
      Customer cust = DB.Customers.First (c => c.CustomerID == "BONAP");
      TestExecutor.Execute (cust, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    ///  This sample uses First to select an Order with freight greater than 10.00.
    /// </summary>
    [Test]
    public void LinqToSqlWhere08 ()
    {
      Order ord = DB.Orders.First (o => o.Freight > 10.00M);
      TestExecutor.Execute (ord, MethodBase.GetCurrentMethod ());
    }
  }
}
