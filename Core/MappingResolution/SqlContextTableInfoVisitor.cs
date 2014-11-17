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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  public class SqlContextTableInfoVisitor : ITableInfoVisitor
  {
    public static ITableInfo ApplyContext (ITableInfo tableInfo, SqlExpressionContext expressionContext, IMappingResolutionStage stage, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      var visitor = new SqlContextTableInfoVisitor (stage, expressionContext, mappingResolutionContext);
      return tableInfo.Accept (visitor);
    }

    private readonly IMappingResolutionStage _stage;
    private readonly SqlExpressionContext _expressionContext;
    private readonly IMappingResolutionContext _mappingResolutionContext;

    protected SqlContextTableInfoVisitor (IMappingResolutionStage stage, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      _stage = stage;
      _expressionContext = expressionContext;
      _mappingResolutionContext = mappingResolutionContext;
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      var newStatement = _stage.ApplySelectionContext (tableInfo.SqlStatement, _expressionContext, _mappingResolutionContext);
      if (newStatement != tableInfo.SqlStatement)
        return new ResolvedSubStatementTableInfo (tableInfo.TableAlias, newStatement);
      return tableInfo;
    }

    public ITableInfo VisitJoinedGroupingTableInfo (ResolvedJoinedGroupingTableInfo tableInfo)
    {
      var newStatement = _stage.ApplySelectionContext (tableInfo.SqlStatement, _expressionContext, _mappingResolutionContext);
      if (newStatement != tableInfo.SqlStatement)
        return new ResolvedJoinedGroupingTableInfo (
            tableInfo.TableAlias, 
            newStatement, 
            tableInfo.AssociatedGroupingSelectExpression, 
            tableInfo.GroupSourceTableAlias);
      return tableInfo;
    }

    public ITableInfo VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      var newJoinInfo = _stage.ApplyContext (joinedTable.JoinInfo, _expressionContext, _mappingResolutionContext); 
      joinedTable.JoinInfo = newJoinInfo;

      return joinedTable;
    }

    public ITableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      throw new InvalidOperationException ("UnresolvedTableInfo is not valid at this point.");
    }

    public ITableInfo VisitUnresolvedJoinTableInfo (UnresolvedJoinTableInfo tableInfo)
    {
      throw new InvalidOperationException ("UnresolvedJoinTableInfo is not valid at this point.");
    }

    public ITableInfo VisitUnresolvedGroupReferenceTableInfo (UnresolvedGroupReferenceTableInfo tableInfo)
    {
      throw new InvalidOperationException ("UnresolvedGroupReferenceTableInfo is not valid at this point.");
    }
  }
}