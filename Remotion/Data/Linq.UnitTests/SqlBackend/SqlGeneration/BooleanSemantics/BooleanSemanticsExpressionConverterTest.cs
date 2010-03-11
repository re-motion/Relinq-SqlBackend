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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "It is not allowed to specify a non-boolean expression when a predicate is required.")]
    public void Convert_WithPredicateSemantics_NonBooleanExpression_Throws ()
    {
      var constant = Expression.Constant (0);

      BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constant, BooleanSemanticsKind.PredicateRequired);
    }
    
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
    public void Convert_WithPredicateSemantics_ConstantTrue_ConvertedToEquals ()
    {
      var constantTrue = Expression.Constant (true);

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constantTrue, BooleanSemanticsKind.PredicateRequired);

      var expectedExpression = Expression.Equal (Expression.Constant (1), Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithPredicateSemantics_ConstantFalse_ConvertedToEquals ()
    {
      var constantTrue = Expression.Constant (false);

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constantTrue, BooleanSemanticsKind.PredicateRequired);

      var expectedExpression = Expression.Equal (Expression.Constant (0), Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_NonBooleanConstant_Unchanged ()
    {
      var constant = Expression.Constant (0);

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (constant, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (constant));
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
    public void Convert_WithPredicateSemantics_BoolColumn_ConvertedToIntColumnEqualsOne ()
    {
      var column = new SqlColumnExpression (typeof (bool), "x", "y");

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (column, BooleanSemanticsKind.PredicateRequired);

      var expectedExpression = Expression.Equal (new SqlColumnExpression (typeof (int), "x", "y"), Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_ValueColumn_Unchanged ()
    {
      var column = new SqlColumnExpression (typeof (string), "x", "y");

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (column, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (column));
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
    public void Convert_WithPredicateSemantics_BinaryBoolExpression_Unchanged ()
    {
      var binaryExpression = Expression.Equal (Expression.Constant (0), Expression.Constant (0));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, BooleanSemanticsKind.PredicateRequired);

      Assert.That (result, Is.SameAs (binaryExpression));
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToValue_ForEqual ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.Equal (Expression.Constant (1), Expression.Constant (0));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToValue_ForNotEqual ()
    {
      var expression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.NotEqual (Expression.Constant (1), Expression.Constant (0));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var expression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.AndAlso (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOrElse ()
    {
      var expression = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.OrElse (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAnd ()
    {
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.And (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOr ()
    {
      var expression = Expression.Or (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.Or (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForExclusiveOr ()
    {
      var expression = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));
      var expectedExpression = Expression.ExclusiveOr (
              Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
              Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.PredicateRequired);
      CheckLeftRightValueConverted (expression, expectedExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_WithValueSemantics_ValueBinaryExpression_Unchanged ()
    {
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binary, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void Convert_WithValueSemantics_ValueBinaryExpression_ChangedWhenInnerExpressionReplaced ()
    {
      var binary = BinaryExpression.And (Expression.Convert (Expression.Constant (true), typeof (int)), Expression.Constant (5));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binary, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = BinaryExpression.And (Expression.Convert (Expression.Constant (1), typeof (int)), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_UnaryBoolExpression_ConvertedToCaseWhen_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (unaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = new SqlCaseExpression (
          Expression.Not (Expression.Equal (Expression.Constant (1), Expression.Constant (1))), 
          Expression.Constant (1), 
          Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithPredicateSemantics_UnaryBoolExpression_Unchanged_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (unaryExpression, BooleanSemanticsKind.PredicateRequired);

      var expectedExpression = Expression.Not (Expression.Equal (Expression.Constant (1), Expression.Constant (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "'Convert' expressions are not supported with boolean type.")]
    public void Convert_BooleanConvertUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.Convert (Expression.Constant (true), typeof (bool));

      BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (unaryExpression, BooleanSemanticsKind.ValueRequired);
    }

    [Test]
    public void Convert_WithValueSemantics_ValueUnaryExpression_Unchanged ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (5));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (unaryExpression, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void Convert_WithValueSemantics_ValueUnaryExpression_ChangedWhenInnerExpressionReplaced ()
    {
      var unaryExpression = Expression.Not (Expression.Convert (Expression.Constant (true), typeof (int)));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (unaryExpression, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = Expression.Not (Expression.Convert (Expression.Constant (1), typeof (int)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_WithValueSemantics_SqlColumnList_Unchanged ()
    {
      var columnList = new SqlColumnListExpression (typeof (Cook));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (columnList, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (columnList));
    }

    [Test]
    public void Convert_WithValueSemantics_AnyOtherExpression_Unchanged ()
    {
      var expression = new NotSupportedExpression (typeof (Cook));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (expression, BooleanSemanticsKind.ValueRequired);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Convert_SqlCaseExpression_ConvertsTestToPredicate ()
    {
      var caseExpression = new SqlCaseExpression (Expression.Constant (true), Expression.Constant (0), Expression.Constant (1));
      
      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (caseExpression, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = new SqlCaseExpression (
        Expression.Equal (Expression.Constant (1), Expression.Constant (1)), 
        Expression.Constant (0), 
        Expression.Constant (1));
      
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Convert_SqlCaseExpression_ConvertsValuesToValues ()
    {
      var caseExpression = new SqlCaseExpression (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));

      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (caseExpression, BooleanSemanticsKind.ValueRequired);

      var expectedExpression = new SqlCaseExpression (
        Expression.Equal (Expression.Constant (1), Expression.Constant (1)),
        Expression.Constant (1),
        Expression.Constant (0));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Expression type 'Remotion.Data.Linq.UnitTests.SqlBackend.NotSupportedExpression' was not expected to have boolean type.")]
    public void Convert_WithValueSemantics_AnyOtherExpression_ThrowsWhenBool ()
    {
      var expression = new NotSupportedExpression (typeof (bool));
      BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (expression, BooleanSemanticsKind.ValueRequired);
    }

    private void CheckLeftRightValueConverted (
       BinaryExpression binaryExpression,
       BinaryExpression expectedBinaryExpression,
       BooleanSemanticsKind semanticsKind)
    {
      var result = BooleanSemanticsExpressionConverter.ConvertBooleanExpressions (binaryExpression, semanticsKind);

      Expression expectedExpression;
      if (semanticsKind == BooleanSemanticsKind.ValueRequired)
        expectedExpression = new SqlCaseExpression (expectedBinaryExpression, Expression.Constant (1), Expression.Constant (0));
      else
        expectedExpression = expectedBinaryExpression;

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

  }
}