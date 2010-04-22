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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlContextTableVisitor"/> applies <see cref="SqlExpressionContext"/> to <see cref="ResolvedSubStatementTableInfo"/>s 
  /// in the specified <see cref="SqlTableBase"/>.
  /// </summary>
  public class SqlContextTableVisitor : ISqlTableBaseVisitor, ITableInfoVisitor
  {
    private readonly ISqlContextResolutionStage _stage;
    private SqlExpressionContext _context;

    public static void ApplyContext (SqlTableBase sqlTableBase, SqlExpressionContext context, ISqlContextResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("sqlTableBase", sqlTableBase);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlContextTableVisitor (stage, context);
      sqlTableBase.Accept (visitor);
    }

    public SqlContextTableVisitor (ISqlContextResolutionStage stage, SqlExpressionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);

      _stage = stage;
      _context = context;
    }

    public void VisitSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      ITableInfo newTableInfo = sqlTable.TableInfo.Accept (this);
      if (newTableInfo != sqlTable.TableInfo)
        sqlTable.TableInfo = newTableInfo;
    }

    public void VisitSqlJoinedTable (SqlJoinedTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      var newStatement = _stage.ApplyContext (tableInfo.SqlStatement, _context);
      if (newStatement != tableInfo.SqlStatement)
        return new ResolvedSubStatementTableInfo (tableInfo.TableAlias, newStatement);
      return tableInfo;
    }

    public ITableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      return tableInfo;
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      return tableInfo;
    }
  }
}