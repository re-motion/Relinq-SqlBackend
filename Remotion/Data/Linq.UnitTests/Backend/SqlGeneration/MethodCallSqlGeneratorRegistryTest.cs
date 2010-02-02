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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration
{
  [TestFixture]
  public class MethodCallSqlGeneratorRegistryTest
  {
    [Test]
    public void RegisterNewMethod ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var generator = new DummyMetthodCallSqlGenerator();

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();

      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      IMethodCallSqlGenerator expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);

      Assert.That (generator, Is.EqualTo (expectedGenerator));
    }

    [Test]
    public void RegisterMethodTwice ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var generator = new DummyMetthodCallSqlGenerator();

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();

      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      var generator2 = new DummyMetthodCallSqlGenerator();
      methodCallSqlGeneratorRegistry.Register (methodInfo, generator2);

      IMethodCallSqlGenerator expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);

      Assert.That (generator, Is.Not.EqualTo (expectedGenerator));
      Assert.That (generator2, Is.EqualTo (expectedGenerator));
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException), ExpectedMessage = "The method System.String.Concat is not supported by this code "
                                                                           + "generator, and no custom generator has been registered.")]
    public void DontFindGenerator_Exception ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();

      methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
    }

    [Test]
    public void GetGeneratorForGenericMethodInfo ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ReflectionUtility.GetMethod (() => source.Distinct());

      var methodInfoDistinct = (from m in typeof (Queryable).GetMethods()
                                where m.Name == "Distinct" && m.GetParameters().Length == 1
                                select m).Single();
      var registry = new MethodCallSqlGeneratorRegistry();
      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
      registry.Register (methodInfoDistinct, generatorStub);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.That (expectedGenerator, Is.SameAs (generatorStub));
    }

    [Test]
    public void GetGeneratorForMethodWithOneParameter ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ReflectionUtility.GetMethod (() => source.Count());

      var methodInfoCount = (from m in typeof (Queryable).GetMethods()
                             where m.Name == "Count" && m.GetParameters().Length == 1
                             select m).Single();

      var registry = new MethodCallSqlGeneratorRegistry();
      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
      registry.Register (methodInfoCount, generatorStub);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.That (expectedGenerator, Is.SameAs (generatorStub));
    }

    [Test]
    public void GetGeneratorForMethodWithTwoParameters ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ReflectionUtility.GetMethod (() => source.Single (s => s.ID == 5));

      var methodInfoSingle = (from m in typeof (Queryable).GetMethods()
                              where m.Name == "Single" && m.GetParameters().Length == 2
                              select m).Single();

      var registry = new MethodCallSqlGeneratorRegistry();
      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator> ();
      registry.Register (methodInfoSingle, generatorStub);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.That (expectedGenerator, Is.EqualTo (generatorStub));
    }
  }

  public class DummyMetthodCallSqlGenerator : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      throw new NotImplementedException();
    }
  }
}