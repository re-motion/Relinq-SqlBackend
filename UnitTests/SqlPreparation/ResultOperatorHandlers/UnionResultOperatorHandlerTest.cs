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
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class UnionResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private UnionResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _generator = new UniqueIdentifierGenerator ();
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

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

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

      Assert.That(() =>_handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context),
          Throws.TypeOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The Union result operator is only supported for combining two query results, but a 'ConstantExpression' was supplied as the "
                  + "second sequence: value(System.Int32[])"));
    }
  }
}