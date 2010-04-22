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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public class SqlContextStatementVisitor
  {
    private readonly ISqlContextResolutionStage _stage;

    public static SqlStatement ApplyContext (SqlStatement sqlStatement, SqlExpressionContext context, ISqlContextResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlContextStatementVisitor (stage);
      return visitor.VisitSqlStatement (sqlStatement, context);
    }

    private SqlContextStatementVisitor (ISqlContextResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);

      _stage = stage;
    }
    
    public SqlStatement VisitSqlStatement (SqlStatement sqlStatement, SqlExpressionContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      if (context == SqlExpressionContext.PredicateRequired)
        throw new NotSupportedException ("A sql-statement cannot return a predicate");

      var statementBuilder = new SqlStatementBuilder (sqlStatement);

      var newSelectProjection = _stage.ApplyContext (sqlStatement.SelectProjection, context);
      if (newSelectProjection != statementBuilder.SelectProjection)
      {
        statementBuilder.DataInfo = GetNewDataInfo (sqlStatement.DataInfo, newSelectProjection);
        statementBuilder.SelectProjection = newSelectProjection;
      }

      if(statementBuilder.WhereCondition!=null)
        statementBuilder.WhereCondition = _stage.ApplyContext (sqlStatement.WhereCondition, SqlExpressionContext.PredicateRequired);

      foreach (var ordering in statementBuilder.Orderings)
         ordering.Expression = _stage.ApplyContext (ordering.Expression, SqlExpressionContext.SingleValueRequired);

      if(statementBuilder.TopExpression!=null)
        statementBuilder.TopExpression = _stage.ApplyContext (sqlStatement.TopExpression, SqlExpressionContext.SingleValueRequired);

      return statementBuilder.GetSqlStatement();
    }

    private IStreamedDataInfo GetNewDataInfo (IStreamedDataInfo previousDataInfo, Expression newSelectProjection)
    {
      var previousStreamedSequenceInfo = previousDataInfo as StreamedSequenceInfo;
      if (previousStreamedSequenceInfo != null)
        return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (newSelectProjection.Type), newSelectProjection);

      var previousSingleValueInfo = previousDataInfo as StreamedSingleValueInfo;
      if (previousSingleValueInfo != null)
        return new StreamedSingleValueInfo (newSelectProjection.Type, previousSingleValueInfo.ReturnDefaultWhenEmpty);

      return previousDataInfo;
    }
  }
}