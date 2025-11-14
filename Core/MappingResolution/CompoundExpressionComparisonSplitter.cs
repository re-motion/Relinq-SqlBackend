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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Splits comparisons involving a compound expression, e.g., new { A = 1, B = 2 } == new { A = 3, B = 4 } => (1 == 3) AND (2 == 4).
  /// </summary>
  public class CompoundExpressionComparisonSplitter : ICompoundExpressionComparisonSplitter
  {
    private class NullEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
    }

    public Expression SplitPotentialCompoundComparison (BinaryExpression potentialCompoundComparison)
    {
      ArgumentUtility.CheckNotNull (nameof(potentialCompoundComparison), potentialCompoundComparison);

      var left = potentialCompoundComparison.Left;
      var right = potentialCompoundComparison.Right;
      var nodeType = potentialCompoundComparison.NodeType;

      // NewExpressions are compared by comparing them member-wise
      var leftExpressionAsNewExpression = left as NewExpression;
      var rightExpressionAsNewExpression = right as NewExpression;

      if (leftExpressionAsNewExpression != null && rightExpressionAsNewExpression != null)
      {
        return CreateCtorArgComparison (
            nodeType, leftExpressionAsNewExpression, rightExpressionAsNewExpression);
      }

      // If only one of the expressions is a NewExpression, we'll assume the form "new { Member = Value } == object", and use the members for the 
      // comparison: "Value == object.Member".

      if (leftExpressionAsNewExpression != null)
        return CreateMemberAccessComparison (nodeType, leftExpressionAsNewExpression, right);

      if (rightExpressionAsNewExpression != null)
        return CreateMemberAccessComparison (nodeType, rightExpressionAsNewExpression, left);

      return potentialCompoundComparison;
    }

    public Expression SplitPotentialCompoundComparison (SqlIsNullExpression potentialCompoundComparison)
    {
      ArgumentUtility.CheckNotNull (nameof(potentialCompoundComparison), potentialCompoundComparison);

      var innerExpressionAsNewExpression = potentialCompoundComparison.Expression as NewExpression;
      if (innerExpressionAsNewExpression != null)
      {
        if (innerExpressionAsNewExpression.Arguments.Count == 0)
          return Expression.Constant (false);

        return innerExpressionAsNewExpression.Arguments
            .Select (arg => (Expression) new SqlIsNullExpression (arg))
            .Aggregate (Expression.AndAlso);
      }

      return potentialCompoundComparison;
    }

    public Expression SplitPotentialCompoundComparison (SqlIsNotNullExpression potentialCompoundComparison)
    {
      ArgumentUtility.CheckNotNull (nameof(potentialCompoundComparison), potentialCompoundComparison);

      var innerExpressionAsNewExpression = potentialCompoundComparison.Expression as NewExpression;
      if (innerExpressionAsNewExpression != null)
      {
        if (innerExpressionAsNewExpression.Arguments.Count == 0)
          return Expression.Constant (true);

        return innerExpressionAsNewExpression.Arguments
            .Select (arg => (Expression) new SqlIsNotNullExpression (arg))
            .Aggregate (Expression.OrElse);
      }

      return potentialCompoundComparison;
    }

    private Expression CreateCtorArgComparison (ExpressionType expressionType, NewExpression leftNewExpression, NewExpression rightNewExpression)
    {
      if (!leftNewExpression.Constructor.Equals (rightNewExpression.Constructor))
      {
        var message = string.Format (
            "The results of constructor invocations can only be compared if the same constructors are used for both invocations. "
            + "Expressions: '{0}', '{1}'",
            leftNewExpression,
            rightNewExpression);
        throw new NotSupportedException (message);
      }

      return leftNewExpression.Arguments
          .Select ((left, i) => (Expression) Expression.MakeBinary (expressionType, left, rightNewExpression.Arguments[i]))
          .Aggregate ((previous, current) => CombineComparisons (previous, current, expressionType, leftNewExpression, rightNewExpression));
    }

    private Expression CreateMemberAccessComparison (ExpressionType expressionType, NewExpression newExpression, Expression otherExpression)
    {
      // The ReSharper warning is wrong - newExpression.Members can be null
      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      if (newExpression.Members == null || newExpression.Members.Count == 0)
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      {
        var message = string.Format (
            "Compound values can only be compared if the respective constructor invocation has members associated with it. Expressions: '{0}', '{1}'",
            newExpression,
            otherExpression);
        throw new NotSupportedException (message);
      }

      var combinedComparison = newExpression.Arguments
          .Select ((arg, i) => (Expression) Expression.MakeBinary (expressionType, arg, GetMemberExpression (newExpression.Members[i], otherExpression)))
          .Aggregate ((previous, current) => CombineComparisons (previous, current, expressionType, newExpression, otherExpression));
      //TODO RMLNQSQL-91: check if the filter needs to actually filter like it does during building the querymodel
      return PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees (combinedComparison, new NullEvaluatableExpressionFilter());
    }

    private Expression GetMemberExpression (MemberInfo memberInfo, Expression instance)
    {
      if (memberInfo.MemberType == MemberTypes.Method)
        return Expression.Call (instance, (MethodInfo) memberInfo);
      else
        return Expression.MakeMemberAccess (instance, memberInfo);
    }

    private Expression CombineComparisons (
        Expression previousParts,
        Expression currentPart,
        ExpressionType comparisonExpressionType,
        Expression leftCompoundExpression,
        Expression rightCompoundExpression)
    {
      if (previousParts == null)
      {
        previousParts = currentPart;
      }
      else
      {
        switch (comparisonExpressionType)
        {
          case ExpressionType.Equal:
            previousParts = Expression.AndAlso (previousParts, currentPart);
            break;
          case ExpressionType.NotEqual:
            previousParts = Expression.OrElse (previousParts, currentPart);
            break;
          default:
            var message = string.Format (
                "Compound values can only be compared using 'Equal' and 'NotEqual', not '{0}'. Expressions: {1}, {2}",
                comparisonExpressionType,
                leftCompoundExpression,
                rightCompoundExpression);
            throw new NotSupportedException (message);
        }
      }
      return previousParts;
    } 
  }
}