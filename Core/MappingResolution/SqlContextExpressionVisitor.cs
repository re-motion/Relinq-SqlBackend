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
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  /// <remarks>
  /// <see cref="SqlContextExpressionVisitor"/> traverses an expression tree and ensures that the tree fits SQL server requirements for
  /// expressions. In scenarios where a value is required as per SQL server standards, boolean expressions are converted to integers using
  /// CASE WHEN expressions. In such situations, <see langword="true" /> and <see langword="false" /> constants are converted to 1 and 0 values,
  /// and boolean columns are interpreted as integer values. In scenarios where a predicate is required, boolean expressions are constructed by 
  /// comparing those integer values to 1 and 0 literals. In scenarios where a single value is required, an exception is thrown where compound 
  /// values (<see cref="NewExpression"/>) or entities are encountered.
  /// </remarks>
  public class SqlContextExpressionVisitor
      : RelinqExpressionVisitor,
        ISqlSpecificExpressionVisitor,
        IResolvedSqlExpressionVisitor,
        ISqlSubStatementVisitor,
        ISqlGroupingSelectExpressionVisitor,
        ISqlConvertedBooleanExpressionVisitor,
        INamedExpressionVisitor,
        IAggregationExpressionVisitor
  {
    public static Expression ApplySqlExpressionContext (
        Expression expression, SqlExpressionContext initialSemantics, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      var visitor = new SqlContextExpressionVisitor (initialSemantics, stage, context);
      return visitor.Visit (expression);
    }

    private readonly SqlExpressionContext _currentContext;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    protected SqlContextExpressionVisitor (SqlExpressionContext currentContext, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      _currentContext = currentContext;
      _stage = stage;
      _context = context;
    }

    public override Expression Visit (Expression expression)
    {
      if (expression == null)
        return null;

      var currentContext = _currentContext;
      switch (currentContext)
      {
        case SqlExpressionContext.SingleValueRequired:
        case SqlExpressionContext.ValueRequired:
          return HandleValueSemantics (expression);
        case SqlExpressionContext.PredicateRequired:
          return HandlePredicateSemantics (expression);
      }

      throw new InvalidOperationException ("Invalid enum value: " + currentContext);
    }

    public Expression VisitSqlConvertedBoolean (SqlConvertedBooleanExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newInner = ApplyValueContext (expression.Expression);
      Assertion.DebugAssert (
          newInner == expression.Expression,
          "There is currently no visit method that would change an int-typed expression with ValueRequired.");

      // This condition cannot be true at the moment because there currently is no int-typed expression that would be changed by ValueRequired.
      //if (newInner != expression.Expression)
      //  return new ConvertedBooleanExpression (newInner);

      return expression;
    }

    protected override Expression VisitConstant (ConstantExpression expression)
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
      
      return expression; // rely on Visit to apply correct semantics
    }

    public Expression VisitSqlColumn (SqlColumnExpression expression)
    {
      // We always need to convert boolean columns to int columns because in the database, the column is represented as a bit (integer) value
      if (BooleanUtility.IsBooleanType (expression.Type))
      {
        var intType = BooleanUtility.GetMatchingIntType (expression.Type);
        Expression convertedExpression = expression.Update (intType, expression.OwningTableAlias, expression.ColumnName, expression.IsPrimaryKey);
        return new SqlConvertedBooleanExpression (convertedExpression);
      }
      
      return expression; // rely on Visit to apply correct semantics
    }

    public Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      if (_currentContext == SqlExpressionContext.SingleValueRequired)
      {
        string message = string.Format (
            "Cannot use an entity expression ('{0}' of type '{1}') in a place where SQL requires a single value.",
            expression,
            expression.Type.Name);
        throw new NotSupportedException (message);
      }

      return expression; // rely on Visit to apply correct semantics
    }

    protected override Expression VisitBinary (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var childContext = BooleanUtility.IsBooleanType (expression.Type)
                             ? GetChildSemanticsForBinaryBoolExpression (expression.NodeType)
                             : SqlExpressionContext.SingleValueRequired;
      var left = ApplySqlExpressionContext (expression.Left, childContext);
      var right = ApplySqlExpressionContext (expression.Right, childContext);

      if (BooleanUtility.IsBooleanType (expression.Type) && expression.NodeType == ExpressionType.Coalesce)
      {
        // In predicate context, we can ignore coalesces towards false, treat like a conversion to bool instead. (SQL treats NULL values in a falsey
        // way in predicate contexts.)
        if (_currentContext == SqlExpressionContext.PredicateRequired
            && expression.Right is ConstantExpression
            && Equals (((ConstantExpression) expression.Right).Value, false))
        {
          return Visit (Expression.Convert (expression.Left, expression.Type));
        }

        // We'll pull out the bool conversion marker from the operands of the Coalesce expression and instead put it around the whole expression.
        // That way, HandleValueSemantics will not try to convert us back to a value; this avoids double CASE WHENs.
        // We know that left and right must be ConvertedBooleanExpressions because Coalesce has single value semantics for its operands, and boolean
        // Coalesces must have booleans operands. Applying value semantics to boolean operands results in ConvertedBooleanExpression values.
        
        Assertion.DebugAssert (childContext == SqlExpressionContext.SingleValueRequired);
        Assertion.DebugAssert (left is SqlConvertedBooleanExpression);
        Assertion.DebugAssert (right is SqlConvertedBooleanExpression);
        var newCoalesceExpression = Expression.Coalesce (((SqlConvertedBooleanExpression) left).Expression, ((SqlConvertedBooleanExpression) right).Expression);
        return new SqlConvertedBooleanExpression (newCoalesceExpression);
      }

      if (left != expression.Left || right != expression.Right)
        return Expression.MakeBinary (expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);

      return expression;
    }

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var childContext = GetChildSemanticsForUnaryExpression (expression);
      var newOperand = ApplySqlExpressionContext (expression.Operand, childContext);

      if (newOperand != expression.Operand)
      {
        if (expression.NodeType == ExpressionType.Convert)
        {
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

    public Expression VisitSqlIsNull (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newExpression = ApplySingleValueContext (expression.Expression);
      if (newExpression != expression.Expression)
        return new SqlIsNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlIsNotNull (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newExpression = ApplySingleValueContext (expression.Expression);
      if (newExpression != expression.Expression)
        return new SqlIsNotNullExpression (newExpression);
      return expression;
    }

    public Expression VisitSqlEntityConstant (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (_currentContext == SqlExpressionContext.SingleValueRequired)
      {
        string message = string.Format (
            "Cannot use an entity constant ('{0}' of type '{1}') in a place where SQL requires a single value.",
            expression,
            expression.Type.Name);
        throw new NotSupportedException (message);
      }
      return expression; // rely on Visit to apply correct semantics
    }

    public Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newSqlStatement = _stage.ApplySelectionContext (expression.SqlStatement, _currentContext, _context);
      if (!ReferenceEquals (expression.SqlStatement, newSqlStatement))
        return new SqlSubStatementExpression (newSqlStatement);
      return expression;
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (_currentContext == SqlExpressionContext.SingleValueRequired)
      {
        string message = string.Format ("Cannot use a complex expression ('{0}') in a place where SQL requires a single value.", expression);
        throw new NotSupportedException (message);
      }

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

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      // Method arguments and target instance are always values

      var newInstance = expression.Object != null ? ApplyValueContext (expression.Object) : null;
      var newArguments = expression.Arguments.Select (ApplyValueContext).ToArray();
      if (newInstance != expression.Object || !newArguments.SequenceEqual (expression.Arguments))
        return Expression.Call (newInstance, expression.Method, newArguments);

      return expression;
    }

    public Expression VisitNamed (NamedExpression expression)
    {
      var newInnerExpression = Visit (expression.Expression);
      if (newInnerExpression is SqlConvertedBooleanExpression)
      {
        var convertedBooleanExpression = (SqlConvertedBooleanExpression) newInnerExpression;
        var innerNamedExpression = new NamedExpression (expression.Name, convertedBooleanExpression.Expression);
        return Visit (new SqlConvertedBooleanExpression (innerNamedExpression));
      }

      if (newInnerExpression != expression.Expression)
        return new NamedExpression (expression.Name, newInnerExpression);

      return expression;
    }

    public Expression VisitSqlGroupingSelect (SqlGroupingSelectExpression expression)
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

    public Expression VisitSqlFunction (SqlFunctionExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
    }

    public Expression VisitSqlConvert (SqlConvertExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
    }

    public Expression VisitSqlExists (SqlExistsExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.ValueRequired);
    }

    public Expression VisitSqlRowNumber (SqlRowNumberExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
    }

    public Expression VisitSqlLike (SqlLikeExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
    }

    public Expression VisitSqlLength (SqlLengthExpression expression)
    {
      return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
    }

    public Expression VisitSqlCase (SqlCaseExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newCases = Visit (
          expression.Cases,
          caseWhenPair =>
          {
            var newWhen = ApplyPredicateContext (caseWhenPair.When);
            var newThen = ApplySingleValueContext (caseWhenPair.Then);
            return caseWhenPair.Update (newWhen, newThen);
          });
      var newElseCase = expression.ElseCase != null ? ApplySingleValueContext (expression.ElseCase) : null;
      return expression.Update (newCases, newElseCase);
    }

    public Expression VisitSqlLiteral (SqlLiteralExpression expression)
    {
      // No children.
      return expression;
    }

    public Expression VisitSqlIn (SqlInExpression expression)
    {
      try
      {
        return VisitChildrenWithGivenSemantics (expression, SqlExpressionContext.SingleValueRequired);
      }
      catch (NotSupportedException ex)
      {
        var message = string.Format (
            "The SQL 'IN' operator (originally probably a call to a 'Contains' method) requires a single value, so the following expression cannot "
            + "be translated to SQL: '{0}'.",
            expression);
        throw new NotSupportedException (message, ex);
      }
    }

    public Expression VisitAggregation (AggregationExpression expression)
    {
      Expression newInnerExpression;
      if (expression.AggregationModifier == AggregationModifier.Count)
        newInnerExpression = ApplyValueContext (expression.Expression);
      else
        newInnerExpression = ApplySingleValueContext (expression.Expression);

      if (newInnerExpression != expression.Expression)
        return new AggregationExpression (expression.Type, newInnerExpression, expression.AggregationModifier);

      return expression;
    }

    protected override Expression VisitInvocation (InvocationExpression expression)
    {
      var message = string.Format ("InvocationExpressions are not supported in the SQL backend. Expression: '{0}'.", expression);
      throw new NotSupportedException (message);
    }

    protected override Expression VisitLambda<T> (Expression<T> expression)
    {
      var message = string.Format ("LambdaExpressions are not supported in the SQL backend. Expression: '{0}'.", expression);
      throw new NotSupportedException (message);
    }

    private Expression VisitChildrenWithGivenSemantics (Expression expression, SqlExpressionContext childContext)
    {
      var visitor = new SqlContextExpressionVisitor (childContext, _stage, _context);
      return visitor.VisitExtension (expression);
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
      var newExpression = base.Visit (expression);
      
      // An SqlConvertedBooleanExpression is already a converted value (boolean to integer), so we don't need to convert again.
      if (newExpression is SqlConvertedBooleanExpression)
        return newExpression;

      // We don't adjust the results of local method calls, every value is a supported value here. This is a workaround, better solution in RM-5348.
      if (newExpression is MethodCallExpression)
        return newExpression;

      // A sub-select already returns a correct value; case in point: a boolean sub-select returns an integer, so we don't need to convert.
      if (newExpression is SqlSubStatementExpression)
        return newExpression;

      // We cannot use boolean expressions directly, because we always compare to integer -> convert boolean to integer using CASE WHEN clauses
      if (BooleanUtility.IsBooleanType (newExpression.Type))
      {
        var convertedExpression = CreateValueExpressionForPredicate (newExpression);
        return new SqlConvertedBooleanExpression (convertedExpression);
      }

      return newExpression;
    }

    private Expression HandlePredicateSemantics (Expression expression)
    {
      var newExpression = base.Visit (expression);

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
            expression);
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