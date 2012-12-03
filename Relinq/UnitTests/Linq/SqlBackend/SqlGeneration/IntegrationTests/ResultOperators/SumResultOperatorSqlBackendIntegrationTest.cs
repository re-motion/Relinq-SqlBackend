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

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests.ResultOperators
{
  [TestFixture]
  public class SumResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
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
          + "[t1].[LastCleaningDay] AS [Key_LastCleaningDay],[t1].[PassedLastInspection] AS [Key_PassedLastInspection],"
          + "[t1].[LastInspectionScore] AS [Key_LastInspectionScore],[t1].[Name] AS [Value] "
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
  }
}