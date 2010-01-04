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
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;
using Remotion.Data.Linq.UnitTests.Utilities;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerEvaluationVisitorTest
  {
    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder();
      _commandText.Append ("xyz ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _databaseInfo = StubDatabaseInfo.Instance;
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, _databaseInfo, new MethodCallSqlGeneratorRegistry());
    }

    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandBuilder _commandBuilder;
    private CommandParameter _defaultParameter;
    private IDatabaseInfo _databaseInfo;

    private void CheckBinaryEvaluation (BinaryEvaluation binaryEvaluation)
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
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
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ([alias1].[id1]" + aoperator + "[alias2].[id2])"));
      _commandText = new StringBuilder();
      _commandText.Append ("xyz ");
      _commandBuilder = new CommandBuilder (_commandText, _commandParameters, StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
    }

    [Test]
    public void VisitBinaryCondition ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var binaryCondition = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"),
          BinaryCondition.ConditionKind.Equal);

      visitor.VisitBinaryCondition (binaryCondition);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ([s].[LastColumn] = @2)"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("Garcia"));
    }

    [Test]
    public void VisitBinaryEvaluation ()
    {
      var column1 = new Column (new Table ("table1", "alias1"), "id1");
      var column2 = new Column (new Table ("table2", "alias2"), "id2");

      var binaryEvaluation1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      var binaryEvaluation2 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Divide);
      var binaryEvaluation3 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Modulo);
      var binaryEvaluation4 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Multiply);
      var binaryEvaluation5 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Subtract);

      CheckBinaryEvaluation (binaryEvaluation1);
      CheckBinaryEvaluation (binaryEvaluation2);
      CheckBinaryEvaluation (binaryEvaluation3);
      CheckBinaryEvaluation (binaryEvaluation4);
      CheckBinaryEvaluation (binaryEvaluation5);
    }

    [Test]
    public void VisitBinaryEvaluation_Encapsulated ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var column1 = new Column (new Table ("table1", "alias1"), "id1");
      var column2 = new Column (new Table ("table2", "alias2"), "id2");

      var binaryEvaluation1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      var binaryEvaluation2 = new BinaryEvaluation (binaryEvaluation1, column2, BinaryEvaluation.EvaluationKind.Divide);

      visitor.VisitBinaryEvaluation (binaryEvaluation2);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (([alias1].[id1] + [alias2].[id2]) / [alias2].[id2])"));
    }

    [Test]
    public void VisitColumn ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var column = new Column (new Table ("table", "alias"), "name");

      visitor.VisitColumn (column);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz [alias].[name]"));
    }

    [Test]
    public void VisitColumn_ColumnSource_NoTable ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var column = new Column (new LetColumnSource ("test", false), null);

      visitor.VisitColumn (column);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz [test].[test]"));
    }

    [Test]
    public void VisitComplexCriterion_And ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      var binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      var complexCriterion = new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.And);

      visitor.VisitComplexCriterion (complexCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ((@2 = @3) AND (@4 = @5))"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitComplexCriterion_Or ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      var binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      var complexCriterion = new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.Or);

      visitor.VisitComplexCriterion (complexCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ((@2 = @3) OR (@4 = @5))"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitConstant ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var constant = new Constant (5);

      visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz @2"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo (5));
    }

    [Test]
    public void VisitConstant_False ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var constant = new Constant (false);

      visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (1<>1)"));
    }

    [Test]
    public void VisitConstant_Null ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var constant = new Constant (null);

      visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz NULL"));
    }

    [Test]
    public void VisitConstant_True ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var constant = new Constant (true);

      visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (1=1)"));
    }

    [Test]
    public void VisitConstant_WithCollection ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var ls = new List<int> { 1, 2, 3, 4 };
      var constant = new Constant (ls);

      visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (@2, @3, @4, @5)"));
    }

    [Test]
    public void VisitNewObjectEvaluation ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      var newObject = new NewObject (
          typeof (Tuple<int, string>).GetConstructors()[0],
          new IEvaluation[] { new Constant (1), new Constant ("2") });

      visitor.VisitNewObjectEvaluation (newObject);

      Assert.That (_commandBuilder.CommandParameters.Count, Is.EqualTo (3));
      Assert.That (_commandBuilder.CommandParameters[1].Value, Is.EqualTo (1));
      Assert.That (_commandBuilder.CommandParameters[2].Value, Is.EqualTo ("2"));

      Assert.That (_commandBuilder.CommandText.ToString(), Is.EqualTo ("xyz @2, @3"));
    }

    [Test]
    public void VisitNotCriterion ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      var notCriterion = new NotCriterion (new Constant ("foo"));

      visitor.VisitNotCriterion (notCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz NOT @2"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitSubQuery ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());

      IQueryable<Student> source = ExpressionHelper.CreateStudentQueryable();
      IQueryable<string> query = SelectTestQueryGenerator.CreateSimpleQuery_WithProjection (source);
      QueryModel model = ExpressionHelper.ParseQuery (query.Expression);

      var subQuery = new SubQuery (model, ParseMode.SubQueryInSelect, "sub_alias");

      visitor.VisitSubQuery (subQuery);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (SELECT [s].[FirstColumn] FROM [studentTable] [s]) [sub_alias]"));
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException),
        ExpectedMessage = "The method System.DateTime.get_Now is not supported by this code generator, "
                          + "and no custom generator has been registered.")]
    public void VisitUnknownMethodCall ()
    {
      var visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      var methodCall = new MethodCall (methodInfo, null, new List<IEvaluation>());

      visitor.VisitMethodCall (methodCall);
    }
  }
}
