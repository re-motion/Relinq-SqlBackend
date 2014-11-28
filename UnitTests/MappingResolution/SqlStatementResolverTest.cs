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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SqlStatementResolverTest
  {
    private TestableSqlStatementResolver _visitor;

    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedSimpleTableInfo _fakeResolvedSimpleTableInfo;
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _mappingResolutionContext = new MappingResolutionContext();

      _visitor = new TestableSqlStatementResolver (_stageMock.Object, _mappingResolutionContext);

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
    }

    [Test]
    public void ResolveSqlTable_ResolvesTableInfo ()
    {
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _mappingResolutionContext))
          .Returns (_fakeResolvedSimpleTableInfo)
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      Assert.That (_sqlTable.TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinedTable ()
    {
      var unresolvedJoinTableInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook();
      var joinedTable = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo);
      var joinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _sqlTable.AddJoin (new SqlJoin (joinedTable, JoinSemantics.Left, joinCondition));

      var sequence = new MockSequence();
      _stageMock
          .InSequence (sequence)
          .Expect (mock => mock.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext))
          .Return (_fakeResolvedSimpleTableInfo);
          .Verifiable();
      var fakeResolvedJoinedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _stageMock
          .InSequence (sequence)
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedJoinedTableInfo)
          .Verifiable();
      var fakeResolvedJoinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .InSequence (sequence)
          .Setup (mock => mock.ResolveJoinCondition (joinCondition, _mappingResolutionContext))
          .Returns (fakeResolvedJoinCondition)
          .Callback ((Expression _1, IMappingResolutionContext _2) => Assert.That (joinedTable.TableInfo, Is.SameAs (fakeResolvedJoinedTableInfo)))
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      Assert.That (joinedTable.TableInfo, Is.SameAs (fakeResolvedJoinedTableInfo));
      Assert.That (_sqlTable.OrderedJoins.Single().JoinCondition, Is.SameAs (fakeResolvedJoinCondition));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_Recursive ()
    {
      var unresolvedJoinTableInfo1 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook();
      var joinedTable1 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo1);
      var joinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      _sqlTable.AddJoin (new SqlJoin (joinedTable1, JoinSemantics.Left, joinCondition1));

      var unresolvedJoinTableInfo2 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_CookSubstitution();
      var joinedTable2 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo2);
      var joinCondition2 = ExpressionHelper.CreateExpression (typeof (bool));
      _sqlTable.AddJoin (new SqlJoin (joinedTable2, JoinSemantics.Left, joinCondition2));

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext))
          .Returns (_fakeResolvedSimpleTableInfo);
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo1, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo2, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition1, _mappingResolutionContext))
          .Returns (ExpressionHelper.CreateExpression (typeof (bool)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition2, _mappingResolutionContext))
          .Returns (ExpressionHelper.CreateExpression (typeof (bool)))
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
    }

    [Test]
    public void ResolveSelectProjection_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var sqlStatementBuilder = new SqlStatementBuilder();
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.ResolveSelectProjection (expression, sqlStatementBuilder);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveTopExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveTopExpression (expression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.ResolveTopExpression (expression);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveGroupByExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveGroupByExpression (expression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.ResolveGroupByExpression (expression);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveWhereCondition_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveWhereExpression (expression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.ResolveWhereCondition (expression);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveOrderingExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveOrderingExpression (expression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.ResolveOrderingExpression (expression);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    // TODO RMLNQSQL-64
    //[Test]
    //public void ResolveJoin ()
    //{
    //  var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
    //  var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);

    //  _stageMock
    //      .Setup (mock => mock.ResolveSqlJoinedTable (joinedTable, _mappingResolutionContext))
    //      .Verifiable();

    //  _visitor.ResolveJoin (joinedTable);

    //  _stageMock.VerifyAllExpectations();
    //}

    [Test]
    public void ResolveSqlStatement ()
    {
      var constantExpression = Expression.Constant(new Restaurant());
      var whereCondition = Expression.Constant(true);
      var topExpression = Expression.Constant("top");
      var groupExpression = Expression.Constant ("group");
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Desc);
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                                {
                                    SelectProjection = constantExpression,
                                    DataInfo = new StreamedSequenceInfo(typeof(Restaurant[]), constantExpression),
                                    WhereCondition = whereCondition,
                                    GroupByExpression =  groupExpression,
                                    TopExpression = topExpression,
                                    SetOperationCombinedStatements = { setOperationCombinedStatement }
                                };
      builder.Orderings.Add (ordering);
      var sqlStatement = builder.GetSqlStatement();
      var fakeExpression = Expression.Constant (new Cook());
      var fakeWhereCondition = Expression.Constant (true);
      var fakeGroupExpression = Expression.Constant ("group");
      var fakeTopExpression = Expression.Constant ("top");
      var fakeOrderExpression = Expression.Constant ("order");
      var fakeSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      Assert.That (fakeSqlStatement, Is.Not.EqualTo (setOperationCombinedStatement.SqlStatement), "This is important for the test below.");

      _stageMock
          .Setup (mock => mock.ResolveSelectExpression (constantExpression, It.IsAny<SqlStatementBuilder>(), _mappingResolutionContext))
          .Returns (fakeExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveWhereExpression(whereCondition, _mappingResolutionContext))
          .Returns (fakeWhereCondition)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveGroupByExpression (groupExpression, _mappingResolutionContext))
          .Returns (fakeGroupExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTopExpression(topExpression, _mappingResolutionContext))
          .Returns (fakeTopExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveOrderingExpression(ordering.Expression, _mappingResolutionContext))
          .Returns (fakeOrderExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo(sqlStatement.SqlTables[0].TableInfo, _mappingResolutionContext))
          .Returns (new ResolvedSimpleTableInfo(typeof(Cook), "CookTable", "c"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveSqlStatement (setOperationCombinedStatement.SqlStatement, _mappingResolutionContext))
          .Returns (fakeSqlStatement)
          .Verifiable();

      var resolvedSqlStatement = _visitor.ResolveSqlStatement (sqlStatement);

      _stageMock.Verify();
      Assert.That (resolvedSqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) resolvedSqlStatement.DataInfo).DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(Cook))));
      Assert.That (resolvedSqlStatement.SelectProjection, Is.SameAs(fakeExpression));
      Assert.That (resolvedSqlStatement.WhereCondition, Is.SameAs (fakeWhereCondition));
      Assert.That (resolvedSqlStatement.TopExpression, Is.SameAs (fakeTopExpression));
      Assert.That (resolvedSqlStatement.GroupByExpression, Is.SameAs (fakeGroupExpression));
      Assert.That (resolvedSqlStatement.Orderings[0].Expression, Is.SameAs (fakeOrderExpression));
      Assert.That (sqlStatement.Orderings[0].Expression, Is.SameAs (ordering.Expression));
      Assert.That (resolvedSqlStatement.SetOperationCombinedStatements.Single().SqlStatement, Is.SameAs (fakeSqlStatement));
    }

    [Test]
    public void ResolveSqlStatement_WithNoChanges_ShouldLeaveAllObjectsTheSame ()
    {
      var constantExpression = Expression.Constant(new Restaurant());
      var whereCondition = Expression.Constant(true);
      var topExpression = Expression.Constant("top");
      var groupExpression = Expression.Constant ("group");
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Desc);
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                                {
                                    SelectProjection = constantExpression,
                                    DataInfo = new StreamedSequenceInfo(typeof(Restaurant[]), constantExpression),
                                    WhereCondition = whereCondition,
                                    GroupByExpression =  groupExpression,
                                    TopExpression = topExpression,
                                    SetOperationCombinedStatements = { setOperationCombinedStatement }
                                };
      builder.Orderings.Add (ordering);
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Setup (mock => mock.ResolveSelectExpression (constantExpression, It.IsAny<SqlStatementBuilder>(), _mappingResolutionContext))
          .Returns (constantExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveWhereExpression(whereCondition, _mappingResolutionContext))
          .Returns (whereCondition)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveGroupByExpression (groupExpression, _mappingResolutionContext))
          .Returns (groupExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTopExpression(topExpression, _mappingResolutionContext))
          .Returns (topExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveOrderingExpression(ordering.Expression, _mappingResolutionContext))
          .Returns (ordering.Expression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo(sqlStatement.SqlTables[0].TableInfo, _mappingResolutionContext))
          .Returns ((IResolvedTableInfo) sqlStatement.SqlTables[0].TableInfo)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveSqlStatement (setOperationCombinedStatement.SqlStatement, _mappingResolutionContext))
          .Returns (setOperationCombinedStatement.SqlStatement)
          .Verifiable();

      var resolvedSqlStatement = _visitor.ResolveSqlStatement (sqlStatement);

      _stageMock.Verify();

      Assert.That (resolvedSqlStatement, Is.EqualTo (sqlStatement));
    }
  }
}