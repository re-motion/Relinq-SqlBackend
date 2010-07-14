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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
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
    public void StripSurroundingNames_Unnamed ()
    {
      var result = NamedExpression.StripSurroundingNames (_wrappedExpression);
      Assert.That (result, Is.SameAs (_wrappedExpression));
    }

    [Test]
    public void StripSurroundingNames_Named ()
    {
      var result = NamedExpression.StripSurroundingNames (_namedExpression);
      Assert.That (result, Is.SameAs (_wrappedExpression));
    }

    [Test]
    public void StripSurroundingNames_DoubleNamed ()
    {
      var result = NamedExpression.StripSurroundingNames (new NamedExpression ("outer", _namedExpression));
      Assert.That (result, Is.SameAs (_wrappedExpression));
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
    public void CreateFromMemberName_NameStartsWith_get_ ()
    {
      var memberInfo = typeof (MemberTest).GetProperty ("get_A");
      var innerExpression = Expression.Constant ("inner");

      var result = NamedExpression.CreateFromMemberName (memberInfo.Name, innerExpression);

      Assert.That (result.Name, Is.EqualTo ("A"));
      Assert.That (result.Expression, Is.SameAs (innerExpression));
    }

    [Test]
    public void CreateFromMemberName_NameIs_get_ ()
    {
      var memberInfo = typeof (MemberTest).GetProperty ("get_");
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
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();

      visitorMock
          .Expect (mock => mock.VisitExpression (_wrappedExpression))
          .Return (_wrappedExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_namedExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_namedExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewSqlInExpression ()
    {
      var newExpression = Expression.Constant (5);
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();

      visitorMock
          .Expect (mock => mock.VisitExpression (_wrappedExpression))
          .Return (newExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_namedExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (_namedExpression));
      Assert.That (((NamedExpression) result).Expression, Is.SameAs (newExpression));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("test"));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<NamedExpression, INamedExpressionVisitor> (
          _namedExpression,
          mock => mock.VisitNamedExpression (_namedExpression));
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
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { innerExpression },
          memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments);

      var expectedResult = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { new NamedExpression ("A", innerExpression) }, memberInfo);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedCorrectly ()
    {
      var innerExpression = new NamedExpression("m0", Expression.Constant (0));
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { innerExpression });

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedWithDifferentName ()
    {
      var innerExpression = new NamedExpression ("test", Expression.Constant (0));
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { innerExpression });

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments);

      var expectedResult = Expression.New (
           typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { new NamedExpression ("m0", innerExpression) });

      Assert.That (result, Is.Not.SameAs (expression));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateNewExpressionWithNamedArguments_ArgumentsAlreadyNamedCorrectly_WithMembers ()
    {
      var innerExpression = new NamedExpression ("A", Expression.Constant (0));
      var memberInfo = (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A");
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { innerExpression }, memberInfo);

      var result = NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments);

      Assert.That (result, Is.SameAs (expression));
    }
    
  }

  internal class MemberTest
  {
    public string get_A { get; set; }
    public string get_ { get; set; }
  }
}