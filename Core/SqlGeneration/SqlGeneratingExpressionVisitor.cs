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
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlGeneratingExpressionVisitor"/> generates SQL text for <see cref="Expression"/> trees that have been prepared and resolved.
  /// </summary>
  public class SqlGeneratingExpressionVisitor
      : ThrowingExpressionVisitor,
        IResolvedSqlExpressionVisitor,
        ISqlSpecificExpressionVisitor,
        ISqlSubStatementVisitor,
        ISqlCustomTextGeneratorExpressionVisitor,
        INamedExpressionVisitor,
        IAggregationExpressionVisitor,
        ISqlColumnExpressionVisitor,
        IConstantCollectionExpressionVisitor
  {
    public static void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      var visitor = new SqlGeneratingExpressionVisitor (commandBuilder, stage);
      visitor.Visit (expression);
    }

    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly BinaryExpressionTextGenerator _binaryExpressionTextGenerator;
    private readonly ISqlGenerationStage _stage;

    protected SqlGeneratingExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      _commandBuilder = commandBuilder;
      _binaryExpressionTextGenerator = new BinaryExpressionTextGenerator (commandBuilder, this);
      _stage = stage;
    }

    protected ISqlCommandBuilder CommandBuilder
    {
      get { return _commandBuilder; }
    }

    // ReSharper disable UnusedMember.Global
    protected ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    // ReSharper restore UnusedMember.Global

    public virtual Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.AppendSeparated (",", expression.Columns, (cb, column) => AppendColumnForEntity (expression, column));
      return expression;
    }

    public virtual Expression VisitSqlColumnDefinition (SqlColumnDefinitionExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      AppendColumn (expression.ColumnName, expression.OwningTableAlias, null);

      return expression;
    }

    public virtual Expression VisitSqlColumnReference (SqlColumnReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      SqlColumnExpression firstColumn = expression.ReferencedEntity.Columns.FirstOrDefault();
      string referencedEntityName = firstColumn != null && firstColumn.ColumnName == "*" ? null : expression.ReferencedEntity.Name;
      AppendColumn (expression.ColumnName, expression.OwningTableAlias, referencedEntityName);

      return expression;
    }

    protected override Expression VisitConstant (ConstantExpression expression)
    {
      Assertion.DebugAssert (expression.Type != typeof (bool), "Boolean constants should have been removed by SqlContextExpressionVisitor.");
      Assertion.DebugAssert (
          !typeof (ICollection).IsAssignableFrom (expression.Type),
          "Collections should have been replaced with SqlCollectionExpressions by SqlPreparationExpressionVisitor.");

      if (expression.Value == null)
      {
        _commandBuilder.Append ("NULL");
      }
      else
      {
        var parameter = _commandBuilder.GetOrCreateParameter (expression);
        _commandBuilder.Append (parameter.Name);
      }

      return expression;
    }

    public virtual Expression VisitSqlLiteral (SqlLiteralExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (expression.Type == typeof (string))
        _commandBuilder.AppendStringLiteral ((string) expression.Value);
      else
        _commandBuilder.Append (Convert.ToString (expression.Value, CultureInfo.InvariantCulture));
      return expression;
    }

    public virtual Expression VisitSqlIn (SqlInExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      Visit (expression.LeftExpression);
      _commandBuilder.Append (" IN ");
      Visit (expression.RightExpression);

      return expression;
    }

    public virtual Expression VisitSqlIsNull (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("(");
      Visit (expression.Expression);
      _commandBuilder.Append (" IS NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlIsNotNull (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("(");
      Visit (expression.Expression);
      _commandBuilder.Append (" IS NOT NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlFunction (SqlFunctionExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append (expression.SqlFunctioName);
      _commandBuilder.Append ("(");
      _commandBuilder.AppendSeparated (", ", expression.Args, (cb, exp) => Visit (exp));
      _commandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlConvert (SqlConvertExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("CONVERT");
      _commandBuilder.Append ("(");
      _commandBuilder.Append (expression.GetSqlTypeName());
      _commandBuilder.Append (", ");
      Visit (expression.Source);
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlExists (SqlExistsExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("EXISTS");
      _commandBuilder.Append ("(");
      Visit (expression.Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlRowNumber (SqlRowNumberExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("ROW_NUMBER() OVER (ORDER BY ");
      _commandBuilder.AppendSeparated (", ", expression.Orderings, _stage.GenerateTextForOrdering);
      _commandBuilder.Append (")");

      return expression;
    }

    public Expression VisitSqlLike (SqlLikeExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      Visit (expression.Left);
      _commandBuilder.Append (" LIKE ");
      Visit (expression.Right);
      _commandBuilder.Append (" ESCAPE ");
      Visit (expression.EscapeExpression);

      return expression;
    }

    public Expression VisitSqlLength (SqlLengthExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      // Since the SQL LEN function ignores trailing blanks, we add one character and subtract 1 from the result.
      // LEN (x + '#') - 1

      var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (object), typeof (object) });
      var extendedString = Expression.Add (
          expression.Expression.Type == typeof (char) ? Expression.Convert (expression.Expression, typeof (object)) : expression.Expression,
          new SqlLiteralExpression ("#"),
          concatMethod);

      var newExpression = Expression.Subtract (new SqlFunctionExpression (typeof (int), "LEN", extendedString), new SqlLiteralExpression (1));
      return Visit (newExpression);
    }

    public Expression VisitSqlCase (SqlCaseExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("CASE");

      foreach (var caseWhenPair in expression.Cases)
      {
        _commandBuilder.Append (" WHEN ");
        Visit (caseWhenPair.When);
        _commandBuilder.Append (" THEN ");
        Visit (caseWhenPair.Then);
      }

      if (expression.ElseCase != null)
      {
        _commandBuilder.Append (" ELSE ");
        Visit (expression.ElseCase);
      }
      _commandBuilder.Append (" END");
      return expression;
    }

    protected override Expression VisitBinary (BinaryExpression expression)
    {
      _commandBuilder.Append ("(");
      _binaryExpressionTextGenerator.GenerateSqlForBinaryExpression (expression);
      _commandBuilder.Append (")");
      return expression;
    }

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          if (BooleanUtility.IsBooleanType (expression.Operand.Type))
            _commandBuilder.Append ("NOT ");
          else
            _commandBuilder.Append ("~");
          break;
        case ExpressionType.Negate:
          _commandBuilder.Append ("-");
          break;
        case ExpressionType.UnaryPlus:
          _commandBuilder.Append ("+");
          break;
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          break;
        default:
          var message = string.Format ("Cannot generate SQL for unary expression '{0}'.", expression);
          throw new NotSupportedException (message);
      }

      Visit (expression.Operand);

      return expression;
    }

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom transformer has been registered. Expression: '{2}'",
          expression.Method.DeclaringType,
          expression.Method.Name,
          expression);
      throw new NotSupportedException (message);
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      var message = string.Format (
          "The expression '{0}' cannot be translated to SQL text by this SQL generator. Expression type '{1}' is not supported.",
          unhandledItem,
          unhandledItem.GetType().Name);

      throw new NotSupportedException (message);
    }

    public virtual Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, expression.SqlStatement);
      _commandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlCustomTextGenerator (SqlCustomTextGeneratorExpressionBase expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      expression.Generate (_commandBuilder, this, _stage);
      return expression;
    }

    public virtual Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      Visit (expression.Expression);

      return expression;
    }

    public virtual Expression VisitAggregation (AggregationExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (expression.AggregationModifier == AggregationModifier.Count)
      {
        _commandBuilder.Append ("COUNT(*)");
        return expression;
      }

      if (expression.AggregationModifier == AggregationModifier.Average)
        _commandBuilder.Append ("AVG");
      else if (expression.AggregationModifier == AggregationModifier.Max)
        _commandBuilder.Append ("MAX");
      else if (expression.AggregationModifier == AggregationModifier.Min)
        _commandBuilder.Append ("MIN");
      else if (expression.AggregationModifier == AggregationModifier.Sum)
        _commandBuilder.Append ("SUM");
      else
      {
        var message = string.Format ("Cannot generate SQL for aggregation '{0}'. Expression: '{1}'", expression.AggregationModifier, expression);
        throw new NotSupportedException (message);
      }

      _commandBuilder.Append ("(");

      Visit (expression.Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitConstantCollection (ConstantCollectionExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.Append ("(");
      _commandBuilder.AppendCollection (expression);
      _commandBuilder.Append (")");

      return expression;
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _commandBuilder.AppendSeparated (",", expression.Arguments, (cb, expr) => Visit (expr));
      return expression;
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumn (SqlColumnExpression expression)
    {
      return VisitExtension (expression);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlEntityConstant (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var message = string.Format (
          "It is not supported to use a constant entity object in any other context than to compare it with another entity. "
          + "Expression: {0} (of type: '{1}').",
          expression,
          expression.Type);
      throw new NotSupportedException (message);
    }

    protected virtual void AppendColumnForEntity (SqlEntityExpression entity, SqlColumnExpression column)
    {
      Visit (column);
    }

    protected virtual void AppendColumn (string columnName, string prefix, string referencedEntityName)
    {
      if (columnName == "*")
      {
        _commandBuilder.AppendIdentifier (prefix);
        _commandBuilder.Append (".*");
      }
      else
      {
        _commandBuilder.AppendIdentifier (prefix);
        _commandBuilder.Append (".");
        if (referencedEntityName != null)
          _commandBuilder.AppendIdentifier (referencedEntityName + "_" + (columnName ?? NamedExpression.DefaultName));
        else
          _commandBuilder.AppendIdentifier (columnName ?? NamedExpression.DefaultName);
      }
    }
  }
}