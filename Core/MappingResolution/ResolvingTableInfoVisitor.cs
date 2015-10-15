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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingTableInfoVisitor"/> modifies <see cref="UnresolvedTableInfo"/>s and generates <see cref="ResolvedSimpleTableInfo"/>s.
  /// </summary>
  public class ResolvingTableInfoVisitor : ITableInfoVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static IResolvedTableInfo ResolveTableInfo (ITableInfo tableInfo, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new ResolvingTableInfoVisitor (resolver, generator, stage, context);
      return (IResolvedTableInfo) tableInfo.Accept (visitor);
    }

    protected ResolvingTableInfoVisitor (IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _resolver = resolver;
      _generator = generator;
      _stage= stage;
      _context = context;
    }

    public IMappingResolver Resolver
    {
      get { return _resolver; }
    }

    public UniqueIdentifierGenerator Generator
    {
      get { return _generator; }
    }

    public IMappingResolutionStage Stage
    {
      get { return _stage; }
    }

    public IMappingResolutionContext Context
    {
      get { return _context; }
    }

    public ITableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      var result =  _resolver.ResolveTableInfo (tableInfo, _generator);
      return result.Accept (this);
    }

    public ITableInfo VisitUnresolvedJoinTableInfo (UnresolvedJoinTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      var result = _resolver.ResolveJoinTableInfo (tableInfo, _generator);
      return result.Accept (this);
    }

    public ITableInfo VisitUnresolvedCollectionJoinTableInfo (UnresolvedCollectionJoinTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      
      var resolvedExpression = _stage.ResolveCollectionSourceExpression (tableInfo.SourceExpression, _context);
      while (resolvedExpression is UnaryExpression)
      resolvedExpression = _stage.ResolveCollectionSourceExpression (((UnaryExpression)resolvedExpression).Operand, _context);

      var resolvedExpressionAsEntity = resolvedExpression as SqlEntityExpression;
      if (resolvedExpressionAsEntity != null)
      {
        var unresolvedJoinTableInfo = new UnresolvedJoinTableInfo (
            resolvedExpressionAsEntity,
            tableInfo.MemberInfo,
            JoinCardinality.Many);
        _context.AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (tableInfo, resolvedExpressionAsEntity);

        return unresolvedJoinTableInfo.Accept (this);
      }

      var message = string.Format (
          "Only entities can be used as the collection source in from expressions, '{0}' cannot. Member: '{1}'",
          resolvedExpression,
          tableInfo.MemberInfo);
      throw new NotSupportedException (message);
    }

    public ITableInfo VisitUnresolvedDummyRowTableInfo (UnresolvedDummyRowTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      var nullAsEmptyStatementBuilder = new SqlStatementBuilder();
      var selectProjection = new NamedExpression ("Empty", SqlLiteralExpression.Null (tableInfo.ItemType));
      nullAsEmptyStatementBuilder.SelectProjection = selectProjection;
      nullAsEmptyStatementBuilder.DataInfo = new StreamedSequenceInfo (
          typeof (IEnumerable<>).MakeGenericType (selectProjection.Type),
          selectProjection);

      return new ResolvedSubStatementTableInfo ("Empty", nullAsEmptyStatementBuilder.GetSqlStatement());
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      var newSqlStatement = _stage.ResolveSqlStatement (tableInfo.SqlStatement, _context);
      if (newSqlStatement.Equals (tableInfo.SqlStatement))
        return tableInfo;
      else
        return new ResolvedSubStatementTableInfo (tableInfo.TableAlias, newSqlStatement);
    }

    public ITableInfo VisitJoinedGroupingTableInfo (ResolvedJoinedGroupingTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      var newSqlStatement = _stage.ResolveSqlStatement (tableInfo.SqlStatement, _context);
      if (newSqlStatement.Equals (tableInfo.SqlStatement))
      {
        return tableInfo;
      }
      else
      {
        return new ResolvedJoinedGroupingTableInfo (
            tableInfo.TableAlias,
            newSqlStatement,
            tableInfo.AssociatedGroupingSelectExpression,
            tableInfo.GroupSourceTableAlias);
      }
    }

    public ITableInfo VisitUnresolvedGroupReferenceTableInfo (UnresolvedGroupReferenceTableInfo tableInfo)
    {
      var groupSourceSubStatementTableInfo = tableInfo.ReferencedGroupSource.GetResolvedTableInfo() as ResolvedSubStatementTableInfo;
      if (groupSourceSubStatementTableInfo == null)
      {
        var message = string.Format (
            "This SQL generator only supports sequences in from expressions if they are members of an entity or if they come from a GroupBy operator. "
            + "Sequence: '{0}'", tableInfo);
        throw new NotSupportedException (message);
      }

      var groupingSelectExpression = groupSourceSubStatementTableInfo.SqlStatement.SelectProjection as SqlGroupingSelectExpression;
      if (groupingSelectExpression == null)
      {
        throw new NotSupportedException (
            "When a sequence retrieved by a subquery is used in a from expression, the subquery must end with a GroupBy operator.");
      }

      var elementSelectingStatementBuilder = new SqlStatementBuilder (groupSourceSubStatementTableInfo.SqlStatement) { GroupByExpression = null };

      var currentKeyExpression = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (tableInfo.ReferencedGroupSource), 
          groupingSelectExpression.Type.GetProperty ("Key"));

      var groupKeyJoinCondition = _stage.ResolveWhereExpression (
          Expression.OrElse (
              Expression.AndAlso (new SqlIsNullExpression (groupingSelectExpression.KeyExpression), new SqlIsNullExpression (currentKeyExpression)),
              Expression.AndAlso (
                  Expression.AndAlso (
                      new SqlIsNotNullExpression (groupingSelectExpression.KeyExpression), 
                      new SqlIsNotNullExpression (currentKeyExpression)),
                  Expression.Equal (groupingSelectExpression.KeyExpression, currentKeyExpression))), 
          _context);
      elementSelectingStatementBuilder.AddWhereCondition (groupKeyJoinCondition);

      elementSelectingStatementBuilder.SelectProjection = groupingSelectExpression.ElementExpression;
      elementSelectingStatementBuilder.RecalculateDataInfo (groupingSelectExpression);

      return new ResolvedJoinedGroupingTableInfo (
          _generator.GetUniqueIdentifier("q"), 
          elementSelectingStatementBuilder.GetSqlStatement(), 
          groupingSelectExpression,
          groupSourceSubStatementTableInfo.TableAlias);
    }
  }
}