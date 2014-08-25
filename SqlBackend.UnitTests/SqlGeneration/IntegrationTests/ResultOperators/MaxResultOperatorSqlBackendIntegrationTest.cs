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
  public class MaxResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Max_OnTopLevel ()
    {
      CheckQuery (
          () => (from c in Cooks select c.ID).Max(),
          "SELECT MAX([t0].[ID]) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    public void Max_InSubquery ()
    {
      CheckQuery (
           from s in Cooks where (from s2 in Cooks select s2.ID).Max() > 5 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ((SELECT MAX([t1].[ID]) AS [value] FROM [CookTable] AS [t1]) > @1)",
          new CommandParameter("@1", 5));
    }
  }
}