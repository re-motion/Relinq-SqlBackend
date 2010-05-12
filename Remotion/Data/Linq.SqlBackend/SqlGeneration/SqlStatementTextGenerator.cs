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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates SQL text for a resolved <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlStatementTextGenerator
  {
    private readonly ISqlGenerationStage _stage;

    public SqlStatementTextGenerator (ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);

      _stage = stage;
    }

    protected ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public virtual void Build (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      BuildSelectPart (sqlStatement, commandBuilder);
      BuildFromPart (sqlStatement, commandBuilder);
      BuildWherePart (sqlStatement, commandBuilder);
      BuildOrderByPart (sqlStatement, commandBuilder);
    }

    protected virtual void BuildSelectPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      commandBuilder.Append ("SELECT ");

      bool condition = !((sqlStatement.AggregationModifier != AggregationModifier.None && sqlStatement.TopExpression != null)
                         || (sqlStatement.AggregationModifier != AggregationModifier.None && sqlStatement.IsDistinctQuery));
      Debug.Assert (condition, "A SqlStatement cannot contain both aggregation and Top or aggregation and Distinct.");

      if (sqlStatement.AggregationModifier == AggregationModifier.None)
      {
        BuildDistinctPart (sqlStatement, commandBuilder);
        BuildTopPart (sqlStatement, commandBuilder);

        _stage.GenerateTextForSelectExpression (commandBuilder, sqlStatement.SelectProjection);
      }
      else if (sqlStatement.AggregationModifier == AggregationModifier.Count)
        commandBuilder.Append ("COUNT(*)");
      else
      {
        BuildAggregationPart (sqlStatement, commandBuilder);

        commandBuilder.Append ("(");
        _stage.GenerateTextForSelectExpression (
            commandBuilder,
            sqlStatement.SelectProjection is NamedExpression
                ? ((NamedExpression) sqlStatement.SelectProjection).Expression
                : sqlStatement.SelectProjection);
        commandBuilder.Append (")");
      }
    }

    protected virtual void BuildFromPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if (sqlStatement.SqlTables.Count > 0)
      {
        commandBuilder.Append (" FROM ");

        bool first = true;
        foreach (var sqlTable in sqlStatement.SqlTables)
        {
          _stage.GenerateTextForFromTable (commandBuilder, sqlTable, first);
          first = false;
        }
      }
    }

    protected virtual void BuildWherePart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if ((sqlStatement.WhereCondition != null))
      {
        commandBuilder.Append (" WHERE ");

        _stage.GenerateTextForWhereExpression (commandBuilder, sqlStatement.WhereCondition);
      }
    }

    protected virtual void BuildOrderByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if (sqlStatement.Orderings.Count > 0)
      {
        commandBuilder.Append (" ORDER BY ");

        bool first = true;
        foreach (var orderByClause in sqlStatement.Orderings)
        {
          if (!first)
            commandBuilder.Append (", ");

          if (orderByClause.Expression.NodeType == ExpressionType.Constant)
          {
            commandBuilder.Append ("(SELECT ");
            _stage.GenerateTextForOrderByExpression (commandBuilder, orderByClause.Expression);
            commandBuilder.Append (")");
          }
          else
            _stage.GenerateTextForOrderByExpression (commandBuilder, orderByClause.Expression);

          commandBuilder.Append (string.Format (" {0}", orderByClause.OrderingDirection.ToString().ToUpper()));
          first = false;
        }
      }
    }

    protected virtual void BuildTopPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      if (sqlStatement.TopExpression != null)
      {
        commandBuilder.Append ("TOP (");
        _stage.GenerateTextForTopExpression (commandBuilder, sqlStatement.TopExpression);
        commandBuilder.Append (") ");
      }
    }

    protected virtual void BuildDistinctPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      if (sqlStatement.IsDistinctQuery)
        commandBuilder.Append ("DISTINCT ");
    }

    protected virtual void BuildAggregationPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      if (sqlStatement.AggregationModifier == AggregationModifier.Average)
        commandBuilder.Append ("AVG");
      else if (sqlStatement.AggregationModifier == AggregationModifier.Max)
        commandBuilder.Append ("MAX");
      else if (sqlStatement.AggregationModifier == AggregationModifier.Min)
        commandBuilder.Append ("MIN");
      else if (sqlStatement.AggregationModifier == AggregationModifier.Sum)
        commandBuilder.Append ("SUM");
      else
        throw new NotSupportedException (string.Format ("AggregationModifier '{0}' is not supported.", sqlStatement.AggregationModifier));
    }
  }
}