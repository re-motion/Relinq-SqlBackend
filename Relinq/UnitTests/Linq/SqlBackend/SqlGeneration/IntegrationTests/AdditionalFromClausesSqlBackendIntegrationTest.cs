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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class AdditionalFromClausesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleAdditionalFromClause_TwoTables ()
    {
      CheckQuery (
          from s in Cooks from k in Kitchens select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1]");
    }

    [Test]
    public void SelectMany_WithoutResultSelector ()
    {
      CheckQuery (
          Cooks.SelectMany (c => Kitchens).Select (k => k.ID),
          "SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1]");
    }

    [Test]
    public void SimpleAdditionalFromClause_ThreeTables ()
    {
      CheckQuery (
          from s in Cooks from k in Kitchens from r in Restaurants select k.Name,
          "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1] CROSS JOIN [RestaurantTable] AS [t2]");
    }

    [Test]
    public void SimpleAdditionalFromClause_WithJoins ()
    {
      CheckQuery (
          from s in Cooks from k in Kitchens where s.Substitution.Name == "Hugo" select k.Cook.FirstName,
          "SELECT [t2].[FirstName] AS [value] FROM [CookTable] AS [t0] " +
          "LEFT OUTER JOIN [CookTable] AS [t3] ON [t0].[ID] = [t3].[SubstitutedID] " +
          "CROSS JOIN [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[KitchenID] WHERE ([t3].[Name] = @1)",
          new CommandParameter ("@1", "Hugo")
          );
    }

    [Test]
    public void AdditionalFromClause_WithMemberAccess ()
    {
      CheckQuery (
          from s in Cooks from a in s.Assistants select a.Name,
          "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "CROSS JOIN [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID])"
          );
    }

    [Test]
    public void AdditionalFromClause_WithMemberAccess_AndCrossJoin ()
    {
      CheckQuery (
          from s in Cooks from a in s.Assistants from r in Restaurants from c in r.Cooks where a.Name != null select c.Name,
          "SELECT [t3].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "CROSS JOIN [CookTable] AS [t1] "
          + "CROSS JOIN [RestaurantTable] AS [t2] "
          + "CROSS JOIN [CookTable] AS [t3] "
          + "WHERE "
          + "((([t0].[ID] = [t1].[AssistedID]) AND "
          + "([t2].[ID] = [t3].[RestaurantID])) AND "
          + "([t1].[Name] IS NOT NULL))");
    }

    [Test]
    [Ignore ("TODO 3021")]
    public void AdditionalFromClause_WithNestedItemsFrom ()
    {
      CheckQuery (
        from c in Cooks
        let nested = new { Source = Cooks }
        from y in nested.Source
        select y.ID,
        "?");
    }
  }
}