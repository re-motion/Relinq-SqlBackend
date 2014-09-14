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
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="ContainsFulltextMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the 
  /// <see cref="StringExtensions.SqlContainsFulltext(string, string)"/> extension methods.
  /// </summary>
  public class ContainsFulltextMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               MethodCallTransformerUtility.GetStaticMethod (
                                                                   typeof (StringExtensions), "SqlContainsFulltext", typeof (string), typeof (string)),
                                                               MethodCallTransformerUtility.GetStaticMethod (
                                                                   typeof (StringExtensions),
                                                                   "SqlContainsFulltext",
                                                                   typeof (string),
                                                                   typeof (string),
                                                                   typeof (string))
                                                           };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 2, 3);
      MethodCallTransformerUtility.CheckStaticMethod (methodCallExpression);

      if (methodCallExpression.Arguments.Count == 2) // overload without language
        return new SqlFunctionExpression (typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
      else 
      {
        MethodCallTransformerUtility.CheckConstantExpression (methodCallExpression.Method.Name, methodCallExpression.Arguments[2], "language parameter");
        
        var compositeExpression = new SqlCompositeCustomTextGeneratorExpression (
            typeof (string), new SqlCustomTextExpression ("LANGUAGE ", typeof (string)), methodCallExpression.Arguments[2]);

        return new SqlFunctionExpression (
            typeof (bool), "CONTAINS", methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], compositeExpression);
      }
    }
  }
}