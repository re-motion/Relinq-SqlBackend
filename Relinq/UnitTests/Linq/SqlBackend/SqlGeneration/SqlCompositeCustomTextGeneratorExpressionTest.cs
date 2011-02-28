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
using System.Collections.ObjectModel;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
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
      var visitor = MockRepository.GeneratePartialMock<TestableExpressionTreeVisitor>();
      var commandBuilder = new SqlCommandBuilder();

      visitor
          .Expect (mock => mock.VisitExpression (_expression1))
          .Return (_expression1);
      visitor
          .Expect (mock => mock.VisitExpression (_expression2))
          .Return (_expression2);
      visitor.Replay();

      _sqlCompositeCustomTextGeneratorExpression.Generate (commandBuilder, visitor, MockRepository.GenerateStub<ISqlGenerationStage>());

      visitor.VerifyAllExpectations();
    }

    [Test]
    public void VisitChildren_ExpressionsChanged ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      var expressions = _sqlCompositeCustomTextGeneratorExpression.Expressions;
      visitorMock.Expect (mock => mock.VisitAndConvert (expressions, "VisitChildren")).Return (expressions);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlCompositeCustomTextGeneratorExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_sqlCompositeCustomTextGeneratorExpression));
    }

    [Test]
    public void VisitChildren_ChangeExpression ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      var expressions = new ReadOnlyCollection<Expression> (new List<Expression> { Expression.Constant (1) });
      visitorMock.Expect (mock => mock.VisitAndConvert (_sqlCompositeCustomTextGeneratorExpression.Expressions, "VisitChildren")).Return (expressions);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlCompositeCustomTextGeneratorExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
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