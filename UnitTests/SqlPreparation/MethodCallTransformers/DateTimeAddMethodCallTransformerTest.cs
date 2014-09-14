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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  [SetCulture ("")]
  public class DateTimeAddMethodCallTransformerTest
  {
    private ConstantExpression _dateTimeInstance;
    private DateTimeAddMethodCallTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _dateTimeInstance = Expression.Constant (new DateTime (2012, 12, 17));
      _transformer = new DateTimeAddMethodCallTransformer();
    }

    [Test]
    public void SupportedMethods ()
    {
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.Add (new TimeSpan ()))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddDays (1.5))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddHours (1.5))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddMilliseconds (1.5))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddMinutes (1.5))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddMonths (15))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddSeconds (1.5))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddTicks (15))));
      Assert.That (DateTimeAddMethodCallTransformer.SupportedMethods, Has.Member (ReflectionUtility.GetMethod (() => DateTime.Now.AddYears (15))));
    }

    [Test]
    public void Transform_Add_TimeSpan ()
    {
      var value = Expression.Constant (new TimeSpan (123456789L));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.Add (new TimeSpan()));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              Expression.Divide (Expression.Constant (123456789L), new SqlLiteralExpression (10000L)), new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  Expression.Divide (Expression.Constant (123456789L), new SqlLiteralExpression (10000L)), new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_Add_TimeSpan_NoConstantTimeSpan ()
    {
      var value = new CustomExpression (typeof (TimeSpan));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.Add (new TimeSpan ()));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      Assert.That (
          () => _transformer.Transform (expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "The method 'System.DateTime.Add' can only be transformed to SQL when its argument is a constant value. "
              + "Expression: '12/17/2012 00:00:00.Add(CustomExpression)'."));
    }

    [Test]
    public void Transform_AddDays ()
    {
      var value = new CustomExpression (typeof (double));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddDays (0.0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (86400000.0))),
              new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression
              (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (86400000.0))),
                  new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddHours ()
    {
      var value = new CustomExpression (typeof (double));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddHours (0.0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (3600000.0))),
              new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression
              (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (3600000.0))),
                  new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddMilliseconds ()
    {
      var value = new CustomExpression (typeof (double));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddMilliseconds (0.0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              new SqlConvertExpression (typeof (long), value),
              new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  new SqlConvertExpression (typeof (long), value),
                  new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddMinutes ()
    {
      var value = new CustomExpression (typeof (double));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddMinutes (0.0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (60000.0))),
              new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression
              (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (60000.0))),
                  new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddMonths ()
    {
      var value = new CustomExpression (typeof (int));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddMonths (0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("month", typeof (string)),
          value,
          _dateTimeInstance);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddSeconds ()
    {
      var value = new CustomExpression (typeof (double));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddSeconds (0.0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (1000.0))),
              new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression
              (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  new SqlConvertExpression (typeof (long), Expression.Multiply (value, new SqlLiteralExpression (1000.0))),
                  new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddTicks ()
    {
      var value = new CustomExpression (typeof (long));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddTicks (123456));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("millisecond", typeof (string)),
          Expression.Modulo (
              Expression.Divide (value, new SqlLiteralExpression (10000L)), new SqlLiteralExpression (86400000L)),
          new SqlFunctionExpression (
              typeof (DateTime),
              "DATEADD",
              new SqlCustomTextExpression ("day", typeof (string)),
              Expression.Divide (
                  Expression.Divide (value, new SqlLiteralExpression (10000L)), new SqlLiteralExpression (86400000L)),
              _dateTimeInstance));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddYears ()
    {
      var value = new CustomExpression (typeof (int));
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.AddYears (0));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateTime),
          "DATEADD",
          new SqlCustomTextExpression ("year", typeof (string)),
          value,
          _dateTimeInstance);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_InvalidMethod ()
    {
      var methodInfo = ReflectionUtility.GetMethod (() => DateTime.Now.ToString (""));
      var expression = Expression.Call (_dateTimeInstance, methodInfo, Expression.Constant (""));

      Assert.That (
          () => _transformer.Transform (expression),
          Throws.TypeOf<ArgumentException>().With.Message.EqualTo (
              "The method 'System.DateTime.ToString' is not a supported method.\r\nParameter name: methodCallExpression"));
    }
  }
}