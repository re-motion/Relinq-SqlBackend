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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class EndsWithMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (EndsWithMethodCallTransformer.SupportedMethods.Contains (typeof (string).GetMethod("EndsWith", new[] { typeof (string)})), Is.True);
    }

    [Test]
    public void Transform_ArgumentNotNull ()
    {
      var method = typeof (string).GetMethod ("EndsWith", new[] { typeof (string) });
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant ("test");
      var expression = Expression.Call (objectExpression, method, argument1);
      var transformer = new EndsWithMethodCallTransformer();
      var result = transformer.Transform (expression);

      var rightExpression = Expression.Constant (string.Format ("%{0}", argument1.Value));

      var expectedResult = new SqlLikeExpression (objectExpression, rightExpression, new SqlLiteralExpression (@"\"));
      
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_ArgumentIsNull ()
    {
      var method = typeof (string).GetMethod ("EndsWith", new[] { typeof (string) });
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant (null, typeof(string));
      var expression = Expression.Call (objectExpression, method, argument1);
      var transformer = new EndsWithMethodCallTransformer ();
      
      var result = transformer.Transform (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (Expression.Constant(false), result);
    }

    [Test]
    public void Transform_ArgumentIsNotNullAndIsNoConstantValue_ ()
    {
      var method = typeof (string).GetMethod ("StartsWith", new[] { typeof (string) });
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.MakeMemberAccess (Expression.Constant (new Cook ()), typeof (Cook).GetProperty ("Name"));
      var expression = Expression.Call (objectExpression, method, argument1);
      var transformer = new EndsWithMethodCallTransformer ();

      var result = transformer.Transform (expression);

      Expression rightExpression = new SqlFunctionExpression (
          typeof (string),
          "REPLACE",
          new SqlFunctionExpression (
              typeof (string),
              "REPLACE",
                 new SqlFunctionExpression (
                    typeof (string),
                    "REPLACE",
                      new SqlFunctionExpression (
                          typeof (string),
                          "REPLACE",
                          argument1,
                          new SqlLiteralExpression (@"\"),
                          new SqlLiteralExpression (@"\\")),
                      new SqlLiteralExpression (@"%"),
                      new SqlLiteralExpression (@"\%")),
                  new SqlLiteralExpression (@"_"),
                  new SqlLiteralExpression (@"\_")),
              new SqlLiteralExpression (@"["),
              new SqlLiteralExpression (@"\["));
      rightExpression = Expression.Add (
          new SqlLiteralExpression ("%"), rightExpression, typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) }));
      var expectedResult = new SqlLikeExpression (objectExpression, rightExpression, new SqlLiteralExpression (@"\"));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}