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
using Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Parsing;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.BooleanSemantics
{
  [TestFixture]
  public class BooleanSemanticsExpressionConverterTest
  {
    [Test]
    public void Convert_WithValueSemantics_ConstantTrue_ConvertedToIntOne ()
    {
      var constantTrue = Expression.Constant (true);
      
      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constantTrue, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_ConstantFalse_ConvertedToIntZero ()
    {
      var constantFalse = Expression.Constant (false);

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constantFalse, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = Expression.Constant (0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_NonBoolean_Unchanged ()
    {
      var constant = Expression.Constant (0);
      var column = new SqlColumnExpression (typeof (string), "x", "y");
      var columnList = new SqlColumnListExpression (typeof (Cook));
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result1 = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constant, BooleanSemanticsKind.ValueRequired);
      var result2 = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (column, BooleanSemanticsKind.ValueRequired);
      var result3 = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (columnList, BooleanSemanticsKind.ValueRequired);
      var result4 = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binary, BooleanSemanticsKind.ValueRequired);

      Assert.That (result1, Is.SameAs (constant));
      Assert.That (result2, Is.SameAs (column));
      Assert.That (result3, Is.SameAs (columnList));
      Assert.That (result4, Is.SameAs (binary));
    }

    [Test]
    public void Convert_WithValueSemantics_BoolColumn_ConvertedToIntColumn ()
    {
      var column = new SqlColumnExpression (typeof (bool), "x", "y");

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (column, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = new SqlColumnExpression (typeof (int), "x", "y");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_BinaryBoolExpression_ConvertedToCaseWhen ()
    {
      var binaryExpression = Expression.Equal (Expression.Constant (0), Expression.Constant (0));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = new SqlCaseExpression (binaryExpression, Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_BinaryBoolExpression_ConvertsLeftRightToValue_ForEqual ()
    {
      var binaryExpression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedBinaryExpression = Expression.Equal (Expression.Constant (1), Expression.Constant (0));
      var expectedExpression = new SqlCaseExpression (expectedBinaryExpression, Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_BinaryBoolExpression_ConvertsLeftRightToValue_ForNotEqual ()
    {
      var binaryExpression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedBinaryExpression = Expression.NotEqual (Expression.Constant (1), Expression.Constant (0));
      var expectedExpression = new SqlCaseExpression (expectedBinaryExpression, Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [Ignore ("TODO 2362, add And (with booleans), OrElse, Or (with booleans), ExclusiveOr (with booleans)")]
    public void Convert_WithValueSemantics_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var binaryExpression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedBinaryExpression = 
          Expression.AndAlso (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)), 
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));
      var expectedExpression = new SqlCaseExpression (expectedBinaryExpression, Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Expression type 'System.Linq.Expressions.Expression' was not expected to have boolean type.")]
    public void UnexpectedExpression ()
    {
      var expression = new NotSupportedExpression (typeof (bool));
      BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (expression, BooleanSemanticsKind.ValueRequired);
    }
  }
}