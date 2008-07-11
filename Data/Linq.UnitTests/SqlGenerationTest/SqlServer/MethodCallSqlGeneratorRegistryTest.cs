/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
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
    [ExpectedException (typeof (SqlGenerationException), ExpectedMessage = "The method System.String.Concat is not supported by the SQL Server code "+
      "generator, and no custom generator has been registered.")]
    public void DontFindGenerator_Exception ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry ();

      methodCallSqlGeneratorRegistry.GetGenerator (methodInfo);
    }

    [Test]
    public void DefaultRegistration ()
    {
      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new MethodCallSqlGeneratorRegistry();

      IMethodCallSqlGenerator removeGenerator =
        methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("Remove", new Type[] { typeof (int) }));

      IMethodCallSqlGenerator upperGenerator =
        methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("ToUpper", new Type[] { }));

      Assert.IsNotNull (removeGenerator);
      Assert.IsNotNull (upperGenerator);
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