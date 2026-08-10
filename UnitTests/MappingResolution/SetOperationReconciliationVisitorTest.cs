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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SetOperationReconciliationVisitorTest
  {
    private Mock<IMappingResolutionStage2> _stageMock;
    private Mock<ISetOperationReconciliationContext> _reconciliationContextMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage2> (MockBehavior.Strict);
      _reconciliationContextMock = new Mock<ISetOperationReconciliationContext> (MockBehavior.Strict);
    }

    [Test]
    public void ApplyReconciliation_SqlEntity_RequiringReconciliation_AppliesReconciliationViaStage ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _reconciliationContextMock
          .Setup (mock => mock.IsReconciliationRequired (entityExpression))
          .Returns (true)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ApplySetOperationReconciliationContext (entityExpression, _reconciliationContextMock.Object))
          .Returns (fakeResult)
          .Verifiable();

      var result = SetOperationReconciliationVisitor.ApplyReconciliation (entityExpression, _reconciliationContextMock.Object, _stageMock.Object);

      _reconciliationContextMock.Verify();
      _stageMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ApplyReconciliation_SqlEntity_NotRequiringReconciliation_ReturnsSameExpression ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _reconciliationContextMock
          .Setup (mock => mock.IsReconciliationRequired (entityExpression))
          .Returns (false)
          .Verifiable();

      var result = SetOperationReconciliationVisitor.ApplyReconciliation (entityExpression, _reconciliationContextMock.Object, _stageMock.Object);

      _reconciliationContextMock.Verify();
      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void ApplyReconciliation_SqlColumn_ReturnsSameExpression ()
    {
      var columnExpression = SqlStatementModelObjectMother.CreateSqlColumn();

      var result = SetOperationReconciliationVisitor.ApplyReconciliation (columnExpression, _reconciliationContextMock.Object, _stageMock.Object);

      Assert.That (result, Is.SameAs (columnExpression));
    }

    [Test]
    public void ApplyReconciliation_SqlEntityConstant_ReturnsSameExpression ()
    {
      var entityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), new Cook(), Expression.Constant (0));

      var result = SetOperationReconciliationVisitor.ApplyReconciliation (
          entityConstantExpression, _reconciliationContextMock.Object, _stageMock.Object);

      Assert.That (result, Is.SameAs (entityConstantExpression));
    }

    [Test]
    public void ApplyReconciliation_NonSqlSpecificExpression_ReturnsSameExpression ()
    {
      var expression = Expression.Constant (0);

      var result = SetOperationReconciliationVisitor.ApplyReconciliation (expression, _reconciliationContextMock.Object, _stageMock.Object);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
