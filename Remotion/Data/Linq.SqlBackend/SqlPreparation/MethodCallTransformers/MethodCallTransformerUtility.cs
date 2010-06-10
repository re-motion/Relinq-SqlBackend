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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
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
        throw new NotSupportedException (
            string.Format ("Only expressions that can be evaluated locally can be used as the argument for {0} ('{1}').", methodName, parameterName));
      }
      return argumentAsConstantExpression;
    }

    public static MethodInfo GetStaticMethod (Type type, string methodName, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("methodName", methodName);

      return type.GetMethod (
          methodName,
          BindingFlags.Public | BindingFlags.Static,
          null,
          argumentTypes,
          null);
    }

    public static MethodInfo GetInstanceMethod (Type type, string methodName, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("methodName", methodName);

      return type.GetMethod (methodName, argumentTypes);
    }

    public static void CheckArgumentCount (MethodCallExpression methodCallExpression, params int[] allowedArgumentCounts)
    {
      if (!allowedArgumentCounts.Contains (methodCallExpression.Arguments.Count))
      {
        throw new NotSupportedException (
            string.Format (
                "{0} function with {1} arguments is not supported.", methodCallExpression.Method.Name, methodCallExpression.Arguments.Count));
      }
    }

    public static void CheckStaticMethod (MethodCallExpression methodCallExpression)
    {
      if (!methodCallExpression.Method.IsStatic)
      {
        throw new NotSupportedException (
            string.Format ("{0} is not supported by this transformer.", methodCallExpression.Method.Name));
      }
    }

    public static void CheckInstanceMethod (MethodCallExpression methodCallExpression)
    {
      if (methodCallExpression.Method.IsStatic)
        throw new NotSupportedException (string.Format ("{0} is not supported by this transformer.", methodCallExpression.Method.Name));
    }
  }
}