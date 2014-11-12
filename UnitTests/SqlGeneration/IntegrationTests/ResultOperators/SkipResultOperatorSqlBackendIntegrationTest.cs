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
  public class SkipResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Skip_WithEntity ()
    {
      CheckQuery (
          () => (from r in Restaurants orderby r.ID select r).Skip (5),
          "SELECT [q0].[Key_ID] AS [ID],[q0].[Key_CompanyID] AS [CompanyID] "+
          "FROM (SELECT [t1].[ID] AS [Key_ID],[t1].[CompanyID] AS [Key_CompanyID],"+
          "ROW_NUMBER() OVER (ORDER BY [t1].[ID] ASC) AS [Value] FROM [RestaurantTable] AS [t1]) AS [q0] "+
          "WHERE ([q0].[Value] > @1) ORDER BY [q0].[Value] ASC",
           row => (object) row.GetEntity<Restaurant> (
              new ColumnID ("ID", 0),
              new ColumnID ("CompanyID", 1)),
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void Skip_WithEntity_WithoutExplicitOrdering ()
    {
      CheckQuery (
          () => (from r in Restaurants select r).Skip (5),
          "SELECT [q0].[Key_ID] AS [ID],[q0].[Key_CompanyID] AS [CompanyID] "+
          "FROM (SELECT [t1].[ID] AS [Key_ID],[t1].[CompanyID] AS [Key_CompanyID],"+
          "ROW_NUMBER() OVER (ORDER BY (SELECT @1) ASC) AS [Value] FROM [RestaurantTable] AS [t1]) AS [q0] "+
          "WHERE ([q0].[Value] > @2) ORDER BY [q0].[Value] ASC",
          new CommandParameter("@1", 1),
          new CommandParameter("@2", 5));
    }

    [Test]
    public void Skip_WithColumn ()
    {
      CheckQuery (
          () => (from c in Cooks orderby c.Name select c.FirstName).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT [t1].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY [t1].[Name] ASC) AS [Value] " +
          "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[Value] > @1) ORDER BY [q0].[Value] ASC",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 100));
    }

    [Test]
    public void Skip_WithColumn_WithoutExplicitOrdering ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT [t1].[FirstName] AS [Key],ROW_NUMBER() OVER (ORDER BY (SELECT @1) ASC) AS [Value] " +
          "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[Value] > @2) ORDER BY [q0].[Value] ASC",
          new CommandParameter ("@1", 1),
          new CommandParameter("@2", 100));
    }

    [Test]
    public void Skip_WithConstant ()
    {
      CheckQuery (
          () => (from c in Cooks orderby 20 select 10).Skip (100),
          "SELECT [q0].[Key] AS [value] FROM (SELECT @1 AS [Key],ROW_NUMBER() OVER (ORDER BY (SELECT @2) ASC) AS [Value] " +
          "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[Value] > @3) ORDER BY [q0].[Value] ASC",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10),
          new CommandParameter ("@2", 20),
          new CommandParameter ("@3", 100));
    }
  }
}