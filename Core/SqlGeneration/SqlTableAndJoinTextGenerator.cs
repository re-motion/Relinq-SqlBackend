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
using System.Collections.Generic;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates SQL text for <see cref="SqlTable"/> objects and its joined tables.
  /// </summary>
  public class SqlTableAndJoinTextGenerator
  {
    private readonly ISqlGenerationStage _stage;

    public SqlTableAndJoinTextGenerator (ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);

      _stage = stage;
    }

    public ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public void Build (SqlAppendedTable table, ISqlCommandBuilder commandBuilder, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("table", table);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      var tableInfoVisitor = new TableInfoVisitor (commandBuilder, _stage);
      if (isFirstTable)
        GenerateTextForFirstTable (tableInfoVisitor, commandBuilder, table);
      else
        GenerateTextForAppendedTable (tableInfoVisitor, commandBuilder, table);
    }

    private void GenerateTextForFirstTable (ITableInfoVisitor visitor, ISqlCommandBuilder commandBuilder, SqlAppendedTable table)
    {
      if (table.JoinSemantics == JoinSemantics.Left)
      {
        commandBuilder.Append ("(SELECT NULL AS [Empty]) AS [Empty]");
        commandBuilder.Append (" OUTER APPLY ");
      }

      table.SqlTable.TableInfo.Accept (visitor);
      GenerateTextForJoins (visitor, commandBuilder, table.SqlTable.Joins);
    }

    private void GenerateTextForAppendedTable (ITableInfoVisitor visitor, ISqlCommandBuilder commandBuilder, SqlAppendedTable table)
    {
      // TODO RMLNQSQL-78: Move decision about CROSS JOIN, CROSS APPLY, or OUTER APPLY to SqlAppendedTable?
      if (table.JoinSemantics == JoinSemantics.Left)
        commandBuilder.Append (" OUTER APPLY ");
      else if (table.SqlTable.TableInfo is ResolvedSimpleTableInfo)
        commandBuilder.Append (" CROSS JOIN ");
      else
        commandBuilder.Append (" CROSS APPLY ");

      table.SqlTable.TableInfo.Accept (visitor);
      GenerateTextForJoins (visitor, commandBuilder, table.SqlTable.Joins);
    }

    private void GenerateTextForJoinedTable (ITableInfoVisitor visitor, ISqlCommandBuilder commandBuilder, SqlJoin join)
    {
      if (join.JoinSemantics == JoinSemantics.Left)
        commandBuilder.Append (" LEFT OUTER JOIN ");
      else
        commandBuilder.Append (" INNER JOIN ");

      join.JoinedTable.TableInfo.Accept (visitor);
      GenerateTextForJoins (visitor, commandBuilder, join.JoinedTable.Joins);

      commandBuilder.Append (" ON ");
      _stage.GenerateTextForJoinCondition (commandBuilder, join.JoinCondition);
    }

    private void GenerateTextForJoins (ITableInfoVisitor visitor, ISqlCommandBuilder commandBuilder, IEnumerable<SqlJoin> joins)
    {
      foreach (var join in joins)
      {
        if (join.JoinCondition is NullJoinConditionExpression)
          GenerateTextForAppendedTable (visitor, commandBuilder, new SqlAppendedTable (join.JoinedTable, join.JoinSemantics));
        else
          GenerateTextForJoinedTable (visitor, commandBuilder, join);
      }
    }

    private class TableInfoVisitor : ITableInfoVisitor
    {
      private readonly ISqlCommandBuilder _commandBuilder;
      private readonly ISqlGenerationStage _stage;

      public TableInfoVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
      {
        ArgumentUtility.DebugCheckNotNull ("commandBuilder", commandBuilder);
        ArgumentUtility.DebugCheckNotNull ("stage", stage);

        _commandBuilder = commandBuilder;
        _stage = stage;
      }

      public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
      {
        ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

        var identifiers = tableInfo.TableName.Split ('.');

        _commandBuilder.AppendSeparated (".", identifiers, (commandBuilder, identifier) => commandBuilder.AppendIdentifier (identifier));
        _commandBuilder.Append (" AS ");
        _commandBuilder.AppendIdentifier (tableInfo.TableAlias);

        return tableInfo;
      }

      public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
      {
        ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

        _commandBuilder.Append ("(");
        _stage.GenerateTextForSqlStatement (_commandBuilder, tableInfo.SqlStatement);
        _commandBuilder.Append (")");
        _commandBuilder.Append (" AS ");
        _commandBuilder.AppendIdentifier (tableInfo.TableAlias);

        return tableInfo;
      }

      public ITableInfo VisitJoinedGroupingTableInfo (ResolvedJoinedGroupingTableInfo tableInfo)
      {
        ArgumentUtility.DebugCheckNotNull ("tableInfo", tableInfo);

        return VisitSubStatementTableInfo (tableInfo);
      }

      ITableInfo ITableInfoVisitor.VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
      {
        throw new InvalidOperationException ("UnresolvedTableInfo is not valid at this point.");
      }

      ITableInfo ITableInfoVisitor.VisitUnresolvedJoinTableInfo (UnresolvedJoinTableInfo tableInfo)
      {
        throw new InvalidOperationException ("UnresolvedJoinTableInfo is not valid at this point.");
      }

      ITableInfo ITableInfoVisitor.VisitUnresolvedCollectionJoinTableInfo (UnresolvedCollectionJoinTableInfo tableInfo)
      {
        throw new InvalidOperationException ("UnresolvedCollectionJoinTableInfo is not valid at this point.");
      }

      ITableInfo ITableInfoVisitor.VisitUnresolvedDummyRowTableInfo (UnresolvedDummyRowTableInfo tableInfo)
      {
        throw new InvalidOperationException ("UnresolvedDummyRowTableInfo is not valid at this point.");
      }

      ITableInfo ITableInfoVisitor.VisitUnresolvedGroupReferenceTableInfo (UnresolvedGroupReferenceTableInfo tableInfo)
      {
        throw new InvalidOperationException ("UnresolvedGroupReferenceTableInfo is not valid at this point.");
      }
    }
  }
}