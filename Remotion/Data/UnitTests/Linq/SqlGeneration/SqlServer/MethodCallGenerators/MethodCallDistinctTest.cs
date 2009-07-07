// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using NUnit.Framework;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer.MethodCallGenerators
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
      _commandText = new StringBuilder ();
      _commandText.Append ("xyz ");
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, new List<CommandParameter>(), _databaseInfo, new MethodCallSqlGeneratorRegistry ());
    }

    [Test]
    public void Distinct ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Queryable.Distinct (query));
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor (Type.EmptyTypes),
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