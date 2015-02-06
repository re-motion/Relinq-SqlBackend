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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates SQL text for <see cref="SqlTable"/> objects and its joined tables.
  /// </summary>
  public class SqlTableAndJoinTextGenerator : ITableInfoVisitor
  {
    public static void GenerateSql (SqlTable sqlTable, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      GenerateTextForSqlTable (new SqlTableAndJoinTextGenerator (commandBuilder, stage), sqlTable, commandBuilder, isFirstTable);
      GenerateSqlForJoins (sqlTable, commandBuilder, new SqlTableAndJoinTextGenerator (commandBuilder, stage), stage);
    }

    private static void GenerateSqlForJoins (SqlTable sqlTable, ISqlCommandBuilder commandBuilder, SqlTableAndJoinTextGenerator visitor, ISqlGenerationStage stage)
    {
      foreach (var join in sqlTable.OrderedJoins)
      {
        GenerateTextForJoin (visitor, @join, commandBuilder, stage);
        GenerateSqlForJoins (@join.JoinedTable, commandBuilder, visitor, stage);
      }
    }

    private static void GenerateTextForSqlTable (ITableInfoVisitor visitor, SqlTable sqlTable, ISqlCommandBuilder commandBuilder, bool isFirstTable)
    {
        // TODO RMLNQSQL-78: Move decision about CROSS JOIN, CROSS APPLY, or OUTER APPLY to SqlAppendedTable?
      if (sqlTable.JoinSemantics == JoinSemantics.Left)
      {
        if (isFirstTable)
          commandBuilder.Append ("(SELECT NULL AS [Empty]) AS [Empty]");
        commandBuilder.Append (" OUTER APPLY ");
      }
      else
      {
        if (!isFirstTable)
        {
          commandBuilder.Append (" CROSS ");
          if (sqlTable.TableInfo is ResolvedSimpleTableInfo)
            commandBuilder.Append ("JOIN ");
          else
            commandBuilder.Append ("APPLY ");
        }
      }

      sqlTable.TableInfo.Accept (visitor);
    }

    private static void GenerateTextForJoin (ITableInfoVisitor visitor, SqlJoin join, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      if (join.JoinSemantics == JoinSemantics.Inner)
        commandBuilder.Append (" INNER JOIN ");
      else
        commandBuilder.Append (" LEFT OUTER JOIN ");

      join.JoinedTable.TableInfo.Accept (visitor);
      commandBuilder.Append (" ON ");
      
      stage.GenerateTextForJoinCondition (commandBuilder, join.JoinCondition);
    }

    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ISqlGenerationStage _stage;

    protected SqlTableAndJoinTextGenerator (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _commandBuilder = commandBuilder;
      _stage = stage;
    }

    public ISqlCommandBuilder CommandBuilder
    {
      get { return _commandBuilder; }
    }

    public ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    ITableInfo ITableInfoVisitor.VisitUnresolvedGroupReferenceTableInfo (UnresolvedGroupReferenceTableInfo tableInfo)
    {
      throw new InvalidOperationException ("UnresolvedGroupReferenceTableInfo is not valid at this point.");
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      string[] identifiers = tableInfo.TableName.Split ('.');
      var newTableName = string.Join(".", identifiers.Select (idf => "[" + idf + "]").ToArray());
      
      _commandBuilder.Append (newTableName);
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
  }
}