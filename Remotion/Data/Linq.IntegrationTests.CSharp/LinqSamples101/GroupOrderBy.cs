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
using System.Linq;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupOrderBy:Executor
  {
    //This sample uses orderby to sort Employees by hire date.
    public void LinqToSqlOrderBy01 ()
    {
      var q =
          from e in db.Employees
          orderby e.HireDate
          select e;

      serializer.Serialize (q);
    }

    //This sample uses where and orderby to sort Orders shipped to London by freight.
    public void LinqToSqlOrderBy02 ()
    {
      var q =
          from o in db.Orders
          where o.ShipCity == "London"
          orderby o.Freight
          select o;

      serializer.Serialize (q);
    }

    //This sample uses orderby to sort Products by unit price from highest to lowest.
    public void LinqToSqlOrderBy03 ()
    {
      var q =
          from p in db.Products
          orderby p.UnitPrice descending
          select p;

      serializer.Serialize (q);
    }

    //This sample uses a compound orderby to sort Customers by city and then contact name.
    public void LinqToSqlOrderBy04 ()
    {
      var q =
          from c in db.Customers
          orderby c.City, c.ContactName
          select c;

      serializer.Serialize (q);
    }

    //This sample uses orderby to sort Orders from EmployeeID 1 by ship-to country, and then by freight from highest to lowest.
    public void LinqToSqlOrderBy05 ()
    {
      var q =
          from o in db.Orders
          where o.EmployeeID == 1
          orderby o.ShipCountry, o.Freight descending
          select o;

      serializer.Serialize (q);
    }


    //This sample uses orderby, Max and Group By to find the Products that have the highest unit price in 
    //each category, and sorts the group by category id.
    public void LinqToSqlOrderBy06 ()
    {
      var categories =
          from p in db.Products
          group p by p.CategoryID into g
          orderby g.Key
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
  }
}
