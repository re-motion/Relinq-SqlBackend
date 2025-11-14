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
  /// <see cref="SubstringMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the <see cref="string.Substring(int)"/> methods.
  /// </summary>
  public class SubstringMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "Substring", typeof (int)),
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "Substring", typeof (int), typeof (int))
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1, 2);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

      var startIndexExpression = Expression.Add (methodCallExpression.Arguments[0], new SqlLiteralExpression (1));

      if (methodCallExpression.Arguments.Count == 1)
      {
        return new SqlFunctionExpression (
            methodCallExpression.Type,
            "SUBSTRING",
            methodCallExpression.Object,
            startIndexExpression,
            new SqlLengthExpression (methodCallExpression.Object));
      }
      else if (methodCallExpression.Arguments.Count == 2)
      {
        return new SqlFunctionExpression (
            methodCallExpression.Type,
            "SUBSTRING",
            methodCallExpression.Object,
            startIndexExpression,
            methodCallExpression.Arguments[1]);
      }
      else
      {
        var message = string.Format (
            "Substring function with {0} arguments is not supported. Expression: '{1}'",
            methodCallExpression.Arguments.Count,
            methodCallExpression);
        throw new NotSupportedException (message);
      }
    }
  }
}