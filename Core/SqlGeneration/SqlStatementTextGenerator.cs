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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates SQL text for a resolved <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlStatementTextGenerator
  {
    private readonly ISqlGenerationStage _stage;

    public SqlStatementTextGenerator (ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      _stage = stage;
    }

    protected ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public virtual void Build (
        SqlStatement sqlStatement, 
        ISqlCommandBuilder commandBuilder, 
        bool isOutermostStatement)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      BuildSelectPart (sqlStatement, commandBuilder, isOutermostStatement);
      BuildFromPart (sqlStatement, commandBuilder);
      BuildWherePart (sqlStatement, commandBuilder);
      BuildGroupByPart (sqlStatement, commandBuilder);
      BuildOrderByPart (sqlStatement, commandBuilder);
      BuildSetOperationCombinedStatementsPart (sqlStatement, commandBuilder);
    }

    protected virtual void BuildSelectPart (
        SqlStatement sqlStatement,
        ISqlCommandBuilder commandBuilder,
        bool isOutermostStatement)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      commandBuilder.Append ("SELECT ");

      if (!(sqlStatement.SelectProjection is AggregationExpression))
      {
        BuildDistinctPart (sqlStatement, commandBuilder);
        BuildTopPart (sqlStatement, commandBuilder);
      }

      if (isOutermostStatement)
      {
        var setOperationsMode = sqlStatement.SetOperationCombinedStatements.Any()
            ? SetOperationsMode.StatementIsSetCombined
            : SetOperationsMode.StatementIsNotSetCombined;
        _stage.GenerateTextForOuterSelectExpression (commandBuilder, sqlStatement.SelectProjection, setOperationsMode);
      }
      else
      {
        _stage.GenerateTextForSelectExpression (commandBuilder, sqlStatement.SelectProjection);
      }
    }

    protected virtual void BuildFromPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      if (sqlStatement.SqlTables.Count > 0)
      {
        commandBuilder.Append (" FROM ");

        bool isFirstTable = true;
        foreach (var sqlTable in sqlStatement.SqlTables)
        {
          _stage.GenerateTextForFromTable (commandBuilder, sqlTable, isFirstTable);
          isFirstTable = false;
        }
      }
    }

    protected virtual void BuildWherePart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      if ((sqlStatement.WhereCondition != null))
      {
        commandBuilder.Append (" WHERE ");

        _stage.GenerateTextForWhereExpression (commandBuilder, sqlStatement.WhereCondition);
      }
    }

    protected virtual void BuildGroupByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      if (sqlStatement.GroupByExpression != null)
      {
        commandBuilder.Append (" GROUP BY ");

        _stage.GenerateTextForGroupByExpression (commandBuilder, sqlStatement.GroupByExpression);
      }
    }

    protected virtual void BuildOrderByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      if (sqlStatement.Orderings.Count > 0)
      {
        commandBuilder.Append (" ORDER BY ");
        commandBuilder.AppendSeparated (", ", sqlStatement.Orderings, _stage.GenerateTextForOrdering);
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

    protected virtual void BuildSetOperationCombinedStatementsPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);

      foreach (var combinedStatement in sqlStatement.SetOperationCombinedStatements)
      {
        switch (combinedStatement.SetOperation)
        {
          case SetOperation.Union:
            commandBuilder.Append (" UNION (");
            break;

          case SetOperation.UnionAll:
            commandBuilder.Append (" UNION ALL (");
            break;

          default:
            throw new InvalidOperationException ("Invalid enum value: " + combinedStatement.SetOperation);
        }

        _stage.GenerateTextForSqlStatement (commandBuilder, combinedStatement.SqlStatement);
        commandBuilder.Append (")");
      }
    }
   
  }
}