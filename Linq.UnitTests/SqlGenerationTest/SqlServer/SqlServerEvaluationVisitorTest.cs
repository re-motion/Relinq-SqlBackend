using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SqlServerEvaluationVisitorTest
  {
    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandBuilder _commandBuilder;
    private CommandParameter _defaultParameter;
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder ();
      _commandText.Append ("xyz ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters);
      _databaseInfo = StubDatabaseInfo.Instance;
    }

    [Test]
    public void VisitColumn ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Column column = new Column (new Table ("table", "alias"), "name");

      visitor.VisitColumn (column);

      Assert.AreEqual ("xyz [alias].[name]", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Constant constant = new Constant(5);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz @2", _commandBuilder.GetCommandText ());
      Assert.AreEqual (5, _commandBuilder.GetCommandParameters ()[1].Value);

    }

    [Test]
    public void VisitConstant_Null ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Constant constant = new Constant (null);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz NULL", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant_True ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Constant constant = new Constant (true);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz (1=1)", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant_False ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Constant constant = new Constant (false);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz (1<>1)", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitBinaryCondition ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      BinaryCondition binaryCondition = new BinaryCondition (new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal);

      visitor.VisitBinaryCondition (binaryCondition);

      Assert.AreEqual("xyz ([s].[LastColumn] = @2)",_commandBuilder.GetCommandText());
      Assert.AreEqual ("Garcia", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitBinaryEvaluation_Add ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      Column column1 = new Column (new Table ("table1", "alias1"), "id1");
      Column column2 = new Column (new Table ("table2", "alias2"), "id2");

      BinaryEvaluation binaryEvaluation1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      BinaryEvaluation binaryEvaluation2 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Divide);
      BinaryEvaluation binaryEvaluation3 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Modulo);
      BinaryEvaluation binaryEvaluation4 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Multiply);
      BinaryEvaluation binaryEvaluation5 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Subtract);

      CheckBinaryEvaluation (binaryEvaluation1);
      CheckBinaryEvaluation (binaryEvaluation2);
      CheckBinaryEvaluation (binaryEvaluation3);
      CheckBinaryEvaluation (binaryEvaluation4);
      CheckBinaryEvaluation (binaryEvaluation5);
    }

    private void CheckBinaryEvaluation (BinaryEvaluation binaryEvaluation)
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      visitor.VisitBinaryEvaluation (binaryEvaluation);
      string aoperator = "";
      switch (binaryEvaluation.Kind)
      {
        case (BinaryEvaluation.EvaluationKind.Add):
          aoperator = " + ";
          break;
        case (BinaryEvaluation.EvaluationKind.Divide):
          aoperator = " / ";
          break;
        case (BinaryEvaluation.EvaluationKind.Modulo):
          aoperator = " % ";
          break;
        case (BinaryEvaluation.EvaluationKind.Multiply):
          aoperator = " * ";
          break;
        case (BinaryEvaluation.EvaluationKind.Subtract):
          aoperator = " - ";
          break;
      }
      Assert.AreEqual ("xyz ([alias1].[id1]" + aoperator + "[alias2].[id2])", _commandBuilder.GetCommandText ());
      _commandText = new StringBuilder ();
      _commandText.Append ("xyz ");
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters);
    }
  }
}