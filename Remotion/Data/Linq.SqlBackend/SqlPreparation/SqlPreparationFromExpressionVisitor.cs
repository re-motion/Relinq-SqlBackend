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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Analyzes the <see cref="FromClauseBase.FromExpression"/> of a <see cref="FromClauseBase"/> and returns a <see cref="SqlTableBase"/> that 
  /// represents the data source of the <see cref="FromClauseBase"/>.
  /// </summary>
  public class SqlPreparationFromExpressionVisitor : SqlPreparationExpressionVisitor, ISqlSubStatementVisitor, IUnresolvedSqlExpressionVisitor
  {
    public static FromExpressionInfo AnalyzeFromExpression (
        Expression fromExpression,
        IQuerySource querySource,
        ISqlPreparationStage stage,
        UniqueIdentifierGenerator generator,
        MethodCallTransformerRegistry registry,
        ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("registry", registry);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlPreparationFromExpressionVisitor (querySource, generator, stage, registry, context);
      var result = visitor.VisitExpression (fromExpression);
      var resultAsTableReferenceExpression = result as SqlTableReferenceExpression;
      if (resultAsTableReferenceExpression != null)
        return new FromExpressionInfo (resultAsTableReferenceExpression.SqlTable, visitor._extractedOrderings.ToArray (), resultAsTableReferenceExpression, visitor._whereCondition);

      var message = string.Format ("Expressions of type '{0}' cannot be used as the SqlTables of a from clause.", result.GetType().Name);
      throw new NotSupportedException (message);
    }

    private readonly IQuerySource _querySource;
    private readonly UniqueIdentifierGenerator _generator;
    //private Expression _itemSelector;
    private List<Ordering> _extractedOrderings;
    private Expression _whereCondition;

    protected SqlPreparationFromExpressionVisitor (
        IQuerySource querySource,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        MethodCallTransformerRegistry registry,
        ISqlPreparationContext context)
        : base (context, stage, registry)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _querySource = querySource;
      _generator = generator;

      _extractedOrderings = new List<Ordering>();
    }
    
    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlTable = new SqlTable (new UnresolvedTableInfo (_querySource.ItemType));
      return new SqlTableReferenceExpression (sqlTable);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var joinedTable = new SqlJoinedTable (new UnresolvedCollectionJoinInfo (expression.Expression, expression.Member), JoinSemantics.Inner);

      _whereCondition = new JoinConditionExpression (joinedTable);
      var oldStyleJoinedTable = new SqlTable (joinedTable);
      return new SqlTableReferenceExpression (oldStyleJoinedTable);
    }

    public new Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      // Note: This is a case where the SQL preparation stage already generates a resolved table info (including a table alias) rather than passing
      // on an unresolved table info to the mapping resolution stage. Should we ever have the need to resolve subqueries in the mapping resolution 
      // stage, we should refactor this into an UnresolvedSubStatemenTableInfo. (Of course, the statement inside the ResolvedSubStatementTableInfo is 
      // resolved anyway.)

      var sqlStatement = expression.SqlStatement;
      //if (expression.SqlStatement.Orderings.Count > 0 && expression.SqlStatement.TopExpression == null)
      //{
      //  Expression newSelectProjection = Expression.Constant (null);
      //  var tupleType = typeof (KeyValuePair<,>);

      //  for (int i = sqlStatement.Orderings.Count - 1; i >= 0; --i)
      //  {
      //    tupleType = typeof (KeyValuePair<,>).MakeGenericType (newSelectProjection.Type, sqlStatement.Orderings[i].Expression.Type);

      //    newSelectProjection = Expression.New (tupleType.MakeGenericType (sqlStatement.Orderings[i].Expression.Type, newSelectProjection.Type).GetConstructors ()[0],
      //      new[] { sqlStatement.Orderings[i].Expression, newSelectProjection },
      //      new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
      //  }

      //  newSelectProjection = Expression.New (
      //      tupleType.MakeGenericType (sqlStatement.SelectProjection.Type, newSelectProjection.Type).GetConstructors ()[0],
      //        new[] { sqlStatement.SelectProjection, newSelectProjection },
      //        new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

      //  var builder = new SqlStatementBuilder (expression.SqlStatement) { SelectProjection = newSelectProjection };
      //  builder.Orderings.Clear ();
      //  builder.RecalculateDataInfo (expression.SqlStatement.SelectProjection);
      //  var newSqlStatement = builder.GetSqlStatement ();

      //  var sqlTable = new SqlTable (new ResolvedSubStatementTableInfo (_generator.GetUniqueIdentifier ("q"), newSqlStatement));
      //  ItemSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression(sqlTable), newSelectProjection.Type.GetProperty ("Key"));
        
      //  var currentOrderingTuple = Expression.MakeMemberAccess (new SqlTableReferenceExpression(sqlTable), newSelectProjection.Type.GetProperty ("Value"));
      //  for (int i = 0; i < sqlStatement.Orderings.Count; ++i)
      //  {
      //    ExtractedOrderings.AddOrdering (Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Key")));
      //    currentOrderingTuple = Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Value"));
      //  }
      //  return new SqlTableReferenceExpression (sqlTable);
      //}
      //else
      //{
        var tableInfo = new ResolvedSubStatementTableInfo (_generator.GetUniqueIdentifier ("q"), sqlStatement);
        var sqlTable = new SqlTable (tableInfo);
        return new SqlTableReferenceExpression (sqlTable);
      //}
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression;
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }
    
  }
}