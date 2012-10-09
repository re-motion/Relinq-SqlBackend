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
  public class PartialEvaluationSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    [Ignore ("TODO 4771")]
    public void NullValue_InEvaluatableSubExpression ()
    {
      string nullValue = null;
      CheckQuery (
          from c in Cooks where nullValue.Length > c.ID select c.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE LEN(@0) > [t0].[ID]",
          new CommandParameter ("@0", null));

      CheckQuery (
          from c in Cooks where nullValue != null && nullValue.Length > c.ID select c,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE @0 IS NOT NULL AND LEN(@0) > [t0].[ID]",
          new CommandParameter ("@0", null));
    }
  }
}