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
  /// <see cref="SqlGeneratingExpressionVisitor"/> implements <see cref="ThrowingExpressionTreeVisitor"/> and <see cref="IResolvedSqlExpressionVisitor"/>.
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
        ISqlGroupingSelectExpressionVisitor,
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

    // TODO Review 2977: Use properties, not fields

    protected readonly ISqlCommandBuilder CommandBuilder;
    protected readonly BinaryExpressionTextGenerator BinaryExpressionTextGenerator;
    protected readonly ISqlGenerationStage Stage;

    protected SqlGeneratingExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      CommandBuilder = commandBuilder;
      BinaryExpressionTextGenerator = new BinaryExpressionTextGenerator (commandBuilder, this);
      Stage = stage;
    }

    public virtual Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.AppendSeparated (",", expression.Columns, (cb, column) => AppendColumnForEntity(expression, column));
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
        CommandBuilder.Append ("NULL");
      else if (expression.Value is ICollection)
      {
        CommandBuilder.Append ("(");

        var collection = (ICollection) expression.Value;
        if (collection.Count == 0)
          CommandBuilder.Append ("SELECT NULL WHERE 1 = 0");

        var items = collection.Cast<object>();
        CommandBuilder.AppendSeparated (", ", items, (cb, value) => cb.AppendParameter (value));
        CommandBuilder.Append (")");
      }
      else
      {
        var parameter = CommandBuilder.CreateParameter (expression.Value);
        CommandBuilder.Append (parameter.Name);
      }

      return expression;
    }

    public virtual Expression VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type == typeof (int))
        CommandBuilder.Append (expression.Value.ToString());
      else
        CommandBuilder.AppendStringLiteral ((string) expression.Value);
      return expression;
    }

    public virtual Expression VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.LeftExpression);
      CommandBuilder.Append (string.Format (" {0} ", expression.BinaryOperator));
      VisitExpression (expression.RightExpression);

      return expression;
    }

    public virtual Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      CommandBuilder.Append (" IS NULL");
      CommandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      CommandBuilder.Append (" IS NOT NULL");
      CommandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append (expression.SqlFunctioName);
      CommandBuilder.Append ("(");
      CommandBuilder.AppendSeparated (", ", expression.Args, (cb, exp) => VisitExpression (exp));
      CommandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append ("CONVERT");
      CommandBuilder.Append ("(");
      CommandBuilder.Append (expression.GetSqlTypeName());
      CommandBuilder.Append (", ");
      VisitExpression (expression.Source);
      CommandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append ("EXISTS");
      CommandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      CommandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlRowNumberExpression (SqlRowNumberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.Append ("ROW_NUMBER() OVER (ORDER BY ");
      CommandBuilder.AppendSeparated (", ", expression.Orderings, Stage.GenerateTextForOrdering);
      CommandBuilder.Append (")");

      return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      CommandBuilder.Append ("(");
      BinaryExpressionTextGenerator.GenerateSqlForBinaryExpression (expression);
      CommandBuilder.Append (")");
      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          if (expression.Operand.Type == typeof (bool))
            CommandBuilder.Append ("NOT ");
          else
            CommandBuilder.Append ("~");
          break;
        case ExpressionType.Negate:
          CommandBuilder.Append ("-");
          break;
        case ExpressionType.UnaryPlus:
          CommandBuilder.Append ("+");
          break;
        case ExpressionType.Convert:
          break;
        default:
          throw new NotSupportedException();
      }

      VisitExpression (expression.Operand);

      return expression;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotSupportedException (
          string.Format (
              "The expression '{0}' cannot be translated to SQL text by this SQL generator. Expression type '{1}' is not supported.",
              FormattingExpressionTreeVisitor.Format ((Expression) (object) unhandledItem),
              unhandledItem.GetType().Name));
    }

    public virtual Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      CommandBuilder.Append ("CASE WHEN ");
      VisitExpression (expression.TestPredicate);
      CommandBuilder.Append (" THEN ");
      VisitExpression (expression.ThenValue);
      CommandBuilder.Append (" ELSE ");
      VisitExpression (expression.ElseValue);
      CommandBuilder.Append (" END");
      return expression;
    }

    public virtual Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      CommandBuilder.Append ("(");
      Stage.GenerateTextForSqlStatement (CommandBuilder, expression.SqlStatement);
      CommandBuilder.Append (")");
      return expression;
    }

    public virtual Expression VisitSqlCustomTextGeneratorExpression (SqlCustomTextGeneratorExpressionBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      expression.Generate (CommandBuilder, this, Stage);
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

      if (expression.AggregationModifier == AggregationModifier.Count) {
        CommandBuilder.Append ("COUNT(*)");
        return expression;
      }

      if (expression.AggregationModifier == AggregationModifier.Average)
        CommandBuilder.Append("AVG");
      else if (expression.AggregationModifier == AggregationModifier.Max)
        CommandBuilder.Append ("MAX");
      else if (expression.AggregationModifier == AggregationModifier.Min)
        CommandBuilder.Append ("MIN");
      else if (expression.AggregationModifier == AggregationModifier.Sum)
        CommandBuilder.Append ("SUM");
      else
        throw new NotSupportedException (string.Format ("AggregationModifier '{0}' is not supported.", expression.AggregationModifier));

      CommandBuilder.Append ("(");

      // TODO 3032: Fix this check
      var nextExpression = expression.Expression is NamedExpression ? ((NamedExpression) expression.Expression).Expression : expression.Expression;
      VisitExpression (nextExpression);
      CommandBuilder.Append (")");

      return expression;
    }

    public virtual Expression VisitSqlGroupingSelectExpression (SqlGroupingSelectExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var groupExpressions = new[] { expression.KeyExpression }.Concat (expression.AggregationExpressions);

      CommandBuilder.AppendSeparated (", ", groupExpressions, (cb, exp) => VisitExpression (exp));

      return expression;
    }
    
    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      CommandBuilder.AppendSeparated (",", expression.Arguments, (cb, expr) => VisitExpression (expr));
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
        CommandBuilder.AppendIdentifier (prefix);
        CommandBuilder.Append (".*");
      }
      else
      {
        CommandBuilder.AppendIdentifier (prefix);
        CommandBuilder.Append (".");
        if (referencedEntityName != null)
          CommandBuilder.AppendIdentifier (referencedEntityName + "_" + (columnName ?? "value"));
        else
          CommandBuilder.AppendIdentifier (columnName ?? "value");
      }
    }
   
  }
}