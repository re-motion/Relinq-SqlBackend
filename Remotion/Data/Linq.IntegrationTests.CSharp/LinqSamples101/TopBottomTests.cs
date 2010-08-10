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
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class TopBottomTests : TestBase
  {
    /// <summary>
    /// This sample uses Take to select the first 5 Employees hired.
    /// </summary>
    [Test]
    public void LinqToSqlTop01 ()
    {
      var q = (
                  from e in DB.Employees
                  orderby e.HireDate
                  select e)
          .Take (5);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Skip to select all but the 10 most expensive Products.
    /// </summary>
    [Test]
    public void LinqToSqlTop02 ()
    {
      var q = (
                  from p in DB.Products
                  orderby p.UnitPrice descending
                  select p)
          .Skip (10);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}