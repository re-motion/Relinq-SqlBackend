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
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class NullJoinConditionExpressionTest
  {
    private NullJoinConditionExpression _nullJoinConditionExpression;

    [SetUp]
    public void SetUp ()
    {
      _nullJoinConditionExpression = new NullJoinConditionExpression ();
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (_nullJoinConditionExpression.NodeType, Is.EqualTo (ExpressionType.Extension));
    }

    [Test]
    public void Type ()
    {
      Assert.That (_nullJoinConditionExpression.Type, Is.EqualTo (typeof (bool)));
    }

    [Test]
    public void VisitChildren_ReturnsThis_WithoutCallingVisitMethods ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_nullJoinConditionExpression, visitorMock.Object);

      Assert.That (result, Is.SameAs (_nullJoinConditionExpression));
      visitorMock.Verify();
    }

    [Test]
    public void Accept_VisitorNeverSupportsThisType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_nullJoinConditionExpression);
    }

    [Test]
    public void GetResolvedTableInfo ()
    {
      Assert.That (
          () => UnresolvedDummyRowTableInfo.Instance.GetResolvedTableInfo(),
          Throws.InvalidOperationException.With.Message.EqualTo ("This table has not yet been resolved; call the resolution step first."));
    }

    [Test]
    public void To_String ()
    {
      var result = _nullJoinConditionExpression.ToString();

      Assert.That (result, Is.EqualTo ("NullJoinCondition()"));
    } 
  }
}