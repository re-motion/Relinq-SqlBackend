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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class MethodInfoBasedMethodCallTransformerRegistryTest
  {
    private MethodInfo _methodInfo;
    private MethodInfoBasedMethodCallTransformerRegistry _methodCallTransformerRegistry;
    private Mock<IMethodCallTransformer> _transformerStub;

    [SetUp]
    public void SetUp ()
    {
      _methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _methodCallTransformerRegistry = new MethodInfoBasedMethodCallTransformerRegistry ();
      _transformerStub = new Mock<IMethodCallTransformer>();
    }

    [Test]
    public void CreateDefault ()
    {
      MethodInfoBasedMethodCallTransformerRegistry registry = MethodInfoBasedMethodCallTransformerRegistry.CreateDefault ();

      AssertAllMethodsRegistered (registry, typeof (ContainsMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (EndsWithMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (IndexOfMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (LowerMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (RemoveMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (ReplaceMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (StartsWithMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (SubstringMethodCallTransformer));
      AssertAllMethodsRegistered (registry, typeof (UpperMethodCallTransformer));
    }

    [Test]
    public void Register_NewMethod ()
    {
      _methodCallTransformerRegistry.Register (_methodInfo, _transformerStub.Object);

      var expectedTransformer = _methodCallTransformerRegistry.GetItem (_methodInfo);
      Assert.That (_transformerStub.Object, Is.SameAs (expectedTransformer));
    }

    [Test]
    public void Register_MethodTwice ()
    {
      _methodCallTransformerRegistry.Register (_methodInfo, _transformerStub.Object);

      var transformerStub = new Mock<IMethodCallTransformer>();
      _methodCallTransformerRegistry.Register (_methodInfo, transformerStub.Object);

      var expectedTransformer = _methodCallTransformerRegistry.GetItem (_methodInfo);
      Assert.That (_transformerStub.Object, Is.Not.EqualTo (expectedTransformer));
      Assert.That (transformerStub.Object, Is.EqualTo (expectedTransformer));
    }

    [Test]
    public void Register_SeveralMethodInfos ()
    {
      var methodInfo = typeof (string).GetMethod ("EndsWith", new[] { typeof (string) });
      IEnumerable<MethodInfo> methodInfos = new List<MethodInfo> { _methodInfo, methodInfo };
      _methodCallTransformerRegistry.Register (methodInfos, _transformerStub.Object);

      var expectedGenerator = _methodCallTransformerRegistry.GetItem (_methodInfo);
      Assert.That (_transformerStub.Object, Is.SameAs (expectedGenerator));

      var expectedGenerator2 = _methodCallTransformerRegistry.GetItem (methodInfo);
      Assert.That (_transformerStub.Object, Is.SameAs (expectedGenerator2));
    }

    [Test]
    public void GetTransformer_DontFindGenerator_ReturnsNull ()
    {
      var methodCallSqlGeneratorRegistry = new MethodInfoBasedMethodCallTransformerRegistry ();
      var methodCallExpression = Expression.Call (_methodInfo, Expression.Constant ("a"), Expression.Constant ("b"));

      var result = methodCallSqlGeneratorRegistry.GetTransformer (methodCallExpression);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetItem_DontFindGenerator_ReturnsNull ()
    {
      var methodCallSqlGeneratorRegistry = new MethodInfoBasedMethodCallTransformerRegistry ();

      var result = methodCallSqlGeneratorRegistry.GetItem (_methodInfo);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetTransformer ()
    {
      var methodCallSqlGeneratorRegistry = new MethodInfoBasedMethodCallTransformerRegistry ();
      methodCallSqlGeneratorRegistry.Register (_methodInfo, _transformerStub.Object);
      var methodCallExpression = Expression.Call (_methodInfo, Expression.Constant ("a"), Expression.Constant ("b"));

      var result = methodCallSqlGeneratorRegistry.GetTransformer (methodCallExpression);

      Assert.That (result, Is.SameAs (_transformerStub.Object));
    }

    [Test]
    public void GetItem ()
    {
      var methodCallSqlGeneratorRegistry = new MethodInfoBasedMethodCallTransformerRegistry ();
      methodCallSqlGeneratorRegistry.Register (_methodInfo, _transformerStub.Object);

      var result = methodCallSqlGeneratorRegistry.GetItem (_methodInfo);

      Assert.That (result, Is.SameAs (_transformerStub.Object));
    }

    [Test]
    public void GetTransformer_ForGenericMethodInfo ()
    {
      var genericMethodDefinition = (from m in typeof (Queryable).GetMethods ()
                                     where m.Name == "Distinct" && m.GetParameters ().Length == 1
                                     select m).Single ();
      var closedGenericMethod = genericMethodDefinition.MakeGenericMethod (typeof (Cook));
      var methodCallExpression = Expression.Call (closedGenericMethod, Expression.Constant (null, typeof (IQueryable<>).MakeGenericType (typeof (Cook))));

      _methodCallTransformerRegistry.Register (genericMethodDefinition, _transformerStub.Object);

      var result = _methodCallTransformerRegistry.GetTransformer (methodCallExpression);

      Assert.That (result, Is.SameAs (_transformerStub.Object));
    }

    [Test]
    public void GetItem_ForGenericMethodInfo ()
    {
      var genericMethodDefinition = (from m in typeof (Queryable).GetMethods ()
                                     where m.Name == "Distinct" && m.GetParameters ().Length == 1
                                     select m).Single ();
      var closedGenericMethod = genericMethodDefinition.MakeGenericMethod (typeof (Cook));

      _methodCallTransformerRegistry.Register (genericMethodDefinition, _transformerStub.Object);

      var expectedTransformer = _methodCallTransformerRegistry.GetItem (closedGenericMethod);

      Assert.That (expectedTransformer, Is.SameAs (_transformerStub.Object));
    }

    [Test]
    public void GetTransformer_ForBaseDefinition ()
    {
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => new object ().ToString ());
      _methodCallTransformerRegistry.Register (methodInfo, _transformerStub.Object);

      int i = 5;
      var methodCallExpression = Expression.Call (Expression.Constant (i), MemberInfoFromExpressionUtility.GetMethod (() => i.ToString ()));
      var result = _methodCallTransformerRegistry.GetTransformer (methodCallExpression);

      Assert.That (result, Is.EqualTo (_transformerStub.Object));
    }

    [Test]
    public void GetItem_ForBaseDefintion ()
    {
      var methodInfo = MemberInfoFromExpressionUtility.GetMethod (() => new object ().ToString ());

      _methodCallTransformerRegistry.Register (methodInfo, _transformerStub.Object);

      int i = 5;
      var intMethodInfo = MemberInfoFromExpressionUtility.GetMethod (() => i.ToString ());
      var result = _methodCallTransformerRegistry.GetItem (intMethodInfo);

      Assert.That (result, Is.EqualTo (_transformerStub.Object));
    }

    private void AssertAllMethodsRegistered (MethodInfoBasedMethodCallTransformerRegistry registry, Type type)
    {
      var methodInfos = (MethodInfo[]) type.GetField ("SupportedMethods").GetValue (null);
      Assert.That (methodInfos.Length, Is.GreaterThan (0));

      foreach (var methodInfo in methodInfos)
        Assert.That (registry.GetItem (methodInfo), Is.TypeOf (type));
    }
  }
}