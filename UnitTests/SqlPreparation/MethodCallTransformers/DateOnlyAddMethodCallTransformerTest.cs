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
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Remotion.Utilities;

#if NET6_0_OR_GREATER
namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  [SetCulture ("")]
  public class DateOnlyAddMethodCallTransformerTest
  {
    private ConstantExpression _dateOnlyInstance;
    private DateOnlyAddMethodCallTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _dateOnlyInstance = Expression.Constant (new DateOnly (2012, 12, 17));
      _transformer = new DateOnlyAddMethodCallTransformer();
    }

    [Test]
    public void SupportedMethods ()
    {
      Assert.That (
          DateOnlyAddMethodCallTransformer.SupportedMethods,
          Has.Member (MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddDays (1))));
      Assert.That (
          DateOnlyAddMethodCallTransformer.SupportedMethods,
          Has.Member (MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddMonths (1))));
      Assert.That (
          DateOnlyAddMethodCallTransformer.SupportedMethods,
          Has.Member (MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddYears (1))));
    }

    [Test]
    public void Transform_AddDays ()
    {
      var value = new CustomExpression (typeof (int));
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddDays (5));
      var expression = Expression.Call (_dateOnlyInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateOnly),
          "DATEADD",
          new SqlCustomTextExpression ("day", typeof (string)),
          value,
          _dateOnlyInstance);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddMonths ()
    {
      var value = new CustomExpression (typeof (int));
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddMonths (0));
      var expression = Expression.Call (_dateOnlyInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateOnly),
          "DATEADD",
          new SqlCustomTextExpression ("month", typeof (string)),
          value,
          _dateOnlyInstance);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_AddYears ()
    {
      var value = new CustomExpression (typeof (int));
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.AddYears (0));
      var expression = Expression.Call (_dateOnlyInstance, methodInfo, value);

      var result = _transformer.Transform (expression);

      var expectedResult = new SqlFunctionExpression (
          typeof (DateOnly),
          "DATEADD",
          new SqlCustomTextExpression ("year", typeof (string)),
          value,
          _dateOnlyInstance);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_InvalidMethod ()
    {
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => DateOnly.MinValue.ToString (""));
      var expression = Expression.Call (_dateOnlyInstance, methodInfo, Expression.Constant (""));

      Assert.That (
          () => _transformer.Transform (expression),
          Throws.TypeOf<ArgumentException>().With.ArgumentExceptionMessageEqualTo (
              "The method 'System.DateOnly.ToString' is not a supported method.", "methodCallExpression"));
    }
  }
}
#endif