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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Creates a <see cref="SqlTableBase"/> object from a given <see cref="SqlStatement"/>, extracting all <see cref="SqlStatement.Orderings"/> in the
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

    public FromExpressionInfo CreateSqlTableForStatement (SqlStatement sqlStatement, Func<ITableInfo, SqlTableBase> tableCreator)
    {
      if (sqlStatement.Orderings.Count == 0)
      {
        var tableInfo = new ResolvedSubStatementTableInfo (_uniqueIdentifierGenerator.GetUniqueIdentifier ("q"), sqlStatement);
        var sqlTable = tableCreator (tableInfo);
        return new FromExpressionInfo (sqlTable, new Ordering[0], new SqlTableReferenceExpression (sqlTable), null);
      }

      var selectExpressionWithOrderings = GetNewSelectExpressionWithOrderings (sqlStatement);
      var tableWithSubStatement = CreateSqlCompatibleSubStatementTable (sqlStatement, selectExpressionWithOrderings, tableCreator);
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

    private SqlTableBase CreateSqlCompatibleSubStatementTable (
        SqlStatement originalStatement, 
        Expression newSelectProjection, 
        Func<ITableInfo, SqlTableBase> tableCreator)
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
      return tableCreator (subStatementTableInfo);
    }

    private FromExpressionInfo GetFromExpressionInfoForSubStatement (SqlStatement originalSqlStatement, SqlTableBase tableWithSubStatement)
    {
      var expressionsFromSubStatement = TupleExpressionBuilder.GetExpressionsFromTuple (new SqlTableReferenceExpression (tableWithSubStatement));

      var projectionFromSubStatement = expressionsFromSubStatement.First (); // this was the original projection
      var orderingsFromSubStatement = expressionsFromSubStatement
          .Skip (1) // ignore original projection
          .Select ((expr, i) => new Ordering (expr, originalSqlStatement.Orderings[i].OrderingDirection));

      return new FromExpressionInfo (tableWithSubStatement, orderingsFromSubStatement.ToArray (), projectionFromSubStatement, null);
    }
  }
}