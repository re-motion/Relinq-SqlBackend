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
  /// <see cref="ContainsMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the <see cref="string.Contains"/> method.
  /// </summary>
  public class ContainsMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
            MethodCallTransformerUtility.GetInstanceMethod (typeof (string), "Contains", typeof (string))
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

      // TODO Review 3090: I've refactored this method a little in order to pull out the code to create the like expression into a reusable method. Move this method to SqlLikeExpression.Create as a static method, then refactor the StartsWith and EndsWith transformers to also call the Create method. Add separate tests for Create in SqlLikeExpressionTest.

      return CreateLikeExpression (methodCallExpression.Object, methodCallExpression.Arguments[0], "%", "%");
    }

    private Expression CreateLikeExpression (Expression searchedText, Expression unescapedSearchText, string likePrefix, string likePostfix)
    {
      var rightExpression = BuildRightExpression (unescapedSearchText, likePrefix, likePostfix);
      if (rightExpression == null)
        return Expression.Constant (false);

      return new SqlLikeExpression (searchedText, rightExpression, new SqlLiteralExpression (@"\"));
    }

    private Expression BuildRightExpression (Expression unescapedSearchText, string likePrefix, string likePostfix)
    {
      Expression rightExpression;
      var argumentAsConstantExpression = unescapedSearchText as ConstantExpression;
      if (argumentAsConstantExpression != null)
      {
        if (argumentAsConstantExpression.Value == null)
          rightExpression = null;
        else
        {
          var escapedSearchText = LikeEscapeUtility.Escape ((string) argumentAsConstantExpression.Value, @"\");
          rightExpression = Expression.Constant (string.Format ("{0}{1}{2}", likePrefix, escapedSearchText, likePostfix));
        }
      }
      else
      {
        var escapedSearchExpression = LikeEscapeUtility.Escape (unescapedSearchText, @"\");
        var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
        var patternWithPrefix =
            !string.IsNullOrEmpty (likePrefix)
                ? Expression.Add (new SqlLiteralExpression (likePrefix), escapedSearchExpression, concatMethod)
                : escapedSearchExpression;
        var patternWithPrefixAndPostfix =
            !string.IsNullOrEmpty (likePostfix)
                ? Expression.Add (patternWithPrefix, new SqlLiteralExpression (likePostfix), concatMethod)
                : patternWithPrefix;
        rightExpression = patternWithPrefixAndPostfix;
      }
      return rightExpression;
    }
  }
}