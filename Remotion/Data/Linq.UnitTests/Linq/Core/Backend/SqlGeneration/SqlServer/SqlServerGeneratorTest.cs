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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Backend;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorTest
  {
    private SqlServerGenerator _sqlServerGenerator;

    [SetUp]
    public void SetUp ()
    {
      _sqlServerGenerator = new SqlServerGenerator (StubDatabaseInfo.Instance);
    }

    [Test]
    public void DefaultMethodCallRegistration ()
    {
      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = _sqlServerGenerator.MethodCallRegistry;

      IMethodCallSqlGenerator removeGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("Remove", new[] { typeof (int) }));

      IMethodCallSqlGenerator upperGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("ToUpper", new Type[] { }));

      Assert.That (removeGenerator, Is.Not.Null);
      Assert.That (upperGenerator, Is.Not.Null);
    }

    [Test]
    public void CreateNestedSqlGenerator ()
    {
      var result = _sqlServerGenerator.CreateNestedSqlGenerator (ParseMode.SubQueryInFrom);

      Assert.That (result.ParseMode, Is.EqualTo (ParseMode.SubQueryInFrom));
      Assert.That (result.DatabaseInfo, Is.SameAs (_sqlServerGenerator.DatabaseInfo));
    }

    [Test]
    public void CreateDerivedContext ()
    {
      var commandText = new StringBuilder ();
      var commandParameters = new List<CommandParameter> ();
      var commandBuilder = new CommandBuilder (
          new SqlServerGenerator(StubDatabaseInfo.Instance), 
          commandText, 
          commandParameters, 
          MockRepository.GenerateStub<IDatabaseInfo>(), 
          new MethodCallSqlGeneratorRegistry());

      var result = _sqlServerGenerator.CreateDerivedContext (commandBuilder);

      Assert.That (result.CommandBuilder, Is.Not.SameAs (commandBuilder));
      Assert.That (result.CommandBuilder.DatabaseInfo, Is.SameAs (_sqlServerGenerator.DatabaseInfo));
      Assert.That (result.CommandBuilder.CommandText, Is.SameAs (commandText));
      Assert.That (result.CommandBuilder.CommandParameters, Is.SameAs (commandParameters));
      Assert.That (result.CommandBuilder.MethodCallRegistry, Is.SameAs (_sqlServerGenerator.MethodCallRegistry));
      Assert.That (result.CommandBuilder.SqlGenerator, Is.SameAs (_sqlServerGenerator));
    }

  }
}
