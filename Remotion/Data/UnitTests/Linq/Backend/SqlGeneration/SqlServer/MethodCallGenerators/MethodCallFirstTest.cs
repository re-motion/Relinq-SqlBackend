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
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallFirstTest
  {
    private CommandBuilder _commandBuilder;
    private StringBuilder _commandText;
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder();
      _commandText.Append ("xyz ");
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, new List<CommandParameter>(), _databaseInfo, new MethodCallSqlGeneratorRegistry());
    }

    [Test]
    public void First ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateStudentQueryable());
      var methodInfo = ReflectionUtility.GetMethod (() => query.First());
      IEvaluation evaluation = new Constant();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation>());
      MethodCallFirst methodCallFirst = new MethodCallFirst();
      methodCallFirst.GenerateSql (methodCall, _commandBuilder);

      Assert.AreEqual ("xyz TOP 1", _commandBuilder.GetCommandText());
    }
  }
}
