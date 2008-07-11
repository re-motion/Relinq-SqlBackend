/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Remotion.Collections;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.Linq.DataObjectModel;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class FromBuilderTest
  {
    [Test]
    public void CombineTables_SelectsJoinsPerTable()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<IColumnSource> tables = new List<IColumnSource> { table1 }; // this table does not have a join associated with it
      JoinCollection joins = new JoinCollection ();
      SingleJoin join = new SingleJoin (column2, column1);
      joins.AddPath (new FieldSourcePath(table2, new [] { join }));
      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s1] [s1_alias]", commandBuilder.GetCommandText());
    }

    [Test]
    public void CombineTables_WithJoin ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<IColumnSource> tables = new List<IColumnSource> { table2 };
      JoinCollection joins = new JoinCollection();
      SingleJoin join = new SingleJoin (column2, column1);
      joins.AddPath (new FieldSourcePath (table2, new[] { join }));
      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s2] [s2_alias] LEFT OUTER JOIN [s1] [s1_alias] ON [s2_alias].[c2] = [s1_alias].[c1]", commandBuilder.GetCommandText ());
    }

    [Test]
    public void CombineTables_WithNestedJoin ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      // table2.table1.table3

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Table table3 = new Table ("s3", "s3_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");
      Column column3 = new Column (table1, "c1'");
      Column column4 = new Column (table3, "c3");

      List<IColumnSource> tables = new List<IColumnSource> { table2 };

      JoinCollection joins = new JoinCollection();

      SingleJoin join1 = new SingleJoin (column2, column1);
      SingleJoin join2 = new SingleJoin (column3, column4);
      joins.AddPath (new FieldSourcePath (table2, new[] { join1, join2 }));

      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s2] [s2_alias] "
        + "LEFT OUTER JOIN [s1] [s1_alias] ON [s2_alias].[c2] = [s1_alias].[c1] "
        + "LEFT OUTER JOIN [s3] [s3_alias] ON [s1_alias].[c1'] = [s3_alias].[c3]", commandBuilder.GetCommandText());
    }

    [Test]
    public void CombineTables_WithSubqueries ()
    {
      MockRepository mockRepository = new MockRepository ();

      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), "sub_alias");
      Table table1 = new Table ("s1", "s1_alias");
      List<IColumnSource> tables = new List<IColumnSource> { table1, subQuery };

      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      commandBuilder.AddParameter (1);

      FromBuilder fromBuilderMock = mockRepository.CreateMock<FromBuilder> (commandBuilder, StubDatabaseInfo.Instance);
      ISqlGeneratorBase subQueryGeneratorMock = mockRepository.CreateMock<ISqlGeneratorBase> ();

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (fromBuilderMock, "CreateSqlGeneratorForSubQuery", subQuery, StubDatabaseInfo.Instance,
          commandBuilder)).Return (subQueryGeneratorMock);
      Expect.Call (subQueryGeneratorMock.BuildCommand (subQuery.QueryModel)).Do ((Func<QueryModel, CommandData>) delegate
      {
        commandBuilder.Append ("x");
        commandBuilder.AddParameter (0);
        return new CommandData();
      });

      mockRepository.ReplayAll ();
      fromBuilderMock.BuildFromPart (tables, new JoinCollection());
      mockRepository.VerifyAll ();

      Assert.AreEqual ("FROM [s1] [s1_alias] CROSS APPLY (x) [sub_alias]", commandBuilder.GetCommandText ());
      Assert.That (commandBuilder.GetCommandParameters(), Is.EqualTo (new[] {new CommandParameter ("@1", 1), new CommandParameter ("@2", 0)}));
    }

    [Test]
    public void CreateSqlGeneratorForSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), "sub_alias");
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);
      InlineSqlServerGenerator subQueryGenerator = (InlineSqlServerGenerator) PrivateInvoke.InvokeNonPublicMethod (fromBuilder, "CreateSqlGeneratorForSubQuery", 
          subQuery, StubDatabaseInfo.Instance, commandBuilder);
      //Assert.AreSame (subQuery.QueryModel, subQueryGenerator.QueryModel);
      Assert.AreSame (StubDatabaseInfo.Instance, subQueryGenerator.DatabaseInfo);
      Assert.AreEqual (ParseMode.SubQueryInFrom, subQueryGenerator.ParseMode);
    }

    [Test]
    public void BuildLetPart ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      //let with BinaryEvaluation
      Table table = new Table ("studentTable", "s");
      Column c1 = new Column(table,"FirstColumn");
      Column c2 = new Column(table,"LastColumn");

      BinaryEvaluation binaryEvaluation = new BinaryEvaluation(c1,c2,BinaryEvaluation.EvaluationKind.Add);
      ParameterExpression identifier = Expression.Parameter(typeof(string),"x");

      LetData letData = new LetData (binaryEvaluation, identifier.Name, new LetColumnSource("test",false));
      List<LetData> letDatas = new List<LetData> {letData};
      fromBuilder.BuildLetPart (letDatas);

      Assert.AreEqual (" CROSS APPLY (SELECT ([s].[FirstColumn] + [s].[LastColumn]) [x]) [x]", commandBuilder.GetCommandText ());
    }

    [Test]
    public void BuildLetPart_SeveralEvaluations ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      //let with BinaryEvaluation
      Table table = new Table ("studentTable", "s");
      Column c1 = new Column (table, "FirstColumn");
      Column c2 = new Column (table, "LastColumn");

      BinaryEvaluation binaryEvaluation1 = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);
      BinaryEvaluation binaryEvaluation2 = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);
      ParameterExpression identifier = Expression.Parameter (typeof (string), "x");

      NewObject newObject = new NewObject (typeof (object).GetConstructors()[0], binaryEvaluation1, binaryEvaluation2);
      LetData letData = new LetData (newObject, identifier.Name, 
        new LetColumnSource ("test", false));
      List<LetData> letDatas = new List<LetData> { letData };
      fromBuilder.BuildLetPart (letDatas);

      Assert.AreEqual (" CROSS APPLY (SELECT ([s].[FirstColumn] + [s].[LastColumn]), ([s].[FirstColumn] + [s].[LastColumn]) [x]) [x]", 
        commandBuilder.GetCommandText ());
    }

    [Test]
    public void BuildLetPart_ColumnSourceIsTabel()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      FromBuilder fromBuilder = new FromBuilder(commandBuilder,StubDatabaseInfo.Instance);

      Table table = new Table ("studentTable", "s");
      Column c1 = new Column (table, "FirstColumn");
      
      ParameterExpression identifier = Expression.Parameter (typeof (string), "x");
      
      LetData letData = new LetData (c1, identifier.Name, new LetColumnSource ("test", false));
      List<LetData> letDatas = new List<LetData> { letData };
      fromBuilder.BuildLetPart (letDatas);

      Assert.AreEqual (" CROSS APPLY (SELECT [s].[FirstColumn] [x]) [x]", commandBuilder.GetCommandText ());
    }
  }
}
