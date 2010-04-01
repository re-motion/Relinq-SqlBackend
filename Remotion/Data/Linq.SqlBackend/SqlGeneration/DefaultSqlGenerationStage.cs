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
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Provides a default implementation of <see cref="ISqlGenerationStage"/>.
  /// </summary>
  public class DefaultSqlGenerationStage : ISqlGenerationStage
  {
    public DefaultSqlGenerationStage (MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("registry", registry);
      // ReSharper disable DoNotCallOverridableMethodsInConstructor
      // ReSharper restore DoNotCallOverridableMethodsInConstructor
    }

    public void GenerateTextForFromTable (SqlCommandBuilder commandBuilder, SqlTableBase table, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("table", table);

      SqlTableAndJoinTextGenerator.GenerateSql (table, commandBuilder, this, isFirstTable);
    }

    // TODO Review 2494: Add a single protected virtual method GenerateTextForExpression (SqlCommandBuilder, Expression, SqlExpressionContext); call that method from all GenerateTextFor...Expression methods
    // TODO Review 2494: Rewrite the tests for DefaultSqlGenerationStage using a partial mock that intercepts GenerateTextForExpression

    public void GenerateTextForSelectExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, selectedSqlContext, this);
    }

    public void GenerateTextForWhereExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, selectedSqlContext, this);
    }

    public void GenerateTextForOrderByExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, selectedSqlContext, this);
    }

    public void GenerateTextForTopExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, selectedSqlContext, this);
    }

    public void GenerateTextForSqlStatement (SqlCommandBuilder commandBuilder, SqlStatement sqlStatement, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      var sqlStatementTextGenerator = new SqlStatementTextGenerator (this);
      sqlStatementTextGenerator.Build (sqlStatement, commandBuilder, selectedSqlContext);
    }

    public void GenerateTextForJoinKeyExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, selectedSqlContext, this);
    }

  }
}