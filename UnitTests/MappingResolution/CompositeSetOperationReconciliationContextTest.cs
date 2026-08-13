// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class CompositeSetOperationReconciliationContextTest
  {
    private SqlEntityExpression _entity1;
    private SqlEntityExpression _entity2;
    private SqlEntityExpression _entity3;

    [SetUp]
    public void SetUp ()
    {
      _entity1 = CreateFakeEntityExpression();
      _entity2 = CreateFakeEntityExpression();
      _entity3 = CreateFakeEntityExpression();
    }

    [Test]
    public void IsReconciliationRequired_NoInnerContexts_ReturnsFalse ()
    {
      var context = new CompositeSetOperationReconciliationContext(Array.Empty<ISetOperationReconciliationContext>());

      var result = context.IsReconciliationRequired(_entity1);

      Assert.That(result, Is.False);
    }

    [Test]
    public void GetReconciledColumns_NoInnerContexts_ThrowsInvalidOperationException ()
    {
      var context = new CompositeSetOperationReconciliationContext(Array.Empty<ISetOperationReconciliationContext>());

      Assert.That(
          () => context.GetReconciledColumns(_entity1),
          Throws.InvalidOperationException);
    }

    [Test]
    public void IsReconciliationRequired_InnerContextHandlesEntity_ReturnsTrue ()
    {
      var innerContextStub = CreateInnerContextStub(_entity1);
      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub });

      var result = context.IsReconciliationRequired(_entity1);

      Assert.That(result, Is.True);
    }

    [Test]
    public void IsReconciliationRequired_InnerContextDoesNotHandleEntity_ReturnsFalse ()
    {
      var innerContextStub = CreateInnerContextStub(_entity1);
      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub });

      var result = context.IsReconciliationRequired(_entity2);

      Assert.That(result, Is.False);
    }

    [Test]
    public void GetReconciledColumns_InnerContextHandlesEntity_ReturnsInnerContextsColumns ()
    {
      var fakeColumns = new[] { CreateFakeColumn("Column1") };
      var innerContextStub = CreateInnerContextStub(_entity1, fakeColumns);
      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub });

      var result = context.GetReconciledColumns(_entity1);

      Assert.That(result, Is.SameAs(fakeColumns));
    }

    [Test]
    public void GetReconciledColumns_InnerContextDoesNotHandleEntity_ThrowsInvalidOperationException ()
    {
      var innerContextStub = CreateInnerContextStub(_entity1);
      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub });

      Assert.That(
          () => context.GetReconciledColumns(_entity2),
          Throws.InvalidOperationException);
    }

    [Test]
    public void IsReconciliationRequired_MultipleInnerContexts_DelegatesToTheContextResponsibleForTheEntity ()
    {
      var innerContextStub1 = CreateInnerContextStub(_entity1);
      var innerContextStub2 = CreateInnerContextStub(_entity2);

      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub1, innerContextStub2 });

      Assert.That(context.IsReconciliationRequired(_entity1), Is.True);
      Assert.That(context.IsReconciliationRequired(_entity2), Is.True);
      Assert.That(context.IsReconciliationRequired(_entity3), Is.False);
    }

    [Test]
    public void GetReconciledColumns_MultipleInnerContexts_DelegatesToTheContextResponsibleForTheEntity ()
    {
      var columns1 = new[] { CreateFakeColumn("Column1") };
      var columns2 = new[] { CreateFakeColumn("Column2") };
      var innerContextStub1 = CreateInnerContextStub(_entity1, columns1);
      var innerContextStub2 = CreateInnerContextStub(_entity2, columns2);

      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub1, innerContextStub2 });

      Assert.That(context.GetReconciledColumns(_entity1), Is.SameAs(columns1));
      Assert.That(context.GetReconciledColumns(_entity2), Is.SameAs(columns2));
    }

    [Test]
    public void IsReconciliationRequired_MultipleInnerContextsHandleSameEntity_UsesFirstMatchingContext ()
    {
      var innerContextStub1 = CreateInnerContextStub(_entity1);
      var innerContextStub2 = CreateInnerContextStub(_entity1);

      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub1, innerContextStub2 });

      var result = context.IsReconciliationRequired(_entity1);

      Assert.That(result, Is.True);
      Mock.Get(innerContextStub2).Verify(c => c.IsReconciliationRequired(It.IsAny<SqlEntityExpression>()), Times.Never());
    }

    [Test]
    public void GetReconciledColumns_MultipleInnerContextsHandleSameEntity_UsesFirstMatchingContext ()
    {
      var columns1 = new[] { CreateFakeColumn("Column1") };
      var columns2 = new[] { CreateFakeColumn("Column2") };
      var innerContextStub1 = CreateInnerContextStub(_entity1, columns1);
      var innerContextStub2 = CreateInnerContextStub(_entity1, columns2);

      var context = new CompositeSetOperationReconciliationContext(new[] { innerContextStub1, innerContextStub2 });

      var result = context.GetReconciledColumns(_entity1);

      Assert.That(result, Is.SameAs(columns1));
    }

    private ISetOperationReconciliationContext CreateInnerContextStub (SqlEntityExpression handledEntity, SqlColumnExpression[] columns = null)
    {
      var innerContextStub = new Mock<ISetOperationReconciliationContext>();
      innerContextStub.Setup(stub => stub.IsReconciliationRequired(handledEntity)).Returns(true);
      innerContextStub.Setup(stub => stub.GetReconciledColumns(handledEntity)).Returns(columns ?? Array.Empty<SqlColumnExpression>());

      return innerContextStub.Object;
    }

    private SqlEntityDefinitionExpression CreateFakeEntityExpression ()
    {
      var starColumn = new SqlColumnDefinitionExpression(typeof(object), "o", "*", false);
      return new SqlEntityDefinitionExpression(typeof(object), "o", null, e => e.GetColumn(typeof(object), "ID", true), starColumn);
    }

    private SqlColumnExpression CreateFakeColumn (string columnName)
    {
      return new SqlColumnDefinitionExpression(typeof(int), "o", columnName, false);
    }
  }
}
