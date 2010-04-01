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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class MethodCallSqlGeneratorRegistryTest
  {
    private MethodInfo _methodInfo;
    private MethodCallTransformerRegistry _methodCallTransformerRegistry;
    private IMethodCallSqlGenerator _generatorStub;

    [SetUp]
    public void SetUp ()
    {
      _methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _methodCallTransformerRegistry = new MethodCallTransformerRegistry();
      _generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
    }

    [Test]
    public void CreateDefault ()
    {
      MethodCallTransformerRegistry registry = MethodCallTransformerRegistry.CreateDefault ();

      AssertAllMethodsRegistered (registry, typeof (ContainsMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (ConvertMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (EndsWithMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (LowerMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (RemoveMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (StartsWithMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (SubstringMethodCallSqlGenerator));
      AssertAllMethodsRegistered (registry, typeof (UpperMethodCallSqlGenerator));
    }

    private void AssertAllMethodsRegistered (MethodCallTransformerRegistry registry, Type type)
    {
      var methodInfos = (MethodInfo[]) type.GetField ("SupportedMethods").GetValue (null);
      Assert.That (methodInfos.Length, Is.GreaterThan (0));

      foreach (var methodInfo in methodInfos)
        Assert.That (registry.GetGenerator (methodInfo), Is.TypeOf(type));
    }

    [Test]
    public void Register_NewMethod ()
    {
      _methodCallTransformerRegistry.Register (_methodInfo, _generatorStub);

      var expectedGenerator = _methodCallTransformerRegistry.GetGenerator (_methodInfo);
      Assert.That (_generatorStub, Is.SameAs (expectedGenerator));
    }

    [Test]
    public void Register_MethodTwice ()
    {
      _methodCallTransformerRegistry.Register (_methodInfo, _generatorStub);

      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
      _methodCallTransformerRegistry.Register (_methodInfo, generatorStub);

      var expectedGenerator = _methodCallTransformerRegistry.GetGenerator (_methodInfo);
      Assert.That (_generatorStub, Is.Not.EqualTo (expectedGenerator));
      Assert.That (generatorStub, Is.EqualTo (expectedGenerator));
    }

    [Test]
    public void Register_SeveralMethodInfos ()
    {
      var methodInfo = typeof (string).GetMethod ("EndsWith", new[] { typeof (string) });
      IEnumerable<MethodInfo> methodInfos = new List<MethodInfo> { _methodInfo, methodInfo };
      _methodCallTransformerRegistry.Register (methodInfos, _generatorStub);

      var expectedGenerator = _methodCallTransformerRegistry.GetGenerator (_methodInfo);
      Assert.That (_generatorStub, Is.SameAs (expectedGenerator));

      var expectedGenerator2 = _methodCallTransformerRegistry.GetGenerator (methodInfo);
      Assert.That (_generatorStub, Is.SameAs (expectedGenerator2));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The method 'System.String.Concat' is not supported by this code "
                                                                          + "generator, and no custom generator has been registered.")]
    public void GetGenerator_DontFindGenerator_Exception ()
    {
      var methodCallSqlGeneratorRegistry = new MethodCallTransformerRegistry();
      methodCallSqlGeneratorRegistry.GetGenerator (_methodInfo);
    }

    [Test]
    public void GetGenerator_ForGenericMethodInfo ()
    {
      var genericMethodDefinition = (from m in typeof (Queryable).GetMethods()
                                     where m.Name == "Distinct" && m.GetParameters().Length == 1
                                     select m).Single();
      var closedGenericMethod = genericMethodDefinition.MakeGenericMethod (typeof (Cook));

      _methodCallTransformerRegistry.Register (genericMethodDefinition, _generatorStub);

      var expectedGenerator = _methodCallTransformerRegistry.GetGenerator (closedGenericMethod);

      Assert.That (expectedGenerator, Is.SameAs (_generatorStub));
    }

    [Test]
    public void GetGenerator_ForBaseDefintion ()
    {
      object test = new object();
      var methodInfo = ReflectionUtility.GetMethod (() => test.ToString());

      _methodCallTransformerRegistry.Register (methodInfo, _generatorStub);

      int i = 5;
      var intMethodInfo = ReflectionUtility.GetMethod (() => i.ToString());
      var result = _methodCallTransformerRegistry.GetGenerator (intMethodInfo);

      Assert.That (result, Is.EqualTo (_generatorStub));
    }
  }
}