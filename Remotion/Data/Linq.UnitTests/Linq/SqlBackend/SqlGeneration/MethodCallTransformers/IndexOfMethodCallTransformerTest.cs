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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.MethodCallTransformers
{
  [TestFixture]
  public class IndexOfMethodCallTransformerTest
  {
    [Test]
    public void Transform_WithOneArgument_TypeString ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new [] { typeof(string) });
      var objectExpression = Expression.Constant ("Test");
      
      var argument1 = Expression.Constant("es");
      var expression = Expression.Call (objectExpression, method, argument1);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var lenExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var testPredicate = Expression.Equal (lenExpression, new SqlLiteralExpression (0));
      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX", argument1, objectExpression);
      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, new SqlLiteralExpression (0), elseValue);
      
      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithOneArgument_TypeChar ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new[] { typeof (char) });
      var objectExpression = Expression.Constant ("Test");

      var argument1 = Expression.Constant ('e');
      var expression = Expression.Call (objectExpression, method, argument1);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var lenExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var testPredicate = Expression.Equal (lenExpression, new SqlLiteralExpression (0));
      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX", argument1, objectExpression);
      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, new SqlLiteralExpression (0), elseValue);

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithTwoArgument_TypeString ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new[] { typeof (string), typeof(int) });
      var objectExpression = Expression.Constant ("Test");

      var argument1 = Expression.Constant ("es");
      var argument2 = Expression.Constant (2);
      var expression = Expression.Call (objectExpression, method, argument1, argument2);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var startIndexExpression = Expression.Add (argument2, new SqlLiteralExpression (1));

      var lenArgExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

      var lenObjectExpression = new SqlFunctionExpression (typeof (int), "LEN", objectExpression);
      var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
      var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX",argument1, objectExpression, startIndexExpression);

      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, argument2, elseValue);

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithTwoArgument_TypeChar ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new[] { typeof (char), typeof (int) });
      var objectExpression = Expression.Constant ("Test");

      var argument1 = Expression.Constant ('e');
      var argument2 = Expression.Constant (2);
      var expression = Expression.Call (objectExpression, method, argument1, argument2);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var startIndexExpression = Expression.Add (argument2, new SqlLiteralExpression (1));

      var lenArgExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

      var lenObjectExpression = new SqlFunctionExpression (typeof (int), "LEN", objectExpression);
      var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
      var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX", argument1, objectExpression, startIndexExpression);

      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, argument2, elseValue);

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithThreeArgument_TypeString ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new[] { typeof (string), typeof (int), typeof(int) });
      var objectExpression = Expression.Constant ("Test");

      var argument1 = Expression.Constant ("es");
      var argument2 = Expression.Constant (2);
      var argument3 = Expression.Constant (1);
      var expression = Expression.Call (objectExpression, method, argument1, argument2, argument3);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var startIndexExpression = Expression.Add (argument2, new SqlLiteralExpression (1));

      var lenArgExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

      var lenObjectExpression = new SqlFunctionExpression (typeof (int), "LEN", objectExpression);
      var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
      var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

      var startAddCountExpression = Expression.Add (argument2, argument3);
      var substringExpression = new SqlFunctionExpression (typeof (string), "SUBSTRING", objectExpression, new SqlLiteralExpression (1), startAddCountExpression);

      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX", argument1, substringExpression, startIndexExpression);

      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, argument2, elseValue);

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

    [Test]
    public void Transform_WithThreeArgument_TypeChar ()
    {
      var method = typeof (string).GetMethod ("IndexOf", new[] { typeof (char), typeof (int), typeof (int) });
      var objectExpression = Expression.Constant ("Test");

      var argument1 = Expression.Constant ('c');
      var argument2 = Expression.Constant (2);
      var argument3 = Expression.Constant (1);
      var expression = Expression.Call (objectExpression, method, argument1, argument2, argument3);
      var transformer = new IndexOfMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var startIndexExpression = Expression.Add (argument2, new SqlLiteralExpression (1));

      var lenArgExpression = new SqlFunctionExpression (typeof (int), "LEN", argument1);
      var leftTestPredicate = Expression.Equal (lenArgExpression, new SqlLiteralExpression (0));

      var lenObjectExpression = new SqlFunctionExpression (typeof (int), "LEN", objectExpression);
      var rightTestpredicate = Expression.LessThanOrEqual (startIndexExpression, lenObjectExpression);
      var testPredicate = Expression.AndAlso (leftTestPredicate, rightTestpredicate);

      var startAddCountExpression = Expression.Add (argument2, argument3);
      var substringExpression = new SqlFunctionExpression (typeof (string), "SUBSTRING", objectExpression, new SqlLiteralExpression (1), startAddCountExpression);

      var charIndexExpression = new SqlFunctionExpression (
          expression.Type, "CHARINDEX", argument1, substringExpression, startIndexExpression);

      var elseValue = Expression.Subtract (charIndexExpression, new SqlLiteralExpression (1));

      var fakeResult = new SqlCaseExpression (testPredicate, argument2, elseValue);

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }

  }
}