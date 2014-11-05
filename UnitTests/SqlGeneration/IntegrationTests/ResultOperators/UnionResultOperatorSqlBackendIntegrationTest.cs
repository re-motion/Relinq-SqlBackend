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
  public class UnionResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    [Ignore("TODO RMLNQSQL-30")]
    public void Union_OnTopLevel ()
    {
      CheckQuery (
          () => Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Union (Cooks.Where (c => c.Name == "Boss").Select(c => c.ID)),
          "SELECT [t0].[ID] AS [ID] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1) "
          + "UNION SELECT [t1].[ID] AS [ID] FROM [CookTable] AS [t1] WHERE ([t1].[FirstName] = @2)",
          row => (object) row.GetValue<int> (new ColumnID ("ID", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }

    [Test]
    [Ignore("TODO RMLNQSQL-30")]
    public void Union_InSubQuery ()
    {
      CheckQuery (
          () => from k in Cooks
                from x in (Cooks.Where(c => c.FirstName == "Hugo").Select(c => c.ID).Union (Cooks.Where (c => c.Name == "Boss").Select(c => c.ID)))
                where k.ID == x
                select x,
          "TODO",
          row => (object) row.GetValue<int> (new ColumnID ("ID", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "Boss"));
    }
  }
}