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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="LikeMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the <see cref="StringExtensions.Like"/> extension 
  /// method.
  /// </summary>
  public class LikeMethodCallTransformer : IMethodCallTransformer
  {
    // TODO Review 2509: Add a class MethodCallTransformerUtility with the following methods:
    // TODO Review 2509: public static MethodInfo GetStaticMethod (Type type, string name, params Type[] argumentTypes);
    // TODO Review 2509: public static MethodInfo GetInstanceMethod (Type type, string name, params Type[] argumentTypes);
    // TODO Review 2509: Use those methods to define the SupportedMethods of all transformers in a more concise way
    public static readonly MethodInfo[] SupportedMethods = new[] {
        typeof (StringExtensions).GetMethod (
            "Like", 
            BindingFlags.Public | BindingFlags.Static, 
            null, 
            new[] { typeof (string), typeof (string) }, 
            null)
    };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      return new SqlBinaryOperatorExpression ("LIKE", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
    }
  }
}