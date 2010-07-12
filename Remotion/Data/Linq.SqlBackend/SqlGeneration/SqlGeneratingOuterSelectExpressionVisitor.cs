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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  // TODO Review 2977: Missing docs
  public class SqlGeneratingOuterSelectExpressionVisitor : SqlGeneratingSelectExpressionVisitor
  {
    public static new Expression<Func<IDatabaseResultRow, object>> GenerateSql (
        Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlGeneratingOuterSelectExpressionVisitor (commandBuilder, stage);
      visitor.VisitExpression (expression);

      // TODO Review 2977: Move this to SqlCommandBuilder.GetInMemoryProjection()
      if (visitor.ProjectionExpression != null)
        return Expression.Lambda<Func<IDatabaseResultRow, object>> (
            Expression.Convert (visitor.ProjectionExpression, typeof (object)), visitor.RowParameter);
      return null;
    }

    // TODO Review 2977: Move this to SqlCommandBuilder
    protected readonly ParameterExpression RowParameter = Expression.Parameter (typeof (IDatabaseResultRow), "row");
    // TODO Review 2977: Use commandBuilder.InMemoryProjectionBody instead
    protected Expression ProjectionExpression;
    // TODO Review 2977: Make private
    protected int ColumnPosition;

    protected SqlGeneratingOuterSelectExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
        : base (commandBuilder, stage)
    {
    }

    public override Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO Review 2977: Cache this
      var getValueMethod = RowParameter.Type.GetMethod ("GetValue");
      ProjectionExpression = Expression.Call (
          RowParameter, 
          getValueMethod.MakeGenericMethod (expression.Type), 
          Expression.Constant (new ColumnID (expression.Name ?? "value", ColumnPosition++)));
      // TODO Review 2977: Extract and reuse ColumnID construction

      return base.VisitNamedExpression (expression);
    }

    public override Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var getEntityMethod = RowParameter.Type.GetMethod ("GetEntity");
      ProjectionExpression = Expression.Call (
          RowParameter,
          getEntityMethod.MakeGenericMethod (expression.Type),
          Expression.Constant (expression.Columns.Select (e => new ColumnID (e.ColumnName ?? "value", ColumnPosition++)).ToArray()));

      return base.VisitSqlEntityExpression (expression);
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var projectionExpressions = new List<Expression>();
      CommandBuilder.AppendSeparated (",", expression.Arguments, (cb, expr) => projectionExpressions.Add (VisitArgumentExpression (expr)));

      // TODO Review 2977: Check this warning
      if (expression.Members == null)
        ProjectionExpression = Expression.New (expression.Constructor, projectionExpressions);
      else
        ProjectionExpression = Expression.New (expression.Constructor, projectionExpressions, expression.Members);

      return expression;
    }

    public override Expression VisitConvertedBooleanExpression (ConvertedBooleanExpression expression)
    {
      base.VisitConvertedBooleanExpression (expression);

      if (ProjectionExpression != null)
      {
        // TODO Review 2977: Cache this method; add assertion that the ProjectionExpression is of type int
        var toBooleanMethod = typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) });
        ProjectionExpression = Expression.Call (toBooleanMethod, ProjectionExpression);
      }

      return expression;
    }

    public override Expression VisitSqlGroupingSelectExpression (SqlGroupingSelectExpression expression)
    {
      throw new NotSupportedException (
          "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to "
          + "efficiently return "
          + "LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional from clause, or perform "
          + "an aggregation on the groupings. "
          + Environment.NewLine
          + Environment.NewLine
          + "Eg., instead of: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name', "
          + Environment.NewLine + "write: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
          + Environment.NewLine + " from c in groupedCooks select new { Key = groupedCooks.Key, Item = c }', "
          + Environment.NewLine + "or: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
          + Environment.NewLine + " select new { Key = groupedCooks.Key, Count = groupedCooks.Count() }'.");
    }

    private Expression VisitArgumentExpression (Expression argumentExpression)
    {
      VisitExpression (argumentExpression);
      return ProjectionExpression;
    }
  }
}