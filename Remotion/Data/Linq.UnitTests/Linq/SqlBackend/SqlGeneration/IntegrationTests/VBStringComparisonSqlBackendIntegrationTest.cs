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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class VBStringComparisonSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void VBCompareStringExpression ()
    {
      var query = from c in Cooks select c.Name;
      var queryModel = ExpressionHelper.ParseQuery (query.Expression);
      var vbCompareStringExpression =
          new VBStringComparisonExpression (Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string2")), true);
      queryModel.BodyClauses.Add (new WhereClause (vbCompareStringExpression));

      CheckQuery (
          queryModel,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE (CASE WHEN (@1 = @2) THEN 1 ELSE 0 END = 1)",
          new CommandParameter ("@1", "string1"),
          new CommandParameter ("@2", "string2"));
    }
  }
}