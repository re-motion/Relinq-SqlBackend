// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlTableAndJoinTextGeneratorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private Mock<ISqlGenerationStage> _stageMock;
    private TestableSqlTableAndJoinTextGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<ISqlGenerationStage> (MockBehavior.Strict);
      _commandBuilder = new SqlCommandBuilder();
      _generator = new TestableSqlTableAndJoinTextGenerator (_commandBuilder, _stageMock.Object);
    }

    [Test]
    public void GenerateSql_ForTable ()
    {
      var appendedTable = CreateResolvedAppendedTable("Table", "t", JoinSemantics.Inner);
      SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_ForSeveralTables ()
    {
      var sqlTable1 = CreateResolvedAppendedTable ("Table1", "t1", JoinSemantics.Inner);
      var sqlTable2 = CreateResolvedAppendedTable ("Table2", "t2", JoinSemantics.Inner);
      var sqlTable3 = CreateResolvedAppendedTable ("Table3", "t3", JoinSemantics.Inner);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable1, _commandBuilder, _stageMock.Object, true);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable2, _commandBuilder, _stageMock.Object, false);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable3, _commandBuilder, _stageMock.Object, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table1] AS [t1] CROSS JOIN [Table2] AS [t2] CROSS JOIN [Table3] AS [t3]"));
    }

    [Test]
    public void GenerateSql_ForLeftJoin ()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);

      var join = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Left, "ID", "CookTable", "t2", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t2].[FK])"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[FK])"));
    }

     [Test]
    public void GenerateSql_ForInnerJoin ()
    {
       var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
       var join = CreateResolvedJoin (typeof (Cook), "t1", JoinSemantics.Inner, "ID", "CookTable", "t2", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

       _stageMock
           .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition))
           .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t2].[FK])"))
           .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] INNER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[FK])"));
    }

    [Test]
    public void GenerateSql_ForJoin_Recursive ()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
      var join1 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Left, "ID", "CookTable", "t2", "FK");
      originalTable.SqlTable.AddJoinForExplicitQuerySource (join1);
      var join2 = CreateResolvedJoin(typeof (Cook), "t2", JoinSemantics.Left, "ID2", "CookTable2", "t3", "FK2");
      join1.JoinedTable.AddJoinForExplicitQuerySource (join2);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join1.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("X"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join2.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("Y"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] "
              + "LEFT OUTER JOIN [CookTable] AS [t2] "
              + "LEFT OUTER JOIN [CookTable2] AS [t3] "
              + "ON Y "
              + "ON X"));
    }

    [Test]
    public void GenerateSql_ForJoin_Multiple ()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
      var join1 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Left, "ID", "CookTable", "t2", "FK");
      originalTable.SqlTable.AddJoinForExplicitQuerySource (join1);
      var join2 = CreateResolvedJoin(typeof (Cook), "t2", JoinSemantics.Left, "ID2", "CookTable2", "t3", "FK2");
      originalTable.SqlTable.AddJoinForExplicitQuerySource (join2);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join1.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("X"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join2.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("Y"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, true);

      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] "
              + "LEFT OUTER JOIN [CookTable] AS [t2] ON X "
              + "LEFT OUTER JOIN [CookTable2] AS [t3] ON Y"));
    }

    [Test]
    public void GenerateSql_CrossJoinSemantics_FirstTable ()
    {
      var sqlTable = CreateResolvedAppendedTable ("Table", "t", JoinSemantics.Inner);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock.Object, isFirstTable: true);
      
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_CrossJoinSemantics_NonFirstTable_SimpleTableInfo ()
    {
      var sqlTable = CreateResolvedAppendedTable ("Table", "t", JoinSemantics.Inner);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock.Object, isFirstTable: false);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS JOIN [Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_CrossJoinSemantics_NonFirstTable_SubstatementTableInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var tableInfo = new ResolvedSubStatementTableInfo("q0", sqlStatement);
      var sqlTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (tableInfo, JoinSemantics.Inner);

      _stageMock
        .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
        .Callback((ISqlCommandBuilder commandBuilder, SqlStatement _) => commandBuilder.Append("[Table] AS [t]"))
        .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock.Object, isFirstTable: false);

      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS APPLY ([Table] AS [t]) AS [q0]"));
    }

    [Test]
    public void GenerateSql_OuterApplySemantics_FirstTable ()
    {
      var sqlTable = CreateResolvedAppendedTable ("Table", "t", JoinSemantics.Left);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock.Object, isFirstTable: true);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(SELECT NULL AS [Empty]) AS [Empty] OUTER APPLY [Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_OuterApplySemantics_NonFirstTable ()
    {
      var sqlTable = CreateResolvedAppendedTable ("Table", "t", JoinSemantics.Left);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock.Object, isFirstTable: false);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" OUTER APPLY [Table] AS [t]"));
    }

    [Test]
    public void VisitSimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      _generator.VisitSimpleTableInfo (simpleTableInfo);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c]"));
    }

    [Test]
    public void VisitSimpleTableInfo_FullQualifiedTableNameGetsSplit ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "TestDomain.dbo.CookTable", "c");

      _generator.VisitSimpleTableInfo (simpleTableInfo);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[TestDomain].[dbo].[CookTable] AS [c]"));
    }

    [Test]
    public void VisitSubStatementTableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var resolvedSubTableInfo = new ResolvedSubStatementTableInfo ("cook", sqlStatement);

      _stageMock
          .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .Callback ((ISqlCommandBuilder commandBuilder, SqlStatement _) => commandBuilder.Append ("XXX"))
          .Verifiable();

      _generator.VisitSubStatementTableInfo (resolvedSubTableInfo);

      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    public void VisitResolvedJoinedGroupingTableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var resolvedJoinedGroupingTableInfo = SqlStatementModelObjectMother.CreateResolvedJoinedGroupingTableInfo (sqlStatement);

      _stageMock
          .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .Callback ((ISqlCommandBuilder commandBuilder, SqlStatement _) => commandBuilder.Append ("XXX"))
          .Verifiable();

      _generator.VisitJoinedGroupingTableInfo (resolvedJoinedGroupingTableInfo);

      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    public void GenerateSql_WithUnresolvedTableInfo_RaisesException ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (
          SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo());
      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedTableInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedJoinTableInfo_RaisesException ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (
          SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo());
      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedJoinTableInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedCollectionJoinTableInfo_RaisesException ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (
          SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo());
      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedCollectionJoinTableInfo is not valid at this point."));
    }

    [Test]
    public void ApplyContext_VisitUnresolvedDummyRowTableInfo ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (
          SqlStatementModelObjectMother.CreateUnresolvedDummyRowTableInfo());

      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedDummyRowTableInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedGroupReferenceTableInfo ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable (
          SqlStatementModelObjectMother.CreateUnresolvedGroupReferenceTableInfo());
      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (appendedTable, _commandBuilder, _stageMock.Object, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedGroupReferenceTableInfo is not valid at this point."));
    }

    private SqlJoin CreateResolvedJoin (
        Type type,
        string originalTableAlias,
        JoinSemantics joinSemantics,
        string leftSideKeyName,
        string joinedTableName,
        string joinedTableAlias,
        string rightSideKeyName)
    {
      var joinedTableInfo = new ResolvedSimpleTableInfo (type, joinedTableName, joinedTableAlias);
      var joinedTable = new SqlTable (joinedTableInfo);

      var primaryColumn = new SqlColumnDefinitionExpression (typeof (int), originalTableAlias, leftSideKeyName, false);
      var foreignColumn = new SqlColumnDefinitionExpression (typeof (int), joinedTableAlias, rightSideKeyName, false);

      return new SqlJoin (joinedTable, joinSemantics, Expression.Equal (primaryColumn, foreignColumn));
    }

    private SqlAppendedTable CreateResolvedAppendedTable (string tableName, string tableAlias, JoinSemantics joinSemantics)
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), tableName, tableAlias));
      return SqlStatementModelObjectMother.CreateSqlAppendedTable (sqlTable, joinSemantics);
    }
  }
}