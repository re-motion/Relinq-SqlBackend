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
      _sqlTable.AddJoinForExplicitQuerySource (new SqlJoin (joinedTable, JoinSemantics.Left, joinCondition));

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext))
          .Returns (_fakeResolvedSimpleTableInfo)
          .Verifiable();
      var fakeResolvedJoinedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedJoinedTableInfo)
          .Verifiable();
      var fakeResolvedJoinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition, _mappingResolutionContext))
          .Returns (fakeResolvedJoinCondition)
          .Callback ((Expression _1, IMappingResolutionContext _2) => Assert.That (joinedTable.TableInfo, Is.SameAs (fakeResolvedJoinedTableInfo)))
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      Assert.That (joinedTable.TableInfo, Is.SameAs (fakeResolvedJoinedTableInfo));
      Assert.That (_sqlTable.Joins.Single().JoinCondition, Is.SameAs (fakeResolvedJoinCondition));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_Multiple ()
    {
      var unresolvedJoinTableInfo1 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook();
      var joinedTable1 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo1);
      var originalJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var resolvedJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin1 = new SqlJoin (joinedTable1, JoinSemantics.Left, originalJoinCondition1);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin1);

      var resolvedJoinTableInfo2 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenRestaurant();
      var joinedTable2 = SqlStatementModelObjectMother.CreateSqlTable (resolvedJoinTableInfo2);
      var joinCondition2 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin2 = new SqlJoin (joinedTable2, JoinSemantics.Left, joinCondition2);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin2);

      var unresolvedJoinTableInfo3 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenRestaurant();
      var joinedTable3 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo3);
      var originalJoinCondition3 = ExpressionHelper.CreateExpression (typeof (bool));
      var resolvedJoinCondition3 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin3 = new SqlJoin (joinedTable3, JoinSemantics.Inner, originalJoinCondition3);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin3);

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext))
          .Returns (_fakeResolvedSimpleTableInfo);
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo1, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (resolvedJoinTableInfo2, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Restaurant)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo3, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Restaurant)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (originalJoinCondition1, _mappingResolutionContext))
          .Returns (resolvedJoinCondition1)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition2, _mappingResolutionContext))
          .Returns (joinCondition2)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (originalJoinCondition3, _mappingResolutionContext))
          .Returns (resolvedJoinCondition3)
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      var orderedJoins = _sqlTable.Joins.ToArray();
      Assert.That (orderedJoins.Length, Is.EqualTo (3));

      Assert.That (orderedJoins[0], Is.Not.SameAs (originalJoin1));
      Assert.That (orderedJoins[0].JoinedTable, Is.SameAs (originalJoin1.JoinedTable));
      Assert.That (orderedJoins[0].JoinSemantics, Is.EqualTo (originalJoin1.JoinSemantics));
      Assert.That (orderedJoins[0].JoinCondition, Is.SameAs (resolvedJoinCondition1));

      Assert.That (orderedJoins[1], Is.SameAs (originalJoin2));

      Assert.That (orderedJoins[2], Is.Not.SameAs (originalJoin3));
      Assert.That (orderedJoins[2].JoinedTable, Is.SameAs (originalJoin3.JoinedTable));
      Assert.That (orderedJoins[2].JoinSemantics, Is.EqualTo (originalJoin3.JoinSemantics));
      Assert.That (orderedJoins[2].JoinCondition, Is.SameAs (resolvedJoinCondition3));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_Recursive ()
    {
      var unresolvedJoinTableInfo1 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook();
      var joinedTable1 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo1);
      var originalJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var resolvedJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin1 = new SqlJoin (joinedTable1, JoinSemantics.Left, originalJoinCondition1);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin1);

      var unresolvedJoinTableInfo2 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_CookSubstitution();
      var joinedTable2 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo2);
      var originalJoinCondition2 = ExpressionHelper.CreateExpression (typeof (bool));
      var resolvedJoinCondition2 = ExpressionHelper.CreateExpression (typeof (bool));
      var memberInfo = SqlStatementModelObjectMother.GetCookSubstitutionMemberInfo();
      var originalJoin2 = joinedTable1.GetOrAddMemberBasedLeftJoin (memberInfo, () => new SqlTable.LeftJoinData (joinedTable2, originalJoinCondition2));

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
          .Setup (mock => mock.ResolveJoinCondition (originalJoinCondition1, _mappingResolutionContext))
          .Returns (resolvedJoinCondition1)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (originalJoinCondition2, _mappingResolutionContext))
          .Returns (resolvedJoinCondition2)
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      var orderedJoins = _sqlTable.Joins.ToArray();
      Assert.That (orderedJoins.Length, Is.EqualTo (1));

      Assert.That (orderedJoins[0], Is.Not.SameAs (originalJoin1));
      Assert.That (orderedJoins[0].JoinedTable, Is.SameAs (originalJoin1.JoinedTable));
      Assert.That (orderedJoins[0].JoinSemantics, Is.EqualTo (originalJoin1.JoinSemantics));
      Assert.That (orderedJoins[0].JoinCondition, Is.SameAs (resolvedJoinCondition1));

      var ordedJoins1 = joinedTable1.Joins.ToArray();
      Assert.That (ordedJoins1.Length, Is.EqualTo (1));
      Assert.That (ordedJoins1[0], Is.Not.SameAs (originalJoin2));
      Assert.That (ordedJoins1[0].JoinedTable, Is.SameAs (originalJoin2.JoinedTable));
      Assert.That (ordedJoins1[0].JoinSemantics, Is.EqualTo (originalJoin2.JoinSemantics));
      Assert.That (ordedJoins1[0].JoinCondition, Is.SameAs (resolvedJoinCondition2));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_AddsNewJoinInfoToSqlTable()
    {
      var unresolvedJoinTableInfo1 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook();
      var joinedTable1 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo1);
      var originalJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var resolvedJoinCondition1 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin1 = new SqlJoin (joinedTable1, JoinSemantics.Left, originalJoinCondition1);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin1);

      var resolvedJoinTableInfo2 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenRestaurant();
      var joinedTable2 = SqlStatementModelObjectMother.CreateSqlTable (resolvedJoinTableInfo2);
      var joinCondition2 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin2 = new SqlJoin (joinedTable2, JoinSemantics.Left, joinCondition2);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin2);

      var unresolvedJoinTableInfo3 = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_CookSubstitution();
      var joinedTable3 = SqlStatementModelObjectMother.CreateSqlTable (unresolvedJoinTableInfo3);
      var joinCondition3 = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin3 = new SqlJoin (joinedTable3, JoinSemantics.Left, joinCondition3);

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext))
          .Returns (_fakeResolvedSimpleTableInfo);
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo1, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (resolvedJoinTableInfo2, _mappingResolutionContext))
          .Returns (SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Restaurant)))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (originalJoinCondition1, _mappingResolutionContext))
          .Callback ((Expression _1, IMappingResolutionContext _2) => _sqlTable.AddJoinForExplicitQuerySource (originalJoin3))
          .Returns (resolvedJoinCondition1)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition2, _mappingResolutionContext))
          .Returns (joinCondition2)
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      // Documents that the newly aded table will not be resolved. This means that real code needs to add already resolved tables.
      _stageMock.Verify (mock => mock.ResolveTableInfo (unresolvedJoinTableInfo3, _mappingResolutionContext),Times.Never());
      // Documents that the newly added join will not be resolved. This means that real code needs to add already resolved joins.
      _stageMock.Verify (mock => mock.ResolveJoinCondition (joinCondition3, _mappingResolutionContext), Times.Never());
      _stageMock.Verify();
      var orderedJoins = _sqlTable.Joins.ToArray();
      Assert.That (orderedJoins.Length, Is.EqualTo (3));

      Assert.That (orderedJoins[0], Is.Not.SameAs (originalJoin1));
      Assert.That (orderedJoins[0].JoinedTable, Is.SameAs (originalJoin1.JoinedTable));
      Assert.That (orderedJoins[0].JoinSemantics, Is.EqualTo (originalJoin1.JoinSemantics));
      Assert.That (orderedJoins[0].JoinCondition, Is.SameAs (resolvedJoinCondition1));

      Assert.That (orderedJoins[1], Is.SameAs (originalJoin2));

      Assert.That (orderedJoins[2], Is.SameAs (originalJoin3));
    }

    [Test]
    public void ResolveSqlTable_WithAlreadyResolvedJoinConditions_LeavesJoinsUntouchtes ()
    {
      var joinedTable = SqlStatementModelObjectMother.CreateSqlTable (SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo_KitchenCook());
      var joinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      var originalJoin = new SqlJoin (joinedTable, JoinSemantics.Left, joinCondition);
      _sqlTable.AddJoinForExplicitQuerySource (originalJoin);

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (It.IsAny<ITableInfo>(), It.IsAny<IMappingResolutionContext>()))
          .Returns (_fakeResolvedSimpleTableInfo);
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (joinCondition, _mappingResolutionContext))
          .Returns (joinCondition)
          .Verifiable();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.Verify();
      Assert.That (_sqlTable.Joins.Single(), Is.SameAs (originalJoin));
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
          .Setup (mock => mock.ResolveTableInfo(sqlStatement.SqlTables[0].SqlTable.TableInfo, _mappingResolutionContext))
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
          .Setup (mock => mock.ResolveTableInfo(sqlStatement.SqlTables[0].SqlTable.TableInfo, _mappingResolutionContext))
          .Returns ((IResolvedTableInfo) sqlStatement.SqlTables[0].SqlTable.TableInfo)
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