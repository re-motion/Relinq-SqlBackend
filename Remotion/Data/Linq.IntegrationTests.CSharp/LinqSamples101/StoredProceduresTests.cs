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
using System.Data.Linq;
using System.Reflection;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class StoredProceduresTests:TestBase
  {
    //This sample uses a stored procedure to return the number of Customers in the 'WA' Region.")]
    public void LinqToSqlStoredProc01 ()
    {
      int count = DB.CustomersCountByRegion ("WA");

      TestExecutor.Execute (count, MethodBase.GetCurrentMethod());
    }

    //This sample uses a stored procedure to return the CustomerID, ContactName, CompanyName
    // and City of customers who are in London.")]
    public void LinqToSqlStoredProc02 ()
    {
      ISingleResult<CustomersByCityResult> result = DB.CustomersByCity ("London");

      TestExecutor.Execute (result, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a stored procedure to return a set of 
    //Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. 
    //If the parameter equals 1, all Customer properties are returned. 
    //If the parameter equals 2, the CustomerID, ContactName and CompanyName properties are returned.")]
    public void LinqToSqlStoredProc03_1 ()
    {
      IMultipleResults result = DB.WholeOrPartialCustomersSet (1);
      IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult> ();

      TestExecutor.Execute (shape1, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a stored procedure to return a set of 
    //Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. 
    //If the parameter equals 1, all Customer properties are returned. 
    //If the parameter equals 2, the CustomerID, ContactName and CompanyName properties are returned.")]
    public void LinqToSqlStoredProc03_2 ()
    {
      IMultipleResults result = DB.WholeOrPartialCustomersSet (2);
      IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult> ();

      TestExecutor.Execute (shape2, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a stored procedure to return the Customer 'SEVES' and all their Orders.")]
    public void LinqToSqlStoredProc04_1 ()
    {
      IMultipleResults result = DB.GetCustomerAndOrders ("SEVES");

      IEnumerable<CustomerResultSet> customer = result.GetResult<CustomerResultSet> ();
      TestExecutor.Execute (customer, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a stored procedure to return the Customer 'SEVES' and all their Orders.")]
    public void LinqToSqlStoredProc04_2 ()
    {
      IMultipleResults result = DB.GetCustomerAndOrders ("SEVES");

      IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet> ();
      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a stored procedure that returns an out parameter.")]
    public void LinqToSqlStoredProc05 ()
    {
      decimal? totalSales = 0;
      string customerID = "ALFKI";

      // Out parameters are passed by ref, to support scenarios where
      // the parameter is 'in/out'.  In this case, the parameter is only
      // 'out'.
      DB.CustomerTotalSales (customerID, ref totalSales);

      TestExecutor.Execute (totalSales, MethodBase.GetCurrentMethod ());
    }
  }
}