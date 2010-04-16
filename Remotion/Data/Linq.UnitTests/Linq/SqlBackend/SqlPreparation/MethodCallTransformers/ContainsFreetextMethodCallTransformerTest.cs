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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class ContainsFreetextMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (
          ContainsFreetextMethodCallTransformer.SupportedMethods.Contains (
              MethodCallTransformerUtility.GetStaticMethod (
                  typeof (StringExtensions), "SqlContainsFreetext", typeof (string), typeof (string))));

      Assert.IsTrue (
          ContainsFreetextMethodCallTransformer.SupportedMethods.Contains (
              MethodCallTransformerUtility.GetStaticMethod (
                  typeof (StringExtensions),
                  "SqlContainsFreetext",
                  typeof (string),
                  typeof (string),
                  typeof (string))));
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
      var expression = Expression.Call (objectExpression, method, objectExpression, argument1);
      var transformer = new ContainsFreetextMethodCallTransformer();

      var result = transformer.Transform (expression);

      var rightExpression = Expression.Constant (string.Format ("{0}", argument1.Value));
      var expectedResult = new SqlFunctionExpression (typeof (bool), "FREETEXT", objectExpression, rightExpression);

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
      var expression = Expression.Call (objectExpression, method, objectExpression, argument1, language);
      var transformer = new ContainsFreetextMethodCallTransformer();

      var result = transformer.Transform (expression);

      var argumentExpression = Expression.Constant (string.Format ("{0}", argument1.Value));

      var compositeExpression = new SqlCompositeCustomTextGeneratorExpression (
          typeof (bool), new SqlCustomTextExpression ("LANGUAGE ", typeof (string)), language);

      var expectedResult =
          new SqlFunctionExpression (typeof (bool), "FREETEXT", objectExpression, argumentExpression, compositeExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}