// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallLowerTest
  {
    [Test]
    public void GenerateSql_Lower ()
    {
      var method = typeof (string).GetMethod ("ToLower", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var commandBuilder = new SqlCommandBuilder();

      var sqlGeneratingExpressionMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      sqlGeneratingExpressionMock.Expect (mock => mock.VisitExpression (methodCallExpression)).Return (methodCallExpression);

      var methodCallUpper = new MethodCallLower();
      methodCallUpper.GenerateSql (methodCallExpression, commandBuilder, sqlGeneratingExpressionMock);

      Assert.That (commandBuilder.GetCommandText(), Is.EqualTo ("LOWER()"));
    }
  }
}