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
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.ResultOperators
{
  [TestFixture]
  public class DefaultIfEmptyResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void DefaultIfEmpty ()
    {
      CheckQuery (
          Cooks.DefaultIfEmpty(),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],[t0].[KnifeID],[t0].[KnifeClassID],[t0].[CookRating] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] OUTER APPLY [CookTable] AS [t0]",
          row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("KnifeID", 7),
              new ColumnID ("KnifeClassID", 8),
              new ColumnID ("CookRating", 9)));
    }

    [Test]
    public void DefaultIfEmpty_WithWhereCondition ()
    {
      CheckQuery (
          Cooks.Where (c => c.Name != null).DefaultIfEmpty(),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],[t0].[KnifeID],[t0].[KnifeClassID],[t0].[CookRating] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t0] ON ([t0].[Name] IS NOT NULL)",
          row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("KnifeID", 7),
              new ColumnID ("KnifeClassID", 8),
              new ColumnID ("CookRating", 9)));
    }

    [Test]
    public void DefaultIfEmpty_WithDefaultValue ()
    {
      Assert.That (
          () => CheckQuery (Cooks.DefaultIfEmpty (null), "..."),
          Throws.TypeOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The DefaultIfEmpty operator is not supported if a default value is specified. Use the overload without a specified default value."));
    }

    [Test]
    public void DefaultIfEmpty_InSubquery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2.ID).DefaultIfEmpty().Max() > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] "
          + "FROM [CookTable] AS [t0] "
          + "WHERE ("
          + "(SELECT MAX([t1].[ID]) AS [value] FROM (SELECT NULL AS [Empty]) AS [Empty] OUTER APPLY [CookTable] AS [t1]) > @1"
          + ")",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithMultipleFromClauses ()
    {
      CheckQuery (
          from c in Cooks
          from kitchen in Kitchens.Where (k => k == c.Kitchen).DefaultIfEmpty()
          from knife in Knives.Where (k => k == c.Knife).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = kitchen.ID, KnifeID = knife.ID },
          "SELECT [t2].[ID] AS [CookID],[q0].[ID] AS [KitchenID],[q1].[ID] AS [KnifeID_Value],[q1].[ClassID] AS [KnifeID_ClassID] "
          + "FROM [CookTable] AS [t2] "
          + "CROSS APPLY ("
          + "SELECT [t3].[ID],[t3].[Name],[t3].[RestaurantID],[t3].[LastCleaningDay],[t3].[PassedLastInspection],[t3].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t3] ON ([t3].[ID] = [t2].[KitchenID])"
          + ") AS [q0] "
          + "CROSS APPLY ("
          + "SELECT [t4].[ID],[t4].[ClassID],[t4].[Sharpness] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KnifeTable] AS [t4] ON (([t4].[ID] = [t2].[KnifeID]) AND ([t4].[ClassID] = [t2].[KnifeClassID]))"
          + ") AS [q1]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithImplicitLeftJoinAddedByLeftJoinCondition ()
    {
      CheckQuery (
          from c in Cooks
          from restaurant in Restaurants.Where (r => r == c.Kitchen.Restaurant).DefaultIfEmpty()
          select new { CookID = c.ID, RestaurantID = restaurant.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [RestaurantID] "
          + "FROM [CookTable] AS [t1] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t3] ON ([t1].[KitchenID] = [t3].[ID]) "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[CompanyID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [RestaurantTable] AS [t2] ON ([t2].[ID] = [t3].[RestaurantID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithImplicitLeftJoinAddedByFromClause ()
    {
      CheckQuery (
          from k in Kitchens
          from c in k.Restaurant.Cooks.DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [q0].[ID] AS [CookID],[t1].[ID] AS [KitchenID] "
          + "FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] ON ([t1].[RestaurantID] = [t2].[ID]) "
          + "CROSS APPLY ("
          + "SELECT [t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID],[t3].[KnifeID],[t3].[KnifeClassID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t3] ON ([t2].[ID] = [t3].[RestaurantID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithImplicitLeftJoinAddedByLeftJoinConditionAndFromClause ()
    {
      CheckQuery (
          from k in Kitchens
          from c in k.Restaurant.Cooks
          from restaurant in Restaurants.Where (r => r == k.Cook.Kitchen.Restaurant).DefaultIfEmpty()
          select new { CookID = k.ID, RestaurantID = restaurant.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [RestaurantID] "
          + "FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] ON ([t1].[RestaurantID] = [t2].[ID]) "
          + "LEFT OUTER JOIN [CookTable] AS [t5] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t6] "
          + "ON ([t5].[KitchenID] = [t6].[ID]) "
          + "ON ([t1].[ID] = [t5].[KitchenID]) "
          + "CROSS JOIN [CookTable] AS [t3] "
          + "CROSS APPLY ("
          + "SELECT [t4].[ID],[t4].[CompanyID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [RestaurantTable] AS [t4] ON ([t4].[ID] = [t6].[RestaurantID])"
          + ") AS [q0] "
          + "WHERE ([t2].[ID] = [t3].[RestaurantID])");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_MultipleTimes_WithImplicitLeftJoinsAddedByLeftJoinCondition ()
    {
      CheckQuery (
          from c in Cooks
          from r1 in Restaurants.Where (r => r == c.Kitchen.Restaurant).DefaultIfEmpty()
          from k in Kitchens.Where (k => k == c.Kitchen).DefaultIfEmpty()
          from r2 in Restaurants.Where (r => r == k.Restaurant).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t3].[ID] AS [CookID],[q1].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t3] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t5] ON ([t3].[KitchenID] = [t5].[ID]) "
          + "CROSS APPLY ("
          + "SELECT [t4].[ID],[t4].[CompanyID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [RestaurantTable] AS [t4] ON ([t4].[ID] = [t5].[RestaurantID])"
          + ") AS [q0] "
          + "CROSS APPLY ("
          + "SELECT [t6].[ID],[t6].[Name],[t6].[RestaurantID],[t6].[LastCleaningDay],[t6].[PassedLastInspection],[t6].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t6] ON ([t6].[ID] = [t3].[KitchenID])"
          + ") AS [q1] "
          + "CROSS APPLY ("
          + "SELECT [t7].[ID],[t7].[CompanyID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [RestaurantTable] AS [t7] ON ([t7].[ID] = [q1].[RestaurantID])"
          + ") AS [q2]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithImplicitLeftJoinAddedByLeftJoinCondition_AndSubQueryAccessingSameMembers ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k.Cook.Knife == c.Knife || k.Cook.Assistants.Any (a => a.Knife == c.Knife)).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] "
          + "LEFT OUTER JOIN [CookTable] AS [t3] ON ([t2].[ID] = [t3].[KitchenID]) "
          + "ON ((([t3].[KnifeID] = [t1].[KnifeID]) AND ([t3].[KnifeClassID] = [t1].[KnifeClassID])) OR EXISTS((SELECT [t4].[ID] FROM [CookTable] AS [t4] WHERE (([t3].[ID] = [t4].[AssistedID]) AND (([t4].[KnifeID] = [t1].[KnifeID]) AND ([t4].[KnifeClassID] = [t1].[KnifeClassID]))))))"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithoutWhereClause ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] OUTER APPLY [KitchenTable] AS [t2]"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithWhereClauseConditionFromLeftSide ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithWhereClauseConditionFromRightSide ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k.Cook == c).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t2] "
          + "LEFT OUTER JOIN [CookTable] AS [t3] "
          + "ON ([t2].[ID] = [t3].[KitchenID]) "
          + "ON ([t3].[ID] = [t1].[ID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithWhereClause_AndOuterWhereClause ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen).DefaultIfEmpty()
          where k.Name != null
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0] "
          + "WHERE ([q0].[Name] IS NOT NULL)");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause_WithNewExpressionInProjection ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where (k => k == c.Kitchen).Select (k => new { k.ID, k.Name }).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID, KitchenName = k.Name },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID],[q0].[Name] AS [KitchenName] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID] AS [ID],[t2].[Name] AS [Name] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InJoinClause ()
    {
      CheckQuery (
          from c in Cooks
          join k in Kitchens on c.Kitchen equals k into kitchens
          from k in kitchens.DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t1].[KitchenID] = [t2].[ID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_OnMemberExpression_WithoutWhereClause ()
    {
      CheckQuery (
          from r in Restaurants
          from c in r.Cooks.DefaultIfEmpty()
          select new { RestaurantID = r.ID, CookID = c.ID, CookName = c.Name },
          "SELECT [t1].[ID] AS [RestaurantID],[q0].[ID] AS [CookID],[q0].[Name] AS [CookName] "
          + "FROM [RestaurantTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID],[t2].[KnifeID],[t2].[KnifeClassID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[RestaurantID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_OnMemberExpression_WithWhereClause ()
    {
      CheckQuery (
          from r in Restaurants
          from c in r.Cooks.Where (c => c.IsFullTimeCook).DefaultIfEmpty()
          select new { RestaurantID = r.ID, CookID = c.ID, CookName = c.Name },
          "SELECT [t1].[ID] AS [RestaurantID],[q0].[ID] AS [CookID],[q0].[Name] AS [CookName] "
          + "FROM [RestaurantTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID],[t2].[KnifeID],[t2].[KnifeClassID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t2] ON (([t1].[ID] = [t2].[RestaurantID]) AND ([t2].[IsFullTimeCook] = 1))"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_WithMultipleTables ()
    {
      CheckQuery (
          (from c in Cooks
            from k in Kitchens
            where k == c.Kitchen
            select new { CookID = c.ID, KitchenID = k.ID }).DefaultIfEmpty(),
          "SELECT [q0].[CookID] AS [CookID],[q0].[KitchenID] AS [KitchenID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] "
          + "OUTER APPLY ("
          + "SELECT [t1].[ID] AS [CookID],[t2].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS JOIN [KitchenTable] AS [t2] "
          + "WHERE ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
    }

    // TODO RMLNQSQL-77: This test would generate invalid SQL if optimization is implemented incorrectly.
    [Test]
    public void DefaultIfEmpty_WithEscapingReferenceInSubstatement ()
    {
      CheckQuery (
          from c in Cooks
          from x in (
              from k in (
                  from y in Kitchens
                  where y == c.Kitchen
                  select y
                  ).Distinct()
              where k == c.Kitchen
              select k
              ).DefaultIfEmpty()
          select x,
          "SELECT [q1].[ID],[q1].[Name],[q1].[RestaurantID],[q1].[LastCleaningDay],[q1].[PassedLastInspection],[q1].[LastInspectionScore] "
          + "FROM [CookTable] AS [t2] "
          + "CROSS APPLY ("
          + "SELECT [q0].[ID],[q0].[Name],[q0].[RestaurantID],[q0].[LastCleaningDay],[q0].[PassedLastInspection],[q0].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] "
          + "LEFT OUTER JOIN ("
          + "SELECT DISTINCT [t3].[ID],[t3].[Name],[t3].[RestaurantID],[t3].[LastCleaningDay],[t3].[PassedLastInspection],[t3].[LastInspectionScore] "
          + "FROM [KitchenTable] AS [t3] "
          + "WHERE ([t3].[ID] = [t2].[KitchenID])"
          + ") AS [q0] ON ([q0].[ID] = [t2].[KitchenID])"
          + ") AS [q1]");
    }
  }
}