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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;

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
  }

  public class DummyMetthodCallSqlGenerator : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      throw new System.NotImplementedException();
    }
  }
}
