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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class NamedExpressionTest
  {
    private NamedExpression _namedExpression;
    private ConstantExpression _wrappedExpression;

    [SetUp]
    public void SetUp ()
    {
      _wrappedExpression = Expression.Constant (1);
      _namedExpression = new NamedExpression ("test", _wrappedExpression);
    }

    [Test]
    public void CreateFromMemberName ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Name");
      var innerExpression = Expression.Constant ("inner");

      var result = NamedExpression.CreateFromMemberName (memberInfo.Name, innerExpression);

      Assert.That (result.Name, Is.SameAs (memberInfo.Name));
      Assert.That (result.Expression, Is.SameAs (innerExpression));
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_namedExpression.Type, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      visitorMock
          .Setup (mock => mock.Visit (_wrappedExpression))
          .Returns (_wrappedExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_namedExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (_namedExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewSqlInExpression ()
    {
      var newExpression = Expression.Constant (5);
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      visitorMock
          .Setup (mock => mock.Visit (_wrappedExpression))
          .Returns (newExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_namedExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_namedExpression));
      Assert.That (((NamedExpression) result).Expression, Is.SameAs (newExpression));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("test"));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<NamedExpression, INamedExpressionVisitor> (
          _namedExpression,
          mock => mock.VisitNamed (_namedExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_namedExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _namedExpression.ToString();

      Assert.That (result, Is.EqualTo ("1 AS test"));
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_WithMembers ()
    {
      var innerExpression = Expression.Constant (0);
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A");
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { innerExpression },
          memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { new NamedExpression ("A", innerExpression) }, memberInfo);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_WithGetter ()
    {
      var innerExpression = Expression.Constant (0);
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A").GetGetMethod();
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { innerExpression },
          memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { new NamedExpression ("A", innerExpression) }, memberInfo);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_WithPropertyCalledGet ()
    {
      var innerExpression = Expression.Constant (0);
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("get_");
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { innerExpression },
          memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { new NamedExpression ("get_", innerExpression) }, memberInfo);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedCorrectly ()
    {
      var innerExpression = new NamedExpression("m0", Expression.Constant (0));
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { innerExpression });

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedWithDifferentName ()
    {
      var innerExpression = new NamedExpression ("test", Expression.Constant (0));
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { innerExpression });

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      var expectedResult = Expression.New (
           TypeForNewExpression.GetConstructor (typeof (int)), new[] { new NamedExpression ("m0", innerExpression) });

      Assert.That (result, Is.Not.SameAs (expression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedCorrectly_WithMembers ()
    {
      var innerExpression = new NamedExpression ("A", Expression.Constant (0));
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A");
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { innerExpression }, memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedCorrectly_WithGetters ()
    {
      var innerExpression = new NamedExpression ("A", Expression.Constant (0));
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A").GetGetMethod();
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { innerExpression }, memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_DedicatedArguments ()
    {
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A");
      var originalArgument = ExpressionHelper.CreateExpression (typeof (int));
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { originalArgument }, memberInfo);

      var processedArgument = ExpressionHelper.CreateExpression (typeof (int));

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression, new[] { processedArgument });

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { new NamedExpression ("A", processedArgument) }, memberInfo);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
    
  }

  internal class MemberTest
  {
    public string get_A { get; set; }
    public string get_ { get; set; }
  }
}