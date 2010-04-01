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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallGenerators
{
  [TestFixture]
  public class RemoveMethodCallSqlGeneratorTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (
          RemoveMethodCallSqlGenerator.SupportedMethods.Contains (typeof (string).GetMethod ("Remove", new[] { typeof (int) })));
    }

    [Test]
    public void GenerateSql_Remove ()
    {
      var method = typeof (string).GetMethod ("Remove", new Type[] { typeof (int), typeof (int) });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method, Expression.Constant (0), Expression.Constant (1));

      var commandBuilder = new SqlCommandBuilder();

      var sqlGeneratingExpressionMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      sqlGeneratingExpressionMock.Expect (mock => mock.VisitExpression (methodCallExpression)).Return (methodCallExpression);

      var methodCallUpper = new RemoveMethodCallSqlGenerator();
      methodCallUpper.GenerateSql (methodCallExpression, commandBuilder, sqlGeneratingExpressionMock);

      Assert.That (commandBuilder.GetCommandText(), Is.EqualTo ("STUFF(,,,LEN(), \"\")"));
    }
  }
}