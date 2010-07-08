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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
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

    public ITableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      var result =  _resolver.ResolveTableInfo (tableInfo, _generator);
      return result.Accept (this);
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
      return new ResolvedSubStatementTableInfo (tableInfo.TableAlias, newSqlStatement);
    }

    public ITableInfo VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      var resolvedJoinInfo = _stage.ResolveJoinInfo (joinedTable.JoinInfo, _context);
      joinedTable.JoinInfo = resolvedJoinInfo;

      return joinedTable.GetResolvedTableInfo ();
    }

    public ITableInfo VisitUnresolvedGroupReferenceTableInfo (UnresolvedGroupReferenceTableInfo tableInfo)
    {
      var groupSourceSubStatementTableInfo = tableInfo.ReferencedGroupSource.GetResolvedTableInfo() as ResolvedSubStatementTableInfo;
      if (groupSourceSubStatementTableInfo == null)
      {
        throw new NotSupportedException (
            "This SQL generator only supports sequences in from expressions if they are members of an entity or if they come from a GroupBy "
            + "operator.");
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