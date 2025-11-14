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
using Remotion.Utilities;

#if NET6_0_OR_GREATER
namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// Implements the <see cref="IMethodCallTransformer"/> interface for the <see cref="DateOnly.AddDays(int)"/>,
  /// <see cref="DateOnly.AddMonths(int)"/> and <see cref="DateOnly.AddYears(int)"/> methods.
  /// </summary>
  /// <remarks>
  /// Calls to the methods are represented as calls to the DATEADD SQL function with the respective units.
  /// </remarks>
  public class DateOnlyAddMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
      {
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateOnly), "AddDays", typeof (int)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateOnly), "AddMonths", typeof (int)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateOnly), "AddYears", typeof (int))
      };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

      return methodCallExpression.Method.Name switch
        {
          "AddDays" => AddUnits (methodCallExpression, "day"),
          "AddMonths" => AddUnits (methodCallExpression, "month"),
          "AddYears" => AddUnits (methodCallExpression, "year"),
          _ => throw new ArgumentException (
              $"The method '{methodCallExpression.Method.DeclaringType}.{methodCallExpression.Method.Name}' is not a supported method.",
              nameof(methodCallExpression))
        };
    }

    private Expression AddUnits (MethodCallExpression methodCallExpression, string unit)
    {
      return new SqlFunctionExpression (
          typeof (DateOnly),
          "DATEADD",
          new SqlCustomTextExpression (unit, typeof (string)),
          methodCallExpression.Arguments[0],
          methodCallExpression.Object);
    }
  }
}
#endif
