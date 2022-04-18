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
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class AnyResultOperatorHandlerTest
  {
    private Mock<ISqlPreparationStage> _stageMock;
    private UniqueIdentifierGenerator _generator;
    private AnyResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _handler = new AnyResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                             };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new AnyResultOperator ();
      var sqlStatement = _sqlStatementBuilder.GetSqlStatement();

      var fakePreparedSelectProjection = Expression.Constant (false);

      _stageMock
         .Setup (mock => mock.PrepareSelectExpression (It.Is<Expression> (e => e is SqlExistsExpression), _context))
         .Callback (
             (Expression expression, ISqlPreparationContext context) =>
             {
               Assert.That (expression, Is.TypeOf (typeof (SqlExistsExpression)));

               var expectedExistsExpression = new SqlExistsExpression (new SqlSubStatementExpression (sqlStatement));
               SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExistsExpression, expression);
             })
         .Returns (fakePreparedSelectProjection)
         .Verifiable();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock.Object, _context);

      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Boolean)));

      _stageMock.Verify();

      Assert.That (_sqlStatementBuilder.SelectProjection, Is.SameAs (fakePreparedSelectProjection));
    }
  }
}