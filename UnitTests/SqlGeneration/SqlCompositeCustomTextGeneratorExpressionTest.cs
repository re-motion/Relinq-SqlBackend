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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting.Parsing;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlCompositeCustomTextGeneratorExpressionTest
  {
    private SqlCompositeCustomTextGeneratorExpression _sqlCompositeCustomTextGeneratorExpression;
    private ConstantExpression _expression1;
    private ConstantExpression _expression2;

    [SetUp]
    public void SetUp ()
    {
      _expression1 = Expression.Constant ("5");
      _expression2 = Expression.Constant ("1");
      _sqlCompositeCustomTextGeneratorExpression = new SqlCompositeCustomTextGeneratorExpression (typeof (Cook), _expression1, _expression2);
    }

    [Test]
    public void Generate ()
    {
      var visitor = new Mock<TestableExpressionVisitor>();
      visitor.CallBase = true;
      var commandBuilder = new SqlCommandBuilder();

      visitor
          .Setup (mock => mock.Visit (_expression1))
          .Returns (_expression1)
          .Verifiable();
      visitor
          .Setup (mock => mock.Visit (_expression2))
          .Returns (_expression2)
          .Verifiable();

      _sqlCompositeCustomTextGeneratorExpression.Generate (commandBuilder, visitor.Object, new Mock<ISqlGenerationStage>().Object);

      visitor.Verify();
    }

    [Test]
    public void VisitChildren_ExpressionsNotChanged ()
    {
      var visitorMock = new Mock<ExpressionVisitor>();
      var expressions = _sqlCompositeCustomTextGeneratorExpression.Expressions;
      visitorMock.Setup (mock => mock.Visit (expressions[0])).Returns (expressions[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (expressions[1])).Returns (expressions[1]).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlCompositeCustomTextGeneratorExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (_sqlCompositeCustomTextGeneratorExpression));
    }

    [Test]
    public void VisitChildren_ChangeExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor>();
      var expressions = new ReadOnlyCollection<Expression> (new List<Expression> { Expression.Constant (1), Expression.Constant (2) });
      visitorMock.Setup (mock => mock.Visit (_sqlCompositeCustomTextGeneratorExpression.Expressions[0])).Returns (expressions[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_sqlCompositeCustomTextGeneratorExpression.Expressions[1])).Returns (expressions[1]).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlCompositeCustomTextGeneratorExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_sqlCompositeCustomTextGeneratorExpression));
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlCompositeCustomTextGeneratorExpression.ToString();

      Assert.That (result, Is.EqualTo ("\"5\" \"1\""));
    }
  }
}