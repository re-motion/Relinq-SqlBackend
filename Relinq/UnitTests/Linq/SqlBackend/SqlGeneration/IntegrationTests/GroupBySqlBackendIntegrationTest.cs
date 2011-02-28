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
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class GroupBySqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    [ExpectedException(typeof(NotSupportedException), ExpectedMessage =
        "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to "
         + "efficiently return LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional "
         + "from clause, or perform an aggregation on the groupings.", MatchType = MessageMatch.Contains)]
    public void GroupBy_TopLevel ()
    {
      CheckQuery (
          () => Cooks.GroupBy (c => c.Name),
          "");
    }

    [Test]
    public void GroupBy_SelectKey ()
    {
      CheckQuery (
          from c in Cooks group c by c.Name into cooksByName select cooksByName.Key,
          "SELECT [q0].[key] AS [value] FROM (" +
            "SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] " +
            "GROUP BY [t1].[Name]) AS [q0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void GroupBy_SelectAndCheckEntityKey ()
    {
      CheckQuery (
          from c in Cooks
          group c by c.Substitution into cooksBySubstitution
          where cooksBySubstitution.Key != null
          select cooksBySubstitution.Key.FirstName,
          "SELECT [q0].[key_FirstName] AS [value] FROM ("
              + "SELECT [t2].[ID] AS [key_ID],[t2].[FirstName] AS [key_FirstName],[t2].[Name] AS [key_Name],"
              + "[t2].[IsStarredCook] AS [key_IsStarredCook],[t2].[IsFullTimeCook] AS [key_IsFullTimeCook],"
              + "[t2].[SubstitutedID] AS [key_SubstitutedID],[t2].[KitchenID] AS [key_KitchenID] "
              + "FROM [CookTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[SubstitutedID] "
              + "GROUP BY [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID]"
              + ") AS [q0] "
          + "WHERE ([q0].[key_ID] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void GroupBy_SelectKey_Nesting ()
    {
      CheckQuery (
          from r in Restaurants
          from x in (
            from c in r.Cooks group c by c.Name into cooksByName 
            select new { RestaurantID = r.ID, Cooks = cooksByName }
          )
          select x.Cooks.Key,
          "SELECT [q1].[Cooks_key] AS [value] FROM [RestaurantTable] AS [t2] CROSS APPLY (SELECT [t2].[ID] AS [RestaurantID],"
            + "[q0].[key] AS [Cooks_key] FROM (SELECT [t3].[Name] AS [key] FROM [CookTable] AS [t3] WHERE ([t2].[ID] = [t3].[RestaurantID]) "
            + "GROUP BY [t3].[Name]) AS [q0]) AS [q1]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void GroupBy_SelectKeyAndAggregate ()
    {
      CheckQuery (
          from c in Cooks group c.ID by c.Name into cooksByName select new { Name = cooksByName.Key, Count = cooksByName.Count() }, 
          "SELECT [q0].[key] AS [Name],[q0].[a0] AS [Count] FROM ("+
            "SELECT [t1].[Name] AS [key], COUNT(*) AS [a0] FROM [CookTable] AS [t1] "+
            "GROUP BY [t1].[Name]) AS [q0]",
            row => (object) new { 
                Name = row.GetValue<string> (new ColumnID ("Name", 0)), 
                Count = row.GetValue<int> (new ColumnID ("Count", 1)) });
    }

    [Test]
    public void GoupBy_CountInWhereCondition ()
    {
      CheckQuery (
          from c in Cooks group c by c.Name into cooksByName where cooksByName.Count() > 0 select cooksByName.Key,
          "SELECT [q0].[key] AS [value] FROM (" +
            "SELECT [t1].[Name] AS [key], COUNT(*) AS [a0] FROM [CookTable] AS [t1] "+
            "GROUP BY [t1].[Name]) AS [q0] "+
            "WHERE ([q0].[a0] > @1)",
            new CommandParameter ("@1", 0));
    }

    [Test]
    public void GroupBy_Min_WithNonTrivialElementExpression ()
    {
      CheckQuery (
          from c in Cooks group c.Weight by c.Name into cooksByName where cooksByName.Min () > 18 select cooksByName.Key, 
          "SELECT [q0].[key] AS [value] FROM (" +
            "SELECT [t1].[Name] AS [key], MIN([t1].[Weight]) AS [a0] FROM [CookTable] AS [t1] "+
            "GROUP BY [t1].[Name]) AS [q0] "+
            "WHERE ([q0].[a0] > @1)",
            new CommandParameter ("@1", 18.0));
    }

    [Test]
    public void GroupBy_Min_WithProjection ()
    {
      CheckQuery (
          from c in Cooks group c by c.Name into cooksByName where cooksByName.Min (c => c.Weight) > 18 select cooksByName.Key,
          "SELECT [q0].[key] AS [value] FROM (" +
            "SELECT [t1].[Name] AS [key], MIN([t1].[Weight]) AS [a0] FROM [CookTable] AS [t1] " +
            "GROUP BY [t1].[Name]) AS [q0] " +
            "WHERE ([q0].[a0] > @1)",
            new CommandParameter("@1", 18.0));
    }

    [Test]
    public void GroupBy_Min_WithProjection_AndNestedElements ()
    {
      CheckQuery (
          from c in Cooks group new { c.ID, c.FirstName } by c.Name into cooksByName 
          where cooksByName.Min (c => c.ID) > 18 select cooksByName.Key,
          "SELECT [q0].[key] AS [value] FROM (" +
            "SELECT [t1].[Name] AS [key], MIN([t1].[ID]) AS [a0] FROM [CookTable] AS [t1] " +
            "GROUP BY [t1].[Name]) AS [q0] " +
            "WHERE ([q0].[a0] > @1)",
            new CommandParameter ("@1", 18));
    }

    [Test]
    [Ignore ("TODO 3045")]
    public void GroupBy_Aggregation_FromNestedSubQuery ()
    {
      CheckQuery (
        from x in
          (
            from c in Cooks
            group c.ID by c.Name into cooksByName
            select cooksByName
          ).Take (3)
        where x.Min (c => c) > 18
        select x.Key,
        "SELECT [q1].[key] AS [value] "
            + "FROM (SELECT TOP (@1) [q0].[key] AS [key],[q0].[a0] AS [a0] FROM ("
                + "SELECT [t2].[Name] AS [key], MIN([t2].[ID]) AS [a0] FROM [CookTable] AS [t2] GROUP BY [t2].[Name]) AS [q0]"
            + ") AS [q1] "
            + "WHERE ([q1].[a0] > @3)",
        new CommandParameter ("@1", 18));
    }

    [Test]
    [Ignore ("TODO 3045")]
    public void GroupBy_MinWithProjection_SelectingAnotherTable_InWhereCondition ()
    {
      CheckQuery (
          from c in Cooks group c.Weight by c.Name into cooksByName 
          from k in Kitchens
          where cooksByName.Min (c => k.ID) > 18 select cooksByName.Key,
          "SELECT [q0].[key] AS [value] "
          + "FROM (SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0] "
          + "CROSS JOIN [KitchenTable] AS [t2] "
          + "WHERE (("
              + "SELECT MIN([t2].[ID]) AS [value]"
              + "FROM ("
                  + "SELECT [t1].[Weight] AS [element] FROM [CookTable] AS [t1] "
                  + "WHERE ((([t1].[Name] IS NULL) AND ([q0].[key] IS NULL)) "
                  + "OR ((([t1].[Name] IS NOT NULL) AND ([q0].[key] IS NOT NULL)) AND ([t1].[Name] = [q0].[key])))) AS [q3]"
              + ") > @1)",
          new CommandParameter ("@1", 18));
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression_SimpleElement ()
    {
      CheckQuery (
          from c in Cooks
          group c.ID by c.Name
          into cooksByName
          from id in cooksByName
          select new { cooksByName.Key, CookID = id },
          "SELECT [q0].[key] AS [Key],[q2].[element] AS [CookID] "
          + "FROM "
          + "(SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0] "
          + "CROSS APPLY ("
          + "SELECT [t1].[ID] AS [element] FROM [CookTable] AS [t1] "
          + "WHERE ((([t1].[Name] IS NULL) AND ([q0].[key] IS NULL)) "
          + "OR ((([t1].[Name] IS NOT NULL) AND ([q0].[key] IS NOT NULL)) AND ([t1].[Name] = [q0].[key])))) AS [q2]",
          row => (object) new 
          { 
              Key = row.GetValue<string> (new ColumnID ("Key", 0)), 
              CookID = row.GetValue<int> (new ColumnID ("CookID", 1))
          });
    }
    
    [Test]
    public void GroupBy_UseGroupInFromExpression_EntityElement ()
    {
      CheckQuery (
          from c in Cooks
          group c by c.Name
          into cooksByName
          from cook in cooksByName
          where cook != null
          select new { cooksByName.Key, CookID = cook },
          "SELECT [q0].[key] AS [Key],[q2].[element_ID] AS [CookID_ID],"
          + "[q2].[element_FirstName] AS [CookID_FirstName],[q2].[element_Name] AS [CookID_Name],"
          + "[q2].[element_IsStarredCook] AS [CookID_IsStarredCook],[q2].[element_IsFullTimeCook] AS [CookID_IsFullTimeCook],"
          + "[q2].[element_SubstitutedID] AS [CookID_SubstitutedID],[q2].[element_KitchenID] AS [CookID_KitchenID] "
          + "FROM ("
          + "SELECT [t1].[Name] AS [key] "
          + "FROM [CookTable] AS [t1] "
          + "GROUP BY [t1].[Name]) AS [q0] "
          + "CROSS APPLY ("
          + "SELECT [t1].[ID] AS [element_ID],[t1].[FirstName] AS [element_FirstName],[t1].[Name] AS [element_Name],"
          + "[t1].[IsStarredCook] AS [element_IsStarredCook],[t1].[IsFullTimeCook] AS [element_IsFullTimeCook],"
          + "[t1].[SubstitutedID] AS [element_SubstitutedID],[t1].[KitchenID] AS [element_KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "WHERE ((([t1].[Name] IS NULL) AND ([q0].[key] IS NULL)) "
          + "OR ((([t1].[Name] IS NOT NULL) AND ([q0].[key] IS NOT NULL)) AND ([t1].[Name] = [q0].[key])))) AS [q2] "
          + "WHERE ([q2].[element_ID] IS NOT NULL)",
          row => (object) new 
          {
            Key = row.GetValue<string> (new ColumnID ("Key", 0)),
            CookID = row.GetEntity<Cook> (
              new ColumnID ("CookID_ID", 1),
              new ColumnID ("CookID_FirstName", 2),
              new ColumnID ("CookID_Name", 3),
              new ColumnID ("CookID_IsStarredCook", 4),
              new ColumnID ("CookID_IsFullTimeCook", 5),
              new ColumnID ("CookID_SubstitutedID", 6),
              new ColumnID ("CookID_KitchenID", 7))
          });
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression_BooleanElement ()
    {
      CheckQuery (
          from c in Cooks
          group c.IsFullTimeCook by c.Name
            into fullTimeCooksByName
            from isFullTime in fullTimeCooksByName
            select new { fullTimeCooksByName.Key, Value = isFullTime },
          "SELECT [q0].[key] AS [Key],[q2].[element] AS [Value] "
          + "FROM (SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0] "
          + "CROSS APPLY (SELECT [t1].[IsFullTimeCook] AS [element] FROM [CookTable] AS [t1] "
            + "WHERE ((([t1].[Name] IS NULL) AND ([q0].[key] IS NULL)) "
            + "OR ((([t1].[Name] IS NOT NULL) AND ([q0].[key] IS NOT NULL)) AND ([t1].[Name] = [q0].[key])))) AS [q2]");
    }

    [Test]
    public void GroupBy_UseGroupInFromExpression_UseGroupFromSubQuery ()
    {
      CheckQuery (
          from c in Cooks
          group c.Weight by c.Name 
          into weightsByName
          from cook in (
            from c in weightsByName
            select c).Distinct()
          select new { weightsByName.Key, CookID = cook },
          "SELECT [q0].[key] AS [Key],[q1].[value] AS [CookID] "
          + "FROM ("
              + "SELECT [t2].[Name] AS [key] FROM [CookTable] AS [t2] GROUP BY [t2].[Name]) AS [q0] "
          + "CROSS APPLY ("
              + "SELECT DISTINCT [q3].[element] AS [value] "
              + "FROM ("
                  + "SELECT [t2].[Weight] AS [element] "
                  + "FROM [CookTable] AS [t2] "
                  + "WHERE ((([t2].[Name] IS NULL) AND ([q0].[key] IS NULL)) "
                      + "OR ((([t2].[Name] IS NOT NULL) AND ([q0].[key] IS NOT NULL)) AND ([t2].[Name] = [q0].[key])))) AS [q3]) AS [q1]");
      }

    [Test]
    public void GroupBy_UseGroupInFromExpression_GroupByEntity ()
    {
      CheckQuery (
         from r in Restaurants
         from c in r.Cooks
         group c.ID by r into cooksByRestaurant
         from cook in cooksByRestaurant
         select new { cooksByRestaurant.Key.SubKitchen.ID, CookID = cook },
         "SELECT [t4].[ID] AS [ID],[q3].[element] AS [CookID] "
            + "FROM ("
                + "SELECT [t1].[ID] AS [key_ID],[t1].[CookID] AS [key_CookID],[t1].[Name] AS [key_Name] "
                + "FROM [RestaurantTable] AS [t1] "
                + "CROSS JOIN [CookTable] AS [t2] "
                + "WHERE ([t1].[ID] = [t2].[RestaurantID]) "
                + "GROUP BY [t1].[ID],[t1].[CookID],[t1].[Name]) AS [q0] "
           + "LEFT OUTER JOIN [KitchenTable] AS [t4] ON [q0].[key_ID] = [t4].[RestaurantID] "
           + "CROSS APPLY ("
             + "SELECT [t2].[ID] AS [element] "
             + "FROM [RestaurantTable] AS [t1] "
             + "CROSS JOIN [CookTable] AS [t2] "
             + "WHERE (([t1].[ID] = [t2].[RestaurantID]) "
                + "AND ((([t1].[ID] IS NULL) AND ([q0].[key_ID] IS NULL)) "
                + "OR ((([t1].[ID] IS NOT NULL) AND ([q0].[key_ID] IS NOT NULL)) AND ([t1].[ID] = [q0].[key_ID]))))) AS [q3]");
    }

    [Test]
    [Ignore ("TODO 3045")]
    public void GroupBy_GroupInFromExpression_FromNestedSubQuery ()
    {
      CheckQuery (
        from x in
          (
            from c in Cooks
            group c.ID by c.Name into cooksByName
            select cooksByName
          ).Take (3)
        from y in x
        select new { x.Key, y },
        "SELECT [q1].[key] AS [Key],[q3].[element] AS [y] "
            + "FROM ("
                + "SELECT TOP (@1) [q0].[key] AS [key] "
                + "FROM (SELECT [t2].[Name] AS [key] FROM [CookTable] AS [t2] GROUP BY [t2].[Name]) AS [q0]) AS [q1] "
                + "CROSS APPLY ("
                    + "SELECT [t2].[ID] AS [element] FROM [CookTable] AS [t2] "
                    + "WHERE ((([t2].[Name] IS NULL) AND ([q1].[key] IS NULL)) "
                        + "OR ((([t2].[Name] IS NOT NULL) AND ([q1].[key] IS NOT NULL)) AND ([t2].[Name] = [q1].[key])))) AS [q3]",
        new CommandParameter ("@1", 3));
    }

    [Test]
    public void GroupBy_WithResultSelector ()
    {
      CheckQuery (
        Cooks.GroupBy  (c => c.Name, (key, group) => new { Name = key }),
        "SELECT [q0].[key] AS [Name] "
        + "FROM (SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0]",
          row => (object) new { Name = row.GetValue<string> (new ColumnID ("Name", 0)) });
    }

    [Test]
    public void GroupBy_WithResultSelector_AndElementSelector ()
    {
      CheckQuery (
        Cooks.GroupBy (c => c.Name, c => c.ID, (key, group) => new { Name = key }),
        "SELECT [q0].[key] AS [Name] "
        + "FROM (SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0]",
          row => (object) new { Name = row.GetValue<string> (new ColumnID ("Name", 0)) });
    }

    [Test]
    public void GroupBy_SubqueryUsedAsGroupByKey ()
    {
      var query = (from c in Cooks
                   group c by Kitchens.Where (k => k != null).Select(k => k.Name).First()).Select (g => g.Key);
      CheckQuery (
          query, 
          "SELECT [q1].[key] AS [value] FROM ("
            + "SELECT [t0].[value] AS [key] "
            + "FROM [CookTable] AS [t2] "
            + "CROSS APPLY (SELECT TOP (1) [t3].[Name] AS [value] FROM [KitchenTable] AS [t3] WHERE ([t3].[ID] IS NOT NULL)) AS [t0] "
            + "GROUP BY [t0].[value]) AS [q1]");
    }

    [Test]
    public void GroupBy_WithConstantKey_GetsReplacedBySubquery ()
    {
      CheckQuery (Cooks.GroupBy (c => 0).Select (c => c.Key),
        "SELECT [q1].[key] AS [value] FROM ("
          + "SELECT [t0].[value] AS [key] FROM [CookTable] AS [t2] CROSS APPLY (SELECT @1 AS [value]) AS [t0] GROUP BY [t0].[value]"
        + ") AS [q1]",
        new CommandParameter("@1", 0));
    }

    [Test]
    [Ignore ("TODO 3045")]
    public void GroupBy_WithResultSelector_AndAggregate ()
    {
      CheckQuery (
        Cooks.GroupBy (c => c.Name, (key, group) => new { Name = key, Count = group.Count() }),
        "SELECT [q0].[key] AS [Name],[q0].[a0] AS [Count] "
        + "FROM (SELECT [t1].[Name] AS [key], COUNT(*) AS [a0] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0]",
          row => (object) new 
          { 
            Name = row.GetValue<string> (new ColumnID ("Name", 0)),
            Count = row.GetValue<int> (new ColumnID ("Count", 1))
          });
    }

    [Test]
    [Ignore ("TODO 3045")]
    public void GroupBy_WithResultSelector_AndElementSelector_AndAggregate ()
    {
      CheckQuery (
        Cooks.GroupBy (c => c.Name, c => c.ID, (key, group) => new { Name = key, Count = group.Min () }),
        "SELECT [q0].[key] AS [Name], [q0].[a0] AS [Count] "
        + "FROM (SELECT [t1].[Name] AS [key],MIN([t1].[ID]) AS [a0] FROM [CookTable] [t1] GROUP BY [t1].[Name]) AS [q0]",
        row => (object) new 
          { 
            Name = row.GetValue<string> (new ColumnID ("Name_key", 0)),
            Count = row.GetValue<int> (new ColumnID ("Count", 1))
          });
    }

     [Test]
     [Ignore ("TODO 3021/3045")]
    public void GroupBy_WithMemberInFromClause_AndGroupingComingFromSubQuery ()
    {
      CheckQuery (
          from x in
            (
              from c in Cooks
              group c.ID by c.Name into cooksByName
              select new { X = cooksByName, Y = 12 }
            ).Take (3)
          from y in x.X
          select new { x.X.Key, y },
          "SELECT [q1].[X_key] AS [Key],[q3].[element] AS [y] "
              + "FROM ("
                  + "SELECT TOP (@1) [q0].[key] AS [X_key], @1 AS [Y] "
                  + "FROM (SELECT [t2].[Name] AS [key] FROM [CookTable] AS [t2] GROUP BY [t2].[Name]) AS [q0]) AS [q1] "
              + "CROSS APPLY ("
                  + "SELECT [t2].[ID] AS [element] FROM [CookTable] AS [t2] "
                  + "WHERE ((([t2].[Name] IS NULL) AND ([q1].[X_key] IS NULL)) "
                      + "OR ((([t2].[Name] IS NOT NULL) AND ([q1].[X_key] IS NOT NULL)) AND ([t2].[Name] = [q1].[X_key])))) AS [q3]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 12));
    }
  }
}