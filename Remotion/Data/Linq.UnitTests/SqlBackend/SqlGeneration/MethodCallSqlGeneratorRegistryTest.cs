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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class MethodCallSqlGeneratorRegistryTest
  {
    [Test]
    public void Register_NewMethod ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) }); // TODO Review 2364: Make methodInfo a field
      var generator = new DummyMetthodCallSqlGenerator (); // TODO Review 2364: Typo, rename - consider using stub instead of creating a dummy implementation (MockRepository.GenerateStub)

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry (); // TODO Review 2364: Field?

      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      var expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
      Assert.That (generator, Is.EqualTo (expectedGenerator)); // TODO Review 2364: Use SameAs - references should be checked here.
    }

    [Test]
    public void Register_MethodTwice ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      var generator = new DummyMetthodCallSqlGenerator();

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();
      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      var generator2 = new DummyMetthodCallSqlGenerator();
      methodCallSqlGeneratorRegistry.Register (methodInfo, generator2);

      var expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
      Assert.That (generator, Is.Not.EqualTo (expectedGenerator));
      Assert.That (generator2, Is.EqualTo (expectedGenerator));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The method 'System.String.Concat' is not supported by this code "
                                                                           + "generator, and no custom generator has been registered.")]
    public void GetGenerator_DontFindGenerator_Exception ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

      var methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry ();

      methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
    }

    [Test]
    public void GetGenerator_ForGenericMethodInfo ()
    {
      var genericMethodDefinition = (from m in typeof (Queryable).GetMethods()
                                     where m.Name == "Distinct" && m.GetParameters().Length == 1
                                     select m).Single();
      var closedGenericMethod = genericMethodDefinition.MakeGenericMethod (typeof (Cook));

      var registry = new MethodCallSqlGeneratorRegistry ();
      var generatorStub = MockRepository.GenerateStub<IMethodCallSqlGenerator>();
      registry.Register (genericMethodDefinition, generatorStub);

      var expectedGenerator = registry.GetGenerator (closedGenericMethod);

      Assert.That (expectedGenerator, Is.SameAs (generatorStub));
    }

    // TODO Review 2364: This test doesn't seem to test anything new?
    [Test]
    public void GetGeneratorForMethodWithOneParameter ()
    {
      IQueryable<Cook> source = null;
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

    // TODO Review 2364: This test doesn't seem to test anything new?
    [Test]
    public void GetGeneratorForMethodWithTwoParameters ()
    {
      IQueryable<Cook> source = null;
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

  // TODO Review 2364: Move to separate file (or remove and use stubs instead).
  public class DummyMetthodCallSqlGenerator : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCallExpression methodCallExpression, SqlCommandBuilder commandBuilder, ExpressionTreeVisitor expressionTreeVisitor)
    {
      throw new NotImplementedException();
    }
  }
}