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
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class ConditionalExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    //test conditional in Where expressions and Select expressions (as indicated in the task description)
    [Test]
    public void ConditionalExpressionInWhereClause ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName == (c.FirstName == "Hugo" ? "test1" : "test2") select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE "
          + "([t0].[FirstName] = CASE WHEN ([t0].[FirstName] = @1) THEN @2 ELSE @3 END)",
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "test1"),
          new CommandParameter ("@3", "test2"));
    }

    [Test]
    public void ConditionalExpressionInSelectClause ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName == "Hugo" ? "test1" : "test2",
          "SELECT CASE WHEN ([t0].[FirstName] = @1) THEN @2 ELSE @3 END AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Hugo"),
          new CommandParameter ("@2", "test1"),
          new CommandParameter ("@3", "test2"));
    }
  }
}