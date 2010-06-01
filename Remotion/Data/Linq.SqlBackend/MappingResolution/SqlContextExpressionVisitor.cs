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
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
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
  public class SqlContextExpressionVisitor
      : ExpressionTreeVisitor, ISqlSpecificExpressionVisitor, IResolvedSqlExpressionVisitor, IUnresolvedSqlExpressionVisitor, ISqlSubStatementVisitor, INamedExpressionVisitor
  {
    public static Expression ApplySqlExpressionContext (Expression expression, SqlExpressionContext initialSemantics, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlContextExpressionVisitor (initialSemantics, true, stage, context);
      return visitor.VisitExpression (expression);
    }

    private readonly SqlExpressionContext _currentContext;

    private bool _isTopLevelExpression;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    protected SqlContextExpressionVisitor (SqlExpressionContext currentContext, bool isTopLevelExpression, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _currentContext = currentContext;
      _isTopLevelExpression = isTopLevelExpression;
      _stage = stage;
      _context = context;
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return expression;

      switch (_currentContext)
      {
        case SqlExpressionContext.SingleValueRequired:
          return HandleSingleValueSemantics (expression);
        case SqlExpressionContext.ValueRequired:
          return HandleValueSemantics (expression);
        case SqlExpressionContext.PredicateRequired:
          return HandlePredicateSemantics (expression);
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
        return expression.Update (typeof (int), expression.OwningTableAlias, expression.ColumnName, expression.IsPrimaryKey);
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

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      var testPredicate = ApplySqlExpressionContext (expression.TestPredicate, SqlExpressionContext.PredicateRequired, _stage, _context);
      var thenValue = ApplySqlExpressionContext (expression.ThenValue, SqlExpressionContext.SingleValueRequired, _stage, _context);
      var elseValue = ApplySqlExpressionContext (expression.ElseValue, SqlExpressionContext.SingleValueRequired, _stage, _context);

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

      var childContext = GetChildSemanticsForBinaryBoolExpression (expression.NodeType);
      var left = ApplySqlExpressionContext (expression.Left, childContext, _stage, _context);
      var right = ApplySqlExpressionContext (expression.Right, childContext, _stage, _context);

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);

      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newOperand = ApplySqlExpressionContext (expression.Operand, GetChildSemanticsForUnaryExpression (expression), _stage, _context);
      
      if (newOperand != expression.Operand)
        expression = Expression.MakeUnary (expression.NodeType, newOperand, expression.Type, expression.Method);
      return expression;
    }

    public Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ApplySqlExpressionContext (expression.Expression, SqlExpressionContext.SingleValueRequired, _stage, _context);
      if (newExpression != expression.Expression)
        return new SqlIsNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ApplySqlExpressionContext (expression.Expression, SqlExpressionContext.SingleValueRequired, _stage, _context);
      if (newExpression != expression.Expression)
        return new SqlIsNotNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (_currentContext == SqlExpressionContext.SingleValueRequired)
        return Expression.Constant (expression.PrimaryKeyValue, expression.PrimaryKeyValue.GetType());
      return expression;
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newSqlStatement = _stage.ApplySelectionContext (expression.SqlStatement, _currentContext, _context);
      if (expression.SqlStatement != newSqlStatement)
        return new SqlSubStatementExpression (newSqlStatement);
      return expression;
    }

    public Expression VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedJoinInfo = _stage.ResolveJoinInfo (new UnresolvedJoinInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One), _context);
      switch (_currentContext)
      {
        case SqlExpressionContext.ValueRequired:
          return _stage.ResolveEntityRefMemberExpression (expression, resolvedJoinInfo, _context);
        case SqlExpressionContext.SingleValueRequired:
          var columnExpression = resolvedJoinInfo.RightKey as SqlColumnExpression;
          if (columnExpression != null && columnExpression.IsPrimaryKey)
            return resolvedJoinInfo.LeftKey;
          else
            return _stage.ResolveEntityRefMemberExpression (expression, resolvedJoinInfo, _context).PrimaryKeyColumn;
      }
      throw new NotSupportedException (string.Format ("Context '{0}' is not allowed for expression '{1}'.", _currentContext, FormattingExpressionTreeVisitor.Format(expression)));
    }

    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ApplySqlExpressionContext (expression.Expression, SqlExpressionContext.ValueRequired, _stage, _context);

      if (newExpression is NewExpression)
      {
        var sqlSelectNewExpression = (NewExpression) newExpression;
        var newNewExpression = Expression.New (
            sqlSelectNewExpression.Constructor,
            sqlSelectNewExpression.Arguments.Select (expr => (Expression) new NamedExpression (expression.Name, expr)),
            sqlSelectNewExpression.Members
            );
        return ApplySqlExpressionContext (newNewExpression, _currentContext, _stage, _context);
      }
      else if (newExpression is SqlEntityExpression)
      {
        var entityExpression = (SqlEntityExpression) newExpression;
        string newName = CombineNames (expression.Name, entityExpression.Name);
        return ((SqlEntityExpression) newExpression).Update (newExpression.Type, ((SqlEntityExpression) newExpression).TableAlias, newName);
      }
      else if(newExpression is SqlValueReferenceExpression)
      {
        var sqlValueReferenceExpression = (SqlValueReferenceExpression) newExpression;
        return new NamedExpression (sqlValueReferenceExpression.Name, newExpression);
      }
      else if (newExpression is NamedExpression)
      {
        var namedExpression = (NamedExpression) newExpression;
        var newName = CombineNames (expression.Name, namedExpression.Name);
        return new NamedExpression (newName, namedExpression.Expression);
      }

      if(newExpression != expression.Expression)
        return new NamedExpression (expression.Name, newExpression);
      return expression;
    }

    private string CombineNames (string name1, string name2)
    {
      string newName;
      if (name1 == null)
        newName = name2;
      else if (name2 == null)
        newName = name1;
      else
        newName = name1 + "_" + name2;
      return newName;
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var expressions = expression.Arguments.Select (expr => ApplySqlExpressionContext (expr, SqlExpressionContext.ValueRequired, _stage, _context));
      return Expression.New (expression.Constructor, expressions, expression.Members);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlValueReferenceExpression (SqlValueReferenceExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    private SqlExpressionContext GetChildSemanticsForUnaryExpression (Expression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Convert:
          return _currentContext;
        case ExpressionType.Not:
          if (expression.Type == typeof (bool))
            return SqlExpressionContext.PredicateRequired;
          else
            return SqlExpressionContext.SingleValueRequired;
        default:
          return SqlExpressionContext.SingleValueRequired;
      }
    }

    private SqlExpressionContext GetChildSemanticsForBinaryBoolExpression (ExpressionType expressionType)
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
        default:
          return SqlExpressionContext.SingleValueRequired;
      }
    }

    private Expression HandleSingleValueSemantics (Expression expression)
    {
      _isTopLevelExpression = false;

      var newExpression = base.VisitExpression (expression);
      if (newExpression.Type == typeof (bool))
        return new SqlCaseExpression (newExpression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      else
        return newExpression;
    }

    private Expression HandleValueSemantics (Expression expression)
    {
      if (!_isTopLevelExpression)
        return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired, _stage, _context);

      _isTopLevelExpression = false;

      var newExpression = base.VisitExpression (expression);
      if (newExpression.Type == typeof (bool))
        return new SqlCaseExpression (newExpression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      else
        return newExpression;
    }

    private Expression HandlePredicateSemantics (Expression expression)
    {
      if (!_isTopLevelExpression)
        return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired, _stage, _context);

      _isTopLevelExpression = false;

      var newExpression = base.VisitExpression (expression);
      if (newExpression.Type == typeof (bool))
        return newExpression;
      else if (newExpression.Type == typeof (int))
        return Expression.Equal (newExpression, new SqlLiteralExpression (1));
      else
        throw new NotSupportedException (string.Format ("Cannot convert an expression of type '{0}' to a boolean expression.", newExpression.Type));
    }
    
  }
}