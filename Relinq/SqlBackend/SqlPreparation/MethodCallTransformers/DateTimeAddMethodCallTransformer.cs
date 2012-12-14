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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// Implements the <see cref="IMethodCallTransformer"/> interface for the <see cref="DateTime.Add(TimeSpan)"/> family of methods. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// Calls to those methods are represented as calls to the DATEADD SQL function. For <see cref="DateTime.AddYears"/> and 
  /// <see cref="DateTime.AddMonths"/>, we use DATEADD (year, ...) and DATEADD (month, ...) respectively. For the other methods, we convert the
  /// given number to whole milliseconds and call DATEADD (millisecond, ...). (Converting to milliseconds directly corresponds to the in-memory
  /// behavior. E.g., AddDays (12.5) always adss 12.5 * 24 hours, not respecting DST changes or something like this.)
  /// </para>
  /// <para>
  /// <see cref="DateTime.Add"/> only supports constant values, all the other methods also support column values.
  /// </para>
  /// </remarks>
  public class DateTimeAddMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
      {
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "Add", typeof (TimeSpan)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddYears", typeof (int)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddMonths", typeof (int)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddDays", typeof (double)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddHours", typeof (double)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddMinutes", typeof (double)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddSeconds", typeof (double)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddMilliseconds", typeof (double)),
        MethodCallTransformerUtility.GetStaticMethod (typeof (DateTime), "AddTicks", typeof (double)),
      };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      throw new NotImplementedException();
    }
  }
}