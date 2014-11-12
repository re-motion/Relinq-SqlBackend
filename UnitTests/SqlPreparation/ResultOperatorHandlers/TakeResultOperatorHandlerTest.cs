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
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class TakeResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stage;
    private TakeResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stage = CreateDefaultSqlPreparationStage();
      _handler = new TakeResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator_ConstantExpression ()
    {
      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (((SqlLiteralExpression) _sqlStatementBuilder.TopExpression).Value, Is.EqualTo (2));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (Cook))));
    }

    [Test]
    public void HandleResultOperator_NoConstantExpression ()
    {
      var takeExpression = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That ( _sqlStatementBuilder.TopExpression, Is.SameAs (takeExpression));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (Cook))));
    }

    [Test]
    public void HandleResultOperator_TakeAfterTopExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");

      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_TakeAfterGroupExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("group");

      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_TakeAfterSetOperation ()
    {
      _sqlStatementBuilder.SetOperationCombinedStatements.Add(SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());

      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_TakeAfterSkipExpression ()
    {
      _sqlStatementBuilder.WhereCondition = null;
      _sqlStatementBuilder.RowNumberSelector = Expression.Constant (5);
      _sqlStatementBuilder.CurrentRowNumberOffset = Expression.Constant (3);

      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      var expectedWhereCondition = Expression.LessThanOrEqual (
          _sqlStatementBuilder.RowNumberSelector, Expression.Add (_sqlStatementBuilder.CurrentRowNumberOffset, resultOperator.Count));

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedWhereCondition, _sqlStatementBuilder.WhereCondition);
    }

  }
}