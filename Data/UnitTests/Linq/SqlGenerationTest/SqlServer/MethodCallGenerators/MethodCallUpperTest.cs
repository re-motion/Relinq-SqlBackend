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
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallUpperTest
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
    public void ToUpper ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] { });
      Column column = new Column (fromSource, "FirstColumn");
      List<IEvaluation> arguments = new List<IEvaluation> ();
      MethodCall methodCall = new MethodCall (methodInfo, column, arguments);
      
      MethodCallUpper methodCallUpper = new MethodCallUpper ();
      methodCallUpper.GenerateSql (methodCall, _commandBuilder);
      
      Assert.AreEqual ("xyz UPPER([s].[FirstColumn])", _commandBuilder.GetCommandText ());
    }
  }
}