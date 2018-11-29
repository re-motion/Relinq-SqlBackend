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
  public class FirstResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
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
    [Ignore ("RMLNQSQL-133")]
    public void First_WithEntityExpression_RMLNQSQL_133 ()
    {
      CheckQuery (
          Cooks.Where (c => c.Assistants.First().CookRating == CookRating.Regular),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],[t0].[KnifeID],[t0].[KnifeClassID],[t0].[CookRating] "
          + "FROM [CookTable] AS [t0] "
          + "WHERE ((SELECT TOP (1) [t1].[CookRating] AS [value] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID])) = @1)",
          new CommandParameter ("@1", 0));
      CheckQuery (
          Cooks.Where (c => c.Assistants.FirstOrDefault().CookRating == CookRating.Regular),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],[t0].[KnifeID],[t0].[KnifeClassID],[t0].[CookRating] "
          + "FROM [CookTable] AS [t0] "
          + "WHERE ((SELECT TOP (1) [t1].[CookRating] AS [value] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID])) = @1)",
          new CommandParameter ("@1", 0));
    }
  }
}