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

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.MethodCalls
{
  [TestFixture]
  public class EqualsMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Equals ()
    {
      CheckQuery (from c in Cooks where c.Name.Equals ("abc") select c.Name, 
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
        new CommandParameter("@1", "abc"));

      CheckQuery (from k in Kitchens where k.Equals (k.Restaurant.SubKitchen) select k.Name,
          "SELECT [t0].[Name] AS [value] "
          + "FROM [KitchenTable] AS [t0] "
          + "LEFT OUTER JOIN [RestaurantTable] AS [t1] "
          + "LEFT OUTER JOIN [KitchenTable] AS [t2] "
          + "ON ([t1].[ID] = [t2].[RestaurantID]) "
          + "ON ([t0].[RestaurantID] = [t1].[ID]) "
          + "WHERE ([t0].[ID] = [t2].[ID])");

      CheckQuery (from c in Cooks where Equals(c.Name, "abc") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
        new CommandParameter ("@1", "abc"));

      CheckQuery (from c in Cooks where Equals (c, c.Substitution) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON ([t0].[ID] = [t1].[SubstitutedID]) "+
        "WHERE ([t0].[ID] = [t1].[ID])");

      CheckQuery (from c in Cooks where Equals (c, null) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] IS NULL)");

      CheckQuery (from c in Cooks where c.ID.Equals (10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", 10));
    }

    [Test]
    public void Equals_WithNonMatchingTypes ()
    {
      CheckQuery (from c in Cooks where c.ID.Equals ((int?) 10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", (int?) 10));

      CheckQuery (from c in Cooks where c.ID.Equals ("10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", "10"));

      CheckQuery (from c in Cooks where c.Substitution.Equals ("10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON ([t0].[ID] = [t1].[SubstitutedID]) WHERE ([t1].[ID] = @1)",
        new CommandParameter ("@1", "10"));

      CheckQuery (from c in Cooks where Equals (c.ID, "10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", "10"));

      CheckQuery (from c in Cooks where Equals (c.ID, (int?)10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", (int?) 10));

      CheckQuery (from c in Cooks where Equals (c.Substitution, "10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON ([t0].[ID] = [t1].[SubstitutedID]) WHERE ([t1].[ID] = @1)",
        new CommandParameter ("@1", "10"));
    }
  }
}