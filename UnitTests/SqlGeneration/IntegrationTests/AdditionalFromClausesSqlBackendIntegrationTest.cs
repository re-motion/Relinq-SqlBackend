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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
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
          "LEFT OUTER JOIN [CookTable] AS [t3] ON ([t0].[ID] = [t3].[SubstitutedID]) " +
          "CROSS JOIN [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[KitchenID]) WHERE ([t3].[Name] = @1)",
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
    public void AdditionalFromClause_WithMemberAccess_WithMultipleHops ()
    {
      CheckQuery (
          from k in Kitchens where k.Restaurant.SubKitchen.Restaurant.CompanyIfAny == null
          from c in Cooks where c.Kitchen.Restaurant.CompanyIfAny != null
          select new { KitchenID = k.ID, CookID = c.ID },
          "SELECT [t0].[ID] AS [KitchenID],[t1].[ID] AS [CookID] "
          + "FROM [KitchenTable] AS [t0] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t3] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t4] "
          + "ON ([t3].[RestaurantID] = [t4].[ID]) "
          + "ON ([t2].[ID] = [t3].[RestaurantID]) "
          + "ON ([t0].[RestaurantID] = [t2].[ID]) "
          + "CROSS JOIN [CookTable] AS [t1] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t5] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t6] "
          + "ON ([t5].[RestaurantID] = [t6].[ID]) "
          + "ON ([t1].[KitchenID] = [t5].[ID]) "
          + "WHERE (([t4].[CompanyID] IS NULL) AND ([t6].[CompanyID] IS NOT NULL))");
    }

    [Test]
    public void MemberAccess_OnSubQuery_WithEntities ()
    {
      var query = Cooks.Select (c => (from a in c.Assistants select a.Substitution).First ().Name);
      CheckQuery (query,
        "SELECT [q3].[value] AS [value] FROM [CookTable] AS [t0] CROSS APPLY (SELECT TOP (1) " +
        "[t2].[Name] AS [value] FROM [CookTable] AS [t1] " +
        "LEFT OUTER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[SubstitutedID]) WHERE ([t0].[ID] = [t1].[AssistedID])) AS [q3]");
    }

    [Test]
    public void AdditionalFromClause_WithMemberAccess_WithWhereClause ()
    {
      CheckQuery (
          from r in Restaurants
          from c in r.Cooks.Where (c => c.IsFullTimeCook)
          select new { RestaurantID = r.ID, CookID = c.ID, CookName = c.Name },
          "SELECT [t1].[ID] AS [RestaurantID],[q0].[ID] AS [CookID],[q0].[Name] AS [CookName] "
          + "FROM [RestaurantTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID],[t2].[KnifeID],[t2].[KnifeClassID],[t2].[CookRating] "
          + "FROM [CookTable] AS [t2] WHERE (([t1].[ID] = [t2].[RestaurantID]) AND ([t2].[IsFullTimeCook] = 1))"
          + ") AS [q0]");
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
    public void AdditionalFromClause_AsJoin ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen)
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM [KitchenTable] AS [t2] WHERE ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
    }

    [Test]
    public void AdditionalFromClause_WithMultipleFromClauses_AsJoin ()
    {
      CheckQuery (
          from c in Cooks
          from kitchen in Kitchens.Where (k => k == c.Kitchen)
          from knife in Knives.Where (k => k == c.Knife)
          select new { CookID = c.ID, KitchenID = kitchen.ID, KnifeID = knife.ID },
          "SELECT [t2].[ID] AS [CookID],[q0].[ID] AS [KitchenID],[q1].[ID] AS [KnifeID_Value],[q1].[ClassID] AS [KnifeID_ClassID] "
          + "FROM [CookTable] AS [t2] "
          + "CROSS APPLY ("
          + "SELECT [t3].[ID],[t3].[Name],[t3].[RestaurantID],[t3].[LastCleaningDay],[t3].[PassedLastInspection],[t3].[LastInspectionScore] "
          + "FROM [KitchenTable] AS [t3] WHERE ([t3].[ID] = [t2].[KitchenID])"
          + ") AS [q0] "
          + "CROSS APPLY ("
          + "SELECT [t4].[ID],[t4].[ClassID],[t4].[Sharpness] "
          + "FROM [KnifeTable] AS [t4] WHERE (([t4].[ID] = [t2].[KnifeID]) AND ([t4].[ClassID] = [t2].[KnifeClassID]))"
          + ") AS [q1]");
    }

    [Test]
    public void AdditionalFromClause_AsJoin_WithImplicitLeftJoinAddedByLeftJoinCondition ()
    {
      CheckQuery (
          from c in Cooks
          from restaurant in Restaurants.Where (r => r == c.Kitchen.Restaurant)
          select new { CookID = c.ID, RestaurantID = restaurant.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [RestaurantID] "
          + "FROM [CookTable] AS [t1] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t3] ON ([t1].[KitchenID] = [t3].[ID]) "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[CompanyID] "
          + "FROM [RestaurantTable] AS [t2] WHERE ([t2].[ID] = [t3].[RestaurantID])"
          + ") AS [q0]");
    }

    [Test]
    public void AdditionalFromClause_WithImplicitLeftJoinAddedByFromClause ()
    {
      CheckQuery (
          from k in Kitchens
          from c in k.Restaurant.Cooks
          where c.IsFullTimeCook
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t2].[ID] AS [CookID],[t0].[ID] AS [KitchenID] "
          + "FROM [KitchenTable] AS [t0] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t1] ON ([t0].[RestaurantID] = [t1].[ID]) "
          + "CROSS JOIN [CookTable] AS [t2] "
          + "WHERE (([t1].[ID] = [t2].[RestaurantID]) AND ([t2].[IsFullTimeCook] = 1))");
    }

    [Test]
    public void AdditionalFromClause_AsJoin_AndOuterWhereClause ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen)
          where k.Name != null
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM [KitchenTable] AS [t2] WHERE ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0] "
          + "WHERE ([q0].[Name] IS NOT NULL)");
    }

    [Test]
    public void AdditionalFromClause_AsJoin_WithNewExpressionInProjection ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen).Select (k => new { k.ID, k.Name })
          select new { CookID = c.ID, KitchenID = k.ID, KitchenName = k.Name },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID],[q0].[Name] AS [KitchenName] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID] AS [ID],[t2].[Name] AS [Name] "
          + "FROM [KitchenTable] AS [t2] WHERE ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
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