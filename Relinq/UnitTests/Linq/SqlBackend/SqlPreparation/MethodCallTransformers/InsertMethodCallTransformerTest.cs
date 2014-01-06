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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class InsertMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (
          InsertMethodCallTransformer.SupportedMethods.Contains (typeof (string).GetMethod ("Insert", new[] { typeof (int), typeof (string) })),
          Is.True);
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

      var expectedTestExpression = Expression.Equal (new SqlLengthExpression (objectExpression), Expression.Add (argument1, new SqlLiteralExpression (1)));
      var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var expectedThenExpression = Expression.Add (objectExpression, argument2, concatMethod);
      var expectedElseExpression = new SqlFunctionExpression (typeof (string), "STUFF", objectExpression, Expression.Add (argument1, new SqlLiteralExpression (1)), new SqlLiteralExpression(0), argument2);
      var expectedResult = Expression.Condition (expectedTestExpression, expectedThenExpression, expectedElseExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

  }
}