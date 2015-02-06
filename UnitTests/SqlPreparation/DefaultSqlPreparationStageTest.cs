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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.UnitTests.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class DefaultSqlPreparationStageTest
  {
    private ISqlPreparationContext _context;
    private SqlTable _sqlTable;
    private QuerySourceReferenceExpression _querySourceReferenceExpression;
    private DefaultSqlPreparationStage _stage;

    [SetUp]
    public void SetUp ()
    {
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();

      var querySource = ExpressionHelper.CreateMainFromClause<Cook>();
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook)), JoinSemantics.Inner);

      _context.AddExpressionMapping (new QuerySourceReferenceExpression(querySource), new SqlTableReferenceExpression(_sqlTable));

      _querySourceReferenceExpression = new QuerySourceReferenceExpression (querySource);

      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), new UniqueIdentifierGenerator());
    }

    [Test]
    public void PrepareSelectExpression ()
    {
      var expression = Expression.Constant(0);

      var result = _stage.PrepareSelectExpression (expression, _context);

      Assert.That (result, Is.SameAs(expression));
    }

    [Test]
    public void PrepareWhereExpression ()
    {
      var result = _stage.PrepareWhereExpression (_querySourceReferenceExpression, _context);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareTopExpression ()
    {
      var result = _stage.PrepareTopExpression (_querySourceReferenceExpression, _context);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void GetTableForFromExpression ()
    {
      var fromExpression = Expression.Constant (new Cook[0]);
      var result = _stage.PrepareFromExpression (
          fromExpression,
          _context,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      Assert.That (result.AppendedTable, Is.Not.Null);
    }

    [Test]
    public void PrepareSqlStatement ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();

      var result = _stage.PrepareSqlStatement (queryModel, _context);

      Assert.That (result, Is.Not.Null);
    }
  }
}