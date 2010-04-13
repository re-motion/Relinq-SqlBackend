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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates SQL text for a given <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlStatementTextGenerator
  {
    private readonly ISqlGenerationStage _stage;

    protected ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public SqlStatementTextGenerator (ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      
      _stage = stage;
    }

    public virtual void Build (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("selectedSqlContext", selectedSqlContext);
      
      BuildSelectPart (sqlStatement, commandBuilder, selectedSqlContext);
      BuildFromPart (sqlStatement, commandBuilder);
      BuildWherePart (sqlStatement, commandBuilder, SqlExpressionContext.PredicateRequired);
      BuildOrderByPart (sqlStatement, commandBuilder, selectedSqlContext);
    }

    protected virtual void BuildSelectPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      commandBuilder.Append ("SELECT ");

      if ((sqlStatement.IsCountQuery && sqlStatement.TopExpression != null) || (sqlStatement.IsCountQuery && sqlStatement.IsDistinctQuery))
        throw new NotSupportedException ("A SqlStatement cannot contain both Count and Top or Count and Distinct.");

      if (sqlStatement.IsCountQuery)
      {
        commandBuilder.Append ("COUNT(*)");
      }
      else
      {
        if (sqlStatement.IsDistinctQuery)
        {
          commandBuilder.Append ("DISTINCT ");
        }
        if (sqlStatement.TopExpression != null)
        {
          commandBuilder.Append ("TOP (");
          _stage.GenerateTextForTopExpression (commandBuilder, sqlStatement.TopExpression);
          commandBuilder.Append (") ");
        }

        _stage.GenerateTextForSelectExpression (commandBuilder, sqlStatement.SelectProjection, selectedSqlContext);

        if (!(sqlStatement.SelectProjection is SqlEntityExpression))
          commandBuilder.Append (" AS [value]");
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

    protected virtual void BuildWherePart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if ((sqlStatement.WhereCondition != null))
      {
        commandBuilder.Append (" WHERE ");

        _stage.GenerateTextForWhereExpression (commandBuilder, sqlStatement.WhereCondition);
      }
    }

    protected virtual void BuildOrderByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, SqlExpressionContext selectedSqlContext)
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
          {
            _stage.GenerateTextForOrderByExpression (commandBuilder, orderByClause.Expression);
          }

          commandBuilder.Append (string.Format (" {0}", orderByClause.OrderingDirection.ToString().ToUpper()));
          first = false;
        }
      }
    }
  }
}