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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests.ResultOperators
{
  [TestFixture]
  public class CastResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Cast_TopLevel_OnEntity ()
    {
      CheckQuery (
          (from s in Cooks select s).Cast<Chef> (),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],"
          + "[t0].[KnifeID],[t0].[KnifeClassID] "
          + "FROM [CookTable] AS [t0]",
           row => (object) (Chef) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("KnifeID", 7),
              new ColumnID ("KnifeClassID", 8)));
    }

    [Test]
    public void Cast_SubQuery ()
    {
      CheckQuery (
          (from r in (from c in Cooks select c.FirstName).Cast<object>() select r),
          "SELECT [q0].[value] AS [value] FROM (SELECT [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0]");
    }
  }
}