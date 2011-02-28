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
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlStatementResolverTest
  {
    private TestableSqlStatementResolver _visitor;

    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedSimpleTableInfo _fakeResolvedSimpleTableInfo;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _mappingResolutionContext = new MappingResolutionContext();

      _visitor = new TestableSqlStatementResolver (_stageMock, _mappingResolutionContext);

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
    }

    [Test]
    public void ResolveSqlTable_ResolvesTableInfo ()
    {
      _stageMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _mappingResolutionContext))
          .Return (_fakeResolvedSimpleTableInfo);
      _stageMock.Replay();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (_sqlTable.TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, memberInfo, JoinCardinality.One);
      var join = _sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo, memberInfo);

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _mappingResolutionContext))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo);
      }
      _stageMock.Replay();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join.JoinInfo, Is.SameAs (fakeResolvedJoinInfo));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_Multiple ()
    {
      var memberInfo1 = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression1 = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo1 = new UnresolvedJoinInfo (entityExpression1, memberInfo1, JoinCardinality.One);
      var memberInfo2 = typeof (Kitchen).GetProperty ("Restaurant");
      var entityExpression2 = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo2 = new UnresolvedJoinInfo (entityExpression2, memberInfo2, JoinCardinality.One);
      var join1 = _sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo1, memberInfo1);
      var join2 = _sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo2, memberInfo2);

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Restaurant));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _mappingResolutionContext))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join1.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo1);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join2.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo2);
      }
      _stageMock.Replay();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
    }

    [Test]
    public void ResolveSqlTable_ResolvesJoinInfo_Recursive ()
    {
      var memberInfo1 = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression1 = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo1 = new UnresolvedJoinInfo (entityExpression1, memberInfo1, JoinCardinality.One);
      var memberInfo2 = typeof (Cook).GetProperty ("Substitution");
      var entityExpression2 = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo2 = new UnresolvedJoinInfo (entityExpression2, memberInfo2, JoinCardinality.One);
      var memberInfo3 = typeof (Cook).GetProperty ("Name");
      var entityExpression3 = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo3 = new UnresolvedJoinInfo (entityExpression3, memberInfo3, JoinCardinality.One);
      
      var join1 = _sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo1, memberInfo1);
      var join2 = join1.GetOrAddLeftJoin (unresolvedJoinInfo2, memberInfo2);
      var join3 = join1.GetOrAddLeftJoin (unresolvedJoinInfo3, memberInfo3);

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo3 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (string));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _mappingResolutionContext))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join1.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo1);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join2.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo2);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo (join3.JoinInfo, _mappingResolutionContext))
            .Return (fakeResolvedJoinInfo3);
      }
      _stageMock.Replay();

      _visitor.ResolveSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
      Assert.That (join3.JoinInfo, Is.SameAs (fakeResolvedJoinInfo3));
    }

    [Test]
    public void ResolveSelectProjection_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var sqlStatementBuilder = new SqlStatementBuilder();
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.ResolveSelectProjection (expression, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveTopExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTopExpression (expression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.ResolveTopExpression (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveGroupByExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveGroupByExpression (expression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay ();

      var result = _visitor.ResolveGroupByExpression (expression);

      _stageMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveWhereCondition_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveWhereExpression (expression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.ResolveWhereCondition (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveOrderingExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveOrderingExpression (expression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.ResolveOrderingExpression (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveJoinedTable ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);

      var fakeJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();
      
      _stageMock
          .Expect (mock => mock.ResolveJoinInfo (joinInfo, _mappingResolutionContext))
          .Return (fakeJoinInfo);
      _stageMock.Replay();

      _visitor.ResolveJoinedTable (joinedTable);

      Assert.That (joinedTable.JoinInfo, Is.SameAs (fakeJoinInfo));
    }

    [Test]
    public void ResolveSqlStatement ()
    {
      var constantExpression = Expression.Constant(new Restaurant());
      var whereCondition = Expression.Constant(true);
      var topExpression = Expression.Constant("top");
      var groupExpression = Expression.Constant ("group");
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Desc);
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                                {
                                    SelectProjection = constantExpression,
                                    DataInfo = new StreamedSequenceInfo(typeof(Restaurant[]), constantExpression),
                                    WhereCondition = whereCondition,
                                    GroupByExpression =  groupExpression,
                                    TopExpression = topExpression,
                                };
      builder.Orderings.Add (ordering);
      var sqlStatement = builder.GetSqlStatement();
      var fakeExpression = Expression.Constant (new Cook());
      var fakeWhereCondition = Expression.Constant (true);
      var fakeGroupExpression = Expression.Constant ("group");
      var fakeTopExpression = Expression.Constant ("top");
      var fakeOrderExpression = Expression.Constant ("order");

      _stageMock
          .Expect (mock => mock.ResolveSelectExpression (Arg.Is(constantExpression), Arg<SqlStatementBuilder>.Is.Anything, Arg.Is(_mappingResolutionContext)))
          .Return (fakeExpression);
      _stageMock
          .Expect (mock => mock.ResolveWhereExpression(whereCondition, _mappingResolutionContext))
          .Return (fakeWhereCondition);
      _stageMock
          .Expect (mock => mock.ResolveGroupByExpression (groupExpression, _mappingResolutionContext))
          .Return (fakeGroupExpression);
      _stageMock
          .Expect (mock => mock.ResolveTopExpression(topExpression, _mappingResolutionContext))
          .Return (fakeTopExpression);
      _stageMock
          .Expect (mock => mock.ResolveOrderingExpression(ordering.Expression, _mappingResolutionContext))
          .Return (fakeOrderExpression);
      _stageMock
          .Expect (mock => mock.ResolveTableInfo(((SqlTable) sqlStatement.SqlTables[0]).TableInfo, _mappingResolutionContext))
          .Return (new ResolvedSimpleTableInfo(typeof(Cook), "CookTable", "c"));
      _stageMock.Replay();

      var resolveSqlStatement = _visitor.ResolveSqlStatement (sqlStatement);

      _stageMock.VerifyAllExpectations();
      Assert.That (resolveSqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) resolveSqlStatement.DataInfo).DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(Cook))));
      Assert.That (resolveSqlStatement.SelectProjection, Is.SameAs(fakeExpression));
      Assert.That (resolveSqlStatement.WhereCondition, Is.SameAs (fakeWhereCondition));
      Assert.That (resolveSqlStatement.TopExpression, Is.SameAs (fakeTopExpression));
      Assert.That (resolveSqlStatement.GroupByExpression, Is.SameAs (fakeGroupExpression));
      Assert.That (resolveSqlStatement.Orderings[0].Expression, Is.SameAs (fakeOrderExpression));
      Assert.That (sqlStatement.Orderings[0].Expression, Is.SameAs (ordering.Expression));
   }
  

  }
}