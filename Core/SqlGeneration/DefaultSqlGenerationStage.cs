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
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Provides a default implementation of <see cref="ISqlGenerationStage"/>.
  /// </summary>
  public class DefaultSqlGenerationStage : ISqlGenerationStage
  {
    public virtual void GenerateTextForFromTable (ISqlCommandBuilder commandBuilder, SqlAppendedTable table, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("table", table);

      SqlTableAndJoinTextGenerator.GenerateSql (table, commandBuilder, this, isFirstTable);
    }

    public virtual void GenerateTextForSelectExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, commandBuilder, this);
    }

    public virtual void GenerateTextForOuterSelectExpression (
        ISqlCommandBuilder commandBuilder,
        Expression expression,
        SetOperationsMode setOperationsMode)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (expression, commandBuilder, this, setOperationsMode);
    }

    public virtual void GenerateTextForWhereExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForNonSelectExpression (commandBuilder, expression);
    }

    public virtual void GenerateTextForOrderByExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForNonSelectExpression (commandBuilder, expression);
    }

    public virtual void GenerateTextForTopExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForNonSelectExpression (commandBuilder, expression);
    }

    public virtual void GenerateTextForJoinCondition (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForNonSelectExpression (commandBuilder, expression);
    }

    public void GenerateTextForGroupByExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForNonSelectExpression (commandBuilder, expression);
    }

    public void GenerateTextForOrdering (ISqlCommandBuilder commandBuilder, Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      
      if (ordering.Expression.NodeType == ExpressionType.Constant || ordering.Expression is SqlLiteralExpression)
      {
        commandBuilder.Append ("(SELECT ");
        GenerateTextForOrderByExpression (commandBuilder, ordering.Expression);
        commandBuilder.Append (")");
      }
      else
        GenerateTextForOrderByExpression (commandBuilder, ordering.Expression);

      commandBuilder.AppendFormat (string.Format (" {0}", ordering.OrderingDirection.ToString ().ToUpper ()));
    }

    public virtual void GenerateTextForSqlStatement (ISqlCommandBuilder commandBuilder, SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      var sqlStatementTextGenerator = new SqlStatementTextGenerator (this);
      sqlStatementTextGenerator.Build (sqlStatement, commandBuilder, false);
    }

    public virtual void GenerateTextForOuterSqlStatement (ISqlCommandBuilder commandBuilder, SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      var sqlStatementTextGenerator = new SqlStatementTextGenerator (this);
      sqlStatementTextGenerator.Build (sqlStatement, commandBuilder, true);
    }

    protected virtual void GenerateTextForNonSelectExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, this);
    }
  }
}