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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SetOperationResultOperatorHandlerBaseTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stageMock;
    private UnionResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _handler = new UnionResultOperatorHandler ();
      
      var selectProjection = ExpressionHelper.CreateExpression(typeof (int));
      _sqlStatementBuilder = new SqlStatementBuilder 
      {
        DataInfo = new StreamedSequenceInfo (typeof (int[]), selectProjection),
        SelectProjection = selectProjection
      };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new UnionResultOperator ("x", typeof (int), ExpressionHelper.CreateExpression (typeof (int[])));
      var preparedSource2Statement = SqlStatementModelObjectMother.CreateSqlStatement();
      var preparedSource2Expression = new SqlSubStatementExpression (preparedSource2Statement);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (resultOperator.Source2, _context))
          .Return(preparedSource2Expression);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      
      // Main functionality
      Assert.That (_sqlStatementBuilder.SetOperationCombinedStatements, Has.Count.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SetOperationCombinedStatements[0].SetOperation, Is.EqualTo(SetOperation.Union));
      Assert.That (_sqlStatementBuilder.SetOperationCombinedStatements[0].SqlStatement, Is.SameAs (preparedSource2Statement));

      // Data info
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof(IQueryable<>).MakeGenericType(typeof(int))));
      Assert.That (
          ((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).ItemExpression,
          Is.EqualTo (new QuerySourceReferenceExpression (resultOperator)));

      // Everyone referencing the result operator should reference the (outer) select projection instead.
      Assert.That (
          _context.GetExpressionMapping (new QuerySourceReferenceExpression (resultOperator)),
          Is.SameAs (_sqlStatementBuilder.SelectProjection));
    }

    [Test]
    public void HandleResultOperator_NonStatementAsSource2 ()
    {
      var resultOperator = new UnionResultOperator ("x", typeof (int), Expression.Constant (new[] { 1, 2, 3 }));
      var preparedSource2Expression = ExpressionHelper.CreateExpression (typeof (int[]));
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (resultOperator.Source2, _context))
          .Return(preparedSource2Expression);

      Assert.That(() =>_handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context),
          Throws.TypeOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The 'Union' operation is only supported for combining two query results, but a 'ConstantExpression' was supplied as the "
                  + "second sequence: value(System.Int32[])"));
    }

    [Test]
    public void HandleResultOperator_OrderingsWithoutTopInMainSqlStatement_ShouldBeRemoved ()
    {
      _sqlStatementBuilder.Orderings.Add (SqlStatementModelObjectMother.CreateOrdering());
      var originalSqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      _sqlStatementBuilder.SqlTables.Add (originalSqlTable);

      var stage = CreateDefaultSqlPreparationStage();

      var resultOperator = CreateValidResultOperator();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      Assert.That (_sqlStatementBuilder.Orderings, Is.Empty);
      Assert.That (_sqlStatementBuilder.SqlTables, Is.EqualTo (new[] { originalSqlTable }), "Query was not moved to a substatement.");
    }

    [Test]
    public void HandleResultOperator_OrderingsWithTopInMainSqlStatement_ShouldBeMovedToSubStatement_WithoutAffectingProjectionOrOuterOrderings ()
    {
      var originalOrdering = SqlStatementModelObjectMother.CreateOrdering();
      _sqlStatementBuilder.Orderings.Add (originalOrdering);
      _sqlStatementBuilder.TopExpression = ExpressionHelper.CreateExpression();
      var originalSelectProjection = _sqlStatementBuilder.SelectProjection;

      var resultOperator = CreateValidResultOperator();
      
      var stage = CreateDefaultSqlPreparationStage();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);

      // Statement was moved to substatement as is, without affecting select projection (apart from NamedExpression) 
      // and leaving the orderings in place.
      var subStatement = ((ResolvedSubStatementTableInfo) _sqlStatementBuilder.SqlTables[0].TableInfo).SqlStatement;
      Assert.That (((NamedExpression) subStatement.SelectProjection).Expression, Is.SameAs (originalSelectProjection));
      Assert.That (subStatement.Orderings, Is.EqualTo (new[] { originalOrdering }));

      // Outer statement has no orderings.
      Assert.That (_sqlStatementBuilder.Orderings, Is.Empty);
    }

     [Test]
    public void HandleResultOperator_OrderingsWithTopInSource2_CausesMainStatementToBeMovedToSubStatement ()
    {
       var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder 
       {
         Orderings = { SqlStatementModelObjectMother.CreateOrdering() },
         TopExpression = Expression.Constant(10)
       });
       var resultOperator = new UnionResultOperator ("x", typeof (int), new SqlSubStatementExpression (sqlStatement));
      
      var stage = CreateDefaultSqlPreparationStage();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    private static UnionResultOperator CreateValidResultOperator ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement(Expression.Constant(0));
      return new UnionResultOperator ("x", typeof (int), new SqlSubStatementExpression (sqlStatement));
    }
  }
}