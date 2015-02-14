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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Creates a <see cref="SqlTable"/> object from a given <see cref="SqlStatement"/>, extracting all <see cref="SqlStatement.Orderings"/> in the
  /// statement as required by SQL.
  /// </summary>
  public class SqlPreparationSubStatementTableFactory
  {
    private readonly ISqlPreparationStage _stage;
    private readonly ISqlPreparationContext _context;
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    public SqlPreparationSubStatementTableFactory (
        ISqlPreparationStage stage, 
        ISqlPreparationContext context, 
        UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _stage = stage;
      _uniqueIdentifierGenerator = uniqueIdentifierGenerator;
      _context = context;
    }

    public FromExpressionInfo CreateSqlTableForStatement (SqlStatement sqlStatement, OrderingExtractionPolicy orderingExtractionPolicy)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      if (sqlStatement.Orderings.Count == 0)
      {
        var tableInfo = new ResolvedSubStatementTableInfo (_uniqueIdentifierGenerator.GetUniqueIdentifier ("q"), sqlStatement);
        var sqlTable = new SqlTable (tableInfo);
        return new FromExpressionInfo (
            new SqlAppendedTable (sqlTable, JoinSemantics.Inner),
            new Ordering[0],
            new SqlTableReferenceExpression (sqlTable),
            null);
      }
      
      // If we have orderings, we need to:
      // - Build a nested projection that includes the original orderings if OrderingExtractionPolicy.ExtractOrderingsIntoProjection is specified.
      // - Create a substatement clearing orderings, unless a TOP expression is present.
      // - Put it into a SqlTable.
      // - Put a reference to everything in the select projection (i.e., the original projection and the extracted orderings) into a 
      //   FromExpressionInfo and return that.

      var newSelectProjection = sqlStatement.SelectProjection;
      if (orderingExtractionPolicy == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)
        newSelectProjection = GetNewSelectExpressionWithOrderings (sqlStatement);

      var tableWithSubStatement = CreateSqlCompatibleSubStatementTable (sqlStatement, newSelectProjection);
      return GetFromExpressionInfoForSubStatement (sqlStatement, tableWithSubStatement);
    }

    private Expression GetNewSelectExpressionWithOrderings (SqlStatement sqlStatement)
    {
      // wrap original select projection and all orderings into a large tuple expression (new { proj, new { o1, new { o2, ... }}})
      var expressionsToBeTupelized = new[] { sqlStatement.SelectProjection }.Concat (sqlStatement.Orderings.Select (o => o.Expression));
      var tupleExpression = TupleExpressionBuilder.AggregateExpressionsIntoTuple (expressionsToBeTupelized);
      
      var preparedTupleExpression = _stage.PrepareSelectExpression (tupleExpression, _context);
      if (preparedTupleExpression.Type != tupleExpression.Type)
        throw new InvalidOperationException ("The SQL Preparation stage must not change the type of the select projection.");
      
      return preparedTupleExpression;
    }

    private SqlTable CreateSqlCompatibleSubStatementTable (SqlStatement originalStatement, Expression newSelectProjection)
    {
      // create a new statement equal to the original one, but with the tuple as its select projection
      var builder = new SqlStatementBuilder (originalStatement) { SelectProjection = newSelectProjection };
      builder.RecalculateDataInfo (originalStatement.SelectProjection);

      // clear orderings unless required for TopExpression
      if (originalStatement.TopExpression == null)
        builder.Orderings.Clear();
        
      var newSqlStatement = builder.GetSqlStatement();

      // put new statement into a sub-statement table
      var subStatementTableInfo = new ResolvedSubStatementTableInfo (_uniqueIdentifierGenerator.GetUniqueIdentifier ("q"), newSqlStatement);
      return new SqlTable (subStatementTableInfo);
    }

    private FromExpressionInfo GetFromExpressionInfoForSubStatement (SqlStatement originalSqlStatement, SqlTable tableWithSubStatement)
    {
      var expressionsFromSubStatement = 
          TupleExpressionBuilder.GetExpressionsFromTuple (new SqlTableReferenceExpression (tableWithSubStatement)).ToArray();

      var projectionFromSubStatement = expressionsFromSubStatement.First (); // this was the original projection
      var orderingsFromSubStatement = expressionsFromSubStatement
          .Skip (1) // ignore original projection
          .Select ((expr, i) => new Ordering (expr, originalSqlStatement.Orderings[i].OrderingDirection));

      return new FromExpressionInfo (
          new SqlAppendedTable (tableWithSubStatement, JoinSemantics.Inner),
          orderingsFromSubStatement.ToArray(),
          projectionFromSubStatement,
          null);
    }
  }
}