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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class InsertMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (InsertMethodCallTransformer.SupportedMethods.Contains (typeof (string).GetMethod("Insert", new[] { typeof (int), typeof (string) })));
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (string).GetMethod ("Insert", new[] { typeof(int), typeof(string) });
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant(3);
      var argument2 = Expression.Constant("what");
      var expression = Expression.Call (objectExpression, method, argument1, argument2);
      var transformer = new InsertMethodCallTransformer ();

      var result = transformer.Transform (expression);

      var expectedTestExpression = Expression.Equal (new SqlLengthExpression (objectExpression), new SqlLiteralExpression (4));
      var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var expectedThenExpression = Expression.Add (objectExpression, argument2, concatMethod);
      var expectedElseExpression = new SqlFunctionExpression (typeof (string), "STUFF", objectExpression, new SqlLiteralExpression(4), new SqlLiteralExpression(0), argument2);
      var expectedResult = Expression.Condition (expectedTestExpression, expectedThenExpression, expectedElseExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

  }
}