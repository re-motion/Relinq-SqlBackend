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
        ISqlColumnExpressionVisitor
  {
    public static void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, SqlGenerationMode mode)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlGeneratingExpressionVisitor (commandBuilder, stage, mode);
      visitor.VisitExpression (expression);
    }

    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly BinaryExpressionTextGenerator _binaryExpressionTextGenerator;
    private readonly ISqlGenerationStage _stage;
    private readonly SqlGenerationMode _mode;

    protected SqlGeneratingExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, SqlGenerationMode mode)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _commandBuilder = commandBuilder;
      _binaryExpressionTextGenerator = new BinaryExpressionTextGenerator (commandBuilder, this);
      _stage = stage;
      _mode = mode;
    }

    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.AppendSeparated (",", expression.Columns, (cb, column) => AppendColumnForEntity(expression, column));
      return expression;
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    public Expression VisitSqlColumnDefinitionExpression (SqlColumnDefinitionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      AppendColumn (expression.ColumnName, expression.OwningTableAlias, null);

      return expression;
    }

    public Expression VisitSqlColumnReferenceExpression (SqlColumnReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      AppendColumn (expression.ColumnName, expression.OwningTableAlias, expression.ReferencedEntity.Name);

      return expression;
    }

    public Expression VisitSqlValueReferenceExpression (SqlValueReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // becomes SqlColumnDefinitionExpression _or_ directly emit corresponding SQL
      var columnExpression = new SqlColumnDefinitionExpression (expression.Type, expression.TableAlias, expression.Name ?? "value", false);
      return VisitExpression (columnExpression);
    }

    public Expression VisitJoinConditionExpression (JoinConditionExpression expression)
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

    public Expression VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type == typeof (int))
        _commandBuilder.Append (expression.Value.ToString());
      else
        _commandBuilder.AppendStringLiteral ((string) expression.Value);
      return expression;
    }

    public Expression VisitSqlBinaryOperatorExpression (SqlBinaryOperatorExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.LeftExpression);
      _commandBuilder.Append (string.Format (" {0} ", expression.BinaryOperator));
      VisitExpression (expression.RightExpression);

      return expression;
    }

    public Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (" IS NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (" IS NOT NULL");
      _commandBuilder.Append (")");

      return expression;
    }

    public Expression VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append (expression.SqlFunctioName);
      _commandBuilder.Append ("(");
      _commandBuilder.AppendSeparated (", ", expression.Args, (cb, exp) => VisitExpression (exp));
      _commandBuilder.Append (")");
      return expression;
    }

    public Expression VisitSqlConvertExpression (SqlConvertExpression expression)
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

    public Expression VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("EXISTS");
      _commandBuilder.Append ("(");
      VisitExpression (expression.Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    public Expression VisitSqlRowNumberExpression (SqlRowNumberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _commandBuilder.Append ("ROW_NUMBER() OVER (ORDER BY ");
      bool first = true;
      foreach (var ordering in expression.Orderings)
      {
        if (!first)
            _commandBuilder.Append (", ");
        _stage.GenerateTextForOrderByExpression (_commandBuilder, ordering.Expression);
        _commandBuilder.Append (string.Format (" {0}", ordering.OrderingDirection.ToString().ToUpper()));
        first = false;
      }
      _commandBuilder.Append (")");

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
              unhandledItem,
              unhandledItem.GetType().Name));
    }

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      _commandBuilder.Append ("CASE WHEN ");
      VisitExpression (expression.TestPredicate);
      _commandBuilder.Append (" THEN ");
      VisitExpression (expression.ThenValue);
      _commandBuilder.Append (" ELSE ");
      VisitExpression (expression.ElseValue);
      _commandBuilder.Append (" END");
      return expression;
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, expression.SqlStatement);
      _commandBuilder.Append (")");
      return expression;
    }

    public Expression VisitSqlCustomTextGeneratorExpression (SqlCustomTextGeneratorExpressionBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      expression.Generate (_commandBuilder, this, _stage);
      return expression;
    }

    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.Expression);
      if (_mode == SqlGenerationMode.SelectExpression)
      {
        _commandBuilder.Append (" AS ");
        _commandBuilder.AppendIdentifier (expression.Name ?? "value");
      }

      return expression;
    }

    public Expression VisitAggregationExpression (AggregationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.AggregationModifier == AggregationModifier.Count) {
        _commandBuilder.Append ("COUNT(*)");
        return expression;
      }

      if (expression.AggregationModifier == AggregationModifier.Average)
        _commandBuilder.Append("AVG");
      else if (expression.AggregationModifier == AggregationModifier.Max)
        _commandBuilder.Append ("MAX");
      else if (expression.AggregationModifier == AggregationModifier.Min)
        _commandBuilder.Append ("MIN");
      else if (expression.AggregationModifier == AggregationModifier.Sum)
        _commandBuilder.Append ("SUM");
      else
        throw new NotSupportedException (string.Format ("AggregationModifier '{0}' is not supported.", expression.AggregationModifier));

      _commandBuilder.Append ("(");
      VisitExpression (((NamedExpression) expression.Expression).Expression);
      _commandBuilder.Append (")");

      return expression;
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      bool first = true;
      foreach (var expr in expression.Arguments)
      {
        if (!first)
          _commandBuilder.Append (",");
        first = false;
        VisitExpression (expr);
      }
      return expression;
    }

    private void AppendColumnForEntity (SqlEntityExpression entity, SqlColumnExpression column)
    {
      column.Accept (this);
      if (_mode == SqlGenerationMode.SelectExpression)
      {
        if (entity.Name != null)
        {
          _commandBuilder.Append (" AS ");
          _commandBuilder.AppendIdentifier (entity.Name + "_" + column.ColumnName);
        }
        else if ((entity is SqlEntityReferenceExpression) && ((SqlEntityReferenceExpression) entity).ReferencedEntity.Name != null)
        {
          // entity references without a name that point to an entity with a name must assign aliases to their columns;
          // otherwise, their columns would include the referenced entity's name
          _commandBuilder.Append (" AS ");
          _commandBuilder.AppendIdentifier (column.ColumnName);
        }
      }
    }

    private void AppendColumn (string columnName, string prefix, string referencedEntityName)
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

    //private void AppendReferencedMember (ResolvedSubStatementTableInfo subStatementTableInfo, MemberInfo memberInfo)
    //{
    //  var column = new SqlColumnDefinitionExpression (typeof (int), subStatementTableInfo.TableAlias, memberInfo.Name, false);
    //  VisitExpression (column);
    //}
    
  }
}