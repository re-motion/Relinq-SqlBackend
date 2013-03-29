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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
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
      : ExpressionTreeVisitor,
        ISqlSpecificExpressionVisitor,
        IResolvedSqlExpressionVisitor,
        IUnresolvedSqlExpressionVisitor,
        ISqlSubStatementVisitor,
        INamedExpressionVisitor,
        ISqlGroupingSelectExpressionVisitor,
        ISqlConvertedBooleanExpressionVisitor
  {
    public static Expression ApplySqlExpressionContext (
        Expression expression, SqlExpressionContext initialSemantics, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlContextExpressionVisitor (initialSemantics, stage, context);
      return visitor.VisitExpression (expression);
    }

    private readonly SqlExpressionContext _currentContext;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    protected SqlContextExpressionVisitor (SqlExpressionContext currentContext, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _currentContext = currentContext;
      _stage = stage;
      _context = context;
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;

      switch (_currentContext)
      {
        case SqlExpressionContext.SingleValueRequired:
        case SqlExpressionContext.ValueRequired:
          return HandleValueSemantics (expression);
        case SqlExpressionContext.PredicateRequired:
          return HandlePredicateSemantics (expression);
      }

      throw new InvalidOperationException ("Invalid enum value: " + _currentContext);
    }

    public Expression VisitSqlConvertedBooleanExpression (SqlConvertedBooleanExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newInner = ApplyValueContext (expression.Expression);
      Debug.Assert (
          newInner == expression.Expression,
          "There is currently no visit method that would change an int-typed expression with ValueRequired.");

      // This condition cannot be true at the moment because there currently is no int-typed expression that would be changed by ValueRequired.
      //if (newInner != expression.Expression)
      //  return new ConvertedBooleanExpression (newInner);

      return expression;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      // Always convert boolean constants to int constants because in the database, there are no boolean constants
      
      if (BooleanUtility.IsBooleanType (expression.Type))
      {
        var intType = BooleanUtility.GetMatchingIntType (expression.Type);
        var convertedExpression = expression.Value == null
                                      ? Expression.Constant (null, intType)
                                      : expression.Value.Equals (true)
                                            ? Expression.Constant (1, intType)
                                            : Expression.Constant (0, intType);
        return new SqlConvertedBooleanExpression (convertedExpression);
      }
      
      return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      // We always need to convert boolean columns to int columns because in the database, the column is represented as a bit (integer) value
      if (BooleanUtility.IsBooleanType (expression.Type))
      {
        var intType = BooleanUtility.GetMatchingIntType (expression.Type);
        Expression convertedExpression = expression.Update (intType, expression.OwningTableAlias, expression.ColumnName, expression.IsPrimaryKey);
        return new SqlConvertedBooleanExpression (convertedExpression);
      }
      
      return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      if (_currentContext == SqlExpressionContext.SingleValueRequired)
        // TODO 4878: When primary key can be a compound expression, revisit expression.PrimaryKeyColumn to obtain a single value.
        return expression.PrimaryKeyColumn;
      else
        return expression; // rely on VisitExpression to apply correct semantics
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!BooleanUtility.IsBooleanType (expression.Type))
        return base.VisitBinaryExpression (expression);

      var childContext = GetChildSemanticsForBinaryBoolExpression (expression.NodeType);
      var left = ApplySqlExpressionContext (expression.Left, childContext);
      var right = ApplySqlExpressionContext (expression.Right, childContext);

      if (expression.NodeType == ExpressionType.Coalesce)
      {
        // In predicate context, we can ignore coalesces towards false, treat like a conversion to bool instead. (SQL treats NULL values in a falsey
        // way in predicate contexts.)
        if (_currentContext == SqlExpressionContext.PredicateRequired
            && expression.Right is ConstantExpression
            && Equals (((ConstantExpression) expression.Right).Value, false))
        {
          return VisitExpression (Expression.Convert (expression.Left, expression.Type));
        }

        // We'll pull out the bool conversion marker from the operands of the Coalesce expression and instead put it around the whole expression.
        // That way, HandleValueSemantics will not try to convert us back to a value; this avoids double CASE WHENs.
        // We know that left and right must be ConvertedBooleanExpressions because Coalesce has single value semantics for its operands, and boolean
        // Coalesces must have booleans operands. Applying value semantics to boolean operands results in ConvertedBooleanExpression values.
        
        Debug.Assert (childContext == SqlExpressionContext.SingleValueRequired);
        Debug.Assert (left is SqlConvertedBooleanExpression);
        Debug.Assert (right is SqlConvertedBooleanExpression);
        var newCoalesceExpression = Expression.Coalesce (((SqlConvertedBooleanExpression) left).Expression, ((SqlConvertedBooleanExpression) right).Expression);
        return new SqlConvertedBooleanExpression (newCoalesceExpression);
      }

      if (left != expression.Left || right != expression.Right)
        return ConversionUtility.MakeBinaryWithOperandConversion (expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);

      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var childContext = GetChildSemanticsForUnaryExpression (expression);
      var newOperand = ApplySqlExpressionContext (expression.Operand, childContext);

      if (newOperand != expression.Operand)
      {
        if (expression.NodeType == ExpressionType.Convert)
        {
          // If the operand changes its type due to context application, we must also strip any Convert nodes since they are most likely no longer 
          // applicable after the context switch.
          if (expression.Operand.Type != newOperand.Type)
            return newOperand;

          // If this is a convert of a SqlConvertedBooleanExpression to bool? or bool, move the Convert into the SqlConvertedBooleanExpression
          var convertedBooleanExpressionOperand = newOperand as SqlConvertedBooleanExpression;
          if (convertedBooleanExpressionOperand != null)
          {
            if (expression.Type == typeof (bool))
              return new SqlConvertedBooleanExpression (Expression.Convert (convertedBooleanExpressionOperand.Expression, typeof (int)));
            else if (expression.Type == typeof (bool?))
              return new SqlConvertedBooleanExpression (Expression.Convert (convertedBooleanExpressionOperand.Expression, typeof (int?)));
          }
        }

        return Expression.MakeUnary (expression.NodeType, newOperand, expression.Type, expression.Method);
      }

      return expression;
    }

    public Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ApplySingleValueContext (expression.Expression);
      if (newExpression != expression.Expression)
        return new SqlIsNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ApplySingleValueContext (expression.Expression);
      if (newExpression != expression.Expression)
        return new SqlIsNotNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (_currentContext == SqlExpressionContext.SingleValueRequired)
        // TODO 4878: When primary key can be a compound expression, revisit expression.PrimaryKeyColumn to obtain a single value.
        return expression.PrimaryKeyExpression;
      else
        return expression; // rely on VisitExpression to apply correct semantics
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newSqlStatement = _stage.ApplySelectionContext (expression.SqlStatement, _currentContext, _context);
      if (!ReferenceEquals (expression.SqlStatement, newSqlStatement))
        return new SqlSubStatementExpression (newSqlStatement);
      return expression;
    }

    public Expression VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedJoinInfo = _stage.ResolveJoinInfo (
          new UnresolvedJoinInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One), _context);
      switch (_currentContext)
      {
        case SqlExpressionContext.ValueRequired:
          return _stage.ResolveEntityRefMemberExpression (expression, resolvedJoinInfo, _context);
        case SqlExpressionContext.SingleValueRequired:
          // TODO 4878: Temporarily disable this optimization. With RM-3315, we should get it back because the MappingResolver will replace the 
          // SqlEntityRefMemberExpression if it isn't required.
          var columnExpression = resolvedJoinInfo.RightKey as SqlColumnExpression;
          if (columnExpression != null && columnExpression.IsPrimaryKey)
            return resolvedJoinInfo.LeftKey;
          else
            // TODO 4878: Revisit PrimaryKeyColumn, this could be a compound value.
            return _stage.ResolveEntityRefMemberExpression (expression, resolvedJoinInfo, _context).PrimaryKeyColumn;
      }
      
      var message = string.Format (
          "Context '{0}' is not allowed for members referencing entities: '{1}'.", 
          _currentContext, 
          FormattingExpressionTreeVisitor.Format (expression));
      throw new NotSupportedException (message);
    }

    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var expressionWithAppliedInnerContext = new NamedExpression (
          expression.Name,
          VisitExpression (expression.Expression));

      var result = NamedExpressionCombiner.ProcessNames (_context, expressionWithAppliedInnerContext);

      if (result != expressionWithAppliedInnerContext || expressionWithAppliedInnerContext.Expression != expression.Expression)
        return result;
      else
        return expression;
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO 4878: In single value context, ask the mapping resolver to resolve the NewExpression into a single value. If this is not possible, 
      // throw (compound value cannot be used where a single value is required).

      var newArguments = expression.Arguments.Select (ApplyValueContext).ToArray ();
      if (!newArguments.SequenceEqual (expression.Arguments))
      {
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        if (expression.Members != null && expression.Members.Count > 0)
          return Expression.New (expression.Constructor, newArguments, expression.Members);
        else
          return Expression.New (expression.Constructor, newArguments);
        // ReSharper restore ConditionIsAlwaysTrueOrFalse
      }

      return expression;
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      // Method arguments and target instance are always values

      var newInstance = expression.Object != null ? ApplyValueContext (expression.Object) : null;
      var newArguments = expression.Arguments.Select (ApplyValueContext).ToArray();
      if (newInstance != expression.Object || !newArguments.SequenceEqual (expression.Arguments))
        return Expression.Call (newInstance, expression.Method, newArguments);

      return expression;
    }

    public Expression VisitSqlGroupingSelectExpression (SqlGroupingSelectExpression expression)
    {
      var newKeyExpression = ApplyValueContext (expression.KeyExpression);
      var newElementExpression = ApplyValueContext (expression.ElementExpression);
      var newAggregationExpressions = expression.AggregationExpressions
          .Select (ApplyValueContext)
          .ToArray();

      if (newKeyExpression != expression.KeyExpression
          || newElementExpression != expression.ElementExpression
          || !newAggregationExpressions.SequenceEqual (expression.AggregationExpressions))
        return _context.UpdateGroupingSelectAndAddMapping (expression, newKeyExpression, newElementExpression, newAggregationExpressions);

      return expression;
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      // TODO 4878: Has no children.
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      // TODO 4878: Shouldn't this be Value semantics? EXISTS can deal with multi-column substatements.
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlRowNumberExpression (SqlRowNumberExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlLikeExpression (SqlLikeExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlLengthExpression (SqlLengthExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newCases = VisitList (
          expression.Cases,
          caseWhenPair =>
          {
            var newWhen = ApplyPredicateContext (caseWhenPair.When);
            // Actually, this should be single value context, but we don't yet support entities in a SqlCaseExpression
            var newThen = ApplyValueContext (caseWhenPair.Then);
            return caseWhenPair.Update (newWhen, newThen);
          });
      // Actually, this should be single value context, but we don't yet support entities in a SqlCaseExpression
      var newElseCase = expression.ElseCase != null ? ApplyValueContext (expression.ElseCase) : null;
      return expression.Update (newCases, newElseCase);
    }

    public Expression VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      // TODO 4878: Has no children.
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    // TODO 4878: Rename expression to SqlInExpression
    public Expression VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      return VisitChildrenWithSingleValueSemantics (expression);
    }

    protected override Expression VisitInvocationExpression (InvocationExpression expression)
    {
      var message = string.Format (
          "InvocationExpressions are not supported in the SQL backend. Expression: '{0}'.", FormattingExpressionTreeVisitor.Format (expression));
      throw new NotSupportedException (message);
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      var message = string.Format (
          "LambdaExpressions are not supported in the SQL backend. Expression: '{0}'.", FormattingExpressionTreeVisitor.Format (expression));
      throw new NotSupportedException (message);
    }

    private Expression VisitChildrenWithSingleValueSemantics (ExtensionExpression expression)
    {
      var visitor = new SqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, _stage, _context);
      return visitor.VisitExtensionExpression (expression);
    }

    private SqlExpressionContext GetChildSemanticsForUnaryExpression (Expression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Convert:
          return _currentContext;
        case ExpressionType.Not:
          if (BooleanUtility.IsBooleanType (expression.Type))
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
        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
          return SqlExpressionContext.PredicateRequired;

        default:
          // case ExpressionType.NotEqual:
          // case ExpressionType.Equal:
          // case ExpressionType.Coalesce:
          return SqlExpressionContext.SingleValueRequired;
      }
    }

    private Expression HandleValueSemantics (Expression expression)
    {
      var newExpression = base.VisitExpression (expression);
      if (newExpression is SqlConvertedBooleanExpression)
        return newExpression;

      // We don't adjust the results of local method calls, every value is a supported value here. This is a workaround, better solution in RM-5348.
      if (newExpression is MethodCallExpression)
        return newExpression;

      if (BooleanUtility.IsBooleanType (newExpression.Type))
      {
        var convertedExpression = CreateValueExpressionForPredicate (newExpression);
        return new SqlConvertedBooleanExpression (convertedExpression);
      }
      else
      {
        return newExpression;
      }
    }

    private Expression HandlePredicateSemantics (Expression expression)
    {
      var newExpression = base.VisitExpression (expression);

      var convertedBooleanExpression = newExpression as SqlConvertedBooleanExpression;
      if (convertedBooleanExpression != null)
      {
        var isNullableExpression = convertedBooleanExpression.Expression.Type == typeof (int?);
        return Expression.Equal (convertedBooleanExpression.Expression, new SqlLiteralExpression (1, isNullableExpression), isNullableExpression, null);
      }

      if (!BooleanUtility.IsBooleanType (newExpression.Type))
      {
        var message = string.Format (
            "Cannot convert an expression of type '{0}' to a boolean expression. Expression: '{1}'", 
            newExpression.Type, 
            FormattingExpressionTreeVisitor.Format(expression));
        throw new NotSupportedException (message);
      }

      return newExpression;
    }

    private Expression ApplySingleValueContext (Expression expression)
    {
      return ApplySqlExpressionContext (expression, SqlExpressionContext.SingleValueRequired);
    }

    private Expression ApplyValueContext (Expression expression)
    {
      return ApplySqlExpressionContext (expression, SqlExpressionContext.ValueRequired);
    }

    private Expression ApplyPredicateContext (Expression expression)
    {
      return ApplySqlExpressionContext (expression, SqlExpressionContext.PredicateRequired);
    }

    private Expression ApplySqlExpressionContext (Expression expression, SqlExpressionContext expressionContext)
    {
      return ApplySqlExpressionContext (expression, expressionContext, _stage, _context);
    }

    private Expression CreateValueExpressionForPredicate (Expression predicate)
    {
      // If the predicate is nullable, we have three cases (true, false, null). Otherweise, we just have two cases.
      if (predicate.Type == typeof (bool?))
        return SqlCaseExpression.CreateIfThenElseNull (typeof (int?), predicate, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      else
        return SqlCaseExpression.CreateIfThenElse (typeof (int), predicate, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
    }
  }
}