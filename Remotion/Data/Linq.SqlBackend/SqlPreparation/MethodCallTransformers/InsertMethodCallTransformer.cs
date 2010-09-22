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
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
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
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 2);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);
      MethodCallTransformerUtility.CheckConstantExpression ("Insert", methodCallExpression.Arguments[0], "insertionIndex");

      var insertionIndexExpression = new SqlLiteralExpression (((int) ((ConstantExpression) methodCallExpression.Arguments[0]).Value) + 1);
      var testExpression = Expression.Equal(new SqlFunctionExpression (typeof (int), "LEN", methodCallExpression.Object), insertionIndexExpression);

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