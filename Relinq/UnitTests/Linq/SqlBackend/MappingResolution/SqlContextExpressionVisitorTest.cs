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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlContextExpressionVisitorTest
  {
    private TestableSqlContextExpressionVisitor _valueRequiredVisitor;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private TestableSqlContextExpressionVisitor _singleValueRequiredVisitor;
    private TestableSqlContextExpressionVisitor _singleValuePreferredVisitor;
    private TestableSqlContextExpressionVisitor _predicateRequiredVisitor;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _mappingResolutionContext = new MappingResolutionContext();
      _valueRequiredVisitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);
      _singleValueRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired,
          _stageMock,
          _mappingResolutionContext);
      _singleValuePreferredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValuePreferred,
          _stageMock,
          _mappingResolutionContext);
      _predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void ApplyContext ()
    {
      var valueExpression = Expression.Constant (0);
      var predicateExpression = Expression.Constant (true);

      var convertedValue = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          new SqlConvertedBooleanExpression (valueExpression), SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
      var convertedPredicate = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          predicateExpression, SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);

      var expectedConvertedValue = Expression.Equal (valueExpression, new SqlLiteralExpression (1));
      var expectedConvertedPredicate = new SqlConvertedBooleanExpression (Expression.Constant (1));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedValue, convertedValue);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedPredicate, convertedPredicate);
    }

    [Test]
    public void ApplyContext_SemanticsPropagatedToChildExpressionsByDefault ()
    {
      var expressionOfCorrectType = new TestExtensionExpression (new TestExtensionExpressionWithoutChildren (typeof (bool)));
      var expressionOfIncorrectType =
          new TestExtensionExpression (new SqlConvertedBooleanExpression (new TestExtensionExpressionWithoutChildren (typeof (int))));

      var result1 = _predicateRequiredVisitor.VisitExpression (expressionOfCorrectType);
      var result2 = _predicateRequiredVisitor.VisitExpression (expressionOfIncorrectType);

      Assert.That (result1, Is.SameAs (expressionOfCorrectType));

      var expectedResult2 = new TestExtensionExpression (
          Expression.Equal (((SqlConvertedBooleanExpression) expressionOfIncorrectType.Expression).Expression, new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult2, result2);
    }

    [Test]
    public void VisitExpression_Null_Ignored ()
    {
      var result = _valueRequiredVisitor.VisitExpression (null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitExpression_CallsNodeSpecificVisitMethods ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression.PrimaryKeyColumn));
    }

    [Test]
    public void VisitExpression_ValueRequired_ConvertsBool_ToValue ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);
      var resultNullable = visitor.VisitExpression (nullableExpression);

      var expected = new SqlConvertedBooleanExpression (GetNonNullablePredicateAsValueExpression (expression));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = new SqlConvertedBooleanExpression (GetNullablePredicateAsValueExpression (nullableExpression));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    public void VisitExpression_SingleValueRequired_ConvertsBool_ToSingleValue ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);
      var resultNullable = visitor.VisitExpression (nullableExpression);

      var expected = new SqlConvertedBooleanExpression (GetNonNullablePredicateAsValueExpression (expression));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = new SqlConvertedBooleanExpression (GetNullablePredicateAsValueExpression (nullableExpression));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    public void VisitExpression_SingleValuePreferred_ConvertsBool_ToSingleValue ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValuePreferred, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);
      var resultNullable = visitor.VisitExpression (nullableExpression);

      var expected = new SqlConvertedBooleanExpression (GetNonNullablePredicateAsValueExpression (expression));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = new SqlConvertedBooleanExpression (GetNullablePredicateAsValueExpression (nullableExpression));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    public void VisitExpression_ValueRequired_LeavesExistingValue ()
    {
      var expression = CreateNewExpression ();

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_SingleValueRequired_LeavesExistingSingleValue ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_SingleValuePreferred_LeavesExistingSingleValue ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValuePreferred, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_ValueSemantics_LeavesExistingSqlConvertedBooleanExpression ()
    {
      var convertedBooleanExpression = new SqlConvertedBooleanExpression (Expression.Constant (1));

      var result = _valueRequiredVisitor.VisitExpression (convertedBooleanExpression);

      Assert.That (result, Is.SameAs (convertedBooleanExpression));
    }

    [Test]
    public void VisitExpression_ValueSemantics_LeavesMethodCallExpression ()
    {
      var methodWithBoolResultExpression = Expression.Call (ReflectionUtility.GetMethod (() => MethodWithBoolResult()));

      var result = _valueRequiredVisitor.VisitExpression (methodWithBoolResultExpression);

      Assert.That (result, Is.SameAs (methodWithBoolResultExpression));
    }

    [Test]
    public void VisitExpression_LeavesExistingPredicate ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);
      var resultNullable = visitor.VisitExpression (nullableExpression);

      Assert.That (result, Is.SameAs (expression));
      Assert.That (resultNullable, Is.SameAs (nullableExpression));
    }

    [Test]
    public void VisitExpression_ConvertedInt_ToPredicate ()
    {
      var expression = new SqlConvertedBooleanExpression (new CustomExpression (typeof (int)));
      var nullableExpression = new SqlConvertedBooleanExpression (new CustomExpression (typeof (int?)));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);
      var resultNullable = visitor.VisitExpression (nullableExpression);

      var expected = Expression.Equal (expression.Expression, new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = Expression.Equal (nullableExpression.Expression, new SqlLiteralExpression (1, true), true, null);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot convert an expression of type 'System.String' to a boolean expression. Expression: 'CustomExpression'")]
    public void VisitExpression_ThrowsOnNonConvertible_ToPredicate ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
      visitor.VisitExpression (expression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Invalid enum value: -1")]
    public void VisitExpression_ThrowsOnInvalidContext ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor ((SqlExpressionContext) (-1), _stageMock, _mappingResolutionContext);
      visitor.VisitExpression (expression);
    }

    [Test]
    public void VisitExpression_AppliesSpecifiedSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitSqlConvertedBooleanExpression_InnerUnchanged ()
    {
      var converted = new SqlConvertedBooleanExpression (Expression.Constant (0));

      var result = _valueRequiredVisitor.VisitSqlConvertedBooleanExpression (converted);

      Assert.That (result, Is.SameAs (converted));
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_ValueRequired ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _valueRequiredVisitor.VisitConstantExpression (constantTrue);
      var resultFalse = _valueRequiredVisitor.VisitConstantExpression (constantFalse);
      var resultNullableTrue = _valueRequiredVisitor.VisitConstantExpression (constantNullableTrue);
      var resultNullableFalse = _valueRequiredVisitor.VisitConstantExpression (constantNullableFalse);
      var resultNullableNull = _valueRequiredVisitor.VisitConstantExpression (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_SingleValueRequired ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _singleValueRequiredVisitor.VisitConstantExpression (constantTrue);
      var resultFalse = _singleValueRequiredVisitor.VisitConstantExpression (constantFalse);
      var resultNullableTrue = _singleValueRequiredVisitor.VisitConstantExpression (constantNullableTrue);
      var resultNullableFalse = _singleValueRequiredVisitor.VisitConstantExpression (constantNullableFalse);
      var resultNullableNull = _singleValueRequiredVisitor.VisitConstantExpression (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_SingleValuePreferred ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _singleValuePreferredVisitor.VisitConstantExpression (constantTrue);
      var resultFalse = _singleValuePreferredVisitor.VisitConstantExpression (constantFalse);
      var resultNullableTrue = _singleValuePreferredVisitor.VisitConstantExpression (constantNullableTrue);
      var resultNullableFalse = _singleValuePreferredVisitor.VisitConstantExpression (constantNullableFalse);
      var resultNullableNull = _singleValuePreferredVisitor.VisitConstantExpression (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_PredicateRequired ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _predicateRequiredVisitor.VisitConstantExpression (constantTrue);
      var resultFalse = _predicateRequiredVisitor.VisitConstantExpression (constantFalse);
      var resultNullableTrue = _predicateRequiredVisitor.VisitConstantExpression (constantNullableTrue);
      var resultNullableFalse = _predicateRequiredVisitor.VisitConstantExpression (constantNullableFalse);
      var resultNullableNull = _predicateRequiredVisitor.VisitConstantExpression (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_OtherConstants ()
    {
      var constant = Expression.Constant ("hello");

      var result = _valueRequiredVisitor.VisitExpression (constant);

      Assert.That (result, Is.SameAs (constant));
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_NoPrimaryColumn_ValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);

      var result = _valueRequiredVisitor.VisitSqlColumnExpression (column);
      var nullableResult = _valueRequiredVisitor.VisitSqlColumnExpression (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_IsPrimaryColumn_ValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", true);

      var result = _valueRequiredVisitor.VisitSqlColumnExpression (column);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", true));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_SingleValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);

      var result = _singleValueRequiredVisitor.VisitSqlColumnExpression (column);
      var nullableResult = _singleValueRequiredVisitor.VisitSqlColumnExpression (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_SingleValuePreferred ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);

      var result = _singleValuePreferredVisitor.VisitSqlColumnExpression (column);
      var nullableResult = _singleValuePreferredVisitor.VisitSqlColumnExpression (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_PredicateRequired ()
    {
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);

      var result = _predicateRequiredVisitor.VisitSqlColumnExpression (column);
      var nullableResult = _predicateRequiredVisitor.VisitSqlColumnExpression (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_OtherColumn ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (string), "x", "y", false);

      var result = _valueRequiredVisitor.VisitSqlColumnExpression (column);

      Assert.That (result, Is.SameAs (column));
    }

    [Test]
    public void VisitSqlEntityExpression_WithSingleValueSemantics_ConvertsEntityToPrimaryKey ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var result1 = _singleValueRequiredVisitor.VisitSqlEntityExpression (entityExpression);
      var result2 = _singleValuePreferredVisitor.VisitSqlEntityExpression (entityExpression);

      Assert.That (result1, Is.SameAs (entityExpression.PrimaryKeyColumn));
      Assert.That (result2, Is.SameAs (entityExpression.PrimaryKeyColumn));
    }

    [Test]
    public void VisitSqlEntityExpression_WithNonSingleValueSemantics_LeavesEntity ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitSqlEntityExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToSingleValue_ForEqual ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.Equal (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)), true, null);

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.Equal (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.Equal (
          new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?))),
          new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?))),
          true,
          null);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToSingleValue_ForNotEqual ()
    {
      var expression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.NotEqual (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)), true, null);

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.NotEqual (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.NotEqual (
          new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?))),
          new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?))),
          true,
          null);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var expression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.AndAlso (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOrElse ()
    {
      var expression = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.OrElse (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAnd ()
    {
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.And (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.And (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOr ()
    {
      var expression = Expression.Or (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.Or (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForExclusiveOr ()
    {
      var expression = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.ExclusiveOr (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);

      var expectedExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_PassesMethod ()
    {
      var operatorMethod = ReflectionUtility.GetMethod (() => FakeAndOperator(false, false));
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false), operatorMethod);
      Assert.That (expression.Method, Is.Not.Null);

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)),
          operatorMethod);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_Coalesce ()
    {
      // nullableBool ?? bool is converted not as ConvertedBool (nullableInt) ?? ConvertedBool (int), but as ConvertedBool (nullableInt ?? int).
      var expression = Expression.Coalesce (Expression.Constant (false, typeof (bool?)), Expression.Constant (true));
      var nullableExpression = Expression.Coalesce (Expression.Constant (false, typeof (bool?)), Expression.Constant (true, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinaryExpression (nullableExpression);
      var resultForPredicateSemantics = _predicateRequiredVisitor.VisitBinaryExpression (expression);

      var expectedExpression = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1, typeof (int?))));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
      var expectedResultForPredicateSemantics = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultForPredicateSemantics, resultForPredicateSemantics);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_Coalesce_OptimizationForCoalesceToFalse ()
    {
      // With predicate semantics, nullableBool ?? false is optimized to be the same as Convert (nullableBool, typeof (bool)) because SQL handles 
      // NULL in a falsey way in predicates, and the generated SQL is nicer.
      // With value semantics, this is not optimized.

      var expression = Expression.Coalesce (Expression.Constant (true, typeof (bool?)), Expression.Constant (false));

      var resultForValueSemantics = _valueRequiredVisitor.VisitBinaryExpression (expression);
      var resultForPredicateSemantics = _predicateRequiredVisitor.VisitBinaryExpression (expression);

      var expectedExpressionForValueSemantics = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (1, typeof (int?)),
          Expression.Constant (0)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionForValueSemantics, resultForValueSemantics);
      var expectedResultForPredicateSemantics =
          Expression.Convert (
              Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
              typeof (bool));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultForPredicateSemantics, resultForPredicateSemantics);
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_Unchanged ()
    {
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitBinaryExpression (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_OperandsGetSingleValueSemantics ()
    {
      var binary = BinaryExpression.And (Expression.Convert (Expression.Not (Expression.Constant (true)), typeof (int)), Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitBinaryExpression (binary);

      var expectedExpression =
          BinaryExpression.And (
              Expression.Convert (
                  new SqlConvertedBooleanExpression (
                      GetNonNullablePredicateAsValueExpression (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))))),
                  typeof (int)),
              Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_LiftsOperands_IfNecessary ()
    {
      var left = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "t0", typeof (int));
      var right = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "t0", typeof (int?));
      var binary = BinaryExpression.Equal (left, right);

      var result = _predicateRequiredVisitor.VisitBinaryExpression (binary);

      var expectedExpression = BinaryExpression.Equal (Expression.Convert (left.PrimaryKeyColumn, typeof (int?)), right.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
    
    [Test]
    public void VisitUnaryExpression_UnaryBoolExpression_Not_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));
      var unaryNullableExpression = Expression.Not (Expression.Constant (true, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitUnaryExpression (unaryExpression);
      var resultNullable = _valueRequiredVisitor.VisitUnaryExpression (unaryNullableExpression);

      var expectedExpression = Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression =
          Expression.Not (Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, resultNullable);
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_OperandChanged_WithDifferentType ()
    {
      var entity = new SqlEntityDefinitionExpression (typeof (Cook), "c", "CookTable", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true));
      var unaryExpression = Expression.Convert (entity, typeof (object));

      var result = _singleValueRequiredVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.Not.SameAs (unaryExpression));
      Assert.That (result, Is.SameAs (entity.PrimaryKeyColumn));
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_OperandChanged_WithSameType ()
    {
      var unaryExpression = Expression.Convert (Expression.Constant (true), typeof (object));

      var result = _singleValueRequiredVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.Not.SameAs (unaryExpression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Convert));
      Assert.That (((UnaryExpression) result).Operand, Is.TypeOf<SqlConvertedBooleanExpression>());
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_SameOperand ()
    {
      var unaryExpression =
          Expression.Convert (
              new SqlEntityDefinitionExpression (typeof (Cook), "c", "CookTable", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true)),
              typeof (object));

      var result = _valueRequiredVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_Unchanged ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_OperandConvertedToSingleValue ()
    {
      var unaryExpression = // ValueRequired
          Expression.Not (
              // ValueRequired
              Expression.Convert (
                  Expression.Not (
                      Expression.Constant (true)
                      ),
                  typeof (int)));

      var result = _valueRequiredVisitor.VisitUnaryExpression (unaryExpression);

      var expectedExpression =
          Expression.Not (
              Expression.Convert (
                  new SqlConvertedBooleanExpression (
                    GetNonNullablePredicateAsValueExpression (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))))),
                  typeof (int)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnaryExpression_SqlConvertedBooleanExpression ()
    {
      var convertToBoolExpression = Expression.Convert (Expression.Constant (true, typeof (bool?)), typeof (bool));
      var convertToNullableBoolExpression = Expression.Convert (Expression.Constant (true), typeof (bool?));

      var result = _valueRequiredVisitor.VisitUnaryExpression (convertToBoolExpression);
      var resultNullable = _valueRequiredVisitor.VisitUnaryExpression (convertToNullableBoolExpression);

      var expectedExpression = new SqlConvertedBooleanExpression (Expression.Convert (Expression.Constant (1, typeof (int?)), typeof (int)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (Expression.Convert (Expression.Constant (1), typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, resultNullable);
    }

    [Test]
    public void VisitSqlIsNullExpression_AppliesSingleValueSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNullExpressionWithValue = new SqlIsNullExpression (Expression.Constant (1));
      var sqlIsNullExpressionWithEntity = new SqlIsNullExpression (entityExpression);

      var resultWithValue = _valueRequiredVisitor.VisitSqlIsNullExpression (sqlIsNullExpressionWithValue);
      var resultWithEntity = _valueRequiredVisitor.VisitSqlIsNullExpression (sqlIsNullExpressionWithEntity);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNullExpressionWithValue));
      Assert.That (resultWithEntity, Is.Not.SameAs (sqlIsNullExpressionWithValue));

      var expectedResultWithEntity = new SqlIsNullExpression (entityExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultWithEntity, resultWithEntity);
    }

    [Test]
    public void VisitSqlIsNotNullExpression_AppliesSingleValueSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNotNullExpressionWithValue = new SqlIsNotNullExpression (Expression.Constant (1));
      var sqlIsNotNullExpressionWithEntity = new SqlIsNotNullExpression (entityExpression);

      var resultWithValue = _valueRequiredVisitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpressionWithValue);
      var resultWithEntity = _valueRequiredVisitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpressionWithEntity);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNotNullExpressionWithValue));
      Assert.That (resultWithEntity, Is.Not.SameAs (sqlIsNotNullExpressionWithValue));

      var expectedResultWithEntity = new SqlIsNotNullExpression (entityExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultWithEntity, resultWithEntity);
    }

    [Test]
    public void VisitSqlEntityConstantExpression_ValueRequired ()
    {
      var expression = new SqlEntityConstantExpression (typeof (int), 5, Expression.Constant (1));
      var result = _valueRequiredVisitor.VisitSqlEntityConstantExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlEntityConstantExpression_SingleValueRequired_SingleValuePreferred ()
    {
      var primaryKeyExpression = Expression.Constant (1);
      var expression = new SqlEntityConstantExpression (typeof (int), 5, primaryKeyExpression);

      var result1 = _singleValueRequiredVisitor.VisitSqlEntityConstantExpression (expression);
      var result2 = _singleValuePreferredVisitor.VisitSqlEntityConstantExpression (expression);

      Assert.That (result1, Is.SameAs (primaryKeyExpression));
      Assert.That (result2, Is.SameAs (primaryKeyExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var fakeResult = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      _stageMock
          .Expect (mock => mock.ApplySelectionContext (sqlStatement, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _valueRequiredVisitor.VisitSqlSubStatementExpression (sqlSubStatementExpression);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.Not.SameAs (sqlStatement));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ValueSemantic ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, primaryKeyColumn, foreignKeyColumn);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, primaryKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (entityRefMemberExpression, fakeJoinInfo, _mappingResolutionContext))
          .Return (fakeEntityExpression);
      _stageMock.Replay();

      var result = _valueRequiredVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValueRequired_PrimaryKeyColumnOnLeftSide ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, primaryKeyColumn, foreignKeyColumn);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, primaryKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (entityRefMemberExpression, fakeJoinInfo, _mappingResolutionContext))
          .Return (fakeEntityExpression);
      _stageMock.Replay();

      var result = _singleValueRequiredVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (primaryKeyColumn));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValuePreferred_PrimaryKeyColumnOnLeftSide ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, primaryKeyColumn, foreignKeyColumn);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, primaryKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (entityRefMemberExpression, fakeJoinInfo, _mappingResolutionContext))
          .Return (fakeEntityExpression);
      _stageMock.Replay ();

      var result = _singleValuePreferredVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (primaryKeyColumn));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValueRequired_PrimaryKeyColumnOnRightSide ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, foreignKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock.Replay();

      var result = _singleValueRequiredVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (foreignKeyColumn));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValuePreferred_PrimaryKeyColumnOnRightSide ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, foreignKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock.Replay ();

      var result = _singleValuePreferredVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (foreignKeyColumn));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression_PredicateSemantic ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (Arg<UnresolvedJoinInfo>.Is.Anything, Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (null);
      _stageMock.Replay();

      SqlContextExpressionVisitor.ApplySqlExpressionContext (
          entityRefMemberExpression, SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitNamedExpression_AppliesContextToInnerExpression ()
    {
      var innerExpression = new TestExtensionExpressionWithoutChildren (typeof (bool));
      var namedExpression = new NamedExpression ("test", innerExpression);

      var predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);

      var result = predicateRequiredVisitor.VisitNamedExpression (namedExpression);

      Assert.That (result, Is.SameAs (namedExpression));
    }

    [Test]
    public void VisitNamedExpression_ProcessesNamedExpressions ()
    {
      var innermostExpression = new TestExtensionExpressionWithoutChildren (typeof (int));
      var expression = new NamedExpression ("test", new NamedExpression ("test2", innermostExpression));

      var predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);

      var result = predicateRequiredVisitor.VisitNamedExpression (expression);

      var expectedResult = new NamedExpression ("test_test2", innermostExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitNamedExpression_ProcessesNamedExpressions_AfterContextIsApplied ()
    {
      var innermostExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var expression = new NamedExpression ("test", new NamedExpression ("test2", innermostExpression));

      var predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);

      var result = predicateRequiredVisitor.VisitNamedExpression (expression);

      var expectedResult = new NamedExpression ("test_test2", innermostExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitNewExpression_KeepsValueSemantics ()
    {
      var argument = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var expression = Expression.New (
          GetTypeForNewExpressionCtor (typeof (Cook)),
          new[] { argument },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("D"));

      var result = _valueRequiredVisitor.VisitNewExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewExpression_VisitsChildExpressions ()
    {
      var expression = Expression.New (
          GetTypeForNewExpressionCtor (typeof (int)),
          new[] { new NamedExpression ("test", new NamedExpression ("test2", Expression.Constant (0))) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var result = _valueRequiredVisitor.VisitNewExpression (expression);

      var expected = Expression.New (
          expression.Constructor, new[] { new NamedExpression ("test_test2", Expression.Constant (0)) }, expression.Members);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitNewExpression_NoMembers ()
    {
      var expression = Expression.New (GetTypeForNewExpressionCtor (typeof (int)), new[] { Expression.Constant (0) });

      var result = _valueRequiredVisitor.VisitNewExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewExpression_SingleValueRequired_Throws ()
    {
      var expression = CreateNewExpression();

      Assert.That (
          () => _singleValueRequiredVisitor.VisitNewExpression (expression),
          Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo (
                    "Cannot use a complex expression ('new TypeForNewExpression(0)') in a place where SQL requires a single value."));
    }

    [Test]
    public void VisitNewExpression_SingleValuePreferred_Allowed ()
    {
      var expression = CreateNewExpression ();
      
      var result = _singleValuePreferredVisitor.VisitNewExpression (expression);
      
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMethodCallExpression_KeepsValueSemantics ()
    {
      var instance = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var argument = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Restaurant));
      var expression = Expression.Call (instance, ReflectionUtility.GetMethod (() => ((Cook) null).GetSubKitchenCook (null)), argument);

      var result = _singleValueRequiredVisitor.VisitMethodCallExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMethodCallExpression_VisitsChildExpressions ()
    {
      var instance = ExpressionHelper.CreateExpression (typeof (int));
      var argument = ExpressionHelper.CreateExpression (typeof (string));
      var expression = Expression.Call (
          new NamedExpression ("test", new NamedExpression ("test2", instance)),
          ReflectionUtility.GetMethod (() => 5.ToString("arg")),
          new NamedExpression ("test", new NamedExpression ("test2", argument)));

      var result = _predicateRequiredVisitor.VisitMethodCallExpression (expression);

      var expected = Expression.Call (new NamedExpression ("test_test2", instance), expression.Method, new NamedExpression ("test_test2", argument));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitMethodCallExpression_NoObject ()
    {
      var argument = ExpressionHelper.CreateExpression (typeof (string));
      var expression = Expression.Call (ReflectionUtility.GetMethod (() => int.Parse ("arg")), argument);

      var result = _predicateRequiredVisitor.VisitMethodCallExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_KeepsValueSemantics ()
    {
      var keyExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var elementExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var aggregateExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var expression = new SqlGroupingSelectExpression (keyExpression, elementExpression, new[] { aggregateExpression });
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _mappingResolutionContext.AddGroupReferenceMapping (expression, sqlTable);

      var result = _singleValueRequiredVisitor.VisitSqlGroupingSelectExpression (expression);

      Assert.That (result, Is.SameAs (expression));
      Assert.That (_mappingResolutionContext.GetReferencedGroupSource (((SqlGroupingSelectExpression) result)), Is.SameAs (sqlTable));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_VisitsChildren ()
    {
      var keyExpression = new NamedExpression ("test", new NamedExpression ("test2", Expression.Constant (0)));
      var elementExpression = new NamedExpression ("test", new NamedExpression ("test2", Expression.Constant (0)));
      var aggregateExpression = new NamedExpression ("test", new NamedExpression ("test2", Expression.Constant (0)));

      var expression = new SqlGroupingSelectExpression (keyExpression, elementExpression, new[] { aggregateExpression });
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _mappingResolutionContext.AddGroupReferenceMapping (expression, sqlTable);

      var result = (SqlGroupingSelectExpression) _singleValueRequiredVisitor.VisitSqlGroupingSelectExpression (expression);

      Assert.That (result, Is.Not.SameAs (expression));

      var expectedExpression = new NamedExpression ("test_test2", Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result.KeyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result.ElementExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result.AggregationExpressions[0]);
      Assert.That (_mappingResolutionContext.GetReferencedGroupSource (result), Is.SameAs (sqlTable));
    }

    [Test]
    public void VisitSqlTableReferenceExpression ()
    {
      var expression =
          new SqlTableReferenceExpression (new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner));

      var result = _predicateRequiredVisitor.VisitSqlTableReferenceExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var expression = new SqlFunctionExpression (typeof (int), "Test", Expression.Constant (true));
      var expectedResult = new SqlFunctionExpression (typeof (int), "Test", new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlFunctionExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlConvertExpression ()
    {
      var expression = new SqlConvertExpression (typeof (bool), Expression.Constant (true));
      var expectedResult = new SqlConvertExpression (typeof (bool), new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlConvertExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlExistsExpression ()
    {
      var expression = new SqlExistsExpression (Expression.Constant (true));
      var expectedResult = new SqlExistsExpression (new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlExistsExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlExistsExpression_AllowsComplexValuesInChildren ()
    {
      var expression = new SqlExistsExpression (CreateNewExpression());

      var result = _predicateRequiredVisitor.VisitSqlExistsExpression (expression);

      Assert.That(result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlExistsExpression_OptimizesToSingleValueIfPossible ()
    {
      var entityDefinitionExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var expression = new SqlExistsExpression (entityDefinitionExpression);
      var expectedExpression = new SqlExistsExpression (entityDefinitionExpression.PrimaryKeyColumn);

      var result = _predicateRequiredVisitor.VisitSqlExistsExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlLengthExpression ()
    {
      var expression = new SqlLengthExpression (Expression.Constant ("test"));

      var result = _predicateRequiredVisitor.VisitSqlLengthExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlCaseExpression_NoElse ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), Expression.Constant (true));
      var case2 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (false), Expression.Constant (false));
      var expression = new SqlCaseExpression (typeof (bool?), new[] { case1, case2 }, null);

      var result = _valueRequiredVisitor.VisitSqlCaseExpression (expression);

      var expectedCase1 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));
      var expectedCase2 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      var expectedExpression = new SqlCaseExpression (typeof (bool?), new[] { expectedCase1, expectedCase2 }, null);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlCaseExpression_WithElse ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), Expression.Constant (true));
      var case2 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (false), Expression.Constant (false));
      var elseCase = Expression.Constant (true);
      var expression = new SqlCaseExpression (typeof (bool), new[] { case1, case2 }, elseCase);

      var result = _valueRequiredVisitor.VisitSqlCaseExpression (expression);

      var expectedCase1 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));
      var expectedCase2 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      var expectedElseCase = new SqlConvertedBooleanExpression (Expression.Constant (1));
      var expectedExpression = new SqlCaseExpression (typeof (bool), new[] { expectedCase1, expectedCase2 }, expectedElseCase);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlRowNumberExpression ()
    {
      var expression = new SqlRowNumberExpression (new[] { new Ordering (Expression.Constant (true), OrderingDirection.Asc) });
      var expectedResult =
          new SqlRowNumberExpression (new[] { new Ordering (new SqlConvertedBooleanExpression (Expression.Constant (1)), OrderingDirection.Asc) });

      var result = _predicateRequiredVisitor.VisitSqlRowNumberExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult.Orderings[0].Expression, ((SqlRowNumberExpression) result).Orderings[0].Expression);
    }

    [Test]
    public void VisitSqlLiteralExpression ()
    {
      var expression = new SqlLiteralExpression (1);

      var result = _predicateRequiredVisitor.VisitSqlLiteralExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void SqlBinaryOperatorExpression ()
    {
      var expression = new SqlBinaryOperatorExpression (typeof (bool), "IN", Expression.Constant (true), Expression.Constant (true));
      var expectedResult = new SqlBinaryOperatorExpression (
          typeof (bool), "IN", new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlBinaryOperatorExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SqlBinaryOperatorExpression_WithInvalidChildren ()
    {
      var newExpression = CreateNewExpression();
      var expression = new SqlBinaryOperatorExpression (typeof (bool), "IN", newExpression, Expression.Constant (new[] { 1, 2, 3 }));

      Assert.That (
          () => _predicateRequiredVisitor.VisitSqlBinaryOperatorExpression (expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "The SQL 'IN' operator (originally probably a call to a 'Contains' method) requires a single value, so the following expression "
              + "cannot be translated to SQL: 'new TypeForNewExpression(0) IN value(System.Int32[])'. Cannot use a complex expression "
              + "('new TypeForNewExpression(0)') in a place where SQL requires a single value."));
    }

    [Test]
    public void SqlLikeExpression ()
    {
      var expression = new SqlLikeExpression (Expression.Constant (true), Expression.Constant (true), new SqlLiteralExpression (@"\"));
      var expectedResult = new SqlLikeExpression (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlLiteralExpression (@"\"));

      var result = _predicateRequiredVisitor.VisitSqlLikeExpression (expression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    
    [Test]
    public void InvocationExpression ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Constant (0));
      var invocationExpression = Expression.Invoke (lambdaExpression);

      Assert.That (
          () => _predicateRequiredVisitor.VisitExpression (invocationExpression), 
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
              "InvocationExpressions are not supported in the SQL backend. Expression: 'Invoke(() => 0)'."));
    }

    [Test]
    public void LambdaExpression ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Constant (0));

      Assert.That (
          () => _predicateRequiredVisitor.VisitExpression (lambdaExpression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "LambdaExpressions are not supported in the SQL backend. Expression: '() => 0'."));
    }

    private static bool FakeAndOperator ([UsedImplicitly] bool operand1, [UsedImplicitly] bool operand2)
    {
      throw new NotImplementedException();
    }

    private static SqlCaseExpression GetNonNullablePredicateAsValueExpression (Expression expression)
    {
      return SqlCaseExpression.CreateIfThenElse (typeof (int), expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
    }

    private static SqlCaseExpression GetNullablePredicateAsValueExpression (Expression expression)
    {
      return SqlCaseExpression.CreateIfThenElseNull (typeof (int?), expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
    }

    private static bool MethodWithBoolResult ()
    {
      throw new NotImplementedException();
    }

    private static ConstructorInfo GetTypeForNewExpressionCtor (params Type[] parameterTypes)
    {
      return typeof (TypeForNewExpression).GetConstructor (parameterTypes);
    }

    private static NewExpression CreateNewExpression ()
    {
      return Expression.New (GetTypeForNewExpressionCtor (typeof (int)), Expression.Constant (0));
    }

  }
}