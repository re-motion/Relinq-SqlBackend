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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class ResultOperatorsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c).Count(),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithProperty ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count(),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count (name => name != null),
          "SELECT COUNT(*) AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)");
    }

    [Test]
    public void SimpleDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct(),
          "SELECT DISTINCT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void SimpleTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2408")]
    public void TakeWithMemberExpression ()
    {
      CheckQuery (
          () => (from k in Kitchens from c in k.Restaurant.Cooks.Take (k.RoomNumber) select k.Name),
          "SELECT [t1].[Name] FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] ON [t1].[RestaurantID] = [t2].[ID] "
          + "CROSS APPLY (SELECT TOP ([t1].[RoomNumber]) "
          + "[t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] "
          + "FROM [CookTable] AS [t3] WHERE ([t2].[ID] = [t3].[RestaurantID])) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2422")]
    public void TakeWithSubQuery ()
    {
      CheckQuery (
          () => ((from c in Cooks select c).Take ((from k in Kitchens select k).Count ())),
          "",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void First ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First(),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void First_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Single ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single(),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 2));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 2));
    }

    [Test]
    public void Single_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 2));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 2));
    }

    [Test]
    public void Contains_WithQuery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (s) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] "
          +"WHERE [t0].[ID] "
          +"IN (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1])");
    }

    [Test]
    public void Contains_WithConstant ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (cook) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1])",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_WithConstantAndDependentQuery ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (cook) select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_OnTopLevel ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          ( () => (from s in Cooks select s).Contains(cook)),
          "SELECT CASE WHEN @1 IN (SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END AS [value]",
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
        "SELECT CASE WHEN EXISTS((SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0])) THEN 1 ELSE 0 END AS [value]"
        );
    }

    [Test]
    public void Any_OnTopLevel_WithPredicate ()
    {
      CheckQuery (
        () => Cooks.Any (c=>c.FirstName=="Hugo"),
        "SELECT CASE WHEN EXISTS((SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1))) THEN 1 ELSE 0 END AS [value]",
        new CommandParameter("@1", "Hugo")
        );
    }

    [Test]
    public void Any_InSubquery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Any () select s.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE EXISTS((SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1]))");
    }

    [Test]
    public void All_OnTopLevel ()
    {
      CheckQuery(
       () => Cooks.All(c => c.Name=="Hugo"),
        "SELECT CASE WHEN NOT EXISTS((SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE NOT ([t0].[Name] = @1))) THEN 1 ELSE 0 END AS [value]",
        new CommandParameter("@1", "Hugo")
        );
    }

    [Test]
    public void All_InSubquery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).All(c=>c.FirstName=="Hugo") select s.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE NOT EXISTS((SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] "+
        "WHERE NOT ([t1].[FirstName] = @1)))",
        new CommandParameter ("@1", "Hugo")
        );
    }
    
    [Test]
    public void Cast_TopLevel ()
    {
      CheckQuery (
          (from s in Cooks select s.FirstName).Cast<object>(),
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]");
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
          "FROM [CookTable] AS [t0] WHERE ([t0].[IsStarredCook] = 1)"
          );

      CheckQuery (
          Chefs.OfType<Chef>(),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],"+
          "[t0].[KitchenID],[t0].[LetterOfRecommendation] FROM [ChefTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter("@1", 1));

      CheckQuery (
          Chefs.OfType<Cook> (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],"+
          "[t0].[KitchenID],[t0].[LetterOfRecommendation] FROM [ChefTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter("@1", 1));
    }

    [Test]
    public void DefaultIfEmpty ()
    {
      CheckQuery (
          Cooks.DefaultIfEmpty (),
          "SELECT [q0].[ID],[q0].[FirstName],[q0].[Name],[q0].[IsStarredCook],[q0].[IsFullTimeCook],[q0].[SubstitutedID],[q0].[KitchenID] "+
          "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN "+
          "(SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "+
          "FROM [CookTable] AS [t0]) AS [q0] ON 1 = 1");
    }

    [Test]
    public void DefaultIfEmpty_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).DefaultIfEmpty().Max() > 5 select s.Name,
          "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t1] WHERE ((SELECT MAX([q0].[value]) AS [value] FROM "+
          "(SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2]) AS [q0] ON 1 = 1) > @1)",
          new CommandParameter ("@1", 5));
    }
    
    [Test]
    public void Max_OnTopLevel ()
    {
      CheckQuery (
          () => (from c in Cooks select c.ID).Max(),
          "SELECT MAX([t0].[ID]) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void Max_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).Max()>5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT MAX([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter("@1", 5));
    }

    [Test]
    public void Min_OnTopLevel ()
    {
      CheckQuery (
          () => (from c in Cooks select c.ID).Min (),
          "SELECT MIN([t0].[ID]) AS [value] FROM [CookTable] AS [t0]");
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
          "SELECT AVG([t0].[RoomNumber]) AS [value] FROM [KitchenTable] AS [t0]");
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
          "SELECT SUM([t0].[RoomNumber]) AS [value] FROM [KitchenTable] AS [t0]");
    }

    [Test]
    public void Sum_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2).Sum (c => c.ID) > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT SUM([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter ("@1", 5));
    }

  }
}