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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlContextExpressionVisitorTest
  {
    private TestableSqlExpressionContextExpressionVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _visitor = new TestableSqlExpressionContextExpressionVisitor();
    }

    [Test]
    public void ApplySqlExpressionContext_WithPredicateSemantics_IntExpression_ConvertedToBool ()
    {
      var constant = Expression.Constant (0);

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (constant, SqlExpressionContext.PredicateRequired);

      var expectedExpression = Expression.Equal (constant, new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ApplySqlExpressionContext_WithPredicateSemantics_BoolExpression_LeftAlone ()
    {
      var expression = new CustomExpression (typeof (bool));

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, SqlExpressionContext.PredicateRequired);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ApplySqlExpressionContext_WithPredicateSemantics_BoolColumn ()
    {
      var column = new SqlColumnExpression (typeof (bool), "x", "y");

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (column, SqlExpressionContext.PredicateRequired);

      var expectedExpression = Expression.Equal (new SqlColumnExpression (typeof (int), "x", "y"), new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ApplySqlExpressionContext_WithPredicateSemantics_BoolConstant ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);

      var resultTrue = SqlContextExpressionVisitor.ApplySqlExpressionContext (constantTrue, SqlExpressionContext.PredicateRequired);
      var resultFalse = SqlContextExpressionVisitor.ApplySqlExpressionContext (constantFalse, SqlExpressionContext.PredicateRequired);

      var expectedExpressionTrue = Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);

      var expectedExpressionFalse = Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot convert an expression of type 'System.String' to a boolean expression.")]
    public void ApplySqlExpressionContext_WithPredicateSemantics_OtherExpression_Throws ()
    {
      var expression = new CustomExpression (typeof (string));

      SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, SqlExpressionContext.PredicateRequired);
    }

    [Test]
    public void ApplySqlExpressionContext_WithValueSemantics_ValueExpression_LeftAlone ()
    {
      var constant = Expression.Constant (0);

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (constant, SqlExpressionContext.ValueRequired);

      Assert.That (constant, Is.SameAs (result));
    }

    [Test]
    public void ApplySqlExpressionContext_WithValueSemantics_BoolExpression_Converted ()
    {
      var expression = new CustomExpression (typeof (bool));

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, SqlExpressionContext.ValueRequired);

      var expectedExpression = new SqlCaseExpression (expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ApplySqlExpressionContext_WithValueSemantics_BoolColumn ()
    {
      var column = new SqlColumnExpression (typeof (bool), "x", "y");

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (column, SqlExpressionContext.ValueRequired);

      var expectedExpression = new SqlColumnExpression (typeof (int), "x", "y");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ApplySqlExpressionContext_WithValueSemantics_BoolConstant ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);

      var resultTrue = SqlContextExpressionVisitor.ApplySqlExpressionContext (constantTrue, SqlExpressionContext.ValueRequired);
      var resultFalse = SqlContextExpressionVisitor.ApplySqlExpressionContext (constantFalse, SqlExpressionContext.ValueRequired);

      var expectedExpressionTrue = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);

      var expectedExpressionFalse = Expression.Constant (0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
    }

    [Test]
    public void ApplySqlExpressionContext_WithSingleValueSemantics_WithSqlEntityExpression ()
    {
      var columnExpression = new SqlColumnExpression (typeof (int), "c", "ID");
      var entityExpression = new SqlEntityExpression (SqlStatementModelObjectMother.CreateSqlTable(), columnExpression);

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (entityExpression, SqlExpressionContext.SingleValueRequired);

      Assert.That (result, Is.TypeOf (typeof (SqlColumnExpression)));
      Assert.That (result, Is.SameAs (columnExpression));
    }

    [Test]
    public void ApplySqlExpressionContext_WithSingleValueSemantics_WithSqlEntityConstantExpression ()
    {
      var entityExpressionValue = Expression.Constant ("5").Value;
      var constantEntityExpression = new SqlEntityConstantExpression (typeof (string), entityExpressionValue, 5);

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (constantEntityExpression, SqlExpressionContext.SingleValueRequired);

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (result.Type, Is.EqualTo(typeof(int)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo(5));
    }

    [Test]
    public void ApplySqlExpressionContext_WithSingleValueSemantics_WithoutSqlEntityExpression ()
    {
      var columnExpression = new SqlColumnExpression (typeof (int), "c", "ID");

      var result = SqlContextExpressionVisitor.ApplySqlExpressionContext (columnExpression, SqlExpressionContext.SingleValueRequired);

      Assert.That (result, Is.TypeOf (typeof (SqlColumnExpression)));
      Assert.That (result, Is.SameAs (columnExpression));
    }

    [Test]
    public void VisitExpression_BooleanExpression ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));
      Expression result = SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired);

      Assert.That (result, Is.TypeOf(typeof(SqlCaseExpression)));
    }

    [Test]
    public void VisitExpression_Null_Ignored ()
    {
      var result = _visitor.VisitExpression (null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToValue_ForEqual ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.Equal (Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToValue_ForNotEqual ()
    {
      var expression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.NotEqual (Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var expression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOrElse ()
    {
      var expression = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAnd ()
    {
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOr ()
    {
      var expression = Expression.Or (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForExclusiveOr ()
    {
      var expression = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_OtherBinaryExpression_Unchanged ()
    {
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result = _visitor.VisitBinaryExpression (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void VisitBinaryExpression_OtherBinaryExpression_ChangedWhenInnerExpressionReplaced ()
    {
      var binary = BinaryExpression.And (Expression.Convert (Expression.Not (Expression.Constant (true)), typeof (int)), Expression.Constant (5));

      var result = _visitor.VisitBinaryExpression (binary);

      var expectedExpression =
          BinaryExpression.And (
              Expression.Convert (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))), typeof (int)),
              Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnaryExpression_UnaryBoolExpression_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));

      var result = _visitor.VisitUnaryExpression (unaryExpression);

      var expectedExpression = Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "'Convert' expressions are not supported with boolean type.")]
    public void VisitUnaryExpression_BooleanConvertUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.Convert (Expression.Constant (true), typeof (bool));

      _visitor.VisitUnaryExpression (unaryExpression);
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_Unchanged ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (5));

      var result = _visitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_ChangedWhenInnerExpressionReplaced ()
    {
      var unaryExpression = Expression.Not (Expression.Convert (Expression.Not (Expression.Constant (true)), typeof (int)));

      var result = _visitor.VisitUnaryExpression (unaryExpression);

      var expectedExpression =
          Expression.Not (
              Expression.Convert (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))), typeof (int)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlCaseExpression_ConvertsTestToPredicate ()
    {
      var caseExpression = new SqlCaseExpression (Expression.Constant (true), Expression.Constant (0), Expression.Constant (1));

      var result = _visitor.VisitSqlCaseExpression (caseExpression);

      var expectedExpression = new SqlCaseExpression (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Constant (0),
          Expression.Constant (1));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlCaseExpression_ConvertsValuesToValues ()
    {
      var caseExpression = new SqlCaseExpression (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));

      var result = _visitor.VisitSqlCaseExpression (caseExpression);

      var expectedExpression = new SqlCaseExpression (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Constant (1),
          Expression.Constant (0));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitExpression_AnyOtherExpression_Unchanged ()
    {
      var expression = new CustomExpression (typeof (Cook));

      var result = _visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Subquery selects a collection where a single value is expected.")]
    public void ApplySqlExpressionContext_SqlSubStatementExpression_ThrowsException ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (IQueryable<Cook>));
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);

      SqlContextExpressionVisitor.ApplySqlExpressionContext (sqlSubStatementExpression, SqlExpressionContext.ValueRequired);
    }

    [Test]
    public void VisitSqlBinaryExpression_NoSqlEntityExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook ();
      var expression = new SqlSubStatementExpression (sqlStatement);
      var sqlBinaryOperatorExpression = new SqlBinaryOperatorExpression ("IN", Expression.Constant (1), expression);

      var result = _visitor.VisitSqlBinaryOperatorExpression (sqlBinaryOperatorExpression);

      Assert.That (result, Is.TypeOf (typeof (SqlBinaryOperatorExpression)));
      Assert.That (result, Is.SameAs (sqlBinaryOperatorExpression));
    }

    [Test]
    public void VisitSqlBinaryExpression_WithSqlEnitiyExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook ();
      var expression = new SqlSubStatementExpression (sqlStatement);
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityExpression (typeof (Cook));
      var sqlBinaryOperatorExpression = new SqlBinaryOperatorExpression ("IN", entityExpression, expression);

      var result = _visitor.VisitSqlBinaryOperatorExpression (sqlBinaryOperatorExpression);

      Assert.That (result, Is.Not.SameAs (sqlBinaryOperatorExpression));
    }

    [Test]
    public void VisitSqlIsNullExpression ()
    {
      var sqlIsNullExpression = new SqlIsNullExpression (Expression.Constant (1));

      var result = _visitor.VisitSqlIsNullExpression (sqlIsNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNullExpression));
    }

    [Test]
    public void VisitSqlIsNullExpression_NewExpression ()
    {
      var entityExpression = new SqlEntityExpression (SqlStatementModelObjectMother.CreateSqlTable(), new SqlColumnExpression (typeof (int), "c", "ID"));
      var sqlIsNullExpression = new SqlIsNullExpression (entityExpression);

      var result = _visitor.VisitSqlIsNullExpression (sqlIsNullExpression);
      
      Assert.That (result, Is.Not.SameAs (sqlIsNullExpression));
      Assert.That (((SqlIsNullExpression) result).Expression, Is.TypeOf (typeof (SqlColumnExpression)));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (Expression.Constant (1));

      var result = _visitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNotNullExpression));
    }

    [Test]
    public void VisitSqlIsNotNullExpression_NewExpression ()
    {
      var entityExpression = new SqlEntityExpression (SqlStatementModelObjectMother.CreateSqlTable (), new SqlColumnExpression (typeof (int), "c", "ID"));
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (entityExpression);

      var result = _visitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpression);

      Assert.That (result, Is.Not.SameAs (sqlIsNotNullExpression));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.TypeOf (typeof (SqlColumnExpression)));
    }
  }
}