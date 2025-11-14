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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Extends <see cref="SqlGeneratingExpressionVisitor"/> by emitting "AS" aliases for <see cref="NamedExpression"/> and 
  /// <see cref="SqlEntityExpression"/> instances. This should be used for the 
  /// <see cref="SqlStatement.SelectProjection"/> of substatements. For the outermost statement, <see cref="SqlGeneratingOuterSelectExpressionVisitor"/>
  /// should be used.
  /// </summary>
  public class SqlGeneratingSelectExpressionVisitor : SqlGeneratingExpressionVisitor, ISqlGroupingSelectExpressionVisitor
  {
    public static new void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      EnsureNoCollectionExpression (expression);

      var visitor = new SqlGeneratingSelectExpressionVisitor (commandBuilder, stage);
      visitor.Visit (expression);
    }

    protected static void EnsureNoCollectionExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (expression.Type != typeof (string) && !(expression is SqlGroupingSelectExpression) && typeof (IEnumerable).IsAssignableFrom (expression.Type))
      {
        var message = 
            "Queries selecting collections are not supported because SQL is not well-suited to returning collections. You can use "
            + "SelectMany or an additional 'from' clause to return the elements of the collection, grouping them in-memory."
            + Environment.NewLine
            + Environment.NewLine
            + "Ie., instead of 'from c in Cooks select c.Assistants', write the following query: "
            + Environment.NewLine
            + "'(from c in Cooks from a in Assistants select new { GroupID = c.ID, Element = a }).AsEnumerable().GroupBy (t => t.GroupID, t => t.Element)'"
            + Environment.NewLine
            + Environment.NewLine
            + "Note that above query will group the query result in-memory, which might be inefficient, depending on the number of results returned "
            + "by the query.";
        throw new NotSupportedException (message);
      }
    }

    protected SqlGeneratingSelectExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
        : base (commandBuilder, stage)
    {
    }

    public override Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      Visit (expression.Expression);
      CommandBuilder.Append (" AS ");
      CommandBuilder.AppendIdentifier (expression.Name ?? NamedExpression.DefaultName);
      
      return expression;
    }

    public virtual Expression VisitSqlGroupingSelect (SqlGroupingSelectExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var groupExpressions = new[] { expression.KeyExpression }.Concat (expression.AggregationExpressions);

      CommandBuilder.AppendSeparated (", ", groupExpressions, (cb, exp) => Visit (exp));

      return expression;
    }

    protected override void AppendColumnForEntity (SqlEntityExpression entity, SqlColumnExpression column)
    {
      Visit (column);
      
      string alias = GetAliasForColumnOfEntity(column, entity);
      if (alias != null)
      {
        CommandBuilder.Append (" AS ");
        CommandBuilder.AppendIdentifier (alias);
      }
    }

    protected string GetAliasForColumnOfEntity (SqlColumnExpression column, SqlEntityExpression entity)
    {
      if (column.ColumnName != "*")
      {
        if (entity.Name != null)
        {
          return entity.Name + "_" + column.ColumnName;
        }
        else if ((entity is SqlEntityReferenceExpression) && ((SqlEntityReferenceExpression) entity).ReferencedEntity.Name != null)
        {
          // entity references without a name that point to an entity with a name must assign aliases to their columns;
          // otherwise, their columns would include the referenced entity's name
          return column.ColumnName;
        }
      }
      return null;
    }
  }
}