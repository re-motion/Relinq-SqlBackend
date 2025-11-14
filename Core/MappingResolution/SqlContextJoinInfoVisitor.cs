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
  public class SqlContextJoinInfoVisitor : IJoinInfoVisitor
  {
    public static IJoinInfo ApplyContext (IJoinInfo joinInfo, SqlExpressionContext expressionContext, IMappingResolutionStage stage, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      var visitor = new SqlContextJoinInfoVisitor (stage, expressionContext, mappingResolutionContext);
      return joinInfo.Accept (visitor);
    }

    private readonly IMappingResolutionStage _stage;
    private readonly SqlExpressionContext _expressionContext;
    private IMappingResolutionContext _mappingResolutionContext;

    protected SqlContextJoinInfoVisitor (IMappingResolutionStage stage, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(expressionContext), expressionContext);
      ArgumentUtility.CheckNotNull (nameof(mappingResolutionContext), mappingResolutionContext);

      _stage = stage;
      _expressionContext = expressionContext;
      _mappingResolutionContext = mappingResolutionContext;
    }

    public IJoinInfo VisitUnresolvedJoinInfo (UnresolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);

      return joinInfo;
    }

    public IJoinInfo VisitUnresolvedCollectionJoinInfo (UnresolvedCollectionJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);

      return joinInfo;
    }

    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);

      var newTableInfo = (IResolvedTableInfo) _stage.ApplyContext (joinInfo.ForeignTableInfo, _expressionContext, _mappingResolutionContext); 
      if (joinInfo.ForeignTableInfo != newTableInfo)
        return new ResolvedJoinInfo (newTableInfo, joinInfo.JoinCondition);
      return joinInfo;
    }
  }
}