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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class NameBasedMethodCallTransformerRegistryTest
  {
    private string _methodName;
    private MethodInfo _methodInfo;
    private NameBasedMethodCallTransformerRegistry _methodCallTransformerRegistry;
    private IMethodCallTransformer _transformerStub;

    [SetUp]
    public void SetUp ()
    {
      _methodName = "Concat";
      _methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _methodCallTransformerRegistry = new NameBasedMethodCallTransformerRegistry ();
      _transformerStub = MockRepository.GenerateStub<IMethodCallTransformer> ();
    }

    [Test]
    public void CreateDefault ()
    {
      NameBasedMethodCallTransformerRegistry registry = NameBasedMethodCallTransformerRegistry.CreateDefault ();
      registry.Register ("Test", new TestNameBasedMethodCallTransformer());

      AssertAllMethodsRegistered (registry, typeof (TestNameBasedMethodCallTransformer));
    }

    [Test]
    public void Register_NewMethod ()
    {
      _methodCallTransformerRegistry.Register (_methodName, _transformerStub);

      var expectedTransformer = _methodCallTransformerRegistry.GetItem (_methodName);
      Assert.That (_transformerStub, Is.SameAs (expectedTransformer));
    }

    [Test]
    public void Register_MethodTwice ()
    {
      _methodCallTransformerRegistry.Register (_methodName, _transformerStub);

      var transformerStub = MockRepository.GenerateStub<IMethodCallTransformer> ();
      _methodCallTransformerRegistry.Register (_methodName, transformerStub);

      var actualTransformer = _methodCallTransformerRegistry.GetItem (_methodName);
      
      Assert.That (actualTransformer, Is.Not.SameAs (_transformerStub));
      Assert.That (actualTransformer, Is.SameAs (transformerStub));
    }

    [Test]
    public void Register_SeveralMethodNames ()
    {
      var methodName = "EndsWith";
      var methodInfos = new List<string> { _methodName, methodName };
      _methodCallTransformerRegistry.Register (methodInfos, _transformerStub);

      var expectedGenerator = _methodCallTransformerRegistry.GetItem (_methodName);
      Assert.That (_transformerStub, Is.SameAs (expectedGenerator));

      var expectedGenerator2 = _methodCallTransformerRegistry.GetItem (methodName);
      Assert.That (_transformerStub, Is.SameAs (expectedGenerator2));
    }

    [Test]
    public void GetItem_MethodNameNotRegistered_ReturnsNull ()
    {
      var result = _methodCallTransformerRegistry.GetItem (_methodName);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetItem ()
    {
      _methodCallTransformerRegistry.Register (_methodName, _transformerStub);

      var result = _methodCallTransformerRegistry.GetItem (_methodName);

      Assert.That (result, Is.SameAs (_transformerStub));
    }

    [Test]
    public void GetTransformer ()
    {
      _methodCallTransformerRegistry.Register (_methodName, _transformerStub);
      var methodCallExpression = Expression.Call (_methodInfo, Expression.Constant ("a"), Expression.Constant ("b"));

      var result = _methodCallTransformerRegistry.GetTransformer (methodCallExpression);

      Assert.That (result, Is.SameAs (_transformerStub));
    }

    private void AssertAllMethodsRegistered (NameBasedMethodCallTransformerRegistry registry, Type type)
    {
      var methodNames = (string[]) type.GetField ("SupportedMethodNames").GetValue (null);
      Assert.That (methodNames.Length, Is.GreaterThan (0));

      foreach (var methodName in methodNames)
         Assert.That (registry.GetItem(methodName), Is.TypeOf (type));
    }
  }

  internal class TestNameBasedMethodCallTransformer : IMethodCallTransformer
  {
    public static string[] SupportedMethodNames = { "Test" };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      throw new NotImplementedException();
    }
  }
}