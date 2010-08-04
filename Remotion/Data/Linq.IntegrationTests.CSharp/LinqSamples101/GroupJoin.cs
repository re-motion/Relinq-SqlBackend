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
  class GroupJoin:Executor
  {
    // This sample uses foreign key navigation in the " +
    //             "from clause to select all orders for customers in London.")]
    public void LinqToSqlJoin01 ()
    {
      var q =
          from c in db.Customers
          from o in c.Orders
          where c.City == "London"
          select o;

      serializer.Serialize (q);
    }

    // This sample uses foreign key navigation in the " +
    //             "where clause to filter for Products whose Supplier is in the USA " +
    //             "that are out of stock.")]
    public void LinqToSqlJoin02 ()
    {
      var q =
          from p in db.Products
          where p.Supplier.Country == "USA" && p.UnitsInStock == 0
          select p;

      serializer.Serialize (q);
    }

    // This sample uses foreign key navigation in the " +
    //             "from clause to filter for employees in Seattle, " +
    //             "and also list their territories.")]
    public void LinqToSqlJoin03 ()
    {
      var q =
          from e in db.Employees
          from et in e.EmployeeTerritories
          where e.City == "Seattle"
          select new { e.FirstName, e.LastName, et.Territory.TerritoryDescription };

      serializer.Serialize (q);
    }

    // This sample uses foreign key navigation in the " +
    //             "select clause to filter for pairs of employees where " +
    //             "one employee reports to the other and where " +
    //             "both employees are from the same City.")]
    public void LinqToSqlJoin04 ()
    {
      var q =
          from e1 in db.Employees
          from e2 in e1.Employees
          where e1.City == e2.City
          select new
          {
            FirstName1 = e1.FirstName,
            LastName1 = e1.LastName,
            FirstName2 = e2.FirstName,
            LastName2 = e2.LastName,
            e1.City
          };

      serializer.Serialize (q);
    }

    // This sample explicitly joins two tables and projects results from both tables.")]
    public void LinqToSqlJoin05 ()
    {
      var q =
          from c in db.Customers
          join o in db.Orders on c.CustomerID equals o.CustomerID into orders
          select new { c.ContactName, OrderCount = orders.Count () };

      serializer.Serialize (q);
    }

    // This sample explicitly joins three tables and projects results from each of them.")]
    public void LinqToSqlJoin06 ()
    {
      var q =
          from c in db.Customers
          join o in db.Orders on c.CustomerID equals o.CustomerID into ords
          join e in db.Employees on c.City equals e.City into emps
          select new { c.ContactName, ords = ords.Count (), emps = emps.Count () };

      serializer.Serialize (q);
    }

    // This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty().
    //The DefaultIfEmpty() method returns null when there is no Order for the Employee.")]
    public void LinqToSqlJoin07 ()
    {
      var q =
          from e in db.Employees
          join o in db.Orders on e equals o.Employee into ords
          from o in ords.DefaultIfEmpty ()
          select new { e.FirstName, e.LastName, Order = o };

      serializer.Serialize (q);
    }

    // This sample projects a 'let' expression resulting from a join.")]
    public void LinqToSqlJoin08 ()
    {
      var q =
          from c in db.Customers
          join o in db.Orders on c.CustomerID equals o.CustomerID into ords
          let z = c.City + c.Country
          from o in ords
          select new { c.ContactName, o.OrderID, z };

      serializer.Serialize (q);
    }

    // This sample shows a join with a composite key.")]
    public void LinqToSqlJoin09 ()
    {
      var q =
          from o in db.Orders
          from p in db.Products
          join d in db.OrderDetails
              on new { o.OrderID, p.ProductID } equals new { d.OrderID, d.ProductID }
              into details
          from d in details
          select new { o.OrderID, p.ProductID, d.UnitPrice };

      serializer.Serialize (q);
    }

    // This sample shows how to construct a join where one side is nullable and the other is not.")]
    public void LinqToSqlJoin10 ()
    {
      var q =
          from o in db.Orders
          join e in db.Employees
              on o.EmployeeID equals (int?) e.EmployeeID into emps
          from e in emps
          select new { o.OrderID, e.FirstName };

      serializer.Serialize (q);
    }

  }
}
