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
  public class OrderByClausesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {

    [Test]
    public void SimpleOrderBy_OneOrderByClause ()
    {
      CheckQuery (
         from s in Cooks orderby s.Name select s.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC");
    }

    [Test]
    public void SimpleOrderBy_SeveralOrderByClauses ()
    {
      CheckQuery (
         from s in Cooks orderby s.Name, s.FirstName descending, s.Weight select s.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC, [t0].[FirstName] DESC, [t0].[Weight] ASC");
    }

    [Test]
    public void OrderBy_WithConstantexpression ()
    {
      CheckQuery (
         from s in Cooks orderby 1 select s.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] ORDER BY (SELECT @1) ASC",
         new CommandParameter("@1", 1));
    }

  }
}