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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class SqlEntityRefMemberExpressionTest
  {
    private PropertyInfo _memberInfo;
    private SqlEntityRefMemberExpression _expression;
    private SqlEntityExpression _entityExpression;

    [SetUp]
    public void SetUp ()
    {
      _memberInfo = typeof (Cook).GetProperty ("Substitution");
      _entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "c");
      _expression = new SqlEntityRefMemberExpression (_entityExpression, _memberInfo);
    }

    [Test]
    public void Initialization_TypeInferredFromMemberType ()
    {
      Assert.That (_expression.Type, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void VisitChildren_ReturnsThis_WithoutCallingVisitMethods ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock.Object);

      Assert.That (result, Is.SameAs (_expression));
      visitorMock.Verify();
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlEntityRefMemberExpression, ISqlEntityRefMemberExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlEntityRefMember (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public new void ToString ()
    {
      var result = _expression.ToString();

      Assert.That (result, Is.EqualTo ("[c].[Substitution]"));
    }
  }
}