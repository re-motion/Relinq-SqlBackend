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
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlPreparation;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class AttributeBasedMethodCallTransformerProviderTest
  {
    private AttributeBasedMethodCallTransformerProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _provider = new AttributeBasedMethodCallTransformerProvider ();
    }

    [Test]
    public void GetTransformer_NonAttributedMethod ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<Cook, string> (c => c.ToString ());

      var result = _provider.GetTransformer (methodCallExpression);
      
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetTransformer_AttributedMethod ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<Cook, string> (c => c.GetFullName());

      var result = _provider.GetTransformer (methodCallExpression);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (Cook.FullNameTransformer)));
    }

    [Test]
    public void GetTransformer_AttributedMethod_Inherited ()
    {
      var methodCallExpression = Expression.Call (Expression.Constant (null, typeof (Chef)), typeof (Chef).GetMethod ("GetFullName"));

      var result = _provider.GetTransformer (methodCallExpression);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (Cook.FullNameTransformer)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The method 'Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.AttributeBasedMethodCallTransformerProviderTest.TooManyAttributes' is "
        + "attributed with more than one IMethodCallTransformerAttribute: MethodCallTransformer2Attribute, MethodCallTransformerAttribute. Only one "
        + "such attribute is allowed.")]
    public void GetTransformer_TooManyAttributes ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => TooManyAttributes());

      _provider.GetTransformer (methodCallExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "The method "
        + "'Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.AttributeBasedMethodCallTransformerProviderTest.ThrowingTransformer' "
        + "cannot be transformed to SQL. Exception thrown in GetTransformer")]
    public void GetTransformer_ErrorInstantiatingTransformer ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => ThrowingTransformer());
      _provider.GetTransformer (methodCallExpression);
    }

    [MethodCallTransformer (typeof (Cook.FullNameTransformer))]
    [MethodCallTransformer2 (typeof (Cook.FullNameTransformer))]
    private string TooManyAttributes ()
    {
      throw new NotImplementedException();
    }

    [ThrowingMethodCallTransformer]
    private string ThrowingTransformer ()
    {
      throw new NotImplementedException ();
    }

    private class MethodCallTransformer2Attribute : MethodCallTransformerAttribute {
      public MethodCallTransformer2Attribute (Type transformerType)
          : base(transformerType)
      {
      }
    }

    private class ThrowingMethodCallTransformerAttribute : Attribute, IMethodCallTransformerAttribute
    {
      public IMethodCallTransformer GetTransformer ()
      {
        throw new InvalidOperationException ("Exception thrown in GetTransformer");
      }
    }
  }
}