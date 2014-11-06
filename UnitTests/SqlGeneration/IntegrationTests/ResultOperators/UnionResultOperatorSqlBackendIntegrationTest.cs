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

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.ResultOperators
{
  [TestFixture]
  public class UnionResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Union_OnTopLevel ()
    {
      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID).Union (Cooks.Where (c => c.Name == "Boss").Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));

      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID)
              .Union (Cooks.Where (c => c.Name == "Boss").Select (c => c.ID))
              .Union (Cooks.Where (c => c.ID == 100).Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2)) "
          + "UNION (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[ID] = @3))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"),
          new CommandParameter ("@3", 100));

      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID)
              .Union (
                  Cooks.Where (c => c.Name == "Boss").Select (c => c.ID)
                      .Union (Cooks.Where (c => c.ID == 100).Select (c => c.ID))),
          // The difference is in the parentheses.
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2) "
          + "UNION (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[ID] = @3)))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"),
          new CommandParameter ("@3", 100));
    }

    [Test]
    public void Union_WithDifferentTypes ()
    {
      CheckQuery (
          () => Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Union (Chefs.Where (c => c.Name == "Boss").Select(c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [dbo].[ChefTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));

      CheckQuery (
          () => Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Union (Kitchens.Where (c => c.Name == "Nino's Kitchen").Select(c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [KitchenTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Nino's Kitchen"));
    }

    // TODO RMLNQSQL-30: Error cases? E.g., when selecting Cook and Chef as entities, when producing different projections, etc.
    // TODO RMLNQSQL-30: Not supported: Union with non-sub-query.

    [Test]
    public void Union_InSubQuery ()
    {
      CheckQuery (
          () => from k in Cooks
                from x in (Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Union (Cooks.Where (c => c.Name == "Boss").Select(c => c.ID)))
                where k.ID == x
                select x,
          "SELECT [q0].[value] AS [value] " 
          + "FROM [CookTable] AS [t1] " 
          + "CROSS APPLY ("
          + "SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[FirstName] = @1) "
          + "UNION (SELECT [t3].[ID] AS [value] FROM [CookTable] AS [t3] WHERE ([t3].[Name] = @2))) " 
          + "AS [q0] "
          + "WHERE ([t1].[ID] = [q0].[value])",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }
  }
}