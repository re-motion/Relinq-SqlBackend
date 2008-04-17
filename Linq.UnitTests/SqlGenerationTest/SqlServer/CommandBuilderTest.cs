using System.Collections.Generic;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using System.Text;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;
using System;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
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
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters);
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
    [Ignore]
    public void AppendEvaluation_BinaryEvaluationAdd ()
    {
      _commandText = new StringBuilder ();
      _commandText.Append ("SELECT ");
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");

      BinaryEvaluation binaryEvaluation = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);

      _commandBuilder.AppendEvaluation (binaryEvaluation);

      Assert.AreEqual ("SELECT ([s1].[c1]+[s2].[c2])" ,_commandBuilder.GetCommandText ());

    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The Evaluation of type 'DummyEvaluation' is not supported.")]
    public void AppendEvaluation_Exception ()
    {
      IEvaluation evaluation = new DummyEvaluation ();
      _commandBuilder.AppendEvaluation (evaluation);
    }

    [Test]
    public void AppendConstant_Null()
    {
      _commandBuilder.AppendConstant(new Constant(null));
      Assert.AreEqual ("WHERE NULL", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_True ()
    {
      _commandBuilder.AppendConstant (new Constant (true));
      Assert.AreEqual ("WHERE (1=1)", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_False ()
    {
      _commandBuilder.AppendConstant (new Constant (false));
      Assert.AreEqual ("WHERE (1<>1)", _commandBuilder.GetCommandText ());
      CheckParametersUnchanged ();
    }

    [Test]
    public void AppendConstant_Parameter ()
    {
      _commandBuilder.AppendConstant (new Constant (5));
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