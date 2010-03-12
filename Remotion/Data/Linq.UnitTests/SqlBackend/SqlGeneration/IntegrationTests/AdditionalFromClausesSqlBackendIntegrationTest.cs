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
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class AdditionalFromClausesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleAdditionalFromClause_TwoTables ()
    {
      CheckQuery (
         from s in Cooks from k in Kitchens select s.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1]");
    }

    [Test]
    public void SimpleAdditionalFromClause_ThreeTables ()
    {
      CheckQuery (
         from s in Cooks from k in Kitchens from r in Restaurants select k.Name,
         "SELECT [t1].[Name] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1] CROSS JOIN [RestaurantTable] AS [t2]");
    }

    [Test]
    public void SimpleAdditionalFromClause_WithJoins ()
    {
      CheckQuery (
         from s in Cooks from k in Kitchens where s.Substitution.Name=="Hugo" select k.Cook.FirstName,
         "SELECT [t3].[FirstName] "
         + "FROM [CookTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[SubstitutedID] "
         + "CROSS JOIN [KitchenTable] AS [t2] JOIN [CookTable] AS [t3] ON [t2].[ID] = [t3].[KitchenID] "
         + "WHERE ([t1].[Name] = @1)",
         new CommandParameter("@1", "Hugo")
         );
    }

    [Test]
    [Ignore ("TODO 2403")]
    public void AdditionalFromClause_WithMemberAccess ()
    {
      CheckQuery (
         from s in Cooks from a in s.Assistants select a.Name,
         "SELECT [t1].[Name] "
         + "FROM [CookTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[AssistedID] ",
         new CommandParameter ("@1", "Hugo")
         );
    }

    [Test]
    [Ignore ("TODO 2403")]
    public void AdditionalFromClause_WithMemberAccess_AndCrossJoin ()
    {
      CheckQuery (
         from s in Cooks from a in s.Assistants from r in Restaurants from c in r.Cooks where a.Name != null select c.Name,
         "SELECT [t3].[Name] "
         + "FROM [CookTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[AssistedID] "
         + "CROSS JOIN [RestaurantTable] AS [t2] JOIN [CookTable] AS [t3] ON [t2].[ID] = [t3].[RestuarantID] "
         + "WHERE ([t1].[Name] IS NOT NULL)"
         );
    }
    
  }
}