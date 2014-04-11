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
  public class TakeResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5),
          "SELECT TOP (5) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void TakeWithMemberExpression ()
    {
      CheckQuery (
          () => (from k in Kitchens from c in k.Restaurant.Cooks.Select (c => c.ID).Take (k.RoomNumber) select k.Name),
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t2] ON ([t1].[RestaurantID] = [t2].[ID]) "
          + "CROSS APPLY (SELECT TOP ([t1].[RoomNumber]) [t3].[ID] AS [value] "
          + "FROM [CookTable] AS [t3] WHERE ([t2].[ID] = [t3].[RestaurantID])) AS [q0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void TakeWithSubQuery ()
    {
      CheckQuery (
          () => ((from c in Cooks select c.FirstName).Take ((from k in Kitchens select k).Count ())),
          "SELECT TOP ((SELECT COUNT(*) AS [value] FROM [KitchenTable] AS [t1])) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }
  }
}