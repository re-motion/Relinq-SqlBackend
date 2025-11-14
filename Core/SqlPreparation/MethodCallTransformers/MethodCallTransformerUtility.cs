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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="MethodCallTransformerUtility"/> provides utility methods for MethodCallTransformers.
  /// </summary>
  public class MethodCallTransformerUtility
  {
    public static ConstantExpression CheckConstantExpression (string methodName, Expression argument, string parameterName)
    {
      var argumentAsConstantExpression = argument as ConstantExpression;
      if (argumentAsConstantExpression == null)
      {
        var message = string.Format (
            "Only expressions that can be evaluated locally can be used as an argument for {0} ('{1}'). Expression: '{2}'", 
            methodName, 
            parameterName,
            argument);
        throw new NotSupportedException (message);
      }
      return argumentAsConstantExpression;
    }

    public static MethodInfo GetStaticMethod (Type type, string methodName, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(methodName), methodName);

      return type.GetMethod (
          methodName,
          BindingFlags.Public | BindingFlags.Static,
          null,
          argumentTypes,
          null);
    }

    public static MethodInfo GetInstanceMethod (Type type, string methodName, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(methodName), methodName);

      return type.GetMethod (methodName, argumentTypes);
    }

    public static void CheckArgumentCount (MethodCallExpression methodCallExpression, params int[] allowedArgumentCounts)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);
      ArgumentUtility.CheckNotNull (nameof(allowedArgumentCounts), allowedArgumentCounts);

      if (!allowedArgumentCounts.Contains (methodCallExpression.Arguments.Count))
      {
        var message = string.Format (
            "{0} function with {1} arguments is not supported. Expression: '{2}'", 
            methodCallExpression.Method.Name, 
            methodCallExpression.Arguments.Count,
            methodCallExpression);
        throw new NotSupportedException (
            message);
      }
    }

    public static void CheckStaticMethod (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      if (!methodCallExpression.Method.IsStatic)
      {
        var message = string.Format (
            "Method {0} is not supported by this transformer. Expression: '{1}'",
            methodCallExpression.Method.Name,
            methodCallExpression);
        throw new NotSupportedException (message);
      }
    }

    public static void CheckInstanceMethod (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      if (methodCallExpression.Method.IsStatic)
      {
        var message = string.Format (
            "Method {0} is not supported by this transformer. Expression: '{1}'",
            methodCallExpression.Method.Name,
            methodCallExpression);
        throw new NotSupportedException (message);
      }
    }
  }
}