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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationContextTest
  {
    private ISqlPreparationContext _context;
    private MainFromClause _source;
    private SqlTable _sqlTable;
    private ISqlPreparationContext _parentContext;
    private MainFromClause _parentSource;
    private SqlTable _parentSqlTable;
    private ISqlPreparationContext _contextWithParent;
    private SqlStatementBuilder _sqlStatementBuilder;

    [SetUp]
    public void SetUp ()
    {
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();
      _source = ExpressionHelper.CreateMainFromClause<Cook>();
      var source = new UnresolvedTableInfo (typeof (int));
      _sqlTable = new SqlTable (source);
      _parentContext = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
      _parentSource = ExpressionHelper.CreateMainFromClause<Cook>();
      _parentSqlTable = new SqlTable (new UnresolvedTableInfo (typeof (int)));
      _sqlStatementBuilder = new SqlStatementBuilder();
      _contextWithParent = new SqlPreparationContext (_parentContext, _sqlStatementBuilder);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.IsOuterMostQuery, Is.True);
      Assert.That (_contextWithParent.IsOuterMostQuery, Is.False);
    }

    [Test]
    public void AddExpressionMapping ()
    {
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (_source), new SqlTableReferenceExpression (_sqlTable));
      Assert.That (_context.GetExpressionMapping (new QuerySourceReferenceExpression (_source)), Is.Not.Null);
    }

    [Test]
    public void GetExpressionMapping ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_source);
      _context.AddExpressionMapping (querySourceReferenceExpression, new SqlTableReferenceExpression (_sqlTable));
      Assert.That (
          ((SqlTableReferenceExpression) _context.GetExpressionMapping (querySourceReferenceExpression)).SqlTable,
          Is.SameAs (_sqlTable));
    }

    [Test]
    public void TryGetExpressiontMappingFromHierarchy ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_source);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      _context.AddExpressionMapping (querySourceReferenceExpression, sqlTableReferenceExpression);

      Expression result = _context.GetExpressionMapping (querySourceReferenceExpression);

      Assert.That (result, Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void GetExpressionMapping_GetFromParentContext ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_parentSource);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (_parentSqlTable);
      _parentContext.AddExpressionMapping (querySourceReferenceExpression, sqlTableReferenceExpression);
      Assert.That (_contextWithParent.GetExpressionMapping (querySourceReferenceExpression), Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void TryGetExpressionMappingFromHierarchy_GetFromParentContext ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_parentSource);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (_parentSqlTable);
      _parentContext.AddExpressionMapping (querySourceReferenceExpression, sqlTableReferenceExpression);

      Expression result = _contextWithParent.GetExpressionMapping (querySourceReferenceExpression);

      Assert.That (result, Is.SameAs (sqlTableReferenceExpression));
    }

    [Test]
    public void TryGetExpressionMappingFromHierarchy_ReturnsNullWhenSourceNotAdded ()
    {
      Expression result = _context.GetExpressionMapping (new QuerySourceReferenceExpression (_source));

      Assert.That (result, Is.Null);
    }

    [Test]
    public void AddSqlTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlAppendedTable ();
      
      _contextWithParent.AddSqlTable (sqlTable);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.SameAs (sqlTable));
    }
    
  }
}