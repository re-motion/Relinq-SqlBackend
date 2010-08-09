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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationSelectExpressionVisitorTest
  {
    private UniqueIdentifierGenerator _generator;
    private ISqlPreparationStage _stageMock;
    private ISqlPreparationContext _contextMock;
    private MethodCallTransformerRegistry _registry;
    private TestableSqlPreparationSelectExpressionVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _contextMock = MockRepository.GenerateMock<ISqlPreparationContext>();
      _registry = MethodCallTransformerRegistry.CreateDefault();
      _visitor = new TestableSqlPreparationSelectExpressionVisitor (_contextMock, _stageMock, _generator, _registry);
    }

    [Test]
    public void VisitSqlSubStatementExpression_StreamedSequenceInfo ()
    {
      var sqlStatement = new SqlStatement (
          new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook())),
          Expression.Constant ("select"),
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = _visitor.VisitSqlSubStatementExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_StreamedScalarValueInfo ()
    {
      var selectProjection = Expression.Constant (1);
      var sqlStatement = new SqlStatement (
          new StreamedScalarValueInfo (typeof (int)),
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = _visitor.VisitSqlSubStatementExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_StreamedSingleValueInfo ()
    {
      var selectProjection = Expression.Constant (1);
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var originalSubStatement = new SqlStatement (
          new StreamedSingleValueInfo (typeof(int), false),
          selectProjection,
          new[] { sqlTable},
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var expression = new SqlSubStatementExpression (originalSubStatement);

      var fakeSqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Left);

      SqlTable generatedSqlTable = null;
      _contextMock
        .Expect (mock => mock.AddSqlTable (Arg<SqlTable>.Is.Anything))
        .WhenCalled(mi=>generatedSqlTable=(SqlTable)mi.Arguments[0]);
      _contextMock.Replay ();

      var result = _visitor.VisitSqlSubStatementExpression (expression);

      _contextMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (generatedSqlTable));

      var expectedItemSelector = new SqlTableReferenceExpression (fakeSqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedItemSelector, new SqlTableReferenceExpression(fakeSqlTable));
    }
  }
}