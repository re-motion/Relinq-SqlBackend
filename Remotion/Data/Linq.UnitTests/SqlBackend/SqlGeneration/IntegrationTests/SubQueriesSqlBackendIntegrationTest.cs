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
  public class SubQueriesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void InWhereCondition_First ()
    {
      CheckQuery (
         from c in Cooks where c.Name == (from a in Cooks select a.FirstName).First() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t1].[FirstName] FROM [CookTable] AS [t1]))",
         new CommandParameter("@1", 1));
    }

    [Test]
    public void InWhereCondition_Single ()
    {
      CheckQuery (
         from c in Cooks where c.Name == (from k in Kitchens select k.Name).Single () select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t1].[Name] FROM [KitchenTable] AS [t1]))",
         new CommandParameter ("@1", 1));
    }

    [Test]
    public void InWhereCondition_Count ()
    {
      CheckQuery (
         from c in Cooks where c.ID == (from k in Kitchens select k).Count() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = (SELECT COUNT(*) FROM [KitchenTable] AS [t1]))");
    }

    [Test]
    public void InSelectProjection ()
    {
      CheckQuery (
          from c in Cooks select (from k in Kitchens select k.Name),
         "SELECT (SELECT [t1].[Name] FROM [KitchenTable] AS [t1]) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void InOrderByClause ()
    {
      CheckQuery (
          from c in Cooks orderby (from k in Kitchens select k).Count() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] ORDER BY (SELECT COUNT(*) FROM [KitchenTable] AS [t1]) ASC");
    }

    [Test]
    public void SubQueryInSubQuery ()
    {
      CheckQuery (
          from c in Cooks where c.ID== (from k in Kitchens where k.ID == (from r in Restaurants select r).Count() select k).Count () select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] "
        +"WHERE ([t0].[ID] = (SELECT COUNT(*) FROM [KitchenTable] AS [t1] WHERE ([t1].[ID] = (SELECT COUNT(*) FROM [RestaurantTable] AS [t2]))))");
    }

    [Test]
    [Ignore("TODO: Subquery with Contains")]
    public void Contains ()
    {
    }
  }
}