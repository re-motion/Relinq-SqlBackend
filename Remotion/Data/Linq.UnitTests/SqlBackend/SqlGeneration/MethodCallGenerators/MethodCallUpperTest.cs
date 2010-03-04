// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Rhino.Mocks;


namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallUpperTest
  {
    [Test]
    public void GenerateSql_Upper ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var commandBuilder = new SqlCommandBuilder();

      var sqlGeneratingExpressionMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      sqlGeneratingExpressionMock.Expect (mock => mock.VisitExpression (methodCallExpression)).Return (methodCallExpression);

      var methodCallUpper = new MethodCallUpper();
      methodCallUpper.GenerateSql (methodCallExpression, commandBuilder, sqlGeneratingExpressionMock);

      Assert.That (commandBuilder.GetCommandText(), Is.EqualTo ("UPPER()"));
    }
  }
}