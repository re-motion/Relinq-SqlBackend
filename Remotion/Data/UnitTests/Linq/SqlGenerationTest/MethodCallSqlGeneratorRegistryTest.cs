// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Rhino.Mocks.Constraints;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest
{
  [TestFixture]
  public class MethodCallSqlGeneratorRegistryTest
  {
    [Test]
    public void RegisterNewMethod ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      DummyMetthodCallSqlGenerator generator = new DummyMetthodCallSqlGenerator();

      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();

      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      IMethodCallSqlGenerator expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);

      Assert.AreEqual (expectedGenerator, generator);
    }

    [Test]
    public void RegisterMethodTwice ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      DummyMetthodCallSqlGenerator generator = new DummyMetthodCallSqlGenerator ();

      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry ();

      methodCallSqlGeneratorRegistry.Register (methodInfo, generator);

      DummyMetthodCallSqlGenerator generator2 = new DummyMetthodCallSqlGenerator ();
      methodCallSqlGeneratorRegistry.Register (methodInfo, generator2);

      IMethodCallSqlGenerator expectedGenerator = methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);

      Assert.AreNotEqual (expectedGenerator, generator);
      Assert.AreEqual (expectedGenerator, generator2);
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException), ExpectedMessage = "The method System.String.Concat is not supported by this code "
      + "generator, and no custom generator has been registered.")]
    public void DontFindGenerator_Exception ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry ();

      methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
    }

    [Test]
    public void GetGeneratorForGenericMethodInfo ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ParserUtility.GetMethod (() => source.Distinct ());
      
      var methodInfoDistinct = (from m in typeof (Queryable).GetMethods ()
                                where m.Name == "Distinct" && m.GetParameters ().Length == 1
                                select m).Single ();
      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry();
      MethodCallDistinct generator = new MethodCallDistinct();
      registry.Register (methodInfoDistinct, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }

    [Test]
    public void GetGeneratorForCountWithOneParameter ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ParserUtility.GetMethod (() => source.Count ());

      var methodInfoCount = (from m in typeof (Queryable).GetMethods ()
                                where m.Name == "Count" && m.GetParameters ().Length == 1
                                select m).Single ();

      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry ();
      MethodCallCount generator = new MethodCallCount ();
      registry.Register (methodInfoCount, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }

    [Test]
    public void GetGeneratorForFirstWithOneParameter ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ParserUtility.GetMethod (() => source.First ());

      var methodInfoFirst = (from m in typeof (Queryable).GetMethods ()
                             where m.Name == "First" && m.GetParameters ().Length == 1
                             select m).Single ();

      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry ();
      MethodCallFirst generator = new MethodCallFirst ();
      registry.Register (methodInfoFirst, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }

    [Test]
    public void GetGeneratorForSingleWithOneParameter ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ParserUtility.GetMethod (() => source.Single ());

      var methodInfoSingle = (from m in typeof (Queryable).GetMethods ()
                             where m.Name == "Single" && m.GetParameters ().Length == 1
                             select m).Single ();

      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry ();
      MethodCallSingle generator = new MethodCallSingle ();
      registry.Register (methodInfoSingle, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }

    [Test]
    public void GetGeneratorForSingleWithOneParameter2 ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => query.Single ());

      var methodInfoSingle = (from m in typeof (Queryable).GetMethods ()
                              where m.Name == "Single" && m.GetParameters ().Length == 1
                              select m).Single ();

      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry ();
      MethodCallSingle generator = new MethodCallSingle ();
      registry.Register (methodInfoSingle, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }

    [Test]
    public void GetGeneratorForSingleWithTwoParameter ()
    {
      IQueryable<Student> source = null;
      var methodInfo = ParserUtility.GetMethod (() => source.Single (s => s.ID == 5));

      var methodInfoSingle = (from m in typeof (Queryable).GetMethods ()
                              where m.Name == "Single" && m.GetParameters ().Length == 2
                              select m).Single ();

      MethodCallSqlGeneratorRegistry registry = new MethodCallSqlGeneratorRegistry ();
      MethodCallSingle generator = new MethodCallSingle ();
      registry.Register (methodInfoSingle, generator);

      var expectedGenerator = registry.GetGenerator (methodInfo);

      Assert.AreEqual (generator, expectedGenerator);
    }
    
  }

  public class DummyMetthodCallSqlGenerator : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      throw new System.NotImplementedException();
    }
  }
}
