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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="EqualsMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for different Equals methods. The transformer
  /// is registered by name, not by method, so it will handle every method named "Equals" unless a specific <see cref="MethodInfo"/>-based transformer
  /// has been registered for that method.
  /// </summary>
  public class EqualsMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly string[] SupportedMethodNames = new[] { "Equals" };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      if (methodCallExpression.Arguments.Count == 1)
      {
        MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

        return Expression.Equal (methodCallExpression.Object, methodCallExpression.Arguments[0]);
      }
      else if (methodCallExpression.Arguments.Count == 2)
      {
        MethodCallTransformerUtility.CheckStaticMethod (methodCallExpression);

        return Expression.Equal (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
      }

      var message = string.Format (
          "{0} function with {1} arguments is not supported. Expression: '{2}'",
          methodCallExpression.Method.Name,
          methodCallExpression.Arguments.Count,
          FormattingExpressionTreeVisitor.Format(methodCallExpression));
      throw new NotSupportedException (message);
    }
  }
}