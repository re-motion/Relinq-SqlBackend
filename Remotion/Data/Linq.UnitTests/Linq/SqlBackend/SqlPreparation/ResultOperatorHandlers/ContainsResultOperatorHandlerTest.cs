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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class ContainsResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private ContainsResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private QueryModel _queryModel;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _handler = new ContainsResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo(typeof(Cook[]), Expression.Constant(new Cook()))
      };
      _queryModel = new QueryModel (ExpressionHelper.CreateMainFromClause_Cook(), ExpressionHelper.CreateSelectClause ());
      _context = new SqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator_TopLevel ()
    {
      var itemExpression = Expression.Constant (new Cook ());
      var resultOperator = new ContainsResultOperator (itemExpression);

      var preparedExpression = Expression.Constant (new Cook (), typeof (Cook));
      _stageMock.Expect (mock => mock.PrepareItemExpression (itemExpression)).Return (preparedExpression);
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _queryModel, _sqlStatementBuilder, _generator, _stageMock);

      Assert.That (_sqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (SqlBinaryOperatorExpression)));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Boolean)));

      var binaryOperatorExpression = (SqlBinaryOperatorExpression) _sqlStatementBuilder.SelectProjection;
      Assert.That (binaryOperatorExpression.LeftExpression, Is.SameAs (preparedExpression));
      Assert.That (binaryOperatorExpression.RightExpression, Is.TypeOf (typeof (SqlSubStatementExpression)));

      var subStatementExpression = ((SqlSubStatementExpression) binaryOperatorExpression.RightExpression);
      Assert.That (subStatementExpression.SqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) subStatementExpression.SqlStatement.DataInfo).DataType, Is.EqualTo (typeof (Cook[])));

      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSubQueryExpression_WithContainsAndConstantCollection ()
    {
      var constantExpressionCollection = Expression.Constant (new[] { new Cook(), new Cook()});
      var mainFromClause = new MainFromClause ("generated", typeof (Cook), constantExpressionCollection);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);

      var itemExpression = Expression.Constant (new Cook());
      var containsResultOperator = new ContainsResultOperator (itemExpression);
      queryModel.ResultOperators.Add (containsResultOperator);
      var fakeConstantExpression = Expression.Constant (new Cook());

      _stageMock
          .Expect (mock => mock.PrepareItemExpression (itemExpression))
          .Return (fakeConstantExpression);
      _stageMock.Replay ();

      _handler.HandleResultOperator (containsResultOperator, queryModel, _sqlStatementBuilder, _generator, _stageMock);

      Assert.That (_sqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (SqlBinaryOperatorExpression)));
      Assert.That (((SqlBinaryOperatorExpression) _sqlStatementBuilder.SelectProjection).BinaryOperator, Is.EqualTo ("IN"));
      Assert.That (((SqlBinaryOperatorExpression) _sqlStatementBuilder.SelectProjection).LeftExpression, Is.EqualTo (fakeConstantExpression));
      Assert.That (((SqlBinaryOperatorExpression) _sqlStatementBuilder.SelectProjection).RightExpression, Is.EqualTo (constantExpressionCollection));

      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitSubQueryExpression_WithContainsAndEmptyConstantCollection ()
    {
      var constantExpressionCollection = Expression.Constant (new Cook[] { });
      var mainFromClause = new MainFromClause ("generated", typeof (Cook), constantExpressionCollection);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);

      var itemExpression = Expression.Constant (new Cook());
      var containsResultOperator = new ContainsResultOperator (itemExpression);
      queryModel.ResultOperators.Add (containsResultOperator);

      _handler.HandleResultOperator (containsResultOperator, queryModel, _sqlStatementBuilder, _generator, _stageMock);
     
      Assert.That (_sqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) _sqlStatementBuilder.SelectProjection).Value, Is.EqualTo (false));

      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSubQueryExpression_WithSeveralResultOperatorsAndConstantCollection ()
    {
      var constantExpressionCollection = Expression.Constant (new[] { "Huber", "Maier" });
      var mainFromClause = new MainFromClause ("generated", typeof (string), constantExpressionCollection);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);

      var itemExpression = Expression.Constant ("Huber");
      var containsResultOperator = new ContainsResultOperator (itemExpression);

      var resultOperator = new TakeResultOperator (Expression.Constant (1));
      queryModel.ResultOperators.Add (resultOperator);
      queryModel.ResultOperators.Add (containsResultOperator);

      _handler.HandleResultOperator (containsResultOperator, queryModel, _sqlStatementBuilder, _generator, _stageMock);
    }

  }
}