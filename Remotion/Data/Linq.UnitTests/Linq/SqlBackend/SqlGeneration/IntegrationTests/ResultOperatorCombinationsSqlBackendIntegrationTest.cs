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
  public class ResultOperatorCombinationsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void DistinctAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Count(),
          "SELECT COUNT(*) FROM (SELECT DISTINCT [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]");
    }

    [Test]
    public void DistinctAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Take (5),
          "SELECT DISTINCT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void DistinctAndFirst ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct ().First(),
          "SELECT DISTINCT TOP (1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void DistinctAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct ().Single(),
          "SELECT DISTINCT TOP (2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void TakeAndDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Distinct(),
          "SELECT DISTINCT [q0].[value] AS [value] FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Count(),
          "SELECT COUNT(*) FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 5));
    }
[Test]
    public void TakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take (3),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (3).Take (5),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 3));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First(),
          "SELECT TOP (1) [q0].[value] AS [value] FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single(),
          "SELECT TOP (2) [q0].[value] AS [value] FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First (fn => fn != null),
          "SELECT TOP (1) [q0].[value] AS [value] FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single (fn => fn != null),
          "SELECT TOP (2) [q0].[value] AS [value] FROM (SELECT TOP (@1) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndTakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take(3).Take(7),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [q0].[value] AS [value] FROM (SELECT TOP (@3) [t2].[FirstName] AS [value] "+
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          new CommandParameter ("@1", 7),
          new CommandParameter ("@2", 3),
          new CommandParameter ("@3", 5));
    }

    [Test]
    public void TakeAndContains ()
    {
      Cook cook = new Cook { ID = 5, FirstName = "Hugo", Name = "Hanser" };
      CheckQuery (
          () => Cooks.Take (1).Contains (cook),
          "SELECT CASE WHEN @1 IN (SELECT TOP (@2) [t0].[ID] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END",
          new CommandParameter("@1", cook.ID),
          new CommandParameter("@2", 1)
          );
    }

    [Test]
    public void TakeAndCast ()
    {
      CheckQuery (
          () => Cooks.Take (1).Cast<object>(),
          "SELECT TOP (@1) [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
           +"FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1)
          );
    }

    [Test]
    public void TakeAndOfType ()
    {
      CheckQuery (
          () => Cooks.Take (1).OfType<Chef>(),
          "SELECT TOP (@1) [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          + "FROM [CookTable] AS [t0] WHERE ([t0].[IsStarredCook] = 1)",
          new CommandParameter ("@1", 1)
          );
    }

    [Test]
    public void All_AfterResultOperatorInducedSubquery ()
    {
      CheckQuery (
          () => (from s in Cooks select s).Take (10).Take (20).All (s => s.IsStarredCook),
        "SELECT CASE WHEN NOT EXISTS((SELECT TOP (@1) [q0].[ID] FROM (SELECT TOP (@2) [t1].[ID],[t1].[FirstName],[t1].[Name],[t1].[IsStarredCook],"+
        "[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID] FROM [CookTable] AS [t1]) AS [q0] "+
        "WHERE NOT ([q0].[IsStarredCook] = 1))) THEN 1 ELSE 0 END",
        new CommandParameter ("@1", 20),
        new CommandParameter ("@2", 10)
        );

      CheckQuery (
          () => (from s in Cooks select s.FirstName).Take (10).Take (20).All (s => s != null),
          "SELECT CASE WHEN NOT EXISTS((SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] "+
          "FROM [CookTable] AS [t1]) AS [q0] WHERE NOT ([q0].[value] IS NOT NULL))) THEN 1 ELSE 0 END",
          new CommandParameter ("@1", 20),
          new CommandParameter ("@2", 10)
        );
    }

    [Test]
    public void All_AfterDefaultIfEmpty ()
    {
      CheckQuery (
          () => (from s in Cooks select s).DefaultIfEmpty().All (s => s.IsStarredCook),
        "SELECT CASE WHEN NOT EXISTS((SELECT [q0].[ID] FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN (SELECT [t1].[ID],[t1].[FirstName],"+
        "[t1].[Name],[t1].[IsStarredCook],[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID] FROM [CookTable] AS [t1]) AS [q0] ON 1 = 1 "+
        "WHERE NOT ([q0].[IsStarredCook] = 1))) THEN 1 ELSE 0 END"
        );
    }

    [Test]
    public void DistinctAndSum_WithOrderBy ()
    {
      CheckQuery (
          () => (from s in Cooks orderby s.FirstName select s.ID).Distinct ().Sum (),
        "SELECT SUM([q0].[value]) FROM (SELECT DISTINCT [t1].[ID] AS [value] FROM [CookTable] AS [t1]) AS [q0]"
        );
    }

    [Test]
    public void TakeAfterSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).Take(5),
          "SELECT [q0].[Key] AS [Key] FROM (SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE (([q0].[Value] > @1) AND ([q0].[Value] <= (@2 + @3))) ORDER BY [q0].[Value] ASC",
          new CommandParameter("@1", 100),
          new CommandParameter("@2", 100),
          new CommandParameter("@3", 5));
    }

    [Test]
    public void SingleAfterSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).Single(),
          "SELECT [q0].[Key] AS [Key] FROM (SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE (([q0].[Value] > @1) AND ([q0].[Value] <= (@2 + 2))) ORDER BY [q0].[Value] ASC",
          new CommandParameter ("@1", 100),
          new CommandParameter ("@2", 100));
    }

    [Test]
    public void FirstAfterSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).First(),
          "SELECT [q0].[Key] AS [Key] FROM (SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] " +
          "FROM [CookTable] AS [t0]) AS [q0] WHERE (([q0].[Value] > @1) AND ([q0].[Value] <= (@2 + 1))) ORDER BY [q0].[Value] ASC",
          new CommandParameter ("@1", 100),
          new CommandParameter ("@2", 100));
    }

    [Test]
    public void AllAfterSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).All (name => name != null),
          "SELECT CASE WHEN NOT EXISTS(("
          + "SELECT [q0].[Key] AS [Key] FROM ("
          + "SELECT [t0].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] FROM [CookTable] AS [t0]"
          + ") AS [q0] WHERE (([q0].[Value] > @1) AND NOT ([q0].[Key] IS NOT NULL)))) THEN 1 ELSE 0 END",
          new CommandParameter ("@1", 100));
    }

    [Test]
    public void SkipTakeSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).Take (10).Skip(100),
          "SELECT [q1].[Key_Key] AS [Key] FROM (SELECT [q0].[Key] AS [Key_Key],"+
          "ROW_NUMBER() OVER (ORDER BY [q0].[Value] ASC) AS [Value] FROM (SELECT [t0].[FirstName] AS [Key],"+
          "ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] FROM [CookTable] AS [t0]) AS [q0] "+
          "WHERE (([q0].[Value] > @1) AND ([q0].[Value] <= (@2 + @3)))) AS [q1] WHERE ([q1].[Value] > @4) ORDER BY [q1].[Value] ASC",
          new CommandParameter ("@1", 100),
          new CommandParameter ("@2", 100),
          new CommandParameter ("@3", 10),
          new CommandParameter ("@4", 100));
    }

    [Test]
    public void SkipAfterSkip ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100).Skip (10),
          "SELECT [q1].[Key_Key] AS [Key] FROM (SELECT [q0].[Key] AS [Key_Key],"+
          "ROW_NUMBER() OVER (ORDER BY [q0].[Value] ASC) AS [Value] FROM (SELECT [t0].[FirstName] AS [Key],"+
          "ROW_NUMBER() OVER (ORDER BY [t0].[Name] ASC) AS [Value] FROM [CookTable] AS [t0]) AS [q0] WHERE ([q0].[Value] > @1)) AS [q1] "+
          "WHERE ([q1].[Value] > @2) ORDER BY [q1].[Value] ASC",
          new CommandParameter ("@1", 100),
          new CommandParameter ("@2", 10));
    }

    [Test]
    public void AnyAfterGroupBy ()
    {
      CheckQuery (() => (from c in Cooks group c.Name by c.FirstName).Any (group => group.Key != null),
        "SELECT CASE WHEN EXISTS(("
            + "SELECT [q0].[key] AS [key] FROM (SELECT [t1].[FirstName] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[FirstName]) AS [q0] "
            + "WHERE ([q0].[key] IS NOT NULL))) THEN 1 ELSE 0 END");
    }

    [Test]
    public void AllAfterGroupBy ()
    {
      CheckQuery (() => (from c in Cooks group c.Name by c.FirstName).All (group => group.Key != null),
       "SELECT CASE WHEN NOT EXISTS(("
           + "SELECT [q0].[key] AS [key] FROM (SELECT [t1].[FirstName] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[FirstName]) AS [q0] "
           + "WHERE NOT ([q0].[key] IS NOT NULL))) THEN 1 ELSE 0 END");
    }

    [Test]
    public void AllAfterGroupBy_WithResultSelector ()
    {
      CheckQuery (
        () => Cooks.GroupBy (c => c.Name, c => c.ID, (key, group) => new { Name = key }).All (x => x.Name != null),
        "SELECT CASE WHEN NOT EXISTS(("
        + "SELECT [q0].[key] AS [Name_key] "
        + "FROM (SELECT [t1].[Name] AS [key] FROM [CookTable] AS [t1] GROUP BY [t1].[Name]) AS [q0] "
        + "WHERE NOT ([q0].[key] IS NOT NULL))) "
        + "THEN 1 ELSE 0 END");
    }
  }
}