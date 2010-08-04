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
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupConversionOperators:Executor
  {
    private bool isValidProduct (Product p)
    {
      return p.ProductName.LastIndexOf ('C') == 0;
    }


    //This sample uses AsEnumerable so that the client-side IEnumerable<T> 
    //implementation of Where is used, instead of the default IQueryable<T> 
    //implementation which would be converted to SQL and executed 
    //on the server.  This is necessary because the where clause 
    //references a user-defined client-side method, isValidProduct, 
    //which cannot be converted to SQL.
    public void LinqToSqlConversion01 ()
    {
      var q =
          from p in db.Products.AsEnumerable()
          where isValidProduct (p)
          select p;

      serializer.Serialize (q);
    }

    //This sample uses ToArray to immediately evaluate a query into an array 
    //and get the 3rd element.")]

    public void LinqToSqlConversion02 ()
    {
      var q =
          from c in db.Customers
          where c.City == "London"
          select c;

      Customer[] qArray = q.ToArray();
      serializer.Serialize (qArray[3]);
    }

    //This sample uses ToList to immediately evaluate a query into a List<T>.")]
    public void LinqToSqlConversion03 ()
    {
      var q =
          from e in db.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      List<Employee> qList = q.ToList();
      serializer.Serialize (qList);
    }

    //This sample uses ToDictionary to immediately evaluate a query and 
    //a key expression into an Dictionary<K, T>.")]

    public void LinqToSqlConversion04 ()
    {
      var q =
          from p in db.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;

      Dictionary<int, Product> qDictionary = q.ToDictionary (p => p.ProductID);

      foreach (int key in qDictionary.Keys)
      {
        serializer.Serialize (string.Format ("Key {0}:", key));
        serializer.Serialize (qDictionary[key]);
        serializer.Serialize (Environment.NewLine);
      }
    }
  }
}