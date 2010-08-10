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
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class OrderByTests:TestBase
  {
    /// <summary>
    /// This sample uses orderby to sort Employees by hire date.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy01 ()
    {
      var q =
          from e in DB.Employees
          orderby e.HireDate
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses where and orderby to sort Orders shipped to London by freight.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy02 ()
    {
      var q =
          from o in DB.Orders
          where o.ShipCity == "London"
          orderby o.Freight
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Products by unit price from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy03 ()
    {
      var q =
          from p in DB.Products
          orderby p.UnitPrice descending
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a compound orderby to sort Customers by city and then contact name.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy04 ()
    {
      var q =
          from c in DB.Customers
          orderby c.City, c.ContactName
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Orders from EmployeeID 1 by ship-to country, and then by freight from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy05 ()
    {
      var q =
          from o in DB.Orders
          where o.EmployeeID == 1
          orderby o.ShipCountry, o.Freight descending
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }


    /// <summary>
    /// This sample uses orderby, Max and Group By to find the Products that have the highest 
    /// unit price in each category, and sorts the group by category id.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy06 ()
    {
      var categories =
          from p in DB.Products
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

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod());
    }
  }
}
