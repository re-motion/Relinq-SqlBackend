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
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestUtilities;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerEvaluationVisitorTest
  {
    private StringBuilder _commandText;
    private List<CommandParameter> _commandParameters;
    private CommandBuilder _commandBuilder;
    private CommandParameter _defaultParameter;
    private IDatabaseInfo _databaseInfo;
    private SqlServerGenerator _sqlGeneratorMock;
    private SqlServerEvaluationVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _commandText = new StringBuilder();
      _commandText.Append ("xyz ");
      _defaultParameter = new CommandParameter ("abc", 5);
      _commandParameters = new List<CommandParameter> { _defaultParameter };
      _databaseInfo = StubDatabaseInfo.Instance;
      _sqlGeneratorMock = MockRepository.GenerateMock<SqlServerGenerator> (StubDatabaseInfo.Instance);
      _commandBuilder = new CommandBuilder (_sqlGeneratorMock, _commandText, _commandParameters, _databaseInfo, new MethodCallSqlGeneratorRegistry());
      _visitor = new SqlServerEvaluationVisitor (_sqlGeneratorMock, _commandBuilder, _databaseInfo, new MethodCallSqlGeneratorRegistry ());
    }

    [Test]
    public void VisitBinaryCondition ()
    {
      var binaryCondition = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"),
          BinaryCondition.ConditionKind.Equal);

      _visitor.VisitBinaryCondition (binaryCondition);

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
      var column1 = new Column (new Table ("table1", "alias1"), "id1");
      var column2 = new Column (new Table ("table2", "alias2"), "id2");

      var binaryEvaluation1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      var binaryEvaluation2 = new BinaryEvaluation (binaryEvaluation1, column2, BinaryEvaluation.EvaluationKind.Divide);

      _visitor.VisitBinaryEvaluation (binaryEvaluation2);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (([alias1].[id1] + [alias2].[id2]) / [alias2].[id2])"));
    }

    [Test]
    public void VisitColumn ()
    {
      var column = new Column (new Table ("table", "alias"), "name");

      _visitor.VisitColumn (column);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz [alias].[name]"));
    }

    [Test]
    public void VisitColumn_ColumnSource_NoTable ()
    {
      var column = new Column (new SubQuery (ExpressionHelper.CreateQueryModel_Student(), ParseMode.SubQueryInFrom, "test"), "testC");

      _visitor.VisitColumn (column);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz [test].[testC]"));
    }

    [Test]
    public void VisitComplexCriterion_And ()
    {
      var binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      var binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      var complexCriterion = new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.And);

      _visitor.VisitComplexCriterion (complexCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ((@2 = @3) AND (@4 = @5))"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitComplexCriterion_Or ()
    {
      var binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      var binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      var complexCriterion = new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.Or);

      _visitor.VisitComplexCriterion (complexCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz ((@2 = @3) OR (@4 = @5))"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitComplexCriterion_AdjustsBooleanColumns ()
    {
      var column1 = new Column (new Table ("T1", "t1"), "c1");
      var column2 = new Column (new Table ("T2", "t2"), "c2");
      var complexCriterion = new ComplexCriterion (column1, column2, ComplexCriterion.JunctionKind.And);

      _visitor.VisitComplexCriterion (complexCriterion);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("xyz (([t1].[c1] = @2) AND ([t2].[c2] = @3))"));
      Assert.That (_commandBuilder.GetCommandParameters ()[1].Value, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[2].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstant ()
    {
      var constant = new Constant (5);

      _visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz @2"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo (5));
    }

    [Test]
    public void VisitConstant_False ()
    {
      var constant = new Constant (false);

      _visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (1<>1)"));
    }

    [Test]
    public void VisitConstant_Null ()
    {
      var constant = new Constant (null);

      _visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz NULL"));
    }

    [Test]
    public void VisitConstant_True ()
    {
      var constant = new Constant (true);

      _visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (1=1)"));
    }

    [Test]
    public void VisitConstant_WithCollection ()
    {
      var ls = new List<int> { 1, 2, 3, 4 };
      var constant = new Constant (ls);

      _visitor.VisitConstant (constant);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz (@2, @3, @4, @5)"));
    }

    [Test]
    public void VisitNewObjectEvaluation ()
    {
      var newObject = new NewObject (
          typeof (Tuple<int, string>).GetConstructors()[0],
          new IEvaluation[] { new Constant (1), new Constant ("2") });

      _visitor.VisitNewObjectEvaluation (newObject);

      Assert.That (_commandBuilder.CommandParameters.Count, Is.EqualTo (3));
      Assert.That (_commandBuilder.CommandParameters[1].Value, Is.EqualTo (1));
      Assert.That (_commandBuilder.CommandParameters[2].Value, Is.EqualTo ("2"));

      Assert.That (_commandBuilder.CommandText.ToString(), Is.EqualTo ("xyz @2, @3"));
    }

    [Test]
    public void VisitNotCriterion ()
    {
      var notCriterion = new NotCriterion (new Constant ("foo"));

      _visitor.VisitNotCriterion (notCriterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("xyz NOT @2"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("foo"));
    }

    [Test]
    public void VisitNotCriterion_AdjustsBooleanColumns ()
    {
      var column = new Column (new Table ("T1", "t1"), "c1");
      var notCriterion = new NotCriterion (column);

      _visitor.VisitNotCriterion (notCriterion);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("xyz NOT ([t1].[c1] = @2)"));
      Assert.That (_commandBuilder.GetCommandParameters ()[1].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitSubQuery ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();
      var subQuery = new SubQuery (queryModel, ParseMode.SubQueryInWhere, "sub_alias");

      var nestedGeneratorMock = MockRepository.GenerateMock<SqlServerGenerator> (StubDatabaseInfo.Instance);
      _sqlGeneratorMock
          .Expect (mock => mock.CreateNestedSqlGenerator (ParseMode.SubQueryInWhere))
          .Return (nestedGeneratorMock);
      nestedGeneratorMock
          .Expect (mock => mock.CreateDerivedContext (_commandBuilder))
          .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      nestedGeneratorMock
          .Expect (mock => mock.BuildCommand (
              Arg.Is (queryModel), 
              Arg<SqlServerGenerationContext>.Matches (ctx => 
                  ctx != null
                  && ctx.CommandBuilder.CommandText == _commandText 
                  && ctx.CommandBuilder.CommandParameters == _commandParameters 
                  && ctx.CommandBuilder.SqlGenerator == nestedGeneratorMock)))
          .Return (new CommandData ())
          .WhenCalled (mi =>
          {
            _commandText.Append ("NESTED");
            _commandParameters.Add (new CommandParameter ("@2", "nestedValue"));
          });

      _sqlGeneratorMock.Replay ();
      nestedGeneratorMock.Replay ();

      _commandBuilder.AppendEvaluation (subQuery);

      nestedGeneratorMock.VerifyAllExpectations ();

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("xyz (NESTED) [sub_alias]"));
      Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (new[] { _defaultParameter, new CommandParameter("@2", "nestedValue") }));
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException),
        ExpectedMessage = "The method System.DateTime.get_Now is not supported by this code generator, "
                          + "and no custom generator has been registered.")]
    public void VisitUnknownMethodCall ()
    {
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      var methodCall = new MethodCall (methodInfo, null, new List<IEvaluation>());

      _visitor.VisitMethodCall (methodCall);
    }

    private void CheckBinaryEvaluation (BinaryEvaluation binaryEvaluation)
    {
      _commandText.Length = 0;
      _commandParameters.Clear ();

      _visitor.VisitBinaryEvaluation (binaryEvaluation);
      string operatorSymbol = "";
      switch (binaryEvaluation.Kind)
      {
        case (BinaryEvaluation.EvaluationKind.Add):
          operatorSymbol = " + ";
          break;
        case (BinaryEvaluation.EvaluationKind.Divide):
          operatorSymbol = " / ";
          break;
        case (BinaryEvaluation.EvaluationKind.Modulo):
          operatorSymbol = " % ";
          break;
        case (BinaryEvaluation.EvaluationKind.Multiply):
          operatorSymbol = " * ";
          break;
        case (BinaryEvaluation.EvaluationKind.Subtract):
          operatorSymbol = " - ";
          break;
      }
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("([alias1].[id1]" + operatorSymbol + "[alias2].[id2])"));
    }
  }
}
