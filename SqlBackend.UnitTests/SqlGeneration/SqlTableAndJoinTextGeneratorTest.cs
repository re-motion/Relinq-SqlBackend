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
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlTableAndJoinTextGeneratorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;
    private TestableSqlTableAndJoinTextGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage>();
      _commandBuilder = new SqlCommandBuilder();
      _generator = new TestableSqlTableAndJoinTextGenerator (_commandBuilder, _stageMock);
    }

    [Test]
    public void GenerateSql_ForTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      sqlTable.TableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_ForSeveralTables ()
    {
      var sqlTable1 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo ("Table1", "t1");
      var sqlTable2 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo ("Table2", "t2");
      var sqlTable3 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo ("Table3", "t3");
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable1, _commandBuilder, _stageMock, true);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable2, _commandBuilder, _stageMock, false);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable3, _commandBuilder, _stageMock, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table1] AS [t1] CROSS JOIN [Table2] AS [t2] CROSS JOIN [Table3] AS [t3]"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"), JoinSemantics.Inner);
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var joinedTable = originalTable.GetOrAddLeftJoin (CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK"), kitchenCookMember);

      joinedTable.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK");

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinCondition (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).JoinCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("([t1].[ID] = [t2].[FK])"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (
          _commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t1].[ID] = [t2].[FK])"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable_Recursive ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"), JoinSemantics.Inner);
      var memberInfo1 = typeof (Kitchen).GetProperty ("Cook");
      var memberInfo2 = typeof (Cook).GetProperty ("Substitution");
      var joinedTable1 = originalTable.GetOrAddLeftJoin (CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK"), memberInfo1);
      var joinedTable2 = joinedTable1.GetOrAddLeftJoin (CreateResolvedJoinInfo (typeof (Cook), "t2", "ID2", "CookTable2", "t3", "FK2"), memberInfo2);

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinCondition (_commandBuilder, ((ResolvedJoinInfo) joinedTable1.JoinInfo).JoinCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("X"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinCondition (_commandBuilder, ((ResolvedJoinInfo) joinedTable2.JoinInfo).JoinCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("Y"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] LEFT OUTER JOIN "
              + "[CookTable] AS [t2] ON X LEFT OUTER JOIN "
              + "[CookTable2] AS [t3] ON Y"));
    }

    [Test]
    public void GenerateSql_InnerJoinSemantics_FirstTable ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: true);
      
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_InnerJoinSemantics_NonFirstTable_SimpleTableInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: false);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS JOIN [Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_InnerJoinSemantics_NonFirstTable_SubstatementTableInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var tableInfo = new ResolvedSubStatementTableInfo("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);

      _stageMock
        .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
        .WhenCalled(mi=> ((ISqlCommandBuilder) mi.Arguments[0]).Append("[Table] AS [t]"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: false);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS APPLY ([Table] AS [t]) AS [q0]"));
    }

    [Test]
    public void GenerateSql_LeftJoinSemantics_FirstTable ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Left);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: true);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(SELECT NULL AS [Empty]) AS [Empty] OUTER APPLY [Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_LeftJoinSemantics_NonFirstTable ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Left);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: false);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" OUTER APPLY [Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_JoinedTable_WithLeftJoinSemantics ()
    {
      var condition = Expression.Constant (true);
      var joinInfo = new ResolvedJoinInfo (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), condition);

      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo("KitchenTable", "k", JoinSemantics.Inner);
      sqlTable.GetOrAddLeftJoin (joinInfo, ExpressionHelper.GetMember<Kitchen> (k => k.Cook));
      
      _stageMock
        .Expect (mock => mock.GenerateTextForJoinCondition (_commandBuilder, condition))
        .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("condition"));

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: true);
      
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[KitchenTable] AS [k] LEFT OUTER JOIN [CookTable] AS [c] ON condition"));
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
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("XXX"));
      _stageMock.Replay();

      _generator.VisitSubStatementTableInfo (resolvedSubTableInfo);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    public void VisitResolvedJoinedGroupingTableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var resolvedSubTableInfo = SqlStatementModelObjectMother.CreateResolvedJoinedGroupingTableInfo (sqlStatement);

      _stageMock
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("XXX"));
      _stageMock.Replay ();

      _generator.VisitSubStatementTableInfo (resolvedSubTableInfo);

      _stageMock.VerifyAllExpectations ();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    public void VisitResolvedJoinInfo ()
    {
      var condition = Expression.Constant (true);
      var resolvedJoinInfo = new ResolvedJoinInfo (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), condition);

      _stageMock
        .Expect (mock => mock.GenerateTextForJoinCondition (_commandBuilder, condition))
        .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("condition"));
      _stageMock.Replay();

      _generator.VisitResolvedJoinInfo (resolvedJoinInfo);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c] ON condition"));
    }

    [Test]
    public void GenerateSql_JoinedTable_WithInnerJoinSemantics ()
    {
      var condition = Expression.Constant (true);
      var joinInfo = new ResolvedJoinInfo (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), condition);

      var sqlTable = new SqlTable (new SqlJoinedTable (joinInfo, JoinSemantics.Inner), JoinSemantics.Inner);

      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, isFirstTable: true),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("SqlJoinedTable as TableInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedTableInfo_RaisesException ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedTableInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedJoinInfo ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, kitchenCookMember, JoinCardinality.One);

      originalTable.GetOrAddLeftJoin (unresolvedJoinInfo, kitchenCookMember);

      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedJoinInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedCollectionJoinInfo ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var collectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook[] { }), memberInfo);

      originalTable.GetOrAddLeftJoin (collectionJoinInfo, memberInfo);

      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedCollectionJoinInfo is not valid at this point."));
    }

    [Test]
    public void GenerateSql_WithUnresolvedGroupReferenceTableInfo ()
    {
      var tableInfo = SqlStatementModelObjectMother.CreateUnresolvedGroupReferenceTableInfo();

      Assert.That (
          () => SqlTableAndJoinTextGenerator.GenerateSql (new SqlTable (tableInfo, JoinSemantics.Inner), _commandBuilder, _stageMock, false),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("UnresolvedGroupReferenceTableInfo is not valid at this point."));
    }

    private ResolvedJoinInfo CreateResolvedJoinInfo (
        Type type, string originalTableAlias, string leftSideKeyName, string joinedTableName, string joinedTableAlias, string rightSideKeyName)
    {
      var foreignTableSource = new ResolvedSimpleTableInfo (type, joinedTableName, joinedTableAlias);
      var primaryColumn = new SqlColumnDefinitionExpression (typeof (int), originalTableAlias, leftSideKeyName, false);
      var foreignColumn = new SqlColumnDefinitionExpression (typeof (int), joinedTableAlias, rightSideKeyName, false);
      return new ResolvedJoinInfo (foreignTableSource, Expression.Equal (primaryColumn, foreignColumn));
    }
  }
}