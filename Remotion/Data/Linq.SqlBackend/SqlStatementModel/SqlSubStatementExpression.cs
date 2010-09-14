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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlSubStatementExpression"/> represents a SQL database subquery. The <see cref="QueryModel"/> of the subquery is translated to 
  /// this model, and the <see cref="SqlSubStatementExpression"/> is transformed several times until it can easily be translated to SQL text.
  /// </summary>
  public class SqlSubStatementExpression : ExtensionExpression
  {
    private readonly SqlStatement _sqlStatement;

    public SqlSubStatementExpression (SqlStatement sqlStatement)
        : base (sqlStatement.DataInfo.DataType)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      _sqlStatement = sqlStatement;
    }

    public SqlStatement SqlStatement
    {
      get { return _sqlStatement; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSubStatementVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlSubStatementExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return "(" + _sqlStatement + ")";
    }

    
    public SqlTable CreateSqlTableForSubStatement (
        SqlSubStatementExpression subStatementExpression,  // TODO Review 3091: Remove this parameter. Use the "this" object instead. (Careful: The call to this method in ResolvingSelectExpressionVisitor must be adapted!) Rename the method to "CreateWrappingTable".
        JoinSemantics joinSemantics, 
        string uniqueIdentifier)
    {
      // TODO Review 3091: Argument checks!

      // TODO Review 3091: Check that subStatementExpression has a StreamedSingleValueInfo or StreamedScalarValueInfo before recalculating the DataInfo (and TopExpression). If it has a StreamedSequenceInfo, you can simply return a new ResolvedSubStatementTableInfo without recalculating anything. Test this.
      var newDataInfo = new StreamedSequenceInfo (
          typeof (IEnumerable<>).MakeGenericType (subStatementExpression.Type),
          subStatementExpression.SqlStatement.SelectProjection);

      var adjustedStatementBuilder = new SqlStatementBuilder (subStatementExpression.SqlStatement) { DataInfo = newDataInfo };
      if (subStatementExpression.SqlStatement.DataInfo is StreamedForcedSingleValueInfo)
      {
        Debug.Assert (
            adjustedStatementBuilder.TopExpression is SqlLiteralExpression
            && ((SqlLiteralExpression) adjustedStatementBuilder.TopExpression).Value.Equals (2));
        adjustedStatementBuilder.TopExpression = new SqlLiteralExpression (1);
      }

      var resolvedSubStatementTableInfo = new ResolvedSubStatementTableInfo (uniqueIdentifier, adjustedStatementBuilder.GetSqlStatement());
      return new SqlTable (resolvedSubStatementTableInfo, joinSemantics);
    }
  }
}