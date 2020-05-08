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
using NUnit.Framework;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class BinaryExpressionTextGeneratorTest
  {
    private SqlCommandBuilder _commandBuilder;

    private Expression _leftIntegerExpression;
    private Expression _rightIntegerExpression;

    private Expression _leftDoubleExpression;
    private Expression _rightDoubleExpression;

    private Expression _leftStringExpression;
    private Expression _rightStringExpression;

    private Expression _nullExpression;
    private Expression _trueExpression;
    private Expression _falseExpression;
    private Expression _nullableTrueExpression;
    private Expression _nullableFalseExpression;

    private Expression _sqlEntityExpression;

    private BinaryExpressionTextGenerator _generator;
    private Mock<ExpressionVisitor> _expressionVisitorMock;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new SqlCommandBuilder();

      _expressionVisitorMock = new Mock<ExpressionVisitor>(MockBehavior.Strict);

      _leftIntegerExpression = Expression.Constant (1);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_leftIntegerExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("left"))
          .Returns (_leftIntegerExpression);

      _rightIntegerExpression = Expression.Constant (2);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_rightIntegerExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("right"))
          .Returns (_rightIntegerExpression);

      _leftDoubleExpression = Expression.Constant (1D);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_leftDoubleExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("leftDouble"))
          .Returns (_leftDoubleExpression);

      _rightDoubleExpression = Expression.Constant (2D);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_rightDoubleExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("rightDouble"))
          .Returns (_rightDoubleExpression);

      _leftStringExpression = Expression.Constant ("Left");
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_leftStringExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("leftString"))
          .Returns (_leftStringExpression);

      _rightStringExpression = Expression.Constant ("Right");
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_rightStringExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("rightString"))
          .Returns (_rightStringExpression);

      _nullExpression = Expression.Constant (null, typeof (string));
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_nullExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("NULL"))
          .Returns (_rightStringExpression);

      _trueExpression = Expression.Constant (true);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_trueExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("true"))
          .Returns (_trueExpression);

      _falseExpression = Expression.Constant (false);
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_falseExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("false"))
          .Returns (_falseExpression);

      _nullableTrueExpression = Expression.Constant (true, typeof (bool?));
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_nullableTrueExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("true"))
          .Returns (_nullableTrueExpression);

      _nullableFalseExpression = Expression.Constant (false, typeof (bool?));
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_nullableFalseExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("false"))
          .Returns (_nullableFalseExpression);

      _sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      _expressionVisitorMock
          .Setup (stub => stub.Visit (_sqlEntityExpression))
          .Callback ((Expression _) => _commandBuilder.Append ("[c].[ID]"))
          .Returns (((SqlEntityExpression) _sqlEntityExpression).GetIdentityExpression());

      _generator = new BinaryExpressionTextGenerator (_commandBuilder, _expressionVisitorMock.Object);
    }

    [Test]
    public void VisitBinaryExpression_Add ()
    {
      var binaryExpression = Expression.Add (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left + right"));
    }

    [Test]
    public void VisitBinaryExpression_Subtract ()
    {
      var binaryExpression = Expression.Subtract (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left - right"));
    }

    [Test]
    public void VisitBinaryExpression_Multiply ()
    {
      var binaryExpression = Expression.Multiply (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left * right"));
    }

    [Test]
    public void VisitBinaryExpression_Divide ()
    {
      var binaryExpression = Expression.Divide (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left / right"));
    }

    [Test]
    public void VisitBinaryExpression_Modulo ()
    {
      var binaryExpression = Expression.Modulo (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left % right"));
    }

    [Test]
    public void VisitBinaryExpression_AddChecked ()
    {
      var binaryExpression = Expression.AddChecked (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left + right"));
    }

    [Test]
    public void VisitBinaryExpression_MultiplyChecked ()
    {
      var binaryExpression = Expression.MultiplyChecked (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left * right"));
    }

    [Test]
    public void VisitBinaryExpression_SubtractChecked ()
    {
      var binaryExpression = Expression.SubtractChecked (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left - right"));
    }

    [Test]
    public void VisitBinaryExpression_AndAlso_Boolean ()
    {
      var binaryExpression = Expression.AndAlso (_trueExpression, _trueExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("true AND true")));
    }

    [Test]
    public void VisitBinaryExpression_OrElse ()
    {
      var binaryExpression = Expression.OrElse (_trueExpression, _trueExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("true OR true"));
    }

    [Test]
    public void VisitBinaryExpression_And ()
    {
      var binaryExpression = Expression.And (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left & right"));
    }

    [Test]
    public void VisitBinaryExpression_And_OnBooleans ()
    {
      var binaryExpression = Expression.And (_trueExpression, _falseExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("true AND false"));
    }

    [Test]
    public void VisitBinaryExpression_And_OnNullableBooleans ()
    {
      var binaryExpression = Expression.And (_nullableTrueExpression, _nullableFalseExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("true AND false"));
    }

    [Test]
    public void VisitBinaryExpression_Or ()
    {
      var binaryExpression = Expression.Or (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left | right"));
    }

    [Test]
    public void VisitBinaryExpression_Or_OnBooleans ()
    {
      var binaryExpression = Expression.Or (_trueExpression, _falseExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("true OR false"));
    }

    [Test]
    public void VisitBinaryExpression_Or_OnNullableBooleans ()
    {
      var binaryExpression = Expression.Or (_nullableTrueExpression, _nullableFalseExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("true OR false"));
    }

    [Test]
    public void VisitBinaryExpression_ExclusiveOr ()
    {
      var binaryExpression = Expression.ExclusiveOr (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left ^ right"));
    }

    [Test]
    public void VisitBinaryExpression_ExclusiveOr_OnBooleans ()
    {
      var binaryExpression = Expression.ExclusiveOr (_trueExpression, _falseExpression);
      var expectedXorSimulation = Expression.OrElse (
          Expression.AndAlso (_trueExpression, Expression.Not (_falseExpression)),
          Expression.AndAlso (Expression.Not (_trueExpression), _falseExpression));
      _expressionVisitorMock
          .Setup (mock => mock.Visit (It.Is<Expression> (expr => expr is BinaryExpression)))
          .Callback ((Expression mi) =>
          {
            var expr = (BinaryExpression) mi;
            SqlExpressionTreeComparer.CheckAreEqualTrees (expr, expectedXorSimulation);
            _commandBuilder.Append ("XOR SIMULATION");
          })
          .Returns ((Expression) null);
      
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      _expressionVisitorMock.Verify();

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("XOR SIMULATION"));
    }

    [Test]
    public void VisitBinaryExpression_ExclusiveOr_OnNullableBooleans ()
    {
      var binaryExpression = Expression.ExclusiveOr (_nullableTrueExpression, _nullableFalseExpression);
      var expectedXorSimulation = Expression.OrElse (
          Expression.AndAlso (_nullableTrueExpression, Expression.Not (_nullableFalseExpression)),
          Expression.AndAlso (Expression.Not (_nullableTrueExpression), _nullableFalseExpression));
      _expressionVisitorMock
          .Setup (mock => mock.Visit (It.Is<Expression> (expr => expr is BinaryExpression)))
          .Callback ((Expression mi) =>
          {
            var expr = (BinaryExpression) mi;
            SqlExpressionTreeComparer.CheckAreEqualTrees (expr, expectedXorSimulation);
            _commandBuilder.Append ("XOR SIMULATION");
          })
          .Returns ((Expression) null);

      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      _expressionVisitorMock.Verify ();

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("XOR SIMULATION"));
    }

    [Test]
    public void VisitBinaryExpression_Equals ()
    {
      var binaryExpression = Expression.Equal (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left = right"));
    }

    [Test]
    public void VisitBinaryExpression_GreaterThan ()
    {
      var binaryExpression = Expression.GreaterThan (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left > right"));
    }

    [Test]
    public void VisitBinaryExpression_GreaterThanOrEqual ()
    {
      var binaryExpression = Expression.GreaterThanOrEqual (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left >= right"));
    }

    [Test]
    public void VisitBinaryExpression_LessThan ()
    {
      var binaryExpression = Expression.LessThan (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left < right"));
    }

    [Test]
    public void VisitBinaryExpression_LessThanOrEqual ()
    {
      var binaryExpression = Expression.LessThanOrEqual (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left <= right"));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual ()
    {
      var binaryExpression = Expression.NotEqual (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left <> right"));
    }

    [Test]
    public void VisitBinaryExpression_Coalesce ()
    {
      var binaryExpression = Expression.Coalesce (_leftStringExpression, _rightStringExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("COALESCE (leftString, rightString)"));
    }

    //[Test]
    //public void VisitBinaryExpression_EqualWithNullOnRightSide ()
    //{
    //  Expression nullExpression = Expression.Constant (null);
    //  var binaryExpression = Expression.Equal (_leftStringExpression, nullExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();

    //  Assert.That (result, Is.EqualTo ("leftString IS NULL"));
    //}

    //[Test]
    //public void VisitBinaryExpression_NotEqualWithNullOnRightSide ()
    //{
    //  Expression nullExpression = Expression.Constant (null);
    //  var binaryExpression = Expression.NotEqual (_leftStringExpression, nullExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();

    //  Assert.That (result, Is.EqualTo ("leftString IS NOT NULL"));
    //}

    //[Test]
    //public void VisitBinaryExpression_EqualWithNullOnLeftSide ()
    //{
    //  Expression nullExpression = Expression.Constant (null);
    //  var binaryExpression = Expression.Equal (nullExpression, _rightStringExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();

    //  Assert.That (result, Is.EqualTo ("rightString IS NULL"));
    //}

    //[Test]
    //public void VisitBinaryExpression_NotEqualWithNullOnLeftSide ()
    //{
    //  Expression nullExpression = Expression.Constant (null);
    //  var binaryExpression = Expression.NotEqual (nullExpression, _rightStringExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();

    //  Assert.That (result, Is.EqualTo ("rightString IS NOT NULL"));
    //}

    [Test]
    public void VisitBinaryExpression_AddWithNull ()
    {
      var binaryExpression = Expression.Coalesce (_nullExpression, _rightStringExpression);

      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText ();
      Assert.That (result, Is.EqualTo ("COALESCE (NULL, rightString)"));
    }

    [Test]
    public void VisitBinaryExpression_Power ()
    {
      var binaryExpression = Expression.Power (_leftDoubleExpression, _rightDoubleExpression);

      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();
      Assert.That (result, Is.EqualTo ("POWER (leftDouble, rightDouble)"));
    }

    [Test]
    public void VisitBinaryExpression_EntityComparison ()
    {
      var binaryExpression = Expression.Equal (_sqlEntityExpression, _sqlEntityExpression);

      _generator.GenerateSqlForBinaryExpression (binaryExpression);
      
      var result = _commandBuilder.GetCommandText();
      Assert.That (result, Is.EqualTo ("[c].[ID] = [c].[ID]"));
    }

    //[Test]
    //public void VisitBinaryExpression_EntityComparisonWithNullOnRightSide ()
    //{
    //  var binaryExpression = Expression.Equal (_nullExpression, _sqlEntityExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();
    //  Assert.That (result, Is.EqualTo ("[c].[ID] IS NULL"));
    //}

    //[Test]
    //public void VisitBinaryExpression_EntityComparisonWithNullOnLeftSide ()
    //{
    //  var binaryExpression = Expression.Equal (_sqlEntityExpression, _nullExpression);

    //  _generator.GenerateSqlForBinaryExpression (binaryExpression);

    //  var result = _commandBuilder.GetCommandText();
    //  Assert.That (result, Is.EqualTo ("[c].[ID] IS NULL"));
    //}

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void NotSupportedNodeType ()
    {
      var binaryExpression = Expression.ArrayIndex (Expression.Constant (new[] { 1, 2, 3 }), Expression.Constant (10));
      _generator.GenerateSqlForBinaryExpression (binaryExpression);
    }
  }
}