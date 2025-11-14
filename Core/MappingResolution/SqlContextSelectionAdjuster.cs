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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlContextSelectionAdjuster"/> applies a <see cref="SqlExpressionContext"/> to a <see cref="SqlStatement"/> by modifying
  /// the <see cref="SqlStatement"/>'s <see cref="SqlStatement.SelectProjection"/> and recalculating all dependent data accordingly.
  /// </summary>
  public class SqlContextSelectionAdjuster
  {
    private readonly IMappingResolutionStage _stage;
    private IMappingResolutionContext _mappingResolutionContext;

    public static SqlStatement ApplyContext (SqlStatement sqlStatement, SqlExpressionContext expressionContext, IMappingResolutionStage stage, IMappingResolutionContext mappingresolutionContext)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(mappingresolutionContext), mappingresolutionContext);

      var visitor = new SqlContextSelectionAdjuster (stage, mappingresolutionContext);
      return visitor.VisitSqlStatement (sqlStatement, expressionContext);
    }

    private SqlContextSelectionAdjuster (IMappingResolutionStage stage, IMappingResolutionContext mappingresolutionContext)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(mappingresolutionContext), mappingresolutionContext);

      _stage = stage;
      _mappingResolutionContext = mappingresolutionContext;
    }
    
    public SqlStatement VisitSqlStatement (SqlStatement sqlStatement, SqlExpressionContext expressionContext)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);

      if (expressionContext == SqlExpressionContext.PredicateRequired)
        throw new InvalidOperationException ("A SqlStatement cannot be used as a predicate.");

      var statementBuilder = new SqlStatementBuilder (sqlStatement);

      var newSelectProjection = _stage.ApplyContext (sqlStatement.SelectProjection, expressionContext, _mappingResolutionContext);
      statementBuilder.SelectProjection = newSelectProjection;
      statementBuilder.RecalculateDataInfo (sqlStatement.SelectProjection);

      var newSqlStatement = statementBuilder.GetSqlStatement();
      return newSqlStatement.Equals (sqlStatement) ? sqlStatement : newSqlStatement;
    }
   
  }
}