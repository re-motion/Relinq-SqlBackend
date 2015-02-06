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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class AggregationResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stage;
    private TestableAggregationResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stage = CreateDefaultSqlPreparationStage();
      _handler = new TestableAggregationResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (5)),
        SelectProjection = new NamedExpression (null, Expression.Constant (0))
      };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var averageResultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (averageResultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (((AggregationExpression) _sqlStatementBuilder.SelectProjection).AggregationModifier, Is.EqualTo (AggregationModifier.Max));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void HandleResultOperator_WithOrderingsWithoutTopExpression ()
    {
      _sqlStatementBuilder.Orderings.Add (new Ordering (Expression.Constant ("order"), OrderingDirection.Asc));
      var averageResultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (averageResultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (_sqlStatementBuilder.Orderings.Count, Is.EqualTo (0));
    }

    [Test]
    public void HandleResultOperator_WithOrderingsAndTopExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Asc);
      _sqlStatementBuilder.Orderings.Add (ordering);
      var averageResultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (averageResultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (_sqlStatementBuilder.Orderings.Count, Is.EqualTo (0));
      Assert.That (
          ((ResolvedSubStatementTableInfo) _sqlStatementBuilder.SqlTables[0].SqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (1));
      Assert.That (
          ((ResolvedSubStatementTableInfo) _sqlStatementBuilder.SqlTables[0].SqlTable.TableInfo).SqlStatement.Orderings[0], Is.SameAs(ordering));
    }

    [Test]
    public void HandleResultOperator_AfterTopExpression_CreatesSubstatement ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");

      var resultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_AfterGroupExpression_CreatesSubStatement ()
    {
      _sqlStatementBuilder.GroupByExpression = Expression.Constant ("group");

      var resultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_AfterDistinctExpression_CreatesSubStatement ()
    {
      _sqlStatementBuilder.IsDistinctQuery = true;

      var resultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_AfterSetOperation_CreatesSubStatement ()
    {
      _sqlStatementBuilder.SetOperationCombinedStatements.Add (SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());

      var resultOperator = new MaxResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }
  }
}