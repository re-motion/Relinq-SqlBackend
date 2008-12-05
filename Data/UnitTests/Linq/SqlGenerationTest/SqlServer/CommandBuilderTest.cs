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
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using System.Text;
using Remotion.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;
using System;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class CommandBuilderTest
  {
    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandBuilder _commandBuilder;
    private CommandParameter _defaultParameter;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder ();
      _commandText.Append ("WHERE ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
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
      Assert.AreEqual ("WHERE abc", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged();
    }

    
    [Test]
    public void AppendEvaluation ()
    {
      IEvaluation evaluation = new Column (new Table ("table", "alias"), "name");
      _commandBuilder.AppendEvaluation (evaluation);
      Assert.AreEqual ("WHERE [alias].[name]", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendEvaluation_BinaryEvaluationAdd ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");

      BinaryEvaluation binaryEvaluation = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);

      _commandBuilder.AppendEvaluation (binaryEvaluation);

      Assert.AreEqual ("WHERE ([s1].[c1] + [s2].[c2])" ,_commandBuilder.GetCommandText ());

    }

    [Test]
    public void AppendConstant_Null()
    {
      _commandBuilder.AppendEvaluation(new Constant(null));
      Assert.AreEqual ("WHERE NULL", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_True ()
    {
      _commandBuilder.AppendEvaluation (new Constant (true));
      Assert.AreEqual ("WHERE (1=1)", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_False ()
    {
      _commandBuilder.AppendEvaluation (new Constant (false));
      Assert.AreEqual ("WHERE (1<>1)", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_Parameter ()
    {
      _commandBuilder.AppendEvaluation (new Constant (5));
      Assert.AreEqual ("WHERE @2", _commandBuilder.GetCommandText ());
      Assert.That (_commandBuilder.GetCommandParameters (), Is.EqualTo (new[] { _defaultParameter, new CommandParameter("@2", 5)}));
    }

    [Test]
    public void AddParameter()
    {
      CommandParameter parameter1 = _commandBuilder.AddParameter (10);
      CommandParameter parameter2 = _commandBuilder.AddParameter (12);

      Assert.That (_commandBuilder.GetCommandParameters (), Is.EqualTo (new[] { _defaultParameter, parameter1, parameter2 }));
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

      List<IEvaluation> evaluations = new List<IEvaluation> {evaluation1, evaluation2, evaluation3};
      _commandBuilder.AppendEvaluations (evaluations);

      Assert.AreEqual (_commandBuilder.GetCommandText (), _commandText.ToString ());
      Assert.AreEqual ("WHERE [alias1].[name1], [alias2].[name2], [alias3].[name3]", _commandText.ToString ());
    }

    [Test]
    public void AppendBinaryEvaluation ()
    {
      IEvaluation evaluation1 = new Column (new Table ("table1", "alias1"), "name1");
      IEvaluation evaluation2 = new Column (new Table ("table2", "alias2"), "name2");
      IEvaluation evaluation = new BinaryEvaluation (evaluation1, evaluation2, BinaryEvaluation.EvaluationKind.Add);

      _commandBuilder.AppendEvaluation (evaluation);

      Assert.AreEqual (_commandBuilder.GetCommandText (), _commandText.ToString ());
      Assert.AreEqual ("WHERE ([alias1].[name1] + [alias2].[name2])", _commandText.ToString ());
    }

    [Test]
    public void AppendSeparatedItems_WithAppendColumn ()
    {
      var items = new List<string> { "a", "b", "c" };
      _commandBuilder.AppendSeparatedItems (items, _commandBuilder.Append);
      Assert.AreEqual ("WHERE a, b, c", _commandText.ToString ());
    }
    
    private void CheckTextUnchanged ()
    {
      Assert.AreEqual ("WHERE ", _commandBuilder.GetCommandText ());
    }

    private void CheckParametersUnchanged ()
    {
      Assert.That (_commandBuilder.GetCommandParameters (), Is.EqualTo (new[] { _defaultParameter }));
    }
   
  }
}
