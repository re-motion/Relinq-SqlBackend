// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class RMLNQSQL_171 : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Test_WithBug ()
    {
      var kitchenList = new[] { new Company() { ID = 42 } };
      var kitchenListWithFilter = kitchenList.Where(g => true); //unevaluated sequence results in incorrect query
      
      CheckQuery (Restaurants.Where(fhc => kitchenListWithFilter.Contains(fhc.CompanyIfAny)),
          "SELECT [t0].[ID],[t0].[CompanyID] "
          + "FROM [RestaurantTable] AS [t0] "
          + "WHERE [t0].[CompanyID] IN (SELECT [t1].[ID] FROM [CompanyTable] AS [t1])");
    }

    [Test]
    public void Test_Correct ()
    {
      var kitchenList = new[] { 42 };
      var kitchenListWithFilter = kitchenList.Where(g => true).ToArray(); // evaluated sequence is handled correctly
      
      CheckQuery (Restaurants.Where(fhc => kitchenListWithFilter.Contains(fhc.CompanyIfAny.ID)),
          "SELECT [t0].[ID],[t0].[CompanyID] "
          + "FROM [RestaurantTable] AS [t0] "
          + "WHERE [t0].[CompanyID] IN (@1)",
          new CommandParameter("@1", 42));
    }

    [Test]
    public void Test_WithBug2 ()
    {
      var kitchenList = new[] { 2.2 };
      var kitchenListWithFilter = kitchenList.Where(g => true);
      
      CheckQuery (Knives.Where(fhc => kitchenListWithFilter.Contains(fhc.Sharpness)),
          "SELECT [t0].[ID],[t0].[ClassID],[t0].[Sharpness] "
          + "FROM [KnifeTable] AS [t0] "
          + "WHERE [t0].[Sharpness] IN (@1)",
          new CommandParameter("@1", 2.2));
    }
  }
}