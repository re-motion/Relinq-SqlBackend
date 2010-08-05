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
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  internal class ObjectLoadingTests : TestBase
  {
    //01 uses trigger, not necessary to test
    //This sample demonstrates how navigating through relationships in 
    //retrieved objects can end up triggering new queries to the database 
    //if the data was not requested by the original query.
    //public void LinqToSqlObject01 ()
    //{
    //  var custs =
    //      from c in DB.Customers
    //      where c.City == "Sao Paulo"
    //      select c;

    //  TestExecutor.Execute (custs, MethodBase.GetCurrentMethod());

    //  //foreach (var cust in custs)
    //  //{
    //  //  foreach (var ord in cust.Orders)
    //  //    serializer.Serialize (String.Format ("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID));
    //  //}
    //}

    //TODO: won't compile since needs Northwind object, do we need this test ?
    ////This sample demonstrates how to use LoadWith to request related 
    ////data during the original query so that additional roundtrips to the 
    ////database are not required later when navigating through 
    ////the retrieved objects.
    //public void LinqToSqlObject02 ()
    //{
    //  Northwind db2 = new Northwind (connString);

    //  DataLoadOptions ds = new DataLoadOptions();
    //  ds.LoadWith<Customer> (p => p.Orders);

    //  db2.LoadOptions = ds;

    //  var custs = (
    //                  from c in db2.Customers
    //                  where c.City == "Sao Paulo"
    //                  select c);

    //  TestExecutor.Execute (custs, MethodBase.GetCurrentMethod ());

    //  //foreach (var cust in custs)
    //  //{
    //  //  foreach (var ord in cust.Orders)
    //  //    serializer.Serialize (String.Format ("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID));
    //  //}
    //}

    //TODO: won't compile since needs Northwind object, do we need this test ?
    //This sample demonstrates how navigating through relationships in 
    //retrieved objects can end up triggering new queries to the database 
    //if the data was not requested by the original query. Also this sample shows relationship 
    //objects can be filtered using Assoicate With when they are deferred loaded.
    //public void LinqToSqlObject03 ()
    //{
    //  Northwind db2 = new Northwind (connString);

    //  DataLoadOptions ds = new DataLoadOptions();
    //  ds.AssociateWith<Customer> (p => p.Orders.Where (o => o.ShipVia > 1));

    //  db2.LoadOptions = ds;
    //  var custs =
    //      from c in db2.Customers
    //      where c.City == "London"
    //      select c;

    //  foreach (var cust in custs)
    //  {
    //    foreach (var ord in cust.Orders)
    //    {
    //      foreach (var orderDetail in ord.OrderDetails)
    //      {
    //        serializer.Serialize (
    //            String.Format (
    //                "CustomerID {0} has an OrderID {1} that ShipVia is {2} with ProductID {3} that has name {4}.",
    //                cust.CustomerID,
    //                ord.OrderID,
    //                ord.ShipVia,
    //                orderDetail.ProductID,
    //                orderDetail.Product.ProductName));
    //      }
    //    }
    //  }
    //}


    //TODO: won't compile since needs Northwind object, do we need this test ?
    ////This sample demonstrates how to use LoadWith to request related 
    ////data during the original query so that additional roundtrips to the 
    ////database are not required later when navigating through 
    ////the retrieved objects. Also this sample shows relationship 
    ////objects can be ordered by using Assoicate With when they are eager loaded.")]
    //public void LinqToSqlObject04 ()
    //{
    //  Northwind db2 = new Northwind (connString);

    //  DataLoadOptions ds = new DataLoadOptions();
    //  ds.LoadWith<Customer> (p => p.Orders);
    //  ds.LoadWith<Order> (p => p.OrderDetails);
    //  ds.AssociateWith<Order> (p => p.OrderDetails.OrderBy (o => o.Quantity));

    //  db2.LoadOptions = ds;

    //  var custs = (
    //                  from c in db2.Customers
    //                  where c.City == "London"
    //                  select c);

    //  foreach (var cust in custs)
    //  {
    //    foreach (var ord in cust.Orders)
    //    {
    //      foreach (var orderDetail in ord.OrderDetails)
    //      {
    //        serializer.Serialize (
    //            string.Format (
    //                "CustomerID {0} has an OrderID {1} with ProductID {2} that has Quantity {3}.",
    //                cust.CustomerID,
    //                ord.OrderID,
    //                orderDetail.ProductID,
    //                orderDetail.Quantity));
    //      }
    //    }
    //  }
    //}

    //05 and 06 use Triggers - not necessary to test

    ////This sample demonstrates how navigating through relationships in 
    ////retrieved objects can result in triggering new queries to the database 
    ////if the data was not requested by the original query.")]
    //public void LinqToSqlObject05 ()
    //{
    //  var emps = from e in DB.Employees
    //             select e;

    //  TestExecutor.Execute (emps, MethodBase.GetCurrentMethod());
    //}

    ////This sample demonstrates how navigating through Link in 
    ////retrieved objects can end up triggering new queries to the database 
    ////if the data type is Link.")]
    //public void LinqToSqlObject06 ()
    //{
    //  var emps = from c in DB.Employees
    //             select c;

    //  TestExecutor.Execute (emps, MethodBase.GetCurrentMethod());
    //}


    //TODO: won't compile since needs Northwind object, do we need this test ?
    ////This samples overrides the partial method LoadProducts in Category class. When products of a category are being loaded,
    ////LoadProducts is being called to load products that are not discontinued in this category. ")]
    //public void LinqToSqlObject07 ()
    //{
    //  Northwind db2 = new Northwind (connString);

    //  DataLoadOptions ds = new DataLoadOptions();

    //  ds.LoadWith<Category> (p => p.Products);
    //  db2.LoadOptions = ds;

    //  var q = (
    //              from c in db2.Categories
    //              where c.CategoryID < 3
    //              select c);

    //  TestExecutor.Execute (q, MethodBase.GetCurrentMethod ());
    //}
  }
}