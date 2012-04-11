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

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class WhereConditionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void BooleanColumn ()
    {
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] = 1)"
          );
    }

    [Test]
    public void True ()
    {
      CheckQuery (
          from c in Cooks where true select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter ("@1", 1)
          );
    }

    [Test]
    public void False ()
    {
      CheckQuery (
          from c in Cooks where false select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void BinaryExpression ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName == "hugo" select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = @1)",
          new CommandParameter ("@1", "hugo")
          );
    }
  }
}