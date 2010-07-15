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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private SqlPreparationContext _context;
    private MethodCallTransformerRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _context = new SqlPreparationContext();
      _registry = MethodCallTransformerRegistry.CreateDefault();
    }

    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, info=>new SqlTable(info, JoinSemantics.Inner));

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = ((SqlTable) result.SqlTable).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetTableForFromExpression_SqlMemberExpression_ReturnsSqlTableWithJoinedTable ()
    {
      // from r in Restaurant => sqlTable 
      // from c in r.Cooks => MemberExpression (QSRExpression (r), "Cooks") => Join: sqlTable.Cooks

      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Restaurant()), memberInfo);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression, _stageMock, _generator, _registry, _context, info=>new SqlTable(info, JoinSemantics.Inner));

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (((SqlTable) result.SqlTable).TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (sqlTable.JoinedTables.ToArray().Contains (result.SqlTable), Is.False);
      Assert.That (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var joinInfo = ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo;

      Assert.That (joinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));

      Assert.That (((UnresolvedCollectionJoinInfo) joinInfo).MemberInfo, Is.EqualTo (memberInfo));
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Error parsing expression 'CustomExpression'. Expressions of type 'Cook[]' cannot be used as the SqlTables of a from clause.")]
    public void GetTableForFromExpression_UnsupportedExpression_Throws ()
    {
      var customExpression = new CustomExpression (typeof (Cook[]));

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          customExpression, _stageMock, _generator, _registry, _context, null);
    }

    [ExpectedException (typeof (NotSupportedException))]
    [Test]
    public void VisitEntityRefMemberExpression_ThrowsNotSupportException ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var stage = new DefaultSqlPreparationStage (_registry, new ResultOperatorHandlerRegistry (), _generator);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          stage,
          _generator,
          _registry,
          _context, 
          info => new SqlTable(info, JoinSemantics.Inner));

      Assert.That (((SqlTable) result.SqlTable).TableInfo, Is.InstanceOfType (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithOrderingsAndNoTopExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                    { Orderings = { new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc) } };
      var statement = builder.GetSqlStatement();
      var sqlSubStatementExpression = new SqlSubStatementExpression (statement);

      var stage = new DefaultSqlPreparationStage (_registry, new ResultOperatorHandlerRegistry (), _generator);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          stage,
          _generator,
          _registry,
          _context, 
          info => new SqlTable(info, JoinSemantics.Inner));

      var sqlTable = (SqlTable) result.SqlTable;
      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo(0));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Cook()), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression, _stageMock, _generator, _registry, _context, info=>new SqlTable(info, JoinSemantics.Inner));

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (((SqlTable) result.SqlTable).TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo).SourceExpression,
          Is.EqualTo (memberExpression.Expression));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo).MemberInfo,
          Is.EqualTo (memberExpression.Member));
      var expectedWherecondition = new JoinConditionExpression (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedWherecondition, result.WhereCondition);
    }

    [Test]
    public void VisitMemberExpression_InnerExpressionIsPrepared ()
    {
      var fakeQuerySource = MockRepository.GenerateStub<IQuerySource>();
      fakeQuerySource.Stub (stub => stub.ItemType).Return (typeof (Cook));
      
      var replacement = Expression.Constant (null, typeof (Cook));
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (fakeQuerySource), replacement);

      var memberExpression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (fakeQuerySource), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (memberExpression, _stageMock, _generator, _registry, _context, info=>new SqlTable(info, JoinSemantics.Inner));

      var sqlTable = (SqlTable) result.SqlTable;
      var joinedTable = (SqlJoinedTable) sqlTable.TableInfo;
      Assert.That (((UnresolvedCollectionJoinInfo) joinedTable.JoinInfo).SourceExpression, Is.SameAs (replacement));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_NoGrouping ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);

      Assert.That (result.SqlTable, Is.SameAs (sqlTable));
      Assert.That (result.IsNewTable, Is.False);
    }

    [Test]
    public void VisitSqlTableReferenceExpression_NoGrouping_String ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (string));
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);

      Assert.That (result.SqlTable, Is.SameAs (sqlTable));
      Assert.That (result.IsNewTable, Is.False);
    }

    [Test]
    public void VisitSqlTableReferenceExpression_Grouping ()
    {
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (IGrouping<string, int>), "test", "t0"), JoinSemantics.Inner);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);

      Assert.That (result.SqlTable, Is.Not.SameAs (sqlTable));
      Assert.That (result.IsNewTable, Is.True);
      Assert.That (result.WhereCondition, Is.Null);
      Assert.That (result.ExtractedOrderings, Is.Empty);

      var expectedItemSelector = new SqlTableReferenceExpression (result.SqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedItemSelector, result.ItemSelector);
      
      var tableInfo = ((SqlTable) result.SqlTable).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedGroupReferenceTableInfo)));
      var castTableInfo = (UnresolvedGroupReferenceTableInfo) tableInfo;
      Assert.That (castTableInfo.ItemType, Is.SameAs (typeof (int)));
      Assert.That (castTableInfo.ReferencedGroupSource, Is.SameAs (sqlTable));
      Assert.That (result.SqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), "test", "test");

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _registry, _context, null);
    }
  }
}