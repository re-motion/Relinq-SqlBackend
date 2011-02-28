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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public class SqlContextJoinInfoVisitor : IJoinInfoVisitor
  {
    public static IJoinInfo ApplyContext (IJoinInfo joinInfo, SqlExpressionContext expressionContext, IMappingResolutionStage stage, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlContextJoinInfoVisitor (stage, expressionContext, mappingResolutionContext);
      return joinInfo.Accept (visitor);
    }

    private readonly IMappingResolutionStage _stage;
    private readonly SqlExpressionContext _expressionContext;
    private IMappingResolutionContext _mappingResolutionContext;

    protected SqlContextJoinInfoVisitor (IMappingResolutionStage stage, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("expressionContext", expressionContext);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      _stage = stage;
      _expressionContext = expressionContext;
      _mappingResolutionContext = mappingResolutionContext;
    }

    public IJoinInfo VisitUnresolvedJoinInfo (UnresolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      return joinInfo;
    }

    public IJoinInfo VisitUnresolvedCollectionJoinInfo (UnresolvedCollectionJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      return joinInfo;
    }

    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      var newTableInfo = (IResolvedTableInfo) _stage.ApplyContext(joinInfo.ForeignTableInfo, _expressionContext, _mappingResolutionContext); 
      if (joinInfo.ForeignTableInfo != newTableInfo)
        return new ResolvedJoinInfo (newTableInfo, joinInfo.LeftKey, joinInfo.RightKey);
      return joinInfo;
    }
  }
}