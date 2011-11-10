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

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class OrderByClausesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void OneOrderByClause ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC");
    }

    [Test]
    public void SeveralOrderings ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name , s.FirstName descending , s.Weight select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC, [t0].[FirstName] DESC, [t0].[Weight] ASC");
    }

    [Test]
    public void SeveralOrderByClauses ()
    {
      CheckQuery (
          from s in Cooks
          orderby s.Name , s.FirstName descending , s.Weight
          orderby s.ID , s.IsFullTimeCook descending
          select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "ORDER BY [t0].[ID] ASC, [t0].[IsFullTimeCook] DESC, [t0].[Name] ASC, [t0].[FirstName] DESC, [t0].[Weight] ASC");
    }

    [Test]
    public void SeveralOrderByClauses_SeparatedByOtherClauses ()
    {
      CheckQuery (
          from s in Cooks
          orderby s.Name
          where s.FirstName != null
          orderby s.ID
          select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "WHERE ([t0].[FirstName] IS NOT NULL) "
          + "ORDER BY [t0].[ID] ASC, [t0].[Name] ASC");
    }

    [Test]
    public void WithConstantExpression ()
    {
      CheckQuery (
          from s in Cooks orderby 1 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY (SELECT @1) ASC",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void WithConstantExpression_InComplexExpression ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name + " " + s.FirstName select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY (([t0].[Name] + @1) + [t0].[FirstName]) ASC",
          new CommandParameter ("@1", " "));
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InSelectClause_WithoutTopExpression ()
    {
      CheckQuery (
          from c in Cooks select (from k in Kitchens orderby k.Name where k.Cook==c select k).Count(),
          "SELECT (SELECT COUNT(*) AS [value] FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[KitchenID] "+
          "WHERE ([t2].[ID] = [t0].[ID])) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InSelectClause_WithTopExpression ()
    {
      CheckQuery (
         from c in Cooks select (from k in Kitchens orderby k.Name where k.Cook == c select k).Single(),
         "SELECT [q3].[ID],[q3].[CookID],[q3].[Name],[q3].[RestaurantID],[q3].[SubKitchenID],[q3].[LastCleaningDay] "
          + "FROM [CookTable] AS [t0] "
          + "CROSS APPLY "
          + "(SELECT TOP (1) [t1].[ID],[t1].[CookID],[t1].[Name],[t1].[RestaurantID],[t1].[SubKitchenID],[t1].[LastCleaningDay] "
          + "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN "
          +"[CookTable] AS [t2] ON [t1].[ID] = [t2].[KitchenID] WHERE ([t2].[ID] = [t0].[ID]) ORDER BY [t1].[Name] ASC) AS [q3]");
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InWhereClause_WithTopExpression ()
    {
      CheckQuery (
          from c in Cooks
          where (from sc in Cooks orderby sc.Name select sc.Name).Single() != null
          select c,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "+
          "FROM [CookTable] AS [t0] WHERE ((SELECT TOP (2) [t1].[Name] AS [value] FROM [CookTable] AS [t1] ORDER BY [t1].[Name] ASC) IS NOT NULL)");
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InWhereClause_WithoutTopExpression ()
    {
      CheckQuery (
          (from c in Cooks
          where (from sc in Cooks orderby sc.Name select sc).Count()>0
          select c),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "+
          "FROM [CookTable] AS [t0] WHERE ((SELECT COUNT(*) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter("@1", 0));
    }
  }
}