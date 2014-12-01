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
  public class DefaultIfEmptyResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void DefaultIfEmpty ()
    {
      CheckQuery (
          Cooks.DefaultIfEmpty (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],"
          + "[t0].[KnifeID],[t0].[KnifeClassID],[t0].[CookRating] " 
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t0] ON (1 = 1)",
           row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("KnifeID", 7),
              new ColumnID ("KnifeClassID", 8),
              new ColumnID ("CookRating", 9)));
    }

    [Test]
    public void DefaultIfEmpty_WithDefaultValue ()
    {
      Assert.That (
          () => CheckQuery (Cooks.DefaultIfEmpty (null), "..."),
          Throws.TypeOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The DefaultIfEmpty operator is not supported if a default value is specified. Use the overload without a specified default value."));
    }

    [Test]
    public void DefaultIfEmpty_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).DefaultIfEmpty().Max() > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT MAX([t1].[ID]) AS [value] FROM " +
          "(SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [t1] ON (1 = 1)) > @1)",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InFromClause ()
    {
      CheckQuery (
          from c in Cooks
          from k in Kitchens.Where(k => k == c.Kitchen).DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_AsLeftJoin_InJoinClause ()
    {
      CheckQuery (
          from c in Cooks
          join k in Kitchens on c.Kitchen equals k into kitchens
          from k in kitchens.DefaultIfEmpty()
          select new { CookID = c.ID, KitchenID = k.ID },
          "SELECT [t1].[ID] AS [CookID],[q0].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS APPLY ("
          + "SELECT [t2].[ID],[t2].[Name],[t2].[RestaurantID],[t2].[LastCleaningDay],[t2].[PassedLastInspection],[t2].[LastInspectionScore] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t1].[KitchenID] = [t2].[ID])"
          + ") AS [q0]");
    }

    [Test]
    public void DefaultIfEmpty_WithMultipleTables ()
    {
      CheckQuery (
          (from c in Cooks
            from k in Kitchens
            where k == c.Kitchen
            select new { CookID = c.ID, KitchenID = k.ID }).DefaultIfEmpty(),
          "SELECT [q0].[CookID] AS [CookID],[q0].[KitchenID] AS [KitchenID] "
          + "FROM (SELECT NULL AS [Empty]) AS [Empty] "
          + "LEFT OUTER JOIN ("
          + "SELECT [t1].[ID] AS [CookID],[t2].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t1] "
          + "CROSS JOIN [KitchenTable] AS [t2] "
          + "WHERE ([t2].[ID] = [t1].[KitchenID])"
          + ") AS [q0] ON (1 = 1)");
    }
  }
}