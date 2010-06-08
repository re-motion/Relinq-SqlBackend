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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Provides a default implementation of <see cref="ISqlGenerationStage"/>.
  /// </summary>
  public class DefaultSqlGenerationStage : ISqlGenerationStage
  {
    public virtual void GenerateTextForFromTable (ISqlCommandBuilder commandBuilder, SqlTableBase table, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("table", table);

      SqlTableAndJoinTextGenerator.GenerateSql (table, commandBuilder, this, isFirstTable);
    }

    public virtual void GenerateTextForSelectExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (string) && typeof (IEnumerable).IsAssignableFrom (expression.Type))
        throw new NotSupportedException ("Subquery selects a collection where a single value is expected.");

      GenerateTextForExpression (commandBuilder, expression, SqlGenerationMode.SelectExpression);
    }

    public virtual void GenerateTextForWhereExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForExpression (commandBuilder, expression, SqlGenerationMode.NonSelectExpression);
    }

    public virtual void GenerateTextForOrderByExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForExpression (commandBuilder, expression, SqlGenerationMode.NonSelectExpression);
    }

    public virtual void GenerateTextForTopExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForExpression (commandBuilder, expression, SqlGenerationMode.NonSelectExpression);
    }

    public virtual void GenerateTextForJoinKeyExpression (ISqlCommandBuilder commandBuilder, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      GenerateTextForExpression (commandBuilder, expression, SqlGenerationMode.NonSelectExpression);
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
      sqlStatementTextGenerator.Build (sqlStatement, commandBuilder);
    }

    protected virtual void GenerateTextForExpression (ISqlCommandBuilder commandBuilder, Expression expression, SqlGenerationMode sqlGenerationMode)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, this, sqlGenerationMode);
    }
  }
}