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
using System.Reflection;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupView:TestBase
  {
    //This sample uses SELECT and WHERE to return a sequence of invoices
    //where shipping city is London.")]
    public void LinqToSqlView01 ()
    {
      var q =
          from i in DB.Invoices
          where i.ShipCity == "London"
          select new { i.OrderID, i.ProductName, i.Quantity, i.CustomerName };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses SELECT to query QuarterlyOrders.")]
    public void LinqToSqlView02 ()
    {
      var q =
          from qo in DB.QuarterlyOrders
          select qo;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}