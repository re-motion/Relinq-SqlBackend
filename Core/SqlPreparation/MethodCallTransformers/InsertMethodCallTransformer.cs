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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="InsertMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the <see cref="string.Trim()"/> method.
  /// </summary>
  public class InsertMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "Insert", new[] { typeof (int), typeof (string) })
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 2);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);
      MethodCallTransformerUtility.CheckConstantExpression ("Insert", methodCallExpression.Arguments[0], "insertionIndex");

      var insertionIndexExpression = Expression.Add (methodCallExpression.Arguments[0], new SqlLiteralExpression (1));
      var testExpression = Expression.Equal(new SqlLengthExpression (methodCallExpression.Object), insertionIndexExpression);

      var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var thenExpression = Expression.Add (methodCallExpression.Object, methodCallExpression.Arguments[1], concatMethod);

      var elseExpression = new SqlFunctionExpression (
          methodCallExpression.Type,
          "STUFF",
          methodCallExpression.Object,
          insertionIndexExpression,
          new SqlLiteralExpression (0),
          methodCallExpression.Arguments[1]);

      return Expression.Condition(testExpression, thenExpression, elseExpression);
    }
  }
}