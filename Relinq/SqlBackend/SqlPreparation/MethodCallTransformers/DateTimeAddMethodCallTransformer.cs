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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.SqlGeneration;
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
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "Add", typeof (TimeSpan)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddDays", typeof (double)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddHours", typeof (double)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddMilliseconds", typeof (double)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddMinutes", typeof (double)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddMonths", typeof (int)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddSeconds", typeof (double)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddTicks", typeof (long)),
        MethodCallTransformerUtility.GetInstanceMethod (typeof (DateTime), "AddYears", typeof (int))
      };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1);
      MethodCallTransformerUtility.CheckInstanceMethod (methodCallExpression);

      switch (methodCallExpression.Method.Name)
      {
        case "Add":
          Trace.Assert (methodCallExpression.Method.Name == "Add");
          var constantTimeSpanExpression = methodCallExpression.Arguments[0] as ConstantExpression;
          if (constantTimeSpanExpression == null)
          {
            var message =
                string.Format (
                    "The method 'System.DateTime.Add' can only be transformed to SQL when its argument is a constant value. Expression: '{0}'.",
                    FormattingExpressionTreeVisitor.Format (methodCallExpression));
            throw new NotSupportedException (message);
          }

          return AddTimeSpan ((TimeSpan) constantTimeSpanExpression.Value, methodCallExpression.Object);
        case "AddDays":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddHours":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerHour / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddMilliseconds":
          return AddWithConversion (methodCallExpression.Arguments[0], methodCallExpression.Object);
        case "AddMinutes":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerMinute / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddMonths":
          return AddUnits (methodCallExpression.Arguments[0], "month", methodCallExpression.Object);
        case "AddSeconds":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerSecond / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddTicks":
          return AddMilliseconds (
              Expression.Divide (methodCallExpression.Arguments[0], new SqlLiteralExpression (TimeSpan.TicksPerMillisecond)),
              methodCallExpression.Object);
        case "AddYears":
          return AddUnits (methodCallExpression.Arguments[0], "year", methodCallExpression.Object);
        default:
          var argumentExceptionMessage = string.Format (
              "The method '{0}.{1}' is not a supported method.", methodCallExpression.Method.DeclaringType, methodCallExpression.Method.Name);
          throw new ArgumentException (argumentExceptionMessage, "methodCallExpression");
      }
    }

    private Expression AddWithConversion (Expression value, double factorToMilliseconds, Expression dateTime)
    {
      return AddWithConversion (Expression.Multiply (value, new SqlLiteralExpression (factorToMilliseconds)), dateTime);
    }

    private Expression AddWithConversion (Expression value, Expression dateTime)
    {
      var sqlConvertExpression = new SqlConvertExpression (typeof (long), value);
      return AddMilliseconds(sqlConvertExpression, dateTime);
    }

    private static Expression AddMilliseconds (Expression value, Expression dateTime)
    {
      return AddUnits (value, "millisecond", dateTime);
    }

    private static Expression AddUnits (Expression value, string unit, Expression dateTime)
    {
      return new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression (unit, typeof (string)),
          value,
          dateTime);
    }

    private static Expression AddTimeSpan (TimeSpan value, Expression dateTime)
    {
      return new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          new SqlConvertExpression (typeof (long), Expression.Constant (value.TotalMilliseconds, typeof (double))),
          dateTime);
    }
  }
}