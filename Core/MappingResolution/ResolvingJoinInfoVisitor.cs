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
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingJoinInfoVisitor"/> modifies <see cref="UnresolvedJoinInfo"/>s and generates <see cref="ResolvedJoinInfo"/>s.
  /// </summary>
  public class ResolvingJoinInfoVisitor : IJoinInfoVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static ResolvedJoinInfo ResolveJoinInfo (
        IJoinInfo joinInfo,
        IMappingResolver resolver,
        UniqueIdentifierGenerator generator,
        IMappingResolutionStage stage,
        IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new ResolvingJoinInfoVisitor (resolver, generator, stage, context);
      return (ResolvedJoinInfo) joinInfo.Accept (visitor);
    }

    protected ResolvingJoinInfoVisitor (
        IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _resolver = resolver;
      _generator = generator;
      _stage = stage;
      _context = context;
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

      var resolvedExpression = _stage.ResolveCollectionSourceExpression (joinInfo.SourceExpression, _context);
      while (resolvedExpression is UnaryExpression)
        resolvedExpression = _stage.ResolveCollectionSourceExpression (((UnaryExpression)resolvedExpression).Operand, _context);

      var resolvedExpressionAsEntity = resolvedExpression as SqlEntityExpression;

      if (resolvedExpressionAsEntity != null)
      {
        var unresolvedJoinInfo = new UnresolvedJoinInfo (resolvedExpressionAsEntity, joinInfo.MemberInfo, JoinCardinality.Many);
        return unresolvedJoinInfo.Accept (this);
      }

      var message = string.Format (
          "Only entities can be used as the collection source in from expressions, '{0}' cannot. Member: '{1}'",
          resolvedExpression,
          joinInfo.MemberInfo);
      throw new NotSupportedException (message);
    }

    public IJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      var newForeignTableInfo = _stage.ResolveTableInfo (joinInfo.ForeignTableInfo, _context);
      if (newForeignTableInfo != joinInfo.ForeignTableInfo)
        return new ResolvedJoinInfo (newForeignTableInfo, joinInfo.JoinCondition);
      return joinInfo;
    }
  }
}