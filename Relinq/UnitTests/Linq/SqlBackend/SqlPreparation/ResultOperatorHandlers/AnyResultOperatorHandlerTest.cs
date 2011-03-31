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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class AnyResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private AnyResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
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
         .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Matches (e => e is SqlExistsExpression), Arg.Is (_context)))
         .WhenCalled (
             mi =>
             {
               var selectProjection = (Expression) mi.Arguments[0];
               Assert.That (selectProjection, Is.TypeOf (typeof (SqlExistsExpression)));

               var expectedExistsExpression = new SqlExistsExpression (new SqlSubStatementExpression (sqlStatement));
               ExpressionTreeComparer.CheckAreEqualTrees (expectedExistsExpression, selectProjection);
             })
         .Return (fakePreparedSelectProjection);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Boolean)));

      _stageMock.VerifyAllExpectations ();

      Assert.That (_sqlStatementBuilder.SelectProjection, Is.SameAs (fakePreparedSelectProjection));
    }
  }
}