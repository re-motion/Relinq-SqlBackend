// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer.MethodCallGenerators
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
      _commandText = new StringBuilder ();
      _commandText.Append ("xyz ");
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, new List<CommandParameter> (), _databaseInfo, new MethodCallSqlGeneratorRegistry ());
    }

    [Test]
    public void First ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => query.First());
      IEvaluation evaluation = new Constant ();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation> ());
      MethodCallFirst methodCallFirst = new MethodCallFirst ();
      methodCallFirst.GenerateSql (methodCall, _commandBuilder);

      Assert.AreEqual ("xyz TOP 1", _commandBuilder.GetCommandText());
    }
  }
}