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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates sql-text for <see cref="ResolvedSimpleTableInfo"/> and <see cref="ResolvedJoinInfo"/>.
  /// </summary>
  public class SqlTableAndJoinTextGenerator : ITableInfoVisitor, IJoinInfoVisitor, ISqlTableBaseVisitor
  {
    public enum TableContextKind
    {
      FirstTable,
      NonFirstTable,
      JoinedTable
    } 
    
    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ISqlGenerationStage _stage;
    private readonly TableContextKind _tableContext;

    public static void GenerateSql (SqlTableBase sqlTable, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool first)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlTableAndJoinTextGenerator (commandBuilder, stage, first ? TableContextKind.FirstTable : TableContextKind.NonFirstTable);

      sqlTable.Accept (visitor);
      GenerateSqlForJoins (sqlTable, new SqlTableAndJoinTextGenerator (commandBuilder, stage, TableContextKind.JoinedTable));
    }

    protected SqlTableAndJoinTextGenerator (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, TableContextKind tableContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", tableContext);

      _commandBuilder = commandBuilder;
      _stage = stage;
      _tableContext = tableContext;
    }

    public ISqlCommandBuilder CommandBuilder
    {
      get { return _commandBuilder; }
    }

    public ISqlGenerationStage Stage
    {
      get { return _stage; }
    }

    public TableContextKind TableContext
    {
      get { return _tableContext; }
    }

    public void VisitSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      if (sqlTable.JoinSemantics == JoinSemantics.Inner)
      {
        if (_tableContext == TableContextKind.NonFirstTable)
        {
          _commandBuilder.Append (" CROSS ");
          if (sqlTable.TableInfo is ResolvedSimpleTableInfo)
            _commandBuilder.Append ("JOIN ");
          else
            _commandBuilder.Append ("APPLY ");
        }
      }
      else if (sqlTable.JoinSemantics == JoinSemantics.Left)
      {
        if (_tableContext == TableContextKind.FirstTable)
          _commandBuilder.Append ("(SELECT NULL AS [Empty]) AS [Empty]");
        _commandBuilder.Append (" OUTER APPLY ");
      }

      sqlTable.TableInfo.Accept (this);
    }

    public void VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      if (joinedTable.JoinSemantics == JoinSemantics.Left)
      {
        if (_tableContext == TableContextKind.FirstTable)
          _commandBuilder.Append ("(SELECT NULL AS [Empty]) AS [Empty]");

        _commandBuilder.Append (" LEFT OUTER JOIN ");
      }
      else
      {
        if (_tableContext != TableContextKind.FirstTable)
          _commandBuilder.Append (" INNER JOIN ");
      }

      joinedTable.JoinInfo.Accept (this);
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

    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      joinInfo.ForeignTableInfo.Accept (this);

      _commandBuilder.Append (" ON ");

      _stage.GenerateTextForJoinKeyExpression (_commandBuilder, joinInfo.LeftKey);
      _commandBuilder.Append (" = ");
      _stage.GenerateTextForJoinKeyExpression (_commandBuilder, joinInfo.RightKey);

      return joinInfo;
    }

    ITableInfo ITableInfoVisitor.VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      VisitSqlJoinedTable (joinedTable);
      return joinedTable;
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

    protected static void GenerateSqlForJoins (SqlTableBase sqlTable, SqlTableAndJoinTextGenerator visitor)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        joinedTable.Accept (visitor);
        GenerateSqlForJoins (joinedTable, visitor);
      }
    }
    
  }
}