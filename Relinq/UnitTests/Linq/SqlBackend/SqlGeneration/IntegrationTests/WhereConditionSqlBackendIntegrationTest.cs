// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
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

    [Test]
    public void InvocationExpression_Inline ()
    {
      CheckQuery (
          Cooks.Where (c => ((Func<Cook, bool>) (c1 => c1.Name != null)) (c)).Select (c => c.FirstName),
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] IS NOT NULL)");
    }

    [Test]
    public void InvocationExpression_WithCustomExpressions ()
    {
      Expression<Func<Cook, bool>> predicate1 = c => c.ID > 100;
      Expression<Func<Cook, bool>> predicate2 = c => c.Name != null;

      // c => c.ID > 100 && ((c1 => c1.Name != null) (c))
      var combinedPredicate = 
          Expression.Lambda<Func<Cook, bool>> (
              Expression.AndAlso (
                  predicate1.Body,
                  Expression.Invoke (predicate2, predicate1.Parameters.Cast<Expression>())
              ),
          predicate1.Parameters);
      
      CheckQuery (
          Cooks.Where (combinedPredicate).Select (c => c.FirstName),
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[ID] > @1) AND ([t0].[Name] IS NOT NULL))",
          new CommandParameter ("@1", 100)
          );
    }

  }
}