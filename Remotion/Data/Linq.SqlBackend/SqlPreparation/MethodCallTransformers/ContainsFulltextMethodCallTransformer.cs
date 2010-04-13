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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="ContainsFulltextMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the 
  /// <see cref="StringExtensions.ContainsFulltext(string,string)"/> extension methods.
  /// </summary>
  public class ContainsFulltextMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               typeof (StringExtensions).GetMethod (
                                                                   "ContainsFulltext",
                                                                   BindingFlags.Public | BindingFlags.Static,
                                                                   null,
                                                                   CallingConventions.Any,
                                                                   new[] { typeof (string), typeof (string) },
                                                                   null),
                                                               typeof (StringExtensions).GetMethod (
                                                                   "ContainsFulltext",
                                                                   BindingFlags.Public | BindingFlags.Static,
                                                                   null,
                                                                   CallingConventions.Any,
                                                                   new[] { typeof (string), typeof (string), typeof (string) },
                                                                   null)
                                                           };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      if (methodCallExpression.Arguments.Count == 2) // overload without language
        return new SqlFunctionExpression (typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
      else if (methodCallExpression.Arguments.Count == 3)
      {
        MethodCallTransformerUtility.CheckConstantExpression (methodCallExpression.Method.Name, methodCallExpression.Arguments[2]);
        
        //TODO 2509: escape sql text in language argument ???
        // TODO Review 2509: Use the SqlCompositeExpression to create the following languageExpression: SqlCompositeExpression (SqlCustomTextExpression ("LANGUAGE "), SqlConstantExpression (((ConstantExpression)methodCallExpression.Arguments[2]).Value))
        // TODO Review 2509: By using the SqlConstantExpression, escaping is not required.

        var languageExpression =
            new SqlLiteralExpression (string.Format ("LANGUAGE {0}", ((ConstantExpression) methodCallExpression.Arguments[2]).Value));

        return new SqlFunctionExpression (
            typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], languageExpression);
      }
      else  // TODO Review 2509: Encapsulate these checks (in all transformers) into a MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 2, 3) method (taking a params int[] allowedArgumentCounts); also add a CheckInstanceMethod (methodCallExpression) and a CheckStaticMethod (methodCallExpression) method and use them
      {
        throw new NotSupportedException (
            string.Format ("IndexOf function with {0} arguments is not supported.", methodCallExpression.Arguments.Count));
      }
    }
  }
}