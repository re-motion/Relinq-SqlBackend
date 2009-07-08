// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallDistinctTest
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
    public void Distinct ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      var methodInfo = ParserUtility.GetMethod (() => Queryable.Distinct (query));
      IEvaluation evaluation = new NewObject (
          typeof (Student).GetConstructor (Type.EmptyTypes),
          new Column (new Table ("s1", "s1"), "c1"),
          new Column (new Table ("s2", "s2"), "c2")
          );
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation>());
      MethodCallDistinct distinct = new MethodCallDistinct();
      distinct.GenerateSql (methodCall, _commandBuilder);

      Assert.AreEqual ("xyz DISTINCT", _commandBuilder.GetCommandText());
    }
  }
}