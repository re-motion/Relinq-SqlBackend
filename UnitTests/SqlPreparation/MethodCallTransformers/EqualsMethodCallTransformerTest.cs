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
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class EqualsMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (EqualsMethodCallTransformer.SupportedMethodNames, Has.Member ("Equals"));
    }

    [Test]
    public void Transform_InstanceMethod ()
    {
      var method = typeof (object).GetMethod ("Equals", new[] { typeof(object) });
      var instance = Expression.Constant (new object());
      var argument = Expression.Constant(new object());
      var expression = Expression.Call (instance, method, argument);

      var transformer = new EqualsMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var expectedResult = Expression.Equal (instance, argument);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_InstanceMethod_IncompatibleTypes ()
    {
      var method = typeof (object).GetMethod ("Equals", new[] { typeof (object) });
      var instance = Expression.Constant (0);
      var argument = Expression.Constant ("string");
      var expression = Expression.Call (instance, method, argument);

      var transformer = new EqualsMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var expectedResult = Expression.Equal (Expression.Convert (instance, typeof (object)), Expression.Convert (argument, typeof (object)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_StaticMethod ()
    {
      var method = typeof (object).GetMethod ("Equals", new[] { typeof (object), typeof(object) });
      var parameter1 = Expression.Constant (new object ());
      var parameter2 = Expression.Constant (new object ());
      var expression = Expression.Call (method, parameter1, parameter2);

      var transformer = new EqualsMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var expectedResult = Expression.Equal (parameter1, parameter2);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_StaticMethod_NonMatchingTypes ()
    {
      var method = typeof (object).GetMethod ("Equals", new[] { typeof (object), typeof (object) });
      var parameter1 = Expression.Constant ("s");
      var parameter2 = Expression.Constant (null, typeof (Cook));
      var expression = Expression.Call (method, parameter1, parameter2);

      var transformer = new EqualsMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var expectedResult = Expression.Equal (Expression.Convert (parameter1, typeof (object)), Expression.Convert (parameter2, typeof (object)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Transform_WrongNumberOfArguments ()
    {
      var method = typeof (object).GetMethod ("ToString", new Type[0]);
      var expression = Expression.Call (Expression.Constant("test"), method);

      var transformer = new EqualsMethodCallTransformer ();
      Assert.That (
          () => transformer.Transform (expression),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("ToString function with 0 arguments is not supported. Expression: '\"test\".ToString()'"));
    }
  }
}