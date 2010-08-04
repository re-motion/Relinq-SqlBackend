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
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupStoredProcedures:Executor
  {
    //This sample uses a stored procedure to return the number of Customers in the 'WA' Region.")]
    public void LinqToSqlStoredProc01 ()
    {
      int count = db.CustomersCountByRegion ("WA");

      serializer.Serialize (count);
    }

    //This sample uses a stored procedure to return the CustomerID, ContactName, CompanyName
    // and City of customers who are in London.")]
    public void LinqToSqlStoredProc02 ()
    {
      ISingleResult<CustomersByCityResult> result = db.CustomersByCity ("London");

      serializer.Serialize (result);
    }

    //This sample uses a stored procedure to return a set of 
    //Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. 
    //If the parameter equals 1, all Customer properties are returned. 
    //If the parameter equals 2, the CustomerID, ContactName and CompanyName properties are returned.")]
    public void LinqToSqlStoredProc03 ()
    {
      serializer.Serialize ("********** Whole Customer Result-set ***********");
      IMultipleResults result = db.WholeOrPartialCustomersSet (1);
      IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult> ();

      serializer.Serialize (shape1);

      serializer.Serialize (Environment.NewLine);
      serializer.Serialize ("********** Partial Customer Result-set ***********");
      result = db.WholeOrPartialCustomersSet (2);
      IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult> ();

      serializer.Serialize (shape2);
    }

    //This sample uses a stored procedure to return the Customer 'SEVES' and all their Orders.")]
    public void LinqToSqlStoredProc04 ()
    {
      IMultipleResults result = db.GetCustomerAndOrders ("SEVES");

      serializer.Serialize ("********** Customer Result-set ***********");
      IEnumerable<CustomerResultSet> customer = result.GetResult<CustomerResultSet> ();
      serializer.Serialize (customer);
      serializer.Serialize (Environment.NewLine);

      serializer.Serialize ("********** Orders Result-set ***********");
      IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet> ();
      serializer.Serialize (orders);
    }

    //This sample uses a stored procedure that returns an out parameter.")]
    public void LinqToSqlStoredProc05 ()
    {
      decimal? totalSales = 0;
      string customerID = "ALFKI";

      // Out parameters are passed by ref, to support scenarios where
      // the parameter is 'in/out'.  In this case, the parameter is only
      // 'out'.
      db.CustomerTotalSales (customerID, ref totalSales);

      serializer.Serialize (string.Format ("Total Sales for Customer '{0}' = {1:C}", customerID, totalSales));
    }
  }
}