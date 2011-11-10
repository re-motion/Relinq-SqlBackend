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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="IndexOfMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the <see cref="string.IndexOf(char)"/> methods.
  /// </summary>
  public class IndexOfMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (string)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (char)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (string), typeof (int)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (char), typeof (int)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (string), typeof (int), typeof (int)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "IndexOf", typeof (char), typeof (int), typeof (int))
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1, 2, 3);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

      if (methodCallExpression.Arguments.Count == 1)
      {
        var lenExpression = new SqlLengthExpression (methodCallExpression.Arguments[0]);
        var testPredicate = Expression.Equal (lenExpression, new SqlLiteralExpression (0));
        var charIndexExpression = new SqlFunctionExpression (
            methodCallExpression.Type, "CHARINDEX", methodCallExpression.Arguments[0], methodCallExpression.Object);
        var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

        return Expression.Condition (testPredicate, new SqlLiteralExpression (0), elseValue);
      }
      else if (methodCallExpression.Arguments.Count == 2)
      {
        var startIndexExpression = Expression.Add (methodCallExpression.Arguments[1], new SqlLiteralExpression (1));

        var lenArgExpression = new SqlLengthExpression (methodCallExpression.Arguments[0]);
        var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

        var lenObjectExpression = new SqlLengthExpression (methodCallExpression.Object);
        var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
        var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

        var charIndexExpression = new SqlFunctionExpression (
            methodCallExpression.Type, "CHARINDEX", methodCallExpression.Arguments[0], methodCallExpression.Object, startIndexExpression);

        var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

        return Expression.Condition (testPredicate, methodCallExpression.Arguments[1], elseValue);
      }
      else if (methodCallExpression.Arguments.Count == 3)
      {
        var startIndexExpression = Expression.Add (methodCallExpression.Arguments[1], new SqlLiteralExpression (1));

        var lenArgExpression = new SqlLengthExpression (methodCallExpression.Arguments[0]);
        var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

        var lenObjectExpression = new SqlLengthExpression (methodCallExpression.Object);
        var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
        var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

        var startAddCountExpression = Expression.Add (methodCallExpression.Arguments[1], methodCallExpression.Arguments[2]);
        var substringExpression = new SqlFunctionExpression (
            typeof (string), "SUBSTRING", methodCallExpression.Object, new SqlLiteralExpression (1), startAddCountExpression);

        var charIndexExpression = new SqlFunctionExpression (
            methodCallExpression.Type, "CHARINDEX", methodCallExpression.Arguments[0], substringExpression, startIndexExpression);

        var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

        return Expression.Condition (testPredicate, methodCallExpression.Arguments[1], elseValue);
      }
      else
      {
        var message = string.Format (
            "IndexOf function with {0} arguments is not supported. Expression: '{1}'",
            methodCallExpression.Arguments.Count,
            FormattingExpressionTreeVisitor.Format (methodCallExpression));
        throw new NotSupportedException (message);
      }
    }
  }
}