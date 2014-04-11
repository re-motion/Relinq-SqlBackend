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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class ContainsFulltextMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (ContainsFulltextMethodCallTransformer.SupportedMethods.Contains (
          typeof (StringExtensions).GetMethod("SqlContainsFulltext", new[] { typeof (string), typeof (string)})), Is.True);
      Assert.That (ContainsFulltextMethodCallTransformer.SupportedMethods.Contains (
          typeof (StringExtensions).GetMethod("SqlContainsFulltext", new[] { typeof (string), typeof (string), typeof (string)})), Is.True);
    }

    [Test]
    public void Transform_OneArgument ()
    {
      var method = typeof (StringExtensions).GetMethod (
          "SqlContainsFulltext",
          BindingFlags.Public | BindingFlags.Static,
          null,
          CallingConventions.Any,
          new[] { typeof (string), typeof (string) },
          null);
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant ("es");
      var expression = Expression.Call (method, objectExpression, argument1);
      var transformer = new ContainsFulltextMethodCallTransformer();

      var result = transformer.Transform (expression);

      var rightExpression = Expression.Constant (string.Format ("{0}", argument1.Value));
      var expectedResult = new SqlFunctionExpression (typeof (bool), "CONTAINS", objectExpression, rightExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_TwoArguments ()
    {
      var method = typeof (StringExtensions).GetMethod (
          "SqlContainsFulltext",
          BindingFlags.Public | BindingFlags.Static,
          null,
          CallingConventions.Any,
          new[] { typeof (string), typeof (string), typeof (string) },
          null);
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant ("es");
      var language = Expression.Constant ("language");
      var expression = Expression.Call (method, objectExpression, argument1, language);
      var transformer = new ContainsFulltextMethodCallTransformer();

      var result = transformer.Transform (expression);

      var argumentExpression = Expression.Constant (string.Format ("{0}", argument1.Value));

      var compositeExpression = new SqlCompositeCustomTextGeneratorExpression (
          typeof (string), new SqlCustomTextExpression ("LANGUAGE ", typeof (string)), language);

      var expectedResult =
          new SqlFunctionExpression (typeof (bool), "CONTAINS", objectExpression, argumentExpression, compositeExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}