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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingSelectExpressionVisitor"/> is used to resolve sql select projection expressions in the mapping resolutin stage.
  /// </summary>
  public class ResolvingSelectExpressionVisitor : ResolvingExpressionVisitor
  {
    private readonly SqlStatementBuilder _sqlStatementBuilder;

    public static Expression ResolveExpression (
        Expression expression,
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator,
        SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);

      var entityIdentityResolver = new EntityIdentityResolver (stage, resolver, context);
      var comparisonSplitter = new CompoundExpressionComparisonSplitter ();
      var namedExpressionCombiner = new NamedExpressionCombiner (context);
      var groupAggregateSimplifier = new GroupAggregateSimplifier (stage, context);

      var visitor1 = new ResolvingSelectExpressionVisitor (
          resolver,
          stage,
          context,
          generator,
          entityIdentityResolver,
          comparisonSplitter,
          namedExpressionCombiner,
          groupAggregateSimplifier,
          false,
          sqlStatementBuilder);
      var result1 = visitor1.Visit (expression);

      var visitor2 = new ResolvingSelectExpressionVisitor (
          resolver,
          stage,
          context,
          generator,
          entityIdentityResolver,
          comparisonSplitter,
          namedExpressionCombiner,
          groupAggregateSimplifier,
          true,
          sqlStatementBuilder);
      var result2 = visitor2.Visit (result1);

      return result2;
    }

    protected ResolvingSelectExpressionVisitor (
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator, 
        IEntityIdentityResolver entityIdentityResolver,
        ICompoundExpressionComparisonSplitter comparisonSplitter,
        INamedExpressionCombiner namedExpressionCombiner,
        IGroupAggregateSimplifier groupAggregateSimplifier,
        bool resolveEntityRefMemberExpressions,
        SqlStatementBuilder sqlStatementBuilder)
        : base (
            resolver,
            stage,
            context,
            generator,
            entityIdentityResolver,
            comparisonSplitter,
            namedExpressionCombiner,
            groupAggregateSimplifier,
            resolveEntityRefMemberExpressions)
    {
      _sqlStatementBuilder = sqlStatementBuilder;
    }

    public override Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      var newExpression = base.VisitSqlSubStatement (expression);
      var newExpressionAsSqlSubStatementExpression = newExpression as SqlSubStatementExpression;

      // Substatements returning a single value need to be moved to the FROM part of the SQL statement because they might return more than one column.
      // Since a SqlSubStatementExpression must return a single row anyway, we can do this.
      // (However, errors that would have arisen because the statement does not return exactly one row will not be found.)
      if (newExpressionAsSqlSubStatementExpression != null
          && newExpressionAsSqlSubStatementExpression.SqlStatement.DataInfo is StreamedSingleValueInfo)
      {
        var appendedTable = newExpressionAsSqlSubStatementExpression.ConvertToSqlTable (Generator.GetUniqueIdentifier ("q"));
        var sqlTableReferenceExpression = new SqlTableReferenceExpression (appendedTable.SqlTable);

        Context.AddSqlTable (appendedTable, _sqlStatementBuilder);
        return Visit (sqlTableReferenceExpression);
      }
      return newExpression;
    }
  }
}