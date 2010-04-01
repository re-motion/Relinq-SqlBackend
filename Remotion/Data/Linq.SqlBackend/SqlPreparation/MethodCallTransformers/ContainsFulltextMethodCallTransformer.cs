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
  /// <see cref="ContainsFulltextMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the contains fulltext extension method.
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
      if (methodCallExpression.Arguments.Count == 2)
        return new SqlFunctionExpression (typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
      else if (methodCallExpression.Arguments.Count == 3)
      {
        if (!(methodCallExpression.Arguments[2] is ConstantExpression))
        {
          throw new NotSupportedException (
              "Only expressions that can be evaluated locally can be used as the language argument for contains fulltext.");
        }

        //TODO 2509: escape sql text in language argument ???

        var languageExpression =
            new SqlLiteralExpression (string.Format ("LANGUAGE {0}", ((ConstantExpression) methodCallExpression.Arguments[2]).Value));

        return new SqlFunctionExpression (
            typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], languageExpression);
      }
      else
      {
        throw new NotSupportedException (
            string.Format ("IndexOf function with {0} arguments is not supported.", methodCallExpression.Arguments.Count));
      }
    }
  }
}