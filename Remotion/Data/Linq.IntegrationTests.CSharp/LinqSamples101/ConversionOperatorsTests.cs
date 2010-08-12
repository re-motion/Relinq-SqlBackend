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
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class ConversionOperatorsTests:TestBase
  {
    private bool isValidProduct (Product p)
    {
      return p.ProductName.LastIndexOf ('C') == 0;
    }


    ///<summary>
    ///This sample uses AsEnumerable so that the client-side IEnumerable<T> implementation of Where is used, 
    ///instead of the default IQueryable<T> implementation which would be converted to SQL and executed on the server.
    ///This is necessary because the where clause references a user-defined client-side method, isValidProduct, 
    ///which cannot be converted to SQL.
    ///</summary>
    [Test]
    public void LinqToSqlConversion01 ()
    {
      var q =
          from p in DB.Products.AsEnumerable()
          where isValidProduct (p)
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    ///<summary>
    ///This sample uses ToArray to immediately evaluate a query into an array and get the 3rd element.
    ///</summary>
    [Test]
    public void LinqToSqlConversion02 ()
    {
      var q =
          from c in DB.Customers
          where c.City == "London"
          select c;

      Customer[] qArray = q.ToArray();
      TestExecutor.Execute (qArray[3], MethodBase.GetCurrentMethod());
    }

    ///<summary>
    ///This sample uses ToList to immediately evaluate a query into a List<T>.
    ///</summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - InvalidCastException - Unable to cast System.Byte[] to System.Data.Linq.Binary")]
    public void LinqToSqlConversion03 ()
    {
      var q =
          from e in DB.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      List<Employee> qList = q.ToList();
      TestExecutor.Execute (qList, MethodBase.GetCurrentMethod ());
    }

    ///<summary>
    ///This sample uses ToDictionary to immediately evaluate a query and a key expression into an Dictionary<K, T>.
    ///</summary>
    [Test]
    public void LinqToSqlConversion04 ()
    {
      var q =
          from p in DB.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;


      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}