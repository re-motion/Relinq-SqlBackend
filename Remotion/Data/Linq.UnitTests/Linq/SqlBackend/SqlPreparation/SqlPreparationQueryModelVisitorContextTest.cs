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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationQueryModelVisitorContextTest
  {
    private ISqlPreparationContext _parentContext;
    private MainFromClause _parentSource;
    private SqlTable _parentSqlTable;

    private ISqlPreparationContext _context;
    private MainFromClause _source;
    private SqlTable _sqlTable;

    private ISqlPreparationStage _stageMock;
    private SqlPreparationQueryModelVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _parentContext = new SqlPreparationContext();
      _parentSource = ExpressionHelper.CreateMainFromClause_Cook();
      _parentSqlTable = new SqlTable (new UnresolvedTableInfo (typeof (int)));

      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _visitor = new TestableSqlPreparationQueryModelVisitor (
          _parentContext, _stageMock, new UniqueIdentifierGenerator(), ResultOperatorHandlerRegistry.CreateDefault());
      _context = new SqlPreparationQueryModelVisitorContext (_parentContext, _visitor);

      _source = ExpressionHelper.CreateMainFromClause_Cook ();
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (int)));
     
    }

    [Test]
    public void AddContextMapping ()
    {
      _context.AddContextMapping (new QuerySourceReferenceExpression(_source), new SqlTableReferenceExpression(_sqlTable));
      Assert.That (_context.QuerySourceMappingCount, Is.EqualTo (1));
    }

    [Test]
    public void GetContextMapping ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression(_source);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression(_sqlTable);
      _context.AddContextMapping (querySourceReferenceExpression, sqlTableReferenceExpression);
      Assert.That (_context.GetContextMapping (querySourceReferenceExpression), Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void TryGetContextMappingFromHierarchy ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression(_source);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression(_sqlTable);
      _context.AddContextMapping (querySourceReferenceExpression, sqlTableReferenceExpression);

      Expression result = _context.TryGetContextMappingFromHierarchy (querySourceReferenceExpression);
      
      Assert.That (result, Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void GetContextMapping_GetFromParentContext ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression(_parentSource);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression(_parentSqlTable);
      _parentContext.AddContextMapping (querySourceReferenceExpression, sqlTableReferenceExpression);
      Assert.That (_context.GetContextMapping (querySourceReferenceExpression), Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void TryGetContextMappingFromHierarchy_GetFromParentContext ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression(_parentSource);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression(_parentSqlTable);
      _parentContext.AddContextMapping (querySourceReferenceExpression, sqlTableReferenceExpression);

      Expression result = _context.TryGetContextMappingFromHierarchy (querySourceReferenceExpression);
      
      Assert.That (result, Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void GetContextMapping_GroupJoinClause ()
    {
      var groupJoinClause = ExpressionHelper.CreateGroupJoinClause ();

      var preparedExpression = Expression.Constant (0);
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlTable ();

      _stageMock
          .Expect (
              mock =>
              mock.PrepareFromExpression (
                  Arg<Expression>.Matches (e => e == groupJoinClause.JoinClause.InnerSequence),
                  Arg<ISqlPreparationContext>.Matches (c => c is SqlPreparationQueryModelVisitorContext)))
          .Return (preparedExpression);
      _stageMock.Expect (mock => mock.PrepareSqlTable (preparedExpression, typeof (Cook))).Return (preparedSqlTable);
      _stageMock.Replay ();

      var result = _context.GetContextMapping (new QuerySourceReferenceExpression(groupJoinClause));

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.Null);
      
      // TODO Review 2668: Assert that the join was added to the visitor (both SqlTable and WhereCondition)
      // TODO Review 2668: The returned value should be a SqlTableReferenceExpression pointing to the SqlTable
    }

    [Test]
    public void TryGetContextMapping_GroupJoinClause ()
    {
      var groupJoinClause = ExpressionHelper.CreateGroupJoinClause ();

      Expression result = _context.TryGetContextMappingFromHierarchy (new QuerySourceReferenceExpression(groupJoinClause));
      
      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage =
        "The expression 'Cook' could not be found in the list of processed expressions. Probably, the feature declaring 'Cook' isn't supported yet.")]
    public void GetContextMapping_Throws_WhenSourceNotAdded ()
    {
      _context.GetContextMapping (new QuerySourceReferenceExpression(_source));
    }

    [Test]
    public void TryGetContextMappingFromHierarchy_ReturnsNullWhenSourceNotAdded ()
    {
      Expression result = _context.TryGetContextMappingFromHierarchy (new QuerySourceReferenceExpression(_source));
      
      Assert.That (result, Is.Null);
    }
  }
}