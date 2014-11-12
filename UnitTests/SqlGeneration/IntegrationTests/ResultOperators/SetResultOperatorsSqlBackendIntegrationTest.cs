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
  public class SetResultOperatorsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
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
    public void Concat_OnTopLevel ()
    {
      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID).Concat (Cooks.Where (c => c.Name == "Boss").Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION ALL (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));

      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID)
              .Concat (Cooks.Where (c => c.Name == "Boss").Select (c => c.ID))
              .Concat (Cooks.Where (c => c.ID == 100).Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION ALL (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2)) "
          + "UNION ALL (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[ID] = @3))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"),
          new CommandParameter ("@3", 100));

      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").Select (c => c.ID)
              .Concat (
                  Cooks.Where (c => c.Name == "Boss").Select (c => c.ID)
                      .Concat (Cooks.Where (c => c.ID == 100).Select (c => c.ID))),
          // The difference is in the parentheses.
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION ALL (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2) "
          + "UNION ALL (SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[ID] = @3)))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"),
          new CommandParameter ("@3", 100));
    }

    [Test]
    public void SetOperation_WithDifferentTypes ()
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

    [Test]
    public void SetOperation_CausesOrderByToBeIgnored ()
    {
      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").OrderBy (c => c.ID).Select (c => c.ID)
              .Union (Cooks.Where (c => c.Name == "Boss").OrderBy (c => c.ID).Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));

      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").OrderBy (c => c.ID).Select (c => c.ID)
              .Concat (Cooks.Where (c => c.Name == "Boss").OrderBy (c => c.ID).Select (c => c.ID)),
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION ALL (SELECT [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = @2))",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }

    [Test]
    public void SetOperation_CausesOrderByWithTakeToWork ()
    {
      CheckQuery (
          () => Cooks.Where (c => c.FirstName == "Hugo").OrderBy (c => c.ID).Select (c => c.ID).Take(3)
              .Union (Cooks.Where (c => c.Name == "Boss").OrderBy (c => c.ID).Select (c => c.ID).Take(2)),
          "SELECT [q0].[value] AS [value] "
          + "FROM ("
          + "SELECT TOP (3) [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[FirstName] = @1) ORDER BY [t1].[ID] ASC "
          + "UNION (SELECT TOP (2) [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[Name] = @2) ORDER BY [t2].[ID] ASC)"
          + ") AS [q0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }

    [Test]
    [Ignore("TODO RMLNQSQL-63: This should really throw an error, but it generates invalid SQL.")]
    public void SetOperation_WithDifferentColumnLists ()
    {
      CheckQuery (
          () => Cooks.Union (Chefs.Select (c => c)),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],"
          + "[t0].[KnifeID],[t0].[KnifeClassID] "
          + "FROM [CookTable] AS [t0] "
          + "UNION (SELECT [t1].[ID],[t1].[FirstName],[t1].[Name],[t1].[IsStarredCook],[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID],"
          + "[t1].[KnifeID],[t1].[KnifeClassID],[t1].[LetterOfRecommendation] "
          + "FROM [dbo].[ChefTable] AS [t1])");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "In-memory method calls are not supported when a set operation (such as Union or Concat) is used. "
        + "Rewrite the query to perform the in-memory operation after the set operation has been performed.\r\n"
        + "For example, instead of the following query:\r\n"
        + "    SomeOrders.Select (o => SomeMethod (o.ID)).Concat (OtherOrders.Select (o => SomeMethod (o.ID)))\r\n"
        + "Try the following query:\r\n"
        + "    SomeOrders.Select (o => o.ID).Concat (OtherOrders.Select (o => o.ID)).Select (i => SomeMethod (i))")]
    public void SetOperation_WithDifferentInMemoryProjections ()
    {
      CheckQuery (
          () => Cooks.Select(c => InMemoryToUpper (c.Name)).Union (Kitchens.Select (c => c.Name)),
          "SELECT [t0].[Name] AS [Arg0] FROM [CookTable] AS [t0] UNION (SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t1])");
    }

    [Test]
    public void SetOperation_WithSubsequentInMemoryProjection ()
    {
      CheckQuery (
          () => Cooks.Select (c => c.Name).Union (Kitchens.Select (c => c.Name)).Select (n => InMemoryToUpper (n)),
          "SELECT [q0].[value] AS [Arg0] FROM ("
          + "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t1] UNION (SELECT [t2].[Name] AS [value] FROM [KitchenTable] AS [t2])"
          + ") AS [q0]",
          row => (object) InMemoryToUpper (row.GetValue<string> (new ColumnID("Arg0", 0))));
    }

    private static string InMemoryToUpper (string id)
    {
      return id.ToUpper();
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException), ExpectedMessage = 
        "The 'Union' operation is only supported for combining two query results, but a 'ConstantExpression' was supplied as the second sequence: "
        + "value(System.Int32[])")]
    public void SetOperation_WithCollection ()
    {
      CheckQuery (
          () => Cooks.Select(c => c.ID).Union (new[] { 1, 2, 3}),
          "not supported");
    }
    
    [Test]
    public void SetOperation_InSubQuery ()
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

       CheckQuery (
          () => from k in Cooks
                from x in (Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Concat (Cooks.Where (c => c.Name == "Boss").Select(c => c.ID)))
                where k.ID == x
                select x,
          "SELECT [q0].[value] AS [value] " 
          + "FROM [CookTable] AS [t1] " 
          + "CROSS APPLY ("
          + "SELECT [t2].[ID] AS [value] FROM [CookTable] AS [t2] WHERE ([t2].[FirstName] = @1) "
          + "UNION ALL (SELECT [t3].[ID] AS [value] FROM [CookTable] AS [t3] WHERE ([t3].[Name] = @2))) " 
          + "AS [q0] "
          + "WHERE ([t1].[ID] = [q0].[value])",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }
  }
}