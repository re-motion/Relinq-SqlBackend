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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// Implements the <see cref="IMethodCallTransformer"/> interface for the <see cref="string.Concat(string, string)"/> overloads. 
  /// </summary>
  /// <remarks>
  /// Calls to those methods
  /// are represented as simple 
  /// <see cref="Expression.Equal(System.Linq.Expressions.Expression,System.Linq.Expressions.Expression,bool,System.Reflection.MethodInfo)"/> 
  /// expressions that refer to the <see cref="string.Concat(string,string)"/> method. Arguments that are not of static type <see cref="string"/>
  /// are converted to the nvarchar data type via <see cref="SqlConvertExpression"/>.
  /// </remarks>
  public class ConcatMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
      {
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (object)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (object), typeof (object)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (object), typeof (object), typeof (object)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (string), typeof (string)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (string), typeof (string), typeof (string)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (string), typeof (string), typeof (string), typeof (string)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (object[])),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (string[])),
        MethodCallTransformerUtility.GetStaticMethod (typeof (string), "Concat", typeof (IEnumerable<string>)),
        typeof (string).GetMethods().Single (mi => mi.Name == "Concat" && mi.IsGenericMethod && mi.GetGenericArguments().Length == 1)
      };

    private static readonly MethodInfo s_standardConcatMethodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      var concatenatedItems = GetConcatenatedItems (methodCallExpression);
      return concatenatedItems
          .Select (a => a.Type == typeof (string) ? a : (Expression) new SqlConvertExpression (typeof (string), a))
          .Aggregate ((a1, a2) => Expression.Add (a1, a2, s_standardConcatMethodInfo));
    }

    private IEnumerable<Expression> GetConcatenatedItems (MethodCallExpression methodCallExpression)
    {
      if (methodCallExpression.Arguments.Count == 1
          && (typeof (IEnumerable).IsAssignableFrom (methodCallExpression.Arguments[0].Type)
              && methodCallExpression.Arguments[0].Type != typeof (string)))
      {
        ConstantExpression argumentAsConstantExpression;
        NewArrayExpression argumentAsNewArrayExpression;

        if ((argumentAsNewArrayExpression = methodCallExpression.Arguments[0] as NewArrayExpression) != null)
        {
          return argumentAsNewArrayExpression.Expressions;
        }
        else if ((argumentAsConstantExpression = methodCallExpression.Arguments[0] as ConstantExpression) != null)
        {
          return ((object[]) argumentAsConstantExpression.Value).Select (element => (Expression) Expression.Constant (element));
        }
        else
        {
          var message = string.Format (
              "The method call '{0}' is not supported. When the array overloads of String.Concat are used, only constant or new array expressions can "
              + "be translated to SQL; in this usage, the expression has type '{1}'.",
              methodCallExpression,
              methodCallExpression.Arguments[0].GetType ());
          throw new NotSupportedException (message);
        }
      }
      else 
      {
        return methodCallExpression.Arguments;
      }
    }
  }
}