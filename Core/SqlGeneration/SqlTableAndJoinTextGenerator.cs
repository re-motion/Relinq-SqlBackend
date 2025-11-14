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
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates sql-text for <see cref="ResolvedSimpleTableInfo"/> and <see cref="ResolvedJoinInfo"/>.
  /// </summary>
  public class SqlTableAndJoinTextGenerator : ITableInfoVisitor, IJoinInfoVisitor
  {
    public static void GenerateSql (SqlTable sqlTable, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      GenerateTextForSqlTable (new SqlTableAndJoinTextGenerator (commandBuilder, stage), sqlTable, commandBuilder, isFirstTable);
      GenerateSqlForJoins (sqlTable, commandBuilder, new SqlTableAndJoinTextGenerator (commandBuilder, stage));
    }

    private static void GenerateSqlForJoins (SqlTableBase sqlTable, ISqlCommandBuilder commandBuilder, IJoinInfoVisitor visitor)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        GenerateTextForSqlJoinedTable (visitor, joinedTable, commandBuilder);
        GenerateSqlForJoins (joinedTable, commandBuilder, visitor);
      }
    }

    private static void GenerateTextForSqlTable (ITableInfoVisitor visitor, SqlTable sqlTable, ISqlCommandBuilder commandBuilder, bool isFirstTable)
    {
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

    private static void GenerateTextForSqlJoinedTable (IJoinInfoVisitor visitor, SqlJoinedTable joinedTable, ISqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(joinedTable), joinedTable);

      // TODO RMLNQSQL-4: This check can be removed.
      if (joinedTable.JoinSemantics == JoinSemantics.Inner)
        throw new NotSupportedException ("SqlJoinedTables with INNER JoinSemantics are currently not supported. (RMLNQSQL-4)");

      commandBuilder.Append (" LEFT OUTER JOIN ");

      joinedTable.JoinInfo.Accept (visitor);
    }

    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ISqlGenerationStage _stage;

    protected SqlTableAndJoinTextGenerator (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

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
      ArgumentUtility.CheckNotNull (nameof(tableInfo), tableInfo);

      string[] identifiers = tableInfo.TableName.Split ('.');
      var newTableName = string.Join(".", identifiers.Select (idf => "[" + idf + "]").ToArray());
      
      _commandBuilder.Append (newTableName);
      _commandBuilder.Append (" AS ");
      _commandBuilder.AppendIdentifier (tableInfo.TableAlias);

      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(tableInfo), tableInfo);

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

    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);

      joinInfo.ForeignTableInfo.Accept (this);

      _commandBuilder.Append (" ON ");

      _stage.GenerateTextForJoinCondition (_commandBuilder, joinInfo.JoinCondition);

      return joinInfo;
    }

    // TODO RMLNQSQL-4: This method can be removed.
    ITableInfo ITableInfoVisitor.VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      throw new InvalidOperationException ("SqlJoinedTable as TableInfo is not valid at this point.");
    }

    ITableInfo ITableInfoVisitor.VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      throw new InvalidOperationException ("UnresolvedTableInfo is not valid at this point.");
    }

    IJoinInfo IJoinInfoVisitor.VisitUnresolvedJoinInfo (UnresolvedJoinInfo tableSource)
    {
      throw new InvalidOperationException ("UnresolvedJoinInfo is not valid at this point.");
    }

    IJoinInfo IJoinInfoVisitor.VisitUnresolvedCollectionJoinInfo (UnresolvedCollectionJoinInfo joinInfo)
    {
      throw new InvalidOperationException ("UnresolvedCollectionJoinInfo is not valid at this point.");
    }
  }
}