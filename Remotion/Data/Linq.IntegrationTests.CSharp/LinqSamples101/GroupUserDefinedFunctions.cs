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
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupUserDefinedFunctions:Executor
  {
    //This sample demonstrates using a scalar user-defined function in a projection.")]
    public void LinqToSqlUserDefined01 ()
    {
      var q = from c in db.Categories
              select new { c.CategoryID, TotalUnitPrice = db.TotalProductUnitPriceByCategory (c.CategoryID) };

      serializer.Serialize (q);
    }

    //This sample demonstrates using a scalar user-defined function in a where clause.")]
    public void LinqToSqlUserDefined02 ()
    {
      var q = from p in db.Products
              where p.UnitPrice == db.MinUnitPriceByCategory (p.CategoryID)
              select p;

      serializer.Serialize (q);
    }

    //This sample demonstrates selecting from a table-valued user-defined function.")]
    public void LinqToSqlUserDefined03 ()
    {
      var q = from p in db.ProductsUnderThisUnitPrice (10.25M)
              where !(p.Discontinued ?? false)
              select p;

      serializer.Serialize (q);
    }

    //This sample demonstrates joining to the results of a table-valued user-defined function.")]
    public void LinqToSqlUserDefined04 ()
    {
      var q = from c in db.Categories
              join p in db.ProductsUnderThisUnitPrice (8.50M) on c.CategoryID equals p.CategoryID into prods
              from p in prods
              select new { c.CategoryID, c.CategoryName, p.ProductName, p.UnitPrice };

      serializer.Serialize (q);
    }
  }
}