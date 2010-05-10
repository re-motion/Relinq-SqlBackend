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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates sql-text for <see cref="ResolvedSimpleTableInfo"/> and <see cref="ResolvedJoinInfo"/>.
  /// </summary>
  public class SqlTableAndJoinTextGenerator : ITableInfoVisitor, IJoinInfoVisitor, ISqlTableBaseVisitor
  {
    public enum Context
    {
      FirstTable,
      NonFirstTable,
      JoinedTable
    } 
    
    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ISqlGenerationStage _stage;
    private readonly Context _context;

    public static void GenerateSql (SqlTableBase sqlTable, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool first)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlTableAndJoinTextGenerator (commandBuilder, stage, first ? Context.FirstTable : Context.NonFirstTable);

      sqlTable.Accept (visitor);
      GenerateSqlForJoins (sqlTable, new SqlTableAndJoinTextGenerator (commandBuilder, stage, Context.JoinedTable));
    }

    private static void GenerateSqlForJoins (SqlTableBase sqlTable, SqlTableAndJoinTextGenerator visitor)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        joinedTable.Accept (visitor);
        GenerateSqlForJoins (joinedTable, visitor);
      }
    }

    protected SqlTableAndJoinTextGenerator (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, Context context)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _commandBuilder = commandBuilder;
      _stage = stage;
      _context = context;
    }

    public void VisitSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      sqlTable.TableInfo.Accept (this);
    }

    public void VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      if (joinedTable.JoinSemantics == JoinSemantics.Left)
      {
        if (_context == Context.FirstTable)
          _commandBuilder.Append ("(SELECT NULL AS [Empty]) AS [Empty]");

        _commandBuilder.Append (" LEFT OUTER JOIN ");
      }
      else
      {
        if (_context != Context.FirstTable)
          _commandBuilder.Append (" INNER JOIN ");
      }

      joinedTable.JoinInfo.Accept (this);
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      
      if (_context == Context.NonFirstTable)
        _commandBuilder.Append (" CROSS JOIN "); //TODO: move to VisitSqlTable

      _commandBuilder.AppendIdentifier (tableInfo.TableName);
      _commandBuilder.Append (" AS ");
      _commandBuilder.AppendIdentifier (tableInfo.TableAlias);

      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      if (_context == Context.NonFirstTable)
        _commandBuilder.Append (" CROSS APPLY "); //TODO: emit "CROSS JOIN" here and move to VisitSqlTable

      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, tableInfo.SqlStatement);
      _commandBuilder.Append (")");
      _commandBuilder.Append (" AS ");
      _commandBuilder.AppendIdentifier (tableInfo.TableAlias);

      return tableInfo;
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
    
  }
}