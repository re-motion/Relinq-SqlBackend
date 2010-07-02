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

    public virtual Expression<Func<IDatabaseResultRow, object>> Build (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, bool outerStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      var lambdaExpression = BuildSelectPart (sqlStatement, commandBuilder, outerStatement);
      BuildFromPart (sqlStatement, commandBuilder);
      BuildWherePart (sqlStatement, commandBuilder);
      BuildGroupByPart (sqlStatement, commandBuilder);
      BuildOrderByPart (sqlStatement, commandBuilder);

      return lambdaExpression;
    }

    protected virtual Expression<Func<IDatabaseResultRow, object>> BuildSelectPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, bool outerStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      commandBuilder.Append ("SELECT ");

      if (!(sqlStatement.SelectProjection is AggregationExpression))
      {
        BuildDistinctPart (sqlStatement, commandBuilder);
        BuildTopPart (sqlStatement, commandBuilder);
      }

      if (outerStatement)
        return _stage.GenerateTextForOuterSelectExpression (commandBuilder, sqlStatement.SelectProjection);
      
      _stage.GenerateTextForSelectExpression (commandBuilder, sqlStatement.SelectProjection);
      return null;
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

    protected virtual void BuildGroupByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if (sqlStatement.GroupByExpression != null)
      {
        commandBuilder.Append (" GROUP BY ");

        _stage.GenerateTextForGroupByExpression (commandBuilder, sqlStatement.GroupByExpression);
      }
    }

    protected virtual void BuildOrderByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

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
    
  }
}