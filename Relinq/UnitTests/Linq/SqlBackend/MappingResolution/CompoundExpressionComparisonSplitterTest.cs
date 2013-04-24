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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class CompoundExpressionComparisonSplitterTest
  {
    private CompoundExpressionComparisonSplitter _compoundExpressionComparisonSplitter;

    [SetUp]
    public void SetUp ()
    {
      _compoundExpressionComparisonSplitter = new CompoundExpressionComparisonSplitter();
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NoNewExpressions ()
    {
      var leftExpression = Expression.Constant (1);
      var rightExpression = Expression.Constant (1);
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The results of constructor invocations can only be compared if the same constructors are used for both invocations. Expressions: "
        + "'new TypeForNewExpression(1)', 'new TypeForNewExpression(1, 2)'")]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionsWithDifferentCtors_ThrowsException ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var rightArgumentExpression1 = Expression.Constant (1);
      var rightArgumentExpression2 = Expression.Constant (2);
      var leftExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), leftArgumentExpression);
      var rightExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), rightArgumentExpression1, rightArgumentExpression2);
      var expression = Expression.Equal (leftExpression, rightExpression);

      _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionWithOneArgument_ReturnsBinaryExpressionSequence ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var rightArgumentExpression = Expression.Constant (1);
      var leftExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), leftArgumentExpression);
      var rightExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), rightArgumentExpression);
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.Equal (leftArgumentExpression, rightArgumentExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionWithTwoArguments_Equal_ReturnsBinaryExpressionSequence ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var rightArgumentExpression1 = Expression.Constant (1);
      var rightArgumentExpression2 = Expression.Constant (2);
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), leftArgumentExpression1, leftArgumentExpression2);
      var rightExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), rightArgumentExpression1, rightArgumentExpression2);
      var expression = Expression.MakeBinary (ExpressionType.Equal, leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedLeftSideExpression = Expression.Equal (leftArgumentExpression1, rightArgumentExpression1);
      var expectedRightSideExpression = Expression.Equal (leftArgumentExpression2, rightArgumentExpression2);
      var expectedResult = Expression.AndAlso (expectedLeftSideExpression, expectedRightSideExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionWithTwoArguments_NotEqual_ReturnsBinaryExpressionSequence ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var rightArgumentExpression1 = Expression.Constant (1);
      var rightArgumentExpression2 = Expression.Constant (2);
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), leftArgumentExpression1, leftArgumentExpression2);
      var rightExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), rightArgumentExpression1, rightArgumentExpression2);
      var expression = Expression.MakeBinary (ExpressionType.NotEqual, leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedLeftSideExpression = Expression.NotEqual (leftArgumentExpression1, rightArgumentExpression1);
      var expectedRightSideExpression = Expression.NotEqual (leftArgumentExpression2, rightArgumentExpression2);
      var expectedResult = Expression.OrElse (expectedLeftSideExpression, expectedRightSideExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }


    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Compound values can only be compared if the respective constructor invocation has members associated with it. Expressions: "
            + "'new TypeForNewExpression(1)', 'value(Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests.TypeForNewExpression)'")]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithoutMembers_ThrowsException ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), leftArgumentExpression);
      var newConstantExpression = Expression.Constant (new TypeForNewExpression (0));
      var expression = Expression.Equal (leftExpression, newConstantExpression);

      _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithOnePropertyInfoMember ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var leftArgumentMemberInfo = typeof (TypeForNewExpression).GetProperty ("A");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { leftArgumentExpression }, leftArgumentMemberInfo);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.Equal (leftArgumentExpression, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetProperty ("A")));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithTwoPropertyInfoMembers_Equal ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var leftArgumentMemberInfo1 = typeof (TypeForNewExpression).GetProperty ("A");
      var leftArgumentMemberInfo2 = typeof (TypeForNewExpression).GetProperty ("B");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)),
          new[] { leftArgumentExpression1, leftArgumentExpression2 },
          leftArgumentMemberInfo1,
          leftArgumentMemberInfo2);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult =
          Expression.AndAlso (
              Expression.Equal (
                  leftArgumentExpression1, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetProperty ("A"))),
              Expression.Equal (
                  leftArgumentExpression2, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetProperty ("B"))));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithTwoPropertyInfoMembers_NotEqual ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var leftArgumentMemberInfo1 = typeof (TypeForNewExpression).GetProperty ("A");
      var leftArgumentMemberInfo2 = typeof (TypeForNewExpression).GetProperty ("B");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)),
          new[] { leftArgumentExpression1, leftArgumentExpression2 },
          leftArgumentMemberInfo1,
          leftArgumentMemberInfo2);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.NotEqual (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult =
          Expression.OrElse (
              Expression.NotEqual (
                  leftArgumentExpression1, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetProperty ("A"))),
              Expression.NotEqual (
                  leftArgumentExpression2, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetProperty ("B"))));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithOneFieldInfoMembers ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var leftArgumentMemberInfo = typeof (TypeForNewExpression).GetField ("C");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { leftArgumentExpression }, leftArgumentMemberInfo);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.Equal (
          leftArgumentExpression, Expression.MakeMemberAccess (rightExpression, typeof (TypeForNewExpression).GetField ("C")));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithOneMethodInfoMembers ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var leftArgumentMemberInfo = typeof (TypeForNewExpression).GetMethod ("get_A");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { leftArgumentExpression }, leftArgumentMemberInfo);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.Equal (
          leftArgumentExpression, Expression.Call (rightExpression, typeof (TypeForNewExpression).GetMethod ("get_A")));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnLeftSideWithTwoMethodInfoMembers ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var leftArgumentMemberInfo1 = typeof (TypeForNewExpression).GetMethod ("get_A");
      var leftArgumentMemberInfo2 = typeof (TypeForNewExpression).GetMethod ("get_B");
      var leftExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int), typeof (int)),
          new[] { leftArgumentExpression1, leftArgumentExpression2 },
          leftArgumentMemberInfo1,
          leftArgumentMemberInfo2);
      var rightExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.AndAlso (
          Expression.Equal (
              leftArgumentExpression1, Expression.Call (rightExpression, typeof (TypeForNewExpression).GetMethod ("get_A"))),
          Expression.Equal (leftArgumentExpression2, Expression.Call (rightExpression, typeof (TypeForNewExpression).GetMethod ("get_B"))));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Compound values can only be compared if the respective constructor invocation has members associated with it. Expressions: "
        + "'new TypeForNewExpression(1)', 'value(Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests.TypeForNewExpression)'")]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnRightSideWithoutMembers_ThrowsException ()
    {
      var rightArgumentExpression = Expression.Constant (1);
      var rightExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), rightArgumentExpression);
      var newConstantExpression = Expression.Constant (new TypeForNewExpression (0));
      var expression = Expression.Equal (newConstantExpression, rightExpression);

      _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_NewExpressionOnRightSideWithOnePropertyInfoMember ()
    {
      var rightArgumentExpression = Expression.Constant (1);
      var rightArgumentMemberInfo = typeof (TypeForNewExpression).GetProperty ("A");
      var rightExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)), new[] { rightArgumentExpression }, rightArgumentMemberInfo);
      var leftExpression = new CustomExpression (typeof (TypeForNewExpression));
      var expression = Expression.Equal (leftExpression, rightExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      var expectedResult = Expression.Equal (rightArgumentExpression, Expression.MakeMemberAccess (leftExpression, typeof (TypeForNewExpression).GetProperty ("A")));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_BinaryExpression_MethodIsRemoved ()
    {
      MethodInfo method = ((Func<int?, int?, bool>) ((i1, i2) => i1 == i2)).Method;
      var leftArgumentExpression = Expression.Constant (1, typeof (int));
      var rightArgumentExpression = Expression.Constant (1, typeof (int));
      var leftExpression = Expression.New (typeof (Nullable<>).MakeGenericType (typeof (int)).GetConstructors ()[0], leftArgumentExpression);
      var rightExpression = Expression.New (typeof (Nullable<>).MakeGenericType (typeof (int)).GetConstructors ()[0], rightArgumentExpression);
      BinaryExpression expression = Expression.MakeBinary (ExpressionType.Equal, leftExpression, rightExpression, true, method);

      Assert.That (expression.Method, Is.Not.Null);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (expression);

      Assert.That (((BinaryExpression) result).Method, Is.Null);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNullExpression_ReturnsNonNewExpression_Unchanged ()
    {
      var expression = Expression.Constant (1);
      var sqlIsNullExpression = new SqlIsNullExpression (expression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNullExpression));
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNullExpression_ExpandsNewExpression_ByCheckingAllArguments ()
    {
      var arg1 = Expression.Constant (1);
      var arg2 = Expression.Constant (2);
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), new[] { arg1, arg2 });
      var sqlIsNullExpression = new SqlIsNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNullExpression);

      var expected = Expression.AndAlso (new SqlIsNullExpression (arg1), new SqlIsNullExpression (arg2));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNullExpression_ExpandsNewExpression_ByCheckingAllArguments_OnlyOneArgument ()
    {
      var arg1 = Expression.Constant (1);
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { arg1 });
      var sqlIsNullExpression = new SqlIsNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNullExpression);

      var expected = new SqlIsNullExpression (arg1);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNullExpression_ExpandsNewExpression_ByCheckingAllArguments_ZeroArguments ()
    {
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor());
      var sqlIsNullExpression = new SqlIsNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNullExpression);

      var expected = Expression.Constant (false);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNotNullExpression_ReturnsNonNewExpression_Unchanged ()
    {
      var expression = Expression.Constant (1);
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (expression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNotNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNotNullExpression));
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNotNullExpression_ExpandsNewExpression_ByCheckingAllArguments ()
    {
      var arg1 = Expression.Constant (1);
      var arg2 = Expression.Constant (2);
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int), typeof (int)), new[] { arg1, arg2 });
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNotNullExpression);

      var expected = Expression.OrElse (new SqlIsNotNullExpression (arg1), new SqlIsNotNullExpression (arg2));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNotNullExpression_ExpandsNewExpression_ByCheckingAllArguments_OnlyOneArgument ()
    {
      var arg1 = Expression.Constant (1);
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { arg1 });
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNotNullExpression);

      var expected = new SqlIsNotNullExpression (arg1);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SplitPotentialCompoundComparison_SqlIsNotNullExpression_ExpandsNewExpression_ByCheckingAllArguments_ZeroArguments ()
    {
      var newExpression = Expression.New (TypeForNewExpression.GetConstructor ());
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (newExpression);

      var result = _compoundExpressionComparisonSplitter.SplitPotentialCompoundComparison (sqlIsNotNullExpression);

      var expected = Expression.Constant (true);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }


  }
}