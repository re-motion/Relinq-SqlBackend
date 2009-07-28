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
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallRemoveTest
  {
    private CommandBuilder _commandBuilder;
    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandParameter _defaultParameter;
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder ();
      _commandText.Append ("xyz ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, _databaseInfo, new MethodCallSqlGeneratorRegistry());
    }

    [Test]
    public void Remove ()
    {
      var methodInfo = typeof (string).GetMethod ("Remove", new[] { typeof (int) });
      var column = new Column (new Table ("Student", "s"), "FirstColumn");
      var arguments = new List<IEvaluation> { new Constant (5) };
      var methodCall = new MethodCall (methodInfo, column, arguments);

      var methodCallRemove = new MethodCallRemove();
      methodCallRemove.GenerateSql (methodCall, _commandBuilder);

      Assert.AreEqual ("xyz STUFF([s].[FirstColumn],@2,LEN([s].[FirstColumn]), \"\")", _commandBuilder.GetCommandText ());
      Assert.AreEqual (5, _commandBuilder.GetCommandParameters ()[1].Value);
    }
  }
}
