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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
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

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _handler = new ContainsResultOperatorHandler (_generator, _stageMock);
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo(typeof(Cook[]), Expression.Constant(new Cook()))
      };
    }

    [Test]
    public void HandleResultOperator ()
    {
      var itemExpression = Expression.Constant (new Cook ());
      var resultOperator = new ContainsResultOperator (itemExpression);

      var preparedExpression = Expression.Constant (new Cook (), typeof (Cook));
      _stageMock.Expect (mock => mock.PrepareItemExpression (itemExpression)).Return (preparedExpression);
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder);

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
  }
}