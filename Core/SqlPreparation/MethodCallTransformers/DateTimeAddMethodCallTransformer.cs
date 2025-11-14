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
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

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
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

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
                    methodCallExpression);
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
          return AddMilliseconds (methodCallExpression.Arguments[0], methodCallExpression.Object);
        case "AddMinutes":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerMinute / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddMonths":
          return AddUnits (methodCallExpression.Arguments[0], "month", methodCallExpression.Object);
        case "AddSeconds":
          return AddWithConversion (
              methodCallExpression.Arguments[0], TimeSpan.TicksPerSecond / TimeSpan.TicksPerMillisecond, methodCallExpression.Object);
        case "AddTicks":
          return AddTicks (methodCallExpression.Arguments[0], methodCallExpression.Object);
        case "AddYears":
          return AddUnits (methodCallExpression.Arguments[0], "year", methodCallExpression.Object);
        default:
          var argumentExceptionMessage = string.Format (
              "The method '{0}.{1}' is not a supported method.", methodCallExpression.Method.DeclaringType, methodCallExpression.Method.Name);
          throw new ArgumentException (argumentExceptionMessage, nameof(methodCallExpression));
      }
    }

    private Expression AddWithConversion (Expression value, double factorToMilliseconds, Expression dateTime)
    {
      // Convert the value to milliseconds first, then add as milliseconds.
      var milliseconds = Expression.Multiply (value, new SqlLiteralExpression (factorToMilliseconds));
      return AddMilliseconds (milliseconds, dateTime);
    }

    private Expression AddTimeSpan (TimeSpan timeSpan, Expression dateTime)
    {
      // Add a timespan constant by using its ticks value as a long constant.
      var ticks = Expression.Constant (timeSpan.Ticks, typeof (long));
      return AddTicks (ticks, dateTime);
    }

    private Expression AddTicks (Expression ticks, Expression dateTime)
    {
      // Add ticks by converting them to milliseconds (truncating divide by 10000).
      var milliseconds = Expression.Divide (ticks, new SqlLiteralExpression (TimeSpan.TicksPerMillisecond));
      return AddMilliseconds (milliseconds, dateTime);
    }

    private Expression AddMilliseconds (Expression milliseconds, Expression dateTime)
    {
      // Convert milliseconds value to long first (truncating).
      if (milliseconds.Type != typeof (long))
        milliseconds = new SqlConvertExpression (typeof (long), milliseconds);

      // Add milliseconds in two steps: extract the days and add them as a "day" value, then add the remaining milliseconds as a "milliseconds" value.
      // This two-step part is required because SQL Server can only add 32-bit INTs using DATEADD, no BIGINTs. The milliseconds in a day (86,400,000) 
      // fit into an INT. The second INT for days can express up to +/- 2^31 (or so) days, which equals about five million years. This is much
      // more than a SQL DATETIME can hold.
      var days = Expression.Divide (milliseconds, new SqlLiteralExpression (TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond));
      var remainingMilliseconds = Expression.Modulo (milliseconds, new SqlLiteralExpression (TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond));
      return AddUnits (remainingMilliseconds, "millisecond", AddUnits (days, "day", dateTime));
    }

    private Expression AddUnits (Expression value, string unit, Expression dateTime)
    {
      return new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression (unit, typeof (string)),
          value,
          dateTime);
    }

  }
}