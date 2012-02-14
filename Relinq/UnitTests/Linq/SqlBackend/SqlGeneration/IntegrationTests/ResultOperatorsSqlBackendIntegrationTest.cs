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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class ResultOperatorsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c).Count(),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void CountWithOrderings ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c).Count(),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void CountWithProperty ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count(),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void CountWithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count (name => name != null),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void SimpleDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct(),
          "SELECT DISTINCT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void SimpleTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5),
          "SELECT TOP (5) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void TakeWithMemberExpression ()
    {
      CheckQuery (
          () => (from k in Kitchens from c in k.Restaurant.Cooks.Take (k.RoomNumber) select k.Name),
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] ON [t1].[RestaurantID] = [t2].[ID] "
          + "CROSS APPLY (SELECT TOP ([t1].[RoomNumber]) "
          + "[t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] "
          + "FROM [CookTable] AS [t3] WHERE ([t2].[ID] = [t3].[RestaurantID])) AS [q0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void TakeWithSubQuery ()
    {
      CheckQuery (
          () => ((from c in Cooks select c.FirstName).Take ((from k in Kitchens select k).Count ())),
          "SELECT TOP ((SELECT COUNT(*) AS [value] FROM [KitchenTable] AS [t1])) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void First ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First(),
          "SELECT TOP (1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault(),
          "SELECT TOP (1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void First_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First (fn => fn != null),
          "SELECT TOP (1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault (fn => fn != null),
          "SELECT TOP (1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Single ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single(),
          "SELECT TOP (2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault(),
          "SELECT TOP (2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Single_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single (fn => fn != null),
          "SELECT TOP (2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault (fn => fn != null),
          "SELECT TOP (2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Contains_WithQuery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (s) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] "
          +"WHERE [t0].[ID] "
          +"IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])");
    }

    [Test]
    public void Contains_WithConstant ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (cook) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_WithDerivedType ()
    {
      var chef = new Chef { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (chef) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          + "FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter ("@1", 23));
    }

    [Test]
    public void Contains_WithConstantAndDependentQuery ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (cook) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_OnTopLevel ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          ( () => (from s in Cooks select s).Contains(cook)),
          "SELECT CASE WHEN @1 IN (SELECT [t0].[ID] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END AS [value]",
          row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0))),
          new CommandParameter("@1", 23) );
    }

    [Test]
    public void Contains_WithConstantCollection ()
    {
      var cookNames = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));

      IEnumerable<string> cookNamesAsEnumerable = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          from c in Cooks where cookNamesAsEnumerable.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithEmptyCollection ()
    {
      var cookNames = new string[] { };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (SELECT NULL WHERE 1 = 0)");
    }

    [Test]
    public void Any_OnTopLevel_WithoutPredicate ()
    {
      CheckQuery(
        () => Cooks.Any(),
        "SELECT CASE WHEN EXISTS((SELECT [t0].[ID] FROM [CookTable] AS [t0])) THEN 1 ELSE 0 END AS [value]",
          row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0)))
        );
    }

    [Test]
    public void Any_OnTopLevel_WithPredicate ()
    {
      CheckQuery (
        () => Cooks.Any (c=>c.FirstName=="Hugo"),
        "SELECT CASE WHEN EXISTS((SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1))) THEN 1 ELSE 0 END AS [value]",
          row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0))),
        new CommandParameter("@1", "Hugo")
        );
    }

    [Test]
    public void Any_InSubquery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Any () select s.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE EXISTS((SELECT [t1].[ID] FROM [CookTable] AS [t1]))");
    }

    [Test]
    public void Any_OrderingsRemoved ()
    {
      CheckQuery (
        () => Cooks.OrderBy (c => c.FirstName).Any (),
        "SELECT CASE WHEN EXISTS((SELECT [t0].[ID] FROM [CookTable] AS [t0])) THEN 1 ELSE 0 END AS [value]",
          row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0)))
        );
    }

    [Test]
    public void All_OnTopLevel ()
    {
      CheckQuery(
        () => Cooks.All(c => c.Name=="Hugo"),
        "SELECT CASE WHEN NOT EXISTS((SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE NOT ([t0].[Name] = @1))) THEN 1 ELSE 0 END AS [value]",
        row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0))),
        new CommandParameter("@1", "Hugo")
        );
    }

    [Test]
    public void All_InSubquery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).All(c=>c.FirstName=="Hugo") select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE NOT EXISTS((SELECT [t1].[ID] FROM [CookTable] AS [t1] " +
          "WHERE NOT ([t1].[FirstName] = @1)))",
          new CommandParameter ("@1", "Hugo")
        );
    }

    [Test]
    public void All_OrderingsRemoved ()
    {
      CheckQuery (
       () => Cooks.OrderBy (c => c.FirstName).All (c => c.Name == "Hugo"),
        "SELECT CASE WHEN NOT EXISTS((SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE NOT ([t0].[Name] = @1))) THEN 1 ELSE 0 END AS [value]",
          row => (object) Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0))),
        new CommandParameter ("@1", "Hugo")
        );
    }
    
    [Test]
    public void Cast_TopLevel_OnValue ()
    {
      CheckQuery (
          (from s in Cooks select s.ID).Cast<double> (),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0]",
          row => (object) (double) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Cast_TopLevel_OnEntity ()
    {
      CheckQuery (
          (from s in Cooks select s).Cast<Chef> (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          + "FROM [CookTable] AS [t0]",
           row => (object) (Chef) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6)));
    }

    [Test]
    public void Cast_SubQuery ()
    {
      CheckQuery (
          (from r in (from c in Cooks select c.FirstName).Cast<object>() select r),
          "SELECT [q0].[value] AS [value] FROM (SELECT [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]");
    }

    [Test]
    public void OfType ()
    {
      CheckQuery (
          Cooks.OfType<Chef> (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] " +
          "FROM [CookTable] AS [t0] WHERE ([t0].[IsStarredCook] = 1)",
           row => (object) (Chef) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6))
          );

      CheckQuery (
          Chefs.OfType<Chef>(),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],"+
          "[t0].[KitchenID],[t0].[LetterOfRecommendation] FROM [dbo].[ChefTable] AS [t0] WHERE (@1 = 1)",
// ReSharper disable RedundantCast
           row => (object) (Chef) row.GetEntity<Chef> (
// ReSharper restore RedundantCast
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("LetterOfRecommendation", 7)),
          new CommandParameter("@1", 1));

      CheckQuery (
          Chefs.OfType<Cook> (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],"+
          "[t0].[KitchenID],[t0].[LetterOfRecommendation] FROM [dbo].[ChefTable] AS [t0] WHERE (@1 = 1)",
           row => (object) (Cook) row.GetEntity<Chef> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("LetterOfRecommendation", 7)),
          new CommandParameter("@1", 1));
    }

    [Test]
    public void DefaultIfEmpty ()
    {
      CheckQuery (
          Cooks.DefaultIfEmpty (),
          "SELECT [q0].[ID],[q0].[FirstName],[q0].[Name],[q0].[IsStarredCook],[q0].[IsFullTimeCook],[q0].[SubstitutedID],[q0].[KitchenID] "+
          "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN (SELECT [t1].[ID],[t1].[FirstName],[t1].[Name],[t1].[IsStarredCook],"+
          "[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID] FROM [CookTable] AS [t1]) AS [q0] ON 1 = 1",
           row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6)));
    }

    [Test]
    public void DefaultIfEmpty_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).DefaultIfEmpty().Max() > 5 select s.Name,
          "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t1] WHERE ((SELECT MAX([q0].[value]) AS [value] FROM " +
          "(SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2]) AS [q0] ON 1 = 1) > @1)",
          new CommandParameter ("@1", 5));
    }
    
    [Test]
    public void Max_OnTopLevel ()
    {
      CheckQuery (
          () => (from c in Cooks select c.ID).Max(),
          "SELECT MAX([t0].[ID]) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Max_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).Max() > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT MAX([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter("@1", 5));
    }

    [Test]
    public void Min_OnTopLevel ()
    {
      CheckQuery (
          () => (from c in Cooks select c.ID).Min (),
          "SELECT MIN([t0].[ID]) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Min_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).Min () > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT MIN([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void Average_OnTopLevel ()
    {
      CheckQuery (
          () => Kitchens.Average(k=>k.RoomNumber),
          "SELECT AVG([t0].[RoomNumber]) AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<double> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Average_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2).Average(c=>c.ID) > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT AVG([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter("@1", 5.0));
    }

    [Test]
    public void Sum_OnTopLevel ()
    {
      CheckQuery (
          () => Kitchens.Sum (k => k.RoomNumber),
          "SELECT SUM([t0].[RoomNumber]) AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Sum_WithOrderings ()
    {
      CheckQuery (
          () => Kitchens.OrderBy (k => k.Name).Sum (k => k.RoomNumber),
          "SELECT SUM([t0].[RoomNumber]) AS [value] FROM [KitchenTable] AS [t0]"
          );
    }

    [Test]
    public void Sum_WithOrderings2 ()
    {
      CheckQuery (
          () => Kitchens.OrderBy (k => k.Name).Take (5).Sum (k => k.RoomNumber),
          "SELECT SUM([q0].[Key_RoomNumber]) AS [value] FROM (SELECT TOP (5) [t1].[ID] AS [Key_ID],[t1].[CookID] AS [Key_CookID]," 
          + "[t1].[Name] AS [Key_Name],[t1].[RestaurantID] AS [Key_RestaurantID],[t1].[SubKitchenID] AS [Key_SubKitchenID],"
          + "[t1].[LastCleaningDay] AS [Key_LastCleaningDay],[t1].[PassedLastInspection] AS [Key_PassedLastInspection],[t1].[Name] AS [Value] "
          + "FROM [KitchenTable] AS [t1] ORDER BY [t1].[Name] ASC) AS [q0]");
    }

    [Test]
    public void Sum_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2).Sum (c => c.ID) > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT SUM([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void Skip_WithEntity ()
    {
      CheckQuery (
          () => (from r in Restaurants orderby r.ID select r).Skip (5),
          "SELECT [q0].[Key_ID] AS [ID],[q0].[Key_CookID] AS [CookID],[q0].[Key_Name] AS [Name] "+
          "FROM (SELECT [t0].[ID] AS [Key_ID],[t0].[CookID] AS [Key_CookID],[t0].[Name] AS [Key_Name],"+
          "ROW_NUMBER() OVER (ORDER BY [t0].[ID] ASC) AS [Value] FROM [RestaurantTable] AS [t0]) AS [q0] "+
          "WHERE ([q0].[Value] > @1) ORDER BY [q0].[Value] ASC",
           row => (object) row.GetEntity<Restaurant> (
              new ColumnID ("ID", 0),
              new ColumnID ("CookID", 1),
              new ColumnID ("Name", 2)),
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void Skip_WithEntity_WithoutExplicitOrdering ()
    {
      CheckQuery (
          () => (from r in Restaurants select r).Skip (5),
          "SELECT [q0].[Key_ID] AS [ID],[q0].[Key_CookID] AS [CookID],[q0].[Key_Name] AS [Name] "+
          "FROM (SELECT [t0].[ID] AS [Key_ID],[t0].[CookID] AS [Key_CookID],[t0].[Name] AS [Key_Name],"+
          "ROW_NUMBER() OVER (ORDER BY (SELECT @1) ASC) AS [Value] FROM [RestaurantTable] AS [t0]) AS [q0] "+
          "WHERE ([q0].[Value] > @2) ORDER BY [q0].[Value] ASC",
          new CommandParameter("@1", 1),
          new CommandParameter("@2", 5));
    }

    [Test]
    public void Skip_WithColumn ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE ([q0].[Value] > @1) ORDER BY [q0].[Value] ASC",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 100));
    }

    [Test]
    public void Skip_WithColumn_WithoutExplicitOrdering ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY (SELECT @1) ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE ([q0].[Value] > @2) ORDER BY [q0].[Value] ASC",
          new CommandParameter ("@1", 1),
          new CommandParameter("@2", 100));
    }

    [Test]
    public void Skip_WithConstant ()
    {
      CheckQuery (
          () => (from c in Cooks orderby 20 select 10).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT @1 AS [Key],ROW_NUMBER() OVER (ORDER BY (SELECT @2) ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE ([q0].[Value] > @3) ORDER BY [q0].[Value] ASC",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10),
          new CommandParameter ("@2", 20),
          new CommandParameter ("@3", 100));
    }

    [Test]
    public void OrderBy_BeforeDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c).Distinct(),
          "SELECT DISTINCT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "+
          "FROM [CookTable] AS [t0]");
    }
  }
}