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
using System.Linq;


namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallGenerators
{
  [TestFixture]
  public class UpperMethodCallSqlGeneratorTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (
          UpperMethodCallSqlGenerator.SupportedMethods.Contains (typeof (string).GetMethod ("ToUpper", new Type[] { })));
    }

    [Test]
    public void GenerateSql_Upper ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var commandBuilder = new SqlCommandBuilder();

      var sqlGeneratingExpressionMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      sqlGeneratingExpressionMock.Expect (mock => mock.VisitExpression (methodCallExpression)).Return (methodCallExpression);

      var methodCallUpper = new UpperMethodCallSqlGenerator();
      methodCallUpper.GenerateSql (methodCallExpression, commandBuilder, sqlGeneratingExpressionMock);

      Assert.That (commandBuilder.GetCommandText(), Is.EqualTo ("UPPER()"));
    }
  }
}