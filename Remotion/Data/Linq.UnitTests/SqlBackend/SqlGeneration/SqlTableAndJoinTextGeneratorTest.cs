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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlTableAndJoinTextGeneratorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlGenerationStage>();
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void GenerateSql_ForTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      sqlTable.TableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable , _commandBuilder, _stageMock, true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateSql_ForSeveralTables ()
    {
      var sqlTable1 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo("Table1", "t1");
      var sqlTable2 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo("Table2", "t2");
      var sqlTable3 = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo ("Table3", "t3");
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable1, _commandBuilder, _stageMock, true);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable2, _commandBuilder, _stageMock, false);
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable3, _commandBuilder, _stageMock, false);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[Table1] AS [t1] CROSS JOIN [Table2] AS [t2] CROSS JOIN [Table3] AS [t3]"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"));

      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var joinedTable = originalTable.GetOrAddJoin (kitchenCookMember, JoinCardinality.One);

      joinedTable.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK");

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).PrimaryColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t1].[ID]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).ForeignColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[FK]"));
      _stageMock.Replay();
      
      SqlTableAndJoinTextGenerator.GenerateSql (originalTable , _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[FK]"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable_ValueSemantics ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"));

      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var joinedTable = originalTable.GetOrAddJoin (kitchenCookMember, JoinCardinality.One);

      var foreignTableSource = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "t2");
      var primaryColumn = new SqlColumnExpression (typeof (bool), "t1", "ID");
      var foreignColumn = new SqlColumnExpression (typeof (bool), "t2", "FK");
      joinedTable.JoinInfo = new ResolvedJoinInfo (foreignTableSource, primaryColumn, foreignColumn);

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).PrimaryColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t1].[ID]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable.JoinInfo).ForeignColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[FK]"));
      _stageMock.Replay ();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable , _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[FK]"));
    }

    [Test]
    public void GenerateSql_ForJoinedTable_Recursive ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "t1"));

      var joinedTable1 = originalTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
      var joinedTable2 = joinedTable1.GetOrAddJoin (typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);

      joinedTable1.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t1", "ID", "CookTable", "t2", "FK");
      joinedTable2.JoinInfo = CreateResolvedJoinInfo (typeof (Cook), "t2", "ID2", "CookTable2", "t3", "FK2");

      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable1.JoinInfo).PrimaryColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t1].[ID]"));
      _stageMock
         .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable1.JoinInfo).ForeignColumn))
         .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[FK]"));
      _stageMock
          .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable2.JoinInfo).PrimaryColumn))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t2].[ID2]"));
      _stageMock
         .Expect (mock => mock.GenerateTextForJoinKeyExpression (_commandBuilder, ((ResolvedJoinInfo) joinedTable2.JoinInfo).ForeignColumn))
         .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t3].[FK2]"));
      _stageMock.Replay();

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable , _commandBuilder, _stageMock, true);

      _stageMock.VerifyAllExpectations();
      Assert.That (
          _commandBuilder.GetCommandText (), 
          Is.EqualTo (
              "[KitchenTable] AS [t1] LEFT OUTER JOIN "
             +"[CookTable] AS [t2] ON [t1].[ID] = [t2].[FK] LEFT OUTER JOIN "
             +"[CookTable2] AS [t3] ON [t2].[ID2] = [t3].[FK2]"));
    }

    [Test]
    public void VisitSubStatementTableInfo_WithFirstIsFalse ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var resolvedSubTableInfo = new ResolvedSubStatementTableInfo (typeof (Cook), "cook", sqlStatement);
      var sqlTable = new SqlTable (resolvedSubTableInfo);

      _stageMock
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("XXX"));

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, false);
      _stageMock.VerifyAllExpectations ();

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" CROSS APPLY (XXX) AS [cook]"));
    }

    [Test]
    public void VisitSubStatementTableInfo_WithFirstIsTrue ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var resolvedSubTableInfo = new ResolvedSubStatementTableInfo (typeof (Cook), "cook", sqlStatement);
      var sqlTable = new SqlTable (resolvedSubTableInfo);

      _stageMock
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("XXX"));

      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable, _commandBuilder, _stageMock, true);
      _stageMock.VerifyAllExpectations ();

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(XXX) AS [cook]"));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedTableInfo is not valid at this point.")]
    public void GenerateSql_WithUnresolvedTableInfo_RaisesException ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      SqlTableAndJoinTextGenerator.GenerateSql (sqlTable , _commandBuilder, _stageMock, false);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedJoinInfo is not valid at this point.")]
    public void GenerateSql_WithUnresolvedJoinInfo ()
    {
      var originalTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));

      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      originalTable.GetOrAddJoin (kitchenCookMember, JoinCardinality.One);

      SqlTableAndJoinTextGenerator.GenerateSql (originalTable, _commandBuilder, _stageMock, false);
    }

    private ResolvedJoinInfo CreateResolvedJoinInfo (
        Type type, string originalTableAlias, string leftSideKeyName, string joinedTableName, string joinedTableAlias, string rightSideKeyName)
    {
      var foreignTableSource = new ResolvedSimpleTableInfo (type, joinedTableName, joinedTableAlias);
      var primaryColumn = new SqlColumnExpression (typeof (int), originalTableAlias, leftSideKeyName);
      var foreignColumn = new SqlColumnExpression (typeof (int), joinedTableAlias, rightSideKeyName);
      return new ResolvedJoinInfo (foreignTableSource, primaryColumn, foreignColumn);
    }
  }
}