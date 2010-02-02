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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
using Remotion.Data.Linq.Backend;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class CommandBuilderTest
  {
    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandParameter _defaultParameter;
    private SqlServerGenerator _sqlGeneratorMock;
    private CommandBuilder _commandBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder();
      _commandText.Append ("WHERE ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _sqlGeneratorMock = MockRepository.GenerateMock<SqlServerGenerator> (StubDatabaseInfo.Instance);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _commandBuilder = new CommandBuilder (
          _sqlGeneratorMock,
          _commandText,
          _commandParameters,
          StubDatabaseInfo.Instance,
          new MethodCallSqlGeneratorRegistry());
    }

    [Test]
    public void Initialize ()
    {
      CheckTextUnchanged();
      CheckParametersUnchanged();
    }

    [Test]
    public void Append ()
    {
      _commandBuilder.Append ("abc");
      Assert.AreEqual ("WHERE abc", _commandBuilder.GetCommandText());
      CheckParametersUnchanged();
    }

    [Test]
    public void AppendEvaluation ()
    {
      IEvaluation evaluation = new Column (new Table ("table", "alias"), "name");
      _commandBuilder.AppendEvaluation (evaluation);
      Assert.AreEqual ("WHERE [alias].[name]", _commandBuilder.GetCommandText());
    }

    [Test]
    public void AppendEvaluation_BinaryEvaluationAdd ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");
      var c2 = new Column (new Table ("s2", "s2"), "c2");

      var binaryEvaluation = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);

      _commandBuilder.AppendEvaluation (binaryEvaluation);

      Assert.AreEqual ("WHERE ([s1].[c1] + [s2].[c2])", _commandBuilder.GetCommandText());
    }

    [Test]
    public void AppendEvaluation_SubQuery ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();
      var subQuery = new SubQuery (queryModel, ParseMode.SubQueryInWhere, "test");

      var nestedGeneratorMock = MockRepository.GenerateMock<SqlServerGenerator> (StubDatabaseInfo.Instance);
      _sqlGeneratorMock
          .Expect (mock => mock.CreateNestedSqlGenerator (ParseMode.SubQueryInWhere))
          .Return (nestedGeneratorMock);
      nestedGeneratorMock
          .Expect (mock => mock.BuildCommand (Arg.Is (queryModel), Arg<SqlServerGenerationContext>.Is.Anything))
          .Return (new CommandData());
      _sqlGeneratorMock.Replay ();
      nestedGeneratorMock.Replay ();
      
      _commandBuilder.AppendEvaluation (subQuery);

      nestedGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void AppendConstant_Null ()
    {
      _commandBuilder.AppendEvaluation (new Constant (null));
      Assert.AreEqual ("WHERE NULL", _commandBuilder.GetCommandText());
      CheckParametersUnchanged();
    }

    [Test]
    public void AppendConstant_True ()
    {
      _commandBuilder.AppendEvaluation (new Constant (true));
      Assert.AreEqual ("WHERE (1=1)", _commandBuilder.GetCommandText());
      CheckParametersUnchanged();
    }

    [Test]
    public void AppendConstant_False ()
    {
      _commandBuilder.AppendEvaluation (new Constant (false));
      Assert.AreEqual ("WHERE (1<>1)", _commandBuilder.GetCommandText());
      CheckParametersUnchanged();
    }

    [Test]
    public void AppendConstant_Parameter ()
    {
      _commandBuilder.AppendEvaluation (new Constant (5));
      Assert.AreEqual ("WHERE @2", _commandBuilder.GetCommandText());
      Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (new[] { _defaultParameter, new CommandParameter ("@2", 5) }));
    }

    [Test]
    public void AddParameter ()
    {
      CommandParameter parameter1 = _commandBuilder.AddParameter (10);
      CommandParameter parameter2 = _commandBuilder.AddParameter (12);

      Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (new[] { _defaultParameter, parameter1, parameter2 }));
      Assert.AreEqual ("@2", parameter1.Name);
      Assert.AreEqual (10, parameter1.Value);
      Assert.AreEqual ("@3", parameter2.Name);
      Assert.AreEqual (12, parameter2.Value);
    }

    [Test]
    public void CommandBuilderChangesOrginalObjects ()
    {
      _commandBuilder.AddParameter (10);
      _commandBuilder.Append ("abc");
      Assert.AreEqual (_commandBuilder.GetCommandText(), _commandText.ToString());
      Assert.That (_commandParameters, Is.EqualTo (_commandBuilder.GetCommandParameters()));
    }

    [Test]
    public void AppendEvaluations ()
    {
      IEvaluation evaluation1 = new Column (new Table ("table1", "alias1"), "name1");
      IEvaluation evaluation2 = new Column (new Table ("table2", "alias2"), "name2");
      IEvaluation evaluation3 = new Column (new Table ("table3", "alias3"), "name3");

      var evaluations = new List<IEvaluation> { evaluation1, evaluation2, evaluation3 };
      _commandBuilder.AppendEvaluations (evaluations);

      Assert.AreEqual (_commandBuilder.GetCommandText(), _commandText.ToString());
      Assert.AreEqual ("WHERE [alias1].[name1], [alias2].[name2], [alias3].[name3]", _commandText.ToString());
    }

    [Test]
    public void AppendBinaryEvaluation ()
    {
      IEvaluation evaluation1 = new Column (new Table ("table1", "alias1"), "name1");
      IEvaluation evaluation2 = new Column (new Table ("table2", "alias2"), "name2");
      IEvaluation evaluation = new BinaryEvaluation (evaluation1, evaluation2, BinaryEvaluation.EvaluationKind.Add);

      _commandBuilder.AppendEvaluation (evaluation);

      Assert.AreEqual (_commandBuilder.GetCommandText(), _commandText.ToString());
      Assert.AreEqual ("WHERE ([alias1].[name1] + [alias2].[name2])", _commandText.ToString());
    }

    [Test]
    public void AppendSeparatedItems_WithAppendColumn ()
    {
      var items = new List<string> { "a", "b", "c" };
      _commandBuilder.AppendSeparatedItems (items, _commandBuilder.Append);
      Assert.AreEqual ("WHERE a, b, c", _commandText.ToString());
    }

    private void CheckTextUnchanged ()
    {
      Assert.AreEqual ("WHERE ", _commandBuilder.GetCommandText());
    }

    private void CheckParametersUnchanged ()
    {
      Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (new[] { _defaultParameter }));
    }
  }
}