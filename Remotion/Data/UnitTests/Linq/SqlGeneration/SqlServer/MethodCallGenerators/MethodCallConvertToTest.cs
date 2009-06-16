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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallConvertToTest
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
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, _databaseInfo, new MethodCallSqlGeneratorRegistry ());
    }

    [Test]
    public void ConvertIntToString ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetColumnSource (StubDatabaseInfo.Instance);

      MethodInfo methodInfo = typeof (Convert).GetMethod ("ToString",new Type[] {typeof(int) } );
      Column column = new Column (fromSource, "FirstColumn");
      //Constant item = new Constant (5);
      List<IEvaluation> arguments = new List<IEvaluation> { column };
      MethodCall methodCall = new MethodCall (methodInfo, null, arguments);

      MethodCallConvertTo methodCallConvertTo = new MethodCallConvertTo();
      methodCallConvertTo.GenerateSql (methodCall, _commandBuilder);
      Assert.AreEqual ("xyz CONVERT(nvarchar(max),[s].[FirstColumn]) ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void ConvertIntToBoolean ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetColumnSource (StubDatabaseInfo.Instance);

      MethodInfo methodInfo = typeof (Convert).GetMethod ("ToBoolean", new Type[] { typeof (int) });
      Column column = new Column (fromSource, "FirstColumn");
      //Constant item = new Constant (5);
      List<IEvaluation> arguments = new List<IEvaluation> { column };
      MethodCall methodCall = new MethodCall (methodInfo, null, arguments);

      MethodCallConvertTo methodCallConvertTo = new MethodCallConvertTo ();
      methodCallConvertTo.GenerateSql (methodCall, _commandBuilder);
      Assert.AreEqual ("xyz CONVERT(bit,[s].[FirstColumn]) ", _commandBuilder.GetCommandText ());
    }


  }
}