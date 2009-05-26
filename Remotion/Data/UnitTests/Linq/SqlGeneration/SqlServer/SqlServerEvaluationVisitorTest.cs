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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer
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
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, _databaseInfo, new MethodCallSqlGeneratorRegistry());
    }

    [Test]
    public void VisitColumn ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Column column = new Column (new Table ("table", "alias"), "name");

      visitor.VisitColumn (column);

      Assert.AreEqual ("xyz [alias].[name]", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitColumn_ColumnSource_NoTable()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Column column = new Column (new LetColumnSource("test",false), null);
      
      visitor.VisitColumn (column);

      Assert.AreEqual("xyz [test].[test]",_commandBuilder.GetCommandText());
      
    }

    [Test]
    public void VisitConstant ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Constant constant = new Constant(5);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz @2", _commandBuilder.GetCommandText ());
      Assert.AreEqual (5, _commandBuilder.GetCommandParameters ()[1].Value);

    }

    [Test]
    public void VisitConstant_Null ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Constant constant = new Constant (null);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz NULL", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant_True ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Constant constant = new Constant (true);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz (1=1)", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant_False ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      Constant constant = new Constant (false);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz (1<>1)", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void VisitConstant_WithCollection ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry ());

      List<int> ls = new List<int> { 1, 2, 3, 4 };
      Constant constant = new Constant(ls);

      visitor.VisitConstant (constant);

      Assert.AreEqual ("xyz @2, @3, @4, @5", _commandBuilder.GetCommandText());
    }

    [Test]
    public void VisitBinaryCondition ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      BinaryCondition binaryCondition = new BinaryCondition (new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal);

      visitor.VisitBinaryCondition (binaryCondition);

      Assert.AreEqual("xyz ([s].[LastColumn] = @2)",_commandBuilder.GetCommandText());
      Assert.AreEqual ("Garcia", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitBinaryEvaluation ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      NotCriterion notCriterion = new NotCriterion (new Constant ("foo"));

      visitor.VisitNotCriterion (notCriterion);

      Assert.AreEqual ("xyz  NOT @2", _commandBuilder.GetCommandText ());
      Assert.AreEqual ("foo", _commandBuilder.GetCommandParameters ()[1].Value);
    }

    [Test]
    public void VisitSubQuery ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      PropertyInfo member = typeof (Student).GetProperty ("s");
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      IQueryable<string> query = SelectTestQueryGenerator.CreateSimpleQuery_WithProjection (source);
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, query);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();

      SubQuery subQuery = new SubQuery (model, ParseMode.SubQueryInSelect, "sub_alias");

      visitor.VisitSubQuery (subQuery);

      Assert.AreEqual ("xyz (SELECT [s].[FirstColumn] FROM [studentTable] [s]) [sub_alias]",_commandBuilder.GetCommandText());
    }
    
    [Test]
    [ExpectedException (typeof (SqlGenerationException),
        ExpectedMessage = "The method System.DateTime.get_Now is not supported by this code generator, " 
        + "and no custom generator has been registered.")]
    public void VisitUnknownMethodCall ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      MethodCall methodCall = new MethodCall (methodInfo, null, new List<IEvaluation>());

      visitor.VisitMethodCall (methodCall);
    }

    [Test]
    public void VisitNewObjectEvaluation ()
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
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
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
    }
  }
}
