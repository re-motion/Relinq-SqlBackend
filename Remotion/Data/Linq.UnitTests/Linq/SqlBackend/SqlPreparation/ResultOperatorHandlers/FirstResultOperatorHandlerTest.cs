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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class FirstResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private FirstResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _generator = new UniqueIdentifierGenerator ();
      _handler = new FirstResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      };
      _queryModel = new QueryModel (ExpressionHelper.CreateMainFromClause_Cook (), ExpressionHelper.CreateSelectClause ());
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new FirstResultOperator (false);

      var preparedExpression = Expression.Constant (null, typeof (Cook));
      _stageMock
         .Expect (
         mock => mock.PrepareTopExpression (
                     Arg<Expression>.Matches (expr => expr is ConstantExpression && ((ConstantExpression) expr).Value.Equals (1))))
         .Return (preparedExpression);
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _queryModel, _sqlStatementBuilder, _generator, _stageMock);

      Assert.That (_sqlStatementBuilder.TopExpression, Is.SameAs (preparedExpression));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Cook)));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void HandleResultOperator_FirstAfterTopExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");

      var resultOperator = new FirstResultOperator (false);

      _handler.HandleResultOperator (resultOperator, _queryModel, _sqlStatementBuilder, _generator, _stageMock);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

  }
}