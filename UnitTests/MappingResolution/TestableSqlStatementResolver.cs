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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  public class TestableSqlStatementResolver : SqlStatementResolver
  {
    public TestableSqlStatementResolver (IMappingResolutionStage stage, IMappingResolutionContext context)
        : base (stage, context)
    {
    }

    public new Expression ResolveSelectProjection (Expression selectProjection, SqlStatementBuilder sqlStatementBuilder)
    {
      return base.ResolveSelectProjection (selectProjection, sqlStatementBuilder);
    }

    public new void ResolveSqlTable (SqlTable sqlTable)
    {
      base.ResolveSqlTable (sqlTable);
    }

    public new Expression ResolveTopExpression (Expression topExpression)
    {
      return base.ResolveTopExpression (topExpression);
    }

    public new Expression ResolveWhereCondition (Expression whereCondition)
    {
      return base.ResolveWhereCondition (whereCondition);
    }

    public new Expression ResolveGroupByExpression (Expression expression)
    {
      return base.ResolveGroupByExpression (expression);
    }

    public new Expression ResolveOrderingExpression (Expression orderByExpression)
    {
      return base.ResolveOrderingExpression (orderByExpression);
    }

    // TODO RMLNQSQL-64
    //public new void ResolveJoinedTable (SqlJoinedTable joinedTable)
    //{
    //  base.ResolveJoinedTable (joinedTable);
    //}

    public new SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      return base.ResolveSqlStatement (sqlStatement);
    }
  }
}