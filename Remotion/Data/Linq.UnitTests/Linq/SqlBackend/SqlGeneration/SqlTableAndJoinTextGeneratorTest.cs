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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
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
      _generator = new TestableSqlTableAndJoinTextGenerator (_commandBuilder, _stageMock, SqlTableAndJoinTextGenerator.Context.FirstTable);
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
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"));
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (originalTable, kitchenCookMember, JoinCardinality.One);
      var joinedTable = originalTable.GetOrAddLeftJoin (unresolvedJoinInfo, kitchenCookMember);

      joinedTable.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK");

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).LeftKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t1].[ID]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).RightKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[FK]"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (
          _commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[FK]"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable_Recursive ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"));
      var memberInfo1 = typeof (Kitchen).GetProperty ("Cook");
      var unresolvedJoinInfo1 = new UnresolvedJoinInfo (originalTable, memberInfo1, JoinCardinality.One);
      var memberInfo2 = typeof (Cook).GetProperty ("Substitution");
      var unresolvedJoinInfo2 = new UnresolvedJoinInfo (originalTable, memberInfo2, JoinCardinality.One);
      var joinedTable1 = originalTable.GetOrAddLeftJoin (unresolvedJoinInfo1, memberInfo1);
      var joinedTable2 = joinedTable1.GetOrAddLeftJoin (unresolvedJoinInfo2, memberInfo2);

      joinedTable1.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK");
      joinedTable2.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t2", "ID2", "CookTable2", "t3", "FK2");

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable1.JoinInfo).LeftKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t1].[ID]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable1.JoinInfo).RightKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[FK]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable2.JoinInfo).LeftKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[ID2]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable2.JoinInfo).RightKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t3].[FK2]"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo (
              "[KitchenTable] AS [t1] LEFT OUTER JOIN "
              + "[CookTable] AS [t2] ON [t1].[ID] = [t2].[FK] LEFT OUTER JOIN "
              + "[CookTable2] AS [t3] ON [t2].[ID2] = [t3].[FK2]"));
    }

    [Test]
    public void VisitSqlTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo ();
      sqlTable.TableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");

      _generator.VisitSqlTable (sqlTable);
      
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void VisitSqlJoinedTable_WithLeftJoinSemantic ()
    {
      var leftKey = new SqlLiteralExpression (1);
      var rightKey = new SqlLiteralExpression (1);
      var sqlTable =
          new SqlJoinedTable (
              new ResolvedJoinInfo (
                  new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), leftKey, rightKey),JoinSemantics.Left);

      _stageMock
        .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, leftKey))
        .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("leftKey"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, rightKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("rightKey"));

      _generator.VisitSqlJoinedTable (sqlTable);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN [CookTable] AS [c] ON leftKey = rightKey"));
    }

    [Test]
    public void VisitSqlJoinedTable_WithInnerJoinSemantic ()
    {
      var leftKey = new SqlLiteralExpression (1);
      var rightKey = new SqlLiteralExpression (1);
      var sqlTable =
          new SqlJoinedTable (
              new ResolvedJoinInfo (
                  new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), leftKey, rightKey), JoinSemantics.Inner);

      _stageMock
        .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, leftKey))
        .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("leftKey"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, rightKey))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("rightKey"));

      _generator.VisitSqlJoinedTable (sqlTable);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c] ON leftKey = rightKey"));
    }

    [Test]
    public void VisitSimpleTableInfo_FirstTable ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      _generator.VisitSimpleTableInfo (simpleTableInfo);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c]"));
    }

    [Test]
    public void VisitSimpleTableInfo_NonFirstTable ()
    {
      _generator = new TestableSqlTableAndJoinTextGenerator (_commandBuilder, _stageMock, SqlTableAndJoinTextGenerator.Context.NonFirstTable);
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      _generator.VisitSimpleTableInfo (simpleTableInfo);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS JOIN [CookTable] AS [c]"));
    }

    [Test]
    public void VisitSubStatementTableInfo_FirstTable ()
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
    public void VisitSubStatementTableInfo_NonFirstTable ()
    {
      _generator = new TestableSqlTableAndJoinTextGenerator (_commandBuilder, _stageMock, SqlTableAndJoinTextGenerator.Context.NonFirstTable);
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
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS APPLY (XXX) AS [cook]"));
    }

    [Test]
    public void VisitResolvedJoinInfo ()
    {
      var leftKey = new SqlLiteralExpression (1);
      var rightKey = new SqlLiteralExpression (1);
      var resolvedJoinInfo = new ResolvedJoinInfo (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), leftKey, rightKey);

      _stageMock
        .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, leftKey))
        .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("leftKey"));
      _stageMock
         .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, rightKey))
         .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("rightKey"));
      _stageMock.Replay();

      _generator.VisitResolvedJoinInfo (resolvedJoinInfo);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[CookTable] AS [c] ON leftKey = rightKey"));
    }
    
    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedTableInfo is not valid at this point.")
    ]
    public void GenerateSql_WithUnresolvedTableInfo_RaisesException ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo ();
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, false);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedJoinInfo is not valid at this point.")]
    public void GenerateSql_WithUnresolvedJoinInfo ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (originalTable, kitchenCookMember, JoinCardinality.One);

      originalTable.GetOrAddLeftJoin (unresolvedJoinInfo, kitchenCookMember);

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, false);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedCollectionJoinInfo is not valid at this point.")]
    public void GenerateSql_WithUnresolvedCollectionJoinInfo ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var collectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook[] { }), memberInfo);

      originalTable.GetOrAddLeftJoin (collectionJoinInfo, memberInfo);

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, false);
    }

    private ResolvedJoinInfo CreateResolvedJoinInfo (
        Type type, string originalTableAlias, string leftSideKeyName, string joinedTableName, string joinedTableAlias, string rightSideKeyName)
    {
      var foreignTableSource = new ResolvedSimpleTableInfo (type, joinedTableName, joinedTableAlias);
      var primaryColumn = new SqlColumnExpression (typeof (int), originalTableAlias, leftSideKeyName, false);
      var foreignColumn = new SqlColumnExpression (typeof (int), joinedTableAlias, rightSideKeyName, false);
      return new ResolvedJoinInfo (foreignTableSource, primaryColumn, foreignColumn);
    }
  }
}