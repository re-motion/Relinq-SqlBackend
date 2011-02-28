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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
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

    private Expression _sqlEntityExpression;

    private BinaryExpressionTextGenerator _generator;
    private ExpressionTreeVisitor _expressionVisitorStub;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new SqlCommandBuilder();

      _expressionVisitorStub = MockRepository.GenerateStub<ExpressionTreeVisitor>();

      _leftIntegerExpression = Expression.Constant (1);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_leftIntegerExpression))
          .WhenCalled (mi => _commandBuilder.Append ("left"))
          .Return (_leftIntegerExpression);

      _rightIntegerExpression = Expression.Constant (2);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_rightIntegerExpression))
          .WhenCalled (mi => _commandBuilder.Append ("right"))
          .Return (_rightIntegerExpression);

      _leftDoubleExpression = Expression.Constant (1D);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_leftDoubleExpression))
          .WhenCalled (mi => _commandBuilder.Append ("leftDouble"))
          .Return (_leftDoubleExpression);

      _rightDoubleExpression = Expression.Constant (2D);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_rightDoubleExpression))
          .WhenCalled (mi => _commandBuilder.Append ("rightDouble"))
          .Return (_rightDoubleExpression);

      _leftStringExpression = Expression.Constant ("Left");
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_leftStringExpression))
          .WhenCalled (mi => _commandBuilder.Append ("leftString"))
          .Return (_leftStringExpression);

      _rightStringExpression = Expression.Constant ("Right");
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_rightStringExpression))
          .WhenCalled (mi => _commandBuilder.Append ("rightString"))
          .Return (_rightStringExpression);

      _nullExpression = Expression.Constant (null, typeof (string));
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_nullExpression))
          .WhenCalled (mi => _commandBuilder.Append ("NULL"))
          .Return (_rightStringExpression);

      _trueExpression = Expression.Constant (true);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_trueExpression))
          .WhenCalled (mi => _commandBuilder.Append ("true"))
          .Return (_trueExpression);

      _falseExpression = Expression.Constant (false);
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_falseExpression))
          .WhenCalled (mi => _commandBuilder.Append ("false"))
          .Return (_falseExpression);

      _sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false));
      _expressionVisitorStub
          .Stub (stub => stub.VisitExpression (_sqlEntityExpression))
          .WhenCalled (mi => _commandBuilder.Append ("[c].[ID]"))
          .Return (((SqlEntityExpression) _sqlEntityExpression).PrimaryKeyColumn);

      _generator = new BinaryExpressionTextGenerator (_commandBuilder, _expressionVisitorStub);
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
    public void VisitBinaryExpression_Or ()
    {
      var binaryExpression = Expression.Or (_leftIntegerExpression, _rightIntegerExpression);
      _generator.GenerateSqlForBinaryExpression (binaryExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("left | right"));
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