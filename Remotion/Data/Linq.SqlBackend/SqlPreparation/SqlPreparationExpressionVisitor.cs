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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement.SelectProjection"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlPreparationExpressionVisitor : ExpressionTreeVisitor
  {
    private readonly SqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly MethodCallTransformerRegistry _registry;

    public static Expression TranslateExpression (
        Expression projection, SqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, registry);
      var result = visitor.VisitExpression (projection);
      return result;
    }

    protected SqlPreparationExpressionVisitor (SqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      _context = context;
      _stage = stage;
      _registry = registry;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var referencedTable = _context.GetSqlTableForQuerySource (expression.ReferencedQuerySource);
      return new SqlTableReferenceExpression (referencedTable);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlTableReferenceExpression)
      // then newExpression1.Cook => newExpression2 (SqlMemberExpression)
      // then newExpression2.FirstName => result (SqlMemberExpression)
      var newExpression = VisitExpression (expression.Expression);

      // kitchen case: newExpression is a SqlTableReferenceExpression (kitchenTable)
      // create a SqlMemberExpression (kitchenTable, "Cook")
      var newExpressionAsTableReference = newExpression as SqlTableReferenceExpression;
      if (newExpressionAsTableReference != null)
      {
        var sqlTable = newExpressionAsTableReference.SqlTable; // kitchenTable
        return new SqlMemberExpression (sqlTable, expression.Member); // kitchenTable.Cook
      }

      // kitchen.Cook case: newExpression is a SqlMemberExpression (kitchenTable, "Cook")
      // create a join kitchenTable => cookTable via "Cook" member
      // create a SqlMemberExpression (cookTable, "FirstName")
      var newExpressionAsSqlMemberExpression = newExpression as SqlMemberExpression;
      if (newExpressionAsSqlMemberExpression != null)
      {
        var originalSqlTable = newExpressionAsSqlMemberExpression.SqlTable; // kitchenTable

        // create cookTable via join
        var join = originalSqlTable.GetOrAddJoin (newExpressionAsSqlMemberExpression.MemberInfo, JoinCardinality.One); // "Cook"

        return new SqlMemberExpression (join, expression.Member); // cookTable.FirstName
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      var lastOperatorIndex = expression.QueryModel.ResultOperators.Count - 1;
      var containsOperator = lastOperatorIndex >= 0 ? expression.QueryModel.ResultOperators[lastOperatorIndex] as ContainsResultOperator : null;
      if (containsOperator != null)
      {
        // Check whether the query applies Contains to a constant collection
        if (expression.QueryModel.IsIdentityQuery()
            && (expression.QueryModel.MainFromClause.FromExpression is ConstantExpression)
            && typeof (ICollection).IsAssignableFrom (expression.QueryModel.MainFromClause.FromExpression.Type))
        {
          if (expression.QueryModel.ResultOperators.Count > 1)
            throw new NotSupportedException("Expression with more than one results operators are not allowed when using contains.");

          var preparedItemExpression = _stage.PrepareItemExpression (containsOperator.Item);
          return new SqlBinaryOperatorExpression ("IN", preparedItemExpression, expression.QueryModel.MainFromClause.FromExpression);
        }

        var preparedSqlStatement = _stage.PrepareSqlStatement (expression.QueryModel);

        // PrepareSqlStatement will handle the contains operator by putting an "IN" expression into the select projection
        Debug.Assert (
            preparedSqlStatement.SqlTables.Count == 0
            && preparedSqlStatement.WhereCondition == null
            && preparedSqlStatement.Orderings.Count == 0
            && !preparedSqlStatement.IsCountQuery
            && !preparedSqlStatement.IsDistinctQuery
            && preparedSqlStatement.TopExpression == null);

        return preparedSqlStatement.SelectProjection;
      }

      var sqlStatement = _stage.PrepareSqlStatement (expression.QueryModel);
      return new SqlSubStatementExpression (sqlStatement, expression.Type);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (IsNullConstant (expression.Left))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return VisitExpression (new SqlIsNullExpression (expression.Right));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return VisitExpression (new SqlIsNotNullExpression (expression.Right));
        else
          return base.VisitBinaryExpression (expression);
      }
      else if (IsNullConstant (expression.Right))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return VisitExpression (new SqlIsNullExpression (expression.Left));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return VisitExpression (new SqlIsNotNullExpression (expression.Left));
        else
          return base.VisitBinaryExpression (expression);
      }
      else
        return base.VisitBinaryExpression (expression);
      }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO Review 2511: Simply call base.VisitMethodCallExpression to replace the following lines
      var instanceExpression = VisitExpression (expression.Object);

      var arguments = new List<Expression>();
      foreach (var argument in expression.Arguments)
        arguments.Add (VisitExpression (argument));
      
      var newExpression = Expression.Call (instanceExpression, expression.Method, arguments);

      if ((expression.Object != instanceExpression) || (expression.Arguments.ToList() != arguments))
        return _registry.GetTransformer (expression.Method).Transform (newExpression);

      // TODO Review 2511: Everything up to here can be replaced by base.VisitMethodCallExpression; adapt the next line to use newExpression instead
      return _registry.GetTransformer (expression.Method).Transform (expression); // TODO Review 2511: Call VisitExpression on the result of Transform; test this. This is necessary so that a MethodCallTransformer can return an "unprepared" expression.
    }

    private bool IsNullConstant (Expression expression)
    {
      var constantExpression = expression as ConstantExpression;
      if (constantExpression != null)
      {
        if (constantExpression.Value == null)
          return true;
      }
      return false;
    }
  }
}