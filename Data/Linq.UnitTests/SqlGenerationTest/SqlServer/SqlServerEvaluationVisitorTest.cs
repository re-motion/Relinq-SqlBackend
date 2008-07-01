using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
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
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, _databaseInfo);
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
    public void VisitColumn_ColumnSource_NoTable()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      Column column = new Column (new LetColumnSource("test",false), null);
      
      visitor.VisitColumn (column);

      Assert.AreEqual("xyz [test].[test]",_commandBuilder.GetCommandText());
      
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
    public void VisitBinaryEvaluation ()
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

    [Test]
    public void VisitBinaryEvaluation_Encapsulated ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      Column column1 = new Column (new Table ("table1", "alias1"), "id1");
      Column column2 = new Column (new Table ("table2", "alias2"), "id2");

      BinaryEvaluation binaryEvaluation1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      BinaryEvaluation binaryEvaluation2 = new BinaryEvaluation (binaryEvaluation1, column2, BinaryEvaluation.EvaluationKind.Divide);

      visitor.VisitBinaryEvaluation (binaryEvaluation2);

      Assert.AreEqual ("xyz (([alias1].[id1] + [alias2].[id2]) / [alias2].[id2])", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitComplexCriterion_And ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      BinaryCondition binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      BinaryCondition binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      ComplexCriterion complexCriterion = new ComplexCriterion(binaryCondition1,binaryCondition2,ComplexCriterion.JunctionKind.And);

      visitor.VisitComplexCriterion (complexCriterion);

      Assert.AreEqual ("xyz ((@2 = @3) AND (@4 = @5))",_commandBuilder.GetCommandText());
      Assert.AreEqual ("foo", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitComplexCriterion_Or ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      BinaryCondition binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      BinaryCondition binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      ComplexCriterion complexCriterion = new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.Or);

      visitor.VisitComplexCriterion (complexCriterion);

      Assert.AreEqual ("xyz ((@2 = @3) OR (@4 = @5))", _commandBuilder.GetCommandText ());
      Assert.AreEqual ("foo", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitNotCriterion ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      NotCriterion notCriterion = new NotCriterion (new Constant ("foo"));

      visitor.VisitNotCriterion (notCriterion);

      Assert.AreEqual ("xyz  NOT @2", _commandBuilder.GetCommandText ());
      Assert.AreEqual ("foo", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitSubQuery ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);

      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      PropertyInfo member = typeof (Student).GetProperty ("s");
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      IQueryable<string> query = SelectTestQueryGenerator.CreateSimpleQuery_WithProjection (source);
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, query);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();

      SubQuery subQuery = new SubQuery (model, "sub_alias");

      visitor.VisitSubQuery (subQuery);

      Assert.AreEqual ("xyz ((SELECT [s].[FirstColumn] FROM [studentTable] [s]) sub_alias)",_commandBuilder.GetCommandText());
    }

    [Test]
    public void VisitMethodCall_ToUpper ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] {  });
      Column column = new Column (fromSource, "FirstColumn");
      List<IEvaluation> arguments = new List<IEvaluation>();
      MethodCall methodCall = new MethodCall (methodInfo, column, arguments);

      visitor.VisitMethodCallEvaluation (methodCall);

      Assert.AreEqual ("xyz UPPER([s].[FirstColumn])", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitMethodCall_WithArguments ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new Type[] { typeof (int) });
      Column column = new Column (fromSource, "FirstColumn");
      Constant item = new Constant (5);
      List<IEvaluation> arguments = new List<IEvaluation> { item };
      MethodCall methodCall = new MethodCall (methodInfo, column, arguments);

      visitor.VisitMethodCallEvaluation (methodCall);

      Assert.AreEqual ("xyz STUFF([s].[FirstColumn],@2,CONVERT(Int,DATALENGTH([s].[FirstColumn]) / 2), \")", _commandBuilder.GetCommandText ());
      Assert.AreEqual(5,_commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException), 
        ExpectedMessage = "The method System.DateTime.get_Now is not supported by the SQL Server code generator.")]
    public void VisitUnknownMethodCall ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      MethodCall methodCall = new MethodCall (methodInfo, null, new List<IEvaluation>());

      visitor.VisitMethodCallEvaluation (methodCall);
    }

    [Test]
    public void VisitNewObjectEvaluation ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      NewObject newObject = new NewObject (typeof (Tuple<int, string>).GetConstructors()[0],
          new IEvaluation[] {new Constant(1), new Constant("2")});

      visitor.VisitNewObjectEvaluation (newObject);

      Assert.That (_commandBuilder.CommandParameters.Count, Is.EqualTo (3));
      Assert.That (_commandBuilder.CommandParameters[1].Value, Is.EqualTo (1));
      Assert.That (_commandBuilder.CommandParameters[2].Value, Is.EqualTo ("2"));

      Assert.That (_commandBuilder.CommandText.ToString(), Is.EqualTo("xyz @2, @3"));

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
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, StubDatabaseInfo.Instance);
    }
  }
}