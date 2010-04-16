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
  /// <summary>
  /// <see cref="ResolvingJoinInfoVisitor"/> modifies <see cref="UnresolvedJoinInfo"/>s and generates <see cref="ResolvedJoinInfo"/>s.
  /// </summary>
  public class ResolvingJoinInfoVisitor : IJoinInfoVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;

    public static ResolvedJoinInfo ResolveJoinInfo (IJoinInfo joinInfo, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new ResolvingJoinInfoVisitor (resolver, generator, stage);
      return (ResolvedJoinInfo) joinInfo.Accept (visitor);
    }

    protected ResolvingJoinInfoVisitor (IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      
      _resolver = resolver;
      _generator = generator;
      _stage = stage;
    }

    public IJoinInfo VisitUnresolvedJoinInfo (UnresolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      var result = _resolver.ResolveJoinInfo (joinInfo, _generator);
      return result.Accept (this);
    }

    public IJoinInfo VisitUnresolvedCollectionJoinInfo (UnresolvedCollectionJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      var newExpression = _stage.ResolveCollectionSourceExpression (joinInfo.SourceExpression) as SqlEntityExpression;

      if (newExpression != null)
      {
        var sqlTable = newExpression.SqlTable;
        var unresolvedJoinInfo = new UnresolvedJoinInfo (sqlTable, joinInfo.MemberInfo, JoinCardinality.Many);
        return unresolvedJoinInfo.Accept (this);
      }
      return joinInfo;
    }


    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      return joinInfo;
    }
  }
}