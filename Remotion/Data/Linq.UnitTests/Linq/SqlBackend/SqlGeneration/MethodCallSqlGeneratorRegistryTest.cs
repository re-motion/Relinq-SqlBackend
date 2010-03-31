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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class MethodCallSqlGeneratorRegistryTest
  {
    private MethodInfo _methodInfo;
    private MethodCallSqlGeneratorRegistry _methodCallSqlGeneratorRegistry;
    private IMethodCallSqlGenerator _generatorStub;

    [SetUp]
    public void SetUp ()
    {
      _methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();
      _generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
    }

    [Test]
    public void Register_NewMethod ()
    {
      _methodCallSqlGeneratorRegistry.Register (_methodInfo, _generatorStub);

      var expectedGenerator = _methodCallSqlGeneratorRegistry.GetGenerator (_methodInfo);
      Assert.That (_generatorStub, Is.SameAs (expectedGenerator));
    }

    [Test]
    public void Register_MethodTwice ()
    {
      _methodCallSqlGeneratorRegistry.Register (_methodInfo, _generatorStub);

      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
      _methodCallSqlGeneratorRegistry.Register (_methodInfo, generatorStub);

      var expectedGenerator = _methodCallSqlGeneratorRegistry.GetGenerator (_methodInfo);
      Assert.That (_generatorStub, Is.Not.EqualTo (expectedGenerator));
      Assert.That (generatorStub, Is.EqualTo (expectedGenerator));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The method 'System.String.Concat' is not supported by this code "
                                                                          + "generator, and no custom generator has been registered.")]
    public void GetGenerator_DontFindGenerator_Exception ()
    {
      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();
      methodCallSqlGeneratorRegistry.GetGenerator (_methodInfo);
    }

    [Test]
    public void GetGenerator_ForGenericMethodInfo ()
    {
      var genericMethodDefinition = (from m in typeof (Queryable).GetMethods()
                                     where m.Name == "Distinct" && m.GetParameters().Length == 1
                                     select m).Single();
      var closedGenericMethod = genericMethodDefinition.MakeGenericMethod (typeof (Cook));

      _methodCallSqlGeneratorRegistry.Register (genericMethodDefinition, _generatorStub);

      var expectedGenerator = _methodCallSqlGeneratorRegistry.GetGenerator (closedGenericMethod);

      Assert.That (expectedGenerator, Is.SameAs (_generatorStub));
    }

    [Test]
    public void GetGenerator_ForBaseDefintion ()
    {
      object test = new object();
      var methodInfo = ReflectionUtility.GetMethod (() => test.ToString());

      _methodCallSqlGeneratorRegistry.Register (methodInfo, _generatorStub);

      int i = 5;
      var intMethodInfo = ReflectionUtility.GetMethod (() => i.ToString());
      var result = _methodCallSqlGeneratorRegistry.GetGenerator (intMethodInfo);

      Assert.That (result, Is.EqualTo (_generatorStub));
    }
  }
}