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
          "SELECT COUNT(*) FROM (SELECT DISTINCT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]");
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
          "SELECT DISTINCT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void DistinctAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct ().Single(),
          "SELECT DISTINCT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 2));
    }

    [Test]
    public void TakeAndDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Distinct(),
          "SELECT DISTINCT [q0].[value] AS [value] FROM (SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Count(),
          "SELECT COUNT(*) FROM (SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take (3),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (3).Take (5),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q1]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 3));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First(),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single(),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q1]",
          new CommandParameter ("@1", 2),
          new CommandParameter ("@2", 5));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First (fn => fn != null),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single (fn => fn != null),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 2),
          new CommandParameter ("@2", 5));
    }

    [Test]
    public void TakeAndTakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take(3).Take(7),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [q0].[value] AS [value] FROM (SELECT TOP (@3) [t0].[FirstName] AS [value] " +
          "FROM [CookTable] AS [t0]) AS [q0]) AS [q1]",
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
          "SELECT CASE WHEN @1 IN (SELECT TOP (@2) [t0].[ID] AS [value] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END AS [value]",
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
        "SELECT CASE WHEN NOT EXISTS((SELECT TOP (@1) [q0].[ID] AS [value] FROM (SELECT TOP (@2) [t0].[ID],[t0].[FirstName],[t0].[Name]," +
        "[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] FROM [CookTable] AS [t0]) AS [q0] " +
        "WHERE NOT ([q0].[IsStarredCook] = 1))) THEN 1 ELSE 0 END AS [value]",
        new CommandParameter ("@1", 20),
        new CommandParameter ("@2", 10)
        );

      CheckQuery (
          () => (from s in Cooks select s.FirstName).Take (10).Take (20).All (s => s != null),
          "SELECT CASE WHEN NOT EXISTS((SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] "+
          "FROM [CookTable] AS [t0]) AS [q1] WHERE NOT ([q1].[value] IS NOT NULL))) THEN 1 ELSE 0 END AS [value]",
          new CommandParameter ("@1", 20),
          new CommandParameter ("@2", 10)
        );
    }

    [Test]
    public void All_AfterDefaultIfEmpty ()
    {
      CheckQuery (
          () => (from s in Cooks select s).DefaultIfEmpty().All (s => s.IsStarredCook),
        "SELECT CASE WHEN NOT EXISTS((SELECT [q0].[ID] AS [value] FROM (SELECT NULL AS [Empty]) AS [Empty] "
        + "LEFT OUTER JOIN (SELECT [t0].[ID],[t0].[FirstName],[t0].[Name]," +
        "[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] FROM [CookTable] AS [t0]) AS [q0] ON 1 = 1 " +
        "WHERE NOT ([q0].[IsStarredCook] = 1))) THEN 1 ELSE 0 END AS [value]"
        );
    }

  }
}