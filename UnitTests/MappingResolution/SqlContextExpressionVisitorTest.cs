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
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Utilities;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SqlContextExpressionVisitorTest
  {
    private TestableSqlContextExpressionVisitor _valueRequiredVisitor;
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private TestableSqlContextExpressionVisitor _singleValueRequiredVisitor;
    private TestableSqlContextExpressionVisitor _predicateRequiredVisitor;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _mappingResolutionContext = new MappingResolutionContext();
      _valueRequiredVisitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);
      _singleValueRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired,
          _stageMock.Object,
          _mappingResolutionContext);
      _predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext);
    }

    [Test]
    public void ApplyContext ()
    {
      var valueExpression = Expression.Constant (0);
      var predicateExpression = Expression.Constant (true);

      var convertedValue = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          new SqlConvertedBooleanExpression (valueExpression), SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext);
      var convertedPredicate = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          predicateExpression, SqlExpressionContext.SingleValueRequired, _stageMock.Object, _mappingResolutionContext);

      var expectedConvertedValue = Expression.Equal (valueExpression, new SqlLiteralExpression (1));
      var expectedConvertedPredicate = new SqlConvertedBooleanExpression (Expression.Constant (1));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedValue, convertedValue);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedPredicate, convertedPredicate);
    }

    [Test]
    public void ApplyContext_SemanticsPropagatedToChildExpressionsByDefault ()
    {
      var expressionOfCorrectType = new ReducibleExtensionExpression (new TestExtensionExpressionWithoutChildren (typeof (bool)));
      var expressionOfIncorrectType =
          new ReducibleExtensionExpression (new SqlConvertedBooleanExpression (new TestExtensionExpressionWithoutChildren (typeof (int))));

      var result1 = _predicateRequiredVisitor.Visit (expressionOfCorrectType);
      var result2 = _predicateRequiredVisitor.Visit (expressionOfIncorrectType);

      Assert.That (result1, Is.SameAs (expressionOfCorrectType));

      var expectedResult2 = new ReducibleExtensionExpression (
          Expression.Equal (((SqlConvertedBooleanExpression) expressionOfIncorrectType.Expression).Expression, new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult2, result2);
    }

    [Test]
    public void Visit_Null_Ignored ()
    {
      var result = _valueRequiredVisitor.Visit ((Expression) null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void Visit_CallsNodeSpecificVisitMethods ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock.Object, _mappingResolutionContext);
      Assert.That (() => visitor.Visit (entityExpression), Throws.TypeOf<NotSupportedException>());
    }

    [Test]
    public void Visit_ValueRequired_ConvertsBool_ToValue ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);
      var resultNullable = visitor.Visit (nullableExpression);

      var expected = new SqlConvertedBooleanExpression (GetNonNullablePredicateAsValueExpression (expression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = new SqlConvertedBooleanExpression (GetNullablePredicateAsValueExpression (nullableExpression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    public void Visit_SingleValueRequired_ConvertsBool_ToSingleValue ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);
      var resultNullable = visitor.Visit (nullableExpression);

      var expected = new SqlConvertedBooleanExpression (GetNonNullablePredicateAsValueExpression (expression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = new SqlConvertedBooleanExpression (GetNullablePredicateAsValueExpression (nullableExpression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    public void Visit_ValueRequired_LeavesExistingValue ()
    {
      var expression = CreateNewExpression ();

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Visit_SingleValueRequired_LeavesExistingSingleValue ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Visit_ValueSemantics_LeavesExistingSqlConvertedBooleanExpression ()
    {
      var convertedBooleanExpression = new SqlConvertedBooleanExpression (Expression.Constant (1));

      var result = _valueRequiredVisitor.Visit (convertedBooleanExpression);

      Assert.That (result, Is.SameAs (convertedBooleanExpression));
    }

    [Test]
    public void Visit_ValueSemantics_LeavesMethodCallExpression ()
    {
      var methodWithBoolResultExpression = Expression.Call (MemberInfoFromExpressionUtility.GetMethod (() => MethodWithBoolResult()));

      var result = _valueRequiredVisitor.Visit (methodWithBoolResultExpression);

      Assert.That (result, Is.SameAs (methodWithBoolResultExpression));
    }

    [Test]
    public void Visit_LeavesExistingPredicate ()
    {
      var expression = new CustomExpression (typeof (bool));
      var nullableExpression = new CustomExpression (typeof (bool?));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);
      var resultNullable = visitor.Visit (nullableExpression);

      Assert.That (result, Is.SameAs (expression));
      Assert.That (resultNullable, Is.SameAs (nullableExpression));
    }

    [Test]
    public void Visit_ConvertedInt_ToPredicate ()
    {
      var expression = new SqlConvertedBooleanExpression (new CustomExpression (typeof (int)));
      var nullableExpression = new SqlConvertedBooleanExpression (new CustomExpression (typeof (int?)));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (expression);
      var resultNullable = visitor.Visit (nullableExpression);

      var expected = Expression.Equal (expression.Expression, new SqlLiteralExpression (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
      var expectedNullable = Expression.Equal (nullableExpression.Expression, new SqlLiteralExpression (1, true), true, null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullable, resultNullable);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot convert an expression of type 'System.String' to a boolean expression. Expression: 'CustomExpression'")]
    public void Visit_ThrowsOnNonConvertible_ToPredicate ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext);
      visitor.Visit (expression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Invalid enum value: -1")]
    public void Visit_ThrowsOnInvalidContext ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor ((SqlExpressionContext) (-1), _stageMock.Object, _mappingResolutionContext);
      visitor.Visit (expression);
    }

    [Test]
    public void Visit_AppliesSpecifiedSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.Visit (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitSqlConvertedBooleanExpression_InnerUnchanged ()
    {
      var converted = new SqlConvertedBooleanExpression (Expression.Constant (0));

      var result = _valueRequiredVisitor.VisitSqlConvertedBoolean (converted);

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

      var resultTrue = _valueRequiredVisitor.VisitConstant (constantTrue);
      var resultFalse = _valueRequiredVisitor.VisitConstant (constantFalse);
      var resultNullableTrue = _valueRequiredVisitor.VisitConstant (constantNullableTrue);
      var resultNullableFalse = _valueRequiredVisitor.VisitConstant (constantNullableFalse);
      var resultNullableNull = _valueRequiredVisitor.VisitConstant (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_SingleValueRequired ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _singleValueRequiredVisitor.VisitConstant (constantTrue);
      var resultFalse = _singleValueRequiredVisitor.VisitConstant (constantFalse);
      var resultNullableTrue = _singleValueRequiredVisitor.VisitConstant (constantNullableTrue);
      var resultNullableFalse = _singleValueRequiredVisitor.VisitConstant (constantNullableFalse);
      var resultNullableNull = _singleValueRequiredVisitor.VisitConstant (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants_PredicateRequired ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);
      var constantNullableTrue = Expression.Constant (true, typeof (bool?));
      var constantNullableFalse = Expression.Constant (false, typeof (bool?));
      var constantNullableNull = Expression.Constant (null, typeof (bool?));

      var resultTrue = _predicateRequiredVisitor.VisitConstant (constantTrue);
      var resultFalse = _predicateRequiredVisitor.VisitConstant (constantFalse);
      var resultNullableTrue = _predicateRequiredVisitor.VisitConstant (constantNullableTrue);
      var resultNullableFalse = _predicateRequiredVisitor.VisitConstant (constantNullableFalse);
      var resultNullableNull = _predicateRequiredVisitor.VisitConstant (constantNullableNull);

      var expectedExpressionTrue = new SqlConvertedBooleanExpression (Expression.Constant (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);
      var expectedExpressionFalse = new SqlConvertedBooleanExpression (Expression.Constant (0));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
      var expectedExpressionNullableTrue = new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableTrue, resultNullableTrue);
      var expectedExpressionNullableFalse = new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableFalse, resultNullableFalse);
      var expectedExpressionNullableNull = new SqlConvertedBooleanExpression (Expression.Constant (null, typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionNullableNull, resultNullableNull);
    }

    [Test]
    public void VisitConstantExpression_OtherConstants ()
    {
      var constant = Expression.Constant ("hello");

      var result = _valueRequiredVisitor.Visit (constant);

      Assert.That (result, Is.SameAs (constant));
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_NoPrimaryColumn_ValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);

      var result = _valueRequiredVisitor.VisitSqlColumn (column);
      var nullableResult = _valueRequiredVisitor.VisitSqlColumn (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_IsPrimaryColumn_ValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", true);

      var result = _valueRequiredVisitor.VisitSqlColumn (column);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", true));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_SingleValueRequired ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);

      var result = _singleValueRequiredVisitor.VisitSqlColumn (column);
      var nullableResult = _singleValueRequiredVisitor.VisitSqlColumn (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_PredicateRequired ()
    {
      var nullableColumn = new SqlColumnDefinitionExpression (typeof (bool?), "x", "y", false);
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);

      var result = _predicateRequiredVisitor.VisitSqlColumn (column);
      var nullableResult = _predicateRequiredVisitor.VisitSqlColumn (nullableColumn);

      var expectedExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int?), "x", "y", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitSqlColumnExpression_OtherColumn ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (string), "x", "y", false);

      var result = _valueRequiredVisitor.VisitSqlColumn (column);

      Assert.That (result, Is.SameAs (column));
    }

    [Test]
    public void VisitSqlEntityExpression_WithSingleValueSemantics_Throws ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "t");

      Assert.That (
          () => _singleValueRequiredVisitor.VisitSqlEntity (entityExpression), 
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
            "Cannot use an entity expression ('[t]' of type 'Cook') in a place where SQL requires a single value."));
    }

    [Test]
    public void VisitSqlEntityExpression_WithNonSingleValueSemantics_LeavesEntity ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);
      var result = visitor.VisitSqlEntity (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToValue_ForEqual ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.Equal (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)), true, null);

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.Equal (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.Equal (
          new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?))),
          new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?))),
          true,
          null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_RequiresSingleValue_ForEqual ()
    {
      var complexExpressionLeft = Expression.Equal (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)), new CustomExpression (typeof (Cook)));
      var complexExpressionRight = Expression.Equal (
          new CustomExpression (typeof (Cook)), SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)));

      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionLeft), Throws.TypeOf<NotSupportedException> ());
      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionRight), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToValue_ForNotEqual ()
    {
      var expression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.NotEqual (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)), true, null);

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.NotEqual (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.NotEqual (
          new SqlConvertedBooleanExpression (Expression.Constant (1, typeof (int?))),
          new SqlConvertedBooleanExpression (Expression.Constant (0, typeof (int?))),
          true,
          null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_RequiresSingleValue_ForNotEqual ()
    {
      var complexExpressionLeft = Expression.NotEqual (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)), new CustomExpression (typeof (Cook)));
      var complexExpressionRight = Expression.NotEqual (
          new CustomExpression (typeof (Cook)), SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)));

      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionLeft), Throws.TypeOf<NotSupportedException> ());
      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionRight), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var expression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.AndAlso (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOrElse ()
    {
      var expression = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.OrElse (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAnd ()
    {
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.And (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.And (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOr ()
    {
      var expression = Expression.Or (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.Or (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForExclusiveOr ()
    {
      var expression = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));
      var nullableExpression = Expression.ExclusiveOr (Expression.Constant (true, typeof (bool?)), Expression.Constant (false, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);

      var expectedExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
          Expression.Equal (Expression.Constant (0, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_PassesMethod ()
    {
      var operatorMethod = MemberInfoFromExpressionUtility.GetMethod (() => FakeAndOperator(false, false));
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false), operatorMethod);
      Assert.That (expression.Method, Is.Not.Null);

      var result = _valueRequiredVisitor.VisitBinary (expression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)),
          operatorMethod);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_Coalesce ()
    {
      // nullableBool ?? bool is converted not as ConvertedBool (nullableInt) ?? ConvertedBool (int), but as ConvertedBool (nullableInt ?? int).
      var expression = Expression.Coalesce (Expression.Constant (false, typeof (bool?)), Expression.Constant (true));
      var nullableExpression = Expression.Coalesce (Expression.Constant (false, typeof (bool?)), Expression.Constant (true, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitBinary (expression);
      var nullableResult = _valueRequiredVisitor.VisitBinary (nullableExpression);
      var resultForPredicateSemantics = _predicateRequiredVisitor.VisitBinary (expression);

      var expectedExpression = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1, typeof (int?))));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, nullableResult);
      var expectedResultForPredicateSemantics = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (0, typeof (int?)),
          Expression.Constant (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResultForPredicateSemantics, resultForPredicateSemantics);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_Coalesce_OptimizationForCoalesceToFalse ()
    {
      // With predicate semantics, nullableBool ?? false is optimized to be the same as Convert (nullableBool, typeof (bool)) because SQL handles 
      // NULL in a falsey way in predicates, and the generated SQL is nicer.
      // With value semantics, this is not optimized.

      var expression = Expression.Coalesce (Expression.Constant (true, typeof (bool?)), Expression.Constant (false));

      var resultForValueSemantics = _valueRequiredVisitor.VisitBinary (expression);
      var resultForPredicateSemantics = _predicateRequiredVisitor.VisitBinary (expression);

      var expectedExpressionForValueSemantics = new SqlConvertedBooleanExpression (Expression.Coalesce (
          Expression.Constant (1, typeof (int?)),
          Expression.Constant (0)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionForValueSemantics, resultForValueSemantics);
      var expectedResultForPredicateSemantics =
          Expression.Convert (
              Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null),
              typeof (bool));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResultForPredicateSemantics, resultForPredicateSemantics);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_RequiresSingleValue_ForCoalesce ()
    {
      var complexExpressionLeft = Expression.Coalesce (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)), new CustomExpression (typeof (Cook)));
      var complexExpressionRight = Expression.Coalesce (
          new CustomExpression (typeof (Cook)), SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)));

      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionLeft), Throws.TypeOf<NotSupportedException> ());
      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionRight), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_Unchanged ()
    {
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitBinary (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_OperandsGetSingleValueSemantics_SingleValueAllowed ()
    {
      var binary = BinaryExpression.And (Expression.Convert (Expression.Not (Expression.Constant (true)), typeof (int)), Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitBinary (binary);

      var expectedExpression =
          BinaryExpression.And (
              Expression.Convert (
                  new SqlConvertedBooleanExpression (
                      GetNonNullablePredicateAsValueExpression (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))))),
                  typeof (int)),
              Expression.Constant (5));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_OperandsGetSingleValueSemantics_ComplexValueNotAllowed ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var other = new CustomExpression (typeof (Cook));
      var complexExpressionLeft = BinaryExpression.And (entity, other, MemberInfoFromExpressionUtility.GetMethod (() => FakeAndOperator (null, null)));
      var complexExpressionRight = BinaryExpression.And (other, entity, MemberInfoFromExpressionUtility.GetMethod (() => FakeAndOperator (null, null)));

      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionLeft), Throws.TypeOf<NotSupportedException> ());
      Assert.That (() => _valueRequiredVisitor.VisitBinary (complexExpressionRight), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitUnaryExpression_UnaryBoolExpression_Not_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));
      var unaryNullableExpression = Expression.Not (Expression.Constant (true, typeof (bool?)));

      var result = _valueRequiredVisitor.VisitUnary (unaryExpression);
      var resultNullable = _valueRequiredVisitor.VisitUnary (unaryNullableExpression);

      var expectedExpression = Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression =
          Expression.Not (Expression.Equal (Expression.Constant (1, typeof (int?)), new SqlLiteralExpression (1, true), true, null));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, resultNullable);
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_OperandChanged ()
    {
      var unaryExpression = Expression.Convert (Expression.Constant (true), typeof (object));

      var result = _singleValueRequiredVisitor.VisitUnary (unaryExpression);

      Assert.That (result, Is.Not.SameAs (unaryExpression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Convert));
      Assert.That (((UnaryExpression) result).Operand, Is.TypeOf<SqlConvertedBooleanExpression>());
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_SameOperand ()
    {
      var unaryExpression = Expression.Convert (SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)), typeof (object));

      var result = _valueRequiredVisitor.VisitUnary (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_Unchanged ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (5));

      var result = _valueRequiredVisitor.VisitUnary (unaryExpression);

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

      var result = _valueRequiredVisitor.VisitUnary (unaryExpression);

      var expectedExpression =
          Expression.Not (
              Expression.Convert (
                  new SqlConvertedBooleanExpression (
                    GetNonNullablePredicateAsValueExpression (Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))))),
                  typeof (int)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnaryExpression_SqlConvertedBooleanExpression ()
    {
      var convertToBoolExpression = Expression.Convert (Expression.Constant (true, typeof (bool?)), typeof (bool));
      var convertToNullableBoolExpression = Expression.Convert (Expression.Constant (true), typeof (bool?));

      var result = _valueRequiredVisitor.VisitUnary (convertToBoolExpression);
      var resultNullable = _valueRequiredVisitor.VisitUnary (convertToNullableBoolExpression);

      var expectedExpression = new SqlConvertedBooleanExpression (Expression.Convert (Expression.Constant (1, typeof (int?)), typeof (int)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
      var expectedNullableExpression = new SqlConvertedBooleanExpression (Expression.Convert (Expression.Constant (1), typeof (int?)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNullableExpression, resultNullable);
    }

    [Test]
    public void VisitSqlIsNullExpression_AppliesSingleValueSemantics_AllowsSingleValue ()
    {
      var sqlIsNullExpressionWithValue = new SqlIsNullExpression (Expression.Constant (1));

      var resultWithValue = _valueRequiredVisitor.VisitSqlIsNull (sqlIsNullExpressionWithValue);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNullExpressionWithValue));
    }

    [Test]
    public void VisitSqlIsNullExpression_AppliesSingleValueSemantics_ThrowsForComplexValue ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNullExpressionWithEntity = new SqlIsNullExpression (entityExpression);

      Assert.That (() => _valueRequiredVisitor.VisitSqlIsNull (sqlIsNullExpressionWithEntity), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitSqlIsNotNullExpression_AppliesSingleValueSemantics_AllowsSingleValue ()
    {
      var sqlIsNotNullExpressionWithValue = new SqlIsNotNullExpression (Expression.Constant (1));

      var resultWithValue = _valueRequiredVisitor.VisitSqlIsNotNull (sqlIsNotNullExpressionWithValue);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNotNullExpressionWithValue));
    }

    [Test]
    public void VisitSqlIsNotNullExpression_AppliesSingleValueSemantics_ThrowsForComplexValue ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNotNullExpressionWithEntity = new SqlIsNotNullExpression (entityExpression);

      Assert.That (() => _valueRequiredVisitor.VisitSqlIsNotNull (sqlIsNotNullExpressionWithEntity), Throws.TypeOf<NotSupportedException> ());
    }

    [Test]
    public void VisitSqlEntityConstantExpression_ValueRequired ()
    {
      var expression = new SqlEntityConstantExpression (typeof (int), 5, Expression.Constant (1));
      var result = _valueRequiredVisitor.VisitSqlEntityConstant (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlEntityConstantExpression_Throws ()
    {
      var entityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), new Cook(), Expression.Constant (0));

      Assert.That (
          () => _singleValueRequiredVisitor.VisitSqlEntityConstant (entityConstantExpression), 
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
            "Cannot use an entity constant ('ENTITY(0)' of type 'Cook') in a place where SQL requires a single value."));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var fakeResult = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      _stageMock
         .Setup (mock => mock.ApplySelectionContext (sqlStatement, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
         .Returns (fakeResult)
         .Verifiable ();

      var result = _valueRequiredVisitor.VisitSqlSubStatement (sqlSubStatementExpression);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.Not.SameAs (sqlStatement));
      _stageMock.Verify();
    }

    [Test]
    public void VisitNewExpression_KeepsValueSemantics ()
    {
      var argument = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (Cook)),
          new[] { argument },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("D"));

      var result = _valueRequiredVisitor.VisitNew (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewExpression_VisitsChildExpressions ()
    {
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (bool)),
          new[] { Expression.Constant (false) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("E"));

      var result = _valueRequiredVisitor.VisitNew (expression);

      var expected = Expression.New (
          expression.Constructor, 
          new[] { new SqlConvertedBooleanExpression ( Expression.Constant (0)) }, 
          expression.Members);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitNewExpression_NoMembers ()
    {
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { Expression.Constant (0) });

      var result = _valueRequiredVisitor.VisitNew (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewExpression_SingleValueRequired_Throws ()
    {
      var expression = CreateNewExpression();

      Assert.That (
          () => _singleValueRequiredVisitor.VisitNew (expression),
          Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo (
                    "Cannot use a complex expression ('new TypeForNewExpression(0)') in a place where SQL requires a single value."));
    }

    [Test]
    public void VisitMethodCallExpression_KeepsValueSemantics ()
    {
      var instance = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var argument = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Restaurant));
      var expression = Expression.Call (instance, MemberInfoFromExpressionUtility.GetMethod (() => ((Cook) null).GetSubKitchenCook (null)), argument);

      var result = _singleValueRequiredVisitor.VisitMethodCall (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMethodCallExpression_VisitsChildExpressions ()
    {
      var instance = Expression.Constant (false);
      var argument = Expression.Constant (true);
      var expression = Expression.Call (
          instance,
          MemberInfoFromExpressionUtility.GetMethod (() => false.CompareTo (true)),
          argument);

      var result = _predicateRequiredVisitor.VisitMethodCall (expression);

      var expected = Expression.Call (
          new SqlConvertedBooleanExpression (Expression.Constant (0)), 
          expression.Method, 
          new SqlConvertedBooleanExpression (Expression.Constant (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitMethodCallExpression_NoObject ()
    {
      var argument = ExpressionHelper.CreateExpression (typeof (string));
      var expression = Expression.Call (MemberInfoFromExpressionUtility.GetMethod (() => int.Parse ("arg")), argument);

      var result = _predicateRequiredVisitor.VisitMethodCall (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNamedExpression_MovesNameIntoSqlConvertedBooleanExpression ()
    {
      var namedExpression = new NamedExpression ("Name", Expression.Constant (true));

      var result = _valueRequiredVisitor.Visit (namedExpression);

      var expected = new SqlConvertedBooleanExpression (new NamedExpression ("Name", Expression.Constant (1)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
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

      var result = _singleValueRequiredVisitor.VisitSqlGroupingSelect (expression);

      Assert.That (result, Is.SameAs (expression));
      Assert.That (_mappingResolutionContext.GetReferencedGroupSource (((SqlGroupingSelectExpression) result)), Is.SameAs (sqlTable));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_VisitsChildren ()
    {
      var keyExpression = Expression.Constant (false);
      var elementExpression = Expression.Constant (false);
      var aggregateExpression = Expression.Constant (true);

      var expression = new SqlGroupingSelectExpression (keyExpression, elementExpression, new[] { aggregateExpression });
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _mappingResolutionContext.AddGroupReferenceMapping (expression, sqlTable);

      var result = (SqlGroupingSelectExpression) _singleValueRequiredVisitor.VisitSqlGroupingSelect (expression);

      Assert.That (result, Is.Not.SameAs (expression));

      SqlExpressionTreeComparer.CheckAreEqualTrees (new SqlConvertedBooleanExpression (Expression.Constant (0)), result.KeyExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (new SqlConvertedBooleanExpression (Expression.Constant (0)), result.ElementExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (new SqlConvertedBooleanExpression (Expression.Constant (1)), result.AggregationExpressions[0]);
      Assert.That (_mappingResolutionContext.GetReferencedGroupSource (result), Is.SameAs (sqlTable));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var expression = new SqlFunctionExpression (typeof (int), "Test", Expression.Constant (true));
      var expectedResult = new SqlFunctionExpression (typeof (int), "Test", new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlFunction (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlConvertExpression ()
    {
      var expression = new SqlConvertExpression (typeof (bool), Expression.Constant (true));
      var expectedResult = new SqlConvertExpression (typeof (bool), new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlConvert (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlExistsExpression ()
    {
      var expression = new SqlExistsExpression (Expression.Constant (true));
      var expectedResult = new SqlExistsExpression (new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlExists (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlExistsExpression_AllowsComplexValuesInChildren ()
    {
      var expression = new SqlExistsExpression (CreateNewExpression());

      var result = _predicateRequiredVisitor.VisitSqlExists (expression);

      Assert.That(result, Is.SameAs (expression));
    }

    [Test]
    public void SqlLikeExpression ()
    {
      var expression = new SqlLikeExpression (Expression.Constant (true), Expression.Constant (true), new SqlLiteralExpression (@"\"));
      var expectedResult = new SqlLikeExpression (
          new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlLiteralExpression (@"\"));

      var result = _predicateRequiredVisitor.VisitSqlLike (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitSqlLengthExpression ()
    {
      var expression = new SqlLengthExpression (Expression.Constant ("test"));

      var result = _predicateRequiredVisitor.VisitSqlLength (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlCaseExpression_AppliesValueContext ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), Expression.Constant (true));
      var case2 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (false), Expression.Constant (false));
      var elseCase = Expression.Constant (true);
      var expression = new SqlCaseExpression (typeof (bool), new[] { case1, case2 }, elseCase);

      var result = _valueRequiredVisitor.VisitSqlCase (expression);

      var expectedCase1 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));
      var expectedCase2 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      var expectedElseCase = new SqlConvertedBooleanExpression (Expression.Constant (1));
      var expectedExpression = new SqlCaseExpression (typeof (bool), new[] { expectedCase1, expectedCase2 }, expectedElseCase);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlCaseExpression_RequiresSingleValue_InThen ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var nonEntity = Expression.Constant (null, typeof (Cook));

      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), entity);
      var elseCase = nonEntity;
      var expression = new SqlCaseExpression (typeof (Cook), new[] { case1 }, elseCase);

      Assert.That (
          () => _valueRequiredVisitor.VisitSqlCase (expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "Cannot use an entity expression ('[t0]' of type 'Cook') in a place where SQL requires a single value."));
    }

    [Test]
    public void VisitSqlCaseExpression_RequiresSingleValue_InElse ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var nonEntity = Expression.Constant (null, typeof (Cook));

      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), nonEntity);
      var elseCase = entity;
      var expression = new SqlCaseExpression (typeof (Cook), new[] { case1 }, elseCase);

      Assert.That (
          () => _valueRequiredVisitor.VisitSqlCase (expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "Cannot use an entity expression ('[t0]' of type 'Cook') in a place where SQL requires a single value."));
    }
    
    [Test]
    public void VisitSqlCaseExpression_NoElse ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), Expression.Constant (true));
      var case2 = new SqlCaseExpression.CaseWhenPair (Expression.Constant (false), Expression.Constant (false));
      var expression = new SqlCaseExpression (typeof (bool?), new[] { case1, case2 }, null);

      var result = _valueRequiredVisitor.VisitSqlCase (expression);

      var expectedCase1 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));
      var expectedCase2 = new SqlCaseExpression.CaseWhenPair (
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)), new SqlConvertedBooleanExpression (Expression.Constant (0)));
      var expectedExpression = new SqlCaseExpression (typeof (bool?), new[] { expectedCase1, expectedCase2 }, null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlRowNumberExpression ()
    {
      var expression = new SqlRowNumberExpression (new[] { new Ordering (Expression.Constant (true), OrderingDirection.Asc) });
      var expectedResult =
          new SqlRowNumberExpression (new[] { new Ordering (new SqlConvertedBooleanExpression (Expression.Constant (1)), OrderingDirection.Asc) });

      var result = _predicateRequiredVisitor.VisitSqlRowNumber (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult.Orderings[0].Expression, ((SqlRowNumberExpression) result).Orderings[0].Expression);
    }

    [Test]
    public void VisitSqlLiteralExpression ()
    {
      var expression = new SqlLiteralExpression (1);

      var result = _predicateRequiredVisitor.VisitSqlLiteral (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void SqlInExpression ()
    {
      var expression = new SqlInExpression (Expression.Constant (true), Expression.Constant (true));
      var expectedResult = new SqlInExpression (new SqlConvertedBooleanExpression (Expression.Constant (1)), new SqlConvertedBooleanExpression (Expression.Constant (1)));

      var result = _predicateRequiredVisitor.VisitSqlIn (expression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SqlInExpression_WithInvalidChildren ()
    {
      var newExpression = CreateNewExpression();
      var expression = new SqlInExpression (newExpression, Expression.Constant (new[] { 1, 2, 3 }));

      Assert.That (
          () => _predicateRequiredVisitor.VisitSqlIn (expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "The SQL 'IN' operator (originally probably a call to a 'Contains' method) requires a single value, so the following expression "
              + "cannot be translated to SQL: 'new TypeForNewExpression(0) IN value(System.Int32[])'."));
    }


    [Test]
    public void VisitAggregationExpression_AppliesValueContextToInnerExpression ()
    {
      var aggregationExpression = new AggregationExpression (typeof (int), Expression.Constant (true), AggregationModifier.Count);

      var result = _singleValueRequiredVisitor.Visit (aggregationExpression);

      var expected = new AggregationExpression (typeof (int), new SqlConvertedBooleanExpression (Expression.Constant (1)), AggregationModifier.Count);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitAggregationExpression_ComplexValue_AcceptedByCount ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();
      var aggregationExpression = new AggregationExpression (typeof (int), entity, AggregationModifier.Count);

      var result = _singleValueRequiredVisitor.Visit (aggregationExpression);

      Assert.That (result, Is.SameAs (aggregationExpression));
    }

    [Test]
    public void VisitAggregationExpression_ComplexValue_ThrowsWithOtherAggregations ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression ();
      var aggregationExpression = new AggregationExpression (typeof (int), entity, AggregationModifier.Sum);

      Assert.That (() => _valueRequiredVisitor.Visit (aggregationExpression), Throws.TypeOf<NotSupportedException>());
    }

    [Test]
    public void InvocationExpression ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Constant (0));
      var invocationExpression = Expression.Invoke (lambdaExpression);

      Assert.That (
          () => _predicateRequiredVisitor.Visit (invocationExpression), 
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
              "InvocationExpressions are not supported in the SQL backend. Expression: 'Invoke(() => 0)'."));
    }

    [Test]
    public void LambdaExpression ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Constant (0));

      Assert.That (
          () => _predicateRequiredVisitor.Visit (lambdaExpression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "LambdaExpressions are not supported in the SQL backend. Expression: '() => 0'."));
    }

    private static bool FakeAndOperator ([UsedImplicitly] bool operand1, [UsedImplicitly] bool operand2)
    {
      throw new NotImplementedException();
    }

    private static Cook FakeAndOperator ([UsedImplicitly] Cook operand1, [UsedImplicitly] Cook operand2)
    {
      throw new NotImplementedException ();
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

    private static NewExpression CreateNewExpression ()
    {
      return Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), Expression.Constant (0));
    }

  }
}