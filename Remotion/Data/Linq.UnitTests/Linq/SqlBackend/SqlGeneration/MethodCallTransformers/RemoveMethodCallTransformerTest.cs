// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.MethodCallTransformers
{
  [TestFixture]
  public class RemoveMethodCallTransformerTest
  {
    [Test]
    public void Transform_WithOneArgument ()
    {
      var method = typeof (string).GetMethod ("Remove", new Type[] { typeof(int) });
      var objectExpression = Expression.Constant ("Test");
      var expression = Expression.Call (objectExpression, method,Expression.Constant (1));

      var transformer = new RemoveMethodCallTransformer();
      var result = transformer.Transform (expression);

      var fakeResult = new SqlFunctionExpression (
            expression.Type,
            "STUFF",
            objectExpression,
            Expression.Add (Expression.Constant (1), new SqlLiteralExpression (1)),
            new SqlFunctionExpression (objectExpression.Type, "LEN", objectExpression),
            new SqlLiteralExpression ("''"));

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithTwoArgument ()
    {
      var method = typeof (string).GetMethod ("Remove", new Type[] { typeof (int), typeof (int) });
      var objectExpression = Expression.Constant ("Test");
      var expression = Expression.Call (objectExpression, method, Expression.Constant (1), Expression.Constant(3));

      var transformer = new RemoveMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var fakeResult = new SqlFunctionExpression (
            expression.Type,
            "STUFF",
            objectExpression,
            Expression.Add (Expression.Constant (1), new SqlLiteralExpression (1)),
            Expression.Constant (3),
            new SqlLiteralExpression ("''"));

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }
  }
}