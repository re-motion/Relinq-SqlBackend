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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  /// <remarks>
  /// <see cref="SqlContextExpressionVisitor"/> traverses an expression tree and ensures that the tree fits SQL server requirements for
  /// boolean expressions. In scenarios where a value is required as per SQL server standards, bool expressions are converted to integers using
  /// CASE WHEN expressions. In such situations, <see langword="true" /> and <see langword="false" /> constants are converted to 1 and 0 values,
  /// and boolean columns are interpreted as integer values. In scenarios where a predicate is required, boolean expressions are constructed by 
  /// comparing those integer values to 1 and 0 literals.
  /// </remarks>
  public class SqlContextExpressionVisitor : ExpressionTreeVisitor, ISqlSpecificExpressionVisitor, IResolvedSqlExpressionVisitor
  {
    public static Expression ApplySqlExpressionContext (Expression expression, SqlExpressionContext initialSemantics)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var visitor = new SqlContextExpressionVisitor (initialSemantics, true);
      return visitor.VisitExpression (expression);
    }

    private readonly SqlExpressionContext _currentContext;
    
    private bool _isTopLevelExpression;
    
    protected SqlContextExpressionVisitor (SqlExpressionContext currentContext, bool isTopLevelExpression)
    {
      _currentContext = currentContext;
      _isTopLevelExpression = isTopLevelExpression;
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return expression;

      // Expressions that are not on the top level always need SingleValueRequired semantics
      if (!_isTopLevelExpression && _currentContext != SqlExpressionContext.SingleValueRequired)
        return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired);

      // This is only executed if the _currentContext is SingleValueRequired or if we are at the top level

      _isTopLevelExpression = false;

      // TODO 2639: Move to SqlStatement ctor
      //if (expression.Type != typeof (string) && typeof (IEnumerable).IsAssignableFrom (expression.Type))
      //  throw new NotSupportedException ("Subquery selects a collection where a single value is expected.");

      //TODO 2639: add visitor interface, move to VisitSqlEntityConstantExpression method
      if (_currentContext == SqlExpressionContext.SingleValueRequired)
      {
        var entityConstantExpression = expression as SqlEntityConstantExpression;
        if (entityConstantExpression != null)
          return Expression.Constant (entityConstantExpression.PrimaryKeyValue, entityConstantExpression.PrimaryKeyValue.GetType());
      }

      var newExpression = base.VisitExpression (expression);

      switch (_currentContext)
      {
        case SqlExpressionContext.SingleValueRequired:
        case SqlExpressionContext.ValueRequired:
          if (newExpression.Type == typeof (bool))
            return new SqlCaseExpression (newExpression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
          else
            return newExpression;
        case SqlExpressionContext.PredicateRequired:
          if (newExpression.Type == typeof (bool))
            return newExpression;
          else if (newExpression.Type == typeof (int))
            return Expression.Equal (newExpression, new SqlLiteralExpression (1));
          else
            throw new NotSupportedException (string.Format ("Cannot convert an expression of type '{0}' to a boolean expression.", expression.Type));
      }

      throw new InvalidOperationException ("Invalid enum value: " + _currentContext);
    }
     
    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      // Always convert boolean constants to int constants because in the database, there are no boolean constants
      if (expression.Type == typeof (bool))
        return expression.Value.Equals (true) ? Expression.Constant (1) : Expression.Constant (0);
      else
        return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      // We always need to convert boolean columns to int columns because in the database, the column is represented as a bit (integer) value
      if (expression.Type == typeof (bool))
        return new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
      else
        return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      if (_currentContext == SqlExpressionContext.SingleValueRequired)
        return expression.PrimaryKeyColumn;
      else
        return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlValueTableReferenceExpression (SqlValueTableReferenceExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      var testPredicate = ApplySqlExpressionContext (expression.TestPredicate, SqlExpressionContext.PredicateRequired);
      var thenValue = ApplySqlExpressionContext (expression.ThenValue, SqlExpressionContext.SingleValueRequired);
      var elseValue = ApplySqlExpressionContext (expression.ElseValue, SqlExpressionContext.SingleValueRequired);

      if (testPredicate != expression.TestPredicate || thenValue != expression.ThenValue || elseValue != expression.ElseValue)
        return new SqlCaseExpression (testPredicate, thenValue, elseValue);
      else
        return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return base.VisitBinaryExpression (expression);

      var childContext = GetChildSemanticsForBoolExpression (expression.NodeType);
      var left = ApplySqlExpressionContext (expression.Left, childContext);
      var right = ApplySqlExpressionContext (expression.Right, childContext);

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);
      
      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return base.VisitUnaryExpression (expression);

      var childContext = GetChildSemanticsForBoolExpression (expression.NodeType);
      var operand = ApplySqlExpressionContext (expression.Operand, childContext);

      if (operand != expression.Operand)
        expression = Expression.MakeUnary (expression.NodeType, operand, expression.Type, expression.Method);

      return expression;
    }

    public Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      var newExpression = ApplySqlExpressionContext (expression.Expression, SqlExpressionContext.SingleValueRequired);
      if (newExpression != expression.Expression)
        return new SqlIsNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      var newExpression = ApplySqlExpressionContext (expression.Expression, SqlExpressionContext.SingleValueRequired);
      if (newExpression != expression.Expression)
        return new SqlIsNotNullExpression (newExpression);
      return expression;
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }
    
    private SqlExpressionContext GetChildSemanticsForBoolExpression (ExpressionType expressionType)
    {
      switch (expressionType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          return SqlExpressionContext.SingleValueRequired;

        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
          return SqlExpressionContext.PredicateRequired;

        case ExpressionType.Not:
          return SqlExpressionContext.PredicateRequired;

        case ExpressionType.Convert:
          var message = string.Format ("'{0}' expressions are not supported with boolean type.", expressionType);
          throw new NotSupportedException (message);

        default:
          return SqlExpressionContext.SingleValueRequired;
      }
    }

    // TODO 2639
    //  switch (_currentContext)
    //  {
    //    case SqlExpressionContext.SingleValueRequired:
    //      return HandleSingleValueSemantics (expression);
    //    case SqlExpressionContext.ValueRequired:
    //      return HandleValueSemantics (expression);
    //    case SqlExpressionContext.PredicateRequired:
    //      return HandlePredicateSemantics (expression);
    //  }
    //}

    //private Expression HandleSingleValueSemantics (Expression expression)
    //{
    //  if (newExpression.Type == typeof (bool))
    //    return new SqlCaseExpression (newExpression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
    //  else
    //    return newExpression;
    //}

    //private Expression HandleValueSemantics (Expression expression)
    //{
    //  if (!_isTopLevelExpression)
    //    return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired);

    //  if (newExpression.Type == typeof (bool))
    //    return new SqlCaseExpression (newExpression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
    //  else
    //    return newExpression;
    //}

    //private Expression HandlePredicateSemantics (Expression expression)
    //{
    //  if (!_isTopLevelExpression)
    //    return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired);

    //  if (newExpression.Type == typeof (bool))
    //    return newExpression;
    //  else if (newExpression.Type == typeof (int))
    //    return Expression.Equal (newExpression, new SqlLiteralExpression (1));
    //  else
    //    throw new NotSupportedException (string.Format ("Cannot convert an expression of type '{0}' to a boolean expression.", expression.Type));
    //}
   
  }
}