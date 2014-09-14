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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class ConcatMethodCallTransformerTest
  {
    private ConcatMethodCallTransformer _transformer;
    private MethodInfo _twoStringConcatMethod;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new ConcatMethodCallTransformer ();
      _twoStringConcatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
    }

    [Test]
    public void SupportedMethods ()
    {
      // Exclude the Concat (object, object, object, object, __arglist) overload.
      var concatMethods =
          typeof (string).GetMethods()
                         .Where (mi => mi.Name == "Concat" && mi.CallingConvention != CallingConventions.VarArgs)
                         .ToArray();
      Assert.That (ConcatMethodCallTransformer.SupportedMethods, Is.EquivalentTo (concatMethods));
    }

    [Test]
    public void Transform_SingleObject ()
    {
      var argument0 = Expression.Constant (new object());
      var expression = Expression.Call (typeof (string), "Concat", Type.EmptyTypes, argument0);

      var result = _transformer.Transform (expression);

      var expectedExpression = new SqlConvertExpression (typeof (string), argument0);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_SingleString ()
    {
      var argument0 = Expression.Constant ("test");
      var expression = Expression.Call (typeof (string), "Concat", Type.EmptyTypes, argument0);

      var result = _transformer.Transform (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (argument0, result);
    }

    [Test]
    public void Transform_TwoObjects ()
    {
      var argument0 = Expression.Constant (new object ());
      var argument1 = Expression.Constant (new object ());
      var expression = Expression.Call (typeof (string), "Concat", Type.EmptyTypes, argument0, argument1);

      var result = _transformer.Transform (expression);

      var expectedExpression = Expression.Add (
          new SqlConvertExpression (typeof (string), argument0), 
          new SqlConvertExpression (typeof (string), argument1),
          _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_MoreObjects ()
    {
      var argument0 = Expression.Constant (new object ());
      var argument1 = Expression.Constant (new object ());
      var argument2 = Expression.Constant (new object ());
      var expression = Expression.Call (typeof (string), "Concat", Type.EmptyTypes, argument0, argument1, argument2);

      var result = _transformer.Transform (expression);

      var expectedExpression = 
          Expression.Add (
              Expression.Add (
                  new SqlConvertExpression (typeof (string), argument0),
                  new SqlConvertExpression (typeof (string), argument1),
                  _twoStringConcatMethod),
              new SqlConvertExpression (typeof (string), argument2),
              _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_Strings ()
    {
      var argument0 = Expression.Constant ("a");
      var argument1 = Expression.Constant ("b");
      var argument2 = Expression.Constant ("c");
      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string), typeof (string) }), 
          argument0, 
          argument1, 
          argument2);

      var result = _transformer.Transform (expression);

      var expectedExpression = 
          Expression.Add (
              Expression.Add (
                  argument0, 
                  argument1, 
                  _twoStringConcatMethod), 
              argument2, 
              _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_NewObjectArray ()
    {
      var element0 = Expression.Constant ("a");
      var element1 = Expression.Constant (new object());
      var element2 = Expression.Constant ("c");

      var argument = Expression.NewArrayInit (typeof (object), element0, element1, element2);

      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (object[]) }),
          argument);

      var result = _transformer.Transform (expression);

      var expectedExpression =
          Expression.Add (
              Expression.Add (
                  element0,
                  new SqlConvertExpression (typeof (string), element1),
                  _twoStringConcatMethod),
              element2,
              _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_NewStringArray ()
    {
      var element0 = Expression.Constant ("a");
      var element1 = Expression.Constant ("b");
      var element2 = Expression.Constant ("c");
      
      var argument = Expression.NewArrayInit (typeof (string), element0, element1, element2);

      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (string[]) }),
          argument);

      var result = _transformer.Transform (expression);

      var expectedExpression =
          Expression.Add (
              Expression.Add (
                  element0,
                  element1,
                  _twoStringConcatMethod),
              element2,
              _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_ConstantArray ()
    {
      var elements = new[] { "a", "b", "c" };
      var argument = Expression.Constant (elements);

      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (string[]) }),
          argument);

      var result = _transformer.Transform (expression);

      var expectedExpression =
          Expression.Add (
              Expression.Add (
                  Expression.Constant (elements[0]),
                  Expression.Constant (elements[1]),
                  _twoStringConcatMethod),
              Expression.Constant (elements[2]),
              _twoStringConcatMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The method call 'Concat(CustomExpression)' is not supported. When the array overloads of String.Concat are used, only constant or new array "
        + "expressions can be translated to SQL; in this usage, the expression has type "
        + "'Remotion.Linq.SqlBackend.UnitTests.CustomExpression'.")]
    public void Transform_NonParseableArray ()
    {
      var argument = new CustomExpression (typeof (string[]));

      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (string[]) }),
          argument);

      _transformer.Transform (expression);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The method call 'Concat(CustomExpression)' is not supported. When the array overloads of String.Concat are used, only constant or new array "
        + "expressions can be translated to SQL; in this usage, the expression has type "
        + "'Remotion.Linq.SqlBackend.UnitTests.CustomExpression'.")]
    public void Transform_NonParseableEnumerable ()
    {
      var argument = new CustomExpression (typeof (IEnumerable<string>));

      var expression = Expression.Call (
          typeof (string).GetMethod ("Concat", new[] { typeof (IEnumerable<string>) }),
          argument);

      _transformer.Transform (expression);
    }
  }
}