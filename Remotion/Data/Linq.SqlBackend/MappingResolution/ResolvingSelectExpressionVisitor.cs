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
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
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

      var visitor = new ResolvingSelectExpressionVisitor (resolver, stage, context, generator, sqlStatementBuilder);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected ResolvingSelectExpressionVisitor (
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator,
        SqlStatementBuilder sqlStatementBuilder)
        : base (resolver, stage, context, generator)
    {
      _sqlStatementBuilder = sqlStatementBuilder;
    }

    public override Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      var newExpression = base.VisitSqlSubStatementExpression (expression);
      var newExpressionAsSqlSubStatementExpression = newExpression as SqlSubStatementExpression;

      // Substatements returning a single value need to be moved to the FROM part of the SQL statement because they might return more than one column.
      // Since a SqlSubStatementExpression must return a single row anyway, we can do this.
      // (However, errors that might arise because the statement does not return exactly one row, will not be found.)
      if (newExpressionAsSqlSubStatementExpression != null
          && newExpressionAsSqlSubStatementExpression.SqlStatement.DataInfo is StreamedSingleValueInfo)
      {
        var sqlTable = expression.CreateSqlTableForSubStatement (newExpressionAsSqlSubStatementExpression, JoinSemantics.Left, Generator.GetUniqueIdentifier ("q"));
        var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);

        Context.AddSqlTable (sqlTable, _sqlStatementBuilder);
        return VisitExpression(sqlTableReferenceExpression);
      }
      return newExpression;
    }
  }
}