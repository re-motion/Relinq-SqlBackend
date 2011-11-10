// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
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

      var querySource = ExpressionHelper.CreateMainFromClause_Cook();
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook)), JoinSemantics.Inner);

      _context.AddExpressionMapping (new QuerySourceReferenceExpression(querySource), new SqlTableReferenceExpression(_sqlTable));

      _querySourceReferenceExpression = new QuerySourceReferenceExpression (querySource);

      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), new UniqueIdentifierGenerator());
    }

    [Test]
    public void PrepareSelectExpression ()
    {
      var singleDataInfo = new StreamedSingleValueInfo (typeof (int), false);
      var selectProjection = Expression.Constant (0);
      var subStatement = new SqlStatement (singleDataInfo, selectProjection, new SqlTable[0], null, null, new Ordering[0], null, false, null, null);
      var expressionWithSubStatement = new SqlSubStatementExpression (subStatement);

      var result = _stage.PrepareSelectExpression (expressionWithSubStatement, _context);

      Assert.That (result, Is.SameAs(expressionWithSubStatement));
    }

    [Test]
    public void PrepareWhereExpression ()
    {
      var result = _stage.PrepareWhereExpression (_querySourceReferenceExpression, _context);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareTopExpression ()
    {
      var result = _stage.PrepareTopExpression (_querySourceReferenceExpression, _context);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void GetTableForFromExpression ()
    {
      var fromExpression = Expression.Constant (new Cook[0]);
      var result = _stage.PrepareFromExpression (fromExpression, _context, info=>new SqlTable(info, JoinSemantics.Inner));

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
    }

    [Test]
    public void PrepareSqlStatement ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();

      var result = _stage.PrepareSqlStatement (queryModel, _context);

      Assert.That (result, Is.Not.Null);
    }
  }
}