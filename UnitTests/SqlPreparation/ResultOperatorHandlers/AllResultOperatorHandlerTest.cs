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
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class AllResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stageMock;
    private AllResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _handler = new AllResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var predicate = Expression.Constant (true);
      var preparedPredicate = Expression.Constant (false);
      var resultOperator = new AllResultOperator(predicate);
      var sqlStatement = _sqlStatementBuilder.GetSqlStatement();

      var fakePreparedSelectProjection = Expression.Constant (false);

      _stageMock
          .Expect (mock => mock.PrepareWhereExpression (
              Arg<Expression>.Matches(e => e.NodeType == ExpressionType.Not && (((UnaryExpression) e).Operand == predicate)), 
              Arg<ISqlPreparationContext>.Matches(c=>c==_context)))
          .Return (preparedPredicate);
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Matches (e => e.NodeType == ExpressionType.Not), Arg.Is (_context)))
          .WhenCalled (
              mi =>
              {
                var selectProjection = (Expression) mi.Arguments[0];
                var expectedSubStatement = new SqlStatementBuilder (sqlStatement) { WhereCondition = preparedPredicate }.GetSqlStatement();
                var expectedExistsExpression = new SqlExistsExpression (new SqlSubStatementExpression (expectedSubStatement));
                var expectedExpression = Expression.Not (expectedExistsExpression);

                SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, selectProjection);
              })
          .Return (fakePreparedSelectProjection);
      _stageMock.Replay();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();

      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Boolean)));

      Assert.That (_sqlStatementBuilder.SelectProjection, Is.SameAs (fakePreparedSelectProjection));
    }

    [Test]
    public void HandleResultOperator_AllAfterGroupExpression ()
    {
      _sqlStatementBuilder.GroupByExpression = Expression.Constant ("group");

      var stage = CreateDefaultSqlPreparationStage();
      
      var predicate = Expression.Constant (true);
      var resultOperator = new AllResultOperator (predicate);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      var innerStatementOfExistsExpression = GetInnerStatementAfterExistsTransformation(_sqlStatementBuilder);
      AssertStatementWasMovedToSubStatement (innerStatementOfExistsExpression);
    }

    [Test]
    public void HandleResultOperator_AllAfterSetOperation ()
    {
      _sqlStatementBuilder.SetOperationCombinedStatements.Add(SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());

      var stage = CreateDefaultSqlPreparationStage();
      
      var predicate = Expression.Constant (true);
      var resultOperator = new AllResultOperator (predicate);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      var innerStatementOfExistsExpression = GetInnerStatementAfterExistsTransformation(_sqlStatementBuilder);
      AssertStatementWasMovedToSubStatement (innerStatementOfExistsExpression);
    }

    private SqlStatement GetInnerStatementAfterExistsTransformation (SqlStatementBuilder sqlStatementBuilder)
    {
      var notExpression = (UnaryExpression) sqlStatementBuilder.SelectProjection;
      var existsExpression = (SqlExistsExpression) notExpression.Operand;
      var innerStatementOfExistsExpression = ((SqlSubStatementExpression) existsExpression.Expression).SqlStatement;
      return innerStatementOfExistsExpression;
    }
  }
}