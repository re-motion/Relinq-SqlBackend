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
using System.Collections;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class ConstantCollectionExpressionTest
  {
    [Test]
    public void Initialize ()
    {
      var collection = new[] { 4, 2 };

      var result = new ConstantCollectionExpression (collection);

      Assert.That (result.Collection, Is.EqualTo (collection));
      Assert.That (result.IsEmptyCollection, Is.False);
    }

    [Test]
    public void Initialize_WithConstantExpression_ForEmptyCollection_IsEmpty ()
    {
      var collection = new string[0];

      var result = new ConstantCollectionExpression (collection);

      Assert.That (result.Collection, Is.EqualTo (collection));
      Assert.That (result.IsEmptyCollection, Is.True);
    }

    private static IEnumerable<IEnumerable> GetCollectionTestCasesForGetItems ()
    {
      var emptyCollectionStub = new Mock<ICollection>();
      emptyCollectionStub.Setup (_ => _.GetEnumerator()).Returns (Mock.Of<IEnumerator>());
      yield return emptyCollectionStub.Object;

      yield return new[] { 17, 4, 42 };
      yield return new[] { "One", "Two", "Three" };
      yield return new[] { "One".ToCharArray(), "Two".ToCharArray(), "Three".ToCharArray() };
    }

    [Test]
    [TestCaseSource(nameof(GetCollectionTestCasesForGetItems))]
    public void GetItems (IEnumerable collection)
    {
      var constantCollectionExpression = new ConstantCollectionExpression (collection);
      var result = constantCollectionExpression.GetItems();
      Assert.That (result, Is.EqualTo (collection));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      var collection = new[] { 4, 2, 42 };
      var constantCollectionExpression = new ConstantCollectionExpression (collection);
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<ConstantCollectionExpression, IConstantCollectionExpressionVisitor> (
          constantCollectionExpression,
          mock => mock.VisitConstantCollection (constantCollectionExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      var collection = new[] { 4, 2, 42 };
      var constantCollectionExpression = new ConstantCollectionExpression (collection);
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (constantCollectionExpression);
    }

    [CLSCompliant (false)]
    [Test]
    [TestCase (new object[0], "[]")]
    [TestCase (new object[] { 1 }, "[1]")]
    [TestCase (new object[] { 1, "two" }, "[1, two]")]
    [TestCase (new object[] { 4, 2, 42, 17, 4 }, "[4, 2, 42, 17, 4]")]
    [TestCase (new object[] { 4, 2, "text", 42, 17, 4 }, "[4, 2, text, 42, 17, ...]")]
    public void ToString (object[] items, string expectedResult)
    {
      var expression = new ConstantCollectionExpression (items);
      Assert.That (expression.ToString(), Is.EqualTo (expectedResult));
    }
  }
}
