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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlGeneratingExpressionVisitor"/> generates SQL text for <see cref="Expression"/> trees that have been prepared and resolved.
  /// </summary>
  public class SqlGeneratingExpressionVisitor
      : ThrowingExpressionTreeVisitor,
        IResolvedSqlExpressionVisitor,
        ISqlSpecificExpressionVisitor,
        ISqlSubStatementVisitor,
        IJoinConditionExpressionVisitor,
        ISqlCustomTextGeneratorExpressionVisitor,
        INamedExpressionVisitor,
        IAggregationExpressionVisitor,
        ISqlColumnExpressionVisitor,
        IConvertedBooleanExpressionVisitor
  {
    public static void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlGeneratingExpressionVisitor (commandBuilder, stage);
      visitor.VisitExpression (expression);
    }

    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly BinaryExpressionTextGenerator _binaryExpressionTextGenerator;
    private readonly ISqlGenerationStage _stage;

    protected SqlGeneratingExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _commandBuilder = commandBuilder;
      _binaryExpressionTextGenerator = new BinaryExpressionTextGenerator (commandBuilder, this);
      _stage = stage;
    }

    protected ISqlCommandBuilder CommandBuilder
    {
      get { return _commandBuilder; }
    }

    protected ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public virtual Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.AppendSeparated (",", expression.Columns, (cb, column) => AppendColumnForEntity (expression, column));
      return expression;
    }

    public virtual Expression VisitSqlColumnDefinitionExpression (SqlColumnDefinitionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      AppendColumn (expression.ColumnName, expression.OwningTableAlias, null);

      return expression;
    }

    public virtual Expression VisitSqlColumnReferenceExpression (SqlColumnReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlColumnExpression firstColumn = expression.ReferencedEntity.Columns.FirstOrDefault();
      string referencedEntityName = firstColumn != null && firstColumn.ColumnName == "*" ? null : expression.ReferencedEntity.Name;
      AppendColumn (expression.ColumnName, expression.OwningTableAlias, referencedEntityName);

      return expression;
    }

    public virtual Expression VisitJoinConditionExpression (JoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var whereExpression = Expression.Equal (
          expression.JoinedTable.JoinInfo.GetResolvedLeftJoinInfo().LeftKey,
          expression.JoinedTable.JoinInfo.GetResolvedLeftJoinInfo().RightKey);
      return VisitExpression (whereExpression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      Debug.Assert (expression.Type != typeof (bool), "Boolean constants should have been removed by SqlContextExpressionVisitor.");

      if (expression.Value == null)
        _commandBuilder.Append ("NULL");
      else if (expression.Value is ICollection)
      {
        _commandBuilder.Append ("(");

        var collection = (ICollection) expression.Value;
        if (collection.Count == 0)
          _commandBuilder.Append ("SELECT NULL WHERE 1 = 0");

        var items = collection.Cast<object>();
        _commandBuilder.AppendSeparated (", ", items, (cb, value) => cb.AppendParameter (value));
        _commandBuilder.Append (")");
      }
      else
      {
        var parameter = _commandBuilder.CreateParameter (expression.Value);
        _commandBuilder.Append (parameter.Name);
      }

      return expression;
    }

    public virtual Expression VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type == typeof (int))
        _commandBuilder.Append (expression.Value.ToString());
      else
        _commandBuilder.AppendStringLiteral ((string) expression.Value);
      return expression;
    }

    public virtual Expression VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.LeftExpression);
      _commandBuilder.Append (string.Format (" {0} ", expression.BinaryOperator));
      VisitExpression (expression.RightExpression);

      return expression;
    }

    public virtual Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (" IS NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (" IS NOT NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append (expression.SqlFunctioName);
      _commandBuilder.Append ("(");
      _commandBuilder.AppendSeparated (", ", expression.Args, (cb, exp) => VisitExpression (exp));
      _commandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("CONVERT");
      _commandBuilder.Append ("(");
      _commandBuilder.Append (expression.GetSqlTypeName());
      _commandBuilder.Append (", ");
      VisitExpression (expression.Source);
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("EXISTS");
      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlRowNumberExpression (SqlRowNumberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("ROW_NUMBER() OVER (ORDER BY ");
      _commandBuilder.AppendSeparated (", ", expression.Orderings, _stage.GenerateTextForOrdering);
      _commandBuilder.Append (")");

      return expression;
    }

    public Expression VisitSqlLikeExpression (SqlLikeExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.Left);
      _commandBuilder.Append (" LIKE ");
      VisitExpression (expression.Right);
      _commandBuilder.Append (" ESCAPE ");
      VisitExpression (expression.EscapeExpression);

      return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      _commandBuilder.Append ("(");
      _binaryExpressionTextGenerator.GenerateSqlForBinaryExpression (expression);
      _commandBuilder.Append (")");
      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          if (expression.Operand.Type == typeof (bool))
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
          var message = string.Format ("Cannot generate SQL for unary expression '{0}'.", FormattingExpressionTreeVisitor.Format (expression));
          throw new NotSupportedException (message);
      }

      VisitExpression (expression.Operand);

      return expression;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      var message = string.Format (
          "The expression '{0}' cannot be translated to SQL text by this SQL generator. Expression type '{1}' is not supported.",
          FormattingExpressionTreeVisitor.Format ((Expression) (object) unhandledItem),
          unhandledItem.GetType().Name);

      throw new NotSupportedException (message);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      _commandBuilder.Append ("CASE WHEN ");
      VisitExpression (expression.Test);
      _commandBuilder.Append (" THEN ");
      VisitExpression (expression.IfTrue);
      _commandBuilder.Append (" ELSE ");
      VisitExpression (expression.IfFalse);
      _commandBuilder.Append (" END");
      return expression;
    }

    public virtual Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, expression.SqlStatement);
      _commandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlCustomTextGeneratorExpression (SqlCustomTextGeneratorExpressionBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      expression.Generate (_commandBuilder, this, _stage);
      return expression;
    }

    public virtual Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.Expression);

      return expression;
    }

    public virtual Expression VisitAggregationExpression (AggregationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

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
        var message = string.Format (
            "Cannot generate SQL for aggregation '{0}'. Expression: '{1}'", 
            expression.AggregationModifier, 
            FormattingExpressionTreeVisitor.Format (expression));
        throw new NotSupportedException (message);
      }

      _commandBuilder.Append ("(");

      VisitExpression (expression.Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.AppendSeparated (",", expression.Arguments, (cb, expr) => VisitExpression (expr));
      return expression;
    }

    public virtual Expression VisitConvertedBooleanExpression (ConvertedBooleanExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.Expression);
      return expression;
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    protected virtual void AppendColumnForEntity (SqlEntityExpression entity, SqlColumnExpression column)
    {
      column.Accept (this);
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
          _commandBuilder.AppendIdentifier (referencedEntityName + "_" + (columnName ?? "value"));
        else
          _commandBuilder.AppendIdentifier (columnName ?? "value");
      }
    }
  }
}