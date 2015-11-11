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
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlTableAndJoinTextGeneratorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private Mock<ISqlGenerationStage> _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<ISqlGenerationStage> (MockBehavior.Strict);
      _commandBuilder = new SqlCommandBuilder();
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
    public void GenerateSql_ForLeftJoinWithoutJoinCondition_OptimizedToOuterApply ()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);

      var join = CreateResolvedJoinWithoutJoinCondition (typeof (Cook), JoinSemantics.Left, "CookTable", "t2");
      originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] OUTER APPLY [CookTable] AS [t2]"));
    }

    [Test]
    public void GenerateSql_ForLeftJoin_Multiple_WithJoinConditionAndWithoutJoinCondition()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);

      var join1 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Left, "ID", "Table2", "t2", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join1);

      var join2 = CreateResolvedJoinWithoutJoinCondition (typeof (Cook), JoinSemantics.Left, "Table3", "t3");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join2);

      var join3 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Left, "ID", "Table4", "t4", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join3);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join1.JoinCondition))
          .Callback((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t2].[FK])"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join3.JoinCondition))
          .Callback((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t4].[FK])"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join2.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] "
              + "LEFT OUTER JOIN [Table2] AS [t2] ON ([t1].[ID] = [t2].[FK]) "
              + "OUTER APPLY [Table3] AS [t3] "
              + "LEFT OUTER JOIN [Table4] AS [t4] ON ([t1].[ID] = [t4].[FK])"));
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
    public void GenerateSql_ForInnerJoinWithoutJoinCondition_WithResolvedSimpleTable_OptimizedToCrossJoin ()
    {
       var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
       var join = CreateResolvedJoinWithoutJoinCondition (typeof (Cook), JoinSemantics.Inner, "CookTable", "t2");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] CROSS JOIN [CookTable] AS [t2]"));
    }

    [Test]
    public void GenerateSql_ForInnerJoinWithoutJoinCondition_WithSubStatementOptimizedToCrossApply ()
    {
       var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
       var join = CreateResolvedJoinForSubStatementTableInfoWithoutJoinCondition (typeof (Cook), JoinSemantics.Inner, "t2");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

      _stageMock
          .Setup (_ => _.GenerateTextForSqlStatement (_commandBuilder, ((ResolvedSubStatementTableInfo) join.JoinedTable.TableInfo).SqlStatement))
          .Callback ((ISqlCommandBuilder commandBuilder, SqlStatement sqlStatement) => commandBuilder.Append ("SubStatement"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] CROSS APPLY (SubStatement) AS [t2]"));
    }

    [Test]
    public void GenerateSql_ForInnerJoinWithoutJoinCondition_WithGroupingOptimizedToCrossApply ()
    {
       var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
       var join = CreateResolvedJoinForJoinedGroupingTableInfoWithoutJoinCondition (typeof (Cook), JoinSemantics.Inner, "t2");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join);

      _stageMock
          .Setup (_ => _.GenerateTextForSqlStatement (_commandBuilder, ((ResolvedJoinedGroupingTableInfo) join.JoinedTable.TableInfo).SqlStatement))
          .Callback ((ISqlCommandBuilder commandBuilder, SqlStatement sqlStatement) => commandBuilder.Append ("SubStatement"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] CROSS APPLY (SubStatement) AS [t2]"));
    }

    [Test]
    public void GenerateSql_ForInnerJoin_Multiple_WithJoinConditionAndWithoutJoinCondition()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);

      var join1 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Inner, "ID", "Table2", "t2", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join1);

      var join2 = CreateResolvedJoinWithoutJoinCondition (typeof (Cook), JoinSemantics.Inner, "Table3", "t3");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join2);

      var join3 = CreateResolvedJoin(typeof (Cook), "t1", JoinSemantics.Inner, "ID", "Table4", "t4", "FK");
       originalTable.SqlTable.AddJoinForExplicitQuerySource (join3);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join1.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t2].[FK])"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join3.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("([t1].[ID] = [t4].[FK])"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join2.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] "
              + "INNER JOIN [Table2] AS [t2] ON ([t1].[ID] = [t2].[FK]) "
              + "CROSS JOIN [Table3] AS [t3] "
              + "INNER JOIN [Table4] AS [t4] ON ([t1].[ID] = [t4].[FK])"));
    }

    [Test]
    public void GenerateSql_ForLeftJoin_WithJoinCondition_Recursive ()
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
    public void GenerateSql_ForLeftJoin_WithoutJoinCondition_Recursive ()
    {
      var originalTable = CreateResolvedAppendedTable ("KitchenTable", "t1", JoinSemantics.Inner);
      var join1 = CreateResolvedJoinWithoutJoinCondition (typeof (Cook), JoinSemantics.Left, "CookTable", "t2");
      originalTable.SqlTable.AddJoinForExplicitQuerySource (join1);
      var join2 = CreateResolvedJoin (typeof (Cook), "t2", JoinSemantics.Left, "ID2", "CookTable2", "t3", "FK2");
      join1.JoinedTable.AddJoinForExplicitQuerySource (join2);

      _stageMock
          .Setup (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join2.JoinCondition))
          .Callback ((ISqlCommandBuilder commandBuilder, Expression expression) => commandBuilder.Append ("Y"))
          .Verifiable();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock.Object, true);

      _stageMock.Verify (mock => mock.GenerateTextForJoinCondition (_commandBuilder, join1.JoinCondition), Times.Never());
      _stageMock.Verify();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] "
              + "OUTER APPLY [CookTable] AS [t2] "
              + "LEFT OUTER JOIN [CookTable2] AS [t3] "
              + "ON Y"));
    }

    [Test]
    public void GenerateSql_ForLeftJoin_Multiple ()
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
    public void GenerateSql_WithResolvedSimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      SqlTableAndJoinTextGenerator.GenerateSql (
          new SqlAppendedTable (new SqlTable (simpleTableInfo), JoinSemantics.Inner),
          _commandBuilder,
          _stageMock.Object,
          isFirstTable: true);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c]"));
    }

    [Test]
    public void GenerateSql_WithResolvedSimpleTableInfo_FullQualifiedTableNameGetsSplit ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "TestDomain.dbo.CookTable", "c");

      SqlTableAndJoinTextGenerator.GenerateSql (
          new SqlAppendedTable (new SqlTable (simpleTableInfo), JoinSemantics.Inner),
          _commandBuilder,
          _stageMock.Object,
          isFirstTable: true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[TestDomain].[dbo].[CookTable] AS [c]"));
    }

    [Test]
    public void GenerateSql_WithResolvedSubStatementTableInfo ()
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

      SqlTableAndJoinTextGenerator.GenerateSql (
          new SqlAppendedTable (new SqlTable (resolvedSubTableInfo), JoinSemantics.Inner),
          _commandBuilder,
          _stageMock.Object,
          isFirstTable: true);

      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    public void GenerateSql_WithResolvedJoinedGroupingTableInfo ()
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

      SqlTableAndJoinTextGenerator.GenerateSql (
          new SqlAppendedTable (new SqlTable (resolvedJoinedGroupingTableInfo), JoinSemantics.Inner),
          _commandBuilder,
          _stageMock.Object,
          isFirstTable: true);

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

    private SqlJoin CreateResolvedJoinWithoutJoinCondition (Type type, JoinSemantics joinSemantics, string joinedTableName, string joinedTableAlias)
    {
      var joinedTableInfo = new ResolvedSimpleTableInfo (type, joinedTableName, joinedTableAlias);
      var joinedTable = new SqlTable (joinedTableInfo);

      return new SqlJoin (joinedTable, joinSemantics, new NullJoinConditionExpression());
    }

    private SqlJoin CreateResolvedJoinForSubStatementTableInfoWithoutJoinCondition (Type type, JoinSemantics joinSemantics, string joinedTableAlias)
    {
      var joinedTableInfo = new ResolvedSubStatementTableInfo (
          joinedTableAlias,
          SqlStatementModelObjectMother.CreateSqlStatement_Resolved (type));
      var joinedTable = new SqlTable (joinedTableInfo);

      return new SqlJoin (joinedTable, joinSemantics, new NullJoinConditionExpression());
    }

    private SqlJoin CreateResolvedJoinForJoinedGroupingTableInfoWithoutJoinCondition (Type type, JoinSemantics joinSemantics, string joinedTableAlias)
    {
      var joinedTableInfo = new ResolvedJoinedGroupingTableInfo (
          joinedTableAlias,
          SqlStatementModelObjectMother.CreateSqlStatement_Resolved (type),
          SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression(),
          "gs");
      var joinedTable = new SqlTable (joinedTableInfo);

      return new SqlJoin (joinedTable, joinSemantics, new NullJoinConditionExpression());
    }

    private SqlAppendedTable CreateResolvedAppendedTable (string tableName, string tableAlias, JoinSemantics joinSemantics)
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), tableName, tableAlias));
      return SqlStatementModelObjectMother.CreateSqlAppendedTable (sqlTable, joinSemantics);
    }
  }
}