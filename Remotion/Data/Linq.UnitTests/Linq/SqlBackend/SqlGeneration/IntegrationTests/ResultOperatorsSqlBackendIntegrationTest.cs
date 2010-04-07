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
          "SELECT COUNT(*) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithProperty ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count(),
          "SELECT COUNT(*) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count (name => name != null),
          "SELECT COUNT(*) FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)");
    }

    [Test]
    public void SimpleDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct(),
          "SELECT DISTINCT [t0].[FirstName] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void SimpleTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
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
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void First_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Single ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Single_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
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
          "SELECT CASE WHEN @1 IN (SELECT [t0].[ID] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END",
          new CommandParameter("@1", 23) );
    }

    [Test]
    public void Contains_WithConstantCollection ()
    {
      var cookNames = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          (from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName),
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Cast_TopLevel ()
    {
      CheckQuery (
          (from s in Cooks select s.FirstName).Cast<string>(),
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0]");
    }

    [Test]
    [Ignore("TODO: 2542")]
    public void Cast_SubQuery ()
    {
      CheckQuery (
          (from r in (from c in Cooks select c.FirstName).Cast<string>() select r),
          "SELECT [q0].[value] FROM (SELECT [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]");
      
    }


    
  }
}